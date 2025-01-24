using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Expenses.Tests.Helpers;

public static class ExtensionMethodHelpers
{
    public static JsonSerializerOptions JsonOptions => new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    #region Extensions
    public static JsonContent BuildJsonContent<T>(this T content, string contentType = "application/json") where T : class
    {
        var mediaType = new MediaTypeHeaderValue(contentType);
        return JsonContent.Create(content, mediaType: mediaType, options: JsonOptions);
    }

    public static T Deserialize<T>(this string stringResult) =>
        JsonSerializer.Deserialize<T>(stringResult, JsonOptions);

    public static string SerializeToJsonString<T>(this T obj) =>
        JsonSerializer.Serialize(obj);

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