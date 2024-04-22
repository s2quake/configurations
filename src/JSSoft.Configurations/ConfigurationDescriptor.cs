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

using System.Diagnostics;
using System.Reflection;

namespace JSSoft.Configurations;

sealed class ConfigurationDescriptor : ConfigurationDescriptorBase
{
    private readonly PropertyInfo _propertyInfo;
    private readonly ConfigurationPropertyAttribute _attribute;

    public ConfigurationDescriptor(PropertyInfo propertyInfo)
    {
        if (propertyInfo.DeclaringType == null)
            throw new ArgumentException($"Property '{nameof(PropertyInfo.DeclaringType)}' of '{nameof(PropertyInfo)}' cannot be null.", nameof(propertyInfo));
        if (propertyInfo.DeclaringType.Namespace == null)
            throw new ArgumentException($"Property '{nameof(PropertyInfo.DeclaringType)}.{nameof(PropertyInfo.DeclaringType.Namespace)}' of '{nameof(PropertyInfo)}' cannot be null.", nameof(propertyInfo));
        if (propertyInfo.GetCustomAttribute<ConfigurationPropertyAttribute>() is null)
            throw new ArgumentException($"'{nameof(PropertyInfo)}' does not have attribute '{nameof(ConfigurationPropertyAttribute)}'.", nameof(propertyInfo));
        if (propertyInfo.CanWrite == false)
            throw new ArgumentException($"Property '{nameof(PropertyInfo.CanWrite)}' of '{nameof(PropertyInfo)}' must be '{true}'.", nameof(propertyInfo));
        if (propertyInfo.CanRead == false)
            throw new ArgumentException($"Property '{nameof(PropertyInfo.CanRead)}' of '{nameof(PropertyInfo)}' must be '{true}'.", nameof(propertyInfo));
        if (ConfigurationUtility.CanSupportType(propertyInfo.PropertyType) == false)
            throw new ArgumentException($"{nameof(PropertyInfo.PropertyType)} '{propertyInfo.PropertyType}' is not supported.", nameof(propertyInfo));

        _propertyInfo = propertyInfo;
        _attribute = _propertyInfo.GetCustomAttribute<ConfigurationPropertyAttribute>()!;
        Name = _attribute.Name != string.Empty ? _attribute.Name : propertyInfo.Name;
        ScopeType = _attribute.ScopeType;
        Category = _propertyInfo.DeclaringType.Namespace!;
        DefaultValue = AttributeUtility.GetDefaultValue(_propertyInfo);
        Description = AttributeUtility.GetDescription(_propertyInfo);
    }

    public override Type PropertyType => _propertyInfo.PropertyType;

    public override object Owner => _propertyInfo;

    public override string Name { get; }

    public override string Category { get; }

    public override string Description { get; }

    public override object? DefaultValue { get; }

    public override Type? ScopeType { get; }

    public override object? GetValue(object obj) => _propertyInfo.GetValue(obj);

    public override void SetValue(object obj, object? value) => _propertyInfo.SetValue(obj, value);

    public override bool ShouldSerializeValue(object value)
    {
        var type = _propertyInfo.PropertyType;
        if (DefaultValue is not DBNull)
        {
            return Equals(DefaultValue, value);
        }
        else if (ConfigurationUtility.TryGetDefaultValue(type, out var defaultValue))
        {
            return Equals(defaultValue, value) == false;
        }
        else if (ConfigurationUtility.IsArrayType(type) == true && value is Array array)
        {
            return array.Length > 0;
        }
        else
        {
            throw new UnreachableException();
        }
    }
}
