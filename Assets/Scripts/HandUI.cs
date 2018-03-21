using System.Security.Cryptography;
using UnityEngine;

public class HandUI : MonoBehaviour
{
    public GameObject HandUIObj;

    private float lastPressTime = 0;
    
    private void Update()
    {
        if(OVRInput.Get(OVRInput.Button.One) && Time.time - lastPressTime > 0.5)
        {
            lastPressTime = Time.time;
            if (HandUIObj.activeInHierarchy)
            {
                HandUIObj.SetActive(false);
            }
            else
            {
                HandUIObj.SetActive(true);
            }
        }
    }
}