namespace Flarial.Launcher;

using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Xml.Linq;
using System.Net.Http;
using System.Reflection;
using System.Collections;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using Windows.Management.Deployment;

/// <summary>
/// Provides methods to manage Minecraft versions compatible with Flarial Client.
/// </summary>
public sealed class Catalog : IEnumerable<string>
{
    const string Releases = "https://raw.githubusercontent.com/dummydummy123456/BedrockDB/main/releases.json";

    const string Supported = "https://raw.githubusercontent.com/flarialmc/newcdn/main/launcher/Supported.txt";

    const string Store = "https://fe3cr.delivery.mp.microsoft.com/ClientWebService/client.asmx/secured";

    static readonly PackageManager PackageManager = new();

    static string Version(string value)
    {
        var substrings = value.Split('.');
        ushort major = ushort.Parse(substrings[0]), minor, build;

        if (major is 0)
        {
            minor = ushort.Parse(substrings[1].Substring(0, 2));
            build = ushort.Parse(substrings[1].Substring(2));
        }
        else
        {
            minor = ushort.Parse(substrings[1]);
            build = (ushort)(ushort.Parse(substrings[2]) / 100);
        }

        return $"{major}.{minor}.{build}";
    }

    /// <summary>
    /// Asynchronously gets a list of versions.
    /// </summary>
    /// <returns>A list of versions supported by Flarial Client.</returns>
    public static async Task<Catalog> GetAsync()
    {
        Dictionary<string, string> dictionary = [];
        var set = (await Global.HttpClient.GetStringAsync(Supported)).Split('\n').ToHashSet();

        foreach (var item in await Json.ParseAsync(await Global.HttpClient.GetStreamAsync(Releases)))
        {
            var substrings = item.Value.Split(' ');

            var identity = substrings[1].Split('_'); if (identity[2] is not "x64") continue;

            var key = Version(identity[1]);
            if (!set.Contains(key)) continue;

            if (!dictionary.ContainsKey(key)) dictionary.Add(key, substrings[0]);
            else dictionary[key] = substrings[0];
        }

        return new(dictionary);
    }

    public IEnumerator<string> GetEnumerator() => Dictionary.Keys.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    readonly Dictionary<string, string> Dictionary;

    static string Content;

    Catalog(Dictionary<string, string> value) => Dictionary = value;

    static async Task<string> GetExtendedUpdateInfo2()
    {
        if (Content is null)
        {
            using StreamReader reader = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("GetExtendedUpdateInfo2.xml"));
            Content = await reader.ReadToEndAsync();
        }
        return Content;
    }

    static async Task<Uri> UriAsync(string value)
    {
        using StringContent content = new(string.Format(await GetExtendedUpdateInfo2(), value, '1'), Encoding.UTF8, "application/soap+xml");
        using var message = await Global.HttpClient.PostAsync(Store, content); message.EnsureSuccessStatusCode();
        return new(await Task.Run(async () =>
        {
            return XElement.Parse(await message.Content.ReadAsStringAsync()).Descendants().
            FirstOrDefault(_ => _.Value.StartsWith("http://tlu.dl.delivery.mp.microsoft.com", StringComparison.Ordinal)).Value;
        }));
    }

    static readonly AddPackageOptions Options = new()
    {
        ForceAppShutdown = true,
        ForceUpdateFromAnyVersion = true
    };

    const int ERROR_INSTALL_FAILED = unchecked((int)0x80073CF9);

    /// <summary>
    /// Asynchronously starts the installation of a version.
    /// </summary>
    /// <param name="value">The version to be installed.</param>
    /// <param name="action">Callback for installation progress.</param>
    /// <returns>An installation request.</returns>
    public async Task<Request> InstallAsync(string value, Action<int> action = default) =>
    PackageManager.FindPackagesForUser(string.Empty, "Microsoft.MinecraftUWP_8wekyb3d8bbwe").FirstOrDefault()?.IsDevelopmentMode ?? false
    ? throw new Win32Exception(ERROR_INSTALL_FAILED)
    : new(PackageManager.AddPackageByUriAsync(await UriAsync(Dictionary[value]), Options), action);


    /// <summary>
    /// Determines whether the specified version exists.
    /// </summary>
    /// <param name="value">The version to check for.</param>
    /// <returns>If the specified version exists then <c>true</c> else <c>false</c>.</returns>
    public bool Contains(string value) => Dictionary.ContainsKey(value);
}