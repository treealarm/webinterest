// Copyright (c) Microsoft. All rights reserved.

using System;
using Windows.Devices.Gpio;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.Storage;
using System.Threading.Tasks;
using Windows.System;

namespace Blinky
{
    public sealed partial class MainPage : Page
    {
        private const int LED_PIN = 22;
        private const int BUTTON_PIN = 3;
        private GpioPin pin;
        private GpioPinValue pinValue;

        private GpioPin pin_btn;
        private GpioPinValue pinValue_btn;
        private DispatcherTimer timer;
        private DispatcherTimer timer_btn;
        private SolidColorBrush redBrush = new SolidColorBrush(Windows.UI.Colors.Red);
        private SolidColorBrush grayBrush = new SolidColorBrush(Windows.UI.Colors.LightGray);
    
        public MainPage()
        {
            InitializeComponent();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timer.Tick += Timer_Tick;

            timer_btn = new DispatcherTimer();
            timer_btn.Interval = TimeSpan.FromMilliseconds(500);
            timer_btn.Tick += Timer_Tick_btn;

            InitGPIO();
            if (pin != null)
            {
                timer.Start();
            }

            if (pin_btn != null)
            {
                timer_btn.Start();
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
            pinValue = GpioPinValue.Low;
            pin.Write(pinValue);
            pin.SetDriveMode(GpioPinDriveMode.Output);

            pin_btn = gpio.OpenPin(BUTTON_PIN);
            pin_btn.SetDriveMode(GpioPinDriveMode.InputPullUp);
           

            GpioStatus.Text = "GPIO pin initialized correctly.";

        }

        int btn_pressed = 0;
        private void Timer_Tick_btn(object sender, object e)
        {
            pinValue_btn = pin_btn.Read();
            if (pinValue_btn == GpioPinValue.Low)
            {
                pinValue = GpioPinValue.Low;
                SetRelayOn();
                btn_pressed++;
                if(btn_pressed > 20)
                {
                    ShutdownManager.BeginShutdown(ShutdownKind.Shutdown, TimeSpan.FromSeconds(0.5));
                }
            }
            else
            {
                if (btn_pressed > 0)
                {
                    SetRelayOff();
                }
                btn_pressed = 0;
            }
        }

        private void SetRelayOn()
        {
            pinValue = GpioPinValue.Low;
            pin.Write(pinValue);
            LED.Fill = redBrush;
        }
        private void SetRelayOff()
        {
            pinValue = GpioPinValue.High;
            pin.Write(pinValue);
            LED.Fill = grayBrush;
        }
        private void Timer_Tick(object sender, object e)
        {
            if (pinValue == GpioPinValue.High)
            {
                SetRelayOn();
                timer.Interval = TimeSpan.FromSeconds(Convert.ToInt32(DelayOnText.Text));
            }
            else
            {
                SetRelayOff();
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
