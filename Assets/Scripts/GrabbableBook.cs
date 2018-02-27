using UnityEngine;

[RequireComponent(typeof(BookController))]
public class GrabbableBook : OVRGrabbable
{
    private BookController bookController;

    private void OnEnable()
    {
        // Would have preferred to use Awake() but this is not available to be overriden
        if (bookController == null)
        {
            bookController = GetComponent<BookController>();
        }
    }

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

    public override void GrabBegin(OVRGrabber hand, Collider grabPoint)
    {
        base.GrabBegin(hand, grabPoint);
        transform.parent = null;
        if (bookController.ParentBookshelf != null)
        {
            bookController.ParentBookshelf.ReleaseBook(bookController);
        }
    }
}