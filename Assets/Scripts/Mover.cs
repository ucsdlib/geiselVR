using System.Collections;
using UnityEngine;

public class Mover : MonoBehaviour
{
    public float Damping = 10000.0f;

    private Vector3 _startPos;

    private Vector3 _lerpDestPos;
    private bool _lerping = false;

    private void Start()
    {
        _startPos = transform.position;
    }

    private void Update()
    {
        // Handle 'A' button press - jerk back
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
//            transform.Translate(0, 0, 0.5f);
            if (!_lerping)
            {
                Vector3 end = transform.position + Vector3.forward;
                StartCoroutine(SmoothMove(transform.position, end, 1.0f));
            }
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
            if (!_lerping)
            {
                Debug.Log("COUROUTINE STARTED");
                StartCoroutine(SmoothMove(transform.position, transform.position, 2.0f));
            }
        }

        /*
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
        */
    }

    IEnumerator SmoothMove(Vector3 start, Vector3 end, float time)
    {
        if (!_lerping)
        {
            _lerping = true;
            float t = 0f;
            while (t < 1.0f)
            {
                t += Time.deltaTime / time;
                transform.position = Vector3.Slerp(start, end, t);
                Debug.Log("COUROUTINE STARTED: " + t);
                yield return null;
            }
            _lerping = false;
        }
    }
}