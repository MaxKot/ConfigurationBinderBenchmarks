using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using System.Diagnostics.CodeAnalysis;
using ConfigurationBinder = ConfigurationBinderBenchmarks.ConfigurationBinder;

namespace ConfigurationBinderTests
{
    [TestFixture]
    public sealed class ConfigurationBinderTests
    {
        public delegate object? BindDictionaryInterfaceImpl(
            object? source,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties)]
            Type dictionaryType,
            IConfiguration config, BinderOptions options);

        public static Dictionary<string, int>?[] InitialObjects
            => new[]
            {
                null,
                new Dictionary<string, int> (StringComparer.OrdinalIgnoreCase),
                new Dictionary<string, int> (StringComparer.OrdinalIgnoreCase)
                {
                    { "a", 1 }
                }
            };

        public static Type[] DictionaryTypes
            => new[]
            {
                typeof(IDictionary<string, int>),
                typeof(IReadOnlyDictionary<string, int>)
            };

        public static BindDictionaryInterfaceImpl[] Implementations
            => new BindDictionaryInterfaceImpl[]
            {
                ConfigurationBinder.BindDictionaryInterface_LazyReflection,
                ConfigurationBinder.BindDictionaryInterface_AddKvp,
                ConfigurationBinder.BindDictionaryInterface_MakeMethod,
                ConfigurationBinder.BindDictionaryInterface_Factory
            };

        [Test]
        [Combinatorial]
        public void AlternativeImplementationResultsMatchDefaultImplementation
        (
            [ValueSource(nameof(InitialObjects))]
            Dictionary<string, int>? source,
            [ValueSource(nameof(DictionaryTypes))]
            Type dictionaryType,
            [ValueSource(nameof(Implementations))]
            BindDictionaryInterfaceImpl impl
        )
        {
            IConfiguration config = null!;
            BinderOptions options = new();
            var expected = (Dictionary<string, int>) ConfigurationBinder
                .BindDictionaryInterface(source, dictionaryType, config, options)!;
            var actual = (Dictionary<string, int>)
                impl(source, dictionaryType, config, options)!;

            Assert.That(expected, Is.EqualTo(actual));
            Assert.That(expected.Comparer, Is.EqualTo(actual.Comparer));
        }
    }
}
