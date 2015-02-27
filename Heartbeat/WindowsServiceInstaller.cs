using System.ComponentModel;
using System.Configuration.Install;
using System.ServiceProcess;

namespace Heartbeat
{
    [RunInstaller(true)]
    public class WindowsServiceInstaller : Installer
    {
        public WindowsServiceInstaller()
        {
            var processInstaller = new ServiceProcessInstaller();
            var serviceInstaller = new ServiceInstaller();

            processInstaller.Account = ServiceAccount.User;
            processInstaller.Username = @"domain\serviceaccount";
            processInstaller.Password = @"password";

            serviceInstaller.DisplayName = "Heartbeat Service";
            serviceInstaller.Description = "Hardware monitor for computers that run automation or need to be online.";
            serviceInstaller.StartType = ServiceStartMode.Automatic;

            serviceInstaller.ServiceName = "Heartbeat (Hardware Monitor)";

            this.Installers.Add(processInstaller);
            this.Installers.Add(serviceInstaller);
        }
    }
}