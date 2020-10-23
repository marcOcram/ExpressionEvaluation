using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using CodingSeb.ExpressionEvaluator;
using DynamicExpresso;
using Jint;
using Microsoft.ClearScript.V8;
using Microsoft.CodeAnalysis.Scripting;
using NiL.JS.Core;
using NiL.JS.Extensions;
using NReco.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Z.Expressions;

namespace ExpressionEvaluation.Benchmark
{
    [Config(typeof(BenchmarkConfig))]
    [MemoryDiagnoser]
    [MinColumn, MaxColumn, MedianColumn, Q1Column, Q3Column]
    [Outliers(Perfolizer.Mathematics.OutlierDetection.OutlierMode.DontRemove)]
    public class CondensedVariableExpressionBenchmarks
    {
        #region Public Classes

        public class Parameter
        {
            #region Public Properties

            public string[] Statements { get; }

            public int Sum { get; }

            #endregion Public Properties

            #region Public Constructors

            public Parameter(int count)
            {
                Statements = Enumerable.Range(0, count).Select(c => $"{c} + {c}").ToArray();
                Sum = Enumerable.Range(0, count).Sum() * 2;
            }

            #endregion Public Constructors

            #region Public Methods

            public override string ToString()
            {
                return Statements.Length.ToString("D6");
            }

            #endregion Public Methods
        }

        #endregion Public Classes

        #region Public Methods

        [Benchmark(Description = "\\clearscript")]
        [BenchmarkCategory("JavaScript")]
        [ArgumentsSource(nameof(Parameters))]
        public bool ClearScript(Parameter parameter)
        {
            StringBuilder statementBuilder = new StringBuilder();

            for (int i = 0; i < parameter.Statements.Length; ++i)
            {
                statementBuilder.Append("results[").Append(i).Append("] = ").Append(parameter.Statements[i]).AppendLine(";");
            }

            int[] results = new int[parameter.Statements.Length];
            using (V8ScriptEngine engine = new V8ScriptEngine())
            {
                engine.AddHostObject("results", results);

                engine.Execute(statementBuilder.ToString());

                return Assert(results, parameter.Sum);
            }
        }

        [Benchmark(Description = "\\dynamicexpresso")]
        [BenchmarkCategory("C#")]
        [ArgumentsSource(nameof(Parameters))]
        public bool DynamicExpresso(Parameter parameter)
        {
            // NOTE: Does not have any options to optimize calls with multiple statements because it is unable to interpret multi line statements
            Interpreter interpreter = new Interpreter();

            List<int> results = new List<int>(parameter.Statements.Length);
            foreach (string statement in parameter.Statements)
            {
                int result = interpreter.Eval<int>(statement);

                results.Add(result);
            }

            return Assert(results, parameter.Sum);
        }

        [Benchmark(Description = "\\evalexpression")]
        [BenchmarkCategory("C#")]
        [ArgumentsSource(nameof(Parameters))]
        public bool EvalExpressionNET(Parameter parameter)
        {
            StringBuilder statementBuilder = new StringBuilder()
                .AppendLine("new int[] {")
                .AppendJoin("," + Environment.NewLine, parameter.Statements)
                .AppendLine("}");

            using (EvalContext context = new EvalContext())
            {
                context.UseLocalCache = true;

                int[] results = context.Execute<int[]>(statementBuilder.ToString());

                return Assert(results, parameter.Sum);
            }
        }

        [Benchmark(Description = "\\expressionevaluator")]
        [BenchmarkCategory("C#")]
        [ArgumentsSource(nameof(Parameters))]
        public bool ExpressionEvaluator(Parameter parameter)
        {
            StringBuilder statementBuilder = new StringBuilder()
                    .AppendLine("new int[] {")
                    .AppendJoin("," + Environment.NewLine, parameter.Statements)
                    .AppendLine("}");

            ExpressionEvaluator expressionEvaluator = new ExpressionEvaluator();

            int[] results = expressionEvaluator.Evaluate<int[]>(statementBuilder.ToString());

            return Assert(results, parameter.Sum);
        }

