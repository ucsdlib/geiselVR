using System.Collections.Generic;
using System.Data;
using Mono.Data.Sqlite;
using UnityEngine;

/// <summary>
/// Wraps SQL queries and handles database connection
/// </summary>
public class DbWrapper
{
    public static DbWrapper Instance
    {
        get { return _instance ?? (_instance = new DbWrapper(DataBasePath)); }
    }

    private static DbWrapper _instance;
    private const string DataBasePath = "call-only.db"; // within the Assets folder
    private const string TableName = "testing";

    private IDbConnection _connection;
    private string _dbPath;

    private bool Connected
    {
        get { return _connection.State == ConnectionState.Open; }
    }

    public DbWrapper(string assetsDbPath)
    {
        _dbPath = assetsDbPath;
        Connect();
    }

    private void Connect()
    {
        var dbUri = "URI=file:" + Application.dataPath + "/" + _dbPath;
        _connection = new SqliteConnection(dbUri);
        _connection.Open();
    }

    public void QueryCallNum(string callNum)
    {
        if (!Connected) Connect();

        // Execute query
        var query = string.Format(
            "SELECT * FROM {0} WHERE call == '{1}'", TableName, callNum);
        var command = _connection.CreateCommand();
        command.CommandText = query;
        var reader = command.ExecuteReader();

        // Read results
        reader.Read();
        var entry = new DataEntry();
        entry.Read(reader);

        // output data in some way

        reader.Close();
        command.Dispose();
    }

    /// <summary>
    /// Gets a certain number of data points starting at a call number
    /// </summary>
    /// <param name="results">where to append data points to</param>
    /// <param name="startCallNum">starting call number</param>
    /// <param name="count">how many data points to load</param>
    /// <param name="forward">if true then loading is in increasing call num direction</param>
    public void QueryCount(ref List<DataEntry> results, string startCallNum, int count, bool forward)
    {
        if (!Connected) Connect();

        // construct query
        string op, order;
        if (forward)
        {
            op = ">";
            order = "ASC";
        }
        else
        {
            op = "<";
            order = "DESC";
        }
        var query = string.Format(
            "SELECT * FROM {0} " +
            "WHERE call {1} '{2}' " +
            "ORDER BY call {3} " +
            "LIMIT {4}",
            TableName, op, startCallNum, order, count);

        // execute query
        var command = _connection.CreateCommand();
        command.CommandText = query;
        var reader = command.ExecuteReader();

        // read results
        while (reader.Read())
        {
            var entry = new DataEntry();
            entry.Read(reader);
            results.Add(entry);
        }

        reader.Close();
        command.Dispose();
    }

    public List<DataEntry> QueryRange(ref List<DataEntry> results, string startCallNum, string endCallNum,
        bool startInclusive, bool endInclusive)
    {
        if (!Connected) Connect();

        // construct query
        var lowOp = (startInclusive) ? ">=" : ">";
        var highOp = (endInclusive) ? "<=" : "<";
        var query = string.Format(
            "SELECT * FROM {0} " +
            "WHERE call {1} '{2}' AND call {3} '{4}' " +
            "ORDER BY call",
            TableName, lowOp, startCallNum, highOp, endCallNum);

        // execute query
        var command = _connection.CreateCommand();
        command.CommandText = query;
        var reader = command.ExecuteReader();

        // read results
        while (reader.Read())
        {
            var entry = new DataEntry();
            entry.Read(reader);
            results.Add(entry);
        }

        reader.Close();
        command.Dispose();
        return results;
    }
}

public class DataEntry
{
    public string CallNum;
    public string Title;
    public double Width;

    public void Read(IDataReader reader)
    {
        CallNum = reader.GetString(0);
        Title = reader.GetString(1);
        Width = reader.GetDouble(2);
    }

    public override string ToString()
    {
        return string.Format("Data: {0}|{1}|{2}", CallNum, Title, Width);
    }
}