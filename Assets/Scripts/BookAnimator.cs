using UnityEngine;

/// <summary>
/// Causes all <see cref="BookController"/> within colliders to start animating
/// </summary>
public class BookAnimator : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var bookController = other.GetComponent<BookController>();
        if (bookController == null)
        {
            return;
        }

        bookController.Display.Animate = true;
    }

    private void OnTriggerExit(Collider other)
    {
        var bookController = other.GetComponent<BookController>();
        if (bookController == null)
        {
            return;
        }

        bookController.Display.Animate = false;
    }
}