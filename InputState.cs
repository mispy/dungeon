using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace Dungeon {
    /// <summary>
    /// Wraps keyboard/mouse input with temporal metadata
    /// </summary>
    public class InputState {
        public static InputState LastInput;
        public static double LastKeyChange = 0; // Milliseconds since last keystate change    
        public static bool Repeating = false;

        public KeyboardState Keys;
        public MouseState Mouse;

        public static InputState GetState(GameTime gameTime) {
            var input = new InputState();

            var keys = Microsoft.Xna.Framework.Input.Keyboard.GetState();
            var mouse = Microsoft.Xna.Framework.Input.Mouse.GetState();
            input.Initialize(gameTime, keys, mouse);

            LastInput = input;
            return input;
        }

        public void Initialize(GameTime gameTime, KeyboardState keys, MouseState mouse) {
            Keys = keys;
            Mouse = mouse;

            if (LastInput == null || LastInput.Keys != Keys) {
                LastKeyChange = 0;
                Repeating = false;
            }
            else {
                LastKeyChange += gameTime.ElapsedGameTime.TotalMilliseconds;
                //Console.WriteLine("{0}", LastKeyChange);
                if (LastKeyChange > 500) Repeating = true;
            }
        }

        public bool KeyPressed(Keys key) {
            return (Keys.IsKeyDown(key) && (LastKeyChange == 0 || Repeating));
        }
    }
}
