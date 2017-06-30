using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

public class DbBuffer
{
    private DbWrapper _db;
    private string _startCallNum;
    private int _capacity;
    private int _index;
    private List<DataEntry> _bookData;

    public DbBuffer(string startCallNum, int capacity)
    {
        _db = DbWrapper.Instance;
        _startCallNum = startCallNum;
        _capacity = capacity;
        _index = 0;
        LoadData();
    }

    private void LoadData()
    {
        _db.QueryCount(ref _bookData, _startCallNum, _capacity, false);
        if (_bookData.Count == 0)
            throw new IndexOutOfRangeException("Database query returned 0 entries");
    }

    private Book NextBook()
    {
        if (_bookData.Count == 0) LoadData();
        
        // load more data if needed
        if (_index >= _bookData.Count)
        {
            _startCallNum = _bookData[_bookData.Count - 1].CallNum;
            LoadData();
        }
        
        
    }
}