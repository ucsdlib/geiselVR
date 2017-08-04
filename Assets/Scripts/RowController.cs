using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

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
    private bool _canScrollRight = true;
    private bool _canScrollLeft = true;

    private void Start()
    {
        // set starting positions
        _firstPos = Vector3.zero;
        _rowInitPos = transform.position;

        var refTranform = transform.Find(TemplateUnit.name);
        if (refTranform) // reference unit found as child
        {
            // get width and prefab from reference
            _width = CalculateLocalBounds(refTranform.gameObject).size.x;
            var prefab = (Unit) PrefabUtility.GetPrefabParent(TemplateUnit);
            if (prefab) TemplateUnit = prefab;
            Destroy(refTranform.gameObject);

            StartCoroutine(InstantiateArray(RowSize));
        }
        else
        {
            // get width from new unit
            var unit = InstantiateUnit(Vector3.zero);
            _width = CalculateLocalBounds(unit.gameObject).size.x;
            Destroy(unit.gameObject);

            StartCoroutine(InstantiateArray(RowSize));
        }
        _lastPos = _firstPos + Vector3.right * (RowSize - 1) * _width;
    }

    private void Update()
    {
        HandleShiftInput();
    }

    public void NotifyEnd(Direction direction)
    {
        Debug.Log("Notifying end for: " + direction);
        switch (direction)
        {
            case Direction.Right:
                _canScrollRight = false;
                break;
            case Direction.Left:
                _canScrollLeft = false;
                break;
            case Direction.Identity:
                break;
            default:
                throw new ArgumentOutOfRangeException("direction", direction, null);
        }
    }

    private void HandleShiftInput()
    {
        // get both hand x-axis thumbstick value [-2, 1]
        float flexL = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick)[0];
        float flexR = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick)[0];

        // scroll to the left -> move shelf right
        if ((flexL > 0.5 || flexR > 0.5) && !_lerping && _canScrollLeft
            && _activeUnits.Count > 0 && _activeUnits.First.Value.DoneLoading)
        {
            StartCoroutine(Scroll(Direction.Right, ScrollTime));
            _canScrollRight = true;
        }
        // scroll to the right -> move shelf left
        else if ((flexL < -0.5 || flexR < -0.5) && !_lerping && _canScrollRight
                 && _activeUnits.Count > 0 && _activeUnits.Last.Value.DoneLoading)
        {
            StartCoroutine(Scroll(Direction.Left, ScrollTime));
            _canScrollLeft = true;
        }
    }

    IEnumerator Scroll(Direction direction, float time)
    {
        if (!_lerping)
        {
            _lerping = true;
            // Calculate direction dependent parameters
            Vector3 start = transform.position;
            Vector3 end;
            if (direction == Direction.Right)
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
            ShiftFrame(direction);
            _lerping = false;
        }
    }

    private void ShiftFrame(Direction direction)
    {
        // Reset position of row
        transform.position = _rowInitPos;

        if (direction == Direction.Right)
        {
            // Shift units to compensate
            foreach (Unit unit in _activeUnits)
            {
                unit.transform.Translate(Vector3.right * _width);
            }

            // Remove invalid unit
            var invalidUnit = _activeUnits.Last.Value;
            _activeUnits.RemoveLast();
            Destroy(invalidUnit.gameObject);

            // Instantiate new unit
            var newUnit = InstantiateUnit(_firstPos);
            StartCoroutine(newUnit.UpdateContents(_activeUnits.First.Value, Direction.Left));
            _activeUnits.AddFirst(newUnit);
        }
        else
        {
            foreach (Unit unit in _activeUnits)
            {
                unit.transform.Translate(Vector3.left * _width);
            }

            var invalidUnit = _activeUnits.First.Value;
            _activeUnits.RemoveFirst();
            Destroy(invalidUnit.gameObject);

            var newUnit = InstantiateUnit(_lastPos);
            StartCoroutine(newUnit.UpdateContents(_activeUnits.Last.Value, Direction.Right));
            _activeUnits.AddLast(newUnit);
        }
    }

    private IEnumerator InstantiateArray(int size)
    {
        _canScrollLeft = _canScrollRight = false;
        
        var firstUnit = InstantiateUnit(_firstPos);
        yield return firstUnit.UpdateContents(firstUnit, Direction.Identity); // load itself
        _activeUnits.AddFirst(firstUnit);

        for (var i = 1; i < size; i++)
        {
            var unit = InstantiateUnit(_firstPos + Vector3.right * i * _width);
            yield return unit.UpdateContents(_activeUnits.Last.Value, Direction.Right);
            _activeUnits.AddLast(unit);
        }

        _canScrollLeft = _canScrollRight = true;
    }

    private Unit InstantiateUnit(Vector3 position)
    {
        Unit unit = Instantiate(TemplateUnit, transform);
        unit.transform.localPosition = position;
        unit.Row = this;
        return unit;
    }

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