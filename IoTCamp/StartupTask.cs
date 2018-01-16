using System;
using System.Text;
using Windows.ApplicationModel.Background;
using Windows.System.Threading;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using System.Threading.Tasks;
using GrovePi;
using GrovePi.Sensors;

namespace IoTCamp
{
    public sealed class StartupTask : IBackgroundTask
    {
        static BackgroundTaskDeferral deferral;
        static ThreadPoolTimer timer;
        static DeviceClient deviceClient;

        static string iotHubUri = "{Place Hub Uri Here}";
        static string deviceKey = "{Place IoT Hub Device Key Here}";
        static string deviceId = "{Place Device ID Here}";

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
            byte rgbVal;
            double currentTemp;

            //Update the LCD screen
            DeviceFactory.Build.Buzzer(Pin.DigitalPin2).ChangeState(SensorStatus.Off);

            rgbVal = Convert.ToByte(DeviceFactory.Build.RotaryAngleSensor(Pin.AnalogPin2).SensorValue() / 4); 

            currentTemp = DeviceFactory.Build.TemperatureAndHumiditySensor(Pin.AnalogPin1, Model.Dht11).TemperatureInCelsius();
            currentTemp = ConvertTemp.ConvertCelsiusToFahrenheit(currentTemp);

            DeviceFactory.Build.RgbLcdDisplay().SetText("Temp: " + currentTemp.ToString("F") + "     Now:  " + DateTime.Now.ToString("H:mm:ss")).SetBacklightRgb(124, rgbVal, 65);

            //Send telemetry to the cloud
            await SendDeviceToCloudMessagesAsync(currentTemp, 32);
        }
        

        /**********************************************
        Placeholder: SendDeviceToCloudMessageAsnyc
        ***********************************************/
        private static async Task SendDeviceToCloudMessagesAsync(double Temperature, double Humidity)
        {

            var telemetryDataPoint = new
            {
                deviceId = deviceId,
                temperature = Temperature,
                humidity = Humidity
            };

            string messageString = JsonConvert.SerializeObject(telemetryDataPoint);
            Message message = new Message(Encoding.ASCII.GetBytes(messageString));

            await deviceClient.SendEventAsync(message);

            Console.WriteLine("{0} > Sent message: {1}", DateTime.Now, messageString);
        }

    }
}

