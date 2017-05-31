using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace fwknop.uwp
{
    static class SocketUtil
    {
        public static async Task<int> SendAsync(string spaServer, int spaServerPort, byte[] data)
        {
            var hostEntry = await Dns.GetHostEntryAsync(spaServer);
            IPEndPoint endPoint = new IPEndPoint(hostEntry.AddressList.First(i => i.AddressFamily == AddressFamily.InterNetwork), spaServerPort);
            using (Socket sock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                return sock.SendTo(data, endPoint);
            }

        }
    }
}
