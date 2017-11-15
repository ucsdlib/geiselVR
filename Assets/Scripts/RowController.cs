using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NUnit.Framework.Constraints;
using UnityEditor;
using UnityEngine;

public class RowController : MonoBehaviour
{
    public Unit TemplateUnit; // unit from which to instantiate
    public int RowSize = 2; // number of units in this row at any given time
    public int CenterPos = 2; // center shelf which contains position set by SetPosition
    public float ScrollTime = 0.12f; // time period for scroll to complete

    private readonly LinkedList<Unit> activeUnits = new LinkedList<Unit>(); // current active units
    private bool lerping;
    private Vector3 firstPos; // first position in array
    private Vector3 lastPos; // last position in array
    private float width; // width of one unit
    private bool canScrollRight = true;
    private bool canScrollLeft = true;
    private GameObject container;

    private void Start()
    {
        // set starting positions
        firstPos = Vector3.zero;

        // set up container for shifting
        container = new GameObject("Container");
        container.transform.parent = transform;
        container.transform.localPosition = Vector3.zero;
        container.transform.rotation = transform.rotation;

        var refTranform = transform.Find(TemplateUnit.name);
        if (refTranform) // reference unit found as child
        {
            var prefab = (Unit) PrefabUtility.GetPrefabParent(TemplateUnit);
            if (prefab) TemplateUnit = prefab;
            Destroy(refTranform.gameObject);
        }

        var unit = InstantiateUnit(Vector3.zero);
        unit.transform.rotation = Quaternion.Euler(0, 0, 0);
        width = CalculateLocalBounds(unit.gameObject).size.x;
        Destroy(unit.gameObject);

        StartCoroutine(InstantiateArray(RowSize));
        lastPos = firstPos + Vector3.right * (RowSize - 1) * width;
    }

    private void Update()
    {
        HandleInput();
    }

    public void NotifyEnd(Direction direction)
    {
        Debug.Log("Notifying end for: " + direction);
        switch (direction)
        {
            case Direction.Right:
                canScrollRight = false;
                break;
            case Direction.Left:
                canScrollLeft = false;
                break;
            default:
                throw new ArgumentOutOfRangeException("direction", direction, null);
        }
    }

