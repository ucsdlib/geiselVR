using UnityEngine;

public class GrabbableBook : OVRGrabbable
{
    /// <summary>
    /// Forces the <see cref="OVRGrabber"/> to release this object
    /// </summary>
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
