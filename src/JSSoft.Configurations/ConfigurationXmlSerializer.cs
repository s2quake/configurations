// Released under the MIT License.
// 
// Copyright (c) 2024 Jeesu Choi
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation the
// rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit
// persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the
// Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE
// WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 

using System.Xml;

namespace JSSoft.Configurations;

public class ConfigurationXmlSerializer : IConfigurationSerializer
{
    public const string Namespace = "http://schemas.jssoft.com/configs";
    public const string Configurations = nameof(Configurations);

    private static readonly Dictionary<Type, Func<XmlReader, object>> GetterByType = new()
    {
        { typeof(string), new Func<XmlReader, object>(reader => reader.ReadContentAsString()) },
        { typeof(bool), new Func<XmlReader, object>(reader => reader.ReadContentAsBoolean()) },
        { typeof(int), new Func<XmlReader, object>(reader => reader.ReadContentAsInt()) },
        { typeof(long), new Func<XmlReader, object>(reader => reader.ReadContentAsLong()) },
        { typeof(float), new Func<XmlReader, object>(reader => reader.ReadContentAsFloat()) },
        { typeof(double), new Func<XmlReader, object>(reader => reader.ReadContentAsDouble()) },
        { typeof(decimal), new Func<XmlReader, object>(reader => reader.ReadContentAsDecimal()) },
        { typeof(Guid), new Func<XmlReader, object>(reader => XmlConvert.ToGuid(reader.ReadContentAsString())) },
        { typeof(TimeSpan), new Func<XmlReader, object>(reader => XmlConvert.ToTimeSpan(reader.ReadContentAsString())) },
        {
            typeof(DateTimeOffset),
            new Func<XmlReader, object>(reader => XmlConvert.ToDateTimeOffset(reader.ReadContentAsString()))
        },
        {
            typeof(DateTime),
            new Func<XmlReader, object>(reader =>
            {
                return XmlConvert.ToDateTime(reader.ReadContentAsString(), XmlDateTimeSerializationMode.Utc);
            })
        },
    };

    private static readonly Dictionary<Type, Action<XmlWriter, object>> SetterByType = new()
    {
        { typeof(string), new Action<XmlWriter, object>((writer, value) => writer.WriteValue((string)value)) },
        { typeof(bool), new Action<XmlWriter, object>((writer, value) => writer.WriteValue((bool)value)) },
        { typeof(int), new Action<XmlWriter, object>((writer, value) => writer.WriteValue((int)value)) },
        { typeof(long), new Action<XmlWriter, object>((writer, value) => writer.WriteValue((long)value)) },
        { typeof(float), new Action<XmlWriter, object>((writer, value) => writer.WriteValue((float)value)) },
        { typeof(double), new Action<XmlWriter, object>((writer, value) => writer.WriteValue((double)value)) },
        { typeof(decimal), new Action<XmlWriter, object>((writer, value) => writer.WriteValue((decimal)value)) },
        {
            typeof(Guid),
            new Action<XmlWriter, object>((writer, value) => writer.WriteValue(XmlConvert.ToString((Guid)value)))
        },
        {
            typeof(TimeSpan),
            new Action<XmlWriter, object>((writer, value) => writer.WriteValue(XmlConvert.ToString((TimeSpan)value)))
        },
        {
            typeof(DateTimeOffset),
            new Action<XmlWriter, object>((writer, value) => writer.WriteValue((DateTimeOffset)value))
        },
        { typeof(DateTime), new Action<XmlWriter, object>((writer, value) => writer.WriteValue((DateTime)value)) },
    };

    private static readonly char[] Separator = [' '];

    public string Name => "xml";

    public void Serialize(Stream stream, IReadOnlyDictionary<ConfigurationDescriptorBase, object> properties)
    {
        var settings = new XmlWriterSettings() { Indent = true };
        using var writer = XmlWriter.Create(stream, settings);
        using var scope = new WriteElementScope(writer, Configurations, Namespace);
        WriteGroups(writer, properties);
    }

