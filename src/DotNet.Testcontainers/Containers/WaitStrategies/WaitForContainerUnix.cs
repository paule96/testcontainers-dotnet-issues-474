namespace DotNet.Testcontainers.Containers.WaitStrategies
{
  using DotNet.Testcontainers.Containers.WaitStrategies.Unix;

  /// <inheritdoc cref="IWaitForContainerOS" />
  internal class WaitForContainerUnix : WaitForContainerOS
  {
    /// <inheritdoc />
    public override IWaitForContainerOS UntilCommandIsCompleted(string command)
    {
      this.AddCustomWaitStrategy(new UntilCommandIsCompleted(command));
      return this;
    }

    /// <inheritdoc />
    public override IWaitForContainerOS UntilPortIsAvailable(int port)
    {
      this.AddCustomWaitStrategy(new UntilPortIsAvailable(port));
      return this;
    }
  }
}
