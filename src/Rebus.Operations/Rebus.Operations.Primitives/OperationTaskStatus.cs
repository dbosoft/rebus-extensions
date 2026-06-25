namespace Dbosoft.Rebus.Operations;

public enum OperationTaskStatus
{
    // Explicit values: these are persisted as integers by consumers, so the
    // ordinals must stay stable. Append new members, never reorder.
    Queued = 0,
    Running = 1,
    Failed = 2,
    Completed = 3,
    Cancelled = 4
}