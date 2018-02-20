using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerLogUtility : MonoBehaviour
{
    public string Tag = "TriggerLog";
    
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(Tag + ": trigger enter");
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log(Tag + ": trigger exit");
    }
}
