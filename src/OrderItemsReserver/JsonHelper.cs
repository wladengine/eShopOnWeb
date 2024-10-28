using Microsoft.Azure.Functions.Worker.Http;
using System.Text.Json;

namespace Shop.Functions;

public static class JsonHelper
{
    private static readonly JsonSerializerOptions JsonDeserializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
    };

    internal static async ValueTask<T> DeserializeRequestAsync<T>(HttpRequestData req)
        where T : class
    {
        var requestObject = await JsonSerializer.DeserializeAsync<T>(
                req.Body,
                JsonDeserializerOptions);
        return requestObject;
    }

    internal static async ValueTask SerializeAsync<T>(Stream stream, T data)
        where T : class => await JsonSerializer.SerializeAsync(stream, data, JsonDeserializerOptions);
}
