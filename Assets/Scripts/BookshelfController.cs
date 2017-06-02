using System.Collections;
using System.Collections.Generic;
using System.Deployment.Internal;
using Assets.Scripts;
using UnityEngine;

public class BookshelfController : MonoBehaviour
{
    public int Position;

    public Book BookTemplate;
    public int ShelfCount = 3;
    public float ShelfHeight = 0.37f;
    public float ShelfWidth = 1.0f;
    public float TopShelfY = 1.6f;
    public Vector3 Offset = Vector3.zero;
    public bool ShowGuides = false;

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

    private void OnDrawGizmos()
    {
        // Draw guides if needed
        if (ShowGuides)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < ShelfCount; i++)
            {
                Vector3 center = Vector3.zero;
                center.x = ShelfWidth / 2;
                center.y = TopShelfY - i * ShelfHeight;
                center.z = -0.14f;
                center += transform.TransformDirection(Offset);
                center = transform.TransformPoint(center);

                Vector3 size = new Vector3(ShelfWidth, 0, 0.28f);
                
                Gizmos.DrawCube(center, size);
            }
        }
    }

    public void HandleUpdateEvent(Unit unit, Direction direction)
    {
        BookshelfController last = unit.GetComponent<BookshelfController>();
        if (!last)
        {
            Debug.Log("Could not get last shelf");
        }

        if (direction == Direction.Right)
        {
            _startCallNumber = last._nextCallNumber;
            _nextCallNumber = _startCallNumber;
        }
        else
        {
            _startCallNumber = 0;
            _nextCallNumber = _startCallNumber;
        }

        LoadBooks(direction);
    }

    private void LoadBooks(Direction direction)
    {
        if (direction == Direction.Right)
        {
            for (int i = 0; i < ShelfCount; i++)
            {
                Vector3 start = new Vector3(0, TopShelfY - i * ShelfHeight, 0);
                _table.AddLast(InstantiateShelf(start, Vector3.right));
            }
        }
        else
        {
            for (int i = ShelfCount - 1; i >= 0; i--)
            {
                Vector3 start = new Vector3(ShelfWidth, TopShelfY - i * ShelfHeight, 0);
                InstantiateShelf(start, Vector3.left); // DEBUG
                _table.AddFirst(InstantiateShelf(start, Vector3.left));
            }
        }
    }

    private LinkedList<Book> InstantiateShelf(Vector3 start, Vector3 u)
    {
        Vector3 current = start;
        LinkedList<Book> books = new LinkedList<Book>();
        float totalWidth = 0.0f;
        
        while (totalWidth < ShelfWidth)
        {
            // Create book
            Book book = NextBook();
            book.transform.localPosition = current;
            
            // Offset book
            Vector3 bookOffset = book.transform.InverseTransformDirection(
                transform.TransformDirection(Offset));
            book.transform.Translate(bookOffset);
            
            // Bookkeeping
            books.AddLast(book); // FIXME the internal order of the linked list is not guaranteed
            current += u * book.Width;
            totalWidth += book.Width;
        }
        
        // FIXME 
        // Put back last book that went over
        // FIXME this is too expensive, use NextWidth()
//        PutBackBook(books.Last.Value);
//        books.RemoveLast();
        
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
    {
        Destroy(book.gameObject);
        _nextCallNumber--;
    }
}