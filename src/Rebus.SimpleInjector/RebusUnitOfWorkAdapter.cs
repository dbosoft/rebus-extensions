using System.Diagnostics;
using Rebus.Pipeline;
using SimpleInjector;

namespace Dbosoft.Rebus;

public class RebusUnitOfWorkAdapter
{
    public async Task Dispose(IMessageContext context)
    {
        var scope = context.TransactionContext.Items["SI_scope"] as Scope;
        Debug.Assert(scope != null);

        await scope.GetInstance<IRebusUnitOfWork>().DisposeAsync().ConfigureAwait(false);
    }

    public async Task Commit(IMessageContext context)
    {
        var scope = context.TransactionContext.Items["SI_scope"] as Scope;
        Debug.Assert(scope != null);

        await scope.GetInstance<IRebusUnitOfWork>().Commit().ConfigureAwait(false);
    }
}