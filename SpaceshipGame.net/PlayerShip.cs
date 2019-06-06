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
using System.Text;
using SFML.System;
using SFML.Graphics;

namespace SpaceshipGame.net
{
    public class PlayerShip : GameEntity
    {
        // Internals
        private Sprite[] _sprites = null;          // List of sprites we use when drawing this ship
        private Sprite   _curSprite = null;        // Current sprite to be drawn this frame
        private Int32    _frameCounter = 0;        // Counter of frames drawn while the engines are on, used to animate thrusters
        private Int32    _lastBulletFiredTime = 0; // Used to keep a resonable fire rate when holding down the fire button

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
        public Vector2f VelocityVector { get; set; } = new Vector2f( 0f, 0f );

        /// <summary>
        /// Amount of thrust when applying engines
        /// </summary>
        public float Thrust { get; set; } = 0.05f;
        
        /// <summary>
        /// How fast the ship can turn
        /// </summary>
        public float TurnSpeed { get; set; } = 2.0f;

        /// <summary>
        /// Delay between "firing" bullets, in msecs
        /// </summary>
        public Int32 FireRate { get; set; } = 350;

        /// <summary>
        /// Bullet travel speed
        /// </summary>
        public float BulletSpeed { get; set; } = 7.0f;

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
            _sprites = new Sprite[4];
            for (int i = 0; i < 4; i++)
            {
                // In this case, we know that each frame is 64x64.  
                _sprites[i] = new Sprite(new Texture(img), new IntRect(0 + (64 * i), 0, 63, 63));

                // We are going to scale each frame times 2
                _sprites[i].Scale = new Vector2f(2.0f, 2.0f);

                // Set the origin point of the image for rotation.  Since we know the coords will be
                //  0 to 63 (64x64) then we know the center is at 31.5 x 31.5
                _sprites[i].Origin = new Vector2f(31.5f, 31.5f);
            }

            // Set the default value for the current sprite to use when we draw ourselves
            _curSprite = _sprites[0];

            // Initialize last bullet fired time
            _lastBulletFiredTime = Game.GameClock.ElapsedTime.AsMilliseconds();
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
        /// Shoot a bullet!
        /// </summary>
        public void Fire()
        {
            // Make sure enough time has passed so we can fire another bullet
            if (Game.GameClock.ElapsedTime.AsMilliseconds() - _lastBulletFiredTime >= FireRate)
            {
                // Create the bullet based on our position, rotation, and velocity
                Bullet b = new Bullet()
                {
                    Lifespan = 3000,
                    Position = this.Position,
                    Rotation = this.Rotation,
                    VelocityVector = new Vector2f( (float)(BulletSpeed * Math.Cos(RotationInRads)),
                                                   (float)(BulletSpeed * Math.Sin(RotationInRads)))
                };

                // Add in the ship's velocity
                b.VelocityVector += this.VelocityVector;

                // Tell the game to "spawn" this new bullet
                Game.Spawn(b);

                // Update last bullet fired time stamp
                _lastBulletFiredTime = Game.GameClock.ElapsedTime.AsMilliseconds();
            }
        }

        /// <summary>
        /// Update the ship position, rotation, etc.
        /// </summary>
        public override void Update(Int32 deltaTime)
        {
            if (EnginesOn)
            {
                // If our engines are on, apply forward thrusters in the direction the
                //   ship is pointing.
                VelocityVector += new Vector2f((float)(Thrust * Math.Cos(RotationInRads)),
                                               (float)(Thrust * Math.Sin(RotationInRads)));


                // Move through our "engines" on sprites, which will be indexes 1,2 & 3
                // So we will change every 2 frames
                int idx = (_frameCounter / 6) + 1;
                _curSprite = _sprites[idx];
                if (_frameCounter > 6) _frameCounter = 0;

                // Add to the count of the frames that we have drawn while the engines are on
                _frameCounter++;
            }
            else
            {
                // Display the "engines off" sprite
                _curSprite = _sprites[0];

                // Reset thrust frame counter
                _frameCounter = 0;
            }

            // Update position based on velocity vector
            Position += VelocityVector;

            // Update collision rect directly, we don't want to make a new object every frame
            //   because that would be inefecient/costly.

            // NOTE: Width of the sprite is 64 pixels and we are scaled x 2
            //   But we are going to pull that in a bit, to better represent
            //   the "body" of the ship.
            _collisionRect.Left = (int)(Position.X - 40f);
            _collisionRect.Top = (int)(Position.Y - 40);
            _collisionRect.Height = 80;
            _collisionRect.Width = 80;

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
