/*** FTD2XX_NET.cs
**
** Copyright © 2009-2012 Future Technology Devices International Limited
** (с) 2013  ИКИ РАН
 *
** C# Source file for .NET wrapper of the Windows FTD2XX.dll API calls.
** Main module
**
** Author: FTDI, Семенов Александр
** Project: CDM Windows Driver Package
** Module: FTD2XX_NET Managed Wrapper
** Requires: 
** Comments:
**
** History:
**  1.0.0	-	Initial version
**  1.0.12	-	Included support for the FT232H device.
**  1.0.14	-	Included Support for the X-Series of devices.
 *  1.1.00  -   Убрал из модулей Windows.Forms - не нужно вызывать messagebox.show - мы будем в случае ошибки логгировать в текстовый файл, выставляя признак в объекте FTDI о том, что произошла ошибка
 *              Проверить все комментарии типа //! и /*! - закоментирована важная информация, в дальнейшем нужно будет ее учесть
 *              Оставил только основные функции работы с FTDI, всю работу с EEPROM убрал
 *  1.2.00 (27.11.2013) - перенес инициализацию делегатов в конструктор, чтобы их вызов не занимал лишнее время
**
*/
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;

namespace FTD2XXNET
{
	/// <summary>
	/// Class wrapper for FTD2XX.DLL
	/// </summary>
	public class FTDICustom
	{
        tFT_OpenEx FT_OpenEx;
        tFT_Close  FT_Close;
        tFT_Read FT_Read;
        tFT_Write FT_Write;
        tFT_GetQueueStatus FT_GetQueueStatus;
        tFT_GetStatus FT_GetStatus;
        tFT_ResetDevice FT_ResetDevice;
        tFT_ResetPort FT_ResetPort;
        tFT_CyclePort FT_CyclePort;
        tFT_Rescan FT_Rescan;
        tFT_Reload FT_Reload;
        tFT_Purge FT_Purge;
        tFT_SetTimeouts FT_SetTimeouts;
        tFT_GetDriverVersion FT_GetDriverVersion;
        tFT_GetLibraryVersion FT_GetLibraryVersion;
        tFT_SetBitMode FT_SetBitMode;
        tFT_SetLatencyTimer FT_SetLatencyTimer;
        tFT_GetLatencyTimer FT_GetLatencyTimer;
        tFT_SetUSBParameters FT_SetUSBParameters;

        #region CONSTRUCTOR_DESTRUCTOR
		// constructor
		/// <summary>
		/// Constructor for the FTDI class.
		/// </summary>
		public FTDICustom()
		{
			// If FTD2XX.DLL is NOT loaded already, load it
			if (hFTD2XXDLL == IntPtr.Zero)
			{
				// Load our FTD2XX.DLL library
				hFTD2XXDLL = LoadLibrary(@"FTD2XX.DLL");
				if (hFTD2XXDLL == IntPtr.Zero)
				{
					// Failed to load our FTD2XX.DLL library from System32 or the application directory
					// Try the same directory that this FTD2XX_NET DLL is in
					//!MessageBox.Show("Attempting to load FTD2XX.DLL from:\n" + Path.GetDirectoryName(GetType().Assembly.Location));
					hFTD2XXDLL = LoadLibrary(@Path.GetDirectoryName(GetType().Assembly.Location) + "\\FTD2XX.DLL");
				}
			}

			// If we have succesfully loaded the library, get the function pointers set up
			if (hFTD2XXDLL != IntPtr.Zero)
			{
				// Set up our function pointers for use through our exported methods
//                pFT_Open = GetProcAddress(hFTD2XXDLL, "FT_Open");
                pFT_OpenEx = GetProcAddress(hFTD2XXDLL, "FT_OpenEx");
                if (pFT_OpenEx != IntPtr.Zero)
                {
                    FT_OpenEx = (tFT_OpenEx)Marshal.GetDelegateForFunctionPointer(pFT_OpenEx, typeof(tFT_OpenEx));
                }
                else
                {

                }

                pFT_Close = GetProcAddress(hFTD2XXDLL, "FT_Close");
                FT_Close = (tFT_Close)Marshal.GetDelegateForFunctionPointer(pFT_Close, typeof(tFT_Close));

                pFT_Read = GetProcAddress(hFTD2XXDLL, "FT_Read");
                FT_Read = (tFT_Read)Marshal.GetDelegateForFunctionPointer(pFT_Read, typeof(tFT_Read));

                pFT_Write = GetProcAddress(hFTD2XXDLL, "FT_Write");
                FT_Write = (tFT_Write)Marshal.GetDelegateForFunctionPointer(pFT_Write, typeof(tFT_Write));

                pFT_GetQueueStatus = GetProcAddress(hFTD2XXDLL, "FT_GetQueueStatus");
                FT_GetQueueStatus = (tFT_GetQueueStatus)Marshal.GetDelegateForFunctionPointer(pFT_GetQueueStatus, typeof(tFT_GetQueueStatus));

                pFT_GetStatus = GetProcAddress(hFTD2XXDLL, "FT_GetStatus");
                FT_GetStatus = (tFT_GetStatus)Marshal.GetDelegateForFunctionPointer(pFT_GetStatus, typeof(tFT_GetStatus));

                pFT_ResetDevice = GetProcAddress(hFTD2XXDLL, "FT_ResetDevice");
                FT_ResetDevice = (tFT_ResetDevice)Marshal.GetDelegateForFunctionPointer(pFT_ResetDevice, typeof(tFT_ResetDevice));

                pFT_ResetPort = GetProcAddress(hFTD2XXDLL, "FT_ResetPort");
                FT_ResetPort = (tFT_ResetPort)Marshal.GetDelegateForFunctionPointer(pFT_ResetPort, typeof(tFT_ResetPort));

                pFT_CyclePort = GetProcAddress(hFTD2XXDLL, "FT_CyclePort");
                FT_CyclePort = (tFT_CyclePort)Marshal.GetDelegateForFunctionPointer(pFT_CyclePort, typeof(tFT_CyclePort));

                pFT_Rescan = GetProcAddress(hFTD2XXDLL, "FT_Rescan");
                FT_Rescan = (tFT_Rescan)Marshal.GetDelegateForFunctionPointer(pFT_Rescan, typeof(tFT_Rescan));

                pFT_Reload = GetProcAddress(hFTD2XXDLL, "FT_Reload");
                FT_Reload = (tFT_Reload)Marshal.GetDelegateForFunctionPointer(pFT_Reload, typeof(tFT_Reload));

                pFT_Purge = GetProcAddress(hFTD2XXDLL, "FT_Purge");
                FT_Purge = (tFT_Purge)Marshal.GetDelegateForFunctionPointer(pFT_Purge, typeof(tFT_Purge));

                pFT_SetTimeouts = GetProcAddress(hFTD2XXDLL, "FT_SetTimeouts");
                FT_SetTimeouts = (tFT_SetTimeouts)Marshal.GetDelegateForFunctionPointer(pFT_SetTimeouts, typeof(tFT_SetTimeouts));

                pFT_GetDriverVersion = GetProcAddress(hFTD2XXDLL, "FT_GetDriverVersion");
                FT_GetDriverVersion = (tFT_GetDriverVersion)Marshal.GetDelegateForFunctionPointer(pFT_GetDriverVersion, typeof(tFT_GetDriverVersion));

                pFT_GetLibraryVersion = GetProcAddress(hFTD2XXDLL, "FT_GetLibraryVersion");
                FT_GetLibraryVersion = (tFT_GetLibraryVersion)Marshal.GetDelegateForFunctionPointer(pFT_GetLibraryVersion, typeof(tFT_GetLibraryVersion));

                pFT_SetDeadmanTimeout = GetProcAddress(hFTD2XXDLL, "FT_SetDeadmanTimeout");

                pFT_SetBitMode = GetProcAddress(hFTD2XXDLL, "FT_SetBitMode");
                FT_SetBitMode = (tFT_SetBitMode)Marshal.GetDelegateForFunctionPointer(pFT_SetBitMode, typeof(tFT_SetBitMode));
                
                pFT_SetLatencyTimer = GetProcAddress(hFTD2XXDLL, "FT_SetLatencyTimer");
                FT_SetLatencyTimer = (tFT_SetLatencyTimer)Marshal.GetDelegateForFunctionPointer(pFT_SetLatencyTimer, typeof(tFT_SetLatencyTimer));

                pFT_GetLatencyTimer = GetProcAddress(hFTD2XXDLL, "FT_GetLatencyTimer");
                FT_GetLatencyTimer = (tFT_GetLatencyTimer)Marshal.GetDelegateForFunctionPointer(pFT_GetLatencyTimer, typeof(tFT_GetLatencyTimer));

                pFT_SetUSBParameters = GetProcAddress(hFTD2XXDLL, "FT_SetUSBParameters");
                FT_SetUSBParameters = (tFT_SetUSBParameters)Marshal.GetDelegateForFunctionPointer(pFT_SetUSBParameters, typeof(tFT_SetUSBParameters));
            }
			else
			{
				// Failed to load our DLL - alert the user
				//!MessageBox.Show("Failed to load FTD2XX.DLL.  Are the FTDI drivers installed?");
			}
		}

