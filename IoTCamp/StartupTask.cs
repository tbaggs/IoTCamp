using System;
using System.Text;
using Windows.ApplicationModel.Background;
using Windows.System.Threading;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace IoTCamp
{
    public sealed class StartupTask : IBackgroundTask
    {
        static BackgroundTaskDeferral deferral;
        static ThreadPoolTimer timer;
        static DeviceClient deviceClient;

        static string iotHubUri = "{Place Hub Uri Here";
        static string deviceKey = "{Place IoT Hub Device Key Here}";
        static string deviceId = "{Place Device ID Here};

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            deferral = taskInstance.GetDeferral();

            deviceClient = DeviceClient.Create(iotHubUri, new DeviceAuthenticationWithRegistrySymmetricKey(deviceId, deviceKey), TransportType.Mqtt);

            timer = ThreadPoolTimer.CreatePeriodicTimer(Timer_Tick, TimeSpan.FromSeconds(5));
        }

        ///**********************************************
        //    Placeholder: Timer_Tick Code
        //***********************************************/
        private async void Timer_Tick(ThreadPoolTimer timer)
        {
            await SendDeviceToCloudMessagesAsync();
        }
        

        /**********************************************
        Placeholder: SendDeviceToCloudMessageAsnyc
        ***********************************************/
        private static async Task SendDeviceToCloudMessagesAsync()
        {
            double minTemperature = 20;
            double minHumidity = 60;
            Random rand = new Random();

            double currentTemperature = minTemperature + rand.NextDouble() * 15;
            double currentHumidity = minHumidity + rand.NextDouble() * 20;

            var telemetryDataPoint = new
            {
                deviceId = deviceId,
                temperature = currentTemperature,
                humidity = currentHumidity
            };

            string messageString = JsonConvert.SerializeObject(telemetryDataPoint);
            Message message = new Message(Encoding.ASCII.GetBytes(messageString));

            await deviceClient.SendEventAsync(message);

            Console.WriteLine("{0} > Sent message: {1}", DateTime.Now, messageString);
        }

    }
}
