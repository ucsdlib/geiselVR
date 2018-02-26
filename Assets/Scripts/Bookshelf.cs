using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// A model class containing a collection of <see cref="Book"/> objects which is able to
/// self-populate given initial conditions. Can be used as the internal data of a
/// <see cref="BookshelfController"/>.
/// </summary>
public class Bookshelf : IUnit
{
    public string Start { get; private set; }
    public string End { get; private set; }
    public LinkedList<LinkedList<Book>> Table { get; private set; }

    private const float MinWidth = 10 / 1000f; // min width of a valid book

    public bool Done
    {
        get { return done; }
    }

    // Delegates used to abstract away book loading direction
    private delegate void ShelfAdder(LinkedList<Book> books, Book book);

    private delegate void TableAdder(LinkedList<LinkedList<Book>> table, LinkedList<Book> shelf);

    private ShelfAdder addShelf;
    private TableAdder addTable;
    private volatile bool done;

    private List<ChainEntry> chain; // these will be loaded as well after Load() is called
    private readonly int shelfCount; // 
    private readonly float shelfWidth;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="start">starting identifier used in loading</param>
    /// <param name="end">ending identifier used in loading</param>
    /// <param name="shelfCount">number of shelves</param>
    /// <param name="shelfWidth">width of each shelf. determines book count</param>
    public Bookshelf(string start, string end, int shelfCount, float shelfWidth)
    {
        Start = start;
        End = end;
        Table = new LinkedList<LinkedList<Book>>();
        chain = new List<ChainEntry>();
        done = true;
        this.shelfCount = shelfCount;
        this.shelfWidth = shelfWidth;
    }

    /// <summary>
    /// Given initial conditions, query the database until this object is filled with
    /// <see cref="Book"/> objects. Any chained <see cref="Bookshelf"/> objects will also have this
    /// function called.
    /// </summary>
    /// <param name="direction">direction to load</param>
    /// <exception cref="ArgumentOutOfRangeException">if direction is invalid</exception>
    public void Load(Direction direction)
    {
        if (!done) return;
        done = false;

        // Load self
        // FIXME The Identity direction does not include the starting book when populating
        DbBuffer buffer;
        switch (direction)
        {
            case Direction.Right:
                addShelf = AddRight;
                addTable = AddRight;
                buffer = new DbBuffer(End, direction);
                break;
            case Direction.Left:
                addShelf = AddLeft;
                addTable = AddLeft;
                buffer = new DbBuffer(Start, direction);
                break;
            case Direction.Identity:
                addShelf = AddRight;
                addTable = AddRight;
                buffer = new DbBuffer(Start, Direction.Right);
                break;
            default:
                throw new ArgumentOutOfRangeException("direction", direction, message: null);
        }

        PopulateTable(buffer);
        done = true;

        // Load chain
        foreach (var entry in chain)
        {
            entry.bookshelf.Start = Start;
            entry.bookshelf.End = End;
            entry.bookshelf.Load(entry.direction);
        }
    }

    // Populate the table using the given database buffer
    private void PopulateTable(DbBuffer buffer)
    {
        for (var i = 0; i < shelfCount; i++)
        {
            var books = GenerateShelf(buffer);
            if (books.Count == 0) break;

            addTable(Table, books);
        }

        if (Table.Count > 0)
        {
            Start = Table.First.Value.First.Value.CallNumber;
            End = Table.Last.Value.Last.Value.CallNumber;
        }
        else
        {
            Start = End = "";
        }
    }

    // Generate one shelf of books using the given database buffer
    private LinkedList<Book> GenerateShelf(DbBuffer buffer)
    {
        var books = new LinkedList<Book>();
        var totalWidth = 0.0f;

        while (true)
        {
            // find entry with good size if it exsists
            DataEntry entry;
            do
            {
                if ((entry = buffer.NextEntry()) == null)
                {
                    Debug.Log("Database buffer return null");
                    return books; // db ran out
                }
            } while (entry.Width > shelfWidth * 100);

            var book = new Book(entry);
            if (book.Width < MinWidth) continue;

            totalWidth += book.Width;
            if (totalWidth >= shelfWidth) break;

            addShelf(books, book);
        }

        return books;
    }

    private void AddRight<T>(LinkedList<T> list, T item)
    {
        list.AddLast(item);
    }

    private void AddLeft<T>(LinkedList<T> list, T item)
    {
        list.AddFirst(item);
    }

    /// <summary>
    /// Given initial conditions, query the database until this object is filled with
    /// <see cref="Book"/> objects.
    /// </summary>
    /// <param name="unit">Unit to use as reference for loading</param>
    /// <param name="direction">Direction in reference to previous unit in which to load</param>
    /// <returns></returns>
    public bool Load(IUnit unit, Direction direction)
    {
        var bookshelf = unit as Bookshelf;
        if (bookshelf == null)
        {
            Debug.LogError("Bookshelf: received IUnit of wrong type on Load");
            return false;
        }

        Start = bookshelf.Start;
        End = bookshelf.End;
        Load(direction);
        return true;
    }

    /// <summary>
    /// Add a unit to this chain. It will be loaded when <see cref="Load()"/> is called on
    /// this object.
    /// </summary>
    /// <param name="unit">unit in question</param>
    /// <param name="direction">parameter to units <see cref="Load()"/> function</param>
    public void Chain(IUnit unit, Direction direction)
    {
        var bookshelf = unit as Bookshelf;
        if (bookshelf == null)
        {
            Debug.LogError("Bookshelf: receveived IUnit of wrong type on Chain");
            return;
        }

        chain.Add(new ChainEntry(bookshelf, direction));
    }

    private class ChainEntry
    {
        // Bookshelf will be loaded in direction
        public Bookshelf bookshelf;
        public Direction direction;

        public ChainEntry(Bookshelf bookshelf, Direction direction)
        {
            this.bookshelf = bookshelf;
            this.direction = direction;
        }
    }
}