        [Benchmark(Description = "\\ironpython")]
        [BenchmarkCategory("Python")]
        [ArgumentsSource(nameof(Parameters))]
        public bool IronPython(Parameter parameter)
        {
            StringBuilder statementBuilder = new StringBuilder()
                       .AppendLine("[")
                       .AppendJoin("," + Environment.NewLine, parameter.Statements)
                       .AppendLine("]");

            Microsoft.Scripting.Hosting.ScriptEngine engine = global::IronPython.Hosting.Python.CreateEngine();

            int[] results = engine.Execute<IronPython.Runtime.List>(statementBuilder.ToString()).Select(Convert.ToInt32).ToArray();

            return Assert(results, parameter.Sum);
        }

        [Benchmark(Description = "\\jint")]
        [BenchmarkCategory("JavaScript")]
        [ArgumentsSource(nameof(Parameters))]
        public bool Jint(Parameter parameter)
        {
            StringBuilder statementBuilder = new StringBuilder()
                   .AppendLine("(() => {")
                   .AppendLine("const results = [];");

            for (int i = 0; i < parameter.Statements.Length; ++i)
            {
                statementBuilder.Append("results[").Append(i).Append("] = ").Append(parameter.Statements[i]).AppendLine(";");
            }

            statementBuilder
                .AppendLine("return results;")
                .AppendLine("})()");

            Engine engine = new Engine();

            object[] results = (object[])engine.Execute(statementBuilder.ToString()).GetCompletionValue().ToObject();

            return Assert(results.Select(Convert.ToInt32), parameter.Sum);
        }

        [Benchmark(Description = "\\jurassic")]
        [BenchmarkCategory("JavaScript")]
        [ArgumentsSource(nameof(Parameters))]
        public bool Jurassic(Parameter parameter)
        {
            StringBuilder statementBuilder = new StringBuilder();

            for (int i = 0; i < parameter.Statements.Length; ++i)
            {
                statementBuilder.Append("setValue(").Append(i).Append(", ").Append(parameter.Statements[i]).AppendLine(");");
            }

            Jurassic.ScriptEngine engine = new Jurassic.ScriptEngine();

            int[] results = new int[parameter.Statements.Length];

            engine.SetGlobalFunction("setValue", new Action<int, int>((index, value) => results[index] = value));

            engine.Execute(statementBuilder.ToString());

            return Assert(results, parameter.Sum);
        }

        [Benchmark(Description = "\\lambdaparser")]
        [BenchmarkCategory("C#")]
        [ArgumentsSource(nameof(Parameters))]
        public bool LambdaParser(Parameter parameter)
        {
            StringBuilder statementBuilder = new StringBuilder()
                .AppendLine("new [] {")
                .AppendJoin("," + Environment.NewLine, parameter.Statements)
                .AppendLine("}");

            LambdaParser lambdaParser = new LambdaParser();

            object[] results = (object[])lambdaParser.Eval(statementBuilder.ToString(), _ => null);

            return Assert(results.Select(Convert.ToInt32), parameter.Sum);
        }

        [Benchmark(Description = "\\moonsharp")]
        [BenchmarkCategory("Lua")]
        [ArgumentsSource(nameof(Parameters))]
        public bool MoonSharp(Parameter parameter)
        {
            StringBuilder statementBuilder = new StringBuilder()
                .AppendLine("return {")
                .AppendJoin("," + Environment.NewLine, parameter.Statements)
                .AppendLine("}");

            MoonSharp.Interpreter.Script script = new MoonSharp.Interpreter.Script();

            int[] results = script.DoString(statementBuilder.ToString()).Table.Values.Select(v => (int)v.Number).ToArray();

            return Assert(results, parameter.Sum);
        }

        [Benchmark(Baseline = true, Description = "!\\native")]
        [BenchmarkCategory("C#")]
        [ArgumentsSource(nameof(Parameters))]
        public bool Native(Parameter parameter)
        {
            List<int> results = new List<int>(parameter.Statements.Length);
            for (int i = 0; i < parameter.Statements.Length; ++i)
            {
                int result = i + i;

                results.Add(result);
            }

            return Assert(results, parameter.Sum);
        }

