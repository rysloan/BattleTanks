using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using TankWars;

namespace TankWars
{

    /// <summary>
    /// Stores and retrieves the projectile information that has
    /// been recieved
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Projectile
    {
        /// <summary>
        /// Projectile ID
        /// </summary>
        [JsonProperty(PropertyName = "proj")]
        public int ID { get; private set; }

        /// <summary>
        /// Whether or not the projectile has disappeared
        /// </summary>
        [JsonProperty(PropertyName = "died")]
        public bool died { get; internal set; }

        /// <summary>
        /// Location of the projectile
        /// </summary>
        [JsonProperty(PropertyName = "loc")]
        public Vector2D location { get; internal set; }

        /// <summary>
        /// Who owns the projectile/who shot it
        /// </summary>
        [JsonProperty(PropertyName = "owner")]
        public int owner { get; private set; }

        /// <summary>
        /// the direction where the projectile is shot
        /// </summary>
        [JsonProperty(PropertyName = "dir")]
        public Vector2D direction { get; private set; }

        internal const int projectileSpeed  = Settings.projectileSpeed; //Stores projectile speed from settings

        private static int nextID = 0; //Id value that is updated when new projectiles are added or removed

        internal Vector2D velocity { get; set; }

        internal const double Size = Settings.ProjectileSize;

        /// <summary>
        /// Stoes the information from the controller into their correspoinding
        /// fields
        /// </summary>
        public Projectile(int playerID, bool dead, Vector2D loc, Vector2D dir)
        {
            ID = nextID;
            owner = playerID;
            died = dead;
            location = loc;
            direction = dir;
            nextID++;
            velocity = new Vector2D(0, 0);
        }

        /// <summary>
        /// Returns a string with JSON Serilized object along with a terminator character
        /// </summary>
        /// <returns>Serialized JSON object for Projectiles</returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this) + "\n";
        }
    }
}
