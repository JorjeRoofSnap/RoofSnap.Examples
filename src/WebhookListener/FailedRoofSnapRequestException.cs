using System.Runtime.Serialization;

namespace WebhookListener;

[Serializable]
public class FailedRoofSnapRequestException : Exception
{
    private const string DefaultMessage =
        "Could not fetch a response from RoofSnap's servers. Please double check your configuration and make sure you are using valid credentials.";

    public FailedRoofSnapRequestException(HttpRequestMessage request, HttpResponseMessage? response,
        string? message = DefaultMessage) : base(message)
    {
        RequestUrl = request.RequestUri?.ToString() ?? "Unknown";

        RequestHeaders = request.Headers.ToDictionary(k => k.Key, k => k.Value);

        if (response == null) return;

        ResponseStatusCode = (int)response.StatusCode;

        ResponseHeaders = response.Headers.ToDictionary(k => k.Key, k => k.Value);

        ResponseMessage = response.Content.ReadAsStringAsync().Result;
    }

    protected FailedRoofSnapRequestException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        RequestHeaders = (Dictionary<string, IEnumerable<string>>)info.GetValue(nameof(RequestHeaders),
            typeof(Dictionary<string, IEnumerable<string>>))!;

        RequestUrl = info.GetString(nameof(RequestUrl)) ?? "Unknown";

        ResponseHeaders = (Dictionary<string, IEnumerable<string>>)info.GetValue(nameof(ResponseHeaders),
            typeof(Dictionary<string, IEnumerable<string>>))!;

        ResponseMessage = info.GetString(nameof(ResponseMessage)) ?? "";

        ResponseStatusCode = info.GetInt32(nameof(ResponseStatusCode));
    }

    public Dictionary<string, IEnumerable<string>> RequestHeaders { get; private set; }
    public string RequestUrl { get; private set; }

    public Dictionary<string, IEnumerable<string>>? ResponseHeaders { get; private set; }

    public string? ResponseMessage { get; private set; }

    public int ResponseStatusCode { get; private set; }

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);

        info.AddValue(nameof(RequestHeaders), RequestHeaders);
        info.AddValue(nameof(RequestUrl), RequestUrl);
        info.AddValue(nameof(ResponseHeaders), ResponseHeaders);
        info.AddValue(nameof(ResponseMessage), ResponseMessage);
        info.AddValue(nameof(ResponseStatusCode), ResponseStatusCode);
    }
}