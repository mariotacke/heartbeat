using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.ServiceProcess;
using System.Timers;
using System.Xml.Linq;

namespace Heartbeat
{
    class Program : ServiceBase
    {
        static Timer timer;
        static List<Machine> machines = new List<Machine>();
        static string emailSender = ConfigurationManager.AppSettings["emailSender"];
        static string emailSubject = ConfigurationManager.AppSettings["emailSubject"];
        static string smtpServer = ConfigurationManager.AppSettings["smtpServer"];
        static string intervalInMinutes = ConfigurationManager.AppSettings["intervalInMinutes"];
        static string sourceDirectory = @"C:\Services\Heartbeat\";

        public Program()
        {
            this.ServiceName = "Heartbeat (Hardware Monitor)";
        }

        static void Main(string[] args)
        {
            ServiceBase.Run(new Program());
        }

        protected override void OnStart(string[] args)
        {
            base.OnStart(args);
            int interval = Convert.ToInt32(intervalInMinutes);

            timer = new Timer();
            timer.Interval = 1000 * 60 * interval;
            timer.Elapsed += timer_Elapsed;

            Beat();

            timer.Start();
        }

        void timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Beat();
        }

        protected override void OnStop()
        {
            base.OnStop();
            timer.Stop();
        }

        public static void Beat()
        {
            ReadHeartbeats();
            StartHeartBeat();
        }

        public static void StartHeartBeat()
        {
            foreach (var machine in machines)
            {
                if (!IsLoggedIn(machine.Address))
                {
                    SendEmail(machine);
                    LogAlert(machine);
                }
            }
        }

        public static void LogAlert(Machine machine)
        {
            using (var writer = new StreamWriter(sourceDirectory + "log.txt", true))
            {
                writer.WriteLine("[{0}] Cannot reach \"{1}\" ({3} | {2}) as {4}", 
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), 
                    machine.Name, 
                    machine.Address,
                    machine.Hostname,
                    Environment.UserName);
            }
        }

        public static void SendEmail(Machine machine)
        {
            var message = new MailMessage();
            var smtpClient = new SmtpClient(smtpServer);

            message.From = new MailAddress(emailSender, "{{ <3 }}");

            foreach (var recipient in machine.Recipients)
            {
                message.To.Add(recipient);
            }

            message.Subject = string.Format(emailSubject, machine.Name, machine.Address, machine.Hostname);
            message.Body =
                string.Format(
                "ALERT\nAt {0} on {1}, \"{2}\" ({4} | {3}) has become unresponsive.\n" +
                "Please log in to the machine to assure automation continues.",
                DateTime.Now.ToString("hh:mmtt"), 
                DateTime.Now.ToString("M/d/yyyy"),
                machine.Name,
                machine.Address,
                machine.Hostname);

            message.Priority = MailPriority.High;

            smtpClient.Send(message);
        }

        // reference: http://stackoverflow.com/questions/7119806/c-sharp-reading-data-from-xml
        public static void ReadHeartbeats()
        {
            var xml = XDocument.Load(sourceDirectory + "Heartbeats.xml");
            var heartbeats = xml.Descendants("machine");

            // remove all machines from collection as we are reading in the configuration
            // file again. This enables us to change the config without restarting the 
            // service.
            machines.Clear();

            foreach (var machine in heartbeats)
            {
                var emails = new List<string>();
                var name = machine.Descendants("name").FirstOrDefault().Value.ToString();
                var address = machine.Descendants("address").FirstOrDefault().Value.ToString();
                var hostname = machine.Descendants("hostname").FirstOrDefault().Value.ToString();
                var recipients = machine.Descendants("recipients");

                foreach (var email in recipients)
                {
                    emails.AddRange(email.Descendants("email").Select(x=> x.Value.ToString()));
                }

                machines.Add(new Machine(name, address, hostname, emails));
            }
        }

        // reference: http://stackoverflow.com/questions/11800958/using-ping-in-c-sharp
        public static bool PingHost(string nameOrAddress)
        {
            bool pingable = false;
            var pinger = new Ping();

            try
            {
                PingReply reply = pinger.Send(nameOrAddress);

                pingable = reply.Status == IPStatus.Success;
            }
            catch (PingException)
            {
                // Discard PingExceptions and return false;
            }

            return pingable;
        }

        // reference: http://arstechnica.com/civis/viewtopic.php?f=20&t=809824
        public static bool IsLoggedIn(string nameOrAddress)
        {
            Process[] processes = null;

            try
            {
                // Remote Registry service has to be enabled and started for
                // remote process polling to function correctly! Also, the 
                // executing user needs to have administrative privileges.
                processes = Process.GetProcesses(nameOrAddress);
            }
            catch (InvalidOperationException)
            {
                // GetProcesses will throw exception if the machine is off
                return false;
            }

            if (processes != null)
            {
                foreach (var process in processes)
                {
                    if (process.ProcessName.Equals("explorer"))
                    {
                        return true;
                    }
                }

                return false;
            }
            else
            {
                return false;
            }
        }
    }
}
