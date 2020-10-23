using BenchmarkDotNet.Attributes;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ExpressionEvaluation.Benchmark
{
    [Config(typeof(BenchmarkConfig))]
    [MemoryDiagnoser]
    [MinColumn, MaxColumn, MedianColumn, Q1Column, Q3Column]
    [Outliers(Perfolizer.Mathematics.OutlierDetection.OutlierMode.DontRemove)]
    public class ConstantExpressionBenchmarks
    {
        #region Public Classes

        public class Parameter
        {
            #region Public Properties

            public int[] Numbers { get; }

            public int Sum { get; }

            #endregion Public Properties

            #region Public Constructors

            public Parameter(int count)
            {
                Numbers = Enumerable.Range(0, count).ToArray();
                Sum = Enumerable.Range(0, count).Sum() * 2;
            }

            #endregion Public Constructors

            #region Public Methods

            public override string ToString()
            {
                return Numbers.Length.ToString("D6");
            }

            #endregion Public Methods
        }

        public class ScriptGlobals
        {
            #region Public Properties

            public int n { get; set; }

            #endregion Public Properties
        }

        #endregion Public Classes

        #region Private Fields

        private const string EXPRESSION = "n + n";

        #endregion Private Fields

        #region Public Methods

        [Benchmark(Description = "\\clearscript")]
        [BenchmarkCategory("JavaScript")]
        [ArgumentsSource(nameof(Parameters))]
        public bool ClearScript(Parameter parameter)
        {
            using (Microsoft.ClearScript.V8.V8ScriptEngine engine = new Microsoft.ClearScript.V8.V8ScriptEngine())
            {
                List<int> results = new List<int>(parameter.Numbers.Length);
                foreach (int number in parameter.Numbers)
                {
                    engine.Script["n"] = number;

                    int result = Convert.ToInt32(engine.Evaluate(EXPRESSION));

                    results.Add(result);
                }

                return Assert(results, parameter.Sum);
            }
        }

        [Benchmark(Description = "\\clearscriptcompiled")]
        [BenchmarkCategory("JavaScript")]
        [ArgumentsSource(nameof(Parameters))]
        public bool ClearScriptCompiled(Parameter parameter)
        {
            using (Microsoft.ClearScript.V8.V8ScriptEngine engine = new Microsoft.ClearScript.V8.V8ScriptEngine())
            {
                Microsoft.ClearScript.V8.V8Script script = engine.Compile(EXPRESSION);

                List<int> results = new List<int>(parameter.Numbers.Length);
                foreach (int number in parameter.Numbers)
                {
                    engine.Script["n"] = number;

                    int result = Convert.ToInt32(engine.Evaluate(script));

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
            foreach (int number in parameter.Numbers)
            {
                int result = interpreter.Eval<int>(EXPRESSION, new DynamicExpresso.Parameter("n", number));

                results.Add(result);
            }

            return Assert(results, parameter.Sum);
        }

        [Benchmark(Description = "\\dynamicexpressocompiled")]
        [BenchmarkCategory("C#")]
        [ArgumentsSource(nameof(Parameters))]
        public bool DynamicExpressoCompiled(Parameter parameter)
        {
            DynamicExpresso.Interpreter interpreter = new DynamicExpresso.Interpreter();

            Func<int, int> @delegate = interpreter.ParseAsDelegate<Func<int, int>>(EXPRESSION, "n");

            List<int> results = new List<int>(parameter.Numbers.Length);
            foreach (int number in parameter.Numbers)
            {
                int result = @delegate.Invoke(number);

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

                List<int> results = new List<int>(parameter.Numbers.Length);
                foreach (int number in parameter.Numbers)
                {
                    context.AliasLocalVariables["n"] = number;

                    int result = context.Execute<int>(EXPRESSION);

                    results.Add(result);
                }

                return Assert(results, parameter.Sum);
            }
        }

        [Benchmark(Description = "\\evalexpressioncompiled")]
        [BenchmarkCategory("C#")]
        [ArgumentsSource(nameof(Parameters))]
        public bool EvalExpressionNETCompiled(Parameter parameter)
        {
            using (Z.Expressions.EvalContext context = new Z.Expressions.EvalContext())
            {
                context.UseLocalCache = true;
                Func<int, int> @delegate = context.Compile<Func<int, int>>(EXPRESSION, "n");

                List<int> results = new List<int>(parameter.Numbers.Length);
                foreach (int number in parameter.Numbers)
                {
                    int result = @delegate(number);

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

            List<int> results = new List<int>(parameter.Numbers.Length);
            foreach (int number in parameter.Numbers)
            {
                expressionEvaluator.Variables["n"] = number;

                int result = expressionEvaluator.Evaluate<int>(EXPRESSION);

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

            List<int> results = new List<int>(parameter.Numbers.Length);
            foreach (int number in parameter.Numbers)
            {
                scope.SetVariable("n", number);

                int result = engine.Execute<int>(EXPRESSION, scope);

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

            List<int> results = new List<int>(parameter.Numbers.Length);
            foreach (int number in parameter.Numbers)
            {
                engine.SetValue("n", number);

                int result = (int)global::Jint.JsValueExtensions.AsNumber(engine.Execute(EXPRESSION).GetCompletionValue());

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

            List<int> results = new List<int>(parameter.Numbers.Length);
            foreach (int number in parameter.Numbers)
            {
                engine.SetGlobalValue("n", number);

                int result = engine.Evaluate<int>(EXPRESSION);

                results.Add(result);
            }

            return Assert(results, parameter.Sum);
        }

        [Benchmark(Description = "\\jurassiccompiled")]
        [BenchmarkCategory("JavaScript")]
        [ArgumentsSource(nameof(Parameters))]
        public bool JurassicCompiled(Parameter parameter)
        {
            Jurassic.ScriptEngine engine = new Jurassic.ScriptEngine();

            Jurassic.CompiledScript compiledScript = engine.Compile(new Jurassic.StringScriptSource("n = " + EXPRESSION));

            List<int> results = new List<int>(parameter.Numbers.Length);
            foreach (int number in parameter.Numbers)
            {
                engine.SetGlobalValue("n", number);

                compiledScript.Execute(engine);

                int result = engine.GetGlobalValue<int>("n");

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

            List<int> results = new List<int>(parameter.Numbers.Length);
            foreach (int number in parameter.Numbers)
            {
                int result = (int)(decimal)lambdaParser.Eval(EXPRESSION, name => name == "n" ? (object)number : null);

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

            List<int> results = new List<int>(parameter.Numbers.Length);
            foreach (int number in parameter.Numbers)
            {
                script.Globals["n"] = number;

                int result = (int)script.DoString("return " + EXPRESSION).Number;

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
                int result = n + n;

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

            List<int> results = new List<int>(parameter.Numbers.Length);
            foreach (int number in parameter.Numbers)
            {
                context.DefineVariable("n").Assign(number);

                int result = NiL.JS.Extensions.JSValueExtensions.As<int>(context.Eval(EXPRESSION));

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
                List<int> results = new List<int>(parameter.Numbers.Length);
                foreach (int number in parameter.Numbers)
                {
                    lua["n"] = number;

                    int result = Convert.ToInt32(lua.DoString("return " + EXPRESSION)[0]);

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
                    List<int> results = new List<int>(parameter.Numbers.Length);
                    foreach (int number in parameter.Numbers)
                    {
                        scope.Set("n", number);

                        int result = scope.Eval<int>(EXPRESSION);

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
            Script baseScript = Microsoft.CodeAnalysis.CSharp.Scripting.CSharpScript.Create("", globalsType: typeof(ScriptGlobals));

            List<int> results = new List<int>(parameter.Numbers.Length);
            foreach (int number in parameter.Numbers)
            {
                ScriptState<int> result = await baseScript.ContinueWith<int>(EXPRESSION).RunAsync(new ScriptGlobals() { n = number }).ConfigureAwait(false);

                results.Add(result.ReturnValue);
            }

            return Assert(results, parameter.Sum);
        }

        [Benchmark(Description = "\\roslyncompiled")]
        [BenchmarkCategory("C#")]
        [ArgumentsSource(nameof(Parameters))]
        public async Task<bool> RoslynCompiled(Parameter parameter)
        {
            Script<int> script = Microsoft.CodeAnalysis.CSharp.Scripting.CSharpScript.Create<int>(EXPRESSION, globalsType: typeof(ScriptGlobals));

            List<int> results = new List<int>(parameter.Numbers.Length);
            foreach (int number in parameter.Numbers)
            {
                ScriptState<int> state = await script.RunAsync(new ScriptGlobals() { n = number }).ConfigureAwait(false);

                results.Add(state.ReturnValue);
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
                ClearScriptCompiled(parameter);
                DynamicExpresso(parameter);
                DynamicExpressoCompiled(parameter);
                EvalExpressionNET(parameter);
                EvalExpressionNETCompiled(parameter);
                ExpressionEvaluator(parameter);
                IronPython(parameter);
                Jint(parameter);
                Jurassic(parameter);
                JurassicCompiled(parameter);
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