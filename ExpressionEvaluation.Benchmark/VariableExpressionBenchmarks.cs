using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionEvaluation.Benchmark
{
    [Config(typeof(BenchmarkConfig))]
    [MemoryDiagnoser]
    [MinColumn, MaxColumn, MedianColumn, Q1Column, Q3Column]
    [Outliers(Perfolizer.Mathematics.OutlierDetection.OutlierMode.DontRemove)]
    public class VariableExpressionBenchmarks
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
            using (Microsoft.ClearScript.V8.V8ScriptEngine engine = new Microsoft.ClearScript.V8.V8ScriptEngine())
            {
                List<int> results = new List<int>(parameter.Statements.Length);
                foreach (string statement in parameter.Statements)
                {
                    int result = Convert.ToInt32(engine.Evaluate(statement));

                    results.Add(result);
                }

                return Assert(results, parameter.Sum);
            }
        }

        [Benchmark(Description = "\\roslyn")]
        [BenchmarkCategory("C#")]
        [ArgumentsSource(nameof(Parameters))]
        public async Task<bool> Roslyn(Parameter parameter)
        {
            Script baseScript = Microsoft.CodeAnalysis.CSharp.Scripting.CSharpScript.Create("");

            List<int> results = new List<int>(parameter.Statements.Length);
            foreach (string statement in parameter.Statements)
            {
                ScriptState<int> result = await baseScript.ContinueWith<int>(statement).RunAsync().ConfigureAwait(false);

                results.Add(result.ReturnValue);
            }

            return Assert(results, parameter.Sum);
        }

        [Benchmark(Description = "\\dynamicexpresso")]
        [BenchmarkCategory("C#")]
        [ArgumentsSource(nameof(Parameters))]
        public bool DynamicExpresso(Parameter parameter)
        {
            DynamicExpresso.Interpreter interpreter = new DynamicExpresso.Interpreter();

            List<int> results = new List<int>(parameter.Statements.Length);
            foreach (string statement in parameter.Statements)
            {
                int result = interpreter.Eval<int>(statement);

                results.Add(result);
            }

            return Assert(results, parameter.Sum);
        }

        [Benchmark(Description = "\\expressionevaluator")]
        [BenchmarkCategory("C#")]
        [ArgumentsSource(nameof(Parameters))]
        public bool ExpressionEvaluator(Parameter parameter)
        {
            CodingSeb.ExpressionEvaluator.ExpressionEvaluator expressionEvaluator = new CodingSeb.ExpressionEvaluator.ExpressionEvaluator();

            List<int> results = new List<int>(parameter.Statements.Length);
            foreach (string statement in parameter.Statements)
            {
                int result = expressionEvaluator.Evaluate<int>(statement);

                results.Add(result);
            }

            return Assert(results, parameter.Sum);
        }

        [Benchmark(Description = "\\ironpython")]
        [BenchmarkCategory("Python")]
        [ArgumentsSource(nameof(Parameters))]
        public bool IronPython(Parameter parameter)
        {
            Microsoft.Scripting.Hosting.ScriptEngine engine = global::IronPython.Hosting.Python.CreateEngine();

            List<int> results = new List<int>(parameter.Statements.Length);
            foreach (string statement in parameter.Statements)
            {
                int result = engine.Execute<int>(statement);

                results.Add(result);
            }

            return Assert(results, parameter.Sum);
        }

        [Benchmark(Description = "\\jint")]
        [BenchmarkCategory("JavaScript")]
        [ArgumentsSource(nameof(Parameters))]
        public bool Jint(Parameter parameter)
        {
            Jint.Engine engine = new Jint.Engine();

            List<int> results = new List<int>(parameter.Statements.Length);
            foreach (string statement in parameter.Statements)
            {
                int result = (int)global::Jint.JsValueExtensions.AsNumber(engine.Execute(statement).GetCompletionValue());

                results.Add(result);
            }

            return Assert(results, parameter.Sum);
        }

        [Benchmark(Description = "\\jurassic")]
        [BenchmarkCategory("JavaScript")]
        [ArgumentsSource(nameof(Parameters))]
        public bool Jurassic(Parameter parameter)
        {
            Jurassic.ScriptEngine engine = new Jurassic.ScriptEngine();

            List<int> results = new List<int>(parameter.Statements.Length);
            foreach (string statement in parameter.Statements)
            {
                int result = engine.Evaluate<int>(statement);

                results.Add(result);
            }

            return Assert(results, parameter.Sum);
        }

        [Benchmark(Description = "\\lambdaparser")]
        [BenchmarkCategory("C#")]
        [ArgumentsSource(nameof(Parameters))]
        public bool LambdaParser(Parameter parameter)
        {
            NReco.Linq.LambdaParser lambdaParser = new NReco.Linq.LambdaParser();

            List<int> results = new List<int>(parameter.Statements.Length);
            foreach (string statement in parameter.Statements)
            {
                int result = (int)(decimal)lambdaParser.Eval(statement, _ => null);

                results.Add(result);
            }

            return Assert(results, parameter.Sum);
        }

        [Benchmark(Description = "\\moonsharp")]
        [BenchmarkCategory("Lua")]
        [ArgumentsSource(nameof(Parameters))]
        public bool MoonSharp(Parameter parameter)
        {
            MoonSharp.Interpreter.Script script = new MoonSharp.Interpreter.Script();

            List<int> results = new List<int>(parameter.Statements.Length);
            foreach (string statement in parameter.Statements)
            {
                int result = (int)script.DoString("return " + statement).Number;

                results.Add(result);
            }

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
        public bool NilJS(Parameter parameter)
        {
            NiL.JS.Core.Context context = new NiL.JS.Core.Context();

            List<int> results = new List<int>(parameter.Statements.Length);
            foreach (string statement in parameter.Statements)
            {
                int result = NiL.JS.Extensions.JSValueExtensions.As<int>(context.Eval(statement));

                results.Add(result);
            }

            return Assert(results, parameter.Sum);
        }

        [Benchmark(Description = "\\nlua")]
        [BenchmarkCategory("Lua")]
        [ArgumentsSource(nameof(Parameters))]
        public bool NLua(Parameter parameter)
        {
            using (NLua.Lua lua = new NLua.Lua())
            {
                List<int> results = new List<int>(parameter.Statements.Length);
                foreach (string statement in parameter.Statements)
                {
                    int result = Convert.ToInt32(lua.DoString("return " + statement)[0]);

                    results.Add(result);
                }

                return Assert(results, parameter.Sum);
            }
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
                    List<int> results = new List<int>(parameter.Statements.Length);
                    foreach (string statement in parameter.Statements)
                    {
                        int result = scope.Eval<int>(statement);

                        results.Add(result);
                    }

                    return Assert(results, parameter.Sum);
                }
            }
        }

        public IEnumerable<Parameter> Parameters()
        {
            yield return new Parameter(100);
            yield return new Parameter(1000);
        }

        [GlobalSetup]
        public void Setup()
        {
            // workaround piloting because jitting takes too long because of long cold start
            foreach (Parameter parameter in Parameters())
            {
                ClearScript(parameter);
                DynamicExpresso(parameter);
                EvalExpressionNET(parameter);
                ExpressionEvaluator(parameter);
                IronPython(parameter);
                Jint(parameter);
                Jurassic(parameter);
                LambdaParser(parameter);
                Native(parameter);
                NilJS(parameter);
                NLua(parameter);
                PythonNet(parameter);
            }
        }

        [Benchmark(Description = "\\evalexpression")]
        [BenchmarkCategory("C#")]
        [ArgumentsSource(nameof(Parameters))]
        public bool EvalExpressionNET(Parameter parameter)
        {
            using (Z.Expressions.EvalContext context = new Z.Expressions.EvalContext())
            {
                context.UseLocalCache = true;

                List<int> results = new List<int>(parameter.Statements.Length);
                foreach (string statement in parameter.Statements)
                {
                    int result = context.Execute<int>(statement);

                    results.Add(result);
                }

                return Assert(results, parameter.Sum);
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