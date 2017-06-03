using System;
using System.Collections.Generic;
using System.Net.Sockets;
using Assets.Scripts;
using UnityEngine;
using UnitySampleAssets.Vehicles.Car;

// ReSharper disable ConvertIfStatementToSwitchStatement

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
        var last = unit.GetComponent<BookshelfController>();
        if (!last)
        {
            Debug.Log("Could not get last shelf");
        }

        switch (direction)
        {
            case Direction.Right:
                _startCallNumber = last._nextCallNumber;
                _nextCallNumber = _startCallNumber;
                LoadBooks(direction);
                break;
            case Direction.Left:
                _startCallNumber = 0;
                _nextCallNumber = _startCallNumber;
                LoadBooks(direction);
                break;
            case Direction.Identity:
                _startCallNumber = last._startCallNumber;
                _nextCallNumber = _startCallNumber;
                LoadBooks(Direction.Right);
                break;
            default:
                throw new ArgumentOutOfRangeException("direction", direction, null);
        }
    }

    private void LoadBooks(Direction direction)
    {
        if (direction == Direction.Right)
        {
            for (var i = 0; i < ShelfCount; i++)
            {
                var start = new Vector3(0, TopShelfY - i * ShelfHeight, 0);
                _table.AddLast(InstantiateShelf(start, Direction.Right));
            }
        }
        else if (direction == Direction.Left)
        {
            for (var i = ShelfCount - 1; i >= 0; i--)
            {
                var start = new Vector3(ShelfWidth, TopShelfY - i * ShelfHeight, 0);
                _table.AddFirst(InstantiateShelf(start, Direction.Left));
            }
        }
        else
        {
            const string msg = "Can only load books Left or Right";
            throw new ArgumentOutOfRangeException("direction", direction, msg);
        }
    }

    private LinkedList<Book> InstantiateShelf(Vector3 start, Direction direction)
    {
        var books = new LinkedList<Book>();
        var totalWidth = 0.0f;

        if (direction == Direction.Right)
        {
            var current = start;
            while (totalWidth < ShelfWidth)
            {
                // Create book
                var book = Instantiate(BookTemplate, transform);
                book.LoadData(_nextCallNumber);
                _nextCallNumber++;

                // Place book
                var bookOffset = book.transform.InverseTransformDirection(
                    transform.TransformDirection(Offset));
                book.transform.localPosition = current;
                book.transform.Translate(bookOffset);

                // Bookkeeping
                books.AddLast(book);
                current += book.Width * Vector3.right;
                totalWidth += book.Width;
            }

            // Put back the book that went over
            var lastBook = books.Last.Value;
            books.RemoveLast();
            Destroy(lastBook.gameObject);
            _nextCallNumber--;
        }
        else if (direction == Direction.Left)
        {
            var current = start;
            while (totalWidth < ShelfWidth)
            {
                var book = Instantiate(BookTemplate, transform);
                book.LoadData(_nextCallNumber);
                _nextCallNumber--;

                var bookOffset = book.transform.InverseTransformDirection(
                    transform.TransformDirection(Offset));
                book.transform.localPosition = current;
                book.transform.Translate(bookOffset);

                books.AddFirst(book);
                current += book.Width * Vector3.left;
                totalWidth += book.Width;
            }

            // Put back the book that went over
            var lastBook = books.First.Value;
            books.RemoveFirst();
            Destroy(lastBook.gameObject);
            _nextCallNumber++;

            // Shift shelf over by width of first book
            var shiftWidth = books.Last.Value.Width;
            foreach (var book in books)
            {
                book.transform.Translate(shiftWidth * Vector3.left);
            }
        }
        else
        {
            const string msg = "Can only instantiate shelf Left or Right";
            throw new ArgumentOutOfRangeException("direction", direction, msg);
        }

        return books;
    }
}