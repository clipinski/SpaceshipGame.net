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
    public class Bullet : GameEntity 
    {
        // Internals
        private Sprite[] _sprites = null;  // List of sprites we use when drawing the bullet
        private Sprite _curSprite = null;  // Current sprite to be drawn this frame
        private Int32 _frameCounter = 0;   // Frame counter used to animate sprites
        private Int32 _creationTime = 0;   // Keep track of when we got created so we can track our lifespan

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Properties
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Velocity vector for the ship
        /// </summary>
        public Vector2f VelocityVector { get; set; } = new Vector2f(0f, 0f);

        /// <summary>
        /// Lifespan (in msecs)
        /// </summary>
        public Int32 Lifespan { get; set; } = 0;

        /// <summary>
        /// Returns the current amount of time (in msecs) that the bullet has been "alive"
        /// </summary>
        public Int32 Lifetime
        {
            get
            {
                return Game.GameClock.ElapsedTime.AsMilliseconds() - _creationTime;
            }
        }

        #endregion

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Methods
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Constructor
        /// </summary>
        public Bullet()
        {
            // Load the image from the path passed in
            Image img = new Image("gfx/bullet.bmp");

            // Setup mask
            img.CreateMaskFromColor(Color.Black);

            // Each ship consists of 4 sprites, used to animate the engines, so load them from that image
            _sprites = new Sprite[4];
            for (int i = 0; i < 4; i++)
            {
                // In this case, we know that each frame is 7x7  
                _sprites[i] = new Sprite(new Texture(img), new IntRect(0 + (7 * i), 0, 6, 6));

                // We are going to scale each frame times 2
                _sprites[i].Scale = new Vector2f(2.0f, 2.0f);

                // Set the origin point of the image for rotation.  Since we know the coords will be
                //  0 to 6 (7x7) then we know the center is at 3.5, 3.5
                _sprites[i].Origin = new Vector2f(3.5f, 3.5f);
            }

            // Set the default value for the current sprite to use when we draw ourselves
            _curSprite = _sprites[0];

            // Store creation time for this bullet
            _creationTime = Game.GameClock.ElapsedTime.AsMilliseconds();
        }

        /// <summary>
        /// Update bullet position and image
        /// </summary>
        public override void Update(Int32 deltaTime)
        {
            // Check our lifespan
            if ( Lifetime > Lifespan)
            {
                // Past our lifespan, so mark oursleves as "dead"
                Kill();
            }
            else
            {
                // Update position based on velocity vector
                Position += VelocityVector;

                // Move through our sprites, which will be indexes 0-3
                // Display a different image every 5 frames
                _curSprite = _sprites[_frameCounter++ / 5];
                if (_frameCounter > 15) _frameCounter = 0;
            }

            // Update collision rect directly, we don't want to make a new object every frame
            //   because that would be inefecient/costly.

            // NOTE: Allow a few frames of invulernability so the ship that fires the bullet
            //        won't blow itself up.
            if (Lifetime > 1600)
            {
                // NOTE: Width of the sprite is 7 pixels and we are scaled x 2
                _collisionRect.Left = (int)(Position.X - 7f);
                _collisionRect.Top = (int)(Position.Y - 7f);
                _collisionRect.Height = 14;
                _collisionRect.Width = 14;
            }

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
