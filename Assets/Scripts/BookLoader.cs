
using System.Collections.Generic;

public class BookLoader
{
    private DBWrapper _db;
    private string _startCallNum;
    private int _capacity;
    private List<DataEntry> _bookData;

    public BookLoader(string startCallNum, int capacity)
    {
        _db = DBWrapper.Instance;
        _startCallNum = startCallNum;
        _capacity = capacity;
        _bookData = new List<DataEntry>(capacity);
    }

    private void LoadData()
    {
        
    }
}