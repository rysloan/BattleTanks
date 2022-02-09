using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using TankWars;

namespace TankWars
{
    /// <summary>
    /// Stores and retrieves the information of the Powerups
    /// recieved
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Powerup
    {
        /// <summary>
        /// ID of the powerup
        /// </summary>
        [JsonProperty(PropertyName = "power")]
        public int ID { get; private set; }

        /// <summary>
        /// Whether the powerup has dissapeared or not
        /// </summary>
        [JsonProperty(PropertyName = "died")]
        public bool died { get; internal set; }

        /// <summary>
        /// Location of powerup on the map
        /// </summary>
        [JsonProperty(PropertyName = "loc")]
        public Vector2D location { get; private set; }

        internal const double Size = Settings.PowerupSize;

        private static int nextID = 0;

        /// <summary>
        /// Stores the information from the controller into the corresponding
        /// properties
        /// </summary>
        public Powerup(bool dead, Vector2D loc)
        {
            ID = nextID;
            died = dead;
            location = loc;
            nextID++;
        }

        /// <summary>
        /// ToSring to take JSON objects and serialize them along with a terminating
        /// character
        /// </summary>
        /// <returns>The serialized JSON information for Powerups</returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this) + "\n";
        }

    }
}
