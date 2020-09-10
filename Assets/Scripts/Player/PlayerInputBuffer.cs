using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PlayerInput
{
    public class PlayerInputBuffer
    {
        #region Members

        public Frame [] Frames = null;

        public int Size
        {
            get
            {
                if ( Frames == null )
                {
                    return -1;
                }
                return Frames.Length;
            }
        }

        /// <summary>
        /// The total time delta for the whole input buffer.
        /// </summary>
        public float BufferDuration
        {
            get
            {
                float duration = 0f;
                foreach ( Frame frame in Frames )
                {
                    duration += frame.deltaTime;
                }
                return duration;
            }
        }

        #endregion

        #region Constructor

        public PlayerInputBuffer ()
        {
            Frames = new Frame [ 0 ];
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds <paramref name="newInput"/> to the buffer.
        /// </summary>
        /// <param name="newInput">The new player input.</param>
        public void Add ( Frame newInput )
        {
            if ( Frames == null )
            {
                Debug.LogError ( "Cannot add new input. The input buffer has not been initialized properly." );
                return;
            }
            Frame [] newFrames = new Frame [ Frames.Length + 1 ];

            // Copy existing elements
            for ( int i = 0; i < Frames.Length; i++ )
            {
                newFrames [ i ] = Frames [ i ];
            }
            // Assign new frame to end of array
            newFrames [ Frames.Length ] = newInput;
            Frames = newFrames;
        }

        /// <summary>
        /// Removes the element at index <paramref name="removeIndex"/> from the input buffer.
        /// </summary>
        /// <param name="removeIndex">Index of the element to remove.</param>
        public void RemoveAt ( int removeIndex )
        {
            if ( Frames == null || removeIndex < 0 || removeIndex >= Frames.Length )
            {
                return;
            }
            Frames = Frames.Where ( ( f, i ) => i != removeIndex ).ToArray ();
        }

        /// <summary>
        /// Clears the input buffer.
        /// </summary>
        public void Clear ()
        {
            Frames = new Frame [ 0 ];
        }

        #endregion
    }
}