		/// <summary>
		/// Destructor for the FTDI class.
		/// </summary>
		~FTDICustom()
		{
			// FreeLibrary here - we should only do this if we are completely finished
			FreeLibrary(hFTD2XXDLL);
			hFTD2XXDLL = IntPtr.Zero;
		}
		#endregion

		#region LOAD_LIBRARIES
		/// <summary>
		/// Built-in Windows API functions to allow us to dynamically load our own DLL.
		/// Will allow us to use old versions of the DLL that do not have all of these functions available.
		/// </summary>
		[DllImport("kernel32.dll")]
		private static extern IntPtr LoadLibrary(string dllToLoad);
		[DllImport("kernel32.dll")]
		private static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);
		[DllImport("kernel32.dll")]
		private static extern bool FreeLibrary(IntPtr hModule);
		#endregion

		#region DELEGATES
		// Definitions for FTD2XX functions
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_Open(UInt32 index, ref IntPtr ftHandle);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_OpenEx(string devstring, UInt32 dwFlags, ref IntPtr ftHandle);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_Close(IntPtr ftHandle);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_Read(IntPtr ftHandle, byte[] lpBuffer, Int32 dwBytesToRead, ref Int32 lpdwBytesReturned);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_Write(IntPtr ftHandle, byte[] lpBuffer, UInt32 dwBytesToWrite, ref UInt32 lpdwBytesWritten);

		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_GetQueueStatus(IntPtr ftHandle, ref Int32 lpdwAmountInRxQueue);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_GetStatus(IntPtr ftHandle, ref UInt32 lpdwAmountInRxQueue, ref UInt32 lpdwAmountInTxQueue, ref UInt32 lpdwEventStatus);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_ResetDevice(IntPtr ftHandle);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_ResetPort(IntPtr ftHandle);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_CyclePort(IntPtr ftHandle);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_Rescan();
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_Reload(UInt16 wVID, UInt16 wPID);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_Purge(IntPtr ftHandle, UInt32 dwMask);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_SetTimeouts(IntPtr ftHandle, UInt32 dwReadTimeout, UInt32 dwWriteTimeout);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_GetDriverVersion(IntPtr ftHandle, ref UInt32 lpdwDriverVersion);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_GetLibraryVersion(ref UInt32 lpdwLibraryVersion);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_SetDeadmanTimeout(IntPtr ftHandle, UInt32 dwDeadmanTimeout);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate FT_STATUS tFT_SetBitMode(IntPtr ftHandle, byte ucMask, byte ucMode);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_SetLatencyTimer(IntPtr ftHandle, byte ucLatency);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_GetLatencyTimer(IntPtr ftHandle, ref byte ucLatency);
		[UnmanagedFunctionPointer(CallingConvention.StdCall)]
		private delegate FT_STATUS tFT_SetUSBParameters(IntPtr ftHandle, UInt32 dwInTransferSize, UInt32 dwOutTransferSize);
		#endregion

		#region CONSTANT_VALUES
		// Constants for FT_STATUS
		/// <summary>
		/// Status values for FTDI devices.
		/// </summary>
		public enum FT_STATUS
		{
			/// <summary>
			/// Status OK
			/// </summary>
			FT_OK = 0,
			/// <summary>
			/// The device handle is invalid
			/// </summary>
			FT_INVALID_HANDLE,
			/// <summary>
			/// Device not found
			/// </summary>
			FT_DEVICE_NOT_FOUND,
			/// <summary>
			/// Device is not open
			/// </summary>
			FT_DEVICE_NOT_OPENED,
			/// <summary>
			/// IO error
			/// </summary>
			FT_IO_ERROR,
			/// <summary>
			/// Insufficient resources
			/// </summary>
			FT_INSUFFICIENT_RESOURCES,
			/// <summary>
			/// A parameter was invalid
			/// </summary>
			FT_INVALID_PARAMETER,
			/// <summary>
			/// The requested baud rate is invalid
			/// </summary>
			FT_INVALID_BAUD_RATE,
			/// <summary>
			/// Device not opened for erase
			/// </summary>
			FT_DEVICE_NOT_OPENED_FOR_ERASE,
			/// <summary>
			/// Device not poened for write
			/// </summary>
			FT_DEVICE_NOT_OPENED_FOR_WRITE,
			/// <summary>
			/// Failed to write to device
			/// </summary>
			FT_FAILED_TO_WRITE_DEVICE,
			/// <summary>
			/// Failed to read the device EEPROM
			/// </summary>
			FT_EEPROM_READ_FAILED,
			/// <summary>
			/// Failed to write the device EEPROM
			/// </summary>
			FT_EEPROM_WRITE_FAILED,
			/// <summary>
			/// Failed to erase the device EEPROM
			/// </summary>
			FT_EEPROM_ERASE_FAILED,
			/// <summary>
			/// An EEPROM is not fitted to the device
			/// </summary>
			FT_EEPROM_NOT_PRESENT,
			/// <summary>
			/// Device EEPROM is blank
			/// </summary>
			FT_EEPROM_NOT_PROGRAMMED,
			/// <summary>
			/// Invalid arguments
			/// </summary>
			FT_INVALID_ARGS,
			/// <summary>
			/// An other error has occurred
			/// </summary>
			FT_OTHER_ERROR
		};

