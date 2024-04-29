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

namespace JSSoft.Configurations;

public abstract class ConfigurationsBase(IEnumerable<Type> types, ConfigurationsSettings settings)
    : IDictionary<string, object>
{
    private readonly ConfigurationsSettings _settings = settings;
    private readonly Dictionary<string, object> _valueByKey = [];
    private readonly Dictionary<string, ConfigurationDescriptorBase> _descriptorByKey
        = types.Select(ConfigurationDescriptorCollection.GetDescriptors)
               .SelectMany(item => item)
               .ToDictionary(item => item.Key);

    protected ConfigurationsBase(IEnumerable<Type> types)
        : this(types, ConfigurationsSettings.Default)
    {
    }

    public int Count => _valueByKey.Count;

    public virtual string Name => "configurations";

    public IReadOnlyDictionary<string, ConfigurationDescriptorBase> Descriptors => _descriptorByKey;

    public ICollection<string> Keys => _valueByKey.Keys;

    public ICollection<object> Values => _valueByKey.Values;

    bool ICollection<KeyValuePair<string, object>>.IsReadOnly => false;

    public object this[string key]
    {
        get => _valueByKey[key];
        set
        {
            if (_descriptorByKey.TryGetValue(key, out var descriptor) == true)
            {
                if (descriptor.PropertyType.IsAssignableFrom(value.GetType()) == true)
                {
                    _valueByKey[key] = value;
                }
                else
                {
                    throw new ArgumentException($"The value type is not matched.: '{key}'.");
                }
            }
            else
            {
                throw new ArgumentException($"The key is not available.: '{key}'.");
            }
        }
    }

    public void Commit(params object[] objects)
    {
        foreach (var item in objects)
        {
            CommitObject(obj: item);
        }
    }

    public void Update(params object[] objects)
    {
        foreach (var item in objects)
        {
            UpdateObject(obj: item);
        }
    }

    public void Write(string path, IConfigurationSerializer serializer)
    {
        using var stream = File.OpenWrite(path);
        Write(stream, serializer);
    }

    public void Write(Stream stream, IConfigurationSerializer serializer)
    {
        var query = from item in _valueByKey
                    let descriptor = _descriptorByKey[item.Key]
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
        var properties = serializer.Deserialize(stream, _descriptorByKey.Values);
        var query = from item in properties
                    let descriptor = item.Key
                    where item.Value is not DBNull
                    select (key: descriptor.Key, value: item.Value);
        foreach (var (key, value) in query)
        {
            _valueByKey[key] = value;
        }
    }

    public bool ContainsKey(string key)
    {
        return _valueByKey.ContainsKey(key);
    }

    public bool TryGetValue(string key, out object value)
    {
        if (_valueByKey.TryGetValue(key, out var v) == true)
        {
            value = v;
            return true;
        }

        value = DBNull.Value;
        return false;
    }

    public void Add(string key, object value)
    {
        if (_valueByKey.ContainsKey(key) == true)
        {
            throw new ArgumentException($"The key is already exists.: '{key}'.");
        }
        else if (_descriptorByKey.TryGetValue(key, out var descriptor) == true)
        {
            if (descriptor.PropertyType.IsAssignableFrom(value.GetType()) == true)
            {
                _valueByKey.Add(key, value);
            }
            else
            {
                throw new ArgumentException($"The value type is not matched.: '{key}'.");
            }
        }
        else
        {
            throw new ArgumentException($"The key is not available.: '{key}'.");
        }
    }

    public bool Remove(string key)
    {
        return _valueByKey.Remove(key);
    }

    void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
    {
        if (_valueByKey is ICollection<KeyValuePair<string, object>> collection)
        {
            collection.Add(item);
        }
        else
        {
            throw new NotSupportedException();
        }
    }

    public void Clear()
    {
        _valueByKey.Clear();
    }

    bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
        => _valueByKey.Contains(item);

    void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
    {
        if (_valueByKey is ICollection<KeyValuePair<string, object>> collection)
        {
            collection.CopyTo(array, arrayIndex);
        }
        else
        {
            throw new NotSupportedException();
        }
    }

    bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
    {
        if (_valueByKey is ICollection<KeyValuePair<string, object>> collection)
        {
            return collection.Remove(item);
        }
        else
        {
            throw new NotSupportedException();
        }
    }

    IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
    {
        foreach (var item in _valueByKey)
        {
            yield return item;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        foreach (var item in _valueByKey)
        {
            yield return item;
        }
    }

    private void CommitObject(object obj)
    {
        var scopeType = _settings.ScopeType;
        var descriptors = ConfigurationDescriptorCollection.GetDescriptors(obj.GetType());
        foreach (var descriptor in descriptors)
        {
            if (descriptor.ScopeType != null && descriptor.ScopeType != scopeType)
            {
                continue;
            }

            if (descriptor.GetValue(obj) is object value)
            {
                _valueByKey[descriptor.Key] = value;
            }
            else if (_valueByKey.ContainsKey(descriptor.Key) == true)
            {
                _valueByKey.Remove(descriptor.Key);
            }
        }
    }

    private void UpdateObject(object obj)
    {
        var scopeType = _settings.ScopeType;
        var descriptors = ConfigurationDescriptorCollection.GetDescriptors(obj.GetType());
        foreach (var descriptor in descriptors)
        {
            if (descriptor.ScopeType != null && descriptor.ScopeType != scopeType)
            {
                continue;
            }

            try
            {
                if (_valueByKey.TryGetValue(descriptor.Key, out var value) == true)
                {
                    descriptor.SetValue(obj, value);
                }
            }
            catch
            {
            }
        }
    }
}
