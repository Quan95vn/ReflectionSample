using System.Reflection;
using Microsoft.Extensions.Configuration;

namespace ReflectionSample;

public static class NetworkMonitor
{
    private static NetworkMonitorSettings _networkMonitorSettings = new NetworkMonitorSettings();
    private static Type _warningServiceType;
    private static MethodInfo? _warningServiceMethod;
    private static List<object> _warningServiceParameterValues = new();
    private static object _warningService;

    public static void Warn()
    {
        if (_warningService == null)
        {
            _warningService = Activator.CreateInstance(_warningServiceType);
        }

        var parameters = new List<object>();
        foreach (var propertyBagItem in _networkMonitorSettings.PropertyBag)
        {
            parameters.Add(propertyBagItem.Value);
        }

        _warningServiceMethod?.Invoke(_warningService, _warningServiceParameterValues.ToArray());
    }
    
    public static void BootstrapFromConfiguration()
    {
        var appSettingsConfig = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", false, true)
            .Build();

        appSettingsConfig.Bind("NetworkMonitorSettings", _networkMonitorSettings);

        // inspect the assembly to check whether the correct types are contained within
        _warningServiceType = Assembly.GetExecutingAssembly()
            .GetType(_networkMonitorSettings.WarningService);
        if (_warningServiceType == null)
        {
            throw new Exception("Configuration is invalid - warning service not found");
        }
        
        // inspect the service for the required method
        _warningServiceMethod = _warningServiceType.GetMethod(_networkMonitorSettings.MethodToExecute);
        if (_warningServiceMethod == null)
        {
            throw new Exception("Configuration is invalid - method to execute on warning service not found");
        }
        
        // check if the parameters match
        foreach (var parameterInfo in _warningServiceMethod.GetParameters())
        {
            if (!_networkMonitorSettings.PropertyBag.TryGetValue(parameterInfo.Name, out string parameterValue))
            {
                throw new Exception($"Configuration is invalid - parameter {parameterInfo.Name} not found.");
            }

            try
            {
                var typedValue = Convert.ChangeType(parameterValue, parameterInfo.ParameterType);
                _warningServiceParameterValues.Add(typedValue);
            }
            catch
            {
                throw new Exception(
                    $"Configuration is invalid - parameter {parameterInfo.Name} cannot be converted to expected type {parameterInfo.ParameterType}");
            }
        }
    }
}