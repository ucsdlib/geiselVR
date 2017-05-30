using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

public class BookshelfController : MonoBehaviour
{
    public int Position;
    
    public Book BookTemplate;
    public float BookWidth = 0.1f;
    public int ShelfCount = 3;
    public float ShelfHeight = 0.37f;
    public float ShelfWidth = 1.0f;
    public float TopShelfY = 1.6f;
    public Vector3 Offset = Vector3.zero;

    private int _startCallNumber; // call number of first book
    private int _nextCallNumber; // points to the next
    private readonly LinkedList<LinkedList<Book>> _table = new LinkedList<LinkedList<Book>>();

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

        if (right)
        {
            _startCallNumber = last._nextCallNumber;
            _nextCallNumber = _startCallNumber;
        }
        else
        {
            _startCallNumber = 0;
            _nextCallNumber = _startCallNumber;
        }

        LoadBooks();
    }

    private void LoadBooks()
    {
        for (int i = 0; i < ShelfCount; i++)
        {
            Vector3 start = new Vector3(0, TopShelfY - (i * ShelfHeight), 0);
            _table.AddLast(InstantiateShelf(start, Vector3.right));
        }
    }

    private LinkedList<Book> InstantiateShelf(Vector3 start, Vector3 direction)
    {
        Vector3 current = start;
        Vector3 end = current + direction * (ShelfWidth - BookWidth);
        LinkedList<Book> books = new LinkedList<Book>();
        while (Vector3.Dot(current, direction) < Vector3.Dot(end, direction))
        {
            Book book = NextBook();
            book.transform.localPosition = current;
            book.transform.Translate(Offset);
            books.AddLast(book);
            current += direction * BookWidth;
        }
        return books;
    }

    private Book NextBook()
    {
        Book book = Instantiate(BookTemplate, transform);
        book.LoadData(_nextCallNumber);
        _nextCallNumber++;
        return book;
    }

    private void PutBackBook(Book book)
    { }
}