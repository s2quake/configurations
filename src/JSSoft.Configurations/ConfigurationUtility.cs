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

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Text.RegularExpressions;

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
        { typeof(sbyte), default(sbyte) },
        { typeof(byte), default(byte) },
        { typeof(short), default(short) },
        { typeof(ushort), default(ushort) },
        { typeof(int), default(int) },
        { typeof(uint), default(uint) },
        { typeof(long), default(long) },
        { typeof(ulong), default(ulong) },
        { typeof(BigInteger), default(BigInteger) },
        { typeof(float), default(float) },
        { typeof(double), default(double) },
        { typeof(decimal), default(decimal) },
        { typeof(Guid), Guid.Empty },
        { typeof(TimeSpan), TimeSpan.MinValue },
        { typeof(DateTimeOffset), DateTimeOffset.MinValue },
        { typeof(DateTime), DateTime.MinValue },
    };

    public static readonly Dictionary<Type, Func<object, string>> ConvertToStringByType = new()
    {
        [typeof(string)] = value => value as string ?? throw new InvalidOperationException("Invalid type"),
        [typeof(bool)] = value => $"{value}",
        [typeof(sbyte)] = value => $"{value}",
        [typeof(byte)] = value => $"{value}",
        [typeof(short)] = value => $"{value}",
        [typeof(ushort)] = value => $"{value}",
        [typeof(int)] = value => $"{value}",
        [typeof(uint)] = value => $"{value}",
        [typeof(long)] = value => $"{value}",
        [typeof(ulong)] = value => $"{value}",
        [typeof(BigInteger)] = value => $"{value}",
        [typeof(float)] = value => $"{value:R}",
        [typeof(double)] = value => $"{value:R}",
        [typeof(decimal)] = value => $"{value}",
        [typeof(char)] = value => Regex.Escape($"{value}"),
        [typeof(Guid)] = value => $"{value}",
        [typeof(DateTime)] = value => $"{value:O}",
        [typeof(TimeSpan)] = value => $"{value:c}",
        [typeof(DateTimeOffset)] = value => $"{value:O}",
    };

    public static readonly Dictionary<Type, Func<string, object>> ConvertFromStringByType = new()
    {
        [typeof(string)] = value => value,
        [typeof(bool)] = value => bool.Parse(value),
        [typeof(sbyte)] = value => sbyte.Parse(value),
        [typeof(byte)] = value => byte.Parse(value),
        [typeof(short)] = value => short.Parse(value),
        [typeof(ushort)] = value => ushort.Parse(value),
        [typeof(int)] = value => int.Parse(value),
        [typeof(uint)] = value => uint.Parse(value),
        [typeof(long)] = value => long.Parse(value),
        [typeof(ulong)] = value => ulong.Parse(value),
        [typeof(BigInteger)] = value => BigInteger.Parse(value),
        [typeof(float)] = value => float.Parse(value),
        [typeof(double)] = value => double.Parse(value),
        [typeof(decimal)] = value => decimal.Parse(value),
        [typeof(char)] = value => Regex.Unescape(value)[0],
        [typeof(Guid)] = value => Guid.Parse(value),
        [typeof(DateTime)] = value =>
        {
            return DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal);
        },
        [typeof(TimeSpan)] = value => TimeSpan.Parse(value),
        [typeof(DateTimeOffset)] = value => DateTimeOffset.Parse(value),
    };

    public static bool CanSupportType(Type value)
    {
        if (SupportedTypes.Contains(value) == true)
        {
            return true;
        }
        else if (value.IsArray == true &&
            value.HasElementType == true &&
            CanSupportType(value.GetElementType()!) == true)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public static bool TryGetDefaultValue(Type type, [MaybeNullWhen(false)] out object value)
    {
        if (DefaultValueByType.TryGetValue(type, out value) == true)
        {
            return true;
        }

        value = null;
        return false;
    }

    public static string ConvertToString(Type type, object value)
    {
        if (ConvertToStringByType.TryGetValue(type, out var converter) == true)
        {
            return converter(value);
        }

        throw new NotSupportedException($"The type is not supported: '{type}'.");
    }

    public static object ConvertFromString(Type type, string value)
    {
        if (ConvertFromStringByType.TryGetValue(type, out var converter) == true)
        {
            return converter(value);
        }

        throw new NotSupportedException($"The type is not supported: '{type}'.");
    }

    public static bool IsArrayType(Type type)
    {
        return type.IsArray == true && type.HasElementType == true && CanSupportType(type.GetElementType()!) == true;
    }
}
