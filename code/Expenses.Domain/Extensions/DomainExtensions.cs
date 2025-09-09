namespace Expenses.Domain.Extensions;

public static class DomainExtensions
{
    public static T GetPropertyValue<T>(this T entity, string propName)
    {
        var property = typeof(T).GetProperty(propName);
        return (T)property?.GetValue(entity) ?? throw new ArgumentException($"Property '{propName}' not found in {typeof(T).Name}");
    }
}