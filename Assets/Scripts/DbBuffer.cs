using System.Collections.Generic;
using JetBrains.Annotations;

public class DbBuffer
{
    public bool Forward
    {
        get { return _forward; }
    }

    private readonly DbWrapper _db;
    private readonly int _capacity;
    private readonly bool _forward;
    private string _startCallNum;
    private int _index;
    private List<DataEntry> _buffer;

    public DbBuffer(string startCallNum, int capacity, bool forward)
    {
        _db = DbWrapper.Instance;
        _startCallNum = startCallNum;
        _capacity = capacity;
        _forward = forward;
        _index = 0;
        _buffer = new List<DataEntry>(capacity);
    }

    private bool LoadData()
    {
        _buffer.Clear();
        _index = 0;
        _db.QueryCount(ref _buffer, _startCallNum, _capacity, forward: _forward);
        return _buffer.Count > 0; // check if we added data
    }
    
    [CanBeNull]
    public DataEntry NextEntry()
    {
        if (_buffer.Count == 0 && !LoadData()) return null;

        // load next round of data if needed
        if (_index >= _buffer.Count)
        {
            _startCallNum = _buffer[_buffer.Count - 1].CallNum;
            if (!LoadData()) return null;
        }

        return _buffer[_index++];
    }
}