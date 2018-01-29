using System.Collections.Generic;
using UnityEngine;

public class ObjectPool<T> where T : MonoBehaviour {
    private readonly List<T> pool;
    private readonly Instantiator instantiator;
    private readonly Vector3 initPos;
    private readonly Quaternion initRot;
    

    public ObjectPool(Instantiator instantiator, int startSize)
    {
        this.instantiator = instantiator;
        pool = new List<T>();
        var objList = instantiator.InstantiateGroup(startSize);
        if (objList.Count == 0) return;

        var component = objList[0].GetComponent<T>();
        if (component == null)
        {
            Debug.LogError("ObjectPool: could not find required component from Instantiator return");
            return;
        }
        
        foreach (var o in objList)
        {
            pool.Add(o.GetComponent<T>());
        }

        initPos = objList[0].transform.position;
        initRot = objList[0].transform.rotation;
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
        o.transform.position = initPos;
        o.transform.rotation = initRot;
    }
}
