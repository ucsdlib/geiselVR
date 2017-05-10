using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class RowController : MonoBehaviour
{
    public Unit FirstUnit;
    public float Width = 1.2f; // TODO find programatically
    public int RowSize = 2;
    public float ScrollTime = 0.12f;

    private readonly List<Unit> _activeUnits = new List<Unit>();
    private bool _lerping;
    private Vector3 _lerpDestPos;
    private Vector3 _firstPos; // first position in array
    private Vector3 _lastPos; // last position in array
    private Vector3 _rowInitPos; // initial position of row in world


    private void Start()
    {
        InstantiateArray(RowSize);

        // Save initial positions
        _firstPos = FirstUnit.transform.position;
        _lastPos = _firstPos + Vector3.left * (RowSize - 1) * Width;
        _rowInitPos = transform.position;
    }

    private void Update()
    {
        HandleShiftInput();
    }

    private void HandleShiftInput()
    {
        // Get both hand x-axis thumbstick value [-1, 1]
        // TODO enable both joysticks
        float flexR = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick)[0];

        if (flexR > 0.5 && !_lerping)
        {
            Vector3 destination = transform.position + Vector3.right * Width;
            StartCoroutine(SmoothMove(transform.position, destination, ScrollTime));
        }
    }

    IEnumerator SmoothMove(Vector3 start, Vector3 end, float time)
    {
        if (!_lerping)
        {
            _lerping = true;
            float t = 0f;
            while (t < 1.0f)
            {
                t += Time.deltaTime / time; // scale by time factor
                transform.position = Vector3.Slerp(start, end, t);
                yield return null;
            }
            _lerping = false;
            ShiftFrame();
        }
    }

    private void ShiftFrame()
    {
        // Reset position of row
        transform.position = _rowInitPos;

        // Shift units internally to compensate
        Unit invalidUnit = null;
        foreach (Unit unit in _activeUnits)
        {
            unit.transform.Translate(Vector3.right * Width);
            unit.Position--;
            if (unit.Position < 0)
            {
                invalidUnit = unit;
            }
        }

        // Remove invalid units
        if (invalidUnit)
        {
            _activeUnits.Remove(invalidUnit);
            Destroy(invalidUnit.gameObject);
        }

        // Instantiate a new unit
        Unit newUnit = Instantiate(FirstUnit, _lastPos, Quaternion.identity, transform);
        newUnit.Position = RowSize - 1;
        _activeUnits.Add(newUnit);
    }

    private void InstantiateArray(int size)
    {
        // Register first unit
        FirstUnit.Position = 0;
        FirstUnit.Row = this;
        _activeUnits.Add(FirstUnit);

        // Instantiate units in correct position and add to _activeUnits
        for (int i = 1; i < size; i++)
        {
            // Create unit
            Vector3 position = FirstUnit.transform.position + Vector3.left * i * Width;
            Unit item = Instantiate(FirstUnit, position, Quaternion.identity, transform);

            // Register script and assign position
            item.Position = i;
            item.Row = this;
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