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

	public string Author 
	{
		get { return author; }
	}

	public string Description
	{
		get { return description; }
	}

    private readonly string callNumber;
    private readonly string title;
    private readonly double widthDb;
	private readonly string author;
	private readonly string description;

    public Book(DataEntry entry)
    {
        callNumber = entry.CallNum;
        title = entry.Title;
        widthDb = entry.Width;
    }

	public Book(string callNumber, string title, double width,
		string author, string description)
	{
		this.callNumber = callNumber;
		this.title = title;
		this.Width = width;
		this.author = author;
		this.description = description;
	}
}