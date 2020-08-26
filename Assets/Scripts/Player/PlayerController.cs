using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private void FixedUpdate ()
    {
        SendInputToServer ();
    }

    private void SendInputToServer ()
    {
        bool [] _inputs = new bool []
        {
            Input.GetKey(KeyCode.W),            // [0] Forward
            Input.GetKey(KeyCode.S),            // [1] Backward
            Input.GetKey(KeyCode.A),            // [2] Left
            Input.GetKey(KeyCode.D),            // [3] Right
            Input.GetKey(KeyCode.LeftShift),    // [4] Run
            Input.GetKey(KeyCode.Space),        // [5] Jump
            Input.GetKey(KeyCode.C),            // [6] Crouch
            Input.GetKey(KeyCode.LeftControl)   // [7] Slide
        };

        ClientSend.PlayerMovement ( _inputs );
    }
}