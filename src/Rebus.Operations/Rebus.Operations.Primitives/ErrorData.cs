using JetBrains.Annotations;

namespace Dbosoft.Rebus.Operations;

[PublicAPI]
public class ErrorData
{
    public string? ErrorMessage { get; set; }
    public object? AdditionalData { get; set; }
}