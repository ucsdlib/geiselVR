﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

// ReSharper disable ConvertIfStatementToSwitchStatement

public class BookshelfController : MonoBehaviour
{
    public bool MetaLoaded { get; private set; }

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
    public Text Display;

    private delegate void ShelfAdder(LinkedList<MetaBook> books, MetaBook book);

    private delegate void TableAdder(LinkedList<LinkedList<MetaBook>> table, LinkedList<MetaBook> shelf);

    private string startCallNumber;
    private string endCallNumber;
    private Unit unit;
    private readonly LinkedList<LinkedList<MetaBook>> table = new LinkedList<LinkedList<MetaBook>>();
    private ShelfAdder addShelf;
    private TableAdder addTable;

    private void Awake()
    {
        // Establish connection to Unit
        unit = GetComponent<Unit>();
        if (unit != null)
        {
            unit.UpdateContents += HandleUpdateEvent;
            unit.DoneLoading = false;
        }

        startCallNumber = CallNumber; // DEBUG
        endCallNumber = CallNumber;
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
            center += Offset;

            Vector3 size = new Vector3(ShelfWidth, 0, 0.28f);

            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(center, size);
            Gizmos.matrix = Matrix4x4.identity;
        }
    }

    public IEnumerator HandleUpdateEvent(Unit unit, Direction direction)
    {
        var last = unit.GetComponent<BookshelfController>();
        if (!last) Debug.Log("Could not get last shelf");

        // FIXME The Identity direction does not include the starting book when populating
        DbBuffer buffer;
        switch (direction)
        {
            case Direction.Right:
                addShelf = AddRight;
                addTable = AddRight;
                buffer = new DbBuffer(last.endCallNumber, DbBufferSize, direction);
                break;
            case Direction.Left:
                addShelf = AddLeft;
                addTable = AddLeft;
                buffer = new DbBuffer(last.startCallNumber, DbBufferSize, direction);
                break;
            case Direction.Identity:
                addShelf = AddRight;
                addTable = AddRight;
                buffer = new DbBuffer(last.startCallNumber, DbBufferSize, Direction.Right);
                break;
            default:
                throw new ArgumentOutOfRangeException("direction", direction, message: null);
        }

        MetaLoaded = false;
        var thread = new Thread(o => PopulateTable(buffer, direction));
        thread.Start();
        yield return InstantiateTable();
        Display.text = startCallNumber;
    }

    private void AddRight<T>(LinkedList<T> list, T item)
    {
        list.AddLast(item);
    }

    private void AddLeft<T>(LinkedList<T> list, T item)
    {
        list.AddFirst(item);
    }

    private void PopulateTable(DbBuffer buffer, Direction direction)
    {
        for (var i = 0; i < ShelfCount; i++)
        {
            var books = GenerateShelf(buffer);
            if (books.Count == 0)
            {
                unit.Row.NotifyEnd(direction);
                return;
            }

            addTable(table, books);
        }

        if (table.Count > 0)
        {
            // table should never be empty due to NotifyEnd() above
            startCallNumber = table.First.Value.First.Value.CallNumber;
            endCallNumber = table.Last.Value.Last.Value.CallNumber;
        }

        MetaLoaded = true;
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

            addShelf(books, book);
        }
        return books;
    }

    private IEnumerator InstantiateTable()
    {
        // wait until we have loaded all meta
        while (MetaLoaded == false)
        {
            yield return null;
        }

        var i = 0;
        foreach (var shelf in table)
        {
            var start = Vector3.up * (TopShelfY - i++ * ShelfHeight);
            InstantiateShelf(shelf, start);
        }
        unit.DoneLoading = true;
    }

    private void InstantiateShelf(LinkedList<MetaBook> shelf, Vector3 start)
    {
        var shelfGameObj = new GameObject("Shelf");
        shelfGameObj.transform.parent = transform;
        shelfGameObj.transform.localPosition = start;
        shelfGameObj.transform.rotation = transform.rotation;
        shelfGameObj.transform.Translate(Offset);

        // Instantiate in order
        var current = Vector3.zero;
        foreach (var meta in shelf)
        {
            var book = Instantiate(BookTemplate);
            book.SetMeta(meta);

            book.transform.parent = shelfGameObj.transform;
            book.transform.localPosition = current;
            book.transform.rotation = shelfGameObj.transform.rotation;
            current += book.Width * Vector3.right;

            book.LoadData();
        }
    }
}