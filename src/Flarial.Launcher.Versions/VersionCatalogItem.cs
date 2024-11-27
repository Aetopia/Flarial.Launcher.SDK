namespace Flarial.Launcher.Versions;

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Management.Deployment;

public sealed class VersionCatalogItem
{
    readonly Task Task;

    readonly IAsyncOperationWithProgress<DeploymentResult, DeploymentProgress> Operation;

    internal VersionCatalogItem(IAsyncOperationWithProgress<DeploymentResult, DeploymentProgress> _) => Task = (Operation = _).AsTask();

    public TaskAwaiter GetAwaiter() => Task.GetAwaiter();

    public void Cancel() { Operation.Cancel(); ((IAsyncResult)Task).AsyncWaitHandle.WaitOne(); }

    public async Task CancelAsync() => await Task.Run(Cancel);
}