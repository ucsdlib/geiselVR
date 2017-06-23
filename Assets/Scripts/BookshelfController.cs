using System;
using System.Collections.Generic;
using UnityEngine;

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
    public bool ShowGuides;

    private int _startCallNumber; // call number of first book
    private int _endCallNumber; // call number of last book
    private int _nextCallNumber; // internal iteration
    private readonly List<LinkedList<Book>> _table = new List<LinkedList<Book>>();

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
                _startCallNumber = last._endCallNumber + 1;
                _nextCallNumber = _startCallNumber;
                LoadBooks(direction);
                break;
            case Direction.Left:
                _startCallNumber = 0; // determined after load
                _nextCallNumber = last._startCallNumber - 1;
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
            // Generate shelf list
            for (var i = 0; i < ShelfCount; i++)
            {
                var books = new LinkedList<Book>();
                var totalWidth = 0.0f;

                // Populate until over limit
                while (totalWidth <= ShelfWidth)
                {
                    // Create book
                    var book = Instantiate(BookTemplate);
                    book.LoadMeta(_nextCallNumber);
                    _nextCallNumber++;

                    // Bookkeeping
                    totalWidth += book.Width;
                    books.AddLast(book);
                }

                // Put back offending book
                if (books.Count != 0)
                {
                    Destroy(books.Last.Value.gameObject);
                    books.RemoveLast();
                    _nextCallNumber--;
                }
                _table.Add(books);
            }

            // Position and load
            InstantiateTable();
        }
        else if (direction == Direction.Left)
        {
            // Generate shelf list
            for (var i = 0; i < ShelfCount; i++)
            {
                var books = new LinkedList<Book>();
                var totalWidth = 0.0f;

                // Populate until over limit
                while (totalWidth <= ShelfWidth)
                {
                    var book = Instantiate(BookTemplate);
                    book.LoadMeta(_nextCallNumber);
                    _nextCallNumber--;

                    totalWidth += book.Width;
                    books.AddFirst(book);
                }

                // Put back offending book
                if (books.Count != 0)
                {
                    Destroy(books.First.Value.gameObject);
                    books.RemoveFirst();
                    _nextCallNumber++;
                }
                _table.Insert(0, books);
            }

            // Position and load
            InstantiateTable();
            _startCallNumber = _table[0].First.Value.CallNumber;
        }
        else
        {
            const string msg = "Can only load books Left or Right";
            throw new ArgumentOutOfRangeException("direction", direction, msg);
        }

        // Store call number of last book
        _endCallNumber = _table[_table.Count - 1].Last.Value.CallNumber;
    }

    private void InstantiateTable()
    {
        for (var i = 0; i < ShelfCount; i++)
        {
            var start = new Vector3(0, TopShelfY - i * ShelfHeight, 0);
            InstantiateShelf(start, Vector3.right, _table[i]);
        }
    }

    private void InstantiateShelf(Vector3 start, Vector3 u, LinkedList<Book> books)
    {
        var shelfGameObj = new GameObject("Shelf");
        shelfGameObj.transform.parent = transform;
        shelfGameObj.transform.localPosition = start;
        shelfGameObj.transform.Translate(Offset);

        // Instantiate in order
        var current = Vector3.zero;
        foreach (var book in books)
        {
            book.transform.parent = shelfGameObj.transform;
            book.transform.localPosition = current;
            current += book.Width * u;

            book.LoadData();
        }
    }
}