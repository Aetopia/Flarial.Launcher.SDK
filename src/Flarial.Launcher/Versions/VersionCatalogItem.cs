namespace Flarial.Launcher.Versions;

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Management.Deployment;

/// <summary>
/// Represents an installation item within a version catalog.
/// </summary>
public sealed class VersionCatalogItem
{
    readonly Task Task;

    readonly IAsyncOperationWithProgress<DeploymentResult, DeploymentProgress> Operation;

    internal VersionCatalogItem(IAsyncOperationWithProgress<DeploymentResult, DeploymentProgress> operation, Action<int> action)
    {
        Operation = operation;
        Task = action is null ? operation.AsTask() : operation.AsTask(new Progress<DeploymentProgress>((_) =>
        {
            if (_.state is DeploymentProgressState.Processing) action((int)_.percentage);
        }));
    }
    public TaskAwaiter GetAwaiter() => Task.GetAwaiter();

    /// <summary>
    /// Cancel the underlying package installation.
    /// </summary>
    public void Cancel() { Operation.Cancel(); ((IAsyncResult)Task).AsyncWaitHandle.WaitOne(); }


    /// <summary>
    /// Asynchronously cancel the underlying package installation.
    /// </summary>
    public async Task CancelAsync() => await Task.Run(Cancel);
}