		// Constants for other error states internal to this class library
		/// <summary>
		/// Error states not supported by FTD2XX DLL.
		/// </summary>
		private enum FT_ERROR
		{
			FT_NO_ERROR = 0,
			FT_INCORRECT_DEVICE,
			FT_INVALID_BITMODE,
			FT_BUFFER_SIZE
		};

		// Flags for FT_OpenEx
		private const UInt32 FT_OPEN_BY_SERIAL_NUMBER	= 0x00000001;
		private const UInt32 FT_OPEN_BY_DESCRIPTION		= 0x00000002;
		private const UInt32 FT_OPEN_BY_LOCATION		= 0x00000004;

        // Flow Control
		/// <summary>
		/// Permitted flow control values for FTDI devices
		/// </summary>
		public class FT_FLOW_CONTROL
		{
			/// <summary>
			/// No flow control
			/// </summary>
			public const UInt16 FT_FLOW_NONE		= 0x0000;
			/// <summary>
			/// RTS/CTS flow control
			/// </summary>
			public const UInt16 FT_FLOW_RTS_CTS		= 0x0100;
			/// <summary>
			/// DTR/DSR flow control
			/// </summary>
			public const UInt16 FT_FLOW_DTR_DSR		= 0x0200;
			/// <summary>
			/// Xon/Xoff flow control
			/// </summary>
			public const UInt16 FT_FLOW_XON_XOFF	= 0x0400;
		}

		// Purge Rx and Tx buffers
		/// <summary>
		/// Purge buffer constant definitions
		/// </summary>
		public class FT_PURGE
		{
			/// <summary>
			/// Purge Rx buffer
			/// </summary>
			public const byte FT_PURGE_RX = 0x01;
			/// <summary>
			/// Purge Tx buffer
			/// </summary>
			public const byte FT_PURGE_TX = 0x02;
		}

        // Bit modes
		/// <summary>
		/// Permitted bit mode values for FTDI devices.  For use with SetBitMode
		/// </summary>
		public class FT_BIT_MODES
		{
			/// <summary>
			/// Reset bit mode
			/// </summary>
			public const byte FT_BIT_MODE_RESET			= 0x00;
			/// <summary>
			/// Asynchronous bit-bang mode
			/// </summary>
			public const byte FT_BIT_MODE_ASYNC_BITBANG	= 0x01;
			/// <summary>
			/// MPSSE bit mode - only available on FT2232, FT2232H, FT4232H and FT232H
			/// </summary>
			public const byte FT_BIT_MODE_MPSSE			= 0x02;
			/// <summary>
			/// Synchronous bit-bang mode
			/// </summary>
			public const byte FT_BIT_MODE_SYNC_BITBANG	= 0x04;
			/// <summary>
			/// MCU host bus emulation mode - only available on FT2232, FT2232H, FT4232H and FT232H
			/// </summary>
			public const byte FT_BIT_MODE_MCU_HOST		= 0x08;
			/// <summary>
			/// Fast opto-isolated serial mode - only available on FT2232, FT2232H, FT4232H and FT232H
			/// </summary>
			public const byte FT_BIT_MODE_FAST_SERIAL	= 0x10;
			/// <summary>
			/// CBUS bit-bang mode - only available on FT232R and FT232H
			/// </summary>
			public const byte FT_BIT_MODE_CBUS_BITBANG	= 0x20;
			/// <summary>
			/// Single channel synchronous 245 FIFO mode - only available on FT2232H channel A and FT232H
			/// </summary>
			public const byte FT_BIT_MODE_SYNC_FIFO		= 0x40;
		}

        // Flag values for FT_GetDeviceInfoDetail and FT_GetDeviceInfo
		/// <summary>
		/// Flags that provide information on the FTDI device state
		/// </summary>
		public class FT_FLAGS
		{
			/// <summary>
			/// Indicates that the device is open
			/// </summary>
			public const UInt32 FT_FLAGS_OPENED		= 0x00000001;
			/// <summary>
			/// Indicates that the device is enumerated as a hi-speed USB device
			/// </summary>
			public const UInt32 FT_FLAGS_HISPEED	= 0x00000002;
		}

		// Valid drive current values for FT2232H, FT4232H and FT232H devices
		/// <summary>
		/// Valid values for drive current options on FT2232H, FT4232H and FT232H devices.
		/// </summary>
		public class FT_DRIVE_CURRENT
		{
			/// <summary>
			/// 4mA drive current
			/// </summary>
			public const byte FT_DRIVE_CURRENT_4MA	= 4;
			/// <summary>
			/// 8mA drive current
			/// </summary>
			public const byte FT_DRIVE_CURRENT_8MA	= 8;
			/// <summary>
			/// 12mA drive current
			/// </summary>
			public const byte FT_DRIVE_CURRENT_12MA	= 12;
			/// <summary>
			/// 16mA drive current
			/// </summary>
			public const byte FT_DRIVE_CURRENT_16MA	= 16;
		}

		// Device type identifiers for FT_GetDeviceInfoDetail and FT_GetDeviceInfo
		/// <summary>
		/// List of FTDI device types
		/// </summary>
		public enum FT_DEVICE
		{
			/// <summary>
			/// FT232B or FT245B device
			/// </summary>
			FT_DEVICE_BM = 0,
			/// <summary>
			/// FT8U232AM or FT8U245AM device
			/// </summary>
			FT_DEVICE_AM,
			/// <summary>
			/// FT8U100AX device
			/// </summary>
			FT_DEVICE_100AX,
			/// <summary>
			/// Unknown device
			/// </summary>
			FT_DEVICE_UNKNOWN,
			/// <summary>
			/// FT2232 device
			/// </summary>
			FT_DEVICE_2232,
			/// <summary>
			/// FT232R or FT245R device
			/// </summary>
			FT_DEVICE_232R,
			/// <summary>
			/// FT2232H device
			/// </summary>
			FT_DEVICE_2232H,
			/// <summary>
			/// FT4232H device
			/// </summary>
			FT_DEVICE_4232H,
			/// <summary>
			/// FT232H device
			/// </summary>
			FT_DEVICE_232H,
			/// <summary>
			/// FT232X device
			/// </summary>
			FT_DEVICE_X_SERIES
		};
#endregion

		#region DEFAULT_VALUES
		private const UInt32 FT_DEFAULT_BAUD_RATE			= 9600;
		private const UInt32 FT_DEFAULT_DEADMAN_TIMEOUT		= 5000;
		private const Int32 FT_COM_PORT_NOT_ASSIGNED		= -1;
		private const UInt32 FT_DEFAULT_IN_TRANSFER_SIZE	= 0x1000;
		private const UInt32 FT_DEFAULT_OUT_TRANSFER_SIZE	= 0x1000;
		private const byte FT_DEFAULT_LATENCY				= 16;
		private const UInt32 FT_DEFAULT_DEVICE_ID			= 0x04036001;
		#endregion

		#region VARIABLES
		// Create private variables for the device within the class
		private IntPtr ftHandle = IntPtr.Zero;
		#endregion

