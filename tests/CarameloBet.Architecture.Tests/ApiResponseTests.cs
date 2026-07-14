using System.Text.Json;
using CarameloBet.API.Models;

namespace CarameloBet.Architecture.Tests;

public class ApiResponseTests
{
    [Fact]
    public void SuccessResponseOmitsNullErrorProperty()
    {
        var response = ApiResponse<string>.Ok("completed");

        var json = JsonSerializer.Serialize(
            response,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.Contains("\"success\":true", json);
        Assert.Contains("\"data\":\"completed\"", json);
        Assert.DoesNotContain("\"error\"", json);
    }
}
