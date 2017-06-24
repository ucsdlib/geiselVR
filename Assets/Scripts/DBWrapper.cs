using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Mono.Data.Sqlite;
using UnityEngine;

public class DBWrapper : MonoBehaviour
{
    private IDbConnection _connection;

    private const string TableName = "testing";

    private bool Connected
    {
        get { return _connection.State == ConnectionState.Open; }
    }

    private void Awake()
    {
        Connect();
        QueryCallNum("VK18 .G5");
        QueryRange("CB161 .W54 1983b", "CB19 .B69 1985", true, true);
    }

    private void Connect()
    {
        var dbUri = "URI=file:" + Application.dataPath + "/call-only.db";
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