using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using NetworkUtil;
using Newtonsoft.Json;

namespace TankWars
{
    public class Controller
    {
        ///Controller events that the view can subscribe to:
        ///----------------------------------------------------
        public delegate void UpdateHandler();
        public event UpdateHandler UpdatesArrived;

        public delegate void ConnectedHandler(World world, int ID);
        public event ConnectedHandler Connected;

        public delegate void ErrorHandler(string error);
        public event ErrorHandler Error;



        /// All Server Info For Controller:
        ///---------------------------------
        private SocketState theServer = null;
        readonly int portNum = 11000; //Tank War's prot is 11000


        /// All Game Info For Controller:
        ///-------------------------------
        // Player name for this controller
        private string playerName { get; set; }
        // Player ID for this controllers player
        private int playerID = -1;
        // Keeps track of the users inputs
        private string curProj = "none";
        private Vector2D curDir = new Vector2D(1, 0);
        private List<string> movementBuffer = new List<string>() { "none" };
        // World object for this controller and player
        private World tankWars;

        public void Connect(string address, string name)
        {
            playerName = name;
            Networking.ConnectToServer(OnConnect, address, portNum);
        }

        /// <summary>
        /// Checks for the connection and sends the servers player name when it connects
        /// </summary>
        /// <param name="state">the socketstate that is connecting</param>
        private void OnConnect(SocketState state)
        {
            if(state.ErrorOccurred)
            {
                if (!(state.TheSocket is null))
                {
                    state.TheSocket.Close();
                }
                //Informs the view about the error
                Error("Error connecting to the server. Please try again and/or restart client");
                return;
            }

            theServer = state;
            state.OnNetworkAction = ReceiveUpdate;
            Networking.GetData(state);
            // Sends the server the players name so the server can start to send game info
            UpdateSend(playerName);
        }

        /// <summary>
        /// Checks socket and recieves information from the server  which is then processed
        /// with another method
        /// </summary>
        /// <param name="state">the Socketstate that has incoming information</param>
        private void ReceiveUpdate(SocketState state)
        {
            if(state.ErrorOccurred)
            {
                if (!(state.TheSocket is null))
                {
                    state.TheSocket.Close();
                }
                //Informs the view that an error has occured
                Error("Lost connection to server");
                return;
            }
            ProcessUpdates(state);
        }

        /// <summary>
        /// Takes the server information that has been sent through the socket and begins processing the
        /// message to split apart the terminating value which is \n and add to a list. Afterwads it sends the list to
        /// another method to process the JSON message
        /// <param name="state">the state that contains the information from the server</param>
        private void ProcessUpdates(SocketState state)
        {
            string data = state.GetData();
            string[] dataParts = Regex.Split(data, @"(?<=[\n])");

            List<string> jsonMessages = new List<string>();

            foreach (string update in dataParts)
            {

                if (update.Length == 0)
                    continue;

                if (update[update.Length - 1] != '\n')
                    break;

                if (playerID == -1)
                {
                    playerID = Int32.Parse(update);
                    state.RemoveData(0, update.Length);
                    continue;
                }
                else if (tankWars == null)
                {
                    tankWars = new World(Int32.Parse(update));
                    Connected(tankWars, playerID);
                    state.RemoveData(0, update.Length);
                    continue;
                }
                else
                {
                    jsonMessages.Add(update);
                    state.RemoveData(0, update.Length);
                }
            }

            if (jsonMessages.Count > 0)
                ProcessJsonMessages(jsonMessages);

            //informs the view about new messages
            UpdatesArrived();
            Networking.GetData(state);
        }

        /// <summary>
        /// Takes the various JSON fields and deserializes them before adding them to the correspoinding model
        /// information to be used later
        /// </summary>
        /// <param name="jsonMessages">list of JSON messages from the server</param>
        private void ProcessJsonMessages(List<string> jsonMessages)
        {
            lock (tankWars) {
            foreach (string json in jsonMessages)
            {

                    if (json.Contains("tank"))
                    {
                        Tank t = JsonConvert.DeserializeObject<Tank>(json);
                        tankWars.tanks[t.ID] = t;
                    }

                    if (json.Contains("wall"))
                    {
                        Wall w = JsonConvert.DeserializeObject<Wall>(json);
                        tankWars.walls[w.ID] = w;
                    }

                    if (json.Contains("proj"))
                    {
                        Projectile p = JsonConvert.DeserializeObject<Projectile>(json);
                        if (p.died)
                            tankWars.projectiles.Remove(p.ID);
                        else
                            tankWars.projectiles[p.ID] = p;
                    }

                    if (json.Contains("power"))
                    {
                        Powerup p = JsonConvert.DeserializeObject<Powerup>(json);
                        if (p.died)
                            tankWars.powerups.Remove(p.ID);
                        else
                            tankWars.powerups[p.ID] = p;
                    }

                    if (json.Contains("beam"))
                    {
                        Beam b = JsonConvert.DeserializeObject<Beam>(json);
                        tankWars.beams[b.ID] = b;
                    }

                    ControlCommands cmd = new ControlCommands(movementBuffer[movementBuffer.Count - 1], curProj, curDir);
                    UpdateSend(JsonConvert.SerializeObject(cmd));
                }
            }
        }

        /// <summary>
        /// Closes the socket to the server
        /// </summary>
        private void Close()
        {
            theServer?.TheSocket.Close();
        }

        /// <summary>
        /// Sends a message to the server using the NetworkController send method
        /// </summary>
        /// <param name="message">message to be sent to the server</param>
        public void UpdateSend(string message)
        {
            if (theServer != null)
                Networking.Send(theServer.TheSocket, message + "\n");
        }

        /// <summary>
        /// Shuts down the socket
        /// </summary>
        public void ShutDown()
        {
            if (theServer != null)
                theServer.TheSocket.Shutdown(System.Net.Sockets.SocketShutdown.Both);
        }

        /// <summary>
        /// Handels the movement of the tank based on key inputs from the view
        /// </summary>
        /// <param name="dir">direction of movement, up,down,left,right</param>
        /// <param name="keyUp">whether a key is up</param>
        public void MovementHandler(string dir, bool keyUp)
        {
            lock (movementBuffer)
            {
                if (keyUp)
                {
                    movementBuffer.Remove(dir);
                }
                else
                {
                    if (!movementBuffer.Contains(dir))
                        movementBuffer.Add(dir);
                }
            }
        }

        /// <summary>
        /// Handles a mouse button press from the view which fires a projectile from the tank
        /// </summary>
        /// <param name="proj">projectile information to be sent</param>
        /// <param name="mouseUp">Whether the mouse is up</param>
        public void FiringHandler(string proj, bool mouseUp)
        {
            lock (curProj)
            {
                if (mouseUp)
                {
                    curProj = "none";
                }
                else
                {
                    curProj = proj;
                }
            }
        }

        /// <summary>
        /// Handels the direction of the turret based on where the mouse is on the form
        /// </summary>
        /// <param name="mouse">point where the mouse is on the form</param>
        /// <param name="tank">the point where the tank is</param>
        public void TurretDirectionHandler(Point mouse, Point tank)
        {
            Vector2D angle = new Vector2D(mouse.X, mouse.Y) - new Vector2D(tank.X, tank.Y);
            angle.Normalize();
            lock (curDir)
            {
                curDir = angle;
            }
        }
    }
}
