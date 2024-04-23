using Beter.TestingTools.Common.Models;
using System.Collections.Concurrent;

namespace Beter.TestingTools.Emulator.Services;

public interface IConnectionManager
{
    bool IsActive(string connectionId);
    IEnumerable<FeedConnection> GetAll();
    void Set(FeedConnection connectionId);
    void Remove(string connectionId);
}

public class ConnectionManager : IConnectionManager
{
    private readonly ConcurrentDictionary<string, FeedConnection> _connections = new();

    public IEnumerable<FeedConnection> GetAll() => _connections.Values.ToList();

    public bool IsActive(string connectionId) => _connections.ContainsKey(connectionId);

    public void Remove(string connectionId) => _connections.TryRemove(connectionId, out _);

    public void Set(FeedConnection connection) => _connections.TryAdd(connection.Id, connection);
}
