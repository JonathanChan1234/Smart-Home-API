using System.Reflection;

namespace smart_home_server.Utils;

public static class DictionaryHelper
{
    public static T ToObject<T>(this Dictionary<string, object?> dict)
        where T : class, new()
    {
        var obj = new T();
        foreach (var pair in dict)
        {
            var property = obj.GetType().GetProperty(pair.Key.FirstCharToUppercase());
            if (property == null) continue;

            var propertyType = property.PropertyType;
            var targetType = IsNullableType(propertyType) ? Nullable.GetUnderlyingType(propertyType) : propertyType;

            if (targetType == null) throw new Exception("Cannot get closed generic type");

            property.SetValue(obj, Convert.ChangeType(pair.Value, targetType), null);
        }
        return obj;
    }

    private static bool IsNullableType(Type type)
    {
        return type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>));
    }

    public static Dictionary<string, object?> ToDict<T>(
        this T obj,
        BindingFlags bindingAttr = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance
    )
    {
        var properties = obj?.GetType().GetProperties(bindingAttr).ToDictionary(
            propInfo => propInfo.Name.FirstCharToLowerCase(),
            propInfo => propInfo.GetValue(obj, null)
        );
        return properties ?? new Dictionary<string, object?>();
    }

    public static Dictionary<string, object?> UpdateDict<T>(this Dictionary<string, object?> dict, T obj) where T : class, new()
    {
        var properties = obj.GetType().GetProperties();
        foreach (var property in properties)
        {
            var key = property.Name.FirstCharToLowerCase();
            var newValue = property.GetValue(obj, null);
            if (newValue == null) continue;
            if (dict.TryGetValue(key, out var value))
            {
                dict[key] = newValue;
            }
            else
            {
                dict.TryAdd(key, newValue);
            }
        }
        return new Dictionary<string, object?>(dict);
    }

    public static void DictLogger(this Dictionary<string, object?> dict, ILogger logger)
    {
        foreach (var pair in dict)
        {
            logger.LogDebug($"key: {pair.Key}, value: {pair.Value}");
        }
    }
}