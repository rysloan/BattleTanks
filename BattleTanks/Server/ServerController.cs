using System;
using System.Collections.Generic;
using System.Text;
using TankWars;
using NetworkUtil;
using System.Text.RegularExpressions;
using System.Threading;
using System.Diagnostics;
using Newtonsoft.Json;

namespace TankWars
{
    /// <summary>
    /// ServerController does the network related connections and updates,recieves, and sends information
    /// which can be processed into movement and such.
    /// </summary>
    class ServerController
    {
        private Settings settings { get; set; }
        private int playerIDcount { get; set; } = 0;

        private Dictionary<int, SocketState> players;

        private World serverWorld;

        private string WallStartupInfo;

        //--Server & Client connection process--
        //--------------------------------------

        /// <summary>
        /// Adds the wall information to the server's world and gets ready to send the
        /// world size to the client with a string builder that has been created
        /// </summary>
        /// <param name="serverSettings">settings for the server</param>
        public ServerController(Settings serverSettings)
        {
            settings = serverSettings;
            players = new Dictionary<int, SocketState>();
            serverWorld = new World(settings.worldSize);

            lock (serverWorld)
            {
                foreach (Wall w in settings.serverWorld.walls.Values)
                {
                    serverWorld.walls[w.ID] = w;
                }
            }

            StringBuilder sb = new StringBuilder();
            sb.Append(serverWorld.size + "\n");
            foreach (Wall w in serverWorld.walls.Values)
            {
                sb.Append(w.ToString());
            }
            WallStartupInfo = sb.ToString();
        }

        /// <summary>
        /// Starts the network connection between the client and the server
        /// </summary>
        public void Start()
        {
            Networking.StartServer(ConnectClient, 11000);
            Thread t = new Thread(Update);
            t.Start();
            Console.WriteLine("Waiting for client");
        }

        /// <summary>
        /// Update constantly runs a loop that appends information from the model components
        /// into the string and thenn sends that information by the frame to the client.
        /// </summary>
        private void Update()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            while (true)
            {
                while (watch.ElapsedMilliseconds < settings.MSPerFrame)
                    ;

                watch.Restart();
                StringBuilder sb = new StringBuilder();
                lock (serverWorld)
                {
                    serverWorld.Update();
                    foreach (Tank t in serverWorld.tanks.Values)
                    {
                        sb.Append(t.ToString());
                    }
                    foreach (Projectile p in serverWorld.projectiles.Values)
                    {
                        sb.Append(p.ToString());
                    }
                    foreach (Powerup p in serverWorld.powerups.Values)
                    {
                        sb.Append(p.ToString());
                    }
                    foreach (Beam b in serverWorld.beams.Values)
                    {
                        sb.Append(b.ToString());
                    }
                }
                string frame = sb.ToString();
                
                lock (players)
                {
                    foreach (SocketState client in players.Values)
                    {
                        Networking.Send(client.TheSocket, frame);
                    }
                }
            }
        }

        /// <summary>
        /// Connects the client to the server unless there is an issue. If there's
        /// an issue the client socket is closed and an error message is sent to
        /// the console
        /// </summary>
        /// <param name="client"></param>
        public void ConnectClient(SocketState client)
        {
            if (client.ErrorOccurred)
            {
                if (!(client.TheSocket is null))
                {
                    client.TheSocket.Close();
                }
                //Informs the view about the error
                Console.WriteLine("A client has falied to connect to the server");
                return;
            }
            client.OnNetworkAction = registerPlayer;
            Networking.GetData(client);
        }

        /// <summary>
        /// Takes the JSON information sent by the client and deserializes it before it's send to 
        /// the ConrolCommands method to be saved.
        /// </summary>
        /// <param name="client">The client that is sending the input information</param>
        public void ProcessMovementUpdates(SocketState client)
        {
            if (client.ErrorOccurred)
            {
                lock (players)
                {
                    players.Remove((int)client.ID);
                    serverWorld.tanks[(int)client.ID].disconnected = true;
                }
                if (!(client.TheSocket is null))
                {
                    client.TheSocket.Close();
                }
                //Informs the view about the error
                Console.WriteLine("Player " + serverWorld.tanks[(int)client.ID].name + " has disconnected from the server");
                return;
            }
            string data = client.GetData();
            string[] dataParts = Regex.Split(data, @"(?<=[\n])");

            List<string> jsonMessages = new List<string>();

            foreach (string update in dataParts)
            {

                if (update.Length == 0)
                    continue;

                if (update[update.Length - 1] != '\n')
                    break;

                ControlCommands cmd = JsonConvert.DeserializeObject<ControlCommands>(update);

                lock (serverWorld)
                {
                    serverWorld.movementCommands[(int)client.ID] = cmd;
                }
                client.RemoveData(0, update.Length);
            }

            Networking.GetData(client);
        }

        /// <summary>
        /// Registers a new player by getting them connected with their client and loading up the
        /// information for the new player such as their ID and spawning location before beginning sending that
        /// information to the client.
        /// </summary>
        /// <param name="client"></param>
        private void registerPlayer(SocketState client)
        {
            if(client.ErrorOccurred)
            {
                if (!(client.TheSocket is null))
                {
                    client.TheSocket.Close();
                }
                //Informs the view about the error
                Console.WriteLine("A client has falied to connect to the server");
                return;
            }
            string playerName = client.GetData();
            if (!playerName.EndsWith("\n"))
            {
                client.GetData();
                return;
            }
            client.RemoveData(0, playerName.Length);
            playerName = playerName.Trim();
            Console.WriteLine("Client Connected:" + playerName);

            Networking.Send(client.TheSocket, client.ID + "\n");
            Networking.Send(client.TheSocket, WallStartupInfo);

            lock (serverWorld)
            {
                serverWorld.tanks[(int)client.ID] = new Tank((int)client.ID, playerName, serverWorld.RandomSpawn());
            }

            lock (players)
            {
                players.Add((int)client.ID, client);
            }

            client.OnNetworkAction = ProcessMovementUpdates;
            Networking.GetData(client);
        }
    }
}

