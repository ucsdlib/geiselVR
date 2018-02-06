using UnityEngine;

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