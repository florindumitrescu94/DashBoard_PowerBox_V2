// TODO fill in this information for your driver, then remove this line!
//
// ASCOM Switch hardware class for DashBoardPowerBoxV2
//
// Description:	 <To be completed by driver developer>
//
// Implements:	ASCOM Switch interface version: <To be completed by driver developer>
// Author:		(XXX) Your N. Here <your@email.here>
//

using ASCOM;
using ASCOM.Astrometry;
using ASCOM.Astrometry.AstroUtils;
using ASCOM.Astrometry.NOVAS;
using ASCOM.DeviceInterface;
using ASCOM.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;

namespace ASCOM.DashBoardPowerBoxV2.Switch
{
    //
    // TODO Replace the not implemented exceptions with code to implement the function or throw the appropriate ASCOM exception.
    //

    /// <summary>
    /// ASCOM Switch hardware class for DashBoardPowerBoxV2.
    /// </summary>
    [HardwareClass()] // Class attribute flag this as a device hardware class that needs to be disposed by the local server when it exits.
    internal static class SwitchHardware
    {
        // Constants used for Profile persistence
        internal const string comPortProfileName = "COM Port";
        internal const string comPortDefault = "COM1";
        internal const string traceStateProfileName = "Trace Level";
        internal const string traceStateDefault = "true";
        internal static string ConnectionDelayProfileName = "Connection Delay (ms)";
        internal static string ConnectionDelayDefault = "5000";
        internal static string ConnectionDelay;

        internal static string numSwitchProfileName = "Max Switches";
        internal static string numSwitchDefault = "10";
        internal static string numSwitch;

        internal static string SwitchName0ProfileName = "DC Jacks";
        internal static string SwitchName1ProfileName = "PWM 1 - Main";
        internal static string SwitchName2ProfileName = "PWM 2 - Guide";
        internal static string SwitchName3ProfileName = "Temperature sensor";
        internal static string SwitchName4ProfileName = "Humidity sensor";
        internal static string SwitchName5ProfileName = "Dew Point sensor";
        internal static string SwitchName6ProfileName = "Voltage sensor";
        internal static string SwitchName7ProfileName = "Current sensor";
        internal static string SwitchName8ProfileName = "Power sensor";
        internal static string SwitchName9ProfileName = "Total power usage";
        internal static string SwitchName0Default = "DC Jacks";
        internal static string SwitchName1Default = "PWM 1 - Main";
        internal static string SwitchName2Default = "PWM 2 - Guide";
        internal static string SwitchName3Default = "Temperature sensor";
        internal static string SwitchName4Default = "Humidity sensor";
        internal static string SwitchName5Default = "Dew Point sensor";
        internal static string SwitchName6Default = "Voltage sensor";
        internal static string SwitchName7Default = "Current sensor";
        internal static string SwitchName8Default = "Power sensor";
        internal static string SwitchName9Default = "Total power consumption";
        internal static string SwitchName0;
        internal static string SwitchName1;
        internal static string SwitchName2;
        internal static string SwitchName3;
        internal static string SwitchName4;
        internal static string SwitchName5;
        internal static string SwitchName6;
        internal static string SwitchName7;
        internal static string SwitchName8;
        internal static string SwitchName9;

        internal static string SwitchState0ProfileName = "DC Jack state";
        internal static string SwitchState1ProfileName = "PWM 1 state";
        internal static string SwitchState2ProfileName = "PWM 2 state";
        internal static string SwitchState3ProfileName = "Temperature reading";
        internal static string SwitchState4ProfileName = "Humidity reading";
        internal static string SwitchState5ProfileName = "Dew point reading";
        internal static string SwitchState6ProfileName = "Voltage reading";
        internal static string SwitchState7ProfileName = "Current reading";
        internal static string SwitchState8ProfileName = "Power reading";
        internal static string SwitchState9ProfileName = "Total power usage reading";
        internal static string SwitchState0Default = "OFF";
        internal static string SwitchState1Default = "0";
        internal static string SwitchState2Default = "0";
        internal static string SwitchState3Default = "0.00";
        internal static string SwitchState4Default = "0.0";
        internal static string SwitchState5Default = "0.00";
        internal static string SwitchState6Default = "0.00";
        internal static string SwitchState7Default = "0.00";
        internal static string SwitchState8Default = "0.00";
        internal static string SwitchState9Default = "0.00";
        internal static string SwitchState0;
        internal static string SwitchState1;
        internal static string SwitchState2;
        internal static string SwitchState3;
        internal static string SwitchState4;
        internal static string SwitchState5;
        internal static string SwitchState6;
        internal static string SwitchState7;
        internal static string SwitchState8;
        internal static string SwitchState9;

        private static string DriverProgId = ""; // ASCOM DeviceID (COM ProgID) for this driver, the value is set by the driver's class initialiser.
        private static string DriverDescription = ""; // The value is set by the driver's class initialiser.
        internal static string comPort; // COM port name (if required)
        private static bool connectedState; // Local server's connected state
        private static bool runOnce = false; // Flag to enable "one-off" activities only to run once.
        internal static Util utilities; // ASCOM Utilities object for use as required
        internal static AstroUtils astroUtilities; // ASCOM AstroUtilities object for use as required
        internal static TraceLogger tl; // Local server's trace logger object for diagnostic log with information that you specify
        private static ASCOM.Utilities.Serial objSerial;