		#region TYPEDEFS
		/// <summary>
		/// Type that holds device information for GetDeviceInformation method.
		/// Used with FT_GetDeviceInfo and FT_GetDeviceInfoDetail in FTD2XX.DLL
		/// </summary>
		public class FT_DEVICE_INFO_NODE
		{
			/// <summary>
			/// Indicates device state.  Can be any combination of the following: FT_FLAGS_OPENED, FT_FLAGS_HISPEED
			/// </summary>
			public UInt32 Flags;
			/// <summary>
			/// Indicates the device type.  Can be one of the following: FT_DEVICE_232R, FT_DEVICE_2232C, FT_DEVICE_BM, FT_DEVICE_AM, FT_DEVICE_100AX or FT_DEVICE_UNKNOWN
			/// </summary>
			public FT_DEVICE Type;
			/// <summary>
			/// The Vendor ID and Product ID of the device
			/// </summary>
			public UInt32 ID;
			/// <summary>
			/// The physical location identifier of the device
			/// </summary>
			public UInt32 LocId;
			/// <summary>
			/// The device serial number
			/// </summary>
			public string SerialNumber;
			/// <summary>
			/// The device description
			/// </summary>
			public string Description;
			/// <summary>
			/// The device handle.  This value is not used externally and is provided for information only.
			/// If the device is not open, this value is 0.
			/// </summary>
			public IntPtr ftHandle;
		}
		#endregion

        #region EXCEPTION_HANDLING
        /// <summary>
		/// Exceptions thrown by errors within the FTDI class.
		/// </summary>
		[global::System.Serializable]
		public class FT_EXCEPTION : Exception
		{
			/// <summary>
			/// 
			/// </summary>
			public FT_EXCEPTION() { }
			/// <summary>
			/// 
			/// </summary>
			/// <param name="message"></param>
			public FT_EXCEPTION(string message) : base(message) { }
			/// <summary>
			/// 
			/// </summary>
			/// <param name="message"></param>
			/// <param name="inner"></param>
			public FT_EXCEPTION(string message, Exception inner) : base(message, inner) { }
			/// <summary>
			/// 
			/// </summary>
			/// <param name="info"></param>
			/// <param name="context"></param>
			protected FT_EXCEPTION(
			System.Runtime.Serialization.SerializationInfo info,
			System.Runtime.Serialization.StreamingContext context)
				: base(info, context) { }
		}
		#endregion

		#region FUNCTION_IMPORTS_FTD2XX.DLL
		// Handle to our DLL - used with GetProcAddress to load all of our functions
		IntPtr hFTD2XXDLL = IntPtr.Zero;
		// Declare pointers to each of the functions we are going to use in FT2DXX.DLL
		// These are assigned in our constructor and freed in our destructor.
		IntPtr pFT_Open = IntPtr.Zero;
		IntPtr pFT_OpenEx = IntPtr.Zero;
		IntPtr pFT_Close = IntPtr.Zero;
		IntPtr pFT_Read = IntPtr.Zero;
		IntPtr pFT_Write = IntPtr.Zero;
		IntPtr pFT_GetQueueStatus = IntPtr.Zero;
		IntPtr pFT_GetStatus = IntPtr.Zero;
		IntPtr pFT_ResetDevice = IntPtr.Zero;
		IntPtr pFT_ResetPort = IntPtr.Zero;
		IntPtr pFT_CyclePort = IntPtr.Zero;
		IntPtr pFT_Rescan = IntPtr.Zero;
		IntPtr pFT_Reload = IntPtr.Zero;
		IntPtr pFT_Purge = IntPtr.Zero;
		IntPtr pFT_SetTimeouts = IntPtr.Zero;
		IntPtr pFT_GetDriverVersion = IntPtr.Zero;
		IntPtr pFT_GetLibraryVersion = IntPtr.Zero;
		IntPtr pFT_SetDeadmanTimeout = IntPtr.Zero;
        IntPtr pFT_SetBitMode = IntPtr.Zero;
		IntPtr pFT_SetLatencyTimer = IntPtr.Zero;
		IntPtr pFT_GetLatencyTimer = IntPtr.Zero;
		IntPtr pFT_SetUSBParameters = IntPtr.Zero;
		#endregion

		#region METHOD_DEFINITIONS
		//**************************************************************************
		// OpenBySerialNumber
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Opens the FTDI device with the specified serial number.  
		/// </summary>
		/// <returns>FT_STATUS value from FT_OpenEx in FTD2XX.DLL</returns>
		/// <param name="serialnumber">Serial number of the device to open.</param>
		/// <remarks>Initialises the device to 8 data bits, 1 stop bit, no parity, no flow control and 9600 Baud.</remarks>
		public FT_STATUS OpenBySerialNumber(string serialnumber)
		{
			// Initialise ftStatus to something other than FT_OK
			FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

			// If the DLL hasn't been loaded, just return here
			if (hFTD2XXDLL == IntPtr.Zero)
				return ftStatus;

			// Call FT_OpenEx
			ftStatus = FT_OpenEx(serialnumber, FT_OPEN_BY_SERIAL_NUMBER, ref ftHandle);

			// Appears that the handle value can be non-NULL on a fail, so set it explicitly
			if (ftStatus != FT_STATUS.FT_OK)
				ftHandle = IntPtr.Zero;

            if (ftHandle != IntPtr.Zero)
			{
                FT_SetBitMode(ftHandle, 0, FT_BIT_MODES.FT_BIT_MODE_SYNC_FIFO);         // для поддержки USB 2.0
			}
			return ftStatus;
		}


		//**************************************************************************
		// Close
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Closes the handle to an open FTDI device.  
		/// </summary>
		/// <returns>FT_STATUS value from FT_Close in FTD2XX.DLL</returns>
		public FT_STATUS Close()
		{
			// Initialise ftStatus to something other than FT_OK
			FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

			// If the DLL hasn't been loaded, just return here
			if (hFTD2XXDLL == IntPtr.Zero)
				return ftStatus;

			// Call FT_Close
			ftStatus = FT_Close(ftHandle);

            if (ftStatus == FT_STATUS.FT_OK)
			{
				ftHandle = IntPtr.Zero;
			}
			return ftStatus;
		}


		//**************************************************************************
		// Read
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Read data from an open FTDI device.
		/// </summary>
		/// <returns>FT_STATUS value from FT_Read in FTD2XX.DLL</returns>
		/// <param name="dataBuffer">An array of bytes which will be populated with the data read from the device.</param>
		/// <param name="numBytesToRead">The number of bytes requested from the device.</param>
		/// <param name="numBytesRead">The number of bytes actually read.</param>
		public FT_STATUS Read(byte[] dataBuffer, Int32 numBytesToRead, ref Int32 numBytesRead)
		{
			// Initialise ftStatus to something other than FT_OK
			FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

			// If the DLL hasn't been loaded, just return here
			if (hFTD2XXDLL == IntPtr.Zero)
				return ftStatus;

			// If the buffer is not big enough to receive the amount of data requested, adjust the number of bytes to read
			if (dataBuffer.Length < numBytesToRead)
			{
				numBytesToRead = dataBuffer.Length;
			}

			if (ftHandle != IntPtr.Zero)
			{
				// Call FT_Read
				ftStatus = FT_Read(ftHandle, dataBuffer, numBytesToRead, ref numBytesRead);
			}
			return ftStatus;
		}

