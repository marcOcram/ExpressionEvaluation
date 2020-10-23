using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Microsoft.Diagnostics.Tracing.Parsers.Kernel;
using NiL.JS.Expressions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace ExpressionEvaluation.Benchmark
{
    public static class Program
    {
        #region Public Methods

        public static void Main(string[] args)
        {
            Dictionary<Type, MethodInfo[]> benchmarks = typeof(Program).Assembly.GetTypes()
                .SelectMany(t => t.GetMethods(), (t, m) => (Type: t, Method: m))
                .Where(x => x.Type.IsClass && x.Method.GetCustomAttribute<BenchmarkAttribute>() != null)
                .GroupBy(x => x.Type, x => x.Method)
                .ToDictionary(g => g.Key, g => g.OrderBy(m => m.Name).ToArray());

            Type[] benchmarkTypes = benchmarks.Keys.OrderBy(t => t.FullName).ToArray();

            bool exit = false;
            do
            {
                Console.Clear();

                for (int i = 0; i < benchmarkTypes.Length; ++i)
                {
                    Console.WriteLine($"[{i}]\t{benchmarkTypes[i].FullName}");
                }

                Console.WriteLine("[ ]\tExit");
                Console.Write("Selection: ");

                string decision = Console.ReadLine();

                if (int.TryParse(decision, out int benchmarkIndex))
                {
                    if (benchmarkIndex > benchmarks.Count)
                    {
                        Console.WriteLine("Invalid selection! Press any key to repeat ...");
                        Console.ReadKey();
                    }
                    else
                    {
                        Type selectedBenchmark = benchmarkTypes[benchmarkIndex];
                        MethodInfo[] methods = benchmarks[selectedBenchmark];
                        Console.Clear();

                        for (int i = 0; i < methods.Length; ++i)
                        {
                            Console.WriteLine($"[{i}]\t{methods[i].Name}");
                        }

                        Console.WriteLine("[ ]\tAll");
                        Console.Write("Selection: ");

                        string methodDecision = Console.ReadLine();

                        MethodInfo[] selectedMethods;
                        if (string.IsNullOrWhiteSpace(methodDecision))
                        {
                            selectedMethods = methods;
                        }
                        else
                        {
                            selectedMethods = methodDecision
                               .Split(',', StringSplitOptions.RemoveEmptyEntries)
                               .Select(e => int.TryParse(e, out int decision) ? (int?)decision : null)
                               .Where(d => d.HasValue && d.Value < methods.Length)
                               .Select(d => methods[d.Value])
                               .ToArray();
                        }

                        Console.Clear();
                        if (Debugger.IsAttached)
                        {
                            BenchmarkRunner.Run(selectedBenchmark, selectedMethods);
                        }
                        else
                        {
                            using (new DisabledStandbyMode())
                            using (new DisabledProcessorBoostMode())
                            using (new DisabledRealTimeMonitoring())
                            {
                                BenchmarkRunner.Run(selectedBenchmark, selectedMethods);
                            }
                        }
                        Console.ReadLine();
                    }
                }
                else
                {
                    exit = true;
                }
            }
            while (!exit);
        }

        #endregion Public Methods
    }
}