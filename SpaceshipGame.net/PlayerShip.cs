using System;
using System.Collections.Generic;
using System.Text;
using SFML.System;
using SFML.Graphics;

namespace SpaceshipGame.net
{
    public class PlayerShip : GameEntity, Drawable
    {
        // Internals
        private Sprite[] _shipSprites;              // List of sprites we use when drawing this ship
        private Sprite   _curSprite;                // Current sprite to be drawn this frame
        private Int32    _thrustFrameCounter;       // Counter of frames drawn while the engines are on, used to animate thrusters

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Properties
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Set to true when firing engines, this will cause the ship to apply thrust
        ///   in the direction it is pointing.
        /// </summary>
        public bool EnginesOn { get; set; } = false;

        /// <summary>
        /// Velocity vector for the ship
        /// </summary>
        public Vector2f Velocity { get; set; } = new Vector2f( 0f, 0f );

        /// <summary>
        /// Amount of thrust when applying engines
        /// </summary>
        public float Thrust { get; set; } = 0.05f;
        
        /// <summary>
        /// How fast the ship can turn
        /// </summary>
        public float TurnSpeed { get; set; } = 2.0f;

        #endregion

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Methods
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="imgFramesPath">The path to the graphics file that holds the frames of animation for the player ship.</param>
        public PlayerShip(string imgFramesPath)
        {
            // Load the image from the path passed in
            Image img = new Image(imgFramesPath);

            // Setup mask
            img.CreateMaskFromColor(Color.Black);

            // Each ship consists of 4 sprites, used to animate the engines, so load them from that image
            _shipSprites = new Sprite[4];
            for (int i = 0; i < 4; i++)
            {
                // In this case, we know that each frame is 64x64.  
                _shipSprites[i] = new Sprite(new Texture(img), new IntRect(0 + (64 * i), 0, 63, 63));

                // We are going to scale each frame times 2
                _shipSprites[i].Scale = new Vector2f(2.0f, 2.0f);

                // Set the origin point of the image for rotation.  Since we know the coords will be
                //  0 to 63 (64x64) then we know the center is at 31.5 x 31.5
                _shipSprites[i].Origin = new Vector2f(31.5f, 31.5f);
            }

            // Set the default value for the current sprite to use when we draw ourselves
            _curSprite = _shipSprites[0];
        }

        /// <summary>
        /// Based on input from the player, turn the ship left
        /// </summary>
        public void TurnLeft()
        {
            Rotation -= TurnSpeed;
        }

        /// <summary>
        /// Based on input from the player, turn the ship right
        /// </summary>
        public void TurnRight()
        {
            Rotation += TurnSpeed;
        }

        /// <summary>
        /// In this class, we perform updates differently than the base class, so we hide the base class
        ///    Update() and use this one instead.
        /// </summary>
        public override void Update(Int32 deltaTime)
        {
            if (EnginesOn)
            {
                // If our engines are on, apply forward thrusters in the direction the
                //   ship is pointing.
                Velocity += new Vector2f( (float)(Thrust * Math.Cos(RotationInRads)),
                                          (float)(Thrust * Math.Sin(RotationInRads))  );


                // Choose the correct image based on the frames we are counting
                if      (_thrustFrameCounter < 2)  _curSprite = _shipSprites[1];   // "Low" Thrust Image
                else if (_thrustFrameCounter < 4)  _curSprite = _shipSprites[2];   // "Medium" Thrust Image  
                else if (_thrustFrameCounter < 6)  _curSprite = _shipSprites[3];   // "High" Thrust Image
                else
                {
                    // Back to "Low" Thrust Image
                    _curSprite = _shipSprites[1];

                    // Reset frame counter
                    _thrustFrameCounter = 0;
                }

                // Add to the count of the frames that we have drawn while the engines are on
                _thrustFrameCounter++;
            }
            else
            {
                // Display the "engines off" sprite
                _curSprite = _shipSprites[0];

                // Reset thrust frame counter
                _thrustFrameCounter = 0;
            }

            // Update position based on velocity vector
            Position += Velocity;

            // Call base class
            base.Update(deltaTime);
        }

        /// <summary>
        /// From Drawable interface, implement the Draw method
        /// </summary>
        /// <param name="target"></param>
        /// <param name="states"></param>
        public override void Draw(RenderTarget target, RenderStates states)
        {
            // Setup the current sprite
            _curSprite.Position = Position;
            _curSprite.Rotation = Rotation;

            // And draw it!
            _curSprite.Draw(target, states);
        }

        #endregion
    }
}