		//**************************************************************************
		// Write
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Write data to an open FTDI device.
		/// </summary>
		/// <returns>FT_STATUS value from FT_Write in FTD2XX.DLL</returns>
		/// <param name="dataBuffer">An array of bytes which contains the data to be written to the device.</param>
		/// <param name="numBytesToWrite">The number of bytes to be written to the device.</param>
		/// <param name="numBytesWritten">The number of bytes actually written to the device.</param>
		public FT_STATUS Write(byte[] dataBuffer, Int32 numBytesToWrite, ref UInt32 numBytesWritten)
		{
			// Initialise ftStatus to something other than FT_OK
			FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

			// If the DLL hasn't been loaded, just return here
			if (hFTD2XXDLL == IntPtr.Zero)
				return ftStatus;

			if (ftHandle != IntPtr.Zero)
			{
				// Call FT_Write
				ftStatus = FT_Write(ftHandle, dataBuffer, (UInt32)numBytesToWrite, ref numBytesWritten);
			}
			return ftStatus;
		}

		//**************************************************************************
		// ResetDevice
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Reset an open FTDI device.
		/// </summary>
		/// <returns>FT_STATUS value from FT_ResetDevice in FTD2XX.DLL</returns>
		public FT_STATUS ResetDevice()
		{
			// Initialise ftStatus to something other than FT_OK
			FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

			// If the DLL hasn't been loaded, just return here
			if (hFTD2XXDLL == IntPtr.Zero)
				return ftStatus;

			if (ftHandle != IntPtr.Zero)
			{
				// Call FT_ResetDevice
				ftStatus = FT_ResetDevice(ftHandle);
			}
			return ftStatus;
		}

		//**************************************************************************
		// Purge
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Purge data from the devices transmit and/or receive buffers.
		/// </summary>
		/// <returns>FT_STATUS value from FT_Purge in FTD2XX.DLL</returns>
		/// <param name="purgemask">Specifies which buffer(s) to be purged.  Valid values are any combination of the following flags: FT_PURGE_RX, FT_PURGE_TX</param>
		public FT_STATUS Purge(UInt32 purgemask)
		{
			// Initialise ftStatus to something other than FT_OK
			FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

			// If the DLL hasn't been loaded, just return here
			if (hFTD2XXDLL == IntPtr.Zero)
				return ftStatus;

			if (ftHandle != IntPtr.Zero)
			{
				// Call FT_Purge
				ftStatus = FT_Purge(ftHandle, purgemask);
			}
			return ftStatus;
		}

		//**************************************************************************
		// ResetPort
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Resets the device port.
		/// </summary>
		/// <returns>FT_STATUS value from FT_ResetPort in FTD2XX.DLL</returns>
		public FT_STATUS ResetPort()
		{
			// Initialise ftStatus to something other than FT_OK
			FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

			// If the DLL hasn't been loaded, just return here
			if (hFTD2XXDLL == IntPtr.Zero)
				return ftStatus;

			if (ftHandle != IntPtr.Zero)
			{
				// Call FT_ResetPort
				ftStatus = FT_ResetPort(ftHandle);
			}
			return ftStatus;
		}

		//**************************************************************************
		// CyclePort
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Causes the device to be re-enumerated on the USB bus.  This is equivalent to unplugging and replugging the device.
		/// Also calls FT_Close if FT_CyclePort is successful, so no need to call this separately in the application.
		/// </summary>
		/// <returns>FT_STATUS value from FT_CyclePort in FTD2XX.DLL</returns>
		public FT_STATUS CyclePort()
		{
			// Initialise ftStatus to something other than FT_OK
			FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

			// If the DLL hasn't been loaded, just return here
			if (hFTD2XXDLL == IntPtr.Zero)
				return ftStatus;

			if (ftHandle != IntPtr.Zero)
			{
				// Call FT_CyclePort
				ftStatus = FT_CyclePort(ftHandle);
				if (ftStatus == FT_STATUS.FT_OK)
				{
					// If successful, call FT_Close
					ftStatus = FT_Close(ftHandle);
					if (ftStatus == FT_STATUS.FT_OK)
					{
						ftHandle = IntPtr.Zero;
					}
				}
			}
			return ftStatus;
		}

		//**************************************************************************
		// Rescan
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Causes the system to check for USB hardware changes.  This is equivalent to clicking on the "Scan for hardware changes" button in the Device Manager.
		/// </summary>
		/// <returns>FT_STATUS value from FT_Rescan in FTD2XX.DLL</returns>
		public FT_STATUS Rescan()
		{
			// Initialise ftStatus to something other than FT_OK
			FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

			// If the DLL hasn't been loaded, just return here
			if (hFTD2XXDLL == IntPtr.Zero)
				return ftStatus;

			// Call FT_Rescan
			ftStatus = FT_Rescan();
			return ftStatus;
		}

		//**************************************************************************
		// Reload
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Forces a reload of the driver for devices with a specific VID and PID combination.
		/// </summary>
		/// <returns>FT_STATUS value from FT_Reload in FTD2XX.DLL</returns>
		/// <remarks>If the VID and PID parameters are 0, the drivers for USB root hubs will be reloaded, causing all USB devices connected to reload their drivers</remarks>
		/// <param name="VendorID">Vendor ID of the devices to have the driver reloaded</param>
		/// <param name="ProductID">Product ID of the devices to have the driver reloaded</param>
		public FT_STATUS Reload(UInt16 VendorID, UInt16 ProductID)
		{
			// Initialise ftStatus to something other than FT_OK
			FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

			// If the DLL hasn't been loaded, just return here
			if (hFTD2XXDLL == IntPtr.Zero)
				return ftStatus;

			// Call FT_Reload
			ftStatus = FT_Reload(VendorID, ProductID);
			return ftStatus;
		}

