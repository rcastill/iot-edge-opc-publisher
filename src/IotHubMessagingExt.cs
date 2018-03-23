namespace OpcPublisher
{
    using System.Threading.Tasks;
    using Microsoft.Azure.Devices.Shared;
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
    }
}