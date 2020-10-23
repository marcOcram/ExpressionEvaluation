using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ExpressionEvaluation.Benchmark
{
    internal static class NativeMethods
    {
        #region Public Enums

        public enum PowerRequestType
        {
            PowerRequestDisplayRequired = 0, // Not to be used by drivers
            PowerRequestSystemRequired,
            PowerRequestAwayModeRequired, // Not to be used by drivers
            PowerRequestExecutionRequired // Not to be used by drivers
        }

        #endregion Public Enums

        #region Public Structs

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct POWER_REQUEST_CONTEXT
        {
            public UInt32 Version;
            public UInt32 Flags;

            [MarshalAs(UnmanagedType.LPWStr)]
            public string SimpleReasonString;
        }

        #endregion Public Structs

        #region Private Fields

        public const int POWER_REQUEST_CONTEXT_SIMPLE_STRING = 0x1;
        public const int POWER_REQUEST_CONTEXT_VERSION = 0;

        #endregion Private Fields

        #region Public Methods

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool PowerClearRequest(IntPtr PowerRequestHandle, PowerRequestType RequestType);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr PowerCreateRequest(ref POWER_REQUEST_CONTEXT Context);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool PowerSetRequest(IntPtr PowerRequestHandle, PowerRequestType RequestType);

        #endregion Public Methods
    }
}