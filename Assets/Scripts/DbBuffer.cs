using System;
using System.Collections.Generic;
using JetBrains.Annotations;

/// <summary>
/// Allows buffered access to a <see cref="DbWrapper"/>
/// </summary>
public class DbBuffer
{
    public bool Forward
    {
        get { return forward; }
    }

    private readonly DbWrapper db;
    private readonly int capacity;
    private readonly bool forward;
    private string startCallNum;
    private int index;
    private List<DataEntry> buffer;

    public DbBuffer(string startCallNum, int capacity, Direction direction)
    {
        switch (direction)
        {
            case Direction.Left:
                forward = false;
                break;
            case Direction.Right:
                forward = true;
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    "direction", direction, message: null);
        }

        db = Manager.DbWrapper;
        this.startCallNum = startCallNum;
        this.capacity = capacity;
        index = 0;
        buffer = new List<DataEntry>(capacity);
    }

    public DbBuffer(string startCallNum, Direction direction) :
        this(startCallNum, Manager.Instance.BufferSize, direction)
    {
    }

    private bool LoadData()
    {
        buffer.Clear();
        index = 0;
        db.QueryCount(ref buffer, startCallNum, capacity, forward: forward);
        return buffer.Count > 0; // check if we added data
    }

    /// <summary>
    /// Get next available data base entry
    /// </summary>
    /// <returns>Next available data base entry</returns>
    [CanBeNull]
    public DataEntry NextEntry()
    {
        if (buffer.Count == 0 && !LoadData()) return null;

        // load next round of data if needed
        if (index >= buffer.Count)
        {
            startCallNum = buffer[buffer.Count - 1].Call;
            if (!LoadData()) return null;
        }

        return buffer[index++];
    }
}