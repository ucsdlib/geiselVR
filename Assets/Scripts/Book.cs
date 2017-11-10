public class Book
{
    public string CallNumber
    {
        get { return _callNumber; }
    }

    public string Title
    {
        get { return _title; }
    }

    public float Width
    {
        get { return (float) _widthDb / 1000 + 0.01f; }
    }

    private readonly string _callNumber;
    private readonly string _title;
    private readonly double _widthDb;

    public Book(DataEntry entry)
    {
        _callNumber = entry.CallNum;
        _title = entry.Title;
        _widthDb = entry.Width;
    }
}