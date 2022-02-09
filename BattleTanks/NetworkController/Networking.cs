using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

/// <author>
/// Ryan Sloan and Kashish Singh
/// </author>
namespace NetworkUtil
{

    public static class Networking
    {
        /////////////////////////////////////////////////////////////////////////////////////////
        // Server-Side Code
        /////////////////////////////////////////////////////////////////////////////////////////



        /// <summary>
        /// Starts a TcpListener on the specified port and starts an event-loop to accept new clients.
        /// The event-loop is started with BeginAcceptSocket and uses AcceptNewClient as the callback.
        /// AcceptNewClient will continue the event-loop.
        /// </summary>
        /// <param name="toCall">The method to call when a new connection is made</param>
        /// <param name="port">The the port to listen on</param>
        public static TcpListener StartServer(Action<SocketState> toCall, int port)
        {

            // TcpListener to begin accepting new clients
            TcpListener listener = new TcpListener(IPAddress.Any, port);

            // Start the listener
            listener.Start();

            // This creates a ListenerState object that holds the toCall delegate for the server and the servers TcpListener
            // This allows me to pass this ListenerState object into the BeginAcceptSocket method as the state parameter
            ListenerState tcpToCall = new ListenerState(listener, toCall);

            // Starts event loop to accept new clients
            listener.BeginAcceptSocket(AcceptNewClient, tcpToCall);

            return listener;
        }

        /// <summary>
        /// To be used as the callback for accepting a new client that was initiated by StartServer, and 
        /// continues an event-loop to accept additional clients.
        ///
        /// Uses EndAcceptSocket to finalize the connection and create a new SocketState. The SocketState's
        /// OnNetworkAction should be set to the delegate that was passed to StartServer.
        /// Then invokes the OnNetworkAction delegate with the new SocketState so the user can take action. 
        /// 
        /// If anything goes wrong during the connection process (such as the server being stopped externally), 
        /// the OnNetworkAction delegate should be invoked with a new SocketState with its ErrorOccurred flag set to true 
        /// and an appropriate message placed in its ErrorMessage field. The event-loop should not continue if
        /// an error occurs.
        ///
        /// If an error does not occur, after invoking OnNetworkAction with the new SocketState, an event-loop to accept 
        /// new clients should be continued by calling BeginAcceptSocket again with this method as the callback.
        /// </summary>
        /// <param name="ar">The object asynchronously passed via BeginAcceptSocket. It must contain a tuple with 
        /// 1) a delegate so the user can take action (a SocketState Action), and 2) the TcpListener</param>
        private static void AcceptNewClient(IAsyncResult ar)
        {
            // Gets the ListenerState object that was passed in by the BeginAcceptSocket method in StartServer
            ListenerState tcpToCall = (ListenerState)ar.AsyncState;

            try
            {
                // Gets the socket of the client. It does this by getting the TcpListener from the tcpToCall variable and calling EndAcceptSocket
                Socket theSocket = tcpToCall.listener.EndAcceptSocket(ar);

                // Create a SocketState object with the networkAction delegate from my tcpToCall variable and the clients socket
                SocketState state = new SocketState(tcpToCall.networkAction, theSocket);

                // Invoke the toCall passed in from StartServer()
                state.OnNetworkAction(state);

                // Continues the event loop to accept new clients
                tcpToCall.listener.BeginAcceptSocket(AcceptNewClient, tcpToCall);
            }
            catch
            {

                // Error Process:
                // Make new SocketState and flag its ErrorOccurred to true with proper error message then invoke OnNetworkAction
                // witht the new SocketState
                SocketState errorState = new SocketState(tcpToCall.networkAction, null);

                errorState.ErrorOccurred = true;

                errorState.ErrorMessage = "Trouble connecting the client to the server";

                errorState.OnNetworkAction(errorState);

            }
        }

        /// <summary>
        /// Stops the given TcpListener.
        /// </summary>
        public static void StopServer(TcpListener listener)
        {
            listener.Stop();
        }

