using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : MonoBehaviour {
    private readonly List<T> pool;
    private Instantiator instantiator;

    public ObjectPool(Instantiator instantiator, int startSize)
    {
        this.instantiator = instantiator;
        pool = new List<T>();
        var objList = instantiator.InstantiateGroup(startSize);

        foreach (var o in objList)
        {
            pool.Add(o.GetComponent<T>());
        }
    }
    
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
    
    public void GiveBack(T o)
    {
        o.gameObject.SetActive(false);
        o.transform.parent = null;
    }
}
