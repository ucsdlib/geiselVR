public class Book
{
    public string CallNumber
    {
        get { return callNumber; }
    }

    public string Title
    {
        get { return title; }
    }

    public float Width
    {
        get { return (float) widthDb / 1000 + 0.01f; }
    }

    private readonly string callNumber;
    private readonly string title;
    private readonly double widthDb;

    public Book(DataEntry entry)
    {
        callNumber = entry.CallNum;
        title = entry.Title;
        widthDb = entry.Width;
    }
}