using Dbosoft.Rebus.Operations.Workflow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dbosoft.Rebus.OperationsDB.Tests.Handlers;

public class ParallelSagaData : TaskWorkflowSagaData
{
    public ISet<Guid> ItemIds { get; set; } = new HashSet<Guid>();
}
