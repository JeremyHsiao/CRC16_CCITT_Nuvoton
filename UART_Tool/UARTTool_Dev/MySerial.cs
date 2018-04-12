using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Text;

namespace MySerialLibrary
{
    //
    // Get a line of command event
    // Reference: https://docs.microsoft.com/zh-tw/dotnet/csharp/programming-guide/events/how-to-publish-events-that-conform-to-net-framework-guidelines
    //
    public class GetLineEventArgs : EventArgs
    {
        public GetLineEventArgs(string s)
        {
            msg = s;
        }
        private string msg;
        public string Message
        {
            get { return msg; }
        }
    }

    class MySerial : IDisposable
    {
        static private Dictionary<string, Object> MySerialDictionary = new Dictionary<string, Object>();

        private SerialPort _serialPort;
        private Queue<char> Rx_char_buffer_QUEUE = new Queue<char>();
        //static bool _system_IO_exception = false;

        //
        // public functions
        //
        public const int Serial_BaudRate = 115200;
        public const Parity Serial_Parity = Parity.None;
        public const int Serial_DataBits = 8;
        public const StopBits Serial_StopBits = StopBits.One;

        public MySerial()
        {
            _serialPort = new SerialPort
            {
                BaudRate = Serial_BaudRate,
                Parity = Serial_Parity,
                DataBits = Serial_DataBits,
                StopBits = Serial_StopBits
            };
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // dispose managed resources
                _serialPort.Close();
            }
            // free native resources
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public MySerial(string com_port) { _serialPort = new SerialPort(com_port, Serial_BaudRate, Serial_Parity, Serial_DataBits, Serial_StopBits); }
        public string GetPortName() { return _serialPort.PortName; }

        public Boolean Serial_OpenPort()
        {
            Boolean bRet = false;
            _serialPort.Handshake = Handshake.None;
            _serialPort.Encoding = Encoding.UTF8;
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;
            _serialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);

            try
            {
                _serialPort.Open();
                Start_SerialReadThread();
                //_system_IO_exception = false;
                MySerialDictionary.Add(_serialPort.PortName, this);
                bRet = true;
            }
            catch (Exception ex232)
            {
                Console.WriteLine("Serial_OpenPort Exception at PORT: " + _serialPort.PortName + " - " + ex232);
                bRet = false;
            }
            return bRet;
        }

        public Boolean Serial_OpenPort(string PortName)
        {
            Boolean bRet = false;
            _serialPort.PortName = PortName;
            bRet = Serial_OpenPort();
            return bRet;
        }

        public Boolean Serial_ClosePort()
        {
            Boolean bRet = false;
            MySerialDictionary.Remove(_serialPort.PortName);
            try
            {
                Stop_SerialReadThread();
                _serialPort.Close();
                bRet = true;
            }
            catch (Exception ex232)
            {
                Console.WriteLine("Serial_ClosePort Exception at PORT: " + _serialPort.PortName + " - " + ex232);
                bRet = false;
            }
            return bRet;
        }

        public Boolean Serial_PortConnection()
        {
            Boolean bRet = false;
            //if ((_serialPort.IsOpen == true) && (readThread.IsAlive))
            if (_serialPort.IsOpen == true)
            {
                bRet = true;
            }
            return bRet;
        }

        //
        // Start of read part
        //

        enum ENUM_RX_BUFFER_CHAR_STATUS
        {
            EMPTY_BUFFER = 0,
            CHAR_RECEIVED,
            DISCARD_CHAR_RECEIVED,
            MAX_STATUS_NO
        };

        private void Start_SerialReadThread()
        {
            Rx_char_buffer_QUEUE.Clear();
        }

        private void Stop_SerialReadThread()
        {
            Rx_char_buffer_QUEUE.Clear();
        }

        private static void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            // Find out which serial port --> which bluerat
            SerialPort sp = (SerialPort)sender;
            MySerialDictionary.TryGetValue(sp.PortName, out Object my_serial_obj);
            MySerial mySerial = (MySerial)my_serial_obj;
            //Rx_char_buffer_QUEUE
            int buf_len = sp.BytesToRead;
            if (buf_len > 0)
            {
                // Read in all char
                char[] input_buf = new char[buf_len];
                sp.Read(input_buf, 0, buf_len);

                int ch_index = 0;
                while (ch_index < buf_len)
                {
                    char ch = input_buf[ch_index];
                    if ((ch == '\n')||(ch == '\r'))
                    {
                        if (mySerial.Rx_char_buffer_QUEUE.Count > 0)
                        {
                            char[] temp_char_array = new char[mySerial.Rx_char_buffer_QUEUE.Count];
                            mySerial.Rx_char_buffer_QUEUE.CopyTo(temp_char_array, 0);
                            mySerial.Rx_char_buffer_QUEUE.Clear();
                            string temp_str = new string(temp_char_array);
                            mySerial.OnRaiseGetLineEvent(new GetLineEventArgs(temp_str));
                        }
                    }
                    else
                    {
                        mySerial.Rx_char_buffer_QUEUE.Enqueue(ch);
                    }
                    ch_index++;
                }
            }
        }
        //
        // End of read part
        //

        public bool SendToSerial(byte[] byte_to_sent)
        {
            bool return_value = false;

            if (_serialPort.IsOpen == true)
            {
                //Application.DoEvents();
                try
                {
                    int temp_index = 0;
                    const int fixed_length = 8;

                    while ((temp_index < byte_to_sent.Length) && (_serialPort.IsOpen == true))
                    {
                        if ((temp_index + fixed_length) < byte_to_sent.Length)
                        {
                            _serialPort.Write(byte_to_sent, temp_index, fixed_length);
                            temp_index += fixed_length;
                        }
                        else
                        {
                            _serialPort.Write(byte_to_sent, temp_index, (byte_to_sent.Length - temp_index));
                            temp_index = byte_to_sent.Length;
                        }
                    }
                    return_value = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("SendToSerial - " + ex);
                    return_value = false;
                }
            }
            else
            {
                Console.WriteLine("COM is closed and cannot send byte data\n");
                return_value = false;
            }
            return return_value;
        }

        //
        // Create a GetLine Event
        // Reference: https://docs.microsoft.com/zh-tw/dotnet/csharp/programming-guide/events/how-to-publish-events-that-conform-to-net-framework-guidelines
        //
        public delegate void GetLineEventHandler(object sender, GetLineEventArgs a);
        public event GetLineEventHandler GetLineEvent;

        // Wrap event invocations inside a protected virtual method
        // to allow derived classes to override the event invocation behavior
        protected virtual void OnRaiseGetLineEvent(GetLineEventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            GetLineEvent?.Invoke(this, e);
        }

        //
        // To process UART IO Exception
        //
        protected virtual void OnUARTException(EventArgs e)
        {
            UARTException?.Invoke(this, e);
        }

        public event EventHandler UARTException;
    }
}
