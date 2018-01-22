using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour {
    public GameObject Template; // note that OnEnable, OnDisable should be implemented
    public int StartSize;

    private readonly List<GameObject> pool = new List<GameObject>();

    private void Start()
    {
        for (var i = 0; i < StartSize; i++)
        {
            var o = Instantiate(Template);
            o.SetActive(false);
            pool.Add(o);
        }
    }
    
    public GameObject Borrow()
    {
        foreach (var o in pool)
        {
            if (o.activeInHierarchy) continue;
            o.SetActive(true);
            return o;
        }
        var newo = Instantiate(Template);
        pool.Add(newo);
        return newo;
    }
    
    public void GiveBack(GameObject o)
    {
        o.SetActive(false);
        o.transform.parent = null;
    }
}
