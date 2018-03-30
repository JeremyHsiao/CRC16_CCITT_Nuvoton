using System;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;
using System.Timers;
using MySerialLibrary;

namespace UARTViewer
{
    public partial class UARTViewer : Form
    {
        private MySerial MySerialPort;
        private bool FormIsClosing = false;
        static bool MyUART_Exception_status = false;

        public UARTViewer()
        {
            InitializeComponent();
            FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MyUARTViewer_Closing);

            // Init MySerialPort and add event handler after a non-empty line-input
            MySerialPort = new MySerial();
            MySerialPort.GetLineEvent += LineReceivedHandler;
            //
            // Reference: https://docs.microsoft.com/zh-tw/dotnet/csharp/programming-guide/events/how-to-publish-events-that-conform-to-net-framework-guidelines
            //
        }

        static void MyUARTException(Object sender, EventArgs e)
        {
            MyUART_Exception_status = true;
        }

        static bool TimeOutIndicator = false;

        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            TimeOutIndicator = true;
        }

        private void ClearTimeOutIndicator()
        {
            TimeOutIndicator = false;
        }

        private bool GetTimeOutIndicator()
        {
            return TimeOutIndicator;
        }

        private void Serial_UpdatePortName()
        {
            lstMyComPort.Items.Clear();

            foreach (string comport_s in SerialPort.GetPortNames())
            {
                lstMyComPort.Items.Add(comport_s);
            }

            if (lstMyComPort.Items.Count > 0)
            {
                lstMyComPort.SelectedIndex = 0;     // this can be modified to preferred default
                EnableConnectButton();
                UpdateToConnectButton();
            }
            else
            {
                DisableConnectButton();
                UpdateToConnectButton();
            }
        }

        private void EnableRefreshCOMButton()
        {
            btnRefreshCOMNo.Enabled = true;
        }

        private void DisableRefreshCOMButton()
        {
            btnRefreshCOMNo.Enabled = false;
        }

        private void EnableConnectButton()
        {
            btnConnectionControl.Enabled = true;
        }

        private void DisableConnectButton()
        {
            btnConnectionControl.Enabled = false;
        }

        const string CONNECT_UART_STRING_ON_BUTTON = "Connect UART";
        const string DISCONNECT_UART_STRING_ON_BUTTON = "Disconnect UART";

        private void UpdateToConnectButton()
        {
            btnConnectionControl.Text = CONNECT_UART_STRING_ON_BUTTON;
        }

        private void UpdateToDisconnectButton()
        {
            btnConnectionControl.Text = DISCONNECT_UART_STRING_ON_BUTTON;
        }

        private void btnFreshCOMNo_Click(object sender, System.EventArgs e)
        {
            Serial_UpdatePortName();
        }

        //
        // Print Serial Port Message on RichTextBox
        //
        delegate void AppendSerialMessageCallback(string text);
        public void AppendUARTViewerMessageLog(string my_str)
        {
            if (this.rtbSignalData.InvokeRequired)
            {
                AppendSerialMessageCallback d = new AppendSerialMessageCallback(AppendUARTViewerMessageLog);
                this.Invoke(d, new object[] { my_str });
            }
            else
            {
                this.rtbSignalData.AppendText(my_str);
                this.rtbSignalData.ScrollToCaret();
            }
        }

        // 這個主程式專用的delay的內部資料與function
        static bool MyUARTViewer_Delay_TimeOutIndicator = false;
        private static void MyUARTViewer_Delay_OnTimedEvent(object source, ElapsedEventArgs e)
        {
            MyUARTViewer_Delay_TimeOutIndicator = true;
        }

        private void MyUARTViewer_Delay(int delay_ms)
        {
            if (delay_ms <= 0) return;
            System.Timers.Timer aTimer = new System.Timers.Timer(delay_ms);
            aTimer.Elapsed += new ElapsedEventHandler(MyUARTViewer_Delay_OnTimedEvent);
            MyUARTViewer_Delay_TimeOutIndicator = false;
            aTimer.Enabled = true;
            while ((FormIsClosing == false) && (MyUARTViewer_Delay_TimeOutIndicator == false)) { Application.DoEvents(); Thread.Sleep(1); }
            aTimer.Stop();
            aTimer.Dispose();
        }

        private void btnConnectionControl_Click(object sender, EventArgs e)
        {
            if (btnConnectionControl.Text.Equals(CONNECT_UART_STRING_ON_BUTTON, StringComparison.Ordinal)) // Check if button is showing "Connect" at this moment.
            {   // User to connect
                string curItem = lstMyComPort.SelectedItem.ToString();
                if (MySerialPort.Serial_OpenPort(curItem) == true)
                {
                    MyUART_Exception_status = false;
                    UpdateToDisconnectButton();
                    DisableRefreshCOMButton();
                }
                else
                {
                    rtbSignalData.AppendText(DateTime.Now.ToString("h:mm:ss tt") + " - Cannot connect to UART.\n");
                }
            }
            else
            {   // User to disconnect
                if (MySerialPort.Serial_ClosePort() == true)
                {
                    UpdateToConnectButton();
                    EnableRefreshCOMButton();
                    if (MyUART_Exception_status)
                    { Serial_UpdatePortName(); }
                    MyUART_Exception_status = false;
                }
                else
                {
                    rtbSignalData.AppendText(DateTime.Now.ToString("h:mm:ss tt") + " - Cannot disconnect from UART.\n");
                }
            }
        }

        private static void LineReceivedHandler(object sender, GetLineEventArgs e)
        {
            Console.WriteLine(e.Message);
        }

        //
        // End of UART part
        //

        //
        // Form Events
        //
        private void MyUARTViewer_Load(object sender, EventArgs e)
        {
            //_serialPort = new SerialPort();
            //Serial_InitialSetting();
            Serial_UpdatePortName();
            //MyBlueRat.UARTException += MyUARTException;
        }

        private void MyUARTViewer_Closing(Object sender, FormClosingEventArgs e)
        {
            Console.WriteLine("BlueRatDevViewer_FormClosing");
            FormIsClosing = true;
            //MyBlueRat.Stop_Current_Tx();
            //MyBlueRat.Force_Init_BlueRat();
            MySerialPort.Serial_ClosePort();
        }
    }
}
