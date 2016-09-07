// Taken from https://msdn.microsoft.com/en-us/library/kb5kfec7(v=vs.110).aspx

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class SynchronousSocketClient
{

    public static void StartClient()
    {
        // Data buffer for incoming data.
        byte[] bytes = new byte[1024];

        // Connect to a remote device.
        try
        {
            // Establish the remote endpoint for the socket.
            IPAddress ipAddress = IPAddress.Loopback; //SET ME
            IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11235); //SET ME

            // Create a TCP/IP  socket.
            Socket sender = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            // Connect the socket to the remote endpoint. Catch any errors.
            try
            {
                sender.Connect(remoteEP);

                Console.WriteLine("Socket connected to {0}",
                        sender.RemoteEndPoint.ToString());

                while (! Console.KeyAvailable)
                {
                    System.Threading.Thread.Sleep(1000);
                    
                    string sendString = "This is a test*EOF*"; //THIS IS WHAT IS SENT, *EOF* IS REQUIRED AT END

                    // Encode the data string into a byte array.
                    byte[] msg = Encoding.ASCII.GetBytes(sendString);

                    // Send the data through the socket.
                    int bytesSent = sender.Send(msg);

                    // Receive the response from the remote device.
                    int bytesRec = sender.Receive(bytes);
                    Console.WriteLine("{0}; Sent: {1}; Echoed = {2}", 
                        DateTime.Now.ToLongTimeString(),
                        sendString,
                        Encoding.ASCII.GetString(bytes, 0, bytesRec));
                }
                // Release the socket.
                sender.Shutdown(SocketShutdown.Both);
                sender.Close();

            }
            catch (ArgumentNullException ane)
            {
                Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
            }
            catch (SocketException se)
            {
                Console.WriteLine("SocketException : {0}", se.ToString());
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
        }
    }

    public static int Main(String[] args)
    {
        StartClient();
        return 0;
    }
}