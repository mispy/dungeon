#region Using Statements
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Storage;
using TiledSharp;
#endregion

namespace Dungeon {
    // NOTE (Mispy): most of the comments in this file are Monogame defaults, not mine

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
        public Creature player;

        public DungeonGame()
            : base() {
            Content.RootDirectory = "Content";

            GraphicsManager = new GraphicsDeviceManager(this);
            GraphicsManager.PreferredBackBufferWidth = 800;
            GraphicsManager.PreferredBackBufferHeight = 600;
            GraphicsManager.IsFullScreen = false;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize() {
            base.Initialize();

            Viewport = Graphics.Viewport.Bounds;
            Viewport.X = 0;
            Viewport.Y = 0;
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent() {
            Graphics = GraphicsManager.GraphicsDevice;
            SpriteBatch = new SpriteBatch(Graphics);
            Map = new Map();
            Map.Initialize(new TmxMap("Content/Map/testmap0.tmx"));
          
            player = new Creature();
            player.cell = Map.Cells[4, 2];

            foreach (var tile in Map.TileTypes) {
                if (tile.Flags.Creature) {
                    player.tile = tile;
                    break;
                }
            }
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
            InputState input = InputState.GetState(gameTime);

            if (input.KeyPressed(Keys.Escape)) {
                Exit();
            }

            // Demo code for scrolling map
            var scrollAmount = 10;

            var newx = player.cell.x;
            var newy = player.cell.y;
            var facing = player.facing;
            if (input.KeyPressed(Keys.Left)) {
                newx -= 1;
                facing = Direction.Left;
            } else if (input.KeyPressed(Keys.Right)) {
                newx += 1;
                facing = Direction.Right;
            } else if (input.KeyPressed(Keys.Up)) {
                newy -= 1;
            } else if (input.KeyPressed(Keys.Down)) {
                newy += 1;
            }

            if ((newx != player.cell.x || newy != player.cell.y) && newx >= 0 && newx < Map.Width && newy >= 0 && newy < Map.Height) {
                var cell = Map.Cells[newx, newy];
                if (player.CanPass(cell)) {
                    player.facing = facing;
                    player.Move(cell);
                }
            }


/*            if (input.KeyPressed(Keys.Left)) {
                Viewport.X = Math.Max(0, Viewport.X - scrollAmount);
            } else if (input.KeyPressed(Keys.Right)) {
                Viewport.X += scrollAmount;
            } else if (input.KeyPressed(Keys.Up)) {
                Viewport.Y = Math.Max(0, Viewport.Y - scrollAmount);
            } else if (input.KeyPressed(Keys.Down)) {
                Viewport.Y += scrollAmount;
            }
 */
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            SpriteBatch.Begin();
            Map.Renderer.Draw(SpriteBatch, Viewport);
            SpriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
