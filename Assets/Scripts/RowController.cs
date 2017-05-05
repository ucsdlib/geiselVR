using System;
using System.ComponentModel.Design;
using System.Net.Sockets;
using UnityEditor;
using UnityEngine;
using UnityEngine.VR.WSA;

public class RowController : MonoBehaviour
{
	public Transform Bookshelf;
	public float Width = 1.2f; // TODO find programatically
	public int Size = 2;


	private void Start ()
	{
		InstantiateArray(Size);
	}

	private void Update()
	{
		
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

	private void InstantiateArray(int size)
	{
		for (int i = 1; i <= size; i++)
		{
			Vector3 position = Bookshelf.position + Vector3.left * i * Width;
			Debug.Log(position); // DEBUG
			Instantiate(Bookshelf, position, Quaternion.identity, transform);
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
