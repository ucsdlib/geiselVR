using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting;
using JetBrains.Annotations;
using UnityEngine;

// TODO optimize

public class RowController : MonoBehaviour
{
    public Unit TemplateUnit;
    public GameObject ReferenceUnit;
    public int RowSize = 2;
    public float ScrollTime = 0.12f;

    private readonly List<Unit> _activeUnits = new List<Unit>();
    private bool _lerping;
    private Vector3 _lerpDestPos;
    private Vector3 _firstPos; // first position in array
    private Vector3 _lastPos; // last position in array
    private Vector3 _rowInitPos; // initial position of row in world
    private float _width;


    private void Start()
    {

        // Compute initial positions
        _firstPos = Vector3.zero;
        _lastPos = _firstPos + Vector3.left * (RowSize - 1) * _width;
        _rowInitPos = transform.position;
        _width = CalculateLocalBounds(ReferenceUnit).size.x;
        Destroy(ReferenceUnit);

        // Create the row
        InstantiateArray(RowSize);
    }

    private void Update()
    {
        HandleShiftInput();
    }

    private void HandleShiftInput()
    {
        // Get both hand x-axis thumbstick value [-1, 1]
        float flexL = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick)[0];
        float flexR = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick)[0];

        // Scroll Right
        if ((flexL > 0.5 || flexR > 0.5) && !_lerping)
        {
            StartCoroutine(Scroll(true, ScrollTime));
        }
        else if ((flexL < -0.5 || flexR < -0.5) && !_lerping)
        {
            StartCoroutine(Scroll(false, ScrollTime));
        }
    }

    IEnumerator Scroll(bool right, float time)
    {
        if (!_lerping)
        {
            _lerping = true;
            // Calculate direction dependent parameters
            Vector3 start = transform.position;
            Vector3 end;
            if (right)
            {
                end = transform.position + Vector3.right * _width;
            }
            else
            {
                end = transform.position + Vector3.left * _width;
            }
            // Lerp
            float t = 0f;
            while (t < 1.0f)
            {
                t += Time.deltaTime / time; // scale by time factor
                transform.position = Vector3.Slerp(start, end, t);
                yield return null;
            }
            ShiftFrame(right);
            _lerping = false;
        }
    }

    private void ShiftFrame(bool right)
    {
        // Reset position of row
        transform.position = _rowInitPos;

        // Shift units to compensate
        if (right) // FIXME redundant with Scroll()
        {
            Unit invalidUnit = null;
            foreach (Unit unit in _activeUnits)
            {
                unit.transform.Translate(Vector3.right * _width);
                unit.Index--; // field based system because list order is not guaranteed
                if (unit.Index < 0)
                {
                    invalidUnit = unit;
                }
            }
            // Remove invalid unit
            if (invalidUnit)
            {
                _activeUnits.Remove(invalidUnit);
                Destroy(invalidUnit.gameObject);
                // Instantiate new unit
                Unit newUnit = InstantiateUnit(_lastPos);
                newUnit.Index = RowSize - 1;
                _activeUnits.Add(newUnit);
            }
        }
        else
        {
            Unit invalidUnit = null;
            foreach (Unit unit in _activeUnits)
            {
                unit.transform.Translate(Vector3.left * _width);
                unit.Index++;
                if (unit.Index >= RowSize)
                {
                    invalidUnit = unit;
                }
            }
            if (invalidUnit)
            {
                _activeUnits.Remove(invalidUnit);
                Destroy(invalidUnit.gameObject);
                Unit newUnit = InstantiateUnit(_firstPos);
                newUnit.Index = 0;
                _activeUnits.Add(newUnit);
            }
        }
    }

    private void InstantiateArray(int size)
    {
        // TODO maybe have one template objects in the scene for convenience
        for (int i = 0; i < size; i++)
        {
            // Create unit with correct offset
            Unit unit = InstantiateUnit(_firstPos + Vector3.left * i * _width);

            // Register script and assign position
            unit.Index = i;
            unit.Row = this;
            _activeUnits.Add(unit);
        }
    }

    private Unit InstantiateUnit(Vector3 position)
    {
        Unit unit = Instantiate(TemplateUnit, transform);
        unit.transform.localPosition = position;
        return unit;
    }

    private Bounds CalculateLocalBounds(GameObject obj)
    {
        // Get bounds of obj first
        Bounds bounds;
        Renderer render = obj.GetComponent<Renderer>();
        if (render)
        {
            bounds = render.bounds;
        }
        else
        {
            bounds = new Bounds(Vector3.zero, Vector3.zero);
        }

        // Try to grow bounds if needed
        if (Math.Abs(bounds.extents.x) < 0.000001f)
        {
            bounds = new Bounds(obj.transform.position, Vector3.zero);
            foreach (Transform child in obj.transform)
            {
                Renderer childRender = child.GetComponent<Renderer>();
                if (childRender)
                {
                    bounds.Encapsulate(childRender.bounds);
                }
                else
                {
                    bounds.Encapsulate(CalculateLocalBounds(child.gameObject));
                }
            }
        }
        return bounds;
    }
}