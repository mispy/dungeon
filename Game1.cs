#region Using Statements
using System;
using System.Collections.Generic;
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
    public class Game1 : Game {
        public static Game1 current;
        public GraphicsDeviceManager GraphicsManager;
        public GraphicsDevice Graphics;
        public SpriteBatch SpriteBatch;
        public Map Map;
        public Rectangle Viewport;

        public Game1()
            : base() {
            Content.RootDirectory = "Content";

            GraphicsManager = new GraphicsDeviceManager(this);
            GraphicsManager.PreferredBackBufferWidth = 800;
            GraphicsManager.PreferredBackBufferHeight = 600;
            GraphicsManager.IsFullScreen = false;
            Game1.current = this;
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
            var keys = Keyboard.GetState();

            if (keys.IsKeyDown(Keys.Escape)) {
                Exit();
            }

            // Demo code for scrolling map
            var scrollAmount = 10;

            if (keys.IsKeyDown(Keys.Left)) {
                Viewport.X = Math.Max(0, Viewport.X - scrollAmount);
            } else if (keys.IsKeyDown(Keys.Right)) {
                Viewport.X += scrollAmount;
            } else if (keys.IsKeyDown(Keys.Up)) {
                Viewport.Y = Math.Max(0, Viewport.Y - scrollAmount);
            } else if (keys.IsKeyDown(Keys.Down)) {
                Viewport.Y += scrollAmount;
            }

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
