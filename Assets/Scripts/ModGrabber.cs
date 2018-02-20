using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModGrabber : OVRGrabber {
    public void ForceRelease()
    {
        GrabEnd();
    }

    protected override void GrabVolumeEnable(bool enabled)
    {
        // this behavior causes problems and it is unclear
        // why oculus chose to implement it
        m_grabVolumeEnabled = enabled;
    }
}
