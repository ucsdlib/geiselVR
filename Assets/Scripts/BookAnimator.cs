using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Advertisements;

public class BookAnimator : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger enter"); // DEBUG
        var bookController = other.GetComponent<BookController>();
        if (bookController == null)
        {
            Debug.Log("Could not find book controller"); // DEBUG
            return;
        }

        Debug.Log("Changing animate"); // DEBUG
        bookController.Display.Animate = true;
    }

    private void OnTriggerExit(Collider other)
    {
        var bookController = other.GetComponent<BookController>();
        if (bookController == null)
        {
            Debug.Log("Could not find book controller"); // DEBUG
            return;
        }

        Debug.Log("Changing animate"); // DEBUG
        bookController.Display.Animate = false;
    }
}