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

using System.ComponentModel;
using System.Reflection;

namespace JSSoft.Configurations;

internal static class AttributeUtility
{
    public static T? GetCustomAttribute<T>(MemberInfo memberInfo)
        where T : Attribute
    {
        if (Attribute.GetCustomAttribute(memberInfo, typeof(T)) is T attribute)
        {
            return attribute;
        }

        return default;
    }

    public static T? GetCustomAttribute<T>(ParameterInfo parameterInfo)
        where T : Attribute
    {
        if (Attribute.GetCustomAttribute(parameterInfo, typeof(T)) is T attribute)
        {
            return attribute;
        }

        return default;
    }

    public static T? GetCustomAttribute<T>(MemberInfo memberInfo, bool inherit)
        where T : Attribute
    {
        if (Attribute.GetCustomAttribute(memberInfo, typeof(T), inherit) is T attribute)
        {
            return attribute;
        }

        return default;
    }

    public static T[] GetCustomAttributes<T>(MemberInfo memberInfo)
        where T : Attribute
    {
        return Attribute.GetCustomAttributes(memberInfo, typeof(T)).OfType<T>().ToArray();
    }

    public static T[] GetCustomAttributes<T>(MemberInfo memberInfo, bool inherit)
        where T : Attribute
    {
        return Attribute.GetCustomAttributes(memberInfo, typeof(T), inherit).OfType<T>().ToArray();
    }

    public static object? GetDefaultValue(MemberInfo memberInfo)
    {
        if (memberInfo.GetCustomAttribute<DefaultValueAttribute>() is { } defaultValueAttribute)
        {
            return defaultValueAttribute.Value;
        }

        return DBNull.Value;
    }

    public static string GetDeclarationName(Type declaringType)
    {
        if (declaringType.GetCustomAttribute<ConfigurationNameAttribute>() is { } configurationNameAttribute &&
            configurationNameAttribute.Name != string.Empty)
        {
            return configurationNameAttribute.Name;
        }

        return declaringType.Name;
    }

    public static object? GetDefaultValue(Type type)
    {
        if (type.GetCustomAttribute<DefaultValueAttribute>() is { } defaultValueAttribute)
        {
            return defaultValueAttribute.Value;
        }

        return DBNull.Value;
    }

    public static string GetCategory(MemberInfo memberInfo)
    {
        if (memberInfo.GetCustomAttribute<ConfigurationPropertyAttribute>() is { } configurationPropertyAttribute &&
            configurationPropertyAttribute.Category != string.Empty)
        {
            return configurationPropertyAttribute.Category;
        }

        if (memberInfo.GetCustomAttribute<CategoryAttribute>() is { } categoryAttribute &&
           categoryAttribute.Category != string.Empty)
        {
            return categoryAttribute.Category;
        }

        return string.Empty;
    }

    public static string GetDescription(MemberInfo memberInfo)
    {
        if (memberInfo.GetCustomAttribute<DescriptionAttribute>() is { } descriptionAttribute)
        {
            return descriptionAttribute.Description;
        }

        return string.Empty;
    }

    public static string GetDisplayName(MemberInfo memberInfo)
    {
        if (memberInfo.GetCustomAttribute<DisplayNameAttribute>() is { } displayNameAttribute)
        {
            return displayNameAttribute.DisplayName;
        }

        return string.Empty;
    }

    public static bool TryGetDisplayName(MemberInfo memberInfo, out string displayName)
    {
        if (memberInfo.GetCustomAttribute<DisplayNameAttribute>() is { } displayNameAttribute &&
            displayNameAttribute.DisplayName != string.Empty)
        {
            displayName = displayNameAttribute.DisplayName;
            return true;
        }

        displayName = string.Empty;
        return false;
    }

    public static bool GetBrowsable(MemberInfo memberInfo)
    {
        if (memberInfo.GetCustomAttribute<BrowsableAttribute>() is { } browsableAttribute)
        {
            return browsableAttribute.Browsable;
        }

        return true;
    }

    // #if !JSSOFT_COMMANDS
    //     public static int GetOrder(MemberInfo memberInfo)
    //     {
    //         if (Attribute.GetCustomAttribute(memberInfo, typeof(OrderAttribute)) is OrderAttribute orderAttribute)
    //         {
    //             return orderAttribute.Order;
    //         }
    //         return 0;
    //     }
    // #endif // !JSSOFT_COMMANDS
}
