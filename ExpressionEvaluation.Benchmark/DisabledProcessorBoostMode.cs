using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ExpressionEvaluation.Benchmark
{
    internal class DisabledProcessorBoostMode : IDisposable
    {
        #region Private Enums

        private enum BoostMode
        {
            Deactivate = 0,
            Activate = 1,
            High = 2,
            ActivateEfficient = 3,
            HighEfficient = 4,
            Aggressive = 5,
            AggressiveEfficient = 6
        }

        #endregion Private Enums

        #region Private Fields

        private const string PERFBOOSTMODE = "be337238-0d82-4146-a960-4f3749d470c7";
        private const string SUB_PROCESSOR = "54533251-82be-4824-96c1-47b60b740d00";

        #endregion Private Fields

        #region Public Constructors

        public DisabledProcessorBoostMode()
        {
            SetBoostMode(BoostMode.Deactivate);
        }

        #endregion Public Constructors

        #region Public Methods

        public void Dispose()
        {
            SetBoostMode(BoostMode.ActivateEfficient);
        }

        #endregion Public Methods

        #region Private Methods

        private void SetBoostMode(BoostMode boostMode)
        {
            Process.Start("powercfg", $" /SETACVALUEINDEX SCHEME_CURRENT {SUB_PROCESSOR} {PERFBOOSTMODE} {(int)boostMode}").WaitForExit();
            Process.Start("powercfg", $" /SETACTIVE SCHEME_CURRENT").WaitForExit();
        }

        #endregion Private Methods
    }
}