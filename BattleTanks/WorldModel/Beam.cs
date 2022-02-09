using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using TankWars;

namespace TankWars
{
    /// <summary>
    /// Stores and retrieves the JSON information for the Beams
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Beam
    {
        /// <summary>
        /// ID of the Beam
        /// </summary>
        [JsonProperty(PropertyName = "beam")]
        public int ID { get; private set; }

        /// <summary>
        /// Owner of the Beam
        /// </summary>
        [JsonProperty(PropertyName = "owner")]
        public int owner { get; private set; }

        /// <summary>
        /// Beam's orion point on the map
        /// </summary>
        [JsonProperty(PropertyName = "org")]
        public Vector2D origin { get; private set; }

        /// <summary>
        /// Direction beam is facing when fired
        /// </summary>
        [JsonProperty(PropertyName = "dir")]
        public Vector2D direction { get; private set; }

        public int frames { get; set; }

        private static int nextID = 0;

        /// <summary>
        /// Sets the information retrieved from the controller into the
        /// corresponding fields of the JSONProperties above
        /// </summary>
        public Beam(int playerID, Vector2D org, Vector2D dir)
        {
            ID = nextID;
            owner = playerID;
            origin = org;
            direction = dir;
            frames = 0;
            nextID++;
        }

        internal bool BeamTankCollision(Beam firedBeam, Tank otherTank)
        {
            if (firedBeam.owner == otherTank.ID)
                return false;
            if (Intersects(firedBeam.origin, firedBeam.direction, otherTank.location, Tank.Size))
                return true;

            return false;

        }

        /// <summary>
        /// Determines if a ray interescts a circle
        /// </summary>
        /// <param name="rayOrig">The origin of the ray</param>
        /// <param name="rayDir">The direction of the ray</param>
        /// <param name="center">The center of the circle</param>
        /// <param name="r">The radius of the circle</param>
        /// <returns></returns>
        private static bool Intersects(Vector2D rayOrig, Vector2D rayDir, Vector2D center, double r)
        {
            // ray-circle intersection test
            // P: hit point
            // ray: P = O + tV
            // circle: (P-C)dot(P-C)-r^2 = 0
            // substituting to solve for t gives a quadratic equation:
            // a = VdotV
            // b = 2(O-C)dotV
            // c = (O-C)dot(O-C)-r^2
            // if the discriminant is negative, miss (no solution for P)
            // otherwise, if both roots are positive, hit

            double a = rayDir.Dot(rayDir);
            double b = ((rayOrig - center) * 2.0).Dot(rayDir);
            double c = (rayOrig - center).Dot(rayOrig - center) - r * r;

            // discriminant
            double disc = b * b - 4.0 * a * c;

            if (disc < 0.0)
                return false;

            // find the signs of the roots
            // technically we should also divide by 2a
            // but all we care about is the sign, not the magnitude
            double root1 = -b + Math.Sqrt(disc);
            double root2 = -b - Math.Sqrt(disc);

            return (root1 > 0.0 && root2 > 0.0);
        }

        /// <summary>
        /// Sets beam information into a Serialized JSON Object with a terminating character
        /// </summary>
        /// <returns>Serialized JSON Object for beams</returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this) + "\n";
        }

    }
}
