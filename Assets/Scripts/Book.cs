public class Book
{
    private const float WidthMult = 2.0f;

    public string Id { get; private set; }
    public string CallNumber { get; private set; }
    public string Title { get; private set; }
    public string Author { get; private set; }
    public float Width
    {
        get { return (float) widthDb * WidthMult / 1000 + 0.01f; }
    }
    public string Genre { get; private set; }
    public string Subject { get; private set; }
    public string Summary { get; private set; }

    private readonly double widthDb;

    public Book(DataEntry entry)
    {
        Id = entry.Id;
        CallNumber = entry.Call;
        Title = entry.Title;
        Author = entry.Author;
        widthDb = entry.Width;
        Genre = entry.Genre;
        Subject = entry.Subject;
        Summary = entry.Summary;
    }

    public Book(
        string id, string callNumber, string title, string author,
        double width, string genre, string subject, string summary)
    {
        Id = id;
        CallNumber = callNumber;
        Title = title;
        Author = author;
        widthDb = width;
        Genre = genre;
        Subject = subject;
        Summary = summary;
    }
}