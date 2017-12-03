using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabbableBook : OVRGrabbable
{
    public void ForceEnd()
    {
        if (!isGrabbed) return;
        var modGrabber = m_grabbedBy as ModGrabber;
        if (modGrabber == null)
        {
            Debug.LogError("Book needs ModGrabber to force end");
            return;
        }
        modGrabber.ForceRelease();
    }
}
