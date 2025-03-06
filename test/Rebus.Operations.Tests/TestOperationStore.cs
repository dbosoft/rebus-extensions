using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dbosoft.Rebus.Operations.Tests;

public class TestOperationStore
{
    public ConcurrentDictionary<Guid, TestOperationModel> Operations { get; } = new();

    public ConcurrentDictionary<Guid, ConcurrentQueue<object?>> Progress { get; } = new();

    public ConcurrentDictionary<Guid, TestOperationTaskModel> Tasks { get; }= new();

    public IList<TestOperationModel> AllOperations => Operations.Values.ToList();

    public IList<TestOperationTaskModel> AllTasks => Tasks.Values.ToList();

    public IList<object?> GetAllProgress(Guid operationId) =>
        Progress.TryGetValue(operationId, out var progress) ? progress.ToList() : [];
}