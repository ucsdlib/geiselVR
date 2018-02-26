using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModGrabber : OVRGrabber {
    /// <summary>
    /// Release grabbed <see cref="OVRGrabbable"/>
    /// </summary>
    public void ForceRelease()
    {
        GrabEnd();
    }

    protected override void GrabVolumeEnable(bool enabled)
    {
        // The original method was problematic, and thus it has been overridden to remove its 
        // functionality
        
        m_grabVolumeEnabled = enabled;
    }
}