        /////////////////////////////////////////////////////////////////////////////////////////
        // Client-Side Code
        /////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Begins the asynchronous process of connecting to a server via BeginConnect, 
        /// and using ConnectedCallback as the method to finalize the connection once it's made.
        /// 
        /// If anything goes wrong during the connection process, toCall should be invoked 
        /// with a new SocketState with its ErrorOccurred flag set to true and an appropriate message 
        /// placed in its ErrorMessage field. Depending on when the error occurs, this should happen either
        /// in this method or in ConnectedCallback.
        ///
        /// This connection process should timeout and produce an error (as discussed above) 
        /// if a connection can't be established within 3 seconds of starting BeginConnect.
        /// 
        /// </summary>
        /// <param name="toCall">The action to take once the connection is open or an error occurs</param>
        /// <param name="hostName">The server to connect to</param>
        /// <param name="port">The port on which the server is listening</param>
        public static void ConnectToServer(Action<SocketState> toCall, string hostName, int port)
        {
            // Establish the remote endpoint for the socket.
            IPHostEntry ipHostInfo;
            IPAddress ipAddress = IPAddress.None;

            // Determine if the server address is a URL or an IP
            try
            {
                ipHostInfo = Dns.GetHostEntry(hostName);
                bool foundIPV4 = false;
                foreach (IPAddress addr in ipHostInfo.AddressList)
                    if (addr.AddressFamily != AddressFamily.InterNetworkV6)
                    {
                        foundIPV4 = true;
                        ipAddress = addr;
                        break;
                    }
                // Didn't find any IPV4 addresses
                if (!foundIPV4)
                {
                    // Error Process:
                    // Make new SocketState and flag its ErrorOccurred to true with proper error message then invoke OnNetworkAction
                    // witht the new SocketState
                    SocketState errorState = new SocketState(toCall, null);
                    errorState.ErrorOccurred = true;
                    errorState.ErrorMessage = "No IP address found connected to the server";
                    errorState.OnNetworkAction(errorState);
                }
            }
            catch (Exception)
            {
                // see if host name is a valid ipaddress
                try
                {
                    ipAddress = IPAddress.Parse(hostName);
                }
                catch (Exception)
                {
                    // Error Process:
                    // Make new SocketState and flag its ErrorOccurred to true with proper error message then invoke OnNetworkAction
                    // witht the new SocketState
                    SocketState errorState = new SocketState(toCall, null);
                    errorState.ErrorOccurred = true;
                    errorState.ErrorMessage = "Not a valid IPAddress";
                    errorState.OnNetworkAction(errorState);
                }
            }
            
            // Create a TCP/IP socket.
            Socket socket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            
            // Create SocketState with the Socket above
            SocketState state = new SocketState(toCall, socket);

            // This disables Nagle's algorithm (google if curious!)
            // Nagle's algorithm can cause problems for a latency-sensitive 
            // game like ours will be 
            socket.NoDelay = true;

            IAsyncResult result = socket.BeginConnect(ipAddress, port, ConnectedCallback, state);   //gets the result of beginConnect and starts connection process
            bool noTimeout = result.AsyncWaitHandle.WaitOne(3000, true);    //If connection happens within 3 seconds, noTimeout set to true
            if(!noTimeout)  //If connection is not established in 3 seconds, sets the error to true and gives an error message
            {
                // Error Process:
                // Make new SocketState and flag its ErrorOccurred to true with proper error message then invoke OnNetworkAction
                // witht the new SocketState
                SocketState errorState = new SocketState(toCall, null);
                errorState.ErrorOccurred = true;
                errorState.ErrorMessage = "Connection 3 second timeout";
                errorState.OnNetworkAction(errorState);
            }
        }

