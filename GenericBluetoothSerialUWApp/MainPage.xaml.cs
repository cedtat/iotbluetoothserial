using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Devices.Enumeration;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Networking.Sockets;
using System.Collections.ObjectModel;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using ninpo.communication;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GenericBluetoothSerialUWApp
{
    /// <summary>
    /// The Main Page for the app
    /// </summary>
    public sealed partial class MainPage : Page
    {
        string Title = "Generic Bluetooth Serial Universal Windows App";
        bool isConnected = false;
        ObservableCollection<PairedDeviceInfo> _pairedDevices;

        public MainPage()
        {
            this.InitializeComponent();
            MyTitle.Text = Title;
            Initialize();
        }

        async void Initialize()
        {
            _pairedDevices = await BluetoothSerial.Instance.InitializeRfcommDeviceService();
            PairedDevices.Source = _pairedDevices;
        }
     
        private async void ConnectDevices_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            PairedDeviceInfo pairedDevice = (PairedDeviceInfo)ConnectDevices.SelectedItem;
            this.TxtBlock_SelectedID.Text = pairedDevice.ID;
            this.textBlockBTName.Text = pairedDevice.Name;

            isConnected = await BluetoothSerial.Instance.ConnectDevice(pairedDevice);

            if (isConnected)
            {
                this.buttonDisconnect.IsEnabled = true;
                this.buttonSend.IsEnabled = true;
                this.buttonStartRecv.IsEnabled = true;
                this.buttonStopRecv.IsEnabled = false;

                string msg = String.Format("Connected to {0}!", pairedDevice.DeviceInfo.Name);
                System.Diagnostics.Debug.WriteLine(msg);
            }
        }

        private async void button_Click(object sender, RoutedEventArgs e)
        {
            //OutBuff = new Windows.Storage.Streams.Buffer(100);
            Button button = (Button)sender;
            if (button != null)
            {
                switch ((string)button.Content)
                {
                    case "Disconnect":
                        BluetoothSerial.Instance.Disconnect();
                        this.textBlockBTName.Text = "";
                        this.TxtBlock_SelectedID.Text = "";
                        this.buttonDisconnect.IsEnabled = false;
                        this.buttonSend.IsEnabled = false;
                        this.buttonStartRecv.IsEnabled = false;
                        this.buttonStopRecv.IsEnabled = false;
                        break;
                    case "Send":
                        //await _socket.OutputStream.WriteAsync(OutBuff);
                        BluetoothSerial.Instance.Send(this.textBoxSendText.Text);
                        this.textBoxSendText.Text = "";
                        break;
                    case "Clear Send":
                        this.textBoxRecvdText.Text = "";
                        break;
                    case "Start Recv":
                        this.buttonStartRecv.IsEnabled = false;
                        this.buttonStopRecv.IsEnabled = true;
                        Listen();
                        break;
                    case "Stop Recv":
                        this.buttonStartRecv.IsEnabled = false;
                        this.buttonStopRecv.IsEnabled = false;
                        BluetoothSerial.Instance.CancelReadTask();
                        break;
                    case "Refresh":
                        PairedDevices.Source = BluetoothSerial.Instance.InitializeRfcommDeviceService();
                        break;
                }
            }
        }

            
        private async void Listen()
        {
            try
            {
                await BluetoothSerial.Instance.Listen(ListenCallback);
            }
            catch (Exception ex)
            {
                this.buttonStopRecv.IsEnabled = false;
                this.buttonStartRecv.IsEnabled = false;
                this.buttonSend.IsEnabled = false;
                this.buttonDisconnect.IsEnabled = false;
                this.textBlockBTName.Text = "";
                this.TxtBlock_SelectedID.Text = "";
                if (ex.GetType().Name == "TaskCanceledException")
                {
                    System.Diagnostics.Debug.WriteLine("Listen: Reading task was cancelled, closing device and cleaning up");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Listen: " + ex.Message);
                }
            }

        }

        private void ListenCallback(string result)
        {
            this.textBoxRecvdText.Text += result;
        }
        
    }
}