		//**************************************************************************
		// SetBitMode
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Puts the device in a mode other than the default UART or FIFO mode.
		/// </summary>
		/// <returns>FT_STATUS value from FT_SetBitMode in FTD2XX.DLL</returns>
		/// <param name="Mask">Sets up which bits are inputs and which are outputs.  A bit value of 0 sets the corresponding pin to an input, a bit value of 1 sets the corresponding pin to an output.
		/// In the case of CBUS Bit Bang, the upper nibble of this value controls which pins are inputs and outputs, while the lower nibble controls which of the outputs are high and low.</param>
		/// <param name="BitMode"> For FT232H devices, valid values are FT_BIT_MODE_RESET, FT_BIT_MODE_ASYNC_BITBANG, FT_BIT_MODE_MPSSE, FT_BIT_MODE_SYNC_BITBANG, FT_BIT_MODE_CBUS_BITBANG, FT_BIT_MODE_MCU_HOST, FT_BIT_MODE_FAST_SERIAL, FT_BIT_MODE_SYNC_FIFO.
		/// For FT2232H devices, valid values are FT_BIT_MODE_RESET, FT_BIT_MODE_ASYNC_BITBANG, FT_BIT_MODE_MPSSE, FT_BIT_MODE_SYNC_BITBANG, FT_BIT_MODE_MCU_HOST, FT_BIT_MODE_FAST_SERIAL, FT_BIT_MODE_SYNC_FIFO.
		/// For FT4232H devices, valid values are FT_BIT_MODE_RESET, FT_BIT_MODE_ASYNC_BITBANG, FT_BIT_MODE_MPSSE, FT_BIT_MODE_SYNC_BITBANG.
		/// For FT232R devices, valid values are FT_BIT_MODE_RESET, FT_BIT_MODE_ASYNC_BITBANG, FT_BIT_MODE_SYNC_BITBANG, FT_BIT_MODE_CBUS_BITBANG.
		/// For FT245R devices, valid values are FT_BIT_MODE_RESET, FT_BIT_MODE_ASYNC_BITBANG, FT_BIT_MODE_SYNC_BITBANG.
		/// For FT2232 devices, valid values are FT_BIT_MODE_RESET, FT_BIT_MODE_ASYNC_BITBANG, FT_BIT_MODE_MPSSE, FT_BIT_MODE_SYNC_BITBANG, FT_BIT_MODE_MCU_HOST, FT_BIT_MODE_FAST_SERIAL.
		/// For FT232B and FT245B devices, valid values are FT_BIT_MODE_RESET, FT_BIT_MODE_ASYNC_BITBANG.</param>
		/// <exception cref="FT_EXCEPTION">Thrown when the current device does not support the requested bit mode.</exception>
		public FT_STATUS SetBitMode(byte Mask, byte BitMode)
		{
			// Initialise ftStatus to something other than FT_OK
			FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;
			//!FT_ERROR ftErrorCondition = FT_ERROR.FT_NO_ERROR;

			// If the DLL hasn't been loaded, just return here
			if (hFTD2XXDLL == IntPtr.Zero)
				return ftStatus;

				if (ftHandle != IntPtr.Zero)
				{
                    /*!
					FT_DEVICE DeviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
					// Set Bit Mode does not apply to FT8U232AM, FT8U245AM or FT8U100AX devices
					GetDeviceType(ref DeviceType);
					if (DeviceType == FT_DEVICE.FT_DEVICE_AM)
					{
						// Throw an exception
						ftErrorCondition = FT_ERROR.FT_INVALID_BITMODE;
						ErrorHandler(ftStatus, ftErrorCondition);
					}
					else if (DeviceType == FT_DEVICE.FT_DEVICE_100AX)
					{
						// Throw an exception
						ftErrorCondition = FT_ERROR.FT_INVALID_BITMODE;
						ErrorHandler(ftStatus, ftErrorCondition);
					}
					else if ((DeviceType == FT_DEVICE.FT_DEVICE_BM) && (BitMode != FT_BIT_MODES.FT_BIT_MODE_RESET))
					{
						if ((BitMode & (FT_BIT_MODES.FT_BIT_MODE_ASYNC_BITBANG)) == 0)
						{
							// Throw an exception
							ftErrorCondition = FT_ERROR.FT_INVALID_BITMODE;
							ErrorHandler(ftStatus, ftErrorCondition);
						}
					}
					else if ((DeviceType == FT_DEVICE.FT_DEVICE_2232) && (BitMode != FT_BIT_MODES.FT_BIT_MODE_RESET))
					{
						if ((BitMode & (FT_BIT_MODES.FT_BIT_MODE_ASYNC_BITBANG | FT_BIT_MODES.FT_BIT_MODE_MPSSE | FT_BIT_MODES.FT_BIT_MODE_SYNC_BITBANG | FT_BIT_MODES.FT_BIT_MODE_MCU_HOST | FT_BIT_MODES.FT_BIT_MODE_FAST_SERIAL)) == 0)
						{
							// Throw an exception
							ftErrorCondition = FT_ERROR.FT_INVALID_BITMODE;
							ErrorHandler(ftStatus, ftErrorCondition);
						}
						if ((BitMode == FT_BIT_MODES.FT_BIT_MODE_MPSSE) & (InterfaceIdentifier != "A"))
						{
							// MPSSE mode is only available on channel A
							// Throw an exception
							ftErrorCondition = FT_ERROR.FT_INVALID_BITMODE;
							ErrorHandler(ftStatus, ftErrorCondition);
						}
					}
					else if ((DeviceType == FT_DEVICE.FT_DEVICE_232R) && (BitMode != FT_BIT_MODES.FT_BIT_MODE_RESET))
					{
						if ((BitMode & (FT_BIT_MODES.FT_BIT_MODE_ASYNC_BITBANG | FT_BIT_MODES.FT_BIT_MODE_SYNC_BITBANG | FT_BIT_MODES.FT_BIT_MODE_CBUS_BITBANG)) == 0)
						{
							// Throw an exception
							ftErrorCondition = FT_ERROR.FT_INVALID_BITMODE;
							ErrorHandler(ftStatus, ftErrorCondition);
						}
					}
					else if ((DeviceType == FT_DEVICE.FT_DEVICE_2232H) && (BitMode != FT_BIT_MODES.FT_BIT_MODE_RESET))
					{
						if ((BitMode & (FT_BIT_MODES.FT_BIT_MODE_ASYNC_BITBANG | FT_BIT_MODES.FT_BIT_MODE_MPSSE | FT_BIT_MODES.FT_BIT_MODE_SYNC_BITBANG | FT_BIT_MODES.FT_BIT_MODE_MCU_HOST | FT_BIT_MODES.FT_BIT_MODE_FAST_SERIAL | FT_BIT_MODES.FT_BIT_MODE_SYNC_FIFO)) == 0)
						{
							// Throw an exception
							ftErrorCondition = FT_ERROR.FT_INVALID_BITMODE;
							ErrorHandler(ftStatus, ftErrorCondition);
						}
						if (((BitMode == FT_BIT_MODES.FT_BIT_MODE_MCU_HOST) | (BitMode == FT_BIT_MODES.FT_BIT_MODE_SYNC_FIFO)) & (InterfaceIdentifier != "A"))
						{
							// MCU Host Emulation and Single channel synchronous 245 FIFO mode is only available on channel A
							// Throw an exception
							ftErrorCondition = FT_ERROR.FT_INVALID_BITMODE;
							ErrorHandler(ftStatus, ftErrorCondition);
						}
					}
					else if ((DeviceType == FT_DEVICE.FT_DEVICE_4232H) && (BitMode != FT_BIT_MODES.FT_BIT_MODE_RESET))
					{
						if ((BitMode & (FT_BIT_MODES.FT_BIT_MODE_ASYNC_BITBANG | FT_BIT_MODES.FT_BIT_MODE_MPSSE | FT_BIT_MODES.FT_BIT_MODE_SYNC_BITBANG)) == 0)
						{
							// Throw an exception
							ftErrorCondition = FT_ERROR.FT_INVALID_BITMODE;
							ErrorHandler(ftStatus, ftErrorCondition);
						}
						if ((BitMode == FT_BIT_MODES.FT_BIT_MODE_MPSSE) & ((InterfaceIdentifier != "A") & (InterfaceIdentifier != "B")))
						{
							// MPSSE mode is only available on channel A and B
							// Throw an exception
							ftErrorCondition = FT_ERROR.FT_INVALID_BITMODE;
							ErrorHandler(ftStatus, ftErrorCondition);
						}
					}
					else if ((DeviceType == FT_DEVICE.FT_DEVICE_232H) && (BitMode != FT_BIT_MODES.FT_BIT_MODE_RESET))
					{
						// FT232H supports all current bit modes!
						if (BitMode > FT_BIT_MODES.FT_BIT_MODE_SYNC_FIFO)
						{
							// Throw an exception
							ftErrorCondition = FT_ERROR.FT_INVALID_BITMODE;
							ErrorHandler(ftStatus, ftErrorCondition);
						}
					}
                    */
					// Requested bit mode is supported
					// Note FT_BIT_MODES.FT_BIT_MODE_RESET falls through to here - no bits set so cannot check for AND
					// Call FT_SetBitMode
					ftStatus = FT_SetBitMode(ftHandle, Mask, BitMode);
				}
			return ftStatus;
		}


