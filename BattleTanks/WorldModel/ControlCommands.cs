using System;
using Newtonsoft.Json;
using TankWars;

namespace TankWars
{
    /// <summary>
    /// ControlCommands takes recieved movement commands sent and keeps track
    /// of them as JsonObjects
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class ControlCommands
    {
        // Keeps track of this players movement inputs
        // Possible values are : "none", "up", "down", "left", "right"
        [JsonProperty(PropertyName = "moving")]
        public string moving { get; private set; }

        // Keeps track of this player firing inputs
        // Possible values are : "none", "main"(main projectile), "alt"(beam projectile)
        [JsonProperty(PropertyName = "fire")]
        public string firing { get; private set; }

        // Keeps track of where the player is aiming
        [JsonProperty(PropertyName = "tdir")]
        public Vector2D aiming { get; private set; }

        public ControlCommands(string movement, string fire, Vector2D dir)
        {
            moving = movement;
            firing = fire;
            aiming = dir;
        }
    }
}
