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
    public abstract class GameEntity : Drawable
    {
        protected IntRect _collisionRect  = new IntRect(0, 0, 0, 0);

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Properties
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public bool IsAlive { get; set; } = false;  // Entities won't be set to alive until they are added to the game
        public Vector2f Position { get; set; } = new Vector2f(0f, 0f);
        public float Rotation { get; set; } = 0f;

        /// <summary>
        /// SFML uses degrees, so we create this property as readonly
        /// </summary>
        public float RotationInRads
        {
            get
            {
                return (float) (Math.PI * Rotation / 180.0);
            }
        }

        /// <summary>
        /// Get the collision rect for this entity
        /// </summary>
        public IntRect CollisionRect
        {
            get
            {
                return _collisionRect;
            }
        }

        /// <summary>
        /// Event handler for when this entity is "killed"
        /// </summary>
        public event EventHandler OnKilled;

        #endregion

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Private Helpers
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Private helper method used to ensure that game entities will "wrap around" as the move through the main
        ///    game window
        /// </summary>
        protected void HandleWrapAround()
        {
            // Get current position
            Vector2f pos = Position;

            // See if it needs to be changed
            if (pos.X > Game.WindowWidth)     pos.X = 0;
            if (pos.X < 0)                    pos.X = Game.WindowWidth;
            if (pos.Y > Game.WindowHeight)    pos.Y = 0;
            if (pos.Y < 0)                    pos.Y = Game.WindowHeight;

            // Change the position if needed
            if ( Position != pos)
            {
                Position = pos;
            }
        }

        #endregion

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Methods
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Kill this entity
        /// </summary>
        public void Kill()
        {
            IsAlive = false;

            // Call event handler, if one has been set
            OnKilled?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Update the object based on time passed.  Derived classes should implement this method and update
        ///    themselves accourdingly.  Call the base class to handle "wrap around" at the end of your
        ///    Update() code.
        /// </summary>
        public virtual void Update(Int32 deltaTime)
        {
            // By Default, make all entities "wrap around" the screen
            HandleWrapAround();
        }

        /// <summary>
        /// From Drawable interface, implement the Draw method.  Derived classes should implement this method
        ///    and draw themselves accordingly.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="states"></param>
        public abstract void Draw(RenderTarget target, RenderStates states);

        #endregion
    }
}
