using System.Collections.Generic;
using UnityEngine;

public class RowController : MonoBehaviour
{
	public Transform Unit;
	public float Width = 1.2f; // TODO find programatically
	public int RowSize = 2;

	private readonly List<Transform> _activeUnits = new List<Transform>();
	private bool _lerping = false;
	private Vector3? _lerpDestPos = null;
	private Vector3 firstPos;				// first position in array
	private Vector3 lastPos;				// last position in array
	private Vector3 rowPos;			// initial position of row in world


	private void Start ()
	{
		InstantiateArray(RowSize);

		// Save initial positions
		firstPos = Unit.position;
		lastPos = firstPos + Vector3.left * RowSize * Width;
		rowPos = transform.position;
	}

	private void Update()
	{
		HandleShiftInput();
	}

	private void InstantiateOnPress()
	{
		// Check use pressed button
		if (OVRInput.GetDown(OVRInput.Button.One))
		{
			Instantiate(Unit, new Vector3(0, 0, -2),
				Quaternion.Euler(0, 180f, 0), transform);
		}
	}

	private void HandleShiftInput()
	{
		// Get both hand x-axis thumbstick value [-1, 1]
		// TODO to have both, you need a flag that sets which hand you are using at the start
//		float flexL = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick)[0];
		float flexR = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick)[0];

		if (flexR > 0.5)
		{
			// Capture destination position
			if (!_lerping)
			{

			}
		}
	}

	private void InstantiateArray(int size)
	{
		// Instantiate units in correct position and add to _activeList
		for (int i = 1; i <= size; i++)
		{
			Vector3 position = Unit.position + Vector3.left * i * Width;
			Transform item = Instantiate(Unit, position, Quaternion.identity, transform);
			_activeUnits.Add(item);
		}
	}


	private void TestCalculateLocalBounds()
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
