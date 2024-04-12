using System;

namespace Dbosoft.Rebus.Operations.Commands
{
    public class CreateNewOperationTaskCommand : IOperationTaskMessage
    {
        // ReSharper disable once UnusedMember.Global
        public CreateNewOperationTaskCommand()
        {
        }

        public CreateNewOperationTaskCommand(string commandType, string commandData, 
            Guid operationId, Guid initiatingTaskId, Guid taskId, DateTimeOffset created)
        {
            CommandType = commandType;
            CommandData = commandData;
            OperationId = operationId;
            InitiatingTaskId = initiatingTaskId;
            TaskId = taskId;
            Created = created;
        }

        public string? CommandData { get; set; }
        public string? CommandType { get; set; }

        public Guid OperationId { get; set; }
        public Guid InitiatingTaskId { get; set; }
        public Guid TaskId { get; set; }
        public DateTimeOffset Created { get; set;  }
    }
}