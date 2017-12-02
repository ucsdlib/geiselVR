using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReceptacleTrigger : MonoBehaviour {

	public DataUI dataUI;

	private void OnTriggerEnter(Collider other) {
		Debug.Log ("Triggered");

		var bookController = other.GetComponentInParent<BookController> ();
		if (bookController == null) {
			Debug.Log ("Could not find controller");
			return;
		}

		Debug.Log ("Found Book");
	}
}
