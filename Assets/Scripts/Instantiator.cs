using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class Instantiator : MonoBehaviour
{
    public GameObject Template;

    public List<GameObject> InstantiateGroup(int size)
    {
        var group = new List<GameObject>();

        for (var i = 0; i < size; i++)
        {
            var o = Instantiate(Template);
            Template.SetActive(false);
            group.Add(o);
        }

        return group;
    }

    public void DestroyGroup(IEnumerable<GameObject> objs)
    {
        foreach (var o in objs)
        {
            Destroy(o);
        }
    }
}