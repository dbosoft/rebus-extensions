using System;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rebus.Config;
using Rebus.DataBus;
using Rebus.DataBus.FileSystem;
using Rebus.DataBus.InMem;

namespace Dbosoft.Rebus.Configuration;

[PublicAPI]
public class DefaultRebusDataBusSelector : GenericRebusSelectorBase<IDataBusStorage>
{
    private readonly InMemDataStore _dataStore;


    public DefaultRebusDataBusSelector(IConfiguration configuration, InMemDataStore dataStore, 
        ILogger logger):
        base(configuration, logger)
    {
        _dataStore = dataStore;

    }

    public override string[] AcceptedConfigTypes => new[] { "inmemory", "filesystem" };
    public override string ConfigurationName => "databus";

    protected override void ConfigureByType(string busType, StandardConfigurer<IDataBusStorage> configurer)
    {
        switch (busType)
        {
            case "filesystem":
                var path = Configuration[$"{ConfigurationName}:path"];

                if (path == null)
                    throw new InvalidOperationException($"Missing configuration entry for {ConfigurationName}::path.");
                configurer.StoreInFileSystem(path);
                return;

            case "inmemory":
                configurer.StoreInMemory(_dataStore);
                return;

        }
    }

}