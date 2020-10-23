using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Policy;
using System.Text;

namespace ExpressionEvaluation.Benchmark
{
    public class DisabledRealTimeMonitoring : IDisposable
    {
        public DisabledRealTimeMonitoring()
        {
            SetRealTimeMonitoring(false);
        }

        public void Dispose()
        {
            SetRealTimeMonitoring(true);
        }

        private void SetRealTimeMonitoring(bool enabled)
        {
            Process.Start("powershell", $" Set-MpPreference -DisableRealtimeMonitoring ${(enabled ? "false" : "true")}").WaitForExit();
        }
    }
}
