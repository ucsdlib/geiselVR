using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModGrabber : OVRGrabber {
    public void ForceRelease()
    {
        GrabEnd();
    }
}