    public IReadOnlyDictionary<ConfigurationDescriptorBase, object> Deserialize(
        Stream stream, IEnumerable<ConfigurationDescriptorBase> descriptors)
    {
        using var reader = XmlReader.Create(stream);
        var properties = descriptors.ToDictionary(item => item, item => (object)DBNull.Value);
        reader.MoveToContent();
        if (reader.IsEmptyElement == false)
        {
            using var scope = new ReadElementScope(reader);
            ReadGroups(reader, properties);
        }
        else
        {
            reader.Skip();
        }

        return properties;
    }

    private static void WriteGroups(
        XmlWriter writer, IReadOnlyDictionary<ConfigurationDescriptorBase, object> properties)
    {
        var query = from item in properties
                    let descriptor = item.Key
                    where Equals(descriptor.DefaultValue, item.Value) == false
                    group item by descriptor.Category into @group
                    select @group;

        foreach (var item in query)
        {
            using var scope = new WriteElementScope(writer, "Group");
            writer.WriteAttributeString("Name", item.Key);
            WriteGroup(writer, item);
        }
    }

    private static void WriteGroup(
        XmlWriter writer, IEnumerable<KeyValuePair<ConfigurationDescriptorBase, object>> items)
    {
        foreach (var item in items)
        {
            var descriptor = item.Key;
            var value = item.Value;
            using var scope = new WriteElementScope(writer, descriptor.Name);
            WriteField(writer, descriptor.PropertyType, value);
        }
    }

    private static void WriteField(XmlWriter writer, Type type, object value)
    {
        if (SetterByType.TryGetValue(type, out var setter) == true)
        {
            setter(writer, value);
        }
        else if (value is Array array)
        {
            var itemType = type.GetElementType()!;
            var rank = array.Rank;
            var indics = new int[rank];
            var lengths = new int[rank];
            for (var i = 0; i < rank; i++)
            {
                lengths[i] = array.GetLength(i);
            }

            writer.WriteAttributeString("Length", string.Join(" ", lengths));
            writer.WriteAttributeString("Type", itemType.Name);
            WriteArray(writer, array, itemType, indics, 0);
        }
        else
        {
            throw new NotSupportedException($"Not supported type: '{type}'.");
        }
    }

    private static void WriteArray(XmlWriter writer, Array array, Type itemType, int[] indics, int dimension)
    {
        var length = array.GetLength(dimension);
        var rank = array.Rank;

        for (var i = 0; i < length; i++)
        {
            indics[dimension] = i;
            using var scope = new WriteElementScope(writer, $"Item{dimension}");
            if (dimension + 1 < rank)
            {
                WriteArray(writer, array, itemType, indics, dimension + 1);
            }
            else
            {
                var value = array.GetValue(indics)!;
                WriteField(writer, itemType, value);
            }
        }
    }

    private static void ReadGroups(XmlReader reader, IDictionary<ConfigurationDescriptorBase, object> properties)
    {
        reader.MoveToContent();
        while (reader.NodeType == XmlNodeType.Element && reader.Name == "Group")
        {
            if (reader.IsEmptyElement == false)
            {
                using var scope = new ReadElementScope(reader, "Group");
                var groupName = scope.Attributes["Name"];
                ReadGroup(reader, groupName, properties);
            }
            else
            {
                reader.Skip();
            }

            reader.MoveToContent();
        }
    }

    private static void ReadGroup(
        XmlReader reader, string groupName, IDictionary<ConfigurationDescriptorBase, object> properties)
    {
        var descriptorByKey = properties.Keys.ToDictionary(item => item.Key);

        reader.MoveToContent();
        while (reader.NodeType == XmlNodeType.Element)
        {
            if (reader.IsEmptyElement == false)
            {
                var key = $"{groupName}.{reader.Name}";
                var descriptor = descriptorByKey[key];
                var type = descriptor.PropertyType;
                using var scope = new ReadElementScope(reader);
                var attributes = scope.Attributes;
                properties[descriptor] = ReadField(reader, type, attributes);
            }
            else
            {
                reader.Skip();
            }

            reader.MoveToContent();
        }
    }

