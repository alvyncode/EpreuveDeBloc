namespace EpreuveDeBloc.Services;

using System.Text.Json.Serialization;

public class ApiResponse
{
    [JsonPropertyName("results")]
    public List<ApiUser> Results { get; set; }
}

public class ApiUser
{
    [JsonPropertyName("name")]
    public ApiName Name { get; set; }

    [JsonPropertyName("email")]
    public string Email { get; set; }

    [JsonPropertyName("phone")]
    public string Phone { get; set; }

    [JsonPropertyName("cell")]
    public string Cell { get; set; }
}

public class ApiName
{
    [JsonPropertyName("first")]
    public string First { get; set; }

    [JsonPropertyName("last")]
    public string Last { get; set; }
}