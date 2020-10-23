using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Toolchains.InProcess.Emit;
using Microsoft.Diagnostics.Tracing.Parsers.ClrPrivate;
using System;
using System.Globalization;

namespace ExpressionEvaluation.Benchmark
{
    internal class BenchmarkConfig : ManualConfig
    {
        #region Public Constructors

        public BenchmarkConfig()
        {
            CultureInfo cultureInfo = (CultureInfo)CultureInfo.InvariantCulture.Clone();

            cultureInfo.NumberFormat.NumberDecimalSeparator = ".";
            cultureInfo.NumberFormat.NumberGroupSeparator = "";

            SummaryStyle summaryStyle = new SummaryStyle(
                cultureInfo: cultureInfo,
                printUnitsInHeader: true,
                sizeUnit: BenchmarkDotNet.Columns.SizeUnit.B,
                timeUnit: Perfolizer.Horology.TimeUnit.Nanosecond,
                printUnitsInContent: false
            );

            CultureInfo = cultureInfo;

            AddExporter(new CsvExporter(CsvSeparator.Comma, summaryStyle));
            AddExporter(new CsvMeasurementsExporter(CsvSeparator.Comma, summaryStyle));

            Job job = Job.Default
                .WithIterationCount(50);
            //.WithAffinity((IntPtr)0b0000_0001); // be sure to stay on core #1

            if (System.Diagnostics.Debugger.IsAttached)
            {
                job = job.WithToolchain(new InProcessEmitToolchain(
                                TimeSpan.FromHours(1), // 1h should be enough to debug the benchmark
                                true));

                Options |= ConfigOptions.DisableOptimizationsValidator;
            }

            AddJob(job);
        }

        #endregion Public Constructors
    }
}