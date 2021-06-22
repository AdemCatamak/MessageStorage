using System.Linq;
using System.Net.NetworkInformation;

namespace TestUtility
{
    public static class NetworkUtility
    {
        public static int GetAvailablePort(int startingPort = 10000)
        {
            var properties = IPGlobalProperties.GetIPGlobalProperties();

            //getting active connections
            var tcpConnectionPorts = properties.GetActiveTcpConnections()
                                               .Where(n => n.LocalEndPoint.Port >= startingPort)
                                               .Select(n => n.LocalEndPoint.Port);

            //getting active tcp listeners - WCF service listening in tcp
            var tcpListenerPorts = properties.GetActiveTcpListeners()
                                             .Where(n => n.Port >= startingPort)
                                             .Select(n => n.Port);

            //getting active udp listeners
            var udpListenerPorts = properties.GetActiveUdpListeners()
                                             .Where(n => n.Port >= startingPort)
                                             .Select(n => n.Port);

            int port = Enumerable
                      .Range(startingPort, ushort.MaxValue)
                      .Where(i => !tcpConnectionPorts.Contains(i))
                      .Where(i => !tcpListenerPorts.Contains(i))
                      .FirstOrDefault(i => !udpListenerPorts.Contains(i));

            return port;
        }
    }
}