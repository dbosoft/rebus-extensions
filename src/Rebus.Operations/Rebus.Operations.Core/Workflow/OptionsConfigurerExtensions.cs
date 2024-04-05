using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Rebus.Config;
using Rebus.Pipeline;
using Rebus.Pipeline.Receive;

namespace Dbosoft.Rebus.Operations.Workflow;

public static class OptionsConfigurerExtensions
{
    /// <summary>
    /// Enforces exclusive access when updating <see cref="IOperation"/>
    /// or <see cref="IOperationTask"/>.
    /// </summary>
    /// <remarks>
    /// It is very much recommended to enable this feature. Otherwise, any implementations
    /// of <see cref="IOperationManager"/> or <see cref="IOperationTaskManager"/> must be able
    /// to handle concurrent updates. There is a good change that not activating this
    /// feature will cause hard to find concurrency bugs.
    /// </remarks>
    public static void EnableOperationExclusiveAccess(
        this OptionsConfigurer optionsConfigurer,
        int numberOfBuckets = 1000)
    {
        optionsConfigurer.Decorate<IPipeline>(c =>
        {
            var pipeline = c.Get<IPipeline>();
            var cancellationToken = c.Get<CancellationToken>();
            var step = new OperationExclusiveAccessStep(numberOfBuckets, cancellationToken);

            // The step must run as early as possible to ensure proper protection.
            // Hence, we attach directly after the message deserialization.
            return new PipelineStepInjector(pipeline)
                .OnReceive(step, PipelineRelativePosition.After, typeof(DeserializeIncomingMessageStep));
        });
    }
}