    private static object ReadField(XmlReader reader, Type type, IReadOnlyDictionary<string, string>? attributes)
    {
        if (GetterByType.TryGetValue(type, out var getter) == true)
        {
            return getter(reader);
        }
        else if (type.IsArray == true)
        {
            var itemType = type.GetElementType()!;
            var length = attributes!["Length"];
            var lengths = length.Split(Separator, StringSplitOptions.RemoveEmptyEntries)
                                .Select(int.Parse)
                                .ToArray();
            var array = Array.CreateInstance(type.GetElementType()!, lengths);
            var indics = new int[array.Rank];
            ReadArray(reader, array, itemType, indics, 0);
            return array;
        }
        else
        {
            throw new NotSupportedException($"Not supported type: '{type}'.");
        }
    }

    private static void ReadArray(XmlReader reader, Array array, Type itemType, int[] indices, int dimension)
    {
        var length = array.GetLength(dimension);
        for (var i = 0; i < length; i++)
        {
            if (reader.IsEmptyElement == false)
            {
                using var scope = new ReadElementScope(reader, $"Item{dimension}");
                indices[dimension] = i;
                if (dimension + 1 < indices.Length)
                {
                    ReadArray(reader, array, itemType, indices, dimension + 1);
                }
                else
                {
                    var attributes = scope.Attributes;
                    var value = ReadField(reader, itemType, attributes);
                    array.SetValue(value, indices);
                }
            }
            else
            {
                reader.Skip();
                reader.MoveToContent();
            }
        }
    }

    private sealed class WriteElementScope : IDisposable
    {
        private readonly XmlWriter _writer;

        public WriteElementScope(XmlWriter writer, string name)
            : this(writer, name, ns: null)
        {
        }

        public WriteElementScope(XmlWriter writer, string name, string? ns)
        {
            _writer = writer;
            _writer.WriteStartElement(name, ns);
        }

        public void Dispose()
        {
            _writer.WriteEndElement();
        }
    }

    private sealed class ReadElementScope : IDisposable
    {
        private readonly XmlReader _reader;
        private readonly string _name;

        public ReadElementScope(XmlReader reader)
            : this(reader, reader.Name)
        {
        }

        public ReadElementScope(XmlReader reader, string name)
        {
            if (reader.NodeType != XmlNodeType.Element)
            {
                var message = $"""
                    Property '{nameof(XmlReader.NodeType)}' of '{nameof(reader)}' must be '{XmlNodeType.Element}'.
                    """;
                throw new ArgumentException(message, nameof(reader));
            }

            if (reader.Name != name)
            {
                var message = $"""
                    Property '{nameof(XmlReader.Name)}' of '{nameof(reader)}' must be '{name}'.
                    """;
                throw new ArgumentException(message, nameof(reader));
            }

            _reader = reader;
            _name = name;
            Attributes = GetAttributes(reader);
            _reader.ReadStartElement(name);
            _reader.MoveToContent();
        }

        public IReadOnlyDictionary<string, string> Attributes { get; }

        public void Dispose()
        {
            if (_reader.NodeType == XmlNodeType.EndElement && _reader.Name == _name)
            {
                _reader.ReadEndElement();
                _reader.MoveToContent();
            }
            else if (_reader.NodeType == XmlNodeType.Element && _reader.IsEmptyElement == true)
            {
                _reader.Skip();
                _reader.MoveToContent();
            }
            else
            {
                var message = $"""
                    Property '{nameof(XmlReader.NodeType)}' of '{nameof(_reader)}' must be '{XmlNodeType.EndElement}'.
                    """;
                System.Diagnostics.Trace.TraceWarning(message);
            }
        }

        private static Dictionary<string, string> GetAttributes(XmlReader reader)
        {
            var attributes = new Dictionary<string, string>(reader.AttributeCount);
            for (var i = 0; i < reader.AttributeCount; i++)
            {
                reader.MoveToAttribute(i);
                attributes.Add(reader.Name, reader.Value);
            }

            return attributes;
        }
    }
}
