using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Payloads
{
    public class ServerClientConnectPayload : LazySingleton<ServerClientConnectPayload>
    {
        public string Ip { get; set; }

        public ushort Port { get; set; }
    }
}
