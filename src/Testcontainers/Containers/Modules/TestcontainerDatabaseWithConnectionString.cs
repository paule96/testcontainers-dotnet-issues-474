namespace DotNet.Testcontainers.Containers
{
  using System.Collections.Generic;
  using DotNet.Testcontainers.Configurations;
  using JetBrains.Annotations;
  using Microsoft.Extensions.Logging;
  using System.Linq;
  using System;

  /// <summary>
  /// This class represents an extended configured Testcontainer for databases with connections strings.
  /// </summary>
  [PublicAPI]
  public abstract class TestcontainerDatabaseWithConnectionString : TestcontainerDatabase
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="TestcontainerDatabaseWithConnectionString" /> class.
    /// </summary>
    /// <param name="configuration">The Testcontainers configuration.</param>
    /// <param name="logger">The logger.</param>
    protected TestcontainerDatabaseWithConnectionString(ITestcontainersConfiguration configuration, ILogger logger)
      : base(configuration, logger)
    {
    }
    internal virtual Dictionary<string, string> ConnectionsStringSettings {get;set;} = new Dictionary<string, string>();
    /// <summary>
    /// Adds the provided custom settings to your connectionstring
    /// </summary>
    [PublicAPI]
    public void AddConnectionSetting(string key, string value){
      if(ConnectionsStringSettings.Any(d => d.Key == key)){
        throw new ArgumentException($"You can only define a setting one time. Tryed to add the key {key} multiple times");
      }
      ConnectionsStringSettings.Add(key, value);
    }
    /// <summary>
    /// This list is used the define functions, that will be replace ConnectionStringsSettings,
    /// if the user set something that is defined by the container itself.
    /// so we can override things like Hostname or Port always like the container define it.
    /// </summary>
    internal IList<Func<KeyValuePair<string,string>>> ConnectionStringSettingsReplacers = new List<Func<KeyValuePair<string,string>>>();

    internal virtual string BuildConnectionString(){
      var definedParameters = ConnectionStringSettingsReplacers.Select(d => d()).ToDictionary(d => d.Key, d => d.Value);
      foreach (var userdefinedParameter in ConnectionsStringSettings)
      {
        if(definedParameters.Any(d => d.Key == userdefinedParameter.Key)){
          // do nothing if the container it self is responsible for the setting
          continue;
        }
        definedParameters.Add(userdefinedParameter.Key, userdefinedParameter.Value);
      }
      return definedParameters
      .Select(d => $"{d.Key}={d.Value}")
      .Aggregate(string.Empty, (currentString, nextString) => currentString == string.Empty ? nextString : $"{currentString};{nextString}");
    }
    /// <summary>
    /// Gets the database connection string.
    /// </summary>
    [PublicAPI]
    public override string ConnectionString => BuildConnectionString();
  }
}
