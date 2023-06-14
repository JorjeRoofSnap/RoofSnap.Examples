using System.Text.Json.Serialization;

namespace WebhookListener.Models;

public class RoofSnapAuthToken
{
    [JsonPropertyName("access_token")] public string? AccessToken { get; set; }

    [JsonPropertyName("token_type")] public string TokenType { get; set; } = null!;

    [JsonPropertyName("expires_in")] public long UnixTimestampExpiresIn { get; set; }

    [JsonIgnore]
    public DateTime ExpiresIn =>
        new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(UnixTimestampExpiresIn);

    [JsonIgnore] public bool IsValid => !string.IsNullOrEmpty(AccessToken) && ExpiresIn >= DateTime.UtcNow;
}