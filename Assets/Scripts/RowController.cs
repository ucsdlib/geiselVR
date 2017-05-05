using System.Net.Sockets;
using UnityEngine;

public class RowController : MonoBehaviour
{
	public Transform Bookshelf;


	private void Start () {
		CalculateLocalBounds();
	}

	private void Update()
	{
		InstantiateOnPress();
	}

	private void InstantiateOnPress()
	{
		// Check use pressed button
		if (OVRInput.GetDown(OVRInput.Button.One))
		{
			Instantiate(Bookshelf, new Vector3(0, 0, -2),
				Quaternion.Euler(0, 180f, 0), transform);
		}
	}

	private void CalculateLocalBounds()
	{
		// Capture the initial rotation and reset to 0
		Quaternion currentRotation = transform.rotation;
		transform.rotation = Quaternion.Euler(0f, 0f, 0f);

		// Create new empty bounds
		Bounds bounds = new Bounds(transform.position, Vector3.zero);

		// Grow bounds based on all children
		foreach (var childRenderer in GetComponents<MeshRenderer>())
		{
			bounds.Encapsulate(childRenderer.bounds);
		}

		// Reset center
		Vector3 localCenter = bounds.center - transform.position;
		bounds.center = localCenter;

		Debug.Log("Local bounds: " + bounds);

		transform.rotation = currentRotation;
	}
}
