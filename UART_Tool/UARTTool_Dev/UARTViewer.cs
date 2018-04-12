using System;
using System.IO.Ports;
using System.Threading;
using System.Windows.Forms;
using System.Timers;
using System.Collections.Generic;
using System.Text;
using MySerialLibrary;
using CRCLibrary;

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

        private void UpdateButtonAfterConnected()
        {
            btnCalcCRC.Enabled = true;
        }

        private void UpdateButtonAfterDisconnected()
        {
            btnCalcCRC.Enabled = false;
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
        // This list stores all running timers. If timer timeout, it is removed at event. When BlueRat stop connection, all timers under this bluerat are also removed.
        static private List<object> TimeOutTimerList = new List<object>();
        // This list stores only running timers belonged to this bluerat object.
        private List<object> MyOwnTimerList = new List<object>();

        private void Stop_MyOwn_HomeMade_Delay()
        {
            // Stop all timer created from own BlueRat Object (in list MyOwnTimerList)
            foreach (var timer in MyOwnTimerList)
            {
                if (TimeOutTimerList.Contains(timer))        // still timer of this bluerat object is running?
                {
                    TimeOutTimerList.Remove(timer);         // if yes, force it to expire
                    //Application.DoEvents();
                }
            }
            MyOwnTimerList.Clear();                         // all timer expired, no need to keep record
        }

        static private void HomeMade_Delay_OnTimedEvent(object source, ElapsedEventArgs e)
        {
            //HomeMade_TimeOutIndicator = true;
            TimeOutTimerList.Remove(source);
        }

        private void MyUARTViewer_Delay(int delay_ms)
        {
            if (delay_ms <= 0) return;
            System.Timers.Timer aTimer = new System.Timers.Timer(delay_ms);
            aTimer.Elapsed += new ElapsedEventHandler(HomeMade_Delay_OnTimedEvent);
            //HomeMade_TimeOutIndicator = false;
            TimeOutTimerList.Add(aTimer);           // This list is to keep running timer until it reaches TimeOutEvent (as indicator of running timer)
            MyOwnTimerList.Add(aTimer);             // This list record all timer created in this bluerat object -- to be removed after timer expired
            aTimer.Enabled = true;
            while ((FormIsClosing == false) && (TimeOutTimerList.Contains(aTimer) == true)) { Thread.Sleep(1); }
            MyOwnTimerList.Remove(aTimer);          // timer expired, so remove it from the list recording timer created.
            aTimer.Stop();
            aTimer.Dispose();
        }
        // END - 這個主程式專用的delay的內部資料與function

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
                    UpdateButtonAfterConnected();
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
                    UpdateButtonAfterDisconnected();
                }
                else
                {
                    rtbSignalData.AppendText(DateTime.Now.ToString("h:mm:ss tt") + " - Cannot disconnect from UART.\n");
                }
            }
        }

        private void LineReceivedHandler(object sender, GetLineEventArgs e)
        {
            AppendUARTViewerMessageLog(e.Message+"\n");
        }

        //
        // End of UART part
        //

        private List<Byte> ConvertInputString2ByteList(String input_str)
        {
            List<Byte> input_byte_for_crc_calculation = new List<Byte>();

            if (input_str.IndexOf('0') == 0)        // if starting with'0', treat it as several hex byte data in C format
            {
                char[] Delimiter = { ' ', ',', '\x0d', '\x0a' };
                string[] splited_str = input_str.Split(Delimiter);
                foreach (string temp in splited_str)
                {
                    input_byte_for_crc_calculation.Add(Convert.ToByte(temp, 16));
                }
            }
            else                            // otherwise, all string as input data
            {
                byte[] asciiBytes = Encoding.ASCII.GetBytes(input_str);
                input_byte_for_crc_calculation.AddRange(asciiBytes);
            }
            return input_byte_for_crc_calculation;
        }

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
            Console.WriteLine("MyUARTViewer_Closing");
            FormIsClosing = true;
            MySerialPort.Serial_ClosePort();
        }

        private void btnCalcCRC_Click(object sender, EventArgs e)
        {
            String input_str = txtDataforCRC.Text;
            List<Byte> crc_input_data = ConvertInputString2ByteList(input_str);
            UInt16 CRC_Value;

            // Display input data
            //foreach (byte byte_data in crc_input_data)
            //{
            //    rtbSignalData.AppendText(byte_data.ToString("X") + " ");
            //}
            //rtbSignalData.AppendText("\n");

            // Calculate CRC byte-by-byte
            CRC_CCITT16.InitCRCValue();
            foreach (byte byte_data in crc_input_data)
            {
                CRC_Value = CRC_CCITT16.GetNextCRC(byte_data);
                //rtbSignalData.AppendText(CRC_Value.ToString("X") + " ");
            }
            CRC_Value = CRC_CCITT16.GetCRCAfterFinalCRC();

            // Calculate CRC all at once
            CRC_Value = CRC_CCITT16.CalculateCRC4ByteList(crc_input_data);
            rtbSignalData.AppendText(CRC_Value.ToString("X") + " ");

            // Base64-encoded before sending
            String EncodedString = Convert.ToBase64String(crc_input_data.ToArray());
            rtbSignalData.AppendText(EncodedString+"\n");

            // Prepare to send via UART
            EncodedString += "\n";
            MySerialPort.SendToSerial(Encoding.ASCII.GetBytes(EncodedString));
        }
    }
}
