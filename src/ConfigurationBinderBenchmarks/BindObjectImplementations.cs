using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace ConfigurationBinderBenchmarks
{
    public class BindObjectImplementations
    {
        [Params(0)]
        public int Initial;

        [Params(0, 1, 2, 3, 5, 10, 100)]
        public int Added;

        [Params(true, false)]
        public bool AccessExistingValue;

        private const int Iterations = 100;

        private Dictionary<string, int>[] _sources = null!;

        private BinderOptions _options = null!;

        private IConfiguration _configuration = null!;

        private DictionaryBinder _binder = null!;

        [GlobalSetup]
        public void GlobalSetup()
        {
            var source = new Dictionary<string, int>(Initial);
            for (var j = 0; j < Initial; ++j)
            {
                source.Add($"i{j}", j);
            }

            var sources = new Dictionary<string, int>[Iterations];
            for (var i = 0; i < Iterations; i++)
            {
                sources[i] = new Dictionary<string, int>(source);
            }
            _sources = sources;

            _options = new BinderOptions();

            var addedItems = new List<KeyValuePair<string, string?>>(Added);
            for (var i = 0; i < Added; ++i)
            {
                var value = i.ToString(CultureInfo.InvariantCulture);
                addedItems.Add(new KeyValuePair<string, string?>($"a{i}", value));
            }

            var configuration = new ConfigurationManager();
            configuration.AddInMemoryCollection(addedItems);
            _configuration = configuration;

            _binder = new DictionaryBinder<string, int>();
        }

        [Benchmark(Baseline = true)]
        public object? Default()
        {
            var result = new object?[Iterations];
            for (var i = 0; i < Iterations; ++i)
            {
                var source = _sources[i];
                ConfigurationBinder.BindDictionary(
                    source, typeof (Dictionary<string, int>), _configuration, _options,
                    AccessExistingValue);
                result[i] = source;
            }

            return result;
        }

        [Benchmark]
        public object? LazyReflection()
        {
            var result = new object?[Iterations];
            for (var i = 0; i < Iterations; ++i)
            {
                var source = _sources[i];
                ConfigurationBinder.BindDictionary_LazyReflection(
                    source, typeof(Dictionary<string, int>), _configuration, _options,
                    AccessExistingValue);
                result[i] = source;
            }

            return result;
        }

        [Benchmark]
        public object? NewBinder()
        {
            var result = new object?[Iterations];
            for (var i = 0; i < Iterations; ++i)
            {
                var source = _sources[i];
                ConfigurationBinder.BindDictionary_Helper(
                    source, typeof(Dictionary<string, int>), _configuration, _options,
                    AccessExistingValue);
                result[i] = source;
            }

            return result;
        }

        [Benchmark]
        public object? ExistingBinder()
        {
            var result = new object?[Iterations];
            for (var i = 0; i < Iterations; ++i)
            {
                var source = _sources[i];
                ConfigurationBinder.BindDictionary_Helper(
                    source, typeof(Dictionary<string, int>), _configuration, _options,
                    AccessExistingValue, _binder);
                result[i] = source;
            }

            return result;
        }

        [Benchmark]
        public object? NonGeneric()
        {
            var result = new object?[Iterations];
            for (var i = 0; i < Iterations; ++i)
            {
                var source = _sources[i];
                ConfigurationBinder.BindDictionary_NonGeneric(
                    source, typeof(Dictionary<string, int>), _configuration, _options,
                    AccessExistingValue);
                result[i] = source;
            }

            return result;
        }
    }
}
