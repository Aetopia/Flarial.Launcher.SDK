namespace Flarial.Launcher;

using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Management.Deployment;

/// <summary>
/// Represents an installation request for a version.
/// </summary>
public sealed class Request
{
    readonly IAsyncOperationWithProgress<DeploymentResult, DeploymentProgress> Operation;

    readonly Task Task;

    internal Request(IAsyncOperationWithProgress<DeploymentResult, DeploymentProgress> operation, Action<int> action = default)
    {
        Operation = operation;
        Task = action is null ? operation.AsTask() : operation.AsTask(new Progress<DeploymentProgress>(_ =>
        {
            if (_.state is DeploymentProgressState.Processing)
                action((int)_.percentage);
        }));
    }

    public TaskAwaiter GetAwaiter() => Task.GetAwaiter();

    /// <summary>
    /// Cancels the installation request.
    /// </summary>
    public void Cancel()
    {
        Operation.Cancel();
        ((IAsyncResult)Task).AsyncWaitHandle.WaitOne();
    }

    /// <summary>
    ///  Asynchronously cancels the installation request.
    /// </summary>
    public async Task CancelAsync() => await Task.Run(Cancel);
}