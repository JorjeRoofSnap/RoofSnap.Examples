using System.Text.Json.Serialization;

namespace WebhookListener.Models;

/*
 Example JSON
 
 {
    "Subject": "/sketchorders/{measurementorderid:long}",
    "EventType": "SketchOrderCompleted",
    "EventId": "{Guid}",
    "Data": {
        "ResourceId": "{measurementorderid:long}",
        "OrganizationId": {long|null},
        "OfficeId": {long|null},
        "UserId": {long|null},
        "Platform": "SketchOsDashboard"
    },
    "DataVersion": "1",
    "MetadataVersion": "1"
}

*/

public class RoofSnapOrderEvent
{
    public string Subject { get; set; } = null!;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public RoofSnapEventType EventType { get; set; }

    public Guid EventId { get; set; }

    public int DataVersion { get; set; }

    public int MetadataVersion { get; set; }

    public RoofSnapOrderEventData? Data { get; set; }
}

public class RoofSnapOrderEventData
{
    public string ResourceId { get; set; } = null!;

    public long? OrganizationId { get; set; }

    public long? OfficeId { get; set; }

    public int? UserId { get; set; }

    public string? Platform { get; set; }
}

public enum RoofSnapEventType
{
    None,
    ProjectCreated,
    SketchOrderCompleted,
    SketchOrderNotCompleted,
    ProjectUpdated,
    ProjectDeleted,
    UserProfileActivated,
    UserProfileDeactivated,
    UserProfileUpdated,
    UserProfileCreated,
    OrganizationUpdated,
    OrganizationCreated,
    ProjectStatusChanged,
    SketchOrderCreated
}