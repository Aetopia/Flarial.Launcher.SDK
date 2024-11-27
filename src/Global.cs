using System.Net.Http;
using Windows.Management.Deployment;

static class Global
{
    internal static readonly HttpClient HttpClient = new();

   internal static readonly PackageManager PackageManager = new();

  internal  static readonly AddPackageOptions AddPackageOptions = new()
    {
        ForceAppShutdown = true,
        ForceUpdateFromAnyVersion = true
    };
}