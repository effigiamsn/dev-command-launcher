using System;
using System.Collections.Generic;
using System.Linq;

namespace DevCommandLauncherApp.Models;

public sealed class LogBuffer
{
    private readonly object _lock = new();
    private readonly Queue<LogEntry> _entries = new();
    private readonly int _maxLines;

    public event Action<LogEntry>? EntryAdded;

    public LogBuffer(int maxLines = 2000)
    {
        _maxLines = Math.Max(100, maxLines);
    }

    public void Append(LogEntry entry)
    {
        lock (_lock)
        {
            _entries.Enqueue(entry);
            while (_entries.Count > _maxLines)
            {
                _entries.Dequeue();
            }
        }

        EntryAdded?.Invoke(entry);
    }

    public IReadOnlyList<LogEntry> Snapshot()
    {
        lock (_lock)
        {
            return _entries.ToList();
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _entries.Clear();
        }
    }
}
