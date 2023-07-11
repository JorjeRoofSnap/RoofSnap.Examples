# Webhook Example

This example demonstrates how to implement a listener for events from the RoofSnap Api.

The example includes a single controlller at `Controllers/RoofSnapOrderController.cs` which contains the bulk of the
code.

This controller demonstrates getting a jwt token for a user created using the RoofSnap web application as well as
getting
a measurement order object using an id provided by a webhook message.

***

## Requirements

In order to run this sample you will need at least version 6.0 of the .NET SDK.

You can download the SDK from https://dotnet.microsoft.com/en-us/download/dotnet/6.0.

## AppSettings

Be sure to set the options in the section titled `RoofSnapClientOptions` within your appsettings json files or user
secrets.

### Acquiring your SubscriptionKey

1. Sign up for a RoofSnap API developer account at https://roofsnap.portal.azure-api.net/.
   Each organization integrating with RoofSnap should share a single developer account
   created here. The signup process will require that a full name be provided for the
   account. You may make this name a point-of-contact for your organization, or fill in your
   organization name. The email address you use to sign up will be used to reset the
   developer account password.
2. Once you have completed the signup process, navigate to the “Products” tab of the
   RoofSnap API portal (https://roofsnap.portal.azure-api.net/products). You should see one
   product “RoofSnap API (Dev/Test)”. Subscribing to this product will grant you access to
   the RoofSnap dev/test environment. A RoofSnap API administrator will need to approve
   your subscription request to finalize your access to the API.
3. After a RoofSnap Admin has approved your subscription request, navigate to your
   developer “Profile” on the RoofSnap API portal:
   Your organization’s assigned API access keys can be viewed here. Both Primary and
   Secondary keys provide equal access to the RoofSnap API. These access keys are
   unique to your organization and should be kept secret. If you need to regenerate your
   API key(s), you may do so through this interface.

### Username and Password

The username and password are simply acquired by creating a new user using the RoofSnap web application.

1. Visit https://app.roofsnap.com/account and click "Users" on the left side of the page.
2. Click the green "Add User" button.
3. Fill in the required details. The email address and password you set here will be what you need to set in the
   configuration
   settings in order to get a valid token.

## Starting the examples
From the solution root run the following commands
```shell
cd src/WebhookListener
dotnet restore
dotnet run
```

Swagger will be available at `/swagger/index.html`

## Testing the controller

### The request

If you would like to test the controller without the overhead of having to create orders post a message like this to
/api/RoofSnapOrder.

Replace {sketchorderid} with the id of an order that you know has already been completed.

Replace {orgid} with your organization's id. You can obtain this from the RoofSnap success department at [success@roofsnap.com](mailto:success@roofsnap.com).

You can optionally set the {officeid}. The RoofSnap Api will set it to an office id if the webhook subscription
is tied to an office otherwise it will be null.

```json
{
    "Subject": "/sketchorders/{sketchorderid:long}",
    "EventType": "SketchOrderCompleted",
    "EventId": "{Guid}",
    "Data": {
        "ResourceId": "{sketchorderid:long}",
        "OrganizationId": "{orgid:long?}",
        "OfficeId": "{officeid:long?}",
        "UserId": null,
        "Platform": "SketchOsDashboard"
    },
    "DataVersion": "1",
    "MetadataVersion": "1"
}
```

### The response

Out of the box the controller method returns a response with an empty body. This is because the service
that will ultimately send the request doesn't need any parameters.

Instead there are a number of log statements within the controller that will log various data points throughout the
process.

The last two log statements demonstrate the sketchorder structure as well as the url for the sketch report pdf.

Logs are displayed

- In Visual Studio
    - In the Debug output window when debugging.
    - In the ASP.NET Core Web Server window.
- In the console window when the app is run with dotnet run.

## Creating the webhook subscription
To create a new webhook subscription create a new post request to ``https://roofsnap.azure-api.net/dev/v1/webhooksubscriptions`` with the following body:
```json
{
   "OrganizationId": "{orgid:long}",
   "OfficeId": "{officeid:long|null}",
   "WebHookEventType": "{eventtype:RoofSnapEventType}",
   "TargetUrl": "{webhookurl}",
   "HttpMethod": "POST"
}
```

Replace {webhookurl} with the url you would like the RoofSnap Api to make a request to whenever {eventtype} is triggered.
Replace {officeid} with null if you would like the subscription to apply to all orders placed by {orgid}

**Note:** Currently the valid WebHookEventTypes are 
- ProjectCreated
- SketchOrderCompleted
- SketchOrderNotCompleted
- ProjectStatusChanged