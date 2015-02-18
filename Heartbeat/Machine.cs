using System;
using System.Collections.Generic;

namespace Heartbeat
{
    class Machine
    {
        public string Name { get; private set; }
        public string Address { get; private set; }
        public string Hostname { get; private set; }
        public IEnumerable<string> Recipients { get; private set; }

        public Machine(string name, string address, string hostname, IEnumerable<string> recipients)
        {
            Name = name;
            Address = address;
            Hostname = hostname;
            Recipients = recipients;
        }
    }
}
