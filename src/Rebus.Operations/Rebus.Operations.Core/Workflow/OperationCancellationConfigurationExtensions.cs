using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Rebus.Bus;
using Rebus.Config;
using Rebus.Pipeline;
using Rebus.Pipeline.Receive;

namespace Dbosoft.Rebus.Operations.Workflow;

public static class OperationCancellationConfigurationExtensions
{
    /// <summary>
    /// Enables automatic handling of cancelled tasks: a task handler that observes
    /// its cancellation token (see <see cref="ITaskMessaging.GetCancellationToken"/>)
    /// and throws <see cref="System.OperationCanceledException"/> is reported as
    /// <see cref="OperationTaskStatus.Cancelled"/> instead of being retried. Without
    /// this, handlers can still observe the token and call
    /// <see cref="ITaskMessaging.CancelTask"/> themselves.
    /// </summary>
    public static OptionsConfigurer EnableOperationCancellation(
        this OptionsConfigurer configurer,
        WorkflowOptions options,
        ITaskCancellationRegistry cancellationRegistry,
        ILogger? logger = null)
    {
        configurer.Decorate<IPipeline>(c =>
        {
            var pipeline = c.Get<IPipeline>();
            var step = new OperationCancellationStep(
                () => c.Get<IBus>(),
                options,
                cancellationRegistry,
                logger ?? NullLogger.Instance);

            return new PipelineStepInjector(pipeline)
                .OnReceive(step, PipelineRelativePosition.Before, typeof(DispatchIncomingMessageStep));
        });

        return configurer;
    }
}
