namespace DotNet.Testcontainers.Containers
{
  using System.Collections.Generic;
  using System.IO;
  using System.Text;
  using System.Threading;
  using System.Threading.Tasks;
  using DotNet.Testcontainers.Configurations;
  using JetBrains.Annotations;
  using Microsoft.Extensions.Logging;
  using System.Linq;
  using System;

  /// <summary>
  /// This class represents an extended configured Testcontainer for databases.
  /// </summary>
  [PublicAPI]
  public abstract class TestcontainerDatabase : HostedServiceContainer, IDatabaseScript
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="TestcontainerDatabase" /> class.
    /// </summary>
    /// <param name="configuration">The Testcontainers configuration.</param>
    /// <param name="logger">The logger.</param>
    protected TestcontainerDatabase(ITestcontainersConfiguration configuration, ILogger logger)
      : base(configuration, logger)
    {
    }
    /// <summary>
    /// Gets the database connection string.
    /// </summary>
    [PublicAPI]
    public abstract string ConnectionString { get; }

    /// <summary>
    /// Gets or sets the database.
    /// </summary>
    [PublicAPI]
    public virtual string Database { get; set; }

    /// <summary>
    /// Creates a path to a temporary script file.
    /// </summary>
    /// <returns>A path to a temporary script file.</returns>
    [PublicAPI]
    public virtual string GetTempScriptFile()
    {
      return Path.Combine("/tmp/", Path.GetRandomFileName());
    }

    /// <summary>
    /// Executes a bash script in the database container.
    /// </summary>
    /// <param name="scriptContent">The content of the bash script to be executed.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Task that completes when the script has been executed.</returns>
    public virtual async Task<ExecResult> ExecScriptAsync(string scriptContent, CancellationToken ct = default)
    {
      var tempScriptFile = this.GetTempScriptFile();

      await this.CopyFileAsync(tempScriptFile, Encoding.Default.GetBytes(scriptContent), 493 /* 755 */, 0, 0, ct)
        .ConfigureAwait(false);

      return await this.ExecAsync(new[] { "/bin/sh", "-c", tempScriptFile }, ct)
        .ConfigureAwait(false);
    }
  }

  /// <summary>
  /// This class represents an extended configured Testcontainer for databases.
  /// </summary>
  [PublicAPI]
  public abstract class TestcontainerDatabaseWithConnectionString : TestcontainerDatabase
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="TestcontainerDatabase" /> class.
    /// </summary>
    /// <param name="configuration">The Testcontainers configuration.</param>
    /// <param name="logger">The logger.</param>
    protected TestcontainerDatabaseWithConnectionString(ITestcontainersConfiguration configuration, ILogger logger)
      : base(configuration, logger)
    {
    }
    internal virtual Dictionary<string, string> ConnectionsStringSettings {get;set;}
    public void AddConnectionSetting(string key, string value){
      ConnectionsStringSettings.Add(key, value);
    }
    /// <summary>
    /// This list is used the define functions, that will be replace ConnectionStringsSettings,
    /// if the user set something that is defined by the container itself.
    /// so we can override things like Hostname or Port always like the container define it.
    /// </summary>
    internal IList<Func<string, string>> ConnectionStringSettingsReplacers = new List<Func<string,string>>();

    internal virtual string BuildConnectionString(){
      return ConnectionsStringSettings.Select(s => {
          // Replace all values, wich are defined by the container
          foreach (var replaceFunction in ConnectionStringSettingsReplacers)
          {
            var replaceValue = replaceFunction(s.Key);
            if(replaceValue != null){
              return new KeyValuePair<string,string>(s.Key, replaceValue);
            }
          }
          return s;
      })
      .Select(d => $"{d.Key}={d.Value}")
      .Aggregate(string.Empty, (currentString, nextString) => $"{currentString};${nextString}");
    }
    /// <summary>
    /// Gets the database connection string.
    /// </summary>
    [PublicAPI]
    public override string ConnectionString => BuildConnectionString();

    /// <summary>
    /// Gets or sets the database.
    /// </summary>
    [PublicAPI]
    public virtual string Database { get; set; }

    /// <summary>
    /// Creates a path to a temporary script file.
    /// </summary>
    /// <returns>A path to a temporary script file.</returns>
    [PublicAPI]
    public virtual string GetTempScriptFile()
    {
      return Path.Combine("/tmp/", Path.GetRandomFileName());
    }

    /// <summary>
    /// Executes a bash script in the database container.
    /// </summary>
    /// <param name="scriptContent">The content of the bash script to be executed.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Task that completes when the script has been executed.</returns>
    public virtual async Task<ExecResult> ExecScriptAsync(string scriptContent, CancellationToken ct = default)
    {
      var tempScriptFile = this.GetTempScriptFile();

      await this.CopyFileAsync(tempScriptFile, Encoding.Default.GetBytes(scriptContent), 493 /* 755 */, 0, 0, ct)
        .ConfigureAwait(false);

      return await this.ExecAsync(new[] { "/bin/sh", "-c", tempScriptFile }, ct)
        .ConfigureAwait(false);
    }
  }
}