        /// <summary>
        /// To be used as the callback for finalizing a connection process that was initiated by ConnectToServer.
        ///
        /// Uses EndConnect to finalize the connection.
        /// 
        /// As stated in the ConnectToServer documentation, if an error occurs during the connection process,
        /// either this method or ConnectToServer should indicate the error appropriately.
        /// 
        /// If a connection is successfully established, invokes the toCall Action that was provided to ConnectToServer (above)
        /// with a new SocketState representing the new connection.
        /// 
        /// </summary>
        /// <param name="ar">The object asynchronously passed via BeginConnect</param>
        private static void ConnectedCallback(IAsyncResult ar)
        {
            // Gets the Socket/SocketState from the IAsyncResult
            SocketState state = (SocketState)ar.AsyncState;
            Socket theSocket = state.TheSocket;

            try
            {
                // Finalizes the connection process
                theSocket.EndConnect(ar);

                // Invokes the toCall Action passed in from ConnectToServer
                state.OnNetworkAction(state);
            }
            catch
            {
                // Error Process:
                // Make new SocketState and flag its ErrorOccurred to true with proper error message then invoke OnNetworkAction
                // witht the new SocketState
                SocketState errorState = new SocketState(state.OnNetworkAction, null);
                errorState.ErrorOccurred = true;
                errorState.ErrorMessage = "Connection callback could not be finalized";
                errorState.OnNetworkAction(errorState);
            }
        }


        /////////////////////////////////////////////////////////////////////////////////////////
        // Server and Client Common Code
        /////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Begins the asynchronous process of receiving data via BeginReceive, using ReceiveCallback 
        /// as the callback to finalize the receive and store data once it has arrived.
        /// The object passed to ReceiveCallback via the AsyncResult should be the SocketState.
        /// 
        /// If anything goes wrong during the receive process, the SocketState's ErrorOccurred flag should 
        /// be set to true, and an appropriate message placed in ErrorMessage, then the SocketState's
        /// OnNetworkAction should be invoked. Depending on when the error occurs, this should happen either
        /// in this method or in ReceiveCallback.
        /// </summary>
        /// <param name="state">The SocketState to begin receiving</param>
        public static void GetData(SocketState state)
        {
            try
            {
                // Begins the process of recieving data
                state.TheSocket.BeginReceive(state.buffer, 0, state.buffer.Length, SocketFlags.None, ReceiveCallback, state);
            }
            catch
            {
                // Error Process:
                // Sets ErrorOccurred to true with proper error message then invokes OnNetworkAction
                // with the SocketState state
                state.ErrorOccurred = true;
                state.ErrorMessage = "Error receiving message from the client";
                state.OnNetworkAction(state);
            }
        }

        /// <summary>
        /// To be used as the callback for finalizing a receive operation that was initiated by GetData.
        /// 
        /// Uses EndReceive to finalize the receive.
        ///
        /// As stated in the GetData documentation, if an error occurs during the receive process,
        /// either this method or GetData should indicate the error appropriately.
        /// 
        /// If data is successfully received:
        ///  (1) Read the characters as UTF8 and put them in the SocketState's unprocessed data buffer (its string builder).
        ///      This must be done in a thread-safe manner with respect to the SocketState methods that access or modify its 
        ///      string builder.
        ///  (2) Call the saved delegate (OnNetworkAction) allowing the user to deal with this data.
        /// </summary>
        /// <param name="ar"> 
        /// This contains the SocketState that is stored with the callback when the initial BeginReceive is called.
        /// </param>
        private static void ReceiveCallback(IAsyncResult ar)
        {
            // Gets the Socket/SocketState from the IAsyncResult
            SocketState state = (SocketState)ar.AsyncState;
            Socket theSocket = state.TheSocket;
            try
            {
                // Finalize the recieve process and get bytes of the message
                int numBytes = theSocket.EndReceive(ar);
                
                // If the bytes is 0 then throw an exception
                if (numBytes == 0)
                {
                    throw new Exception();
                }

                // Converts message from bytes into a string
                string message = Encoding.UTF8.GetString(state.buffer, 0, numBytes);

                // Appends the message on the SocketState's string builder
                lock (state.data)
                {
                    state.data.Append(message);
                }

                // Invokes the SocketState's OnNetworkAction delegate passing in the current SocketState
                state.OnNetworkAction(state);
            }
            catch
            {
                // Error Process:
                // Sets ErrorOccurred to true with proper error message then invokes OnNetworkAction
                // with the SocketState state
                state.ErrorMessage = "Error receiving message from the client";
                state.ErrorOccurred = true;
                state.OnNetworkAction(state);
            }
            
        }

