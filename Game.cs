using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using TiledSharp;

namespace Dungeon {
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class DungeonGame : Game {
        public static DungeonGame current;
        public GraphicsDeviceManager GraphicsManager;
        public GraphicsDevice Graphics;
        public SpriteBatch SpriteBatch;
        public Map Map;
        public Rectangle Viewport;
        public Player Player;
        public SpriteFont Font;

        public static Random Random = new Random();

        // Just a simple millisecond counter, for now
        double timeElapsed = 0; 
        
        bool debug = true;
        string[] messages = new string[] { "one", "two", "three", "four", "five" };

        //new colour for background
        Color bg_colour = new Color(38, 38, 38);

        public DungeonGame() : base() {
            GraphicsManager = new GraphicsDeviceManager(this);
            GraphicsManager.PreferredBackBufferWidth = 940;
            GraphicsManager.PreferredBackBufferHeight = 720;
            GraphicsManager.IsFullScreen = false;
            IsMouseVisible = true;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize() {
            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent() {
            Content.RootDirectory = "Content";

            // Set up the graphics system here so the map can load textures
            Graphics = GraphicsManager.GraphicsDevice;

            Viewport = Graphics.Viewport.Bounds;
            Viewport.X = 0;
            Viewport.Y = 0;

            SpriteBatch = new SpriteBatch(Graphics);

            Map = new Map();
            Map.LoadTMX(new TmxMap("Content/Map/testmap0.tmx"));
            Player = (Player)Map.Cells[19, 11].Creatures[0];

            Font = Content.Load<SpriteFont>("KennyPixel");
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent() {
            // TODO: Unload any non ContentManager content here
            
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime) {
            timeElapsed += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (timeElapsed > 500) {
                //creature bobble animations
                foreach (var cre in Map.Creatures) {
                    if (cre != Player) cre.Bobble();
                }

                //fire and misc map animations


                timeElapsed = 0;
            }
            
            InputState input = InputState.GetState(gameTime);

            if (input.KeyPressed(Keys.Escape)) {
                Exit();
            }

            if (input.Mouse.LeftButton == ButtonState.Pressed) {
                try {
                    var cell = Map.Renderer.CellAt(input.Mouse.X, input.Mouse.Y);
                    var path = Map.PathBetween(Player.Cell, cell, (Cell c) => Player.CanPass(c));
                    if (path != null) {
                        Player.CurrentPath = path;
                    }
                } catch (OutOfBoundsException) {

                }
            }

            // Player movement code.
            // TODO (Mispy): A proper time system.
            var newx = Player.Cell.X;
            var newy = Player.Cell.Y;
            var newFacing = Player.Facing;
            if (Player.CurrentPath != null) {
                var cell = Player.CurrentPath[0];
                newx = cell.X;
                newy = cell.Y;

                if (newx < Player.Cell.X) {
                    newFacing = Direction.Left;
                } else if (newx > Player.Cell.X) {
                    newFacing = Direction.Right;
                }

                Player.CurrentPath.RemoveAt(0);
                if (Player.CurrentPath.Count == 0)
                    Player.CurrentPath = null;
            } else if (input.KeyPressed(Keys.Left)) {
                newx -= 1;
                newFacing = Direction.Left;
            } else if (input.KeyPressed(Keys.Right)) {
                newx += 1;
                newFacing = Direction.Right;
            } else if (input.KeyPressed(Keys.Up)) {
                newy -= 1;
            } else if (input.KeyPressed(Keys.Down)) {
                newy += 1;
            }
            else if (input.KeyPressed(Keys.W))
            {
                //player waits a turn
                // Other creatures take their turns
                foreach (var cre in Map.Creatures)
                {
                    if (cre != Player) cre.TakeTurn();
                }
            }

            if ((newx != Player.Cell.X || newy != Player.Cell.Y) && newx >= 0 && newx < Map.Width && newy >= 0 && newy < Map.Height) {
                var cell = Map.Cells[newx, newy];
                if (Player.CanPass(cell)) {
                    Player.Facing = newFacing;
                    Player.Move(cell);

                    // Other creatures take their turns
                    foreach (var cre in Map.Creatures) {
                        if (cre != Player) cre.TakeTurn();
                    }

                    if (debug) {
                        //Console.WriteLine(player.cell.x + ", " + player.cell.y);
                        //Console.WriteLine(Viewport.X + ", " + Viewport.Y);
                        //Console.WriteLine(Viewport.X + ", " + Viewport.Y);
                    }
                }
            }

            // These controls just scroll around the map instead, for testing
            /*var scrollAmount = 10;
            if (input.KeyPressed(Keys.Left)) {
                Viewport.X -= scrollAmount;
            } else if (input.KeyPressed(Keys.Right)) {
                Viewport.X += scrollAmount;
            } else if (input.KeyPressed(Keys.Up)) {
                Viewport.Y -= scrollAmount;
            } else if (input.KeyPressed(Keys.Down)) {
                Viewport.Y += scrollAmount;
            }*/

            base.Update(gameTime);
        }

        /// <summary>
        /// Centers the current Viewport around the player
        /// </summary>
        protected void CenterCamera() {
            Viewport.X = (Player.Cell.X * Map.TileWidth) - (Viewport.Width / 2);
            Viewport.Y = (Player.Cell.Y * Map.TileHeight) - (Viewport.Height / 2);
            //Console.WriteLine("{0} {1}", Viewport.X, Viewport.Y);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {
            CenterCamera();
            GraphicsDevice.Clear(bg_colour);
            UpdateMessages(messages);
            base.Draw(gameTime);
        }

        public void PrintMessage(string message)
        {

            messages[4] = messages[3];
            messages[3] = messages[2];
            messages[2] = messages[1];
            messages[1] = messages[0];
            messages[0] = message;

        }

        public void UpdateMessages(string[] messages)
        {
            SpriteBatch.Begin();

            Map.Renderer.Draw(SpriteBatch, Viewport);

            //line1
            var pos = new Vector2(50, 570);
            Vector2 origin = Font.MeasureString("test") / 2;
            SpriteBatch.DrawString(Font, messages[0], pos, Color.White, 0, origin, 1.0f, SpriteEffects.None, 0.5f);
            //line2
            pos = new Vector2(50, 550);
            SpriteBatch.DrawString(Font, messages[1], pos, Color.White, 0, origin, 1.0f, SpriteEffects.None, 0.5f);
            //line3
            pos = new Vector2(50, 530);
            SpriteBatch.DrawString(Font, messages[2], pos, Color.White, 0, origin, 1.0f, SpriteEffects.None, 0.5f);
            //line4
            pos = new Vector2(50, 510);
            SpriteBatch.DrawString(Font, messages[3], pos, Color.White, 0, origin, 1.0f, SpriteEffects.None, 0.5f);
            //line5
            pos = new Vector2(50, 490);
            SpriteBatch.DrawString(Font, messages[4], pos, Color.White, 0, origin, 1.0f, SpriteEffects.None, 0.5f);

            SpriteBatch.End();
        }

    }
}
