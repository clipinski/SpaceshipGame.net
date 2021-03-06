﻿///////////////////////////////////////////////////////////////////////////////////
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
        #region Internals
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        // Backing Stores
        static private IConfiguration _settings = null;
        static private RenderWindow _window = null;
        static private uint _windowWidth = 0;
        static private uint _windowHeight = 0;
        static private Clock _clock = null;
        static private Sprite _starfield = null;
        static private Int32 _lastFrameTime = 0;
        static private Random _random = null;

        // Hold on to some references to the player ships
        static private PlayerShip _playerShip1 = null;
        static private PlayerShip _playerShip2 = null;

        // Main list of game entities
        static private List<GameEntity> _entities = new List<GameEntity>();

        // List of entities queued up to spawn in and the time they should be spawned in
        static private List<Tuple<GameEntity, Int32>> _entitiesWaitingToSpawn = new List<Tuple<GameEntity, Int32>>();

        // Items used for displaying each player's score
        static private int _player1Score = 0;
        static private int _player2Score = 0;
        static private Font _scoreFont = null;
        static private Text _player1ScoreTxt = null;
        static private Text _player2ScoreTxt = null;

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
        /// Returns the current elapsed time from the game clock as milliseconds
        /// </summary>
        static public Int32 Now
        {
            get
            {
                return GameClock.ElapsedTime.AsMilliseconds();
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
        /// Random number generator
        /// </summary>
        static public Random Random
        {
            get
            {
                return _random ?? (_random = new Random());
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
                Position = new Vector2f(100, WindowHeight / 2),
                ZOrder = 10
            };

            // Create player ship 1
            _playerShip2 = new PlayerShip("gfx/ShipFrames2.bmp")
            {
                // Initial position will be center height, 100px from the right
                Position = new Vector2f(WindowWidth - 100, WindowHeight / 2),
                Rotation = 180,
                ZOrder = 10
            };

            // Add them to our entities list when we can
            Spawn(_playerShip1, Now);
            Spawn(_playerShip2, Now);

            // Create event handlers for when the player ships are killed
            _playerShip1.OnKilled += OnShipKilled;
            _playerShip2.OnKilled += OnShipKilled;

            // Add some scoring handlers
            _playerShip1.OnKilled += (_, __) => _player2Score++;
            _playerShip2.OnKilled += (_, __) => _player1Score++;

            // Finally, generate our background "starfield"
            GenerateStarfield(200);

            // Setup objects to draw player score
            _scoreFont = new Font("fonts/Xcelsion.ttf");
            _player1ScoreTxt = new Text()
            {
                Font = _scoreFont,
                FillColor = Color.White,
                CharacterSize = 24,
                Position = new Vector2f(100f, 80f)
            };
            _player2ScoreTxt = new Text()
            {
                Font = _scoreFont,
                FillColor = Color.White,
                CharacterSize = 24,
                Position = new Vector2f(WindowWidth - 300f, 80f)
            };
        }

        /// <summary>
        /// Event Handler called when a player ship is "killed"
        /// </summary>
        /// <param name="sender">The player ship object reference</param>
        /// <param name="e">Empty</param>
        static void OnShipKilled(Object sender, EventArgs e)
        {
            // Get the player ship reference
            PlayerShip ship = (PlayerShip)sender;

            // Spawn an explosion at that location
            Spawn(new Explosion()
            {
                ZOrder = 20,
                Position = ship.Position,
                Rotation = ship.Rotation
            }, Now);

            // If our ship is dead, then it has been removed from the
            //   the game entities list.  However, we still have this reference
            //   which we can reuse to save the expense of re-creating
            //   the ship object
            // So we will move the ship to a random location and "respawn" it
            ship.Position = new Vector2f(Random.Next(0, (int)WindowWidth), Random.Next(0, (int)WindowHeight));
            ship.Rotation = Random.Next(0, 360);
            ship.VelocityVector = new Vector2f(0f, 0f);

            // Re-spawn the ship in 2 seconds
            Spawn(ship, Now + 2000);
        }

        /// <summary>
        /// Generate a background of stars and save it as a sprite
        /// </summary>
        /// <param name="numStars">Number of stars to randomly generate</param>
        static void GenerateStarfield(uint numStars)
        {
            RenderTexture rt = new RenderTexture(WindowWidth, WindowHeight);

            // Fill the texture with a black background
            rt.Clear(Color.Black);

            // Generate a random number of "stars", which will be simply randomly placed
            //  tiny rectangles of random colors
            for(uint i = 0; i < numStars; i++)
            {
                int starSize = Random.Next(1, 3);
                rt.Draw(new RectangleShape(new Vector2f(starSize, starSize))
                {
                    Position = new Vector2f(Random.Next(0, (int)WindowWidth), Random.Next(0, (int)WindowHeight)),

                    // Give some variance in color, but keep mostly "white"
                    FillColor = new Color( (byte)Random.Next(200, 255), (byte)Random.Next(200,255), (byte)Random.Next(200,255))
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
            if (_playerShip1.IsAlive)
            {
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
            }

            //////////////////////////
            // Player ship 2
            //////////////////////////
            if (_playerShip2.IsAlive)
            {
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
        }

        /// <summary>
        /// Spawn any entities in the "waiting to spawn" queue
        /// </summary>
        static void SpawnEntities()
        {
            // First, ensure we have any entities waiting to spawn before we do any work
            if (_entitiesWaitingToSpawn.Count > 0)
            {
                // Get all the entities from the "waiting to spawn" list whose time to
                //  spawn is now or has passed. (Item2 in the tuple is the time to spawn) 
                var readyToSpawnList = _entitiesWaitingToSpawn.FindAll(listItem => listItem.Item2 <= Now);

                // Now, ensure we have any entities ready to spawn before doing
                //   any work
                if (readyToSpawnList.Count > 0)
                {
                    // Loop through the list of items ready to spawn
                    readyToSpawnList.ForEach(readyItem =>
                    {
                        // Add the entity itself (item1 in the tuple) to the main list
                        //  of game entities - IE "spawn" it!
                        _entities.Add(readyItem.Item1);

                        // Mark that entity as "alive" cause we just spawned it
                        readyItem.Item1.IsAlive = true;
                    });

                    // Remove the items we just spawned from the waiting to spawn list
                    // (We remove all items in the waiting to spawn list that are
                    //   contained in the ready to spawn list)
                    _entitiesWaitingToSpawn.RemoveAll(waitingItem => readyToSpawnList.Contains(waitingItem));

                    // Finally, sort our main list by ZOrder
                    // NOTE: We only want to do this when the list has actually changed, hence it
                    //        is included in the "if" above, otherwise we would sort this list every
                    //        fame which would be costly and server no purpose.
                    _entities.Sort((a, b) => a.ZOrder.CompareTo(b.ZOrder));
                }
            }
        }

        /// <summary>
        /// This method allows us to queue up enties to be spawned into the game at a given time.
        /// This is especially important because we need to be careful about when we update the
        ///    main list of entities in the game.  For example, we don't want to change the
        ///    contents of that list while we are iterating over the list (like during the main
        ///    game update).
        /// </summary>
        /// <param name="e"></param>
        /// <param name="time"></param>
        static public void Spawn(GameEntity e, Int32 time)
        {
            // So add to a "pending" list for now, and we'll copy these to the main list later
            _entitiesWaitingToSpawn.Add(Tuple.Create(e, time));
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
                Int32 deltaTime = Now - _lastFrameTime;

                Window.DispatchEvents();

                // First, draw our starfield backgound.  All entities will be draw later,
                //  so as to appear "on top" of this.  This also efectively "clears" the
                //  screen for each frame.
                Window.Draw(_starfield);

                // Handle spawning any entities waiting
                SpawnEntities();

                // Handle input from the user
                HandleUserInput();

                // Loop through entities
                _entities.ForEach( (entity) =>
                {
                    // Update each one
                    entity.Update(deltaTime);

                    // Draw it
                    Window.Draw(entity);

                    // Check for collisions with all other entities
                    _entities.ForEach((entityToCheck) =>
                    {
                        // Make sure and not check an entity against itself
                        if (entityToCheck != entity)
                        {
                            if (entityToCheck.CollisionRect.Intersects(entity.CollisionRect))
                            {
                                entity.Kill();
                                entityToCheck.Kill();
                            }
                        }
                    });
                });

                // Remove all "dead" entities
                _entities.RemoveAll((entity) => entity.IsAlive == false);

                // Finally, draw the scores for each player
                _player1ScoreTxt.DisplayedString = $"Player 1: {_player1Score}";
                _player2ScoreTxt.DisplayedString = $"Player 2: {_player2Score}";
                Window.Draw(_player1ScoreTxt);
                Window.Draw(_player2ScoreTxt);

                // Display everything we have draw so far
                Window.Display();

                // Finally, update last frame time
                _lastFrameTime -= Now;
            }
        }
    }

    #endregion
}
