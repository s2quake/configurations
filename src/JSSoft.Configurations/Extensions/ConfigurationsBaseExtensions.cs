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

namespace JSSoft.Configurations.Extensions;

public static class ConfigurationsBaseExtensions
{
    public static string GetValue(this ConfigurationsBase @this, string key)
    {
        if (@this.Descriptors.TryGetValue(key, out var descriptor) == true)
        {
            if (@this.TryGetValue(key, out var value) == true)
            {
                return ConfigurationUtility.ConvertToString(descriptor.PropertyType, value);
            }
            else if (ConfigurationUtility.TryGetDefaultValue(descriptor.PropertyType, out var defaultValue) == true)
            {
                return ConfigurationUtility.ConvertToString(descriptor.PropertyType, defaultValue);
            }
            else
            {
                throw new NotSupportedException("The default value is not supported.");
            }
        }
        else
        {
            throw new ArgumentException("The key is not found.", nameof(key));
        }
    }

    public static void SetValue(this ConfigurationsBase @this, string key, string value)
    {
        if (@this.Descriptors.TryGetValue(key, out var descriptor) == true)
        {
            @this[key] = ConfigurationUtility.ConvertFromString(descriptor.PropertyType, value);
        }
        else
        {
            throw new ArgumentException("The key is not found.", nameof(key));
        }
    }
}
