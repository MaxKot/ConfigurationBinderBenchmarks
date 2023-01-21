using BenchmarkDotNet.Attributes;

namespace ConfigurationBinderBenchmarks
{
    public class BindingImplementations
    {
        [Params(-1, 0, 1, 2, 3, 5, 10, 100)]
        public int ItemCount;

        private IDictionary<string, int>? _source;

        private const int Iterations = 100;

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
                    (_source, typeof(IDictionary<string, int>), null!, null!);
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
                    (_source, typeof(IDictionary<string, int>), null!, null!);
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
                    (_source, typeof(IDictionary<string, int>), null!, null!);
            }

            return result;
        }

        [Benchmark]
        public object? Ctor()
        {
            var result = new object?[Iterations];
            for (var i = 0; i < Iterations; ++i)
            {
                result[i] = ConfigurationBinder.BindDictionaryInterface_Ctor
                    (_source, typeof(IDictionary<string, int>), null!, null!);
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
                    (_source, typeof(IDictionary<string, int>), null!, null!);
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
                    (_source, typeof(IDictionary<string, int>), null!, null!);
            }

            return result;
        }
    }
}
