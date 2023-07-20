using System.Diagnostics;
using System.Reflection;

namespace smart_home_server.Utils;

public static class InheritHelper
{
    public static void FillProperties<T, TBase>(
        this T target,
        TBase baseInstance
    )
    {
        Type t = typeof(T);
        Type tb = typeof(TBase);
        PropertyInfo[] properties = tb.GetProperties();
        foreach (var property in properties)
        {
            if (!property.CanRead || !property.CanWrite) return;
            object? value = property.GetValue(baseInstance, null);
            PropertyInfo? propertyInfo = t.GetProperty(property.Name);
            propertyInfo?.SetValue(target, value, null);
        }
    }
}