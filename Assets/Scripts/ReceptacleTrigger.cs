using UnityEngine;

/// <summary>
/// Controls behavior of a Book Receptacle, a device which takes <see cref="Book"/> objects and
/// populates a <see cref="DataUi"/> object with its data.
/// </summary>
public class ReceptacleTrigger : MonoBehaviour
{
    /// <summary>
    /// Will display <see cref="Book"/> object data
    /// </summary>
    public DataUI DataUi;
    
    /// <summary>
    /// Position to which objects will snap too when received
    /// </summary>
    public Transform SnapPoint;

    private Collider obj = null; // stores currently held object
    
    private void OnTriggerEnter(Collider other)
    {
        if (obj != null) return;
        var bookController = other.GetComponent<BookController>();
        if (bookController == null)
        {
            Debug.Log("Could not find controller");
            return;
        }
        
        other.GetComponent<GrabbableBook>().ForceEnd();
        other.GetComponent<Rigidbody>().isKinematic = true;
        bookController.transform.parent = transform;
        bookController.transform.position = SnapPoint.position;
        bookController.transform.rotation = SnapPoint.rotation;

        DataUi.SetData(bookController.Meta);
        obj = other;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == obj)
        {
            obj = null;
            DataUi.Clear();
        }
    }
}