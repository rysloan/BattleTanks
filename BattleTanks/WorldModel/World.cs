using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using TankWars;

namespace TankWars
{

    /// <summary>
    /// Stores the world information that is used in the game and its
    /// various components
    /// </summary>
    public class World
    {

        public double top, bottom, left, right; //Edges of the world

        /// Dictionaries that hold all objects for this World
        public Dictionary<int, Tank> tanks;

        public Dictionary<int, Wall> walls;

        public Dictionary<int, Beam> beams;
        
        public Dictionary<int, Projectile> projectiles;
         
        public Dictionary<int, Powerup> powerups;

        public Dictionary<int, ControlCommands> movementCommands;



        // Size of this World
        public int size { get; private set; } = 0;

        private const int maxPowerupsInWorld = Settings.MaxWorldPowerupCount;

        private int powerupsInWorld { get; set; } = 0;

        private const int powerupSpawnDelay = Settings.powreupSpawnDelay;

        private int powerupSpawnFrames { get; set; } = 0;

        /// <summary>
        /// Adds the various pieces of the world into their own dictionary
        /// so you can have multiples of the same thing so they can be processed
        /// </summary>
        /// <param name="worldSize">size of the world as sent by the server</param>
        public World(int worldSize)
        {
            size = worldSize;
            tanks = new Dictionary<int, Tank>();
            walls = new Dictionary<int, Wall>();
            beams = new Dictionary<int, Beam>();
            projectiles = new Dictionary<int, Projectile>();
            powerups = new Dictionary<int, Powerup>();
            movementCommands = new Dictionary<int, ControlCommands>();

            left = (-size / 2) + (Tank.Size / 2);
            right = (size / 2) - (Tank.Size / 2);
            top = (-size / 2) + (Tank.Size / 2);
            bottom = (size / 2) - (Tank.Size / 2);
        }

        /// <summary>
        /// Updates the world by calling all the verious methods to handle despawning,
        /// controls, projectile frame updates, and spawning powerups
        /// </summary>
        public void Update()
        {
            // Starts by despawning (removing from the dictionary) all entities with their died property set to true apart from tanks
            DespawnEntities();

            // Handles the Control Commands sent from the clients to the server
            TankCommandsHandler();
            
            // Updates the projectile frame by frame
            UpdateProjectiles();

            // Spawns powerups around the map at random locations
            SpawnPowerups();
        }

        /// <summary>
        /// Updates projectile location information and calles ProjectileCollision()
        /// to handle any collisons
        /// </summary>
        private void UpdateProjectiles()
        {
            foreach(Projectile p in projectiles.Values)
            {
                p.velocity = p.direction;
                p.velocity.Normalize();
                p.velocity *= Projectile.projectileSpeed;
                p.location += p.velocity; 
            }
            ProjectileCollision();
        }

        /// <summary>
        /// Checks all projectiles and checks for collision with the world border or 
        /// all the walls. If detected the projectile is set to "died" and removed
        /// </summary>
        private void ProjectileCollision()
        {
            foreach (Projectile p in projectiles.Values)
            {
                if (p.location.GetX() > size / 2 || p.location.GetX() < -size / 2 || p.location.GetY() > size / 2 || p.location.GetY() < -size / 2)
                    p.died = true;

                foreach (Wall w in walls.Values)
                {
                    if (w.ProjectileCollision(p.location))
                        p.died = true;
                }
            }

        }

        /// <summary>
        /// Checks if a Powerup can spawn and if so, spawns a projectile in a random
        /// location while giving it an ID and increasing the count for powerups in the world
        /// as well as setting the spawn frames to zero
        /// </summary>
        private void SpawnPowerups()
        {
            powerupSpawnFrames++;
            if (powerupsInWorld == maxPowerupsInWorld)
                return;
            if (powerupSpawnFrames < powerupSpawnDelay)
                return;
            Powerup p = new Powerup(false, RandomSpawn());
            powerups[p.ID] = p;
            powerupSpawnFrames = 0;
            powerupsInWorld++;
        }

        /// <summary>
        /// If a beam collids with a Tank and their ID's don't match (tank shooting itself),
        /// the tank's health is set to 0, "died" set to true, starts the tankDeathFrames,
        /// and increases the score of the beam's owner
        /// </summary>
        /// <param name="t">the tank being destroyed</param>
        /// <param name="b">the beam that has collided</param>
        private void BeamCollision(Tank t, Beam b)
        {
            if(b.BeamTankCollision(b, t) && b.ID != t.ID)
            {
                t.health = 0;
                t.died = true;
                t.deathFrames++;
                tanks[b.owner].score++;
            }
        }

        /// <summary>
        /// If a tank collids with a powerup, tank's powoerup count increases, the powerup is
        /// set to "died", and the counter for powerupsinworld is decreased
        /// </summary>
        /// <param name="t">The tank collecting the powerup</param>
        /// <param name="p">The powerup that is being collected</param>
        private void PowerupCollision(Tank t, Powerup p)
        {
            if (t.ProjectileCollision(p.location))
            {
                if (t.powerupCount != Settings.MaxPowerupsPerPlayer)
                {
                    t.powerupCount++;
                    p.died = true;
                    powerupsInWorld--;
                }
            }
        }

