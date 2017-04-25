using System;
using System.Configuration;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

public class Mover : MonoBehaviour
{
    public float damping = 10000.0f;

    private Vector3 startPos;

    private Vector3 lerpDestPos;
    private bool lerping = false;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Handle 'A' button press - jerk back
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            transform.Translate(0, 0, 0.5f);
        }

        // Handle 'B' button press - return to original position
        if (OVRInput.GetDown(OVRInput.Button.Two))
        {
            transform.position = startPos;
        }

        // Handle trigger press
        var flex = OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger);
        if (flex > 0.5)
        {
            // Capture destination position
            if (!lerping)
            {
                lerpDestPos = transform.position + new Vector3(0, 0, 1.0f);
                lerping = true;
            }

            // Smooth movement
            transform.position = Vector3.Slerp(transform.position, lerpDestPos, damping);
        }
        else if (flex < 0.5)
        {
            lerping = false;
        }
    }
}