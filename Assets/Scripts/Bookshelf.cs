using System;
using System.Collections.Generic;
using UnityEngine;

public class Bookshelf : IUnit
{
    public string Start { get; private set; }
    public string End { get; private set; }
    public LinkedList<LinkedList<Book>> Table { get; private set; }
    public bool Done
    {
        get { return done; }
    }

    private delegate void ShelfAdder(LinkedList<Book> books, Book book);

    private delegate void TableAdder(LinkedList<LinkedList<Book>> table, LinkedList<Book> shelf);

    private ShelfAdder addShelf;
    private TableAdder addTable;
    private volatile bool done;
    private List<Tuple<Bookshelf, Direction>> chain;

    private readonly int shelfCount;
    private readonly float shelfWidth;

    public Bookshelf(string start, string end, int shelfCount, float shelfWidth)
    {
        Start = start;
        End = end;
        Table = new LinkedList<LinkedList<Book>>();
        chain = new List<Tuple<Bookshelf, Direction>>();
        done = true;
        this.shelfCount = shelfCount;
        this.shelfWidth = shelfWidth;
    }

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
    }

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
                if ((entry = buffer.NextEntry()) == null) return books; // db ran out
            } while (entry.Width > shelfWidth * 100);

            var book = new Book(entry);

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

    public void Chain(IUnit unit, Direction direction)
    {
        var bookshelf = unit as Bookshelf;
        if (bookshelf == null)
        {
            Debug.LogError("Bookshelf: receveived IUnit of wrong type on Chain");
            return;
        }
        chain.Add(new Tuple<Bookshelf, Direction>(bookshelf, direction));
    }
}