        /// <summary>
        /// Begin the asynchronous process of sending data via BeginSend, using SendCallback to finalize the send process.
        /// 
        /// If the socket is closed, does not attempt to send.
        /// 
        /// If a send fails for any reason, this method ensures that the Socket is closed before returning.
        /// </summary>
        /// <param name="socket">The socket on which to send the data</param>
        /// <param name="data">The string to send</param>
        /// <returns>True if the send process was started, false if an error occurs or the socket is already closed</returns>
        public static bool Send(Socket socket, string data)
        {
            try
            {
                // Converts the string data into and array of bytes
                byte[] messageBytes = Encoding.UTF8.GetBytes(data);
                
                // If the socket is still connected then begin the sending process otherwise return false and don't start the sending process
                if (socket.Connected)
                {
                    socket.BeginSend(messageBytes, 0, messageBytes.Length, SocketFlags.None, SendCallback, socket);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            // If an exception is caught then the socket is closed and returns false
            catch
            {
                socket.Close();
                return false;
            }
        }

        /// <summary>
        /// To be used as the callback for finalizing a send operation that was initiated by Send.
        ///
        /// Uses EndSend to finalize the send.
        /// 
        /// This method must not throw, even if an error occurred during the Send operation.
        /// </summary>
        /// <param name="ar">
        /// This is the Socket (not SocketState) that is stored with the callback when
        /// the initial BeginSend is called.
        /// </param>
        private static void SendCallback(IAsyncResult ar)
        {
            // Gets the Socket/SocketState from the IAsyncResult
            Socket theSocket = (Socket)ar.AsyncState;

            // Finalize the send process 
            theSocket.EndSend(ar);
        }


        /// <summary>
        /// Begin the asynchronous process of sending data via BeginSend, using SendAndCloseCallback to finalize the send process.
        /// This variant closes the socket in the callback once complete. This is useful for HTTP servers.
        /// 
        /// If the socket is closed, does not attempt to send.
        /// 
        /// If a send fails for any reason, this method ensures that the Socket is closed before returning.
        /// </summary>
        /// <param name="socket">The socket on which to send the data</param>
        /// <param name="data">The string to send</param>
        /// <returns>True if the send process was started, false if an error occurs or the socket is already closed</returns>
        public static bool SendAndClose(Socket socket, string data)
        {
            try
            {
                // Converts the string data into and array of bytes
                byte[] messageBytes = Encoding.UTF8.GetBytes(data);

                //If socket is connected, starts the send, else method simply returns false and does not send any data
                if (socket.Connected)
                {
                    socket.BeginSend(messageBytes, 0, messageBytes.Length, SocketFlags.None, SendAndCloseCallback, socket);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            // If an exception is caught then the socket is closed and returns false
            catch
            {
                socket.Close();
                return false;
            }

        }

        /// <summary>
        /// To be used as the callback for finalizing a send operation that was initiated by SendAndClose.
        ///
        /// Uses EndSend to finalize the send, then closes the socket.
        /// 
        /// This method must not throw, even if an error occurred during the Send operation.
        /// 
        /// This method ensures that the socket is closed before returning.
        /// </summary>
        /// <param name="ar">
        /// This is the Socket (not SocketState) that is stored with the callback when
        /// the initial BeginSend is called.
        /// </param>
        private static void SendAndCloseCallback(IAsyncResult ar)
        {
            // Gets the Socket/SocketState from the IAsyncResult
            Socket theSocket = (Socket)ar.AsyncState;

            // Finalizes the sending process
            theSocket.EndSend(ar);
            theSocket.Close();  //Closes the socket after send connection is finalized for SendAndClose
        }

    }


    /// <summary>
    /// Helper class that allows us to pass in a TcpListener and an Action<SocketState> delegate
    /// This helps for moving from StartServer to AcceptNewClient
    /// </summary>
    public class ListenerState
    {
        // TcpListener for the server
        public readonly TcpListener listener;

        // toCall passed in from method
        public Action<SocketState> networkAction
        {
            private set;
            get;
        }

        public ListenerState(TcpListener listen, Action<SocketState> toCall)
        {
            listener = listen;
            networkAction = toCall;
        }
    }
}

