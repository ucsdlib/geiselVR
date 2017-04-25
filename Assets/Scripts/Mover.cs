using UnityEngine;
using Debug = System.Diagnostics.Debug;

public class Mover : MonoBehaviour
{

    private Vector3 startTransform;

	void Start () {
	    startTransform = gameObject.transform.position;
	}

	void Update () {
	    // Handle 'A' button press
	    if (OVRInput.GetDown(OVRInput.Button.One))
	    {
	        gameObject.transform.Translate(0, 0, 0.5f);
	    }

	    // Handle trigger press
	    var flex = OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger);
	    if (flex > 0.5)
	    {
	        gameObject.transform.position = startTransform;
	    }
	}
}
