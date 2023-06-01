using System.Reflection;

namespace CumailNEXT.Components.ChatApp.Schemas;

public class PartialDataclass
{
    private readonly object _anonymousObject;
    private readonly PropertyInfo[] _propertyInfos;

    public PartialDataclass(object anonymousObject)
    {
        _anonymousObject = anonymousObject;
        _propertyInfos = _anonymousObject.GetType().GetProperties(BindingFlags.Public 
                                                                  | BindingFlags.Instance);
    }

    private PropertyInfo? GetField(string fieldName)
    {
        return _propertyInfos.FirstOrDefault(property => property.Name == fieldName);
    }
    protected virtual object? FetchValue(string fieldName)
    {
        var field = GetField(fieldName) ?? throw new KeyNotFoundException();
        return field.GetValue(_anonymousObject);
    }
    public T Fetch<T>(string fieldName) where T : class => FetchValue(fieldName) as T ?? throw new InvalidCastException();
    public T FetchNative<T>(string fieldName) => (T)(FetchValue(fieldName) ?? throw new InvalidCastException());
}

public class DictionaryPartialDataclass : PartialDataclass
{
    private readonly Dictionary<string, object> _dict;

    public DictionaryPartialDataclass(Dictionary<string, object> dict) : base(null!)
    {
        _dict = new(dict);
    }
    protected override object? FetchValue(string fieldName)
        => !_dict.ContainsKey(fieldName) ? null : _dict[fieldName];
}
