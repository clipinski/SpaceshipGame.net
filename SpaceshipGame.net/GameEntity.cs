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
    public class GameEntity : Drawable
    {
        #region Properties

        public bool IsAlive { get; set; } = true;
        public Vector2f Position { get; set; } = new Vector2f(0f, 0f);
        public float Rotation { get; set; } = 0f;
        public float Velocity { get; set; } = 0f;

        #endregion

        #region Methods

        /// <summary>
        /// Kill this entity
        /// </summary>
        public void Kill()
        {
            IsAlive = false;
        }

        /// <summary>
        /// Update the object based on time passed.  By default, we will move the object based on it's
        ///    rotation and velocity
        /// </summary>
        public void Update(Int32 delta)
        {

        }

        /// <summary>
        /// Impement the "Drawable" interface from SFML.Graphics
        /// </summary>
        /// <param name="target"></param>
        /// <param name="states"></param>
        public void Draw(RenderTarget target, RenderStates states)
        {

        }

        #endregion
    }
}
