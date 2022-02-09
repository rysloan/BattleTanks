using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace TankWars
{
    class DrawingPanel : Panel
    {
        public World theWorld;

        public int playerID;

        private Dictionary<string, Image> Images = new Dictionary<string, Image>();

        private Dictionary<int, DeathAnimtion> deaths = new Dictionary<int, DeathAnimtion>();

        /// <summary>
        /// Sets the world up in the drawing panel and doubleBuffers the control to prevent flickering
        /// issues
        /// </summary>
        /// <param name="world">World model with all the differnt components of the game</param>
        public DrawingPanel(World world)
        {
            DoubleBuffered = true;
            theWorld = world;
        }

        //delegate for DrawObjectWithTransform, methods matching this delegate can draw whatever by using 'e'
        public delegate void ObjectDrawer(object o, PaintEventArgs e);

        /// <summary>
        /// This method performs a translation and rotation to drawn an object in the world.
        /// </summary>
        /// <param name="e">PaintEventArgs to access the graphics (for drawing)</param>
        /// <param name="o">The object to draw</param>
        /// <param name="worldX">The X coordinate of the object in world space</param>
        /// <param name="worldY">The Y coordinate of the object in world space</param>
        /// <param name="angle">The orientation of the objec, measured in degrees clockwise from "up"</param>
        /// <param name="drawer">The drawer delegate. After the transformation is applied, the delegate is invoked to draw whatever it wants</param>
        private void DrawObjectWithTransform(PaintEventArgs e, object o, double worldX, double worldY, float angle, ObjectDrawer drawer)
        {
            // "push" the current transform
            System.Drawing.Drawing2D.Matrix oldMatrix = e.Graphics.Transform.Clone();

            e.Graphics.TranslateTransform((int)worldX, (int)worldY);
            e.Graphics.RotateTransform((float)angle);
            drawer(o, e);

            // "pop" the transform
            e.Graphics.Transform = oldMatrix;
        }

        /// <summary>
        /// Draws everything to be displayed on the form and images to be updated to create "movement"
        /// as well as centering the view on the player
        /// </summary>
        /// <param name="e">Paintevent that is used to paint all the different components</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            //Keep in mind that Coordinates (0,0) is the top left pixel of an image in a form
            //Center of an image is (size/2, size/2)
            if (theWorld == null)
            {
                return;
            }
            lock (theWorld)
            {
                int viewSize = Size.Width; //900x900 pixels standard view size

                if (theWorld.tanks.ContainsKey(playerID))
                {
                    double playerX = theWorld.tanks[playerID].location.GetX(); //Player's world-space in X-cordinates GET PLAYER ID FOR CLIENT FROM THE SERVER!!! YOU NEED IT!
                    double playerY = theWorld.tanks[playerID].location.GetY(); //Player's world-space in Y-cordinates
                    //Centers the actual view we say at the player's location
                    e.Graphics.TranslateTransform((float)(-playerX + (viewSize / 2)), (float)(-playerY + (viewSize / 2)));
                }
                else
                {
                    return;
                }
                BackgroundDrawer(theWorld, e);
            }

            lock (theWorld)
            {
                //Draws the tank, turrent, and playerUI
                foreach (Tank t in theWorld.tanks.Values)
                {
                    if (t.health != 0)
                    {
                        DrawObjectWithTransform(e, t, t.location.GetX(), t.location.GetY(), t.orientation.ToAngle(), TankDrawer);
                        DrawObjectWithTransform(e, t, t.location.GetX(), t.location.GetY(), t.direction.ToAngle(), TurretDrawer);
                        DrawObjectWithTransform(e, t, 0, 0, 0, PlayerUIDrawer);
                    }

                    if (t.died)
                    {
                        deaths[t.ID] = new DeathAnimtion(t);
                    }
                }

                //Draws the walls to be placed in the world
                foreach (Wall w in theWorld.walls.Values)
                {
                    DrawObjectWithTransform(e, w, 0, 0, 0, WallDrawer);
                }

                //Draws the projectile shot by the tank
                foreach (Projectile p in theWorld.projectiles.Values)
                {
                    DrawObjectWithTransform(e, p, p.location.GetX(), p.location.GetY(), p.direction.ToAngle(), ProjectileDrawer);
                }

                //Draws the powerup that is placed randomly in the world
                foreach (Powerup pow in theWorld.powerups.Values)
                {
                    DrawObjectWithTransform(e, pow, pow.location.GetX(), pow.location.GetY(), 0, PowerupsDrawer);
                }

                //Draws the beam animation that is shot by the player
                foreach (Beam b in theWorld.beams.Values)
                { 
                    if (b.frames < 30)
                        DrawObjectWithTransform(e, b, b.origin.GetX(), b.origin.GetY(), b.direction.ToAngle(), BeamDrawer);
                }
                List<DeathAnimtion> de = new List<DeathAnimtion>(deaths.Values);
                foreach (DeathAnimtion d in de)
                {
                    if (d.frames < 180)
                    {
                        DrawObjectWithTransform(e, d, 0, 0, 0, DeathParticleDrawer);
                    }
                    else
                        deaths.Remove(d.owner.ID);
                }
            }
            //Do anything that Panel (from which we inherit) needs to do
            base.OnPaint(e);
        }

        //Drawers for drawing the different components
        //--------------------------------------------

        /// <summary>
        /// Draws the tank with the first 8 tanks being a unique color
        /// </summary>
        /// <param name="o">tank object</param>
        /// <param name="e">painter</param>
        private void TankDrawer(object o, PaintEventArgs e)
        {
            Tank t = o as Tank;

            int tankSize = 60;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            string[] color = { "Blue", "Dark", "Green", "LightGreen", "Orange", "Purple", "Red", "Yellow" };
            Image tankImage = ImageHandler(color[t.ID % 8] + "Tank.png"); 
            Graphics g = e.Graphics;
            g.DrawImage(tankImage, -(tankSize / 2), -(tankSize / 2), tankSize, tankSize);
        }

        /// <summary>
        /// Draws the turret on the tank and pointing in whatever direction the mouse is
        /// Color matches the tanks color
        /// </summary>
        /// <param name="o">tank object</param>
        /// <param name="e">painter</param>
        private void TurretDrawer(object o, PaintEventArgs e)
        {
            Tank t = o as Tank;

            int turretSize = 50;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            string[] color = { "Blue", "Dark", "Green", "LightGreen", "Orange", "Purple", "Red", "Yellow" };
            Image turretImage = ImageHandler(color[t.ID % 8] + "Turret.png");
            Graphics g = e.Graphics;
            g.DrawImage(turretImage, -(turretSize / 2), -(turretSize / 2), turretSize, turretSize);

        }

        /// <summary>
        /// Draws the background of the world so that the view and other objects can be drawn or viwed
        /// </summary>
        /// <param name="o">world object</param>
        /// <param name="e">painter</param>
        private void BackgroundDrawer(object o, PaintEventArgs e)
        {
            World w = o as World;
            int worldSize = w.size; //Gets the size of the world that's been given by the server
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            Graphics g = e.Graphics;
            Image worldImage = ImageHandler("Background.png");
            g.DrawImage(worldImage, (-worldSize / 2), (-worldSize / 2), worldSize, worldSize);
        }

        /// <summary>
        /// Draws the walls sent by the server by taking the endpoints, using the distance formula,
        /// dividing by wall size, and drawing from greatest value endpoint to smallest
        /// </summary>
        /// <param name="o">wall object</param>
        /// <param name="e">painter</param>
        private void WallDrawer(object o, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            Wall wall = o as Wall;
            
            Image wallImage = ImageHandler("WallSprite.png");
            Graphics g = e.Graphics;

            int wallSize = 50;
            double spriteNum = 0;

            //Remember the distance formula? d = sqrt((x2-x1)^2 + (y2-y1)^2)
            double xCord = Math.Pow((wall.endpoint2.GetX() - wall.endpoint1.GetX()), 2.0);
            double yCord = Math.Pow((wall.endpoint2.GetY() - wall.endpoint1.GetY()), 2.0);
            spriteNum = (Math.Sqrt(xCord + yCord)) / 50;

            int maxX = (int)Math.Max(wall.endpoint1.GetX(), wall.endpoint2.GetX());
            int maxY = (int)Math.Max(wall.endpoint1.GetY(), wall.endpoint2.GetY());

            if (wall.endpoint1.GetY() == wall.endpoint2.GetY())
            {
                for (int i = 0; i <= spriteNum; i++)
                {
                    g.DrawImage(wallImage, ((float)maxX - (wallSize / 2)) - (i * 50), (float)maxY - (wallSize / 2), wallSize, wallSize);
                }
            }
            else
            {
                for (int i = 0; i <= spriteNum; i++)
                {
                    g.DrawImage(wallImage, ((float)maxX - (wallSize / 2)), ((float)maxY - (wallSize / 2)) - (i * 50), wallSize, wallSize);
                }
            }
        }

        /// <summary>
        /// Draws the power ups as circles in the random locations sent by the server
        /// </summary>
        /// <param name="o">powerup object</param>
        /// <param name="e">painter</param>
        private void PowerupsDrawer(object o, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Powerup p = o as Powerup;

            if (p.died == true) //If power up does not exist it's removed
                return;

            int width = 12;
            int height = 12;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            using (SolidBrush redBrush = new SolidBrush(Color.Red))
            {
                // Circles are drawn starting from the top-left corner.
                // So if we want the circle centered on the powerup's location, we have to offset it
                // by half its size to the left (-width/2) and up (-height/2)
                Rectangle r = new Rectangle(-(width / 2), -(height / 2), width, height);

                e.Graphics.FillEllipse(redBrush, r);
            }
        }

        /// <summary>
        /// Draws a beam that is wide but then shrinks down to nothing
        /// </summary>
        /// <param name="o">beam object</param>
        /// <param name="e">painter</param>
        private void BeamDrawer(object o, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Beam b = o as Beam;
            string[] color = { "LightBlue", "Black", "Green", "LightGreen", "Orange", "Purple", "Red", "Yellow" };
            Color beamColor = new Color();
            beamColor = Color.FromName(color[b.owner % color.Length]);

            using (Pen beamPen = new Pen(beamColor, 30f - b.frames))
            {
                //Draws beam from turret end to world edge
                e.Graphics.DrawLine(beamPen, new Point(0, -30), new Point(0, -theWorld.size));
                b.frames++;
            }
        }

        /// <summary>
        /// Draws the projectile shot by the tank based on the angle of the turret
        /// </summary>
        /// <param name="o">projectile obejct</param>
        /// <param name="e">painter</param>
        private void ProjectileDrawer(object o, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            Projectile p = o as Projectile;

            int projSize = 30;

            if (p.died == true) //If the projectile no longer exists then it is no longer drawn
                return;

            string[] color = { "blue", "grey", "green", "white", "brown", "violet", "red" , "yellow" };
            Image projectileImage = ImageHandler("shot-" + color[p.owner % color.Length] + ".png");
            Graphics g = e.Graphics;
            g.DrawImage(projectileImage, (-projSize / 2), (-projSize / 2), projSize, projSize); //projectiles should be 60x60 pixels for standard size
        }

        /// <summary>
        /// Draws the player UI such as name, healthbar, and socre and updates it accordingly as well
        /// as assigning it colors based on the tank color
        /// </summary>
        /// <param name="o">tank object</param>
        /// <param name="e">painter</param>
        public void PlayerUIDrawer(object o, PaintEventArgs e)
        {
            // Tank Information
            string[] color = { "Blue", "Black", "Green", "LightGreen", "Orange", "Purple", "Red", "Yellow" };
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Graphics g = e.Graphics;
            Tank t = o as Tank;

            // Name Information for the UI
            string tankColor = color[t.ID % color.Length];
            Color nameColor = new Color();
            nameColor = Color.FromName(tankColor);
            string tankName = t.name;
            Font f = new Font("Arial", 10f, FontStyle.Regular);
            Point namePos = new Point((int)t.location.GetX() - 30, (int)t.location.GetY() + 35);


            // Health Information for the UI
            int tankHp = t.health;
            Color hpColor = new Color();
            Point hpPos = new Point((int)t.location.GetX() - 25, (int)t.location.GetY() - 45);
            Size hpSize = new Size((15 * tankHp), 5);

            // Sets the color of the hp bar depending on the players HP value
            switch (tankHp)
            {
                case (3):
                    hpColor = Color.Green;
                    break;
                case (2):
                    hpColor = Color.Yellow;
                    break;
                case (1):
                    hpColor = Color.Red;
                    break;
                default:
                    hpColor = Color.Green;
                    break;
            }

            // Draws the players name
            using (SolidBrush nameBrush = new SolidBrush(nameColor))
            {
                g.DrawString(tankName + ": " + t.score, f, nameBrush, namePos);
            }

            // Draws the HP bar
            using (SolidBrush hpBrush = new SolidBrush(hpColor))
            {
                Rectangle r = new Rectangle(hpPos, hpSize);
                g.FillRectangle(hpBrush, r);
            }
        }

        private void DeathParticleDrawer(object o, PaintEventArgs e)
        {
            DeathAnimtion d = o as DeathAnimtion;
            Graphics g = e.Graphics;
            string[] color = { "Blue", "Dark", "Green", "LightGreen", "Orange", "Purple", "Red", "Yellow" };
            Image tankImage = ImageHandler(color[d.owner.ID % 8] + "Tank.png");

            if (d.frames < 60)
            {
                g.DrawImage(tankImage, (float)d.owner.location.GetX() - 30, (float)d.owner.location.GetY() - 30, 60 - d.frames, 60 - d.frames);
                d.frames++;
            }
        }

        /// <summary>
        /// Stores image file paths in an array to be accessed quickly when needed by the drawer
        /// methods so there isn't slowdown due to constantly searching through filepahts
        /// </summary>
        /// <param name="image">name of the image file</param>
        /// <returns>returns the image from the filepath</returns>
        public Image ImageHandler (string image)
        {
            if (!Images.ContainsKey(image))
                Images[image] = Image.FromFile("..\\..\\..\\Resources\\Images\\" + image);
            return Images[image];
        }
    }

    public class DeathAnimtion
    {
        public Tank owner { get; set; }
        public int frames { get; set; }
        public DeathAnimtion(Tank ownerID)
        {
            owner = ownerID;
            frames = 0;
        }
    }
}
