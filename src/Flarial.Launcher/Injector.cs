namespace Flarial.Launcher;

using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Minecraft.UWP;
using static Unmanaged;

/// <summary>
/// Provides methods for injecting dynamic link libraries.
/// </summary>
public static class Injector
{
    static readonly nint lpStartAddress;

    static readonly SecurityIdentifier Identifier = new("S-1-15-2-1");

    static Injector()
    {
        nint hModule = default;
        try
        {
            hModule = LoadLibraryEx("Kernel32.dll", default, LOAD_LIBRARY_SEARCH_SYSTEM32);
            lpStartAddress = GetProcAddress(hModule, "LoadLibraryW");
        }
        finally { FreeLibrary(hModule); }
    }

    internal static void Inject(int processId, string path)
    {
        FileInfo info = new(path = Path.GetFullPath(path));
        var security = info.GetAccessControl();
        security.AddAccessRule(new(Identifier, FileSystemRights.ReadAndExecute, AccessControlType.Allow));
        info.SetAccessControl(security);

        nint hProcess = default, lpBaseAddress = default, hThread = default;
        try
        {
            hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, processId);
            if (hProcess == default) throw new Win32Exception(Marshal.GetLastWin32Error());

            var dwSize = sizeof(char) * (path.Length + 1);
            lpBaseAddress = VirtualAllocEx(hProcess, default, dwSize, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
            if (lpBaseAddress == default) throw new Win32Exception(Marshal.GetLastWin32Error());

            if (!WriteProcessMemory(hProcess, lpBaseAddress, Marshal.StringToHGlobalUni(path), dwSize, default)) throw new Win32Exception(Marshal.GetLastWin32Error());

            hThread = CreateRemoteThread(hProcess, default, default, lpStartAddress, lpBaseAddress, default, default);
            if (hThread == default) throw new Win32Exception(Marshal.GetLastWin32Error());
            WaitForSingleObject(hThread, Timeout.Infinite);
        }
        finally
        {
            VirtualFreeEx(hProcess, lpBaseAddress, default, MEM_RELEASE);
            CloseHandle(hThread);
            CloseHandle(hProcess);
        }
    }

    /// <summary>
    /// Asynchronously inject a dynamic link library into Minecraft.
    /// </summary>
    /// <param name="_">Path to the dynamic link library.</param>
    public static async Task InjectAsync(string _) => await Task.Run(async () => Inject(await Game.Launch(), _));
}