        /// <summary>
        /// Initializes a new instance of the device Hardware class.
        /// </summary>
        static SwitchHardware()
        {
            try
            {
                // Create the hardware trace logger in the static initialiser.
                // All other initialisation should go in the InitialiseHardware method.
                tl = new TraceLogger("", "DashBoardPowerBoxV2.Hardware");

                // DriverProgId has to be set here because it used by ReadProfile to get the TraceState flag.
                DriverProgId = Switch.DriverProgId; // Get this device's ProgID so that it can be used to read the Profile configuration values

                // ReadProfile has to go here before anything is written to the log because it loads the TraceLogger enable / disable state.
                ReadProfile(); // Read device configuration from the ASCOM Profile store, including the trace state

                LogMessage("SwitchHardware", $"Static initialiser completed.");
            }
            catch (Exception ex)
            {
                try { LogMessage("SwitchHardware", $"Initialisation exception: {ex}"); } catch { }
                MessageBox.Show($"{ex.Message}", "Exception creating ASCOM.DashBoardPowerBoxV2.Switch", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }

        /// <summary>
        /// Place device initialisation code here that delivers the selected ASCOM <see cref="Devices."/>
        /// </summary>
        /// <remarks>Called every time a new instance of the driver is created.</remarks>
        internal static void InitialiseHardware()
        {
            // This method will be called every time a new ASCOM client loads your driver
            LogMessage("InitialiseHardware", $"Start.");

            // Make sure that "one off" activities are only undertaken once
            if (runOnce == false)
            {
                LogMessage("InitialiseHardware", $"Starting one-off initialisation.");

                DriverDescription = Switch.DriverDescription; // Get this device's Chooser description

                LogMessage("InitialiseHardware", $"ProgID: {DriverProgId}, Description: {DriverDescription}");

                connectedState = false; // Initialise connected to false
                utilities = new Util(); //Initialise ASCOM Utilities object
                astroUtilities = new AstroUtils(); // Initialise ASCOM Astronomy Utilities object

                LogMessage("InitialiseHardware", "Completed basic initialisation");

                // Add your own "one off" device initialisation here e.g. validating existence of hardware and setting up communications

                LogMessage("InitialiseHardware", $"One-off initialisation complete.");
                runOnce = true; // Set the flag to ensure that this code is not run again
            }
        }

        // PUBLIC COM INTERFACE ISwitchV2 IMPLEMENTATION

        #region Common properties and methods.

        /// <summary>
        /// Displays the Setup Dialogue form.
        /// If the user clicks the OK button to dismiss the form, then
        /// the new settings are saved, otherwise the old values are reloaded.
        /// THIS IS THE ONLY PLACE WHERE SHOWING USER INTERFACE IS ALLOWED!
        /// </summary>
        public static void SetupDialog()
        {
            // Don't permit the setup dialogue if already connected
            if (IsConnected)
                MessageBox.Show("Already connected, just press OK");

            using (SetupDialogForm F = new SetupDialogForm(tl))
            {
                var result = F.ShowDialog();
                if (result == DialogResult.OK)
                {
                    WriteProfile(); // Persist device configuration values to the ASCOM Profile store
                }
            }
        }

        /// <summary>Returns the list of custom action names supported by this driver.</summary>
        /// <value>An ArrayList of strings (SafeArray collection) containing the names of supported actions.</value>
        public static ArrayList SupportedActions
        {
            get
            {
                LogMessage("SupportedActions Get", "Returning empty ArrayList");
                return new ArrayList();
            }
        }

        /// <summary>Invokes the specified device-specific custom action.</summary>
        /// <param name="ActionName">A well known name agreed by interested parties that represents the action to be carried out.</param>
        /// <param name="ActionParameters">List of required parameters or an <see cref="String.Empty">Empty String</see> if none are required.</param>
        /// <returns>A string response. The meaning of returned strings is set by the driver author.
        /// <para>Suppose filter wheels start to appear with automatic wheel changers; new actions could be <c>QueryWheels</c> and <c>SelectWheel</c>. The former returning a formatted list
        /// of wheel names and the second taking a wheel name and making the change, returning appropriate values to indicate success or failure.</para>
        /// </returns>
        public static string Action(string actionName, string actionParameters)
        {
            LogMessage("Action", $"Action {actionName}, parameters {actionParameters} is not implemented");
            throw new ActionNotImplementedException("Action " + actionName + " is not implemented by this driver");
        }

        /// <summary>
        /// Transmits an arbitrary string to the device and does not wait for a response.
        /// Optionally, protocol framing characters may be added to the string before transmission.
        /// </summary>
        /// <param name="Command">The literal command string to be transmitted.</param>
        /// <param name="Raw">
        /// if set to <c>true</c> the string is transmitted 'as-is'.
        /// If set to <c>false</c> then protocol framing characters may be added prior to transmission.
        /// </param>
        public static void CommandBlind(string command, bool raw)
        {
            CheckConnected("CommandBlind");
            // TODO The optional CommandBlind method should either be implemented OR throw a MethodNotImplementedException
            // If implemented, CommandBlind must send the supplied command to the mount and return immediately without waiting for a response

            throw new MethodNotImplementedException($"CommandBlind - Command:{command}, Raw: {raw}.");
        }

        /// <summary>
        /// Transmits an arbitrary string to the device and waits for a boolean response.
        /// Optionally, protocol framing characters may be added to the string before transmission.
        /// </summary>
        /// <param name="Command">The literal command string to be transmitted.</param>
        /// <param name="Raw">
        /// if set to <c>true</c> the string is transmitted 'as-is'.
        /// If set to <c>false</c> then protocol framing characters may be added prior to transmission.
        /// </param>
        /// <returns>
        /// Returns the interpreted boolean response received from the device.
        /// </returns>
        public static bool CommandBool(string command, bool raw)
        {
            CheckConnected("CommandBool");
            // TODO The optional CommandBool method should either be implemented OR throw a MethodNotImplementedException
            // If implemented, CommandBool must send the supplied command to the mount, wait for a response and parse this to return a True or False value

            throw new MethodNotImplementedException($"CommandBool - Command:{command}, Raw: {raw}.");
        }

        /// <summary>
        /// Transmits an arbitrary string to the device and waits for a string response.
        /// Optionally, protocol framing characters may be added to the string before transmission.
        /// </summary>
        /// <param name="Command">The literal command string to be transmitted.</param>
        /// <param name="Raw">
        /// if set to <c>true</c> the string is transmitted 'as-is'.
        /// If set to <c>false</c> then protocol framing characters may be added prior to transmission.
        /// </param>
        /// <returns>
        /// Returns the string response received from the device.
        /// </returns>
        public static string CommandString(string command, bool raw)
        {
            CheckConnected("CommandString");
            // TODO The optional CommandString method should either be implemented OR throw a MethodNotImplementedException
            // If implemented, CommandString must send the supplied command to the mount and wait for a response before returning this to the client

            throw new MethodNotImplementedException($"CommandString - Command:{command}, Raw: {raw}.");
        }

        /// <summary>
        /// Deterministically release both managed and unmanaged resources that are used by this class.
        /// </summary>
        /// <remarks>
        /// TODO: Release any managed or unmanaged resources that are used in this class.
        /// 
        /// Do not call this method from the Dispose method in your driver class.
        ///
        /// This is because this hardware class is decorated with the <see cref="HardwareClassAttribute"/> attribute and this Dispose() method will be called 
        /// automatically by the  local server executable when it is irretrievably shutting down. This gives you the opportunity to release managed and unmanaged 
        /// resources in a timely fashion and avoid any time delay between local server close down and garbage collection by the .NET runtime.
        ///
        /// For the same reason, do not call the SharedResources.Dispose() method from this method. Any resources used in the static shared resources class
        /// itself should be released in the SharedResources.Dispose() method as usual. The SharedResources.Dispose() method will be called automatically 
        /// by the local server just before it shuts down.
        /// 
        /// </remarks>
        public static void Dispose()
        {
            try { LogMessage("Dispose", $"Disposing of assets and closing down."); } catch { }

            try
            {
                // Clean up the trace logger and utility objects
                tl.Enabled = false;
                tl.Dispose();
                tl = null;
            }
            catch { }

            try
            {
                utilities.Dispose();
                utilities = null;
            }
            catch { }

            try
            {
                astroUtilities.Dispose();
                astroUtilities = null;
            }
            catch { }
        }

        /// <summary>
        /// Set True to connect to the device hardware. Set False to disconnect from the device hardware.
        /// You can also read the property to check whether it is connected. This reports the current hardware state.
        /// </summary>
        /// <value><c>true</c> if connected to the hardware; otherwise, <c>false</c>.</value>
        public static bool Connected
        {
            get
            {
                LogMessage("Connected", $"Get {IsConnected}");
                return IsConnected;
            }
            set
            {
                LogMessage("Connected", $"Set {value}");
                if (value == IsConnected)
                    return;

                if (value)
                {
                    connectedState = true;
                    LogMessage("Connected Set", "Connecting to port " + comPort);
                    // TODO connect to the device
                    objSerial = new ASCOM.Utilities.Serial();
                    string numComPort;
                    numComPort = comPort.Replace("COM", "");
                    objSerial.Port = Convert.ToInt16(numComPort);
                    objSerial.Speed = (SerialSpeed)9600;
                    objSerial.ReceiveTimeout = 5;
                    objSerial.Connected = true;
                    System.Threading.Thread.Sleep(Convert.ToInt16(ConnectionDelay));
                    objSerial.ClearBuffers();
                    if (Convert.ToInt16(numSwitch) >= 1)
                    {
                        SwitchState0 = "OFF";
                    }
                    else if (Convert.ToInt16(numSwitch) >= 2)
                    {
                        SwitchState1 = "0";
                    }
                    else if (Convert.ToInt16(numSwitch) >= 3)
                    {
                        SwitchState2 = "0";
                    }
                    else if (Convert.ToInt16(numSwitch) >= 4)
                    {
                        SwitchState3 = "0";
                    }
                    else if (Convert.ToInt16(numSwitch) >= 5)
                    {
                        SwitchState4 = "0";
                    }
                    else if (Convert.ToInt16(numSwitch) >= 6)
                    {
                        SwitchState5 = "0";
                    }
                    else if (Convert.ToInt16(numSwitch) >= 7)
                    {
                        SwitchState6 = "0";
                    }
                    else if (Convert.ToInt16(numSwitch) >= 8)
                    {
                        SwitchState7 = "0";
                    }
                    else if (Convert.ToInt16(numSwitch) >= 9)
                    {
                        SwitchState8 = "0";
                    }
                    else if (Convert.ToInt16(numSwitch) >= 10)
                    {
                        SwitchState9 = "0";
                    }
                    else
                    {
                        LogMessage("Switch" + numSwitch.ToString(), "Invalid Value");
                        throw new InvalidValueException("Switch", numSwitch.ToString(), string.Format("0 to {0}", Convert.ToInt16(numSwitch) - 1));
                    }
                }
                else
                {
                    connectedState = false;
                    if (Convert.ToInt16(numSwitch) >= 1)
                    {
                        SwitchState0 = "OFF";
                    }
                    else if (Convert.ToInt16(numSwitch) >= 2)
                    {
                        SwitchState1 = "0";
                    }
                    else if (Convert.ToInt16(numSwitch) >= 3)
                    {
                        SwitchState2 = "0";
                    }
                    else if (Convert.ToInt16(numSwitch) >= 4)
                    {
                        SwitchState3 = "0";
                    }
                    else if (Convert.ToInt16(numSwitch) >= 5)
                    {
                        SwitchState4 = "0";
                    }
                    else if (Convert.ToInt16(numSwitch) >= 6)
                    {
                        SwitchState5 = "0";
                    }
                    else if (Convert.ToInt16(numSwitch) >= 7)
                    {
                        SwitchState6 = "0";
                    }
                    else if (Convert.ToInt16(numSwitch) >= 8)
                    {
                        SwitchState7 = "0";
                    }
                    else if (Convert.ToInt16(numSwitch) >= 9)
                    {
                        SwitchState8 = "0";
                    }
                    else if (Convert.ToInt16(numSwitch) >= 10)
                    {
                        SwitchState9 = "0";
                    }
                    else
                    {
                        LogMessage("Switch" + numSwitch.ToString(), "Invalid Value");
                        throw new InvalidValueException("Switch", numSwitch.ToString(), string.Format("0 to {0}", Convert.ToInt16(numSwitch) - 1));
                    }
                    LogMessage("Connected Set", "Disconnecting from port " + comPort);
                    // TODO disconnect from the device
                    objSerial.Connected = false;
                }
            }
        }

        /// <summary>
        /// Returns a description of the device, such as manufacturer and model number. Any ASCII characters may be used.
        /// </summary>
        /// <value>The description.</value>
        public static string Description
        {
            // TODO customise this device description if required
            get
            {
                LogMessage("Description Get", DriverDescription);
                return DriverDescription;
            }
        }

        /// <summary>
        /// Descriptive and version information about this ASCOM driver.
        /// </summary>
        public static string DriverInfo
        {
            get
            {
                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                // TODO customise this driver description if required
                string driverInfo = $"Information about the driver itself. Version: {version.Major}.{version.Minor}";
                LogMessage("DriverInfo Get", driverInfo);
                return driverInfo;
            }
        }

        /// <summary>
        /// A string containing only the major and minor version of the driver formatted as 'm.n'.
        /// </summary>
        public static string DriverVersion
        {
            get
            {
                Version version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                string driverVersion = $"{version.Major}.{version.Minor}";
                LogMessage("DriverVersion Get", driverVersion);
                return driverVersion;
            }
        }

        /// <summary>
        /// The interface version number that this device supports.
        /// </summary>
        public static short InterfaceVersion
        {
            // set by the driver wizard
            get
            {
                LogMessage("InterfaceVersion Get", "2");
                return Convert.ToInt16("2");
            }
        }

        /// <summary>
        /// The short name of the driver, for display purposes
        /// </summary>
        public static string Name
        {
            get
            {
                string name = "DashBoard PowerBox V2 Driver";
                LogMessage("Name Get", name);
                return name;
            }
        }

        #endregion

        #region ISwitchV2 Implementation

        /// <summary>
        /// The number of switches managed by this driver
        /// </summary>
        /// <returns>The number of devices managed by this driver.</returns>
        internal static short MaxSwitch
        {
            get
            {
                LogMessage("MaxSwitch Get", numSwitch.ToString());
                return Convert.ToInt16(numSwitch);
            }
        }

        /// <summary>
        /// Return the name of switch device n.
        /// </summary>
        /// <param name="id">The device number (0 to <see cref="MaxSwitch"/> - 1)</param>
        /// <returns>The name of the device</returns>
        internal static string GetSwitchName(short id)
        {
            Validate("GetSwitchName", id);
            using (var driverProfile = new Profile())
            {
                driverProfile.DeviceType = "Switch";
                if (id == 0 & Convert.ToInt16(numSwitch) >= 1)
                {
                    LogMessage("GetSwitchName " + id.ToString(), SwitchName0);
                    return SwitchName0;
                }
                else if (id == 1 & Convert.ToInt16(numSwitch) >= 2)
                {
                    LogMessage("GetSwitchName " + id.ToString(), SwitchName1);
                    return SwitchName1;
                }
                else if (id == 2 & Convert.ToInt16(numSwitch) >= 3)
                {
                    LogMessage("GetSwitchName " + id.ToString(), SwitchName2);
                    return SwitchName2;
                }
                else if (id == 3 & Convert.ToInt16(numSwitch) >= 4)
                {
                    LogMessage("GetSwitchName " + id.ToString(), SwitchName3);
                    return SwitchName3;
                }
                else if (id == 4 & Convert.ToInt16(numSwitch) >= 5)
                {
                    LogMessage("GetSwitchName " + id.ToString(), SwitchName4);
                    return SwitchName4;
                }
                else if (id == 5 & Convert.ToInt16(numSwitch) >= 6)
                {
                    LogMessage("GetSwitchName " + id.ToString(), SwitchName5);
                    return SwitchName5;
                }
                else if (id == 6 & Convert.ToInt16(numSwitch) >= 7)
                {
                    LogMessage("GetSwitchName " + id.ToString(), SwitchName6);
                    return SwitchName6;
                }
                else if (id == 7 & Convert.ToInt16(numSwitch) >= 8)
                {
                    LogMessage("GetSwitchName " + id.ToString(), SwitchName7);
                    return SwitchName7;
                }
                else if (id == 8 & Convert.ToInt16(numSwitch) >= 9)
                {
                    LogMessage("GetSwitchName " + id.ToString(), SwitchName8);
                    return SwitchName8;
                }
                else if (id == 9 & Convert.ToInt16(numSwitch) >= 10)
                {
                    LogMessage("GetSwitchName " + id.ToString(), SwitchName9);
                    return SwitchName9;
                }
                else
                {
                    LogMessage("GetSwitchName", "Not Implemented");
                    throw new ASCOM.MethodNotImplementedException("GetSwitchName");
                }
            }
        }

        /// <summary>
        /// Set a switch device name to a specified value.
        /// </summary>
        /// <param name="id">The device number (0 to <see cref="MaxSwitch"/> - 1)</param>
        /// <param name="name">The name of the device</param>
        internal static void SetSwitchName(short id, string name)
        {
            Validate("SetSwitchName", id);
            using (var driverProfile = new Profile())
            {
                driverProfile.DeviceType = "Switch";
                if (id == 0 & Convert.ToInt16(numSwitch) >= 1)
                {
                    LogMessage("SetSwitchName " + id.ToString(), name);
                    SwitchName0 = name;
                    driverProfile.WriteValue(DriverProgId, SwitchName0ProfileName, SwitchName0.ToString(), "DC Jacks");
                }
                else if (id == 1 & Convert.ToInt16(numSwitch) >= 2)
                {
                    LogMessage("SetSwitchName " + id.ToString(), name);
                    SwitchName1 = name;
                    driverProfile.WriteValue(DriverProgId, SwitchName1ProfileName, SwitchName1.ToString(), "PWM 1 - Main");
                }
                else if (id == 2 & Convert.ToInt16(numSwitch) >= 3)
                {
                    LogMessage("SetSwitchName " + id.ToString(), name);
                    SwitchName2 = name;
                    driverProfile.WriteValue(DriverProgId, SwitchName2ProfileName, SwitchName2.ToString(), "PWM 2 - Guide");
                }
                else if (id == 3 & Convert.ToInt16(numSwitch) >= 4)
                {
                    LogMessage("SetSwitchName " + id.ToString(), name);
                    SwitchName3 = name;
                    driverProfile.WriteValue(DriverProgId, SwitchName3ProfileName, SwitchName3.ToString(), "Temperature sensor");
                }
                else if (id == 4 & Convert.ToInt16(numSwitch) >= 5)
                {
                    LogMessage("SetSwitchName " + id.ToString(), name);
                    SwitchName4 = name;
                    driverProfile.WriteValue(DriverProgId, SwitchName4ProfileName, SwitchName4.ToString(), "Humidity sensor");
                }
                else if (id == 5 & Convert.ToInt16(numSwitch) >= 6)
                {
                    LogMessage("SetSwitchName " + id.ToString(), name);
                    SwitchName5 = name;
                    driverProfile.WriteValue(DriverProgId, SwitchName5ProfileName, SwitchName5.ToString(), "Dew Point sensor");
                }
                else if (id == 6 & Convert.ToInt16(numSwitch) >= 7)
                {
                    LogMessage("SetSwitchName " + id.ToString(), name);
                    SwitchName6 = name;
                    driverProfile.WriteValue(DriverProgId, SwitchName6ProfileName, SwitchName6.ToString(), "Voltage sensor");
                }
                else if (id == 7 & Convert.ToInt16(numSwitch) >= 8)
                {
                    LogMessage("SetSwitchName " + id.ToString(), name);
                    SwitchName7 = name;
                    driverProfile.WriteValue(DriverProgId, SwitchName7ProfileName, SwitchName7.ToString(), "Current sensor");
                }
                else if (id == 8 & Convert.ToInt16(numSwitch) >= 9)
                {
                    LogMessage("SetSwitchName " + id.ToString(), name);
                    SwitchName8 = name;
                    driverProfile.WriteValue(DriverProgId, SwitchName8ProfileName, SwitchName8.ToString(), "Power sensor");
                }
                else if (id == 9 & Convert.ToInt16(numSwitch) >= 10)
                {
                    LogMessage("SetSwitchName " + id.ToString(), name);
                    SwitchName9 = name;
                    driverProfile.WriteValue(DriverProgId, SwitchName9ProfileName, SwitchName9.ToString(), "Total Power Consumption");
                }
                else
                {
                    LogMessage("SetSwitchName", $"SetSwitchName({id}) = {name} - not implemented");
                    throw new MethodNotImplementedException("SetSwitchName");
                }
            }
        }

        /// <summary>
        /// Gets the description of the specified switch device. This is to allow a fuller description of
        /// the device to be returned, for example for a tool tip.
        /// </summary>
        /// <param name="id">The device number (0 to <see cref="MaxSwitch"/> - 1)</param>
        /// <returns>
        /// String giving the device description.
        /// </returns>
        internal static string GetSwitchDescription(short id)
        {
            Validate("GetSwitchDescription", id);
            string s_GetSwitchDescription = "";
            if (id == 0 & Convert.ToInt16(numSwitch) >= 1)
            {
                s_GetSwitchDescription = "DC Jacks x4 12V";
            }
            else if (id == 1 & Convert.ToInt16(numSwitch) >= 2)
            {
                s_GetSwitchDescription = "PWM 1 Port 0%-100% in 20% steps";
            }
            else if (id == 2 & Convert.ToInt16(numSwitch) >= 3)
            {
                s_GetSwitchDescription = "PWM 2 Port 0%-100% in 20% steps";
            }
            else if (id == 3 & Convert.ToInt16(numSwitch) >= 4)
            {
                s_GetSwitchDescription = "Temperature in Celsius";
            }
            else if (id == 4 & Convert.ToInt16(numSwitch) >= 5)
            {
                s_GetSwitchDescription = "Humidity %";
            }
            else if (id == 5 & Convert.ToInt16(numSwitch) >= 6)
            {
                s_GetSwitchDescription = "Dew point calculation in Celsius";
            }
            else if (id == 6 & Convert.ToInt16(numSwitch) >= 7)
            {
                s_GetSwitchDescription = "Voltage at input in Volts";
            }
            else if (id == 7 & Convert.ToInt16(numSwitch) >= 8)
            {
                s_GetSwitchDescription = "Current used by system in Amps";
            }
            else if (id == 8 & Convert.ToInt16(numSwitch) >= 9)
            {
                s_GetSwitchDescription = "Momentary power usage in Watts";
            }
            else if (id == 9 & Convert.ToInt16(numSwitch) >= 10)
            {
                s_GetSwitchDescription = "Total power consumption since connected in Watts*Hour";
            }
            else
            { 
                LogMessage("GetSwitchDescription", $"GetSwitchDescription({id}) - not implemented");
            throw new MethodNotImplementedException("GetSwitchDescription");
            }
            return s_GetSwitchDescription;
        }

        /// <summary>
        /// Reports if the specified switch device can be written to, default true.
        /// This is false if the device cannot be written to, for example a limit switch or a sensor.
        /// </summary>
        /// <param name="id">The device number (0 to <see cref="MaxSwitch"/> - 1)</param>
        /// <returns>
        /// <c>true</c> if the device can be written to, otherwise <c>false</c>.
        /// </returns>
        internal static bool CanWrite(short id)
        {
            Validate("CanWrite", id);
            var PortCanWrite = default(bool);
            if (id == 0 & Convert.ToInt16(numSwitch) >= 1)
            {
                PortCanWrite = true; //Bool switch
            }
            else if (id == 1 & Convert.ToInt16(numSwitch) >= 2)
            {
                PortCanWrite = true; //Value switch
            }
            else if (id == 2 & Convert.ToInt16(numSwitch) >= 3)
            {
                PortCanWrite = true; //Value switch
            }
            else if (id == 3 & Convert.ToInt16(numSwitch) >= 4)
            {
                PortCanWrite = false; //Gauge
            }
            else if (id == 4 & Convert.ToInt16(numSwitch) >= 5)
            {
                PortCanWrite = false; //Gauge
            }
            else if (id == 5 & Convert.ToInt16(numSwitch) >= 6)
            {
                PortCanWrite = false; //Gauge
            }
            else if (id == 6 & Convert.ToInt16(numSwitch) >= 7)
            {
                PortCanWrite = false; //Gauge
            }
            else if (id == 7 & Convert.ToInt16(numSwitch) >= 8)
            {
                PortCanWrite = false; //Gauge
            }
            else if (id == 8 & Convert.ToInt16(numSwitch) >= 9)
            {
                PortCanWrite = false; //Gauge
            }
            else if (id == 9 & Convert.ToInt16(numSwitch) >= 10)
            {
                PortCanWrite = false; //Gauge
            }
            //tl.LogMessage("CanWrite to Port ", id.ToString(), PortCanWrite);
            LogMessage("CanWrite", $"CanWrite({id}): {PortCanWrite}");
            return PortCanWrite;
        }

        #region Boolean switch members

        /// <summary>
        /// Return the state of switch device id as a boolean
        /// </summary>
        /// <param name="id">The device number (0 to <see cref="MaxSwitch"/> - 1)</param>
        /// <returns>True or false</returns>
        internal static bool GetSwitch(short id)
        {
            Validate("GetSwitch", id);
            LogMessage("GetSwitch", $"GetSwitch({id}) - not implemented");
            throw new MethodNotImplementedException("GetSwitch");
        }

        /// <summary>
        /// Sets a switch controller device to the specified state, true or false.
        /// </summary>
        /// <param name="id">The device number (0 to <see cref="MaxSwitch"/> - 1)</param>
        /// <param name="state">The required control state</param>
        internal static void SetSwitch(short id, bool state)
        {
            Validate("SetSwitch", id);
            if (!CanWrite(id))
            {
                var str = $"SetSwitch({id}) - Cannot Write";
                LogMessage("SetSwitch", str);
                throw new MethodNotImplementedException(str);
            }
            LogMessage("SetSwitch", $"SetSwitch({id}) = {state} - not implemented");
            throw new MethodNotImplementedException("SetSwitch");
        }

        #endregion

        #region Analogue members

        /// <summary>
        /// Returns the maximum value for this switch device, this must be greater than <see cref="MinSwitchValue"/>.
        /// </summary>
        /// <param name="id">The device number (0 to <see cref="MaxSwitch"/> - 1)</param>
        /// <returns>The maximum value to which this device can be set or which a read only sensor will return.</returns>
        internal static double MaxSwitchValue(short id)
        {
            Validate("MaxSwitchValue", id);
            double MaxValue;
            if (id == 0 & Convert.ToInt16(numSwitch) >= 1)
            {
                MaxValue = 1;
            }
            else if (id == 1 & Convert.ToInt16(numSwitch) >= 2)
            {
                MaxValue = 100;
            }
            else if (id == 2 & Convert.ToInt16(numSwitch) >= 3)
            {
                MaxValue = 100;
            }
            else if (id == 3 & Convert.ToInt16(numSwitch) >= 4)
            {
                MaxValue = 100;
            }
            else if (id == 4 & Convert.ToInt16(numSwitch) >= 5)
            {
                MaxValue = 100;
            }
            else if (id == 5 & Convert.ToInt16(numSwitch) >= 6)
            {
                MaxValue = 100;
            }
            else if (id == 6 & Convert.ToInt16(numSwitch) >= 7)
            {
                MaxValue = 30;
            }
            else if (id == 7 & Convert.ToInt16(numSwitch) >= 8)
            {
                MaxValue = 10;
            }
            else if (id == 8 & Convert.ToInt16(numSwitch) >= 9)
            {
                MaxValue = 250;
            }
            else if (id == 9 & Convert.ToInt16(numSwitch) >= 10)
            {
                MaxValue = 2500;
            }
            else
            {
                LogMessage("MaxSwitchValue", $"MaxSwitchValue({id}) - not implemented");
                throw new MethodNotImplementedException("MaxSwitchValue");
            }
            LogMessage("MaxSwitchValue ", id.ToString(), MaxValue);
            return MaxValue;
        }


        /// <summary>
        /// Returns the minimum value for this switch device, this must be less than <see cref="MaxSwitchValue"/>
        /// </summary>
        /// <param name="id">The device number (0 to <see cref="MaxSwitch"/> - 1)</param>
        /// <returns>The minimum value to which this device can be set or which a read only sensor will return.</returns>
        internal static double MinSwitchValue(short id)
        {
            Validate("MinSwitchValue", id);
            double MinValue;
            if (id == 0 & Convert.ToInt16(numSwitch) >= 1)
            {
                MinValue = 0;
            }
            else if (id == 1 & Convert.ToInt16(numSwitch) >= 2)
            {
                MinValue = 0;
            }
            else if (id == 2 & Convert.ToInt16(numSwitch) >= 3)
            {
                MinValue = 0;
            }
            else if (id == 3 & Convert.ToInt16(numSwitch) >= 4)
            {
                MinValue = -50;
            }
            else if (id == 4 & Convert.ToInt16(numSwitch) >= 5)
            {
                MinValue = 0;
            }
            else if (id == 5 & Convert.ToInt16(numSwitch) >= 6)
            {
                MinValue = -50;
            }
            else if (id == 6 & Convert.ToInt16(numSwitch) >= 7)
            {
                MinValue = 0;
            }
            else if (id == 7 & Convert.ToInt16(numSwitch) >= 8)
            {
                MinValue = 0;
            }
            else if (id == 8 & Convert.ToInt16(numSwitch) >= 9)
            {
                MinValue = 0;
            }
            else if (id == 9 & Convert.ToInt16(numSwitch) >= 10)
            {
                MinValue = 0;
            }
            else
            {
                LogMessage("MinSwitchValue", $"MinSwitchValue({id}) - not implemented");
                throw new MethodNotImplementedException("MinSwitchValue");
            }
            LogMessage("MinSwitchValue ", id.ToString(), MinValue);
            return MinValue;
        }

        /// <summary>
        /// Returns the step size that this device supports (the difference between successive values of the device).
        /// </summary>
        /// <param name="id">The device number (0 to <see cref="MaxSwitch"/> - 1)</param>
        /// <returns>The step size for this device.</returns>
        internal static double SwitchStep(short id)
        {
            Validate("SwitchStep", id);
            double SwitchStepValue;
            if (id == 0 & Convert.ToInt16(numSwitch) >= 1)
            {
                SwitchStepValue = 1;
            }
            else if (id == 1 & Convert.ToInt16(numSwitch) >= 2)
            {
                SwitchStepValue = 5;
            }
            else if (id == 2 & Convert.ToInt16(numSwitch) >= 3)
            {
                SwitchStepValue = 5;
            }
            else if (id == 3 & Convert.ToInt16(numSwitch) >= 4)
            {
                SwitchStepValue = 0.01;
            }
            else if (id == 4 & Convert.ToInt16(numSwitch) >= 5)
            {
                SwitchStepValue = 0.1;
            }
            else if (id == 5 & Convert.ToInt16(numSwitch) >= 6)
            {
                SwitchStepValue = 0.01;
            }
            else if (id == 6 & Convert.ToInt16(numSwitch) >= 7)
            {
                SwitchStepValue = 0.01;
            }
            else if (id == 7 & Convert.ToInt16(numSwitch) >= 8)
            {
                SwitchStepValue = 0.01;
            }
            else if (id == 8 & Convert.ToInt16(numSwitch) >= 9)
            {
                SwitchStepValue = 0.01;
            }
            else if (id == 9 & Convert.ToInt16(numSwitch) >= 10)
            {
                SwitchStepValue = 0.01;
            }
            else
            {
                LogMessage("SwitchStep", $"SwitchStep({id}) - not implemented");
                throw new MethodNotImplementedException("SwitchStep");
            }
            LogMessage("SwitchStep ", id.ToString(), SwitchStepValue);
            return SwitchStepValue;
        }

        /// <summary>
        /// Returns the value for switch device id as a double
        /// </summary>
        /// <param name="id">The device number (0 to <see cref="MaxSwitch"/> - 1)</param>
        /// <returns>The value for this switch, this is expected to be between <see cref="MinSwitchValue"/> and
        /// <see cref="MaxSwitchValue"/>.</returns>
        internal static double GetSwitchValue(short id)
        {
            Validate("GetSwitchValue", id);
            string SerialRead = "";
            double ReturnValue;
            if (id == 1 & Convert.ToInt16(numSwitch) >= 2)
            {
                objSerial.Transmit("GETSTATUSPWM1#");
                SerialRead = objSerial.ReceiveTerminated("#");
            }
            else if (id == 2 & Convert.ToInt16(numSwitch) >= 3)
            {
                objSerial.Transmit("GETSTATUSPWM2#");
                SerialRead = objSerial.ReceiveTerminated("#");
            }
            else if (id == 3 & Convert.ToInt16(numSwitch) >= 4)
            {
                objSerial.Transmit("GETTEMPERATURE#");
                SerialRead = objSerial.ReceiveTerminated("#");
            }
            else if (id == 4 & Convert.ToInt16(numSwitch) >= 5)
            {
                objSerial.Transmit("GETHUMIDITY#");
                SerialRead = objSerial.ReceiveTerminated("#");
            }
            else if (id == 5 & Convert.ToInt16(numSwitch) >= 6)
            {
                objSerial.Transmit("GETDEWPOINT#");
                SerialRead = objSerial.ReceiveTerminated("#");
            }
            else if (id == 6 & Convert.ToInt16(numSwitch) >= 7)
            {
                objSerial.Transmit("GETVOLTAGE#");
                SerialRead = objSerial.ReceiveTerminated("#");
            }
            else if (id == 7 & Convert.ToInt16(numSwitch) >= 8)
            {
                objSerial.Transmit("GETCURRENT#");
                SerialRead = objSerial.ReceiveTerminated("#");
            }
            else if (id == 8 & Convert.ToInt16(numSwitch) >= 9)
            {
                objSerial.Transmit("GETPOWER#");
                SerialRead = objSerial.ReceiveTerminated("#");
            }
            else if (id == 9 & Convert.ToInt16(numSwitch) >= 10)
            {
                objSerial.Transmit("GETUSAGE#");
                SerialRead = objSerial.ReceiveTerminated("#");
            }
            else
            {
                LogMessage("GetSwitchValue", $"GetSwitchValue({id}) - not implemented");
                throw new MethodNotImplementedException("GetSwitchValue");
            }
            
            ReturnValue = Convert.ToDouble(SerialRead.Replace( "#", ""));
            LogMessage("GetSwitchValue ", id.ToString(), ReturnValue);
            return ReturnValue;
        }

        /// <summary>
        /// Set the value for this device as a double.
        /// </summary>
        /// <param name="id">The device number (0 to <see cref="MaxSwitch"/> - 1)</param>
        /// <param name="value">The value to be set, between <see cref="MinSwitchValue"/> and <see cref="MaxSwitchValue"/></param>
        internal static void SetSwitchValue(short id, double value)
        {
            Validate("SetSwitchValue", id, value);
            if (!CanWrite(id))
            {
                string SwitchValue = "";
                using (var driverProfile = new Profile())
                {
                    driverProfile.DeviceType = "Switch";
                    if (id == 1 & Convert.ToInt16(numSwitch) >= 2)
                    {
                        LogMessage("SetSwitchValue ", id.ToString(), value);
                        SwitchValue = "SETSTATUSPWM1_" + value.ToString() + "#";
                        objSerial.Transmit(SwitchValue);
                        objSerial.ReceiveTerminated("#");
                    }
                    else if (id == 2 & Convert.ToInt16(numSwitch) >= 3)
                    {
                        LogMessage("SetSwitchValue ", id.ToString(), value);
                        SwitchValue = "SETSTATUSPWM2_" + value.ToString() + "#";
                        objSerial.Transmit(SwitchValue);
                        objSerial.ReceiveTerminated("#");
                    }
                    else if (value < MinSwitchValue(id) | value > MaxSwitchValue(id))
                    {
                        throw new InvalidValueException("", value.ToString(), string.Format("{0} to {1}", MinSwitchValue(id), MaxSwitchValue(id)));
                    }
                    else
                    {
                        LogMessage("SetSwitchValue", $"SetSwitchValue({id}) = {value} - not implemented");
                        throw new MethodNotImplementedException("SetSwitchValue");
                        
                    }
                }
            }
            else
            {
                LogMessage("SetSwitchValue", $"SetSwitchValue({id}) - Cannot write");
                throw new ASCOM.MethodNotImplementedException($"SetSwitchValue({id}) - Cannot write");
            }
        }

        #endregion
        #endregion

        #region Private methods

        /// <summary>
        /// Checks that the switch id is in range and throws an InvalidValueException if it isn't
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="id">The id.</param>
        private static void Validate(string message, short id)
        {
            if (id < 0 || id >= Convert.ToInt16(numSwitch))
            {
                LogMessage(message, string.Format("Switch {0} not available, range is 0 to {1}", id, Convert.ToInt16(numSwitch) - 1));
                throw new InvalidValueException(message, id.ToString(), string.Format("0 to {0}", Convert.ToInt16(numSwitch) - 1));
            }
        }

        /// <summary>
        /// Checks that the switch id and value are in range and throws an
        /// InvalidValueException if they are not.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="id">The id.</param>
        /// <param name="value">The value.</param>
        private static void Validate(string message, short id, double value)
        {
            Validate(message, id);
            var min = MinSwitchValue(id);
            var max = MaxSwitchValue(id);
            if (value < min || value > max)
            {
                LogMessage(message, string.Format("Value {1} for Switch {0} is out of the allowed range {2} to {3}", id, value, min, max));
                throw new InvalidValueException(message, value.ToString(), string.Format("Switch({0}) range {1} to {2}", id, min, max));
            }
        }

        #endregion

        #region Private properties and methods
        // Useful methods that can be used as required to help with driver development

        /// <summary>
        /// Returns true if there is a valid connection to the driver hardware
        /// </summary>
        private static bool IsConnected
        {
            get
            {
                // TODO check that the driver hardware connection exists and is connected to the hardware
                return connectedState;
            }
        }

        /// <summary>
        /// Use this function to throw an exception if we aren't connected to the hardware
        /// </summary>
        /// <param name="message"></param>
        private static void CheckConnected(string message)
        {
            if (!IsConnected)
            {
                throw new NotConnectedException(message);
            }
        }

        /// <summary>
        /// Read the device configuration from the ASCOM Profile store
        /// </summary>
        internal static void ReadProfile()
        {
            using (Profile driverProfile = new Profile())
            {
                driverProfile.DeviceType = "Switch";
                tl.Enabled = Convert.ToBoolean(driverProfile.GetValue(DriverProgId, traceStateProfileName, string.Empty, traceStateDefault));
                comPort = driverProfile.GetValue(DriverProgId, comPortProfileName, string.Empty, comPortDefault);
                ConnectionDelay = driverProfile.GetValue(DriverProgId, ConnectionDelayProfileName, string.Empty, ConnectionDelayDefault);
                numSwitch = driverProfile.GetValue(DriverProgId, numSwitchProfileName, string.Empty, numSwitchDefault);
                SwitchName0 = driverProfile.GetValue(DriverProgId, SwitchName0ProfileName, "DC Jacks", SwitchName0Default);
                SwitchState0 = driverProfile.GetValue(DriverProgId, SwitchState0ProfileName, "DC Jacks", SwitchState0Default);
                SwitchName1 = driverProfile.GetValue(DriverProgId, SwitchName1ProfileName, "PWM 1 - Main", SwitchName1Default);
                SwitchState1 = driverProfile.GetValue(DriverProgId, SwitchState1ProfileName, "PWM 1 - Main", SwitchState1Default);
                SwitchName2 = driverProfile.GetValue(DriverProgId, SwitchName2ProfileName, "PWM 2 - Guide", SwitchName2Default);
                SwitchState2 = driverProfile.GetValue(DriverProgId, SwitchState2ProfileName, "PWM 2 - Guide", SwitchState2Default);
                SwitchName3 = driverProfile.GetValue(DriverProgId, SwitchName3ProfileName, "Temperature sensor", SwitchName3Default);
                SwitchState3 = driverProfile.GetValue(DriverProgId, SwitchState3ProfileName, "Temperature sensor", SwitchState3Default);
                SwitchName4 = driverProfile.GetValue(DriverProgId, SwitchName4ProfileName, "Humidity sensor", SwitchName4Default);
                SwitchState4 = driverProfile.GetValue(DriverProgId, SwitchState4ProfileName, "Humidity sensor", SwitchState4Default);
                SwitchName5 = driverProfile.GetValue(DriverProgId, SwitchName5ProfileName, "Dew point sensor", SwitchName5Default);
                SwitchState5 = driverProfile.GetValue(DriverProgId, SwitchState5ProfileName, "Dew point sensor", SwitchState5Default);
                SwitchName6 = driverProfile.GetValue(DriverProgId, SwitchName6ProfileName, "Voltage sensor", SwitchName6Default);
                SwitchState6 = driverProfile.GetValue(DriverProgId, SwitchState6ProfileName, "Voltage sensor", SwitchState6Default);
                SwitchName7 = driverProfile.GetValue(DriverProgId, SwitchName7ProfileName, "Current sensor", SwitchName7Default);
                SwitchState7 = driverProfile.GetValue(DriverProgId, SwitchState7ProfileName, "Current sensor", SwitchState7Default);
                SwitchName8 = driverProfile.GetValue(DriverProgId, SwitchName8ProfileName, "Power sensor", SwitchName8Default);
                SwitchState8 = driverProfile.GetValue(DriverProgId, SwitchState8ProfileName, "Power sensor", SwitchState8Default);
                SwitchName9 = driverProfile.GetValue(DriverProgId, SwitchName9ProfileName, "Total power usage", SwitchName9Default);
                SwitchState9 = driverProfile.GetValue(DriverProgId, SwitchState9ProfileName, "Total power usage", SwitchState9Default);
            }
        }

        /// <summary>
        /// Write the device configuration to the  ASCOM  Profile store
        /// </summary>
        internal static void WriteProfile()
        {
            using (Profile driverProfile = new Profile())
            {
                driverProfile.DeviceType = "Switch";
                driverProfile.WriteValue(DriverProgId, traceStateProfileName, tl.Enabled.ToString());
                if (comPort != null)
                {
                    driverProfile.WriteValue(DriverProgId, comPortProfileName, comPort.ToString());
                }
                driverProfile.WriteValue(DriverProgId, ConnectionDelayProfileName, ConnectionDelay.ToString());
                driverProfile.WriteValue(DriverProgId, numSwitchProfileName, numSwitch.ToString());
                driverProfile.WriteValue(DriverProgId, SwitchName0ProfileName, SwitchName0.ToString(), "DC Jacks");
                driverProfile.WriteValue(DriverProgId, SwitchState0ProfileName, SwitchState0.ToString(), "DC Jacks");
                driverProfile.WriteValue(DriverProgId, SwitchName1ProfileName, SwitchName1.ToString(), "PWM 1 - Main");
                driverProfile.WriteValue(DriverProgId, SwitchState1ProfileName, SwitchState1.ToString(), "PWM 1 - Main");
                driverProfile.WriteValue(DriverProgId, SwitchName2ProfileName, SwitchName2.ToString(), "PWM 2 - Guide");
                driverProfile.WriteValue(DriverProgId, SwitchState2ProfileName, SwitchState2.ToString(), "PWM 2 - Guide");
                driverProfile.WriteValue(DriverProgId, SwitchName3ProfileName, SwitchName3.ToString(), "Temperature sensor");
                driverProfile.WriteValue(DriverProgId, SwitchState3ProfileName, SwitchState3.ToString(), "Temperature sensor");
                driverProfile.WriteValue(DriverProgId, SwitchName4ProfileName, SwitchName4.ToString(), "Humidity sensor");
                driverProfile.WriteValue(DriverProgId, SwitchState4ProfileName, SwitchState4.ToString(), "Humidity sensor");
                driverProfile.WriteValue(DriverProgId, SwitchName5ProfileName, SwitchName5.ToString(), "Dew point sensor");
                driverProfile.WriteValue(DriverProgId, SwitchState5ProfileName, SwitchState5.ToString(), "Dew point sensor");
                driverProfile.WriteValue(DriverProgId, SwitchName6ProfileName, SwitchName6.ToString(), "Voltage sensor");
                driverProfile.WriteValue(DriverProgId, SwitchState6ProfileName, SwitchState6.ToString(), "Voltage sensor");
                driverProfile.WriteValue(DriverProgId, SwitchName7ProfileName, SwitchName7.ToString(), "Current sensor");
                driverProfile.WriteValue(DriverProgId, SwitchState7ProfileName, SwitchState7.ToString(), "Current sensor");
                driverProfile.WriteValue(DriverProgId, SwitchName8ProfileName, SwitchName8.ToString(), "Power sensor");
                driverProfile.WriteValue(DriverProgId, SwitchState8ProfileName, SwitchState8.ToString(), "Power sensor");
                driverProfile.WriteValue(DriverProgId, SwitchName9ProfileName, SwitchName9.ToString(), "Total power usage");
                driverProfile.WriteValue(DriverProgId, SwitchState9ProfileName, SwitchState9.ToString(), "Total power usage");
            }
        }

        /// <summary>
        /// Log helper function that takes identifier and message strings
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="message"></param>
        internal static void LogMessage(string identifier, string message)
        {
            tl.LogMessageCrLf(identifier, message);
        }

        /// <summary>
        /// Log helper function that takes formatted strings and arguments
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="message"></param>
        /// <param name="args"></param>
        internal static void LogMessage(string identifier, string message, params object[] args)
        {
            var msg = string.Format(message, args);
            LogMessage(identifier, msg);
        }
        #endregion
    }
}

