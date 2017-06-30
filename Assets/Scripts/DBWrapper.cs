using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Mono.Data.Sqlite;
using UnityEngine;

public class DBWrapper
{
    public static DBWrapper Instance
    {
        get { return _instance ?? (_instance = new DBWrapper(DataBasePath)); }
    }

    private static DBWrapper _instance;
    private const string DataBasePath = "call-only.db"; // within the Assets folder
    private const string TableName = "testing";
    
    private IDbConnection _connection;
    private string _dbPath;
    
    private bool Connected
    {
        get { return _connection.State == ConnectionState.Open; }
    }

    public DBWrapper(string assetsDbPath)
    {
        _dbPath = assetsDbPath;
    }

    private void Connect()
    {
        var dbUri = "URI=file:" + Application.dataPath + "/" + _dbPath;
        _connection = new SqliteConnection(dbUri);
        _connection.Open();
    }

    private void QueryCallNum(string callNum)
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
        var call = reader.GetString(0);
        var title = reader.GetString(1);
        var width = reader.GetDouble(2);

        reader.Close();
        command.Dispose();
    }

    /// <summary>
    /// Returns the set of books within the call number range
    /// </summary>
    /// <param name="startCallNum">starting call number</param>
    /// <param name="endCallNum">ending call number</param>
    /// <param name="startInclusive">controls whether to include startCallNum if there is a match</param>
    /// <param name="endInclusive">controls whether to include endCallNum if there is a match</param>
    /// <returns>a list of <see cref="DataEntry"/></returns>
    private List<DataEntry> QueryRange(string startCallNum, string endCallNum,
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
        var results = new List<DataEntry>();
        while (reader.Read())
        {
            var call = reader.GetString(0);
            var title = reader.GetString(1);
            var width = reader.GetDouble(2);

            var entry = new DataEntry
            {
                CallNum = call,
                Title = title,
                Width = width
            };
            
            results.Add(entry);
        }

        return results;
    }
}

public class DataEntry
{
    public string CallNum;
    public string Title;
    public double Width;

    public override string ToString()
    {
        return string.Format("Data: {0}|{1}|{2}", CallNum, Title, Width);
    }
}