    private void HandleInput()
    {
        // get both hand x-axis thumbstick value [-2, 1]
        var flexL = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick)[0];
        var flexR = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick)[0];

        // scroll to the left -> move shelf right
        if ((flexL > 0.5 || flexR > 0.5) && !lerping && canScrollLeft
            && activeUnits.Count > 0 && activeUnits.First.Value.DoneLoading)
        {
            StartCoroutine(Scroll(Direction.Right));
            canScrollRight = true;
        }
        // scroll to the right -> move shelf left
        else if ((flexL < -0.5 || flexR < -0.5) && !lerping && canScrollRight
                 && activeUnits.Count > 0 && activeUnits.Last.Value.DoneLoading)
        {
            StartCoroutine(Scroll(Direction.Left));
            canScrollLeft = true;
        }

        if (OVRInput.Get(OVRInput.Button.Two))
        {
            foreach (var unit in activeUnits)
            {
                StartCoroutine(unit.UpdateContents(null, Direction.Null));
            }
        }
    }

    private IEnumerator Scroll(Direction direction)
    {
        if (lerping) yield break;
        lerping = true;
        yield return _Scroll(direction);
        lerping = false;
    }

    private IEnumerator _Scroll(Direction direction)
    {
        yield return ShiftFrame(direction, ScrollTime, true);
        CycleUnits(direction);
    }

    private IEnumerator ShiftFrame(Direction direction, float time, bool realign)
    {
        // Calculate direction dependent parameters
        Vector3 end;
        Vector3 realignDir;
        if (direction == Direction.Right)
        {
            end = container.transform.localPosition + Vector3.right * width;
            realignDir = Vector3.right;
        }
        else
        {
            end = container.transform.localPosition + Vector3.left * width;
            realignDir = Vector3.left;
        }

        // Lerp
        var t = 0f;
        while (t < 1.0f)
        {
            t += Time.deltaTime / time; // scale by time factor
            container.transform.localPosition = Vector3.Slerp(Vector3.zero, end, t);
            yield return null;
        }
        container.transform.localPosition = Vector3.zero;

        // Realign position
        if (realign)
        {
            foreach (var unit in activeUnits)
            {
                unit.transform.localPosition += realignDir * width;
            }
        }
    }

    private void CycleUnits(Direction direction)
    {
        if (direction == Direction.Right)
        {
            // Remove invalid unit
            var invalidUnit = activeUnits.Last.Value;
            activeUnits.RemoveLast();
            Destroy(invalidUnit.gameObject);

            // Instantiate new unit
            var newUnit = InstantiateUnit(firstPos);
            StartCoroutine(newUnit.UpdateContents(activeUnits.First.Value, Direction.Left));
            activeUnits.AddFirst(newUnit);
        }
        else
        {
            var invalidUnit = activeUnits.First.Value;
            activeUnits.RemoveFirst();
            Destroy(invalidUnit.gameObject);

            var newUnit = InstantiateUnit(lastPos);
            StartCoroutine(newUnit.UpdateContents(activeUnits.Last.Value, Direction.Right));
            activeUnits.AddLast(newUnit);
        }
    }
    
    private void CycleUnits(Direction direction, Unit newUnit)
    {
        if (direction == Direction.Right)
        {
            // remove invalid input
            var invalidUnit = activeUnits.Last.Value;
            activeUnits.RemoveLast();
            Destroy(invalidUnit.gameObject);
            
            // place new unit
            newUnit.transform.parent = container.transform;
            newUnit.transform.localPosition = firstPos;
            activeUnits.AddFirst(newUnit);
        }
        else
        {
            var invalidUnit = activeUnits.Last.Value;
            activeUnits.RemoveFirst();
            Destroy(invalidUnit.gameObject);
            
            newUnit.transform.parent = container.transform;
            newUnit.transform.localPosition = lastPos;
            activeUnits.AddLast(newUnit);
        }
    }

    private IEnumerator InstantiateArray(int size)
    {
        canScrollLeft = canScrollRight = false;

        var firstUnit = InstantiateUnit(firstPos);
        yield return firstUnit.UpdateContents(firstUnit, Direction.Identity); // load itself
        activeUnits.AddFirst(firstUnit);

        for (var i = 1; i < size; i++)
        {
            var unit = InstantiateUnit(firstPos + Vector3.right * i * width);
            yield return unit.UpdateContents(activeUnits.Last.Value, Direction.Right);
            activeUnits.AddLast(unit);
        }

        canScrollLeft = canScrollRight = true;
    }

    private Unit InstantiateUnit(Vector3 position)
    {
        var unit = Instantiate(TemplateUnit, container.transform);
        unit.transform.localPosition = position;
        unit.Row = this;
        return unit;
    }

    private Unit InstantiateUnit()
    {
        var unit = Instantiate(TemplateUnit, container.transform);
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

    // UI Communication

    public void SetPosition(IUnit refunit)
    {
        StartCoroutine(_SetPosition(refunit));
    }

    private IEnumerator _SetPosition(IUnit refunit)
    {
        if (lerping) yield break;
        lerping = true;
        
        // start population
        var buildUnits = new LinkedList<IUnit>();
        bool result, done = false;
        new Thread(o =>
        {
            result = BuildSearchedUnits(refunit, buildUnits);
            done = true;
        }).Start();

        // clear all books
        foreach (var unit in activeUnits)
        {
            StartCoroutine(unit.UpdateContents(null, Direction.Null));
        }
        while (!UnitsDoneLoading(activeUnits)) yield return null;

        // shift empty units for effect and keep going until new row is built
        for (var i = 0; i < 30; i++)
        {
            yield return ShiftFrame(Direction.Right, ScrollTime / 2, false);
        }
        while (!done)
        {
            yield return ShiftFrame(Direction.Right, ScrollTime / 2, false);
        }

        // load in new units
        foreach (var iunit in buildUnits)
        {
            yield return ShiftFrame(Direction.Right, ScrollTime, true);
            var unit = InstantiateUnit();
            CycleUnits(Direction.Right, unit);
            StartCoroutine(unit.LoadContents(iunit));
        }
        
        lerping = false;
    }

    private bool BuildSearchedUnits(IUnit refunit, LinkedList<IUnit> list)
    {
        // place center object
        list.AddLast(refunit);
        
        // build around
        var lastUnit = refunit;
        for (var i = 0; i < CenterPos - 1; i++) // left
        {
            var unit = Manager.UnitFactory.BlankIUnit();
            list.AddLast(unit);
            lastUnit.Chain(unit, Direction.Right);
            lastUnit = unit;
        }
        lastUnit = refunit;
        for (var i = 0; i < RowSize - CenterPos; i++)
        {
            var unit = Manager.UnitFactory.BlankIUnit();
            list.AddFirst(unit);
            lastUnit.Chain(unit, Direction.Left);
            lastUnit = unit;
        }

        return refunit.Load(refunit, Direction.Identity);
    }
    
    private static bool UnitsDoneLoading(IEnumerable<Unit> units)
    {
        return units.All(unit => unit.DoneLoading);
    }
}