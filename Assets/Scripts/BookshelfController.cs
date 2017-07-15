using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.AccessControl;
using UnityEngine;

// ReSharper disable ConvertIfStatementToSwitchStatement

public class BookshelfController : MonoBehaviour
{
    public string CallNumber;
    public Book BookTemplate;
    public int DbBufferSize = 50;
    public float MaxBookSize = 500f;
    public int ShelfCount = 3;
    public float ShelfHeight = 0.37f;
    public float ShelfWidth = 1.0f;
    public float TopShelfY = 1.6f;
    public Vector3 Offset = Vector3.zero;
    public bool ShowGuides;

    private string _startCallNumber;
    private string _endCallNumber;
    private readonly List<LinkedList<Book>> _table = new List<LinkedList<Book>>();

    private void Awake()
    {
        // Establish connection to Unit
        Unit unit = GetComponent<Unit>();
        if (unit != null)
        {
            unit.UpdateContentsDelegate += HandleUpdateEvent;
        }

        _startCallNumber = CallNumber; // DEBUG
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

        // FIXME The Identity direction does not include the starting book when populating
        switch (direction)
        {
            case Direction.Right:
                GenerateBooks(direction, last._endCallNumber);
                break;
            case Direction.Left:
                GenerateBooks(direction, last._startCallNumber);
                break;
            case Direction.Identity:
                GenerateBooks(Direction.Right, last._startCallNumber);
                break;
            default:
                throw new ArgumentOutOfRangeException("direction", direction, message: null);
        }
    }

    private void GenerateBooks(Direction direction, string startCallNum)
    {
        bool forward;
        if (direction == Direction.Right)
        {
            forward = true;
        }
        else if (direction == Direction.Left)
        {
            forward = false;
        }
        else
        {
            const string msg = "Can only load books Left or Right";
            throw new ArgumentOutOfRangeException("direction", direction, msg);
        }

        var buffer = new DbBuffer(startCallNum, DbBufferSize, forward);

        // generate shelf list // FIXME on left, this loop should be going the other way
        for (var i = 0; i < ShelfCount; i++)
        {
            var books = GenerateShelf(buffer, direction);

            if (books.Count == 0) break;

            _table.Add(books);
        }

        // make sure we actually loaded some books
        if (_table.Count > 0)
        {
            _startCallNumber = _table[0].First.Value.CallNumber;
            _endCallNumber = _table[_table.Count - 1].Last.Value.CallNumber;
        }
        else
        {
            // TODO tell the row controller we are out of books in this direction
        }

        InstantiateTable(); // FIXME this needs to be aware of direction
    }

    private LinkedList<Book> GenerateShelf(DbBuffer buffer, Direction direction)
    {
        var books = new LinkedList<Book>();
        var totalWidth = 0.0f;

        while (totalWidth <= ShelfWidth)
        {

            // find entry with good size if it exsists
            DataEntry entry;
            do
            {
                if ((entry = buffer.NextEntry()) == null) return books;
            } while (entry.Width > MaxBookSize);

            var book = Instantiate(BookTemplate);
            book.LoadMeta(entry);
            totalWidth += book.Width;

            if (direction == Direction.Left)
            {
                books.AddFirst(book);
            }
            else
            {
                books.AddLast(book);
            }
        }

        // put back offending book
        if (books.Count != 0)
        {
            Destroy(books.Last.Value.gameObject);
            books.RemoveLast();
        }
        return books;
    }

    private void InstantiateTable()
    {
        for (var i = 0; i < _table.Count; i++)
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