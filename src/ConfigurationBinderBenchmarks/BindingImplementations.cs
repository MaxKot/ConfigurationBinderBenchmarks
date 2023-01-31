using BenchmarkDotNet.Attributes;

namespace ConfigurationBinderBenchmarks
{
    public class BindingImplementations
    {
        [Params(-1, 0, 1, 2, 3, 5, 10, 100)]
        public int ItemCount;

        private IDictionary<string, int>? _source;

        private const int Iterations = 100;

        [Params(typeof(IReadOnlyDictionary<string, int>), typeof (IDictionary<string, int>))]
        public Type DictionaryType = null!;

        [GlobalSetup]
        public void GlobalSetup()
        {
            if (ItemCount < 0)
            {
                _source = null;
            }
            else
            {
                _source = new Dictionary<string, int>(ItemCount);
                for (var i = 0; i < ItemCount; ++i)
                {
                    _source.Add(i.ToString(), i);
                }
            }
        }

        [Benchmark(Baseline = true)]
        public object? Default()
        {
            var result = new object?[Iterations];
            for (var i = 0; i < Iterations; ++i)
            {
                result[i] = ConfigurationBinder.BindDictionaryInterface
                    (_source, DictionaryType, null!, null!);
            }

            return result;
        }

        [Benchmark]
        public object? LazyReflection()
        {
            var result = new object?[Iterations];
            for (var i = 0; i < Iterations; ++i)
            {
                result[i] = ConfigurationBinder.BindDictionaryInterface_LazyReflection
                    (_source, DictionaryType, null!, null!);
            }

            return result;
        }

        [Benchmark]
        public object? AddKvp()
        {
            var result = new object?[Iterations];
            for (var i = 0; i < Iterations; ++i)
            {
                result[i] = ConfigurationBinder.BindDictionaryInterface_AddKvp
                    (_source, DictionaryType, null!, null!);
            }

            return result;
        }

        [Benchmark]
        public object? MakeMethod()
        {
            var result = new object?[Iterations];
            for (var i = 0; i < Iterations; ++i)
            {
                result[i] = ConfigurationBinder.BindDictionaryInterface_MakeMethod
                    (_source, DictionaryType, null!, null!);
            }

            return result;
        }

        [Benchmark]
        public object? Factory()
        {
            var result = new object?[Iterations];
            for (var i = 0; i < Iterations; ++i)
            {
                result[i] = ConfigurationBinder.BindDictionaryInterface_Factory
                    (_source, DictionaryType, null!, null!);
            }

            return result;
        }
    }
}
