// Copyright (c) Microsoft. All rights reserved.

using System;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.Storage;
using System.Threading.Tasks;
namespace Blinky
{
    public sealed partial class MainPage : Page
    {
        private const int LED_PIN = 22;
        private GpioPin pin;
        private GpioPinValue pinValue;
        private DispatcherTimer timer;
        private SolidColorBrush redBrush = new SolidColorBrush(Windows.UI.Colors.Red);
        private SolidColorBrush grayBrush = new SolidColorBrush(Windows.UI.Colors.LightGray);
    
        public MainPage()
        {
            InitializeComponent();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += Timer_Tick;
            InitGPIO();
            if (pin != null)
            {
                timer.Start();
            }

            ReadFile();
        }

        public async void WriteFile(string data)
        {
            try
            {
                // Create sample file; replace if exists.
                Windows.Storage.StorageFolder storageFolder =
                    Windows.Storage.ApplicationData.Current.LocalFolder;
                Windows.Storage.StorageFile sampleFile =
                     await storageFolder.CreateFileAsync("Settings.xml",
                        Windows.Storage.CreationCollisionOption.ReplaceExisting);

                await Windows.Storage.FileIO.WriteTextAsync(sampleFile, data);
            }
            catch(Exception ex)
            {
                
            }
        }

        public async void ReadFile()
        {
            try
            {
                // Create sample file; replace if exists.
                Windows.Storage.StorageFolder storageFolder =
                    Windows.Storage.ApplicationData.Current.LocalFolder;
                Windows.Storage.StorageFile sampleFile =
                        await storageFolder.GetFileAsync("Settings.xml");
                if(sampleFile == null)
                {
                    return;
                }
                string readed = await Windows.Storage.FileIO.ReadTextAsync(sampleFile);
                Settings set = Settings.Deserialize<Settings>(readed);
                DelayOnText.Text = set.DelayOn;
                DelayOffText.Text = set.DelayOff;
            }
            catch (Exception ex)
            {

            }
        }

        private void InitGPIO()
        {
            var gpio = GpioController.GetDefault();

            // Show an error if there is no GPIO controller
            if (gpio == null)
            {
                pin = null;
                GpioStatus.Text = "There is no GPIO controller on this device.";
                return;
            }

            pin = gpio.OpenPin(LED_PIN);
            pinValue = GpioPinValue.High;
            pin.Write(pinValue);
            pin.SetDriveMode(GpioPinDriveMode.Output);

            GpioStatus.Text = "GPIO pin initialized correctly.";

        }

           private void Timer_Tick(object sender, object e)
        {
            if (pinValue == GpioPinValue.High)
            {
                pinValue = GpioPinValue.Low;
                pin.Write(pinValue);
                LED.Fill = redBrush;
                timer.Interval = TimeSpan.FromSeconds(Convert.ToInt32(DelayOnText.Text));
            }
            else
            {
                pinValue = GpioPinValue.High;
                pin.Write(pinValue);
                LED.Fill = grayBrush;
                timer.Interval = TimeSpan.FromSeconds(Convert.ToInt32(DelayOffText.Text));
                
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Settings set = new Settings();
            set.DelayOn = DelayOnText.Text;
            set.DelayOff = DelayOffText.Text;
            string text = Settings.Serialize(set);
            WriteFile(text);
        }
    }
}
