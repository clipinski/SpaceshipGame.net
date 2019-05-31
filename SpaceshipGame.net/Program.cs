using System;
using Microsoft.Extensions.Configuration;
using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace SpaceshipGame.net
{
    class Program
    {
        /// <summary>
        /// Loads the game config from the json file
        /// </summary>
        /// <returns>IConfiguration interface to retrieve configuration settings</returns>
        static IConfiguration GetConfig()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(System.AppContext.BaseDirectory)
                .AddJsonFile("gameconfig.json",
                optional: false,
                reloadOnChange: true);

            return builder.Build();
        }

        /// <summary>
        /// Main entry point for the game program
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            var settings = GetConfig();

            // Create new render window
            var window = new RenderWindow(new VideoMode(uint.Parse(settings["WindowWidth"]), uint.Parse(settings["WindowHeight"])), "SpaceshipGame.net");
            window.Closed += (sender, eventArgs) =>
            {
                window.Close();
            };

            // Regardless of game settings, we are going to design this for 60 fps
            window.SetFramerateLimit(60);

            // Allow vsync to be optional, based on settings
            window.SetVerticalSyncEnabled(bool.Parse(settings["EnableVSync"]));


            Sprite[] shipSprites = new Sprite[4];
            for(int i = 0; i < 4; i++)
            {
                Image img = new Image(String.Format("gfx/00ShipFrames{0}.bmp", i+1));
                img.CreateMaskFromColor(Color.Black);
                shipSprites[i] = new Sprite(new Texture(img), new IntRect(0, 0, 63, 63));
                shipSprites[i].Scale = new Vector2f(2.0f, 2.0f);
                shipSprites[i].Origin = new Vector2f(31.5f, 31.5f);
            }

         
            // Create a vector to move the shape
            Vector2f speed = new Vector2f(1f, 1f);

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

                window.Display();

                frameCounter += .5f;
                if (frameCounter > 3f)
                {
                    frameCounter = 0f;
                }
            }
        }
    }
}
