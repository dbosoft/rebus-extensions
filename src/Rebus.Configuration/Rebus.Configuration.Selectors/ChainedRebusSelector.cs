using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Rebus.Config;

namespace Dbosoft.Rebus.Configuration;

[PublicAPI]
public class ChainedRebusSelector<TConfigure>: GenericRebusSelectorBase<TConfigure>
{
    private readonly ILogger _log;
    private readonly IDictionary<string, List<GenericRebusSelectorBase<TConfigure>>> _selectors = new Dictionary<string, List<GenericRebusSelectorBase<TConfigure>>>();


    public ChainedRebusSelector(IConfiguration configuration, ILogger log, IEnumerable<GenericRebusSelectorBase<TConfigure>> selectors): base(configuration, log)
    {
        _log = log;

        var selectorsArray = selectors as GenericRebusSelectorBase<TConfigure>[] ?? selectors.ToArray();

        if (!selectorsArray.Any())
        {
            AcceptedConfigTypes = Array.Empty<string>();
            ConfigurationName = string.Empty;
            return;
        }

        var names = selectorsArray.Select(x => x.ConfigurationName).Distinct().ToArray();
        if (names.Length > 1)
        {
            throw new InvalidOperationException(
                $"All rebus selectors in a chained selector have to use the same configuration name. Found names: {string.Join(',', names)}");

        }

        AcceptedConfigTypes = selectorsArray.SelectMany(x => x.AcceptedConfigTypes).Distinct().ToArray();
        ConfigurationName = names[0];
        foreach (var selector in selectorsArray)
        {
            foreach (var configType in selector.AcceptedConfigTypes)
            {
                List<GenericRebusSelectorBase<TConfigure>> list; 
                if (_selectors.ContainsKey(configType))
                {
                    list = _selectors[configType];
                }
                else
                {
                    list = new List<GenericRebusSelectorBase<TConfigure>>();
                    _selectors.Add(configType, list);
                }

                list.Add(selector);
            }
        }


    }

    protected override void ConfigureByType(string type, StandardConfigurer<TConfigure> configurer)
    {
        if (!_selectors.ContainsKey(type))
            return;

        foreach (var selector in _selectors[type])
        {
            _log.LogTrace("Trying to configure rebus with selector {selectorType}", selector.GetType());
            try
            {
                selector.Configure(configurer);
                _log.LogDebug("Using rebus selector {selectorType}", selector.GetType());
                break;
            }
            catch (Exception ex)
            {
                _log.LogTrace(ex,"selector {selectorType} configuration failed", selector.GetType());

            }
        }
    }

    public override string[] AcceptedConfigTypes { get; }
    public override string ConfigurationName { get; }
}