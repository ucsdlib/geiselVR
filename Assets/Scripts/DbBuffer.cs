using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEditor;

public class DbBuffer
{
    public bool Forward
    {
        get { return _forward; }
    }

    private DbWrapper _db;
    private string _startCallNum;
    private int _capacity;
    private bool _forward;
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

    private bool LoadNextData()
    {
        var initCount = _buffer.Count;
        _db.QueryCount(ref _buffer, _startCallNum, _capacity,
            startInclusive: false, forward: true);
        return _buffer.Count > initCount; // check if we added data
    }

    private bool LoadPrevData()
    {
        var initCount = _buffer.Count;
        _db.QueryCount(ref _buffer, _startCallNum, _capacity,
            startInclusive: false, forward: false);
        return _buffer.Count > initCount; // check if we added data
    }
    
    [CanBeNull]
    private DataEntry NextEntry()
    {
        if (_buffer.Count == 0 && !LoadNextData()) return null;

        // load next round of data if needed
        if (_index >= _buffer.Count)
        {
            _startCallNum = _buffer[_buffer.Count - 1].CallNum;
            if (!LoadNextData()) return null;
        }

        return _buffer[_index++];
    }
    
    [CanBeNull]
    private DataEntry PrevEntry()
    {
        if (_buffer.Count == 0 && !LoadNextData()) return null;

        // load next round of data if needed
        if (_index >= _buffer.Count)
        {
            _startCallNum = _buffer[_buffer.Count - 1].CallNum;
            if (!LoadNextData()) return null;
        }

        return _buffer[_index++];
    }
}