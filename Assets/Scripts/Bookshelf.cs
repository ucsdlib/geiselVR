using System.Collections.Generic;

public class Bookshelf
{
    public string Start { get; private set; }
    public string End { get; private set; }
    public LinkedList<LinkedList<Book>> Table { get; private set; }

    private delegate void ShelfAdder(LinkedList<Book> books, Book book);

    private delegate void TableAdder(LinkedList<LinkedList<Book>> table, LinkedList<Book> shelf);

    private ShelfAdder addShelf;
    private TableAdder addTable;

    public void Load(Direction direction)
    {
    }

    private void AddRight<T>(LinkedList<T> list, T item)
    {
        list.AddLast(item);
    }

    private void AddLeft<T>(LinkedList<T> list, T item)
    {
        list.AddFirst(item);
    }
}