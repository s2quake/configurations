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

public abstract class ConfigurationDescriptorBase
{
    public void Reset() => OnReset();

    public override string ToString()
    {
        return $"{Category}.{Name}";
    }

    public abstract object Owner { get; }

    public abstract Type PropertyType { get; }

    public abstract string Name { get; }

    public abstract string Category { get; }

    public abstract string Description { get; }

    public abstract object? DefaultValue { get; }

    public abstract Type? ScopeType { get; }

    public abstract object? GetValue(object obj);

    public abstract void SetValue(object obj, object? value);

    public abstract bool ShouldSerializeValue(object value);

    internal string Key => $"{Category}.{Name}";

    protected virtual void OnReset()
    {
    }

    internal bool TestSerializeValue(object value) => ShouldSerializeValue(value);
}
