using UnityEngine;

public class HandUI : MonoBehaviour
{
    public GameObject HandUIObj;

    private float lastPressTime = 0;

    private void Update()
    {
        if (OVRInput.Get(OVRInput.Button.One) && Time.time - lastPressTime > 0.5)
        {
            lastPressTime = Time.time;
            HandUIObj.SetActive(!HandUIObj.activeInHierarchy);
        }
    }
}