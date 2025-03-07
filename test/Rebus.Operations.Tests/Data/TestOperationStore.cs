using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dbosoft.Rebus.Operations.Tests.Data;

public class TestOperationStore
{
    public ConcurrentDictionary<Guid, TestOperationModel> Operations { get; } = new();

    public ConcurrentDictionary<Guid, TestProgressModel> Progress { get; } = new();

    public ConcurrentDictionary<Guid, TestOperationTaskModel> Tasks { get; }= new();

    public IList<TestOperationModel> AllOperations => Operations.Values
        .OrderBy(o => o.CreatedAt)
        .ToList();

    public IList<TestProgressModel> AllProgress => Progress.Values
        .OrderBy(p => p.Timestamp)
        .ToList();

    public IList<TestOperationTaskModel> AllTasks => Tasks.Values
        .OrderBy(t => t.CreatedAt)
        .ToList();
}
