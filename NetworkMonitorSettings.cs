namespace ReflectionSample;

internal class NetworkMonitorSettings
{
    public string WarningService { get; set; } = string.Empty;
    public string MethodToExecute { get; set; } = string.Empty;

    public Dictionary<string, string> PropertyBag { get; set; } = 
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}