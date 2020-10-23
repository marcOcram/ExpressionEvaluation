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
    public class VariableExpressionArrayBenchmarks
    {
        #region Public Classes

        public class Parameter
        {
            #region Public Properties

            public int[] Numbers { get; }

            public string[] Statements { get; }

            public int Sum { get; }

            #endregion Public Properties

            #region Public Constructors

            public Parameter(int count)
            {
                Numbers = Enumerable.Range(0, count).ToArray();
                Statements = Numbers.Select(c => $"n[{c}] + n[{c}]").ToArray();
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

        public class ScriptGlobals
        {
            #region Public Properties

            public int[] n { get; set; }

            #endregion Public Properties
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
                List<int> results = new List<int>(parameter.Numbers.Length);
                engine.Script["n"] = parameter.Numbers;
                foreach (int number in parameter.Numbers)
                {
                    int result = Convert.ToInt32(engine.Evaluate(parameter.Statements[number]));

                    results.Add(result);
                }

                return Assert(results, parameter.Sum);
            }
        }

        [Benchmark(Description = "\\dynamicexpresso")]
        [BenchmarkCategory("C#")]
        [ArgumentsSource(nameof(Parameters))]
        public bool DynamicExpresso(Parameter parameter)
        {
            DynamicExpresso.Interpreter interpreter = new DynamicExpresso.Interpreter();

            List<int> results = new List<int>(parameter.Numbers.Length);
            DynamicExpresso.Parameter dynamicParameter = new DynamicExpresso.Parameter("n", parameter.Numbers);
            foreach (int number in parameter.Numbers)
            {
                int result = interpreter.Eval<int>(parameter.Statements[number], dynamicParameter);

                results.Add(result);
            }

            return Assert(results, parameter.Sum);
        }

        [Benchmark(Description = "\\evalexpression")]
        [BenchmarkCategory("C#")]
        [ArgumentsSource(nameof(Parameters))]
        public bool EvalExpressionNET(Parameter parameter)
        {
            using (Z.Expressions.EvalContext context = new Z.Expressions.EvalContext())
            {
                context.UseLocalCache = true;
                context.AliasLocalVariables["n"] = parameter.Numbers;

                List<int> results = new List<int>(parameter.Numbers.Length);
                foreach (int number in parameter.Numbers)
                {
                    int result = context.Execute<int>(parameter.Statements[number]);

                    results.Add(result);
                }

                return Assert(results, parameter.Sum);
            }
        }

        [Benchmark(Description = "\\expressionevaluator")]
        [BenchmarkCategory("C#")]
        [ArgumentsSource(nameof(Parameters))]
        public bool ExpressionEvaluator(Parameter parameter)
        {
            CodingSeb.ExpressionEvaluator.ExpressionEvaluator expressionEvaluator = new CodingSeb.ExpressionEvaluator.ExpressionEvaluator();
            expressionEvaluator.Variables["n"] = parameter.Numbers;

            List<int> results = new List<int>(parameter.Numbers.Length);
            foreach (int number in parameter.Numbers)
            {
                int result = expressionEvaluator.Evaluate<int>(parameter.Statements[number]);

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

            Microsoft.Scripting.Hosting.ScriptScope scope = engine.CreateScope();

            scope.SetVariable("n", parameter.Numbers);

            List<int> results = new List<int>(parameter.Numbers.Length);
            foreach (int number in parameter.Numbers)
            {
                int result = engine.Execute<int>(parameter.Statements[number], scope);

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
            engine.SetValue("n", parameter.Numbers);

            List<int> results = new List<int>(parameter.Numbers.Length);
            foreach (int number in parameter.Numbers)
            {
                int result = (int)global::Jint.JsValueExtensions.AsNumber(engine.Execute(parameter.Statements[number]).GetCompletionValue());

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
            //engine.EnableExposedClrTypes = true;
            engine.SetGlobalValue("n", engine.Array.New(parameter.Numbers.Cast<object>().ToArray()));

            List<int> results = new List<int>(parameter.Numbers.Length);
            foreach (int number in parameter.Numbers)
            {
                int result = engine.Evaluate<int>(parameter.Statements[number]);

                results.Add(result);
            }

            return Assert(results, parameter.Sum);
        }

        [Benchmark(Description = "\\lambdaparser")]
        [BenchmarkCategory("C#")]
        [ArgumentsSource(nameof(Parameters))]
        public bool LambdaParser(Parameter parameter)
        {
            object ProvideVariable(string name)
            {
                return name == "n" ? parameter.Numbers : null;
            }

            NReco.Linq.LambdaParser lambdaParser = new NReco.Linq.LambdaParser();

            List<int> results = new List<int>(parameter.Numbers.Length);
            foreach (int number in parameter.Numbers)
            {
                int result = (int)(decimal)lambdaParser.Eval(parameter.Statements[number], ProvideVariable);

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

            script.Globals["n"] = parameter.Numbers;

            List<int> results = new List<int>(parameter.Numbers.Length);
            foreach (int number in parameter.Numbers)
            {
                int result = (int)script.DoString($"return n[{number + 1}] + n[{number + 1}]").Number;

                results.Add(result);
            }

            return Assert(results, parameter.Sum);
        }

        [Benchmark(Baseline = true, Description = "!\\native")]
        [BenchmarkCategory("C#")]
        [ArgumentsSource(nameof(Parameters))]
        public bool Native(Parameter parameter)
        {
            List<int> results = new List<int>(parameter.Numbers.Length);
            foreach (int n in parameter.Numbers)
            {
                int result = parameter.Numbers[n] + parameter.Numbers[n];

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
            context.DefineVariable("n").Assign(NiL.JS.Core.JSValue.Marshal(parameter.Numbers));

            List<int> results = new List<int>(parameter.Numbers.Length);
            foreach (int number in parameter.Numbers)
            {
                int result = NiL.JS.Extensions.JSValueExtensions.As<int>(context.Eval(parameter.Statements[number]));

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
                lua["n"] = parameter.Numbers;

                List<int> results = new List<int>(parameter.Numbers.Length);
                foreach (int number in parameter.Numbers)
                {
                    int result = Convert.ToInt32(lua.DoString("return " + parameter.Statements[number])[0]);

                    results.Add(result);
                }

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
                    scope.Set("n", parameter.Numbers);

                    List<int> results = new List<int>(parameter.Numbers.Length);
                    foreach (int number in parameter.Numbers)
                    {
                        int result = scope.Eval<int>(parameter.Statements[number]);

                        results.Add(result);
                    }

                    return Assert(results, parameter.Sum);
                }
            }
        }

        [Benchmark(Description = "\\roslyn")]
        [BenchmarkCategory("C#")]
        [ArgumentsSource(nameof(Parameters))]
        public async Task<bool> Roslyn(Parameter parameter)
        {
            List<int> results = new List<int>(parameter.Numbers.Length);
            ScriptGlobals globals = new ScriptGlobals() { n = parameter.Numbers };

            Script baseScript = Microsoft.CodeAnalysis.CSharp.Scripting.CSharpScript.Create("", globalsType: typeof(ScriptGlobals));
            foreach (int number in parameter.Numbers)
            {
                ScriptState<int> result = await baseScript.ContinueWith<int>(parameter.Statements[number]).RunAsync(globals).ConfigureAwait(false);

                results.Add(result.ReturnValue);
            }

            return Assert(results, parameter.Sum);
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
                MoonSharp(parameter);
                Native(parameter);
                NilJS(parameter);
                NLua(parameter);
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