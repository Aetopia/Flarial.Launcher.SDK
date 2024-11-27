namespace Flarial.Launcher;

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Management.Deployment;

/// <summary>
/// Represents a collection of Minecraft versions.
/// </summary>
[Obsolete]
public sealed class VersionEntries : IEnumerable<string>
{
    readonly Dictionary<string, string> _;

    internal VersionEntries(Dictionary<string, string> _) => this._ = _;

    /// <summary>
    /// Obtain a version entry.
    /// </summary>
    /// <param name="_">The version string to look for.</param>
    /// <returns></returns>
    public VersionEntry this[string _] => new() { Version = _, UpdateId = this._[_] };

    /// <summary>
    /// Check if a version exists.
    /// </summary>
    /// <param name="_">The version string to look for.</param>
    /// <returns></returns>
    public bool Contains(string _) => this._.ContainsKey(_);

    public IEnumerator<string> GetEnumerator() => _.Keys.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

/// <summary>
/// Represents a version of Minecraft.
/// </summary>
[Obsolete]
public sealed class VersionEntry
{
    internal string Version, UpdateId;

    const string Store = "https://fe3cr.delivery.mp.microsoft.com/ClientWebService/client.asmx/secured";

    static string _;

    static async Task<string> GetExtendedUpdateInfo2()
    {
        if (_ is null)
        {
            using StreamReader reader = new(Assembly.GetExecutingAssembly().GetManifestResourceStream("GetExtendedUpdateInfo2.xml"));
            _ = await reader.ReadToEndAsync();
        }
        return _;
    }

    public static implicit operator string(VersionEntry _) => _.Version;

    /// <summary>
    /// Asynchronously install a version of Minecraft.
    /// </summary>
    /// <param name="action">Callback for installation progress.</param>
    /// <param name="token">Token for cancelling the installation.</param>
    /// <returns></returns>
    public async Task InstallAsync(Action<int> action = default, CancellationToken token = default)
    {
        using StringContent content = new(string.Format(await GetExtendedUpdateInfo2(), UpdateId, '1'), Encoding.UTF8, "application/soap+xml");
        using var message = await Global.HttpClient.PostAsync(Store, content); message.EnsureSuccessStatusCode();

        var value = XElement.Parse(await message.Content.ReadAsStringAsync()).Descendants().FirstOrDefault(_ => _.Value.StartsWith("http://tlu.dl.delivery.mp.microsoft.com", StringComparison.Ordinal)).Value;

        var operation = Global.PackageManager.AddPackageByUriAsync(new Uri(value), Global.AddPackageOptions);
        try
        {
            if (action is not null) await operation.AsTask(token, new Progress<DeploymentProgress>((_) => { if (_.state is DeploymentProgressState.Processing) action((int)_.percentage); }));
            else await operation.AsTask(token);
        }
        catch (TaskCanceledException) { }
    }
}

/// <summary>
/// Provides methods to manage Minecraft versions compatible with Flarial Client.
/// </summary>
[Obsolete]
public static class VersionManager
{
    const string Releases = "https://raw.githubusercontent.com/dummydummy123456/BedrockDB/main/releases.json";

    const string Supported = "https://raw.githubusercontent.com/flarialmc/newcdn/main/launcher/Supported.txt";

    static string Version(string _)
    {
        var substrings = _.Split('.');
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
    /// Asynchronously gets a list of Minecraft versions supported by Flarial Client.
    /// </summary>
    /// <returns>A list of Minecraft versions supported by Flarial Client.</returns>
    public static async Task<VersionEntries> GetAsync()
    {
        Dictionary<string, string> dictionary = [];
        var set = (await Global.HttpClient.GetStringAsync(Supported)).Split('\n').ToHashSet();

        foreach (var _ in await JsonElement.ParseAsync(await Global.HttpClient.GetStreamAsync(Releases)))
        {
            var substrings = _.Value.Split(' ');

            var identity = substrings[1].Split('_'); if (identity[2] is not "x64") continue;

            var key = Version(identity[1]);
            if (!set.Contains(key)) continue;

            if (!dictionary.ContainsKey(key)) dictionary.Add(key, substrings[0]);
            else dictionary[key] = substrings[0];
        }

        return new(dictionary);
    }
}