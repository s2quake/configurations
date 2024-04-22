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

namespace JSSoft.Configurations;

public static class ConfigurationUtility
{
    public static readonly Type[] SupportedTypes =
    [
        typeof(string),
        typeof(bool),
        typeof(int),
        typeof(long),
        typeof(float),
        typeof(double),
        typeof(decimal),
        typeof(Guid),
        typeof(TimeSpan),
        typeof(DateTimeOffset),
        typeof(DateTime),
    ];
    public static readonly IReadOnlyDictionary<Type, object> DefaultValueByType = new Dictionary<Type, object>
    {
        { typeof(string), string.Empty },
        { typeof(bool), false },
        { typeof(int), default(int) },
        { typeof(long), default(long) },
        { typeof(float), default(float) },
        { typeof(double), default(double) },
        { typeof(decimal), default(decimal) },
        { typeof(Guid), Guid.Empty },
        { typeof(TimeSpan), TimeSpan.MinValue },
        { typeof(DateTimeOffset), DateTimeOffset.MinValue },
        { typeof(DateTime), DateTime.MinValue },
    };

    public static bool CanSupportType(Type value)
    {
        if (SupportedTypes.Contains(value) == true)
        {
            return true;
        }
        else if (value.IsArray == true && value.HasElementType == true && CanSupportType(value.GetElementType()!) == true)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool TryGetDefaultValue(Type type, out object? value)
    {
        if (DefaultValueByType.TryGetValue(type, out value) == true)
        {
            return true;
        }
        value = null;
        return false;
    }

    public static bool IsArrayType(Type type)
    {
        return type.IsArray == true && type.HasElementType == true && CanSupportType(type.GetElementType()!) == true;
    }
}
