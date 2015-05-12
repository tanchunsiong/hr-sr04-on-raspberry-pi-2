/*
    Copyright(c) Microsoft Open Technologies, Inc. All rights reserved.

    The MIT License(MIT)

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files(the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions :

    The above copyright notice and this permission notice shall be included in
    all copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
    THE SOFTWARE.
*/

using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Devices.Gpio;
using Windows.Media.SpeechSynthesis;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace Blinky
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += Timer_Tick;


            Unloaded += MainPage_Unloaded;

            InitGPIO();
            timer.Start();
        }

        private void InitGPIO()
        {
            var gpio = GpioController.GetDefault();

            // Show an error if there is no GPIO controller
            if (gpio == null)
            {
                pinTrig = null;
                pinEcho = null;
                GpioStatus.Text = "There is no GPIO controller on this device.";
                return;
            }

            pinTrig = gpio.OpenPin(TRIG_PIN, GpioSharingMode.Exclusive);
            pinEcho = gpio.OpenPin(ECHO_PIN, GpioSharingMode.Exclusive);

            // Show an error if the pin wasn't initialized properly
            if (pinTrig == null)
            {
                GpioStatus.Text = "There were problems initializing the GPIO TRIG pin.";
                return;
            }
            // Show an error if the pin wasn't initialized properly
            if (pinEcho == null)
            {
                GpioStatus.Text = "There were problems initializing the GPIO ECHO pin.";
                return;
            }

            pinTrig.SetDriveMode(GpioPinDriveMode.Output);
            pinEcho.SetDriveMode(GpioPinDriveMode.Input);



            GpioStatus.Text = "GPIO pin initialized correctly.";
        }

        private void MainPage_Unloaded(object sender, object args)
        {
            // Cleanup
            pinTrig.Dispose();
            pinEcho.Dispose();
        }

        private void ReadValue()
        {
            pinTrig.Write(GpioPinValue.Low);


            //GpioStatus.Text = "Waiting for sensor to settle";
            System.Threading.Tasks.Task.Delay(2000).Wait();
            pinTrig.Write(GpioPinValue.High);
            System.Threading.Tasks.Task.Delay(1000).Wait();
            pinTrig.Write(GpioPinValue.Low);
         
            updateTextBox("Waiting for sensor to settle done");
            while (pinEcho.Read() == GpioPinValue.Low)
            {

                pulsestart = DateTime.Now;
                updateTextBox(pulsestart.ToString());

            }
            while (pinEcho.Read() == GpioPinValue.High)
            {
                pulseend = DateTime.Now;
                pinEcho.ValueChanged -= PinEcho_ValueChanged;
                updateTextBox("value changed to " + pinEcho.Read());
                pulseduration = pulseend.Subtract(pulsestart);
                double distance = pulseduration.Seconds * 17150;


                distance = Math.Round(distance, 2);



                updateTextBox("Distance is " + distance);
                break;
            }

        }

        private void PinEcho_ValueChanged(GpioPin sender, GpioPinValueChangedEventArgs args)
        {
            
        }

        private async void updateTextBox(String text)
        {

            
          await  Dispatcher.RunAsync(CoreDispatcherPriority.Normal,() => { GpioStatus.Text = text; });

               
       
        }

        private void Timer_Tick(object sender, object e)
        {
            ReadValue();
        }

        private void Delay_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (timer == null)
            {
                return;
            }
            if (e.NewValue == Delay.Minimum)
            {
                DelayText.Text = "Stopped";
                timer.Stop();

            }
            else
            {
                DelayText.Text = e.NewValue + "ms";
                timer.Interval = TimeSpan.FromMilliseconds(e.NewValue);
                timer.Start();
            }
        }


        private const int TRIG_PIN = 5;
        private const int ECHO_PIN = 6;
        private GpioPin pinTrig;
        private GpioPin pinEcho;
        private DateTime pulsestart;
        private DateTime pulseend;
        private TimeSpan pulseduration;
        private DispatcherTimer timer;
        private SolidColorBrush redBrush = new SolidColorBrush(Windows.UI.Colors.Red);
        private SolidColorBrush grayBrush = new SolidColorBrush(Windows.UI.Colors.LightGray);
    }
}
