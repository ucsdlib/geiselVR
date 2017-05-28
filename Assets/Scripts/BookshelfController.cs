using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

public class BookshelfController : MonoBehaviour
{
    public Book BookTemplate;
    public float BookWidth = 0.1f;
    public int ShelfCount = 3;
    public float ShelfHeight = 0.37f;
    public float ShelfWidth = 1.0f;
    public float TopShelfY = 1.6f;
    public Vector3 Offset = Vector3.zero;

    private int _startCallNumber; // call number of first book
    private int _callNumber; // temporary used during loading

    private void Awake()
    {
        // Establish connection to Unit
        Unit unit = GetComponent<Unit>();
        if (unit != null)
        {
            unit.UpdateContentsDelegate += HandleUpdateEvent;
        }
    }

    public void HandleUpdateEvent(Unit unit, bool right)
    {
        BookshelfController last = unit.GetComponent<BookshelfController>();
        if (!last)
        {
            Debug.Log("Could not get last shelf");
        }

        _startCallNumber = last._startCallNumber - 27; // FIXME
        _callNumber = _startCallNumber;

        LoadBooks();
    }

    private void LoadBooks()
    {
        for (int i = 0; i < ShelfCount; i++)
        {
            Vector3 start = new Vector3(-ShelfWidth, TopShelfY - (i * ShelfHeight), 0);
            InstantiateShelf(start, Vector3.right);
        }
    }

    private void InstantiateShelf(Vector3 start, Vector3 direction)
    {
        Vector3 current = start;
        Vector3 end = current + direction * (ShelfWidth - BookWidth);
        while (Vector3.Dot(current, direction) < Vector3.Dot(end, direction))
        {
            Book book = NextBook();
            book.transform.localPosition = current;
            book.transform.Translate(Offset);
            current += direction * BookWidth;
        }
    }

    private Book NextBook()
    {
        Book book = Instantiate(BookTemplate, transform);
        book.LoadData(_callNumber);
        _callNumber++;
        return book;
    }

    private void PutBackBook(Book book)
    { }
}