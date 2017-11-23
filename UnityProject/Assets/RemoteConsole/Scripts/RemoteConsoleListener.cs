using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

namespace RemoteConsole
{
    public class RemoteConsoleListener : MonoBehaviour
    {
#if DEVELOPMENT_BULID || UNITY_EDITOR
        [SerializeField]
        private int port = 64064;

        private TcpListener listener;
        private IPAddress listenAddress;
        private ConsoleCommandManager commandManager;
        
        private void Start()
        {
            listenAddress = FindNetworkAddress();

            if (listenAddress == null)
            {
                Debug.LogWarning("[REMOTE] Failed to find an address to listen on");
                return;
            }

            Debug.Log("[REMOTE] Listening for commands on address " + listenAddress + ":" + port);
            try
            {
                commandManager = new ConsoleCommandManager();                
                listener = new TcpListener(listenAddress, port);
                listener.Start();
            }
            catch (SocketException e)
            {
                Debug.LogError("[REMOTE] Socket exception: " + e);
                if (listener != null)
                {
                    listener.Stop();
                }
            }
        }
        
        private void OnApplicationQuit()
        {
            if (listener != null)
            {
                listener.Stop();
            }
        }
        
        private void Update()
        {
            if (listener != null && listener.Pending())
            {
                ProcessRemoteCommand();
            }
        }
        
        private void ProcessRemoteCommand()
        {
            try
            {
                TcpClient client = listener.AcceptTcpClient();
                NetworkStream stream = client.GetStream();
                if (stream.CanRead)
                {
                    string command = ReadCommandFromStream(stream);
                    Debug.Log("[REMOTE] Recieved command: " + command);
                    string result = commandManager.ExecuteCommand(command);

                    // Write any output back to the client.
                    if (!string.IsNullOrEmpty(result))
                    {
                        byte[] buffer = Encoding.ASCII.GetBytes(result);
                        stream.Write(buffer, 0, buffer.Length);
                    }
                }
                stream.Close();
                client.Close();
            }
            catch (SocketException e)
            {
                Debug.LogError("[REMOTE] Socket exception: " + e);
            }
        }

        private string ReadCommandFromStream(NetworkStream stream)
        {
            byte[] buffer = new byte[256];
            stream.Read(buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer.ToArray()).TrimEnd('\0');
        }

        // Finds the internal network IPv4 address (e.g. 192.168.0.16).
        private IPAddress FindNetworkAddress()
        {
            foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                foreach (var addr in nic.GetIPProperties().UnicastAddresses)
                {
                    if (IPAddress.IsLoopback(addr.Address))
                    {
                        continue;
                    }
                    if (addr.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        return addr.Address;
                    }
                }
            }
            return null;
        }
    }
#endif
}