        /// <summary>
        /// Checks for world collision by a tank and if it occurs, wraps the tank to 
        /// the other side of the world
        /// </summary>
        /// <param name="t">Tank being checked for world edge collision</param>
        private void IsWrapAround(Tank t)
        {
            double tankX = t.location.GetX();
            double tankY = t.location.GetY();

            if (t.location.GetX() < left || t.location.GetX() > right)
            {
                Vector2D newTankLoc = new Vector2D(-tankX, tankY);
                t.location = newTankLoc;
            }
            else if (t.location.GetY() < top || t.location.GetY() > bottom)
            {
                Vector2D newTankLoc = new Vector2D(tankX, -tankY);
                t.location = newTankLoc;
            }
        }

        /// <summary>
        /// Despawns entities if their "died" value is true, also checks for Tank
        /// disconnects and removes tanks from the Tank Dictionary
        /// </summary>
        private void DespawnEntities()
        {
            foreach (Projectile p in projectiles.Values)
            {
                if (p.died)
                    projectiles.Remove(p.ID);
            }
            foreach (Tank t in tanks.Values)
            {
                if (t.disconnected && t.health == 0)
                    tanks.Remove(t.ID);
                if (t.disconnected)
                {
                    t.health = 0;
                    continue;
                }
                if (t.died)
                    t.died = false;
            }
            foreach (Powerup p in powerups.Values)
            {
                if (p.died)
                    powerups.Remove(p.ID);
            }
            foreach (Beam b in beams.Values)
            {
                beams.Remove(b.ID);
            }
        }

        /// <summary>
        /// Handles the commands sent from the client for the tanks and updates their orientation
        /// and position accordingly. Also handles projectile and beam firing inputs and adds them
        /// to their dictionaries accordingly. Lastly, checks for tank collision
        /// </summary>
        private void TankCommandsHandler()
        {
            foreach (KeyValuePair<int, ControlCommands> cmd in movementCommands)
            {
                Tank t = tanks[cmd.Key];
                if (t.deathFrames > 0)
                    continue;
                switch (cmd.Value.moving)
                {
                    case "up":
                        t.velocity = new Vector2D(0, -Tank.tankSpeed);
                        t.orientation = new Vector2D(0, -1);
                        break;
                    case "down":
                        t.velocity = new Vector2D(0, Tank.tankSpeed);
                        t.orientation = new Vector2D(0, 1);
                        break;
                    case "left":
                        t.velocity = new Vector2D(-Tank.tankSpeed, 0);
                        t.orientation = new Vector2D(-1, 0);
                        break;
                    case "right":
                        t.velocity = new Vector2D(Tank.tankSpeed, 0);
                        t.orientation = new Vector2D(1, 0);
                        break;
                    default:
                        t.velocity = new Vector2D(0, 0);
                        break;
                }
                t.direction = cmd.Value.aiming;

                switch (cmd.Value.firing)
                {
                    case "main":
                        if (t.projFrames >= Settings.fireRate)
                        {
                            Projectile p = new Projectile(t.ID, false, t.location, t.direction);
                            projectiles[p.ID] = p;
                            t.projFrames = 0;
                            break;
                        }
                        else
                            break;
                    case "alt":
                        if (t.powerupCount > 0)
                        {
                            Beam b = new Beam(t.ID, t.location, t.direction);
                            beams[b.ID] = b;
                            t.powerupCount--;
                        }
                        break;
                    default:
                        break;
                }
                t.projFrames++;
            }
            movementCommands.Clear();

            TankCollision();
        }

        /// <summary>
        /// Handles all tank collisions with various objects and updates the informatiion accordingly
        /// based on what object was hit by the tank.
        /// </summary>
        private void TankCollision()
        {
            foreach (Tank t in tanks.Values)
            {
                if (t.deathFrames > 0)
                {
                    t.deathFrames++;
                    if (t.deathFrames > Tank.spawnDelay)
                    {
                        t.deathFrames = 0;
                        t.health = Tank.StartingTankHP;
                        t.location = RandomSpawn();
                    }
                    else
                        continue;
                }
                foreach (Projectile p in projectiles.Values)
                {
                    if (t.ProjectileCollision(p.location))
                    {
                        if (t.ID == p.owner)
                            continue;
                        p.died = true;
                        t.health--;
                        if (t.health == 0)
                        {
                            t.died = true;
                            t.deathFrames++;
                            tanks[p.owner].score++;
                        }
                    }
                }

                foreach (Powerup p in powerups.Values)
                {
                    PowerupCollision(t, p);
                }

                foreach (Beam b in beams.Values)
                {
                    BeamCollision(t, b);
                }

                if (t.velocity.Length() == 0)
                    continue;

                Vector2D newLoc = t.location + t.velocity;
                bool collision = false;
                foreach (Wall wall in walls.Values)
                {
                    if (wall.TankCollision(newLoc))
                    {
                        collision = true;
                        t.velocity = new Vector2D(0, 0);
                        break;
                    }
                }
                if (!collision)
                    t.location = newLoc;

                IsWrapAround(t);
            }
        }

        /// <summary>
        /// Randomly sets a vector value using random integers within the boundaries of the world.
        /// If there is a wall where the random value is, the loop restarts and a new random location
        /// is created and checked again
        /// </summary>
        /// <returns></returns>
        public Vector2D RandomSpawn()
        {
            while (true)
            {
                bool spawnCollision = false;
                Random ran = new Random();
                int x = ran.Next(-size / 2, size / 2);
                int y = ran.Next(-size / 2, size / 2);
                Vector2D spawnLoc = new Vector2D(x, y);
                foreach (Wall wall in walls.Values)
                {
                    if (wall.TankCollision(spawnLoc))
                    {
                        spawnCollision = true;
                        break;
                    }
                }
                if (!spawnCollision)
                {
                    return spawnLoc;
                }
            }
        }
    }
}
