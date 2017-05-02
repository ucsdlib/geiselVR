using UnityEngine;

public class Mover : MonoBehaviour
{
    public float Damping = 10000.0f;

    private Vector3 _startPos;

    private Vector3 _lerpDestPos;
    private bool _lerping;

    private void Start()
    {
        _startPos = transform.position;
    }

    private void Update()
    {
        // Handle 'A' button press - jerk back
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            transform.Translate(0, 0, 0.5f);
        }

        // Handle 'B' button press - return to original position
        if (OVRInput.GetDown(OVRInput.Button.Two))
        {
            transform.position = _startPos;
        }

        // Handle trigger press
        var flex = OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger);
        if (flex > 0.5)
        {
            // Capture destination position
            if (!_lerping)
            {
                _lerpDestPos = transform.position + new Vector3(0, 0, 1.0f);
                _lerping = true;
            }

            // Smooth movement
            transform.position = Vector3.Slerp(transform.position, _lerpDestPos, Damping);
        }
        else if (flex < 0.5)
        {
            _lerping = false;
        }
    }
}