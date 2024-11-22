namespace Minecraft.UWP;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.System;
using Windows.System.Diagnostics;

sealed class App
{
    App(AppDiagnosticInfo _) => AppDiagnosticInfo = _;

    readonly AppDiagnosticInfo AppDiagnosticInfo;

    IEnumerable<ProcessDiagnosticInfo> ProcessDiagnosticInfos => AppDiagnosticInfo.GetResourceGroups().SelectMany(_ => _.GetProcessDiagnosticInfos());

    static readonly ApplicationActivationManager ApplicationActivationManager = new();

    static readonly PackageDebugSettings PackageDebugSettings = new();

    static readonly Win32Exception ERROR_INSTALL_PACKAGE_NOT_FOUND = new(unchecked((int)0x80073CF1));

    static readonly Win32Exception ERROR_INSTALL_WRONG_PROCESSOR_ARCHITECTURE = new(unchecked((int)0x80073D10));

    const int AO_NOERRORUI = 0x00000002;

    internal static async Task<App> GetAsync(string packageFamilyName)
    {
        var _ = (await AppDiagnosticInfo.RequestInfoForPackageAsync(packageFamilyName)).FirstOrDefault();
        if (_ is null) throw ERROR_INSTALL_PACKAGE_NOT_FOUND;
        else if (_.AppInfo.Package.Id.Architecture is not ProcessorArchitecture.X64) throw ERROR_INSTALL_WRONG_PROCESSOR_ARCHITECTURE;
        return new(_);
    }

    internal Process Process
    {
        get
        {
            var _ = ProcessDiagnosticInfos.FirstOrDefault(); if (_ is null) return null;
            return Process.GetProcessById((int)_.ProcessId);
        }
    }

    internal bool Running => ProcessDiagnosticInfos.Any();

    internal Package Package => AppDiagnosticInfo.AppInfo.Package;

    internal Process Launch()
    {
        PackageDebugSettings.EnableDebugging(Package.Id.FullName, default, default);
        ApplicationActivationManager.ActivateApplication(AppDiagnosticInfo.AppInfo.AppUserModelId, default, AO_NOERRORUI, out var processId);
        return Process.GetProcessById(processId);
    }

    internal void Terminate() => PackageDebugSettings.TerminateAllProcesses(Package.Id.FullName);
}