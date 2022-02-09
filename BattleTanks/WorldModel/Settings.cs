using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace TankWars
{
    public class Settings
    {
        public World serverWorld;

        // All entities sizes
        internal const double TankSize = 60;    //Pixel size of Tank
        internal const double WallSize = 50;    //,Pixel size of wall, etc...
        internal const double ProjectileSize = 30;
        internal const double PowerupSize = 12;

        public int worldSize { get; set; }
        public int MSPerFrame { get; set; }
        public int tankSpawnDelay { get; set; }
        public int FramesPerShot { get; set; }

        internal const int tankHP = 3;

        internal const int tankSpeed = 3;

        internal const int projectileSpeed = 25;

        internal const int MaxWorldPowerupCount = 3;

        internal const int powreupSpawnDelay = 1650;

        internal const int fireRate = 80;

        internal const int MaxPowerupsPerPlayer = 2;

        internal const int TankSpawnDelay = 300;

        /// <summary>
        /// Settings creates a file reader that reads the filepath given and sets the appropriate information into
        /// the corresponding Settings field.
        /// </summary>
        /// <param name="filepath">File path for the Settings.xml file to be read</param>
        public Settings (string filepath)
        {
            if (filepath is null || filepath == "")  //If file is missing or the path is empty
            {
                Console.Write("File is missing or filepath is empty!");
                return; //returns nothing and server uses default values from this class
            }

            int xep1, yep1, xep2, yep2;
            xep1 = yep1 = xep2 = yep2 = int.MaxValue-500;   //Rediculusly high number that the universe size should never reach
            int wallID = 0;

            try
            {
                using (XmlReader reader = XmlReader.Create(filepath))
                {
                    while(reader.Read())
                    {
                        if(reader.IsStartElement())
                        {
                            switch (reader.Name)
                            {
                                case "UniverseSize":
                                    worldSize = int.Parse(reader.ReadElementContentAsString());
                                    serverWorld = new World(worldSize);
                                    break;

                                case "MSPerFrame":
                                    MSPerFrame = int.Parse(reader.ReadElementContentAsString());
                                    break;

                                case "FramesPerShot":
                                    FramesPerShot = int.Parse(reader.ReadElementContentAsString());
                                    break;

                                case "RespawnRate":
                                    tankSpawnDelay = int.Parse(reader.ReadElementContentAsString());
                                    break;

                                case "Wall":

                                    bool breakLoop = false;

                                    while (!breakLoop)  //While the loop is running, goes through all elements of the wall until it hits wall end element
                                    {
                                        if (reader.NodeType == XmlNodeType.EndElement)
                                        {
                                            if (reader.Name == "Wall")   //If reaches wall end element, breaks out of loop
                                            {
                                                breakLoop = true;
                                                break;
                                            }
                                        }

                                        switch (reader.Name)
                                        {
                                            case "p1":
                                                reader.Read();
                                                if (reader.Name == "x" && reader.NodeType != XmlNodeType.EndElement)
                                                    xep1 = int.Parse(reader.ReadElementContentAsString());
                                                if (reader.Name == "y" && reader.NodeType != XmlNodeType.EndElement)
                                                    yep1 = int.Parse(reader.ReadElementContentAsString());
                                                reader.Read();
                                                break;

                                            case "p2":
                                                reader.Read();
                                                if (reader.Name == "x" && reader.NodeType != XmlNodeType.EndElement)
                                                    xep2 = int.Parse(reader.ReadElementContentAsString());
                                                if (reader.Name == "y" && reader.NodeType != XmlNodeType.EndElement)
                                                    yep2 = int.Parse(reader.ReadElementContentAsString());
                                                reader.Read();
                                                break;
                                        }
                                        reader.Read();
                                    }

                                    //Once all the wall information has been gathered...
                                    Vector2D ep1 = new Vector2D(xep1, yep1);    //Creates the wall's 1st endpoint
                                    Vector2D ep2 = new Vector2D(xep2, yep2);    //creates the wall's 2nd endpoint
                                    Wall newWall = new Wall(wallID, ep1, ep2);  //creates a wall with the endpoints

                                    serverWorld.walls.Add(wallID, newWall);     //adds the new wall to the world's wall dictionary with wallID as key

                                    wallID++;   //updates wallID so it goes to the next integer and isn't the same

                                    break;  //Finally exits the current "Wall" Element and moves on
                            }
                            //reader.Read();
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Console.Write("The file contents could not be fully loaded: " + e.Message);
                return; //Ends reading and server uses default values for whatever things haven't
                        //been replaced yet (NOTE: walls beyond error point will NOT be drawn!)
            }
        }

    }
}
