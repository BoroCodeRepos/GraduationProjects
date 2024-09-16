using System;
using System.Windows.Forms;
using System.IO.Ports;
using System.Management;
using Microsoft.Win32;

namespace CapacitySensor
{
    public static class Device
    {
        static SerialPort COM;

        const string CAPTION  = "CapacitySensor_PG_2023/2024";
        const string MANUFACT = "S179991 Borowicki Arkadiusz PG";

        public enum Commands
        {
            GET_CONSTANTS        = 'A',     // Get Constants Values
            SET_CONSTANTS        = 'S',     // Set Constants Values
            DEF_CONSTANTS        = 'D',     // Set Default Constants Values
            GET_CORRECTIONS      = 'I',     // Get Corrections Values
            SET_CORRECTIONS      = 'O',     // Set Corrections Values
            DEF_CORRECTIONS      = 'P',     // Set Default Corrections Values
            ENABLE_MEAS_CIRCUIT  = 'L',     // Enable Power On Meas Circuit
            SET_NOMINAL          = 'N',     // Disable Power On Meas Circuit
            TRIGGER_MEAS         = 'M',     // Trigger Measurement
            GET_TEMP_RH          = 'R',     // Read Temperature and Humidity
            LCD_AFTERMEAS        = 'W',     // Send Calculated Data To LCD	
            TEMP                 = 'T',
            RH                   = 'R',
            SAMPLES              = 'C',
        }

        public static void Initialize()
        {
            COM = new SerialPort
            {
                ReadTimeout = 500,
                BaudRate    = 115200,
                DataBits    = 8,
                StopBits    = StopBits.One,
                Handshake   = Handshake.None,
                Parity      = Parity.None,
                RtsEnable   = true,
                DtrEnable   = true,
            };
            COM.ReadTimeout = 5000;
        }

        public static bool IsAvailable()
        {
            return IsAvailable(out _);
        }

        public static bool IsAvailable(out string PortName)
        {
            PortName = "COM10";
            return true;

            using (ManagementClass Entity = new ManagementClass("Win32_PnPEntity"))
            {
                foreach (ManagementObject Inst in Entity.GetInstances())
                {
                    var ClassGuid = Inst.GetPropertyValue("ClassGuid");
                    if (ClassGuid == null || ClassGuid.ToString().ToUpper() != "{4D36E978-E325-11CE-BFC1-08002BE10318}")
                    {
                        continue;
                    }

                    string Caption = Inst.GetPropertyValue("Caption").ToString();
                    string Manufact = Inst.GetPropertyValue("Manufacturer").ToString();
                    string DeviceID = Inst.GetPropertyValue("PnpDeviceID").ToString();
                    string RegPath = "HKEY_LOCAL_MACHINE\\System\\CurrentControlSet\\Enum\\" + DeviceID + "\\Device Parameters";
                    string Name = Registry.GetValue(RegPath, "PortName", "").ToString();

                    Logs.Create(Logs.From.PC, Logs.Type.Info, $"{Caption} {Manufact}");

                    int Pos = Caption.IndexOf(" (COM");
                    if (Pos > 0)
                    {
                        Caption = Caption.Substring(0, Pos);
                    }

                    if (Caption == CAPTION && Manufact == MANUFACT)
                    {
                        PortName = Name;
                        return true;
                    }
                }
            }
            PortName = "";
            return false;
        }

        public static bool Open()
        {
            try
            {
                if (IsAvailable(out string PortName))
                {
                    COM.PortName = PortName;
                    COM.Open();
                    Logs.Create(Logs.From.Device, Logs.Type.Info, $"Device Connection Success - {PortName}");
                }
                else if (COM.IsOpen)
                {
                    Logs.Create(Logs.From.Device, Logs.Type.Error, $"Error: Attempt To Open Serial Port When Is Open");
                    COM.Dispose();
                    Open();
                }
            }
            catch (Exception exception)
            {
                Logs.Create(Logs.From.Device, Logs.Type.Error, $"Device Connection Error: {exception.Message}");
                MessageBox.Show(
                    exception.Message, 
                    "Device Connection Error", 
                    MessageBoxButtons.OK, 
                    MessageBoxIcon.Error
                );            
            }
            return COM.IsOpen;
        }

        public static void Close()
        {
            try
            {
                COM.Close();
            }
            catch (Exception) { }
            finally
            {
                COM.Dispose();
                Logs.Create(Logs.From.Device, Logs.Type.Warning, "Device Disconnected");
            }
        }

        public static bool IsOpen()
        {
            return COM.IsOpen;
        }
        
        public static bool SendCommand(in Commands Command, out string Received)
        {
            Received = "";
            try
            {
                if (!(IsAvailable() && IsOpen()))
                {
                    throw new Exception($"An attempt was made to send a command ({(char)Command}) to a device while it is disconnected.");
                }
                string ToSend = $"{(char)Command}\n";
                COM.Write(ToSend);
                Logs.Create(Logs.From.PC, Logs.Type.Info, 
                    $"Sent Command: {Command} ({(char)Command})");
                Received = COM.ReadLine();
                if (Received.Contains("Error"))
                {
                    throw new Exception(Received);
                }
                Logs.Create(Logs.From.Device, Logs.Type.Info,
                    $"Data Received: {Received}");
                return true;
            }
            catch (Exception exception)
            {
                MainForm.Instance.TIM_Meas.Enabled = false;
                Logs.Create(Logs.From.Device, Logs.Type.Error, exception.Message);
                MessageBox.Show(
                    exception.Message,
                    "Device Communication Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            
            return false;
        }

        public static bool SendString(in Commands Command, in string Text, out string Received)
        {
            Received = "";
            try
            {
                if (!(IsAvailable() && IsOpen()))
                {
                    throw new Exception($"An attempt was made to send a command ({(char)Command}) to a device while it is disconnected.");
                }
                string ToSend = $"{(char)Command} {Text}\n";
                COM.Write(ToSend);
                Logs.Create(Logs.From.PC, Logs.Type.Info,
                    $"Sent Command: {ToSend})");
                Received = COM.ReadLine();
                if (Received.Contains("Error"))
                {
                    throw new Exception(Received);
                }
                Logs.Create(Logs.From.Device, Logs.Type.Info,
                    $"Data Received: {Received}");
                return true;
            }
            catch (Exception exception)
            {
                Logs.Create(Logs.From.Device, Logs.Type.Error, exception.Message);
                MessageBox.Show(
                    exception.Message,
                    "Device Communication Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            return false;
        }

        public static bool SendString(in string Message, out string Received)
        {
            Received = "";
            try
            {
                if (!(IsAvailable() && IsOpen()))
                {
                    throw new Exception($"An attempt was made to send a command ({Message}) to a device while it is disconnected.");
                }
                string ToSend = $"{Message}\n";
                COM.Write(ToSend);
                Logs.Create(Logs.From.User, Logs.Type.Info,
                    $"Sent Command: {ToSend})");
                Received = COM.ReadLine();
                if (Received.Contains("Error"))
                {
                    throw new Exception(Received);
                }
                Logs.Create(Logs.From.Device, Logs.Type.Info,
                    $"Data Received: {Received}");
                return true;
            }
            catch (Exception exception)
            {
                Logs.Create(Logs.From.Device, Logs.Type.Error, exception.Message);
                MessageBox.Show(
                    exception.Message,
                    "Device Communication Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
            return false;
        }

        public static string GetCurrentPortName()
        {
            return COM.PortName;
        }
    }
}
