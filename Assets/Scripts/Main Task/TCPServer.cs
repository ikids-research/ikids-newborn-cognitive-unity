using UnityEngine;
using System.Collections.Generic;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

public class TCPServer {
    public static string endOfMessageString = "*EOF*";
    private AsynchronousSocketListener _socket;
    public TCPServer(int port)
    {
        _socket = new AsynchronousSocketListener(port);
        _socket.StartListening();
    }
    public string[] getCommands()
    {
        int count = _socket.CommandQueue.Count;
        string[] returnValue = new string[count];
        for (int i = 0; i < count; i++)
            returnValue[i] = _socket.CommandQueue.Dequeue();
        return returnValue;
    }
    public void safeShutdown()
    {
        _socket.Shutdown();
    }
    // State object for reading client data asynchronously
    public class StateObject
    {
        // Client  socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 1024;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }

    public class AsynchronousSocketListener
    {
        // Thread signal.
        private ManualResetEvent _allDone = new ManualResetEvent(false);
        private Socket _socket;
        private int _port;
        private Queue<string> _commandQueue;

        public int Port
        {
            get
            {
                return _port;
            }
        }

        public Queue<string> CommandQueue
        {
            get
            {
                return _commandQueue;
            }
        }

        public AsynchronousSocketListener(int port)
        {
            _port = port;
            _commandQueue = new Queue<string>();
        }

        public void StartListening()
        {
            // Data buffer for incoming data.
            byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket.
            // The DNS name of the computer
            // running the listener is "host.contoso.com".
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, _port);

            // Create a TCP/IP socket.
            Socket listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            // Bind the socket to the local endpoint and listen for incoming connections.
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                // Start an asynchronous socket to listen for connections.
                Debug.Log("Waiting for a connection on port " + _port + "...");
                listener.BeginAccept(
                    new AsyncCallback(AcceptCallback),
                    listener);
            } catch (Exception ) { }
        }

        public void AcceptCallback(IAsyncResult ar)
        {
            // Signal the main thread to continue.
            _allDone.Set();

            // Get the socket that handles the client request.
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);
            _socket = handler;
            Debug.Log("TCP connection established.");

            // Create the state object.
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }

        public void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;

            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket. 
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.
                state.sb.Append(Encoding.ASCII.GetString(
                    state.buffer, 0, bytesRead));

                // Check for end-of-file tag. If it is not there, read 
                // more data.
                content = state.sb.ToString();
                if (content.IndexOf(endOfMessageString) > -1)
                {
                    // All the data has been read from the 
                    // client. Display it on the console.
                    string command = content.Remove(content.Length - endOfMessageString.Length, endOfMessageString.Length).Trim();
                    _commandQueue.Enqueue(command);
                    Debug.Log("Read " + content.Length + " bytes from socket. \n Data : " + content);
                    // Echo the data back to the client.
                    Send(handler, content);
                    state.sb = new StringBuilder();
                }
                // Not all data received. Get more.
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
            }
        }

        private void Send(Socket handler, String data)
        {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = handler.EndSend(ar);
                Debug.Log("Sent " + bytesSent + " bytes to client.");
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
            }
        }

        public void Shutdown()
        {
            if (_socket != null)
            {
                _socket.Send(Encoding.ASCII.GetBytes("Done" + endOfMessageString));
                Debug.Log("Send Done response over TCP connection.");
                try
                {
                    _socket.Shutdown(SocketShutdown.Both);
                    Debug.Log("Socket shutdown requested.");
                }
                catch (SocketException) { };
                _socket.Close();
                Debug.Log("Socket close complete.");
            }
        }
    }
}
