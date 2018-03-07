using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

/// <summary>
/// Controls a Row, an abstract entity consiting of several <see cref="Unit"/> objects which
/// manages their movement.
/// </summary>
public class RowController : MonoBehaviour
{
    [Tooltip("Number of units at any given time")]
    public int RowSize = 2;

    [Tooltip("Indicates zero-indexed position of the center unit, i.e. one in front of user")]
    public int CenterPos = 2; // center shelf which contains position set by SetPosition

    [Tooltip("Time period for scroll to complete. Lower is faster")]
    public float ScrollTime = 0.12f; // time period for scroll to complete

    [Tooltip("Enables use of blank units around the main units for padding")]
    public bool UseDummyUnits;

    [Tooltip("Enables use of keyboard as input. Useful for debugging")]
    public bool EnableKeyboardControl;

    private readonly LinkedList<Unit> activeUnits = new LinkedList<Unit>(); // current active units
    private volatile bool lerping; // true if in the process of shifting units
    private Vector3 firstPos; // first position in array
    private Vector3 lastPos; // last position in array
    private float width; // width of one unit
    private bool canScroll = true; // set to false on scroll. set to true by done unit
    private GameObject container; // contains all units
    private GameObject dummyContainer; // contains all dummy units
    private ObjectPool<Unit> unitPool;
    
    /// <summary>
    /// Notifies that scrolling is now allowed
    /// </summary>
    public void NotifyScrollOk()
    {
        canScroll = true;
    }

    private void Start()
    {
        unitPool = Manager.UnitPool;

        firstPos = Vector3.zero;

        // set up container for shifting
        container = new GameObject("Container");
        container.transform.parent = transform;
        container.transform.localPosition = Vector3.zero;
        container.transform.rotation = transform.rotation;

        // calculate width of one unit
        var unit = InstantiateUnit(Vector3.zero);
        unit.transform.rotation = Quaternion.Euler(0, 0, 0);
        width = CalculateLocalBounds(unit.gameObject).size.x;
        DestroyUnit(unit);

        dummyContainer = new GameObject("DummyContainer");
        dummyContainer.transform.parent = transform;
        dummyContainer.transform.localPosition = width * Vector3.left;
        dummyContainer.transform.rotation = transform.rotation;

        StartCoroutine(InstantiateArray(RowSize));
        lastPos = firstPos + Vector3.right * (RowSize - 1) * width;
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        // get both hand x-axis thumbstick value [-2, 1]
        var flexL = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick)[0];
        var flexR = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick)[0];

        // scroll to the left -> move shelf right
        if ((flexL > 0.5 || flexR > 0.5) && !lerping && canScroll
            && activeUnits.Count > 0 && activeUnits.First.Value.DoneLoading)
        {
            StartCoroutine(Scroll(Direction.Right));
        }
        // scroll to the right -> move shelf left
        else if ((flexL < -0.5 || flexR < -0.5) && !lerping && canScroll
                 && activeUnits.Count > 0 && activeUnits.Last.Value.DoneLoading)
        {
            StartCoroutine(Scroll(Direction.Left));
        }

        if (EnableKeyboardControl && canScroll)
        {
            if (Input.GetKey(KeyCode.RightArrow))
            {
                StartCoroutine(Scroll(Direction.Right));
            }
            else if (Input.GetKey(KeyCode.LeftArrow))
            {
                StartCoroutine(Scroll(Direction.Left));
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
        // calculate direction dependent parameters
        var dirVec = direction.ToVector();
        var end = container.transform.localPosition + dirVec * width;
        var dummyStart = firstPos + width * Vector3.left;
        var dummyEnd = dummyContainer.transform.localPosition + dirVec * width;

        // Lerp
        var t = 0f;
        while (t < 1.0f)
        {
            t += Time.deltaTime / time; // scale by time factor
            container.transform.localPosition = Vector3.Slerp(firstPos, end, t);
            dummyContainer.transform.localPosition = Vector3.Slerp(dummyStart, dummyEnd, t);
            yield return null;
        }

        // reset containers
        container.transform.localPosition = firstPos;
        dummyContainer.transform.localPosition = dummyStart;

        // move main units forward since we moved container back
        if (realign)
        {
            foreach (var unit in activeUnits)
            {
                unit.transform.localPosition += dirVec * width;
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
            DestroyUnit(invalidUnit);

            // Instantiate new unit
            var newUnit = InstantiateUnit(firstPos);
            canScroll = false; // new unit will set to true
            StartCoroutine(newUnit.UpdateContents(activeUnits.First.Value, Direction.Left));
            activeUnits.AddFirst(newUnit);
        }
        else
        {
            var invalidUnit = activeUnits.First.Value;
            activeUnits.RemoveFirst();
            DestroyUnit(invalidUnit);

            var newUnit = InstantiateUnit(lastPos);
            canScroll = false;
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
            DestroyUnit(invalidUnit);

            // place new unit
            newUnit.transform.parent = container.transform;
            newUnit.transform.localPosition = firstPos;
            activeUnits.AddFirst(newUnit);
        }
        else
        {
            var invalidUnit = activeUnits.Last.Value;
            activeUnits.RemoveFirst();
            DestroyUnit(invalidUnit);

            newUnit.transform.parent = container.transform;
            newUnit.transform.localPosition = lastPos;
            activeUnits.AddLast(newUnit);
        }
    }

    private IEnumerator InstantiateArray(int size)
    {
        canScroll = false;

        var firstUnit = InstantiateUnit(firstPos);
        yield return firstUnit.UpdateContents(firstUnit, Direction.Identity); // load itself
        activeUnits.AddFirst(firstUnit);

        for (var i = 1; i < size; i++)
        {
            var unit = InstantiateUnit(firstPos + Vector3.right * i * width);
            yield return unit.UpdateContents(activeUnits.Last.Value, Direction.Right);
            activeUnits.AddLast(unit);
        }

        if (UseDummyUnits)
        {
            // place dummies around container
            var dummyL = InstantiateUnit(firstPos + width * Vector3.left);
            var dummyR = InstantiateUnit(lastPos + width * Vector3.right);
            dummyL.transform.parent = dummyContainer.transform;
            dummyR.transform.parent = dummyContainer.transform;
            // note that dummy rotation was set to normal container rotation
            // assumption is that dummyContainer and normal container have same rotation

            // clear dummies
            dummyL.UpdateContents(dummyL, Direction.Null);
            dummyR.UpdateContents(dummyR, Direction.Null);
        }

        canScroll = true;
    }

    private Unit InstantiateUnit(Vector3 position)
    {
        var unit = unitPool.Borrow();
        unit.transform.parent = container.transform;
        unit.transform.rotation = container.transform.rotation * unit.transform.rotation;
        unit.transform.localPosition = position;
        unit.Row = this;
        return unit;
    }

    private Unit InstantiateUnit()
    {
        var unit = unitPool.Borrow();
        unit.transform.parent = container.transform;
        unit.transform.rotation = container.transform.rotation * unit.transform.rotation;
        unit.Row = this;
        return unit;
    }

    private void DestroyUnit(Unit unit)
    {
        unit.Row = null;
        unitPool.GiveBack(unit);
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
                bounds.Encapsulate(childRender
                    ? childRender.bounds
                    : CalculateLocalBounds(child.gameObject));
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
        var itr = buildUnits.Last;
        while (itr != null)
        {
            yield return ShiftFrame(Direction.Right, ScrollTime, true);
            var unit = InstantiateUnit();
            CycleUnits(Direction.Right, unit);
            StartCoroutine(unit.LoadContents(itr.Value));
            itr = itr.Previous;
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
