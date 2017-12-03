using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReceptacleTrigger : MonoBehaviour
{
    public DataUI dataUI;
    public Transform SnapPoint;

    private void OnTriggerEnter(Collider other)
    {
        var bookController = other.GetComponentInParent<BookController>();
        if (bookController == null)
        {
            Debug.Log("Could not find controller");
            return;
        }
        
        other.GetComponent<GrabbableBook>().ForceEnd();
        other.GetComponent<Rigidbody>().isKinematic = true;
//        bookController.transform.parent = transform;
        bookController.transform.position = SnapPoint.position;
        bookController.transform.rotation = SnapPoint.rotation;
    }
}