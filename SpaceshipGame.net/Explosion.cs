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
    public class Explosion : GameEntity
    {
        // Internals
        private Sprite[] _sprites = null;  // List of sprites we use when drawing the bullet
        private Sprite _curSprite = null;  // Current sprite to be drawn this frame
        private Int32 _frameCounter = 0;   // Frame counter used to animate sprites

        /// <summary>
        /// Constructor
        /// </summary>
        public Explosion()
        {
            // Load the image from the path passed in
            Image img = new Image("gfx/explode.bmp");

            // Setup mask
            img.CreateMaskFromColor(Color.Black);

            // The explosion consists of 10 sprites, so load them from that image
            _sprites = new Sprite[10];
            for (int i = 0; i < 10; i++)
            {
                // In this case, we know that each frame is 64x64.  
                _sprites[i] = new Sprite(new Texture(img), new IntRect(0 + (64 * i), 0, 63, 63));

                // We are going to scale each frame times 2.75
                // (Make it a little bigger than the ships)
                _sprites[i].Scale = new Vector2f(2.75f, 2.75f);

                // Set the origin point of the image for rotation.  Since we know the coords will be
                //  0 to 63 (64x64) then we know the center is at 31.5 x 31.5
                _sprites[i].Origin = new Vector2f(31.5f, 31.5f);
            }

            // Set the default value for the current sprite to use when we draw ourselves
            _curSprite = _sprites[0];
        }

        /// <summary>
        /// Update the explosion image
        /// </summary>
        /// <param name="deltaTime"></param>
        public override void Update(Int32 deltaTime)
        {
            if (IsAlive)
            {
                // Move through our sprites, which will be indexes 0-9
                // Display a different image every 2 frames
                _curSprite = _sprites[_frameCounter++ / 2];
                if (_frameCounter > 18)
                {
                    Kill();
                }
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
    }
}
