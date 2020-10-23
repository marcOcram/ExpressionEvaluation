using System;
using System.Collections.Generic;
using System.Text;

namespace ExpressionEvaluation.Benchmark
{
    internal class DisabledStandbyMode : IDisposable
    {
        private readonly IntPtr _powerRequest;
        #region Public Constructors

        public DisabledStandbyMode()
        {
            NativeMethods.POWER_REQUEST_CONTEXT context = new NativeMethods.POWER_REQUEST_CONTEXT() {
                Flags = NativeMethods.POWER_REQUEST_CONTEXT_SIMPLE_STRING,
                Version = NativeMethods.POWER_REQUEST_CONTEXT_VERSION,
                SimpleReasonString = "Benchmarking ..."
            };

            _powerRequest = NativeMethods.PowerCreateRequest(ref context);

            if (_powerRequest == IntPtr.Zero)
            {
                throw new InvalidOperationException();
            }

            bool success = NativeMethods.PowerSetRequest(_powerRequest, NativeMethods.PowerRequestType.PowerRequestSystemRequired);

            if (!success)
            {
                throw new InvalidOperationException();
            }
        }

        #endregion Public Constructors

        #region Public Methods

        public void Dispose()
        {
            bool success = NativeMethods.PowerClearRequest(_powerRequest, NativeMethods.PowerRequestType.PowerRequestSystemRequired);

            if (!success)
            {
                throw new InvalidOperationException();
            }
        }

        #endregion Public Methods
    }
}