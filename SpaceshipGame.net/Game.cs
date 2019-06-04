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
        static private uint? _windowWidth = null;
        static private uint? _windowHeight = null;
        static private Clock _clock = null;
        static private List<GameEntity> _entities = null;

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
                return _windowWidth ?? (_windowWidth = uint.Parse(Settings["WindowWidth"])).Value;
            }
        }

        /// <summary>
        /// Game window height
        /// </summary>
        static public uint WindowHeight
        {
            get
            {
                return _windowHeight ?? (_windowHeight = uint.Parse(Settings["WindowHeight"])).Value;
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
                    // Create new render window
                    _window = new RenderWindow(new VideoMode(WindowWidth, WindowHeight), "SpaceshipGame.net");

                    // Add an event handler to handle when the user presses the "X" (close) button
                    _window.Closed += (sender, e) =>
                    {
                        _window.Close();
                    };

                    // Regardless of game settings, we are going to design this for 60 fps
                    _window.SetFramerateLimit(60);

                    // Allow vsync to be optional, based on settings
                    _window.SetVerticalSyncEnabled(bool.Parse(Settings["EnableVSync"]));
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
                Position = new Vector2f(50f, 400f)
            };

            // Create player ship 1
            _playerShip2 = new PlayerShip("gfx/ShipFrames2.bmp")
            {
                Position = new Vector2f(1050f, 400f),
                Rotation = 180
            };

            // Add them to our entities list
            Entities.Add(_playerShip1);
            Entities.Add(_playerShip2);
        }

        /// <summary>
        /// Handle input from the user and update the game objects accordingly
        /// </summary>
        static void HandleUserInput()
        {
            // Player ship 1
            if (Keyboard.IsKeyPressed(Keyboard.Key.A))
            {
                _playerShip1.TurnLeft();
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.S))
            {
                _playerShip1.TurnRight();
            }
            _playerShip1.EnginesOn = Keyboard.IsKeyPressed(Keyboard.Key.D);

            // Player ship 2
            if (Keyboard.IsKeyPressed(Keyboard.Key.Numpad4))
            {
                _playerShip2.TurnLeft();
            }
            if (Keyboard.IsKeyPressed(Keyboard.Key.Numpad5))
            {
                _playerShip2.TurnRight();
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
                Int32 deltaTime = GameClock.Restart().AsMilliseconds();

                Window.DispatchEvents();

                // Handle input from the user
                HandleUserInput();

                Window.Clear(Color.Black);

                // Loop through entities
                Entities.ForEach( (entity) =>
                {
                    // Update each one
                    entity.Update(deltaTime);

                    // Draw it
                    Window.Draw(entity);
                });

                Window.Display();
            }
        }
    }

    #endregion
}
