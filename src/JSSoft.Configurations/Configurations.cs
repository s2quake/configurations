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

using System.Collections;
using System.Reflection;

namespace JSSoft.Configurations;

public class Configurations : IReadOnlyDictionary<string, object>
{
    private readonly ConfigurationsSettings _settings;
    private readonly Dictionary<string, object> _valueByName = [];
    private readonly Dictionary<string, ConfigurationDescriptorBase> _descriptorByName;

    public Configurations(IEnumerable<Type> types)
        : this(types, ConfigurationsSettings.Default)
    {
    }

    public Configurations(IEnumerable<Type> types, ConfigurationsSettings settings)
    {
        _settings = settings;
        Descriptors = new ConfigurationDescriptorCollection(types, settings);
        _descriptorByName = Descriptors.ToDictionary(item => $"{item.Key}", item => item.Value);
    }

    public static Configurations Create(params object[] objects)
    {
        var configurations = new Configurations(objects.Select(item => item.GetType()));
        configurations.Commit(objects);
        return configurations;
    }

    public void Commit(params object[] objects)
    {
        foreach (var item in objects)
        {
            CommitObject(obj: item);
        }
    }

    private void CommitObject(object obj)
    {
        const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        var scopeType = _settings.ScopeType;
        var properties = obj.GetType().GetProperties(bindingFlags);
        var query = from item in properties
                    where item.CanRead == true && item.CanWrite == true
                    let attribute = item.GetCustomAttribute<ConfigurationPropertyAttribute>()
                    where object.Equals(attribute?.ScopeType, scopeType) == true && Descriptors.ContainsKey(item) == true
                    select Descriptors[item];
        var items = query.ToArray();
        foreach (var item in items)
        {
            if (item.GetValue(obj) is object value)
            {
                _valueByName[item.Key] = value;
            }
            else if (_valueByName.ContainsKey(item.Key) == true)
            {
                _valueByName.Remove(item.Key);
            }
        }
    }

    public void Update(params object[] objects)
    {
        foreach (var item in objects)
        {
            UpdateObject(obj: item);
        }
    }
    private void UpdateObject(object obj)
    {
        const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        var scopeType = _settings.ScopeType;
        var properties = obj.GetType().GetProperties(bindingFlags);
        var query = from item in properties
                    where item.CanRead == true && item.CanWrite == true
                    let attribute = item.GetCustomAttribute<ConfigurationPropertyAttribute>()
                    where object.Equals(attribute?.ScopeType, scopeType) == true && Descriptors.ContainsKey(item) == true
                    select Descriptors[item];
        var items = query.ToArray();
        foreach (var item in items)
        {
            try
            {
                if (_valueByName.TryGetValue(item.Key, out var value) == true)
                {
                    item.SetValue(obj, value);
                }
            }
            catch
            {
            }
        }
    }

    public void Write(string path, IConfigurationSerializer serializer)
    {
        using var stream = File.OpenWrite(path);
        Write(stream, serializer);
    }

    public void Write(Stream stream, IConfigurationSerializer serializer)
    {
        var query = from item in _valueByName
                    let descriptor = _descriptorByName[item.Key]
                    where descriptor.TestSerializeValue(item.Value) == true
                    select (descriptor, value: item.Value);
        var properties = query.ToDictionary(item => item.descriptor, item => item.value);
        serializer.Serialize(stream, properties);
    }

    public void Read(string path, IConfigurationSerializer serializer)
    {
        using var stream = File.OpenRead(path);
        Read(stream, serializer);
    }

    public void Read(Stream stream, IConfigurationSerializer serializer)
    {
        var properties = serializer.Deserialize(stream, _descriptorByName.Values);
        var query = from item in properties
                    let descriptor = item.Key
                    where item.Value is not DBNull
                    select (key: descriptor.Key, value: item.Value);
        foreach (var (key, value) in query)
        {
            _valueByName[key] = value;
        }
    }

    public int Count => _valueByName.Count;

    public virtual string Name => "configurations";

    public ConfigurationDescriptorCollection Descriptors { get; }

    #region IReadOnlyDictionary

    IEnumerable<string> IReadOnlyDictionary<string, object>.Keys => _valueByName.Keys;

    IEnumerable<object> IReadOnlyDictionary<string, object>.Values => _valueByName.Values;

    object IReadOnlyDictionary<string, object>.this[string key] => _valueByName[key];

    bool IReadOnlyDictionary<string, object>.ContainsKey(string key)
    {
        return _valueByName.ContainsKey(key);
    }

    bool IReadOnlyDictionary<string, object>.TryGetValue(string key, out object value)
    {
        if (_valueByName.TryGetValue(key, out var v) == true)
        {
            value = v;
            return true;
        }
        value = DBNull.Value;
        return false;
    }

    IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
    {
        foreach (var item in _valueByName)
        {
            yield return item;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        foreach (var item in _valueByName)
        {
            yield return item;
        }
    }

    #endregion
}
