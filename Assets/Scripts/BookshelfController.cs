using System;
using System.Collections.Generic;
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

    private delegate void ShelfAdder(LinkedList<MetaBook> books, MetaBook book);

    private delegate void TableAdder(LinkedList<LinkedList<MetaBook>> table, LinkedList<MetaBook> shelf);

    private string _startCallNumber;
    private string _endCallNumber;
    private Unit _unit;
    private readonly LinkedList<LinkedList<MetaBook>> _table = new LinkedList<LinkedList<MetaBook>>();
    private ShelfAdder _addShelf;
    private TableAdder _addTable;


    private void Awake()
    {
        // Establish connection to Unit
        _unit = GetComponent<Unit>();
        if (_unit != null)
        {
            _unit.UpdateContentsDelegate += HandleUpdateEvent;
        }

        _startCallNumber = CallNumber; // DEBUG
    }

    private void OnDrawGizmos()
    {
        // Draw guides if needed
        if (!ShowGuides) return;
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

    public void HandleUpdateEvent(Unit unit, Direction direction)
    {
        var last = unit.GetComponent<BookshelfController>();
        if (!last) Debug.Log("Could not get last shelf");

        // FIXME The Identity direction does not include the starting book when populating
        DbBuffer buffer;
        switch (direction)
        {
            case Direction.Right:
                _addShelf = AddRight;
                _addTable = AddRight;
                buffer = new DbBuffer(last._endCallNumber, DbBufferSize, direction);
                break;
            case Direction.Left:
                _addShelf = AddLeft;
                _addTable = AddLeft;
                buffer = new DbBuffer(last._startCallNumber, DbBufferSize, direction);
                break;
            case Direction.Identity:
                _addShelf = AddRight;
                _addTable = AddRight;
                buffer = new DbBuffer(last._startCallNumber, DbBufferSize, Direction.Right);
                break;
            default:
                throw new ArgumentOutOfRangeException("direction", direction, message: null);
        }

        if (!PopulateTable(buffer)) _unit.Row.NotifyEnd(direction);
        InstantiateTable();

        if (_table.Count > 0)
        {
            // table should never be empty due to NotifyEnd() above
            _startCallNumber = _table.First.Value.First.Value.CallNumber;
            _endCallNumber = _table.Last.Value.Last.Value.CallNumber;
        }
    }

    private void AddRight<T>(LinkedList<T> list, T item)
    {
        list.AddLast(item);
    }

    private void AddLeft<T>(LinkedList<T> list, T item)
    {
        list.AddFirst(item);
    }

    private bool PopulateTable(DbBuffer buffer)
    {
        for (var i = 0; i < ShelfCount; i++)
        {
            var books = GenerateShelf(buffer);
            if (books.Count == 0) return false;

            _addTable(_table, books);
        }
        return true;
    }

    private LinkedList<MetaBook> GenerateShelf(DbBuffer buffer)
    {
        var books = new LinkedList<MetaBook>();
        var totalWidth = 0.0f;

        while (true)
        {
            // find entry with good size if it exsists
            DataEntry entry;
            do
            {
                if ((entry = buffer.NextEntry()) == null) return books;
            } while (entry.Width > MaxBookSize);
            
            var book = new MetaBook(entry);

            totalWidth += book.Width;
            if (totalWidth > ShelfWidth) break;

            _addShelf(books, book);
        }
        return books;
    }

    private void InstantiateTable()
    {
        var i = 0;
        foreach (var shelf in _table)
        {
            var start = Vector3.up * (TopShelfY - i++ * ShelfHeight);
            InstantiateShelf(shelf, start);
        }
    }

    private void InstantiateShelf(LinkedList<MetaBook> shelf, Vector3 start)
    {
        var shelfGameObj = new GameObject("Shelf");
        shelfGameObj.transform.parent = transform;
        shelfGameObj.transform.localPosition = start;
        shelfGameObj.transform.Translate(Offset);

        // Instantiate in order
        var current = Vector3.zero;
        foreach (var meta in shelf)
        {
            var book = Instantiate(BookTemplate);
            book.SetMeta(meta);
            
            book.transform.parent = shelfGameObj.transform;
            book.transform.localPosition = current;
            current += book.Width * Vector3.right;

            book.LoadData();
        }
    }
}