		//**************************************************************************
		// GetRxBytesAvailable
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Gets the number of bytes available in the receive buffer.
		/// </summary>
		/// <returns>FT_STATUS value from FT_GetQueueStatus in FTD2XX.DLL</returns>
		/// <param name="RxQueue">The number of bytes available to be read.</param>
		public FT_STATUS GetRxBytesAvailable(ref Int32 RxQueue)
		{
			// Initialise ftStatus to something other than FT_OK
			FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

			// If the DLL hasn't been loaded, just return here
			if (hFTD2XXDLL == IntPtr.Zero)
				return ftStatus;

			if (ftHandle != IntPtr.Zero)
			{
				// Call FT_GetQueueStatus
				ftStatus = FT_GetQueueStatus(ftHandle, ref RxQueue);
			}
			return ftStatus;
		}

		//**************************************************************************
		// GetTxBytesWaiting
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Gets the number of bytes waiting in the transmit buffer.
		/// </summary>
		/// <returns>FT_STATUS value from FT_GetStatus in FTD2XX.DLL</returns>
		/// <param name="TxQueue">The number of bytes waiting to be sent.</param>
		public FT_STATUS GetTxBytesWaiting(ref UInt32 TxQueue)
		{
			// Initialise ftStatus to something other than FT_OK
			FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

			// If the DLL hasn't been loaded, just return here
			if (hFTD2XXDLL == IntPtr.Zero)
				return ftStatus;

			UInt32 RxQueue = 0;
			UInt32 EventStatus = 0;

			if (ftHandle != IntPtr.Zero)
			{
				// Call FT_GetStatus
				ftStatus = FT_GetStatus(ftHandle, ref RxQueue, ref TxQueue, ref EventStatus);
			}
			return ftStatus;
		}

		//**************************************************************************
		// SetTimeouts
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Sets the read and write timeout values.
		/// </summary>
		/// <returns>FT_STATUS value from FT_SetTimeouts in FTD2XX.DLL</returns>
		/// <param name="ReadTimeout">Read timeout value in ms.  A value of 0 indicates an infinite timeout.</param>
		/// <param name="WriteTimeout">Write timeout value in ms.  A value of 0 indicates an infinite timeout.</param>
		public FT_STATUS SetTimeouts(UInt32 ReadTimeout, UInt32 WriteTimeout)
		{
			// Initialise ftStatus to something other than FT_OK
			FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

			// If the DLL hasn't been loaded, just return here
			if (hFTD2XXDLL == IntPtr.Zero)
				return ftStatus;

			if (ftHandle != IntPtr.Zero)
			{
				// Call FT_SetTimeouts
				ftStatus = FT_SetTimeouts(ftHandle, ReadTimeout, WriteTimeout);
			}
			return ftStatus;
		}


		//**************************************************************************
		// GetDriverVersion
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Gets the current FTDIBUS.SYS driver version number.
		/// </summary>
		/// <returns>FT_STATUS value from FT_GetDriverVersion in FTD2XX.DLL</returns>
		/// <param name="DriverVersion">The current driver version number.</param>
		public FT_STATUS GetDriverVersion(ref UInt32 DriverVersion)
		{
			// Initialise ftStatus to something other than FT_OK
			FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

			// If the DLL hasn't been loaded, just return here
			if (hFTD2XXDLL == IntPtr.Zero)
				return ftStatus;

			if (ftHandle != IntPtr.Zero)
			{
				// Call FT_GetDriverVersion
				ftStatus = FT_GetDriverVersion(ftHandle, ref DriverVersion);
			}
			return ftStatus;
		}

		//**************************************************************************
		// GetLibraryVersion
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Gets the current FTD2XX.DLL driver version number.
		/// </summary>
		/// <returns>FT_STATUS value from FT_GetLibraryVersion in FTD2XX.DLL</returns>
		/// <param name="LibraryVersion">The current library version.</param>
		public FT_STATUS GetLibraryVersion(ref UInt32 LibraryVersion)
		{
			// Initialise ftStatus to something other than FT_OK
			FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

			// If the DLL hasn't been loaded, just return here
			if (hFTD2XXDLL == IntPtr.Zero)
				return ftStatus;

			// Call FT_GetLibraryVersion
			ftStatus = FT_GetLibraryVersion(ref LibraryVersion);
			return ftStatus;
		}

		//**************************************************************************
		// SetDeadmanTimeout
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Sets the USB deadman timeout value.  Default is 5000ms.
		/// </summary>
		/// <returns>FT_STATUS value from FT_SetDeadmanTimeout in FTD2XX.DLL</returns>
		/// <param name="DeadmanTimeout">The deadman timeout value in ms.  Default is 5000ms.</param>
		public FT_STATUS SetDeadmanTimeout(UInt32 DeadmanTimeout)
		{
			// Initialise ftStatus to something other than FT_OK
			FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

			// If the DLL hasn't been loaded, just return here
			if (hFTD2XXDLL == IntPtr.Zero)
				return ftStatus;

			// Check for our required function pointers being set up
			if (pFT_SetDeadmanTimeout != IntPtr.Zero)
			{
				tFT_SetDeadmanTimeout FT_SetDeadmanTimeout = (tFT_SetDeadmanTimeout)Marshal.GetDelegateForFunctionPointer(pFT_SetDeadmanTimeout, typeof(tFT_SetDeadmanTimeout));

				if (ftHandle != IntPtr.Zero)
				{
					// Call FT_SetDeadmanTimeout
					ftStatus = FT_SetDeadmanTimeout(ftHandle, DeadmanTimeout);
				}
			}
			else
			{
				if (pFT_SetDeadmanTimeout == IntPtr.Zero)
				{
					//!MessageBox.Show("Failed to load function FT_SetDeadmanTimeout.");
				}
			}
			return ftStatus;
		}

		//**************************************************************************
		// SetLatency
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Sets the value of the latency timer.  Default value is 16ms.
		/// </summary>
		/// <returns>FT_STATUS value from FT_SetLatencyTimer in FTD2XX.DLL</returns>
		/// <param name="Latency">The latency timer value in ms.
		/// Valid values are 2ms - 255ms for FT232BM, FT245BM and FT2232 devices.
		/// Valid values are 0ms - 255ms for other devices.</param>
		public FT_STATUS SetLatency(byte Latency)
		{
			// Initialise ftStatus to something other than FT_OK
			FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

			// If the DLL hasn't been loaded, just return here
			if (hFTD2XXDLL == IntPtr.Zero)
				return ftStatus;

			if (ftHandle != IntPtr.Zero)
			{
                   /*!
                    FT_DEVICE DeviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
					// Set Bit Mode does not apply to FT8U232AM, FT8U245AM or FT8U100AX devices
					GetDeviceType(ref DeviceType);
					if ((DeviceType == FT_DEVICE.FT_DEVICE_BM) || (DeviceType == FT_DEVICE.FT_DEVICE_2232))
					{
						// Do not allow latency of 1ms or 0ms for older devices
						// since this can cause problems/lock up due to buffering mechanism
						if (Latency < 2)
							Latency = 2;
					}
                    */
					// Call FT_SetLatencyTimer
				ftStatus = FT_SetLatencyTimer(ftHandle, Latency);
			}
			return ftStatus;
		}

		//**************************************************************************
		// GetLatency
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Gets the value of the latency timer.  Default value is 16ms.
		/// </summary>
		/// <returns>FT_STATUS value from FT_GetLatencyTimer in FTD2XX.DLL</returns>
		/// <param name="Latency">The latency timer value in ms.</param>
		public FT_STATUS GetLatency(ref byte Latency)
		{
			// Initialise ftStatus to something other than FT_OK
			FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

			// If the DLL hasn't been loaded, just return here
			if (hFTD2XXDLL == IntPtr.Zero)
				return ftStatus;

			if (ftHandle != IntPtr.Zero)
			{
				// Call FT_GetLatencyTimer
				ftStatus = FT_GetLatencyTimer(ftHandle, ref Latency);
			}
			return ftStatus;
		}

		//**************************************************************************
		// SetUSBTransferSizes
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Sets the USB IN and OUT transfer sizes.
		/// </summary>
		/// <returns>FT_STATUS value from FT_SetUSBParameters in FTD2XX.DLL</returns>
		/// <param name="InTransferSize">The USB IN transfer size in bytes.</param>
		public FT_STATUS InTransferSize(UInt32 InTransferSize)
		// Only support IN transfer sizes at the moment
		//public UInt32 InTransferSize(UInt32 InTransferSize, UInt32 OutTransferSize)
		{
			// Initialise ftStatus to something other than FT_OK
			FT_STATUS ftStatus = FT_STATUS.FT_OTHER_ERROR;

			// If the DLL hasn't been loaded, just return here
			if (hFTD2XXDLL == IntPtr.Zero)
				return ftStatus;

			UInt32 OutTransferSize = InTransferSize;

            if (ftHandle != IntPtr.Zero)
			{
				// Call FT_SetUSBParameters
				ftStatus = FT_SetUSBParameters(ftHandle, InTransferSize, OutTransferSize);
			}
			return ftStatus;
		}

		#endregion

		#region PROPERTY_DEFINITIONS
		//**************************************************************************
		// IsOpen
		//**************************************************************************
		// Intellisense comments
		/// <summary>
		/// Gets the open status of the device.
		/// </summary>
		public bool IsOpen
		{
			get
			{
				if (ftHandle == IntPtr.Zero)
					return false;
				else
					return true;
			}
		}

		#endregion

		#region HELPER_METHODS
		//**************************************************************************
		// ErrorHandler
		//**************************************************************************
		/// <summary>
		/// Method to check ftStatus and ftErrorCondition values for error conditions and throw exceptions accordingly.
		/// </summary>
		private void ErrorHandler(FT_STATUS ftStatus, FT_ERROR ftErrorCondition)
		{
			if (ftStatus != FT_STATUS.FT_OK)
			{
				// Check FT_STATUS values returned from FTD2XX DLL calls
				switch (ftStatus)
				{
					case FT_STATUS.FT_DEVICE_NOT_FOUND:
						{
							throw new FT_EXCEPTION("FTDI device not found.");
						}
					case FT_STATUS.FT_DEVICE_NOT_OPENED:
						{
							throw new FT_EXCEPTION("FTDI device not opened.");
						}
					case FT_STATUS.FT_DEVICE_NOT_OPENED_FOR_ERASE:
						{
							throw new FT_EXCEPTION("FTDI device not opened for erase.");
						}
					case FT_STATUS.FT_DEVICE_NOT_OPENED_FOR_WRITE:
						{
							throw new FT_EXCEPTION("FTDI device not opened for write.");
						}
					case FT_STATUS.FT_EEPROM_ERASE_FAILED:
						{
							throw new FT_EXCEPTION("Failed to erase FTDI device EEPROM.");
						}
					case FT_STATUS.FT_EEPROM_NOT_PRESENT:
						{
							throw new FT_EXCEPTION("No EEPROM fitted to FTDI device.");
						}
					case FT_STATUS.FT_EEPROM_NOT_PROGRAMMED:
						{
							throw new FT_EXCEPTION("FTDI device EEPROM not programmed.");
						}
					case FT_STATUS.FT_EEPROM_READ_FAILED:
						{
							throw new FT_EXCEPTION("Failed to read FTDI device EEPROM.");
						}
					case FT_STATUS.FT_EEPROM_WRITE_FAILED:
						{
							throw new FT_EXCEPTION("Failed to write FTDI device EEPROM.");
						}
					case FT_STATUS.FT_FAILED_TO_WRITE_DEVICE:
						{
							throw new FT_EXCEPTION("Failed to write to FTDI device.");
						}
					case FT_STATUS.FT_INSUFFICIENT_RESOURCES:
						{
							throw new FT_EXCEPTION("Insufficient resources.");
						}
					case FT_STATUS.FT_INVALID_ARGS:
						{
							throw new FT_EXCEPTION("Invalid arguments for FTD2XX function call.");
						}
					case FT_STATUS.FT_INVALID_BAUD_RATE:
						{
							throw new FT_EXCEPTION("Invalid Baud rate for FTDI device.");
						}
					case FT_STATUS.FT_INVALID_HANDLE:
						{
							throw new FT_EXCEPTION("Invalid handle for FTDI device.");
						}
					case FT_STATUS.FT_INVALID_PARAMETER:
						{
							throw new FT_EXCEPTION("Invalid parameter for FTD2XX function call.");
						}
					case FT_STATUS.FT_IO_ERROR:
						{
							throw new FT_EXCEPTION("FTDI device IO error.");
						}
					case FT_STATUS.FT_OTHER_ERROR:
						{
							throw new FT_EXCEPTION("An unexpected error has occurred when trying to communicate with the FTDI device.");
						}
					default:
						break;
				}
			}
			if (ftErrorCondition != FT_ERROR.FT_NO_ERROR)
			{
				// Check for other error conditions not handled by FTD2XX DLL
				switch (ftErrorCondition)
				{
					case FT_ERROR.FT_INCORRECT_DEVICE:
						{
							throw new FT_EXCEPTION("The current device type does not match the EEPROM structure.");
						}
					case FT_ERROR.FT_INVALID_BITMODE:
						{
							throw new FT_EXCEPTION("The requested bit mode is not valid for the current device.");
						}
					case FT_ERROR.FT_BUFFER_SIZE:
						{
							throw new FT_EXCEPTION("The supplied buffer is not big enough.");
						}

					default:
						break;
				}

			}

			return;
		}
		#endregion
	}
}
