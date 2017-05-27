using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

/**
Controlls a row of arbitrary units and allows endless scrolling in either direction
*/
public class RowController : MonoBehaviour
{
    public Unit TemplateUnit; // unit from which to instantiate
    public int RowSize = 2; // number of units in this row at any given time
    public float ScrollTime = 0.12f; // time period for scroll to complete

    private readonly LinkedList<Unit> _activeUnits = new LinkedList<Unit>();
    private bool _lerping;
    private Vector3 _lerpDestPos;
    private Vector3 _firstPos; // first position in array
    private Vector3 _lastPos; // last position in array
    private Vector3 _rowInitPos; // initial position of row in world
    private float _width; // width of one unit


    private void Start()
    {
        // TODO make sure TemplateUnit is not null and has/is prefab
        // Compute initial positions
        GameObject refObj = GameObject.Find(TemplateUnit.name);
        if (refObj) // reference object placed in scene
        {
            _width = CalculateLocalBounds(refObj).size.x;
            Destroy(refObj);

            // Find prefab if not already prefab
            Unit prefab = (Unit) PrefabUtility.GetPrefabParent(TemplateUnit);
            if (prefab) TemplateUnit = prefab;
        }
        else // no reference object in scene
        {
            Unit unit = InstantiateUnit(Vector3.zero);
            _width = CalculateLocalBounds(unit.gameObject).size.x;
            Destroy(unit.gameObject);
        }
        _firstPos = Vector3.zero;
        _lastPos = _firstPos + Vector3.left * (RowSize - 1) * _width;
        _rowInitPos = transform.position;

        // Create the row
        InstantiateArray(RowSize);
    }

    private void Update()
    {
        HandleShiftInput();
    }

    /**
    Makes appropiate method calls based on input
    */
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

    /**
    Handles scrolling animation of units
    */
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

    /**
    Handles instantiation to create endless scrolling.
    @param right    true if shifting right, false otherwise
    */
    private void ShiftFrame(bool right)
    {
        // Reset position of row
        transform.position = _rowInitPos;

        if (right) // FIXME redundant with Scroll()
        {
            // Shift units to compensate
            foreach (Unit unit in _activeUnits)
            {
                unit.transform.Translate(Vector3.right * _width);
            }

            // Remove invalid unit
            Unit invalidUnit = _activeUnits.Last.Value;
            _activeUnits.RemoveLast();
            Destroy(invalidUnit.gameObject);

            // Instantiate new unit
            Unit newUnit = InstantiateUnit(_lastPos);
            newUnit.Index = RowSize - 1;
            _activeUnits.AddFirst(newUnit);
        }
        else
        {
            foreach (Unit unit in _activeUnits)
            {
                unit.transform.Translate(Vector3.left * _width);
            }

            Unit invalidUnit = _activeUnits.First.Value;
            _activeUnits.RemoveFirst();
            Destroy(invalidUnit.gameObject);
            
            Unit newUnit = InstantiateUnit(_firstPos);
            newUnit.Index = 0;
            _activeUnits.AddLast(newUnit);
        }
    }

    /**
    Instantiates an array of units dynamically
    @param size    number of units to instantiate
    */
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
            _activeUnits.AddFirst(unit);
        }
    }

    /**
    Creates one unit at the given position
    */
    private Unit InstantiateUnit(Vector3 position)
    {
        Unit unit = Instantiate(TemplateUnit, transform);
        unit.transform.localPosition = position;
        return unit;
    }

    /**
    Calculates bounding box of any gameobject and its descendants
    */
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