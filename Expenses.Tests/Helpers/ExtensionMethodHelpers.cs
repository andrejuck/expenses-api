using System.Text;
using System.Text.Json;

namespace Expenses.Tests.Helpers;

public static class ExtensionMethodHelpers
{
    #region Extensions
    public static StringContent BuildStringContent(this string content) =>
        new StringContent(content, Encoding.UTF8, "application/json");

    public static T Deserialize<T>(this string stringResult) =>
        JsonSerializer.Deserialize<T>(stringResult, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

    public static string BuildQueryParams<T>(this T model)
    {
        var resultString = string.Empty;
        var properties = typeof(T).GetProperties();

        foreach (var property in properties)
        {
            var propValue = property.GetValue(model);

            if (propValue == null || propValue == default)
                continue;

            resultString += $"{char.ToLowerInvariant(property.Name[0]) + property.Name.Substring(1)}={propValue}&";
        }

        return "?" + resultString;
    }
    #endregion
}