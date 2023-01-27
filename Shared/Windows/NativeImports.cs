using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Shared.Windows
{
    public static class NativeImports
    {
        #region kernel32.dll

        [Flags]
        public enum EFileAttributes : uint
        {
            Readonly          = 0x00000001,
            Hidden            = 0x00000002,
            System            = 0x00000004,
            Directory         = 0x00000010,
            Archive           = 0x00000020,
            Device            = 0x00000040,
            Normal            = 0x00000080,
            Temporary         = 0x00000100,
            SparseFile        = 0x00000200,
            ReparsePoint      = 0x00000400,
            Compressed        = 0x00000800,
            Offline           = 0x00001000,
            NotContentIndexed = 0x00002000,
            Encrypted         = 0x00004000,
            Write_Through     = 0x80000000,
            Overlapped        = 0x40000000,
            NoBuffering       = 0x20000000,
            RandomAccess      = 0x10000000,
            SequentialScan    = 0x08000000,
            DeleteOnClose     = 0x04000000,
            BackupSemantics   = 0x02000000,
            PosixSemantics    = 0x01000000,
            OpenReparsePoint  = 0x00200000,
            OpenNoRecall      = 0x00100000,
            FirstPipeInstance = 0x00080000
        }

        /// <summary>
        /// A generic safe handle for any handle that is closed via CloseHandle.
        /// </summary>
        public class SafeObjectHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            public SafeObjectHandle() : base(true)
            {
            }

            public SafeObjectHandle(IntPtr existingHandle, bool ownsHandle) : base(ownsHandle)
            {
                SetHandle(existingHandle);
            }

            protected override bool ReleaseHandle()
            {
                return CloseHandle(handle);
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle(
            IntPtr hObject);

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern SafeFileHandle CreateFile(
            string fileName,
            [MarshalAs(UnmanagedType.U4)] FileAccess fileAccess,
            [MarshalAs(UnmanagedType.U4)] FileShare fileShare,
            IntPtr securityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
            [MarshalAs(UnmanagedType.U4)] EFileAttributes flags,
            IntPtr template);

        [DllImport("kernel32.dll", SetLastError = true)]
        public extern static bool ReadFile(
            SafeFileHandle hFile,
            byte[] lpBuffer,
            uint nNumberofBytesToRead,
            out uint lpNumberOfBytesRead,
            ref NativeOverlapped lpOverlapped);

        /// <summary>
        /// Use to fix sending data to TR controllers
        /// (broken in Windows 7)
        /// </summary>
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public extern static bool WriteFile(
            SafeFileHandle hFile,            // HANDLE
            byte[] lpBuffer,                 // LPCVOID
            uint nNumberOfBytesToWrite,      // DWORD
            out uint lpNumberOfBytesWritten, // LPDWORD
            ref NativeOverlapped lpOverlapped);


        /// <summary>
        /// Async Callback for WriteFileEx
        /// </summary>
        public delegate void WriteFileCompletionDelegate(
            uint dwErrorCode, 
            uint dwNumberOfBytesTransfered, 
            ref NativeOverlapped lpOverlapped);

        /// <summary>
        /// Like WriteFile but provides an asynchronous callback
        /// </summary>
        [DllImport("kernel32.dll", SetLastError = true)]
        public extern static bool WriteFileEx(
            SafeFileHandle hFile, 
            byte[] lpBuffer,
            uint nNumberOfBytesToWrite, 
            ref NativeOverlapped lpOverlapped,
            WriteFileCompletionDelegate lpCompletionRoutine);

        #endregion

        #region setupapi.dll

        /// <summary>
        /// Provided to SetupDiGetClassDevs to specify what to included in the device information
        /// </summary>
        [Flags]
        public enum DIGCF : int     // Device Information Group Control Flag?
        {
            /// <summary>
            /// The device that is associated with the system default device interface
            /// (only valid with DIGCF_DEVICEINTERFACE)
            /// </summary>
            Default         = 0x00000001,

            /// <summary>
            /// Devices that are currently present
            /// </summary>
            Present         = 0x00000002,

            /// <summary>
            /// Devices that are installed for the specified device setup or interface classes
            /// </summary>
            AllClasses      = 0x00000004,

            /// <summary>
            /// Devices that are part of the current hardware profile
            /// </summary>
            Profile         = 0x00000008,

            /// <summary>
            /// Devices that support device interfaces for the specified device classes.
            /// (Must be set if a device instance ID is specified)
            /// </summary>
            DeviceInterface = 0x00000010
        }

        // Used for BT Stack detection
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SP_DEVINFO_DATA
        {
            public uint cbSize;
            public Guid ClassGuid;
            public uint DevInst;
            public IntPtr Reserved;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SP_DEVICE_INTERFACE_DATA
        {
            public int cbSize;
            public Guid interfaceClassGuid;
            public int flags;
            public IntPtr reserved;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct SP_DEVICE_INTERFACE_DETAIL_DATA
        {
            public uint size;           // DWORD
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
            public string devicePath;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct DEVPROPKEY
        {
            public Guid fmtid;
            public uint pid;
        };

        [DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetupDiCreateDeviceInfoList(
          ref Guid classId,
          IntPtr hwndParent
        );
        
        [DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetupDiGetClassDevs(
            ref Guid ClassGuid,
            [MarshalAs(UnmanagedType.LPTStr)] string Enumerator,
            IntPtr hwndParent,
            uint Flags
        );

        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiOpenDeviceInfo(
            IntPtr DevInfoSet,
            string Enumerator,
            IntPtr hWndParent,
            uint Flags,
            ref SP_DEVINFO_DATA DeviceInfoData
        );

        [DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiEnumDeviceInterfaces(
            IntPtr hDevInfo,
            //ref SP_DEVINFO_DATA devInfo,
            IntPtr devInvo,
            ref Guid interfaceClassGuid,
            int memberIndex,
            ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData
        );

        [DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiGetDeviceInterfaceDetail(
            IntPtr hDevInfo,
            ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData,
            IntPtr deviceInterfaceDetailData,
            uint deviceInterfaceDetailDataSize,
            out uint requiredSize,
            IntPtr deviceInfoData);

        [DllImport(@"setupapi.dll", SetLastError = true)]
        public static extern bool SetupDiGetDeviceInterfaceDetail(
            IntPtr hDevInfo,
            ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData,
            ref SP_DEVICE_INTERFACE_DETAIL_DATA deviceInterfaceDetailData,
            uint deviceInterfaceDetailDataSize,
            out uint requiredSize,
            ref SP_DEVINFO_DATA deviceInfoData);

        [DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiGetDeviceProperty(
          IntPtr DeviceInfoSet,
          SP_DEVINFO_DATA DeviceInfoData,
          DEVPROPKEY PropertyKey,
          out ulong PropertyType,
          char[] PropertyBuffer,
          int PropertyBufferSize,
          out int RequiredSize,
          uint Flags
        );

        [DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiDestroyDeviceInfoList(
            IntPtr DeviceInfoSet);

        [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
        public static extern int CM_Get_Device_ID(
           uint dnDevInst,
           //string buffer,
           char[] buffer,
           int bufferLen,
           int flags);

        [DllImport("setupapi.dll")]
        public static extern int CM_Get_Parent(
            out uint pdnDevInst,
            uint dnDevInst,
            int ulFlags);

        [DllImport("setupapi.dll", SetLastError = true)]
        public static extern int CM_Get_DevNode_Status(
            ref int pulStatus, 
            ref int pulProblemNumber, 
            int dnDevInst, 
            int ulFlags);
        
        #endregion

        #region hid.dll

        [StructLayout(LayoutKind.Sequential)]
        public struct HIDD_ATTRIBUTES
        {
            public int Size;
            public short VendorID;
            public short ProductID;
            public short VersionNumber;
        }

        [DllImport(@"hid.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern void HidD_GetHidGuid(
            out Guid gHid);

        [DllImport("hid.dll")]
        public static extern bool HidD_GetAttributes(
            IntPtr HidDeviceObject, 
            ref HIDD_ATTRIBUTES Attributes);

        [DllImport("hid.dll")]
        public extern static bool HidD_SetOutputReport(
            IntPtr HidDeviceObject,
            byte[] lpReportBuffer,
            uint ReportBufferLength);

        #endregion

        #region bthprops.cpl

        public static readonly Guid HidServiceClassGuid = Guid.Parse("00001124-0000-1000-8000-00805F9B34FB");

        [Flags]
        public enum BluetoothServiceFlag : uint
        {
            Disable = 0x00,
            Enable = 0x01
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEMTIME
        {
            [MarshalAs(UnmanagedType.U2)]
            public short Year;
            [MarshalAs(UnmanagedType.U2)]
            public short Month;
            [MarshalAs(UnmanagedType.U2)]
            public short DayOfWeek;
            [MarshalAs(UnmanagedType.U2)]
            public short Day;
            [MarshalAs(UnmanagedType.U2)]
            public short Hour;
            [MarshalAs(UnmanagedType.U2)]
            public short Minute;
            [MarshalAs(UnmanagedType.U2)]
            public short Second;
            [MarshalAs(UnmanagedType.U2)]
            public short Milliseconds;

            public SYSTEMTIME(DateTime dt)
            {
                dt = dt.ToUniversalTime();  // SetSystemTime expects the SYSTEMTIME in UTC
                Year = (short)dt.Year;
                Month = (short)dt.Month;
                DayOfWeek = (short)dt.DayOfWeek;
                Day = (short)dt.Day;
                Hour = (short)dt.Hour;
                Minute = (short)dt.Minute;
                Second = (short)dt.Second;
                Milliseconds = (short)dt.Millisecond;
            }
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct BLUETOOTH_DEVICE_INFO
        {
            public uint dwSize;
            public ulong Address;
            public uint ulClassofDevice;
            public bool fConnected;
            public bool fRemembered;
            public bool fAuthenticated;
            public SYSTEMTIME stLastSeen;
            public SYSTEMTIME stLastUsed;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 248)]
            public string szName;

            public static BLUETOOTH_DEVICE_INFO Create()
            {
                return new BLUETOOTH_DEVICE_INFO()
                {
                    dwSize = (uint)Marshal.SizeOf(typeof(BLUETOOTH_DEVICE_INFO))
                };
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BLUETOOTH_DEVICE_SEARCH_PARAMS
        {
            internal uint dwSize;
            internal bool fReturnAuthenticated;
            internal bool fReturnRemembered;
            internal bool fReturnUnknown;
            internal bool fReturnConnected;
            internal bool fIssueInquiry;
            internal byte cTimeoutMultiplier;
            internal IntPtr hRadio;

            internal static BLUETOOTH_DEVICE_SEARCH_PARAMS Create()
            {
                return new BLUETOOTH_DEVICE_SEARCH_PARAMS()
                {
                    dwSize = (uint)Marshal.SizeOf(typeof(BLUETOOTH_DEVICE_SEARCH_PARAMS))
                };
            }
        }
        
        [StructLayout(LayoutKind.Sequential)]
        public struct BLUETOOTH_FIND_RADIO_PARAMS
        {
            internal uint dwSize;

            internal static BLUETOOTH_FIND_RADIO_PARAMS Create()
            {
                return new BLUETOOTH_FIND_RADIO_PARAMS()
                {
                    dwSize = (uint)Marshal.SizeOf(typeof(BLUETOOTH_FIND_RADIO_PARAMS))
                };
            }
        }

        private const int BLUETOOTH_MAX_NAME_SIZE = 248;
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct BLUETOOTH_RADIO_INFO
        {
            internal uint dwSize;
            internal ulong address;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = BLUETOOTH_MAX_NAME_SIZE)]
            internal string szName;
            internal uint ulClassOfDevice;
            internal ushort lmpSubversion;
            internal ushort manufacturer;

            public string Address
            {
                get
                {
                    var bytes = BitConverter.GetBytes(address);
                    StringBuilder str = new StringBuilder();
                    for (int i = bytes.Length - 1; i >= 0; i--)
                        str.Append(bytes[i].ToString("X2"));
                    return str.ToString();
                }
            }

            internal static BLUETOOTH_RADIO_INFO Create()
            {
                return new BLUETOOTH_RADIO_INFO()
                {
                    dwSize = (uint)Marshal.SizeOf(typeof(BLUETOOTH_RADIO_INFO))
                };
            }
        }

        /// <summary>
        /// A safe handle for Bluetooth radio searches (handles returned by BluetoothFindFirstRadio).
        /// </summary>
        public class SafeBluetoothRadioHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            public SafeBluetoothRadioHandle() : base(true)
            {
            }

            public SafeBluetoothRadioHandle(IntPtr existingHandle, bool ownsHandle) : base(ownsHandle)
            {
                SetHandle(existingHandle);
            }

            protected override bool ReleaseHandle()
            {
                return BluetoothFindRadioClose(handle);
            }
        }

        /// <summary>
        /// A safe handle for Bluetooth device searches (handles returned by BluetoothFindFirstDevice).
        /// </summary>
        public class SafeBluetoothDeviceHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            public SafeBluetoothDeviceHandle() : base(true)
            {
            }

            public SafeBluetoothDeviceHandle(IntPtr existingHandle, bool ownsHandle) : base(ownsHandle)
            {
                SetHandle(existingHandle);
            }

            protected override bool ReleaseHandle()
            {
                return BluetoothFindDeviceClose(handle);
            }
        }

        [DllImport("bthprops.cpl", SetLastError = true)]
        public static extern uint BluetoothGetRadioInfo(
            SafeObjectHandle hRadio, 
            ref BLUETOOTH_RADIO_INFO pRadioInfo);

        [DllImport("bthprops.cpl", SetLastError = true)]
        public static extern SafeBluetoothRadioHandle BluetoothFindFirstRadio(
            in BLUETOOTH_FIND_RADIO_PARAMS pbtfrp, 
            out SafeObjectHandle phRadio);

        [DllImport("bthprops.cpl", SetLastError = true)]
        public static extern bool BluetoothFindNextRadio(
            SafeBluetoothRadioHandle hFind, 
            out SafeObjectHandle phRadio);

        [DllImport("bthprops.cpl", SetLastError = true)]
        public static extern bool BluetoothFindRadioClose(IntPtr hFind);

        [DllImport("bthprops.cpl", SetLastError = true)]
        public static extern SafeBluetoothDeviceHandle BluetoothFindFirstDevice(
            in BLUETOOTH_DEVICE_SEARCH_PARAMS searchParams, 
            ref BLUETOOTH_DEVICE_INFO deviceInfo);

        [DllImport("bthprops.cpl", SetLastError = true)]
        public static extern bool BluetoothFindNextDevice(
            SafeBluetoothDeviceHandle hFind, 
            ref BLUETOOTH_DEVICE_INFO pbtdi);

        [DllImport("bthprops.cpl", SetLastError = true)]
        public static extern bool BluetoothFindDeviceClose(IntPtr hFind);

        [DllImport("bthprops.cpl", SetLastError = true)]
        public static extern uint BluetoothRemoveDevice(in ulong pAddress);

        [DllImport("bthprops.cpl", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern uint BluetoothAuthenticateDevice(
            IntPtr hwndParent, 
            SafeObjectHandle hRadio, 
            in BLUETOOTH_DEVICE_INFO pbtdi, 
            [MarshalAs(UnmanagedType.LPWStr)] string pszPasskey, 
            uint ulPasskeyLength);

        [DllImport("bthprops.cpl", SetLastError = true)]
        public static extern uint BluetoothEnumerateInstalledServices(
            SafeObjectHandle hRadio, 
            in BLUETOOTH_DEVICE_INFO pbtdi, 
            ref uint pcServiceInout, 
            Guid[] pGuidServices);

        [DllImport("bthprops.cpl", SetLastError = true)]
        public static extern uint BluetoothSetServiceState(
            SafeObjectHandle hRadio, 
            in BLUETOOTH_DEVICE_INFO pbtdi, 
            in Guid pGuidService, 
            [MarshalAs(UnmanagedType.U4)] BluetoothServiceFlag dwServiceFlags);

        [DllImport("bthprops.cpl", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool BluetoothEnableDiscovery(
            SafeObjectHandle hRadio,
            [MarshalAs(UnmanagedType.Bool)] bool fEnabled);

        #endregion
    }
}
