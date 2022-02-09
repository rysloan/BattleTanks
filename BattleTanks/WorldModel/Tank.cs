using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TankWars;

namespace TankWars
{
    /// <summary>
    /// Stores and retrieves all the tank information
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Tank
    {
        internal const double Size = Settings.TankSize; //Size of the tank image (60x60) pixels

        /// <summary>
        /// Unique ID of the tank
        /// </summary>
        [JsonProperty(PropertyName = "tank")]
        public int ID { get; private set; }

        /// <summary>
        /// Name of the player/tank
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string name { get; private set; }

        /// <summary>
        /// Location of the tank on the map
        /// </summary>
        [JsonProperty(PropertyName = "loc")]
        public Vector2D location { get; internal set; }

        /// <summary>
        /// Direction of the body of the tank
        /// </summary>
        [JsonProperty(PropertyName = "bdir")]
        public Vector2D orientation { get; set; }

        /// <summary>
        /// Direction of the turret of the tank
        /// </summary>
        [JsonProperty(PropertyName = "tdir")]
        public Vector2D direction { get; set; }

        /// <summary>
        /// Score of the player/tank
        /// </summary>
        [JsonProperty(PropertyName = "score")]
        public int score { get; set; }

        /// <summary>
        /// Health points of the tank
        /// </summary>
        [JsonProperty(PropertyName = "hp")]
        public int health { get; set; }

        /// <summary>
        /// Whether the tank has "died" or not
        /// </summary>
        [JsonProperty(PropertyName = "died")]
        public bool died { get; set; }

        /// <summary>
        /// Whether the tank has disconnected from the
        /// game
        /// </summary>
        [JsonProperty(PropertyName = "dc")]
        public bool disconnected { get; set; }

        /// <summary>
        /// Whether a tank has joined the game
        /// </summary>
        [JsonProperty(PropertyName = "join")]
        public bool joined { get; set; }

        internal Vector2D velocity { get; set; }

        internal const int tankSpeed = Settings.tankSpeed;

        internal const int StartingTankHP = Settings.tankHP;

        internal int projFrames { get; set; } = Settings.fireRate;

        internal const int spawnDelay = Settings.TankSpawnDelay;

        internal int deathFrames { get; set; } = 0; //Used to keep track of how many frames the tank is dead for

        internal int powerupCount { get; set; } = 0; //Used to keep track of how many powerups the tank has

        /// <summary>
        /// Stores the information recieved from the controller into the
        /// proper fields
        /// </summary>
        public Tank(int tankID, string playerName, Vector2D loc)
        {
            ID = tankID;
            name = playerName;
            location = loc;
            health = StartingTankHP;
            joined = true;
            disconnected = false;
            died = false;
            orientation = new Vector2D(1, 0);
            direction = new Vector2D(1, 0);
            score = 0;
            velocity = new Vector2D(0, 0);
        }

        /// <summary>
        /// Returns a string with serialized JSON info and a terminating character
        /// </summary>
        /// <returns>JSON serialized objecct for Tanks</returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this) + "\n";
        }


        /// <summary>
        /// Checks for collision between a projectile and a tank
        /// </summary>
        /// <param name="projLoc">the location of the projectile being checked</param>
        /// <returns>returns true if there is a collision with the tank</returns>
        public bool ProjectileCollision(Vector2D projLoc)
        {
            double top, bottom, left, right;
            //takes leftmost point of the wall and checks 
            left = location.GetX() - (Size / 2);
            right = location.GetX() + (Size / 2);
            top = location.GetY() - (Size / 2);
            bottom = location.GetY() + (Size / 2);

            return left < projLoc.GetX()
                && projLoc.GetX() < right
                && top < projLoc.GetY()
                && projLoc.GetY() < bottom;
        }

    }
}
