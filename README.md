# heartbeat {{ <3 }}
heartbeat is a Windows service that checks other machines periodically for their response. The service offers pinging or checking that a user is logged on (explorer.exe is running), logging and email support. 

##Example
Domain `HOME` has multiple computers (`A`, `B`, and `C`). We install the service on our monitoring server and configure `HOME` via its `XML` file. The service then confirms, at a specified interval, that a user is logged in, and sends a message if it is not.

##Usage
Install the service on your server and set it up to run as an admin account on the monitored computers. Configure the service through the `heartbeats.xml` file

    <heartbeat>
        <machine>
            <name>Computer A</name>
             <hostname>HOSTNAMEA</hostname>
            <address>10.100.50.100</address>
            <recipients>
                <email>email1@domain.com</email>
                <email>email2@domain.com</email>
            </recipients>
        </machine>
        ...
        
The interval to check the computers and smtp email can be configured in `App.config` under `<appSettings>`

    <appSettings>
        <add key="emailSender" value="heartbeat@domain.com" />
        <add key="smtpServer" value="smtp.domain.com" />
        <add key="intervalInMinutes" value="15" />

Start the service and you are up and running.

## FAQ
1. I installed the service, configured it, but my computer can not be reached
    - Check that `Remote Registry Service` is enabled and that the service account which your service uses is an administrator on the remote computer
