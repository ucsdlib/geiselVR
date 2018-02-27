using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic pool of objects which allows efficient reuse during runtime. Requires an
/// <see cref="Instantiator"/> to function. The reason these are separated is to avoid having to
/// call GetComponent.
/// </summary>
/// <typeparam name="T">Type of object to pool. Typically, this is a component attached to a
/// GameObject.
/// <see cref="Instantiator.Template"/></typeparam>
public class ObjectPool<T> where T : MonoBehaviour
{
    private readonly List<T> pool;
    private readonly Instantiator instantiator;
    private readonly Vector3 initPos;
    private readonly Quaternion initRot;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="instantiator">Instantiator which will hold desired gameobject. <see cref="T"/>
    /// must be a componenent on the Instantiator's template</param>
    /// <param name="startSize">Initial size of pool</param>
    public ObjectPool(Instantiator instantiator, int startSize)
    {
        this.instantiator = instantiator;
        pool = new List<T>();
        var objList = instantiator.InstantiateGroup(startSize);
        if (objList.Count == 0) return;

        var component = objList[0].GetComponent<T>();
        if (component == null)
        {
            Debug.LogError(
                "ObjectPool: could not find required component from Instantiator return");
            return;
        }

        foreach (var o in objList)
        {
            pool.Add(o.GetComponent<T>());
        }

        initPos = objList[0].transform.position;
        initRot = objList[0].transform.rotation;
    }

    /// <summary>
    /// Obtain an item from the pool. The transform of the returned object is guaranteed to be the
    /// same as the <see cref="Instantiator.Template"/> in Instantior. If borrowing another object
    /// would exceed the pool's current capacity, it will be expanded to meet demand.
    /// </summary>
    /// <returns>An item from the pool</returns>
    public T Borrow()
    {
        foreach (var o in pool)
        {
            if (o.gameObject.activeInHierarchy) continue;
            o.gameObject.SetActive(true);
            return o;
        }

        // expand size if needed
        var newo = instantiator.InstantiateSingle().GetComponent<T>();
        pool.Add(newo);
        return newo;
    }

    /// <summary>
    /// Give back an item to the pool, effectively relinquishing it. The item will be deactivated
    /// and stored for reuse.
    /// </summary>
    /// <param name="o">Item to give back</param>
    public void GiveBack(T o)
    {
        o.gameObject.SetActive(false);
        o.transform.parent = null;
        o.transform.position = initPos;
        o.transform.rotation = initRot;
    }
}