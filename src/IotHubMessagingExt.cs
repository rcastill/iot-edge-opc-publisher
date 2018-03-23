namespace OpcPublisher
{
    using System.Text;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Shared;
    using Microsoft.Azure.Devices.Client;
    using Newtonsoft.Json;
    using static Workarounds.TraceWorkaround;

    public partial class IotHubMessaging
    {
        const string VERSION = "0.1";

        private async Task ContinueInitializing()
        {
            // Called after _iotHubClient is succesfully set
            Trace($"OPC-UA Module v${VERSION}");
            // Get module twin
            var twin = await _iotHubClient.GetTwinAsync();
            await UpdateConfigFromTwin(twin.Properties.Desired);
            // Set callback to update desired properties
            await _iotHubClient.SetDesiredPropertyUpdateCallbackAsync(
                OnUpdateDesiredProperties, null
            );
            // Set input endpoint for writing operations
            await _iotHubClient.SetInputMessageHandlerAsync("writeInput",
                WriteInputHandler, null);
        }

        private async Task OnUpdateDesiredProperties(TwinCollection desiredProperties, object context)
        {
            await UpdateConfigFromTwin(desiredProperties);
        }

        private async Task UpdateConfigFromTwin(TwinCollection desiredProperties)
        {
            if (!await PublisherNodeConfiguration
                .LoadConfigFromDesiredPropertiesAsync(desiredProperties))
            {
                Trace("Could not update configuration from desired properties.");
                Trace($"Loading last saved config: {PublisherNodeConfiguration.PublisherNodeConfigurationFilename}");
                await PublisherNodeConfiguration.ReadConfigAsync();
            }
        }

        private async Task<MessageResponse> WriteInputHandler(Message message, object userContext)
        {
            // Check command-type property
            if (!message.Properties.ContainsKey("command-type"))
            {
                return MessageResponse.Completed;
            }
            if (message.Properties["command-type"] != "opcua-session-write")
            {
                return MessageResponse.Completed;
            }
            // Deserialize payload
            var payload = JsonConvert.DeserializeObject<WriteValuePayload>(
                Encoding.UTF8.GetString(message.GetBytes())
            );
            // Issue command
            try
            {
                await PublisherNodeConfiguration.OpcSessionsListSemaphore
                    .WaitAsync();
                var session = PublisherNodeConfiguration.OpcSessions
                    .Find(s => s.EndpointUri == payload.EndpointUri);
                if (!session.WriteValue(payload))
                {
                    Trace("Could not write value. Message payload:");
                    Trace(JsonConvert.SerializeObject(payload,
                        Formatting.Indented));
                }
            }
            finally
            {
                PublisherNodeConfiguration.OpcSessionsListSemaphore
                    .Release();
            }
            // Done
            return MessageResponse.Completed;
        }
    }
}