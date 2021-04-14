using System;

namespace VoIP_Server
{
    public class ConnectEventArgs: EventArgs
    {
        public string GameCode { get; set; }
        public string GameName { get; set; }
        public int ID { get; set; }
    }
}