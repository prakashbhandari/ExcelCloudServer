//Title        :  AsyncConnection.cs
//Package      :  ExcelCloudServer
//Project      :  ExcelCloud
//Description  :  Provides connection to server, to send and receive data.
//Created on   :  June 5, 2016
//Author	   :  Prakash Bhandari

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using System.Diagnostics;

namespace ExcelCloudServer
{
    /// <summary>
    /// Class AsyncConnection: Provides an asynchronous Socket
    /// connection to server for sending and receiving data
    /// </summary>
    public class AsyncConnection
    {
        /// <summary>
        /// Server's ip address
        /// </summary>
        private string host;
        /// <summary>
        /// Port server will be listening to
        /// </summary>
        private int port;
        /// <summary>
        /// reset event to signal once connection is done
        /// </summary>
        protected static ManualResetEvent connectDone = new ManualResetEvent(false);
        /// <summary>
        /// reset event to signal once sending is done
        /// </summary>
        public static ManualResetEvent sendDone = new ManualResetEvent(false);
        /// <summary>
        /// reset event to signal once receiving is done
        /// </summary>
        protected static ManualResetEvent receiveDone = new ManualResetEvent(false);
        /// <summary>
        /// request received from server
        /// </summary>
        private static String content = String.Empty;
        /// <summary>
        /// hold the instance of socket
        /// </summary>
        private static Socket listener;
        /// <summary>
        /// Sets connection status to true if connected to server
        /// </summary>
        public volatile bool listening = true;
        public static StateObject state = new StateObject();

        /// <summary>
        /// Initialise the class with host and port
        /// </summary>
        /// <param name="host">Server's ip address</param>
        /// <param name="port">Socket open port</param>
        public AsyncConnection(String host, int port)
        {
            this.host = host;
            this.port = port;
        }

        /// <summary>
        /// Check if the socket can be opened in the given port
        /// </summary>
        /// <returns>Bool: True if port is available else false</returns>
        public bool IsPortAvailable()
        {
            IPAddress ipAddress = IPAddress.Parse(this.host);
            try
            {

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

        /// <summary>
        /// Start listening at the specified host and port
        /// </summary>
        public void StartListening()
        {
            byte[] bytes = new byte[256];
            IPAddress ipAddress = IPAddress.Parse(this.host);
            try
            {
                while (this.listening)
                {
                    connectDone.Reset();

                    Debug.WriteLine("Waiting for connection at " + ipAddress.ToString() + ":" + this.port.ToString());
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

        /// <summary>
        /// If connection is made start accpeting jobs
        /// </summary>
        /// <param name="ar"></param>
        public static void AcceptCallBack(IAsyncResult ar)
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
            catch (ObjectDisposedException ode)
            {
                Debug.WriteLine("Listener has been closed" + ode.ToString());
            }
        }

        /// <summary>
        /// Receives the job description and initialises the job class
        /// </summary>
        /// <param name="ar"></param>
        private static void ReadCallBack(IAsyncResult ar)
        {
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                // All data has been recieved process the job now
                if (state.sb.ToString().IndexOf("EOF") > -1)
                {
                    content = state.sb.ToString();
                    content = content.Remove(content.IndexOf("EOF"), "EOF".Length);
                    state.sb = new StringBuilder();
                    Debug.WriteLine("Received data:" + content);
                    string data = content;
                    try
                    {
                        Job job = JsonConvert.DeserializeObject<Job>(data);
                        job.SubmitTasks();
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine(e.ToString());
                    }
                }
                else
                {
                    // Keep reading data until EOF is received signalling all data sent
                    handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallBack), state);
                }
            }
        }

        /// <summary>
        /// Send data string to the connected client
        /// </summary>
        /// <param name="data">string: data to be sent</param>
        public static void Send(string data)
        {
            // Convert the string data to byte data using ASCII encoding
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Being sending bytes to the client
            state.workSocket.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallBack), state.workSocket);
        }

        /// <summary>
        /// end sending all data and signal data sent
        /// </summary>
        /// <param name="ar"></param>
        private static void SendCallBack(IAsyncResult ar)
        {
            try
            {
                //Retrieve the socket information from Socket Object
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending data to client
                int bytesSent = handler.EndSend(ar);
                Debug.WriteLine("Sent " + bytesSent.ToString() + " bytes to client");
                // Signal data sending completed
                sendDone.Set();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// Stop listening at the host and port specified
        /// </summary>
        public void StopListening()
        {
            this.listening = false;
            // Signal ready fro new connection
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