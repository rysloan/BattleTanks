using System;
using System.Collections.Generic;
using TankWars;
using NetworkUtil;
using System.Text.RegularExpressions;

///-----------------------------------------------------------------------------------------
/// Authors: Ryan Sloan & Kashish Singh
/// Start Date: 12/8/2021
/// End Date: 12/12/2021
/// 
/// Summary: This is a program that creates a client and server that uses networking to
/// communicate with eachother to create a Tank Wars game that can be played via the client.
/// To play, press "Start" in VS to run the client and server. 
/// 
///-----------------------------------------------------------------------------------------

namespace TankWars
{
    /// <summary>
    /// Server class that sends information to Settings and serverConroller to start the server
    /// </summary>
    class Server
    {
        /// <summary>
        /// Main method for the Server that gets the settings and gets the server running
        /// </summary>
        static void Main(string[] args)
        {
            Settings settings = new Settings(@"..\..\..\..\Resources\settings.xml");
            ServerController serverController = new ServerController(settings);
            serverController.Start();
            Console.Read(); // Keeps console open
        }
    }
}