        [Benchmark(Description = "\\niljs")]
        [BenchmarkCategory("JavaScript")]
        [ArgumentsSource(nameof(Parameters))]
        public bool NiLJS(Parameter parameter)
        {
            StringBuilder statementBuilder = new StringBuilder()
                   .AppendLine("(() => {")
                   .AppendLine("const results = [];");

            for (int i = 0; i < parameter.Statements.Length; ++i)
            {
                statementBuilder.Append("results[").Append(i).Append("] = ").Append(parameter.Statements[i]).AppendLine(";");
            }

            statementBuilder
                .AppendLine("return results;")
                .AppendLine("})()");

            Context context = new Context();

            int[] results = context.Eval(statementBuilder.ToString()).As<int[]>();

            return Assert(results, parameter.Sum);
        }

        [Benchmark(Description = "\\nlua")]
        [BenchmarkCategory("Lua")]
        [ArgumentsSource(nameof(Parameters))]
        public bool NLua(Parameter parameter)
        {
            using (NLua.Lua lua = new NLua.Lua())
            {
                StringBuilder statementBuilder = new StringBuilder()
                    .AppendLine("return {")
                    .AppendJoin("," + Environment.NewLine, parameter.Statements)
                    .AppendLine("}");

                NLua.LuaTable table = lua.DoString(statementBuilder.ToString())[0] as NLua.LuaTable;

                int[] results = table.Values.Cast<object>().Select(Convert.ToInt32).ToArray();

                return Assert(results, parameter.Sum);
            }
        }

        public IEnumerable<Parameter> Parameters()
        {
            yield return new Parameter(100);
            yield return new Parameter(1000);
        }

        [Benchmark(Description = "\\pythonnet")]
        [BenchmarkCategory("Python")]
        [ArgumentsSource(nameof(Parameters))]
        public bool PythonNet(Parameter parameter)
        {
            using (Python.Runtime.Py.GIL())
            {
                using (Python.Runtime.PyScope scope = Python.Runtime.Py.CreateScope())
                {
                    StringBuilder statementBuilder = new StringBuilder()
                               .AppendLine("[")
                               .AppendJoin("," + Environment.NewLine, parameter.Statements)
                               .AppendLine("]");

                    int[] results = scope.Eval<int[]>(statementBuilder.ToString());

                    return Assert(results, parameter.Sum);
                }
            }
        }

        [Benchmark(Description = "\\roslyn")]
        [BenchmarkCategory("C#")]
        [ArgumentsSource(nameof(Parameters))]
        public async Task<bool> Roslyn(Parameter parameter)
        {
            StringBuilder statementBuilder = new StringBuilder()
                .Append("int[] results = new int[").Append(parameter.Statements.Length).AppendLine("];");

            for (int i = 0; i < parameter.Statements.Length; ++i)
            {
                statementBuilder.Append("results[").Append(i).Append("] = ").Append(parameter.Statements[i]).AppendLine(";");
            }

            statementBuilder.AppendLine("return results;");

            int[] results = await Microsoft.CodeAnalysis.CSharp.Scripting.CSharpScript.EvaluateAsync<int[]>(statementBuilder.ToString()).ConfigureAwait(false);

            return Assert(results, parameter.Sum);
        }

        [GlobalSetup]
        public void Setup()
        {
            // workaround piloting because jitting takes too long because of long cold start
            foreach (Parameter parameter in Parameters())
            {
                ClearScript(parameter);
                Task.Run(() => Roslyn(parameter)).Wait();
                DynamicExpresso(parameter);
                EvalExpressionNET(parameter);
                ExpressionEvaluator(parameter);
                IronPython(parameter);
                Jint(parameter);
                Jurassic(parameter);
                LambdaParser(parameter);
                MoonSharp(parameter);
                Native(parameter);
                NiLJS(parameter);
                PythonNet(parameter);
            }
        }

        #endregion Public Methods

        #region Private Methods

        private bool Assert(IEnumerable<int> results, int expectedSum)
        {
            if (results.Sum() != expectedSum)
            {
                throw new InvalidOperationException("");
            }

            return true;
        }

        #endregion Private Methods
    }
}