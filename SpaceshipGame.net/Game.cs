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
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace SpaceshipGame.net
{
    class Game
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Backing Stores
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        static private IConfiguration _settings = null;
        static private RenderWindow _window = null;
        static private uint _windowWidth = 0;
        static private uint _windowHeight = 0;
        static private Clock _clock = null;
        static private List<GameEntity> _entities = null;
        static private Sprite _starfield = null;
        static private Int32 _lastFrameTime = 0;

        // Hold on to some references to the player ships
        static private PlayerShip _playerShip1 = null;
        static private PlayerShip _playerShip2 = null;

        #endregion

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Properties
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////


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
        /// Game window width
        /// </summary>
        static public uint WindowWidth
        {
            get
            {
                // Try and get the window width from the config file.  It that fails,
                //   Just default to 1920
                if (_windowWidth == 0)
                {
                    if (!uint.TryParse(Settings["WindowWidth"], out _windowWidth))
                    {
                        _windowWidth = 1920;
                    }
                }

                return _windowWidth;
            }
        }

        /// <summary>
        /// Game window height
        /// </summary>
        static public uint WindowHeight
        {
            get
            {
                // Try and get the window height from the config file.  It that fails,
                //   Just default to 1080
                if (_windowHeight == 0)
                {
                    if (!uint.TryParse(Settings["WindowHeight"], out _windowHeight))
                    {
                        _windowHeight = 1080;
                    }
                }

                return _windowHeight;
            }
        }


        /// <summary>
        /// Main window for the game
        /// </summary>
        static public RenderWindow Window
        {
            get
            {
                // See if we need to create and setup the main window
                if (_window == null)
                {
                    // Window style
                    Styles windowStyle = Styles.Default;

                    // Check for full screen setting in the config file, allow for the possibility
                    //  that the setting might not exist or might not be set properly.
                    if (bool.TryParse(Settings["FullScreen"], out bool fullScreen))
                    {
                        if (fullScreen)
                        {
                            windowStyle = Styles.Fullscreen;
                        }
                    }

                    // Create new render window
                    _window = new RenderWindow(new VideoMode(WindowWidth, WindowHeight), "SpaceshipGame.net", windowStyle);

                    // Add an event handler to handle when the user presses the "X" (close) button
                    _window.Closed += (sender, e) =>
                    {
                        _window.Close();
                    };

                    // Regardless of game settings, we are going to design this for 60 fps
                    _window.SetFramerateLimit(60);

                    // Allow vsync to be optional, based on settings
                    if (bool.TryParse(Settings["EnableVSync"], out bool vsyncEnabled))
                    {
                        if (vsyncEnabled)
                        {
                            _window.SetVerticalSyncEnabled(bool.Parse(Settings["EnableVSync"]));
                        }
                    }  
                }

                return _window;
            }
        }

        /// <summary>
        /// Game clock
        /// </summary>
        static public Clock GameClock
        {
            get
            {
                return _clock ?? (_clock = new Clock());
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

        /// <summary>
        /// List of game entities
        /// </summary>
        static public List<GameEntity> Entities
        {
            get
            {
                return _entities ?? (_entities = new List<GameEntity>());
            }
        }

        #endregion

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Methods
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////


        /// <summary>
        /// Initialize the game
        /// </summary>
        static void Initialize()
        {
            // Create player ship 1
            _playerShip1 = new PlayerShip("gfx/ShipFrames1.bmp")
            {
                // Initial position will be center height, 100px from the left
                Position = new Vector2f(100, WindowHeight / 2)
            };

            // Create player ship 1
            _playerShip2 = new PlayerShip("gfx/ShipFrames2.bmp")
            {
                // Initial position will be center height, 100px from the right
                Position = new Vector2f(WindowWidth - 100, WindowHeight / 2),
                Rotation = 180
            };

            // Add them to our entities list
            Entities.Add(_playerShip1);
            Entities.Add(_playerShip2);

            // Finally, generate our background "starfield"
            GenerateStarfield(200);
        }

        /// <summary>
        /// Generate a background of stars and save it as a sprite
        /// </summary>
        /// <param name="numStars">Number of stars to randomly generate</param>
        static void GenerateStarfield(uint numStars)
        {
            Random random = new Random();
            RenderTexture rt = new RenderTexture(WindowWidth, WindowHeight);

            // Fill the texture with a black background
            rt.Clear(Color.Black);

            // Generate a random number of "stars", which will be simply randomly placed
            //  tiny rectangles of random colors
            for(uint i = 0; i < numStars; i++)
            {
                int starSize = random.Next(1, 3);
                rt.Draw(new RectangleShape(new Vector2f(starSize, starSize))
                {
                    Position = new Vector2f(random.Next(0, (int)WindowWidth), random.Next(0, (int)WindowHeight)),

                    // Give some variance in color, but keep mostly "white"
                    FillColor = new Color( (byte)random.Next(200, 255), (byte)random.Next(200,255), (byte)random.Next(200,255))
                });
            }

            // Create the sprite we will use to draw the starfield because it would be very inneficient
            //  to draw the stars individually each frame.
            _starfield = new Sprite(rt.Texture);
        }

        /// <summary>
        /// Handle input from the user and update the game objects accordingly
        /// </summary>
        static void HandleUserInput()
        {
            // Close the game if the escape key is pressed
            if (Keyboard.IsKeyPressed(Keyboard.Key.Escape))
            {
                Window.Close();
            }

            //////////////////////////
            // Player ship 1
            //////////////////////////
            if (Keyboard.IsKeyPressed(Keyboard.Key.A))
            {
                _playerShip1.TurnLeft();
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.S))
            {
                _playerShip1.TurnRight();
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.F))
            {
                _playerShip1.Fire();
            }
            _playerShip1.EnginesOn = Keyboard.IsKeyPressed(Keyboard.Key.D);

            //////////////////////////
            // Player ship 2
            //////////////////////////
            if (Keyboard.IsKeyPressed(Keyboard.Key.Numpad4))
            {
                _playerShip2.TurnLeft();
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.Numpad5))
            {
                _playerShip2.TurnRight();
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.Add))
            {
                _playerShip2.Fire();
            }
            _playerShip2.EnginesOn = Keyboard.IsKeyPressed(Keyboard.Key.Numpad6);

        }


        /// <summary>
        /// Main entry point for the game program
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // Initialize the game
            Initialize();

            // Main game loop
            while (Window.IsOpen)
            {
                // Delta time in ms per frame
                Int32 deltaTime = GameClock.ElapsedTime.AsMilliseconds() - _lastFrameTime;

                Window.DispatchEvents();

                // Handle input from the user
                HandleUserInput();

                // First, draw our starfield backgound.  All entities will be draw later,
                //  so as to appear "on top" of this.  This also efectively "clears" the
                //  screen for each frame.
                Window.Draw(_starfield);

                // Loop through entities
                Entities.ForEach( (entity) =>
                {
                    // Update each one
                    entity.Update(deltaTime);

                    // Draw it
                    Window.Draw(entity);
                });

                // Remove all "dead" entities
                Entities.RemoveAll((entity) => entity.IsAlive == false);

                // Display everything we have draw so far
                Window.Display();

                // Finally, update last frame time
                _lastFrameTime -= GameClock.ElapsedTime.AsMilliseconds();
            }
        }
    }

    #endregion
}
