using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;

namespace TankWars
{
    public partial class Form1 : Form
    {
        //Controller handles updates from the server and notifies view with an event
        private Controller controller;

        //World is a container for various objects (like player, powerups, etc.)
        //the controller owns the world but we have a reference to it so it can be used
        private World theWorld;

        private int playerID;

        private DrawingPanel worldDrawer;


        public Form1()
        {
            InitializeComponent();
            controller = new Controller();
            ClientSize = new Size(900, 950);
            controller.Error += ShowError;
            controller.Connected += HandleConnected;
            controller.UpdatesArrived += UpdateFrame;
            FormClosed += OnExit;
            worldDrawer = new DrawingPanel(theWorld);
            worldDrawer.MouseDown += playerMouseDown;
            worldDrawer.MouseUp += playerMouseUp;
            worldDrawer.MouseMove += playerMouseMove;
            worldDrawer.Location = new Point(0, 50);
            worldDrawer.Size = new Size(900, 900);
            this.Controls.Add(worldDrawer);
        }

        /// <summary>
        /// When client connects to server, displays a message and holds the information for playerID
        /// and world
        /// </summary>
        /// <param name="world">World class to hold all the model info for the game</param>
        /// <param name="ID">ID of the player connecting with the client</param>
        private void HandleConnected(World world, int ID)
        {
            Console.WriteLine("Connected");
            theWorld = world;
            playerID = ID;
            worldDrawer.playerID = playerID;
            worldDrawer.theWorld = theWorld;
        }

        /// <summary>
        /// Displays a message box with error information
        /// </summary>
        /// <param name="error">error information given</param>
        private void ShowError(string error)
        {
            MessageBox.Show(error);
            this.Invoke(new MethodInvoker(() =>
           {
               connectButton.Enabled = true;
               serverAddressTextBox.Enabled = true;
               playerNameLabel.Enabled = true;
           }));
        }

        /// <summary>
        /// Tells the controller to shutdown when the form is exited so that it shuts down
        /// the connection
        /// </summary>
        private void OnExit(object sender, EventArgs e)
        {
            controller.ShutDown();
        }

        /// <summary>
        /// When the connect button is clicked, sends the information in the text boxes to the controller
        /// or displays a message box if user has not typed what is requiered and then locks the boxes when
        /// entries are done
        /// </summary>
        private void connectButton_Click(object sender, EventArgs e)
        {
            if(serverAddressTextBox.Text == "")
            {
                MessageBox.Show("Please enter a server address");
                return;
            }
            if (playerNameTextBox.Text == "")
            {
                MessageBox.Show("Please enter a player name");
                return;
            }
            if (playerNameTextBox.Text.Length > 16)
            {
                MessageBox.Show("Player Name must be 16 characters or less");
                return;
            }

            serverAddressTextBox.Enabled = false;
            playerNameTextBox.Enabled = false;
            connectButton.Enabled = false;

            controller.Connect(serverAddressTextBox.Text, playerNameTextBox.Text);
        }

        /// <summary>
        /// Checks for the keys that are used for movemtn and if they've been pressed down
        /// WASD keys are used for movement
        /// </summary>
        private void playerKeyDown(object sender, KeyEventArgs e)
        {
            e.SuppressKeyPress = true;
            switch (e.KeyCode)
            {
                case (Keys.W):
                    controller.MovementHandler("up", false);
                    break;
                case (Keys.S):
                    controller.MovementHandler("down", false);
                    break;
                case (Keys.A):
                    controller.MovementHandler("left", false);
                    break;
                case (Keys.D):
                    controller.MovementHandler("right", false);
                    break;
            }
        }

        /// <summary>
        /// Checks for the keys for movement and if they've been pressed up
        /// WASD keys for movement
        /// </summary>
        private void playerKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case (Keys.W):
                    controller.MovementHandler("up", true);
                    break;
                case (Keys.S):
                    controller.MovementHandler("down", true);
                    break;
                case (Keys.A):
                    controller.MovementHandler("left", true);
                    break;
                case (Keys.D):
                    controller.MovementHandler("right", true);
                    break;
            }
        }

        /// <summary>
        /// Checks if the player mouse has been pressed down and sends that information to the 
        /// controller
        /// </summary>
        private void playerMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
                controller.FiringHandler("main", false);
            if (e.Button == MouseButtons.Right)
                controller.FiringHandler("alt", false);
        }

        /// <summary>
        /// Checks if the player mouse has been pressed up nd sends that information to the controller
        /// so that it does not keep firing when mouse is up
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void playerMouseUp(object sender, MouseEventArgs e)
        {
            controller.FiringHandler("none", true);
        }

        /// <summary>
        /// Checks the position of the mouse on the world frame and sends that information to the controller
        /// so that the turret of the tank can be moved
        /// </summary>
        private void playerMouseMove(object sender, MouseEventArgs e)
        {
            controller.TurretDirectionHandler(e.Location, new Point(worldDrawer.Width / 2, worldDrawer.Height / 2));
        }

        /// <summary>
        /// Updates the frame of the game
        /// </summary>
        private void UpdateFrame()
        {
            try
            {
                this.Invoke(new MethodInvoker(() => Invalidate(true)));
            }
            catch
            {
            }
        }
    }
}
