using System;
using System.Collections.Generic;
using NUnit.Framework.Constraints;

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

    private readonly int dbBufferSize;
    private readonly int shelfCount;
    private readonly int shelfWidth;

    public Bookshelf(string start, string end, int dbBufferSize, int shelfCount, int shelfWidth)
    {
        Start = start;
        End = end;
        this.dbBufferSize = dbBufferSize;
        this.shelfCount = shelfCount;
        this.shelfWidth = shelfWidth;
    }

    public void Load(Direction direction)
    {
        // FIXME The Identity direction does not include the starting book when populating
        DbBuffer buffer;
        switch (direction)
        {
            case Direction.Right:
                addShelf = AddRight;
                addTable = AddRight;
                buffer = new DbBuffer(End, dbBufferSize, direction);
                break;
            case Direction.Left:
                addShelf = AddLeft;
                addTable = AddLeft;
                buffer = new DbBuffer(Start, dbBufferSize, direction);
                break;
            case Direction.Identity:
                addShelf = AddRight;
                addTable = AddRight;
                buffer = new DbBuffer(Start, dbBufferSize, Direction.Right);
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
            } while (entry.Width > shelfWidth);

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
        throw new NotImplementedException();
    }

    public void Chain(IUnit unit, Direction direction)
    {
        throw new NotImplementedException();
    }
}