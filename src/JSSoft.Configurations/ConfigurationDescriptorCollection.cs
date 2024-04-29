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
using System.Collections.Specialized;
using System.Reflection;

namespace JSSoft.Configurations;

internal sealed class ConfigurationDescriptorCollection : IEnumerable<ConfigurationDescriptorBase>
{
    private static readonly Dictionary<Type, ConfigurationDescriptorCollection> ItemsByType = [];
    private readonly OrderedDictionary _itemByPropertyInfo = [];

    private ConfigurationDescriptorCollection(Type type)
    {
        const BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
        var query = from propertyInfo in type.GetProperties(bindingFlags)
                    let attribute = propertyInfo.GetCustomAttribute<ConfigurationPropertyAttribute>()
                    where attribute != null
                    where propertyInfo.CanRead == true && propertyInfo.CanWrite == true
                    select propertyInfo;
        var items = query.ToArray();

        foreach (var item in items)
        {
            var configurationPropertyDescriptor = new ConfigurationDescriptor(propertyInfo: item);
            _itemByPropertyInfo.Add(item, configurationPropertyDescriptor);
        }
    }

    public int Count => _itemByPropertyInfo.Count;

    public ConfigurationDescriptorBase this[int index]
        => (ConfigurationDescriptorBase)_itemByPropertyInfo[index]!;

    public ConfigurationDescriptorBase this[string name]
        => (ConfigurationDescriptorBase)_itemByPropertyInfo[name]!;

    public static ConfigurationDescriptorCollection GetDescriptors(Type type)
    {
        if (ItemsByType.ContainsKey(type) != true)
        {
            ItemsByType.Add(type, new ConfigurationDescriptorCollection(type));
        }

        return ItemsByType[type];
    }

    public IEnumerator<ConfigurationDescriptorBase> GetEnumerator()
        => _itemByPropertyInfo.Values.OfType<ConfigurationDescriptorBase>().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
        => _itemByPropertyInfo.Values.GetEnumerator();
}
