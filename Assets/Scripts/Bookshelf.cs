using System.Collections.Generic;

public class Bookshelf
{
    public string Start { get; private set; }
    public string End { get; private set; }
    public LinkedList<LinkedList<Book>> Table { get; private set; }
}