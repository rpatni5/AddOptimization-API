
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;
namespace AddOptimization.Utilities.Common;


public class XElementJsonConverter : JsonConverter<XElement>
{
    public override XElement Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, XElement value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(SaveOptions.DisableFormatting));
    }
}
