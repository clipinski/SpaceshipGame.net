///////////////////////////////////////////////////////////////////////////////////
// MIT License
//
// Copyright (c) 2019 Craig J. Lipinski
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
///////////////////////////////////////////////////////////////////////////////////

using System;
using Microsoft.Extensions.Configuration;
using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace SpaceshipGame.net
{
    class Game
    {
        #region Properties

        // Backing Stores
        static private IConfiguration _settings;
        
        /// <summary>
        /// Returns the game settings, read from the game config file
        /// </summary>
        static public IConfiguration Settings
        {
            get
            {
                return _settings ??
                      (_settings = new ConfigurationBuilder()
                                    .SetBasePath(System.AppContext.BaseDirectory)
                                    .AddJsonFile("gameconfig.json",
                                        optional: false,
                                        reloadOnChange: true).Build());
            }
        }

        /// <summary>
        /// Return the desired frame time in milliseconds.  For our game, our target framerate
        ///   is 60fps
        /// </summary>
        static public Int32 DesiredFrameTime
        {
            get
            {
                return ( (Int32) (1000f / 60f) );
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Main entry point for the game program
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // Create new render window
            var window = new RenderWindow(new VideoMode(uint.Parse(Settings["WindowWidth"]), uint.Parse(Settings["WindowHeight"])), "SpaceshipGame.net");
            // Add an event handler to handle when the user presses the "X" (close) button
            window.Closed += (sender, eventArgs) =>
            {
                window.Close();
            };

            // Regardless of game settings, we are going to design this for 60 fps
            window.SetFramerateLimit(60);

            // Allow vsync to be optional, based on settings
            window.SetVerticalSyncEnabled(bool.Parse(Settings["EnableVSync"]));


            Image img = new Image(String.Format("gfx/ShipFrames1.bmp"));
            img.CreateMaskFromColor(Color.Black);

            Sprite[] shipSprites = new Sprite[4];
            for(int i = 0; i < 4; i++)
            {
                shipSprites[i] = new Sprite(new Texture(img), new IntRect(0+(64*i), 0, 63, 63));
                shipSprites[i].Scale = new Vector2f(2.0f, 2.0f);
                shipSprites[i].Origin = new Vector2f(31.5f, 31.5f);
            }


            // Load a font to display some text
            Font font = new Font("fonts/Xcelsion.ttf");

            // Create a vector to move the shape
            Vector2f speed = new Vector2f(1f, 1f);

            // delta time position
            Vector2f fpsPos = new Vector2f(uint.Parse(Settings["WindowWidth"]) - 400, 10f);

            Clock gameClock = new Clock();
            float frameCounter = 0f;
            while (window.IsOpen)
            {
                // Delta time in ms per frame
                Int32 delta = gameClock.Restart().AsMilliseconds();

                foreach(Sprite ship in shipSprites)
                {
                    ship.Position += speed;
                    ship.Rotation += 1;
                }

                window.DispatchEvents();
                window.Clear(Color.Black);

                window.Draw(shipSprites[(int)frameCounter]);

                Text t = new Text(String.Format("Frametime={0}", delta), font, 20)
                {
                    FillColor = Color.White,
                    Position = fpsPos
                };
               
                window.Draw(t);

                window.Display();

                frameCounter += .5f;
                if (frameCounter > 3f)
                {
                    frameCounter = 0f;
                }
            }
        }
    }

    #endregion
}
