using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dbosoft.Rebus.Operations.Events;
using Dbosoft.Rebus.Operations.Workflow;
using Dbosoft.Rebus.OperationsDB.Tests.Commands;
using Rebus.Handlers;
using Rebus.Sagas;

namespace Dbosoft.Rebus.OperationsDB.Tests.Handlers
{
    internal class ParallelSaga(IWorkflow workflowEngine)
        : OperationTaskWorkflowSaga<ParallelSagaCommand, ParallelSagaData>(workflowEngine),
            IHandleMessages<OperationTaskStatusEvent<ParallelSubCommand>>
    {
        protected override void CorrelateMessages(ICorrelationConfig<ParallelSagaData> config)
        {
            base.CorrelateMessages(config);

            config.Correlate<OperationTaskStatusEvent<ParallelSubCommand>>(
                m => m.InitiatingTaskId,
                d => d.SagaTaskId);
        }

        protected override async Task Initiated(ParallelSagaCommand message)
        {
            var itemIds = Enumerable.Range(0, 10).Select(_ => Guid.NewGuid())
                .ToList();

            Data.ItemIds = new HashSet<Guid>(itemIds);

            foreach (var itemId in itemIds)
            {
                await StartNewTask(new ParallelSubCommand()
                {
                    ItemId = itemId,
                }).ConfigureAwait(false);
            }
        }

        public Task Handle(OperationTaskStatusEvent<ParallelSubCommand> message)
        {
            return FailOrRun<ParallelSubCommand, ParallelSubCommandResponse>(message,
                (response) =>
                {
                    Data.ItemIds.Remove(response.ItemId);

                    return Data.ItemIds.Count == 0
                        ? Complete()
                        : Task.CompletedTask;
                });
        }
    }
}
