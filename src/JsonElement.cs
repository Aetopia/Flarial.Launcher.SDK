using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

readonly struct JsonElement : IEnumerable<JsonElement>
{
    readonly XElement Element; readonly JsonType Type;

    public JsonElement(XElement _) => Type = (Element = _).Attribute("type").Value[0] switch
    {
        'a' => JsonType.Array,
        'o' => JsonType.Object,
        _ => JsonType.Value
    };

    enum JsonType { Value, Array, Object }

    internal readonly JsonElement this[string name] => Type is JsonType.Object ? new(Element.Elements().First(_ => Name(_) == name)) : throw new NotSupportedException();

    internal readonly JsonElement this[int index] => Type is JsonType.Array ? new(Element.Elements().ElementAt(index)) : throw new NotSupportedException();

    public readonly string Value => Type is JsonType.Value ? Element.Value : throw new NotSupportedException();

    public IEnumerator<JsonElement> GetEnumerator() => Element.Elements().Select(_ => new JsonElement(_)).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    internal static JsonElement Parse(Stream stream)
    {
        using var reader = JsonReaderWriterFactory.CreateJsonReader(stream, XmlDictionaryReaderQuotas.Max);
        return new(XElement.Load(reader));
    }

    internal static async Task<JsonElement> ParseAsync(Stream stream) => await Task.Run(() => Parse(stream));

    static string Name(XElement _) => string.IsNullOrEmpty(_.Name.NamespaceName) ? _.Name.LocalName : _.Attribute("item").Value;
}