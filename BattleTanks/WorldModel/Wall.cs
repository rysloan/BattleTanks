using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using TankWars;

namespace TankWars
{
    /// <summary>
    /// Stores the information about the walls
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Wall
    {
        private const double thickness = Settings.WallSize;
        //public double top, bottom, left, right;

        /// <summary>
        /// ID of the wall
        /// </summary>
        [JsonProperty(PropertyName = "wall")]
        public int ID { get; set; }

        /// <summary>
        /// 1st endpoint vector location of the wall
        /// </summary>
        [JsonProperty(PropertyName = "p1")]
        public Vector2D endpoint1 { get; set; }

        /// <summary>
        /// 2nd endpoint vector location of the wall
        /// </summary>
        [JsonProperty(PropertyName = "p2")]
        public Vector2D endpoint2 { get; set; }

        /// <summary>
        /// Stores the wall information sent by the server into the propper
        /// fields
        /// </summary>
        public Wall(int wallID, Vector2D p1, Vector2D p2)
        {
            ID = wallID;
            endpoint1 = p1;
            endpoint2 = p2;

        }

        /// <summary>
        /// Sends back a string JSON Object inforamtion and a terminating charcter
        /// </summary>
        /// <returns>Serialized JSON object for Walls</returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this) + "\n";
        }

        /// <summary>
        /// Checks for collision between a tank and a wall. Uses half of the tank's width as an
        /// extension to avoid clipping issues.
        /// </summary>
        /// <param name="tankLoc">Location of the tank being checked</param>
        /// <returns>Returns true if a collision with a wall is detected</returns>
        public bool TankCollision(Vector2D tankLoc)
        {
            double expansion = thickness / 2 + Tank.Size / 2;
            double top, bottom, left, right;
            //takes leftmost point of the wall and checks 
            left = Math.Min(endpoint1.GetX(), endpoint2.GetX()) - expansion;
            right = Math.Max(endpoint1.GetX(), endpoint2.GetX()) + expansion;
            top = Math.Min(endpoint1.GetY(), endpoint2.GetY()) - expansion;
            bottom = Math.Max(endpoint1.GetY(), endpoint2.GetY()) + expansion;

            return left < tankLoc.GetX() 
                && tankLoc.GetX() < right
                && top < tankLoc.GetY()
                && tankLoc.GetY() < bottom;
        }

        /// <summary>
        /// Checks for collision with a projectile and a tank while using an expansion to avoid
        /// clipping issues.
        /// </summary>
        /// <param name="projLoc">Location of the projectile being checked</param>
        /// <returns>Returns true if there is a collision with a projectile detected</returns>
        public bool ProjectileCollision(Vector2D projLoc)
        {
            double expansion = thickness / 2 + Projectile.Size / 4;
            double top, bottom, left, right;
            //takes leftmost point of the wall and checks 
            left = Math.Min(endpoint1.GetX(), endpoint2.GetX()) - expansion;
            right = Math.Max(endpoint1.GetX(), endpoint2.GetX()) + expansion;
            top = Math.Min(endpoint1.GetY(), endpoint2.GetY()) - expansion;
            bottom = Math.Max(endpoint1.GetY(), endpoint2.GetY()) + expansion;
            
            return left < projLoc.GetX()
                && projLoc.GetX() < right
                && top < projLoc.GetY()
                && projLoc.GetY() < bottom;
        }

        /// <summary>
        /// Checks for a collision with a powerup and uses expansion to avoid clipping issues
        /// </summary>
        /// <param name="powLoc">Location of the powerup being checked</param>
        /// <returns>Returns true if there is a collision with a powerup detected</returns>
        public bool PowerupCollision(Vector2D powLoc)
        {
            double expansion = thickness / 2 + Powerup.Size / 2;
            double top, bottom, left, right;
            //takes leftmost point of the wall and checks 
            left = Math.Min(endpoint1.GetX(), endpoint2.GetX()) - expansion;
            right = Math.Max(endpoint1.GetX(), endpoint2.GetX()) + expansion;
            top = Math.Min(endpoint1.GetY(), endpoint2.GetY()) - expansion;
            bottom = Math.Max(endpoint1.GetY(), endpoint2.GetY()) + expansion;

            return left < powLoc.GetX()
                && powLoc.GetX() < right
                && top < powLoc.GetY()
                && powLoc.GetY() < bottom;
        }
    }
}
