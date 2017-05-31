using System;
using System.Collections;
using System.Collections.Generic;
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
        // Set starting positions
        _firstPos = Vector3.zero;
        _rowInitPos = transform.position;
        
        // Instantiate array
        Transform refTranform = transform.Find(TemplateUnit.name);
        if (refTranform) // reference unit found as child
        {
            _width = CalculateLocalBounds(refTranform.gameObject).size.x;
            
            // Destroy refrence after to instantiate array based on reference parameters
            InstantiateArray(RowSize);
            Destroy(refTranform.gameObject);
            
            // Find prefab if not already prefab
            Unit prefab = (Unit) PrefabUtility.GetPrefabParent(TemplateUnit);
            if (prefab) TemplateUnit = prefab;
        }
        else 
        {
            // Calculate bounds with sample instantiated prefab
            Unit unit = InstantiateUnit(Vector3.zero);
            _width = CalculateLocalBounds(unit.gameObject).size.x;
            Destroy(unit.gameObject);
            InstantiateArray(RowSize);
        }
        _lastPos = _firstPos + Vector3.right * (RowSize - 1) * _width;
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
            Unit newUnit = InstantiateUnit(_firstPos);
            newUnit.UpdateContentsDelegate(_activeUnits.First.Value, false);
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
            
            Unit newUnit = InstantiateUnit(_lastPos);
            newUnit.UpdateContentsDelegate(_activeUnits.Last.Value, true);
            _activeUnits.AddLast(newUnit);
        }
    }

    /**
    Instantiates an array of units dynamically
    @param size    number of units to instantiate
    */
    private void InstantiateArray(int size)
    {
        Unit firstUnit = InstantiateUnit(_firstPos); // only for reference
        firstUnit.Row = this;
        _activeUnits.AddFirst(firstUnit);
        for (int i = 1; i < size; i++)
        {
            // Create unit with correct offset
            Unit unit = InstantiateUnit(_firstPos + Vector3.right * i * _width);

            // Register script and assign position
            unit.Row = this;
            unit.UpdateContentsDelegate(_activeUnits.Last.Value, true);
            _activeUnits.AddLast(unit);
        }
        
        // Regenerate 1st unit based on 2nd one
        firstUnit.UpdateContentsDelegate(_activeUnits.First.Value, false);
        // FIXME first unit does not start on assigned value, only second unit does
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
        Renderer render = obj.GetComponent<Renderer>();
        Bounds bounds = render ? render.bounds : new Bounds(Vector3.zero, Vector3.zero);

        // Try to grow bounds if needed
        if (Math.Abs(bounds.extents.x) < 0.000001f)
        {
            bounds = new Bounds(obj.transform.position, Vector3.zero);
            foreach (Transform child in obj.transform)
            {
                Renderer childRender = child.GetComponent<Renderer>();
                bounds.Encapsulate(childRender ? childRender.bounds : CalculateLocalBounds(child.gameObject));
            }
        }
        return bounds;
    }
}