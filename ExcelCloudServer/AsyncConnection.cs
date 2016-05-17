using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json.Linq;
using System.Diagnostics;

namespace ExcelCloudServer
{
    public class AsyncConnection
    {
        private String host;
        private int port;
        protected static ManualResetEvent connectDone = new ManualResetEvent(false);
        public static ManualResetEvent sendDone = new ManualResetEvent(false);
        protected static ManualResetEvent receiveDone = new ManualResetEvent(false);

        public static StateObject state = new StateObject();
        private String content = String.Empty;
        private static Socket listener;
        public volatile bool listening = true;

        public AsyncConnection(String host, int port)
        {
            this.host = host;
            this.port = port;
        }


        public bool IsPortAvailable()
        {
            IPAddress ipAddress = IPAddress.Parse(this.host);
            try
            {
                /*IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddress = null;
                foreach (IPAddress ip in ipHostInfo.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        ipAddress = ip;
                    }
                }*/

                // Check if port is available if so start the server

                IPEndPoint localEndPoint = new IPEndPoint(ipAddress, this.port);
                listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                listener.Bind(localEndPoint);
                listener.Listen(100);

                return true;
            }
            catch (SocketException ex)
            {
                Debug.WriteLine("SocketException: " + ex.ToString());
            }
            return false;
        }

        public void StartListening()
        {
            byte[] bytes = new byte[256];
            IPAddress ipAddress = IPAddress.Parse(this.host);
            try
            {
                while (this.listening)
                {
                    connectDone.Reset();

                    Debug.WriteLine("Waiting for connection at "+ ipAddress.ToString() + ":"+ this.port.ToString());
                    listener.BeginAccept(new AsyncCallback(AcceptCallBack), listener);

                    // Wait until a connection is made before continuing
                    connectDone.WaitOne();
                }
                Debug.WriteLine("Closing Connection.");
                listener.Close();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        public void AcceptCallBack(IAsyncResult ar)
        {
            try
            {
                // Signal main thread to continue
                connectDone.Set();

                // Get the socket that handles the connection
                Socket listener = (Socket)ar.AsyncState;
                Socket handler = listener.EndAccept(ar);

                state.workSocket = handler;
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallBack), state);
            }
            catch(ObjectDisposedException ode)
            {
                Debug.WriteLine("Listener has been closed");
            }
        }

        private void ReadCallBack(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));

                this.content = state.sb.ToString();
                if (this.content.IndexOf("EOF") > -1)
                {
                    this.content = this.content.Remove(content.IndexOf("EOF"), "EOF".Length);
                    Debug.WriteLine("All Data Received: "+ this.content + " \nRunning Task Now.");
                    try
                    {
                        JObject taskQuery = JObject.Parse(@content);
                        string task = (string)taskQuery["task"];
                        JArray allParams = (JArray)taskQuery["inputParams"];
                        string[] paramList = allParams.ToObject<string[]>();

                        //string values = (string)taskQuery["inputParams"];
                        //TaskExec.execTask(task, paramList);
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.ToString());
                    }
                }
                else
                {
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallBack), state);
                }
            }
        }

        public void Send(Socket handler, String data)
        {
            // Convert the string data to byte data using ASCII encoding
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Being sending bytes to the client
            handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallBack), handler);
        }

        private void SendCallBack(IAsyncResult ar)
        {
            try
            {
                //Retrieve the socket information from Socket Object
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending data to client
                int bytesSent = handler.EndSend(ar);
                Debug.WriteLine("Sent "+ bytesSent.ToString() + " bytes to client");
                sendDone.Set();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        public void StopListening()
        {
            this.listening = false;
            connectDone.Set();
        }
    }

    // State Object for sending/receiving data from client
    public class StateObject
    {
        public Socket workSocket = null;
        public const int BufferSize = 256;
        public byte[] buffer = new byte[BufferSize];
        public StringBuilder sb = new StringBuilder();
    }
}