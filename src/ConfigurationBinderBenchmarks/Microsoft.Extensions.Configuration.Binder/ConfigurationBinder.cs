// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Configuration;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace ConfigurationBinderBenchmarks
{
    internal abstract class DictionaryInterfaceFactory
    {
        public abstract object Copy(object? source);
    }

    internal sealed class DictionaryInterfaceFactory<TKey, TValue>
        : DictionaryInterfaceFactory
        where TKey: notnull
    {
        public override object Copy(object? source)
            => source != null
                ? new Dictionary<TKey, TValue>((IDictionary<TKey, TValue>)source)
                : new Dictionary<TKey, TValue>();
    }

    public static class ConfigurationBinder
    {
        private const BindingFlags DeclaredOnlyLookup = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
        private const string DynamicCodeWarningMessage = "Binding strongly typed objects to configuration values requires generating dynamic code at runtime, for example instantiating generic types.";

        [RequiresDynamicCode(DynamicCodeWarningMessage)]
        [RequiresUnreferencedCode("Cannot statically analyze what the element type is of the value objects in the dictionary so its members may be trimmed.")]
        public static object? BindDictionaryInterface(
            object? source,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties)]
            Type dictionaryType,
            IConfiguration config, BinderOptions options)
        {
            // IDictionary<K,V> is guaranteed to have exactly two parameters
            Type keyType = dictionaryType.GenericTypeArguments[0];
            Type valueType = dictionaryType.GenericTypeArguments[1];
            bool keyTypeIsEnum = keyType.IsEnum;
            bool keyTypeIsInteger =
                keyType == typeof(sbyte) ||
                keyType == typeof(byte) ||
                keyType == typeof(short) ||
                keyType == typeof(ushort) ||
                keyType == typeof(int) ||
                keyType == typeof(uint) ||
                keyType == typeof(long) ||
                keyType == typeof(ulong);

            if (keyType != typeof(string) && !keyTypeIsEnum && !keyTypeIsInteger)
            {
                // We only support string, enum and integer (except nint-IntPtr and nuint-UIntPtr) keys
                return null;
            }

            // addMethod can only be null if dictionaryType is IReadOnlyDictionary<TKey, TValue> rather than IDictionary<TKey, TValue>.
            MethodInfo? addMethod = dictionaryType.GetMethod("Add", DeclaredOnlyLookup);
            if (addMethod is null || source is null)
            {
                dictionaryType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
                var dictionary = Activator.CreateInstance(dictionaryType);
                addMethod = dictionaryType.GetMethod("Add", DeclaredOnlyLookup);

                var orig = source as IEnumerable;
                if (orig is not null)
                {
                    Type kvpType = typeof(KeyValuePair<,>).MakeGenericType(keyType, valueType);
                    PropertyInfo keyMethod = kvpType.GetProperty("Key", DeclaredOnlyLookup)!;
                    PropertyInfo valueMethod = kvpType.GetProperty("Value", DeclaredOnlyLookup)!;
                    object?[] arguments = new object?[2];

                    foreach (object? item in orig)
                    {
                        object? k = keyMethod.GetMethod!.Invoke(item, null);
                        object? v = valueMethod.GetMethod!.Invoke(item, null);
                        arguments[0] = k;
                        arguments[1] = v;
                        addMethod!.Invoke(dictionary, arguments);
                    }
                }

                source = dictionary;
            }

            Debug.Assert(source is not null);
            Debug.Assert(addMethod is not null);

            BindDictionary(source, dictionaryType, config, options);

            return source;
        }

        [RequiresDynamicCode(DynamicCodeWarningMessage)]
        [RequiresUnreferencedCode("Cannot statically analyze what the element type is of the value objects in the dictionary so its members may be trimmed.")]
        public static object? BindDictionaryInterface_LazyReflection(
            object? source,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties)]
            Type dictionaryType,
            IConfiguration config, BinderOptions options)
        {
            // IDictionary<K,V> is guaranteed to have exactly two parameters
            Type keyType = dictionaryType.GenericTypeArguments[0];
            Type valueType = dictionaryType.GenericTypeArguments[1];
            bool keyTypeIsEnum = keyType.IsEnum;
            bool keyTypeIsInteger =
                keyType == typeof(sbyte) ||
                keyType == typeof(byte) ||
                keyType == typeof(short) ||
                keyType == typeof(ushort) ||
                keyType == typeof(int) ||
                keyType == typeof(uint) ||
                keyType == typeof(long) ||
                keyType == typeof(ulong);

            if (keyType != typeof(string) && !keyTypeIsEnum && !keyTypeIsInteger)
            {
                // We only support string, enum and integer (except nint-IntPtr and nuint-UIntPtr) keys
                return null;
            }

            Type genericType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);

            object dictionary = Activator.CreateInstance(genericType)!;

            var orig = source as IEnumerable;

            if (orig != null)
            {
                IEnumerator enumerator = orig.GetEnumerator();
                // This should be the same as the code generated by the compiler for foreach loop
                // with some extra logic added on the first iteration.
                try
                {
                    if (enumerator.MoveNext())
                    {
                        MethodInfo addMethod = genericType.GetMethod("Add", DeclaredOnlyLookup)!;

                        Type kvpType = typeof(KeyValuePair<,>).MakeGenericType(keyType, valueType);
                        PropertyInfo keyMethod = kvpType.GetProperty("Key", DeclaredOnlyLookup)!;
                        PropertyInfo valueMethod = kvpType.GetProperty("Value", DeclaredOnlyLookup)!;
                        object?[] arguments = new object?[2];

                        do
                        {
                            object? item = enumerator.Current;

                            object? k = keyMethod.GetMethod!.Invoke(item, null);
                            object? v = valueMethod.GetMethod!.Invoke(item, null);
                            arguments[0] = k;
                            arguments[1] = v;
                            addMethod.Invoke(dictionary, arguments);
                        } while (enumerator.MoveNext());
                    }
                }
                finally
                {
                    if (enumerator is IDisposable disposableEnumerator)
                    {
                        disposableEnumerator.Dispose();
                    }
                }
            }

            BindDictionary(dictionary, genericType, config, options);

            return dictionary;
        }

        [RequiresDynamicCode(DynamicCodeWarningMessage)]
        [RequiresUnreferencedCode("Cannot statically analyze what the element type is of the value objects in the dictionary so its members may be trimmed.")]
        public static object? BindDictionaryInterface_AddKvp(
            object? source,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties)]
            Type dictionaryType,
            IConfiguration config, BinderOptions options)
        {
            // IDictionary<K,V> is guaranteed to have exactly two parameters
            Type keyType = dictionaryType.GenericTypeArguments[0];
            Type valueType = dictionaryType.GenericTypeArguments[1];
            bool keyTypeIsEnum = keyType.IsEnum;
            bool keyTypeIsInteger =
                keyType == typeof(sbyte) ||
                keyType == typeof(byte) ||
                keyType == typeof(short) ||
                keyType == typeof(ushort) ||
                keyType == typeof(int) ||
                keyType == typeof(uint) ||
                keyType == typeof(long) ||
                keyType == typeof(ulong);

            if (keyType != typeof(string) && !keyTypeIsEnum && !keyTypeIsInteger)
            {
                // We only support string, enum and integer (except nint-IntPtr and nuint-UIntPtr) keys
                return null;
            }

            Type genericType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
            // The name of an explicitly implementation method is compiler-specific, but here
            // Dictionary type can be assumed to be compiled with a specific compiler version.
            // Also, C# compiler behavior in this respect does not seem to change in over 10 years.
            const string addMethodName =
                "System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<TKey,TValue>>.Add";
            MethodInfo addMethod = genericType.GetMethod(addMethodName, DeclaredOnlyLookup)!;

            object dictionary = Activator.CreateInstance(genericType)!;

            var orig = source as IEnumerable;
            object?[] arguments = new object?[1];

            if (orig != null)
            {
                foreach (object? item in orig)
                {
                    arguments[0] = item;
                    addMethod.Invoke(dictionary, arguments);
                }
            }

            BindDictionary(dictionary, genericType, config, options);

            return dictionary;
        }

        [RequiresDynamicCode(DynamicCodeWarningMessage)]
        [RequiresUnreferencedCode("Cannot statically analyze what the element type is of the value objects in the dictionary so its members may be trimmed.")]
        public static object? BindDictionaryInterface_Ctor(
            object? source,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties)]
            Type dictionaryType,
            IConfiguration config, BinderOptions options)
        {
            // IDictionary<K,V> is guaranteed to have exactly two parameters
            Type keyType = dictionaryType.GenericTypeArguments[0];
            Type valueType = dictionaryType.GenericTypeArguments[1];
            bool keyTypeIsEnum = keyType.IsEnum;
            bool keyTypeIsInteger =
                keyType == typeof(sbyte) ||
                keyType == typeof(byte) ||
                keyType == typeof(short) ||
                keyType == typeof(ushort) ||
                keyType == typeof(int) ||
                keyType == typeof(uint) ||
                keyType == typeof(long) ||
                keyType == typeof(ulong);

            if (keyType != typeof(string) && !keyTypeIsEnum && !keyTypeIsInteger)
            {
                // We only support string, enum and integer (except nint-IntPtr and nuint-UIntPtr) keys
                return null;
            }

            Type genericType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);

            object dictionary;
            if (source != null)
            {
                ConstructorInfo copyConstructor = genericType.GetConstructor(new[] { genericType })!;
                dictionary = copyConstructor.Invoke(new[] { source });
            }
            else
            {
                dictionary = Activator.CreateInstance(genericType)!;
            }

            BindDictionary(dictionary, genericType, config, options);

            return dictionary;
        }

        [RequiresDynamicCode(DynamicCodeWarningMessage)]
        [RequiresUnreferencedCode("Cannot statically analyze what the element type is of the value objects in the dictionary so its members may be trimmed.")]
        public static object? BindDictionaryInterface_MakeMethod(
            object? source,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties)]
            Type dictionaryType,
            IConfiguration config, BinderOptions options)
        {
            // IDictionary<K,V> is guaranteed to have exactly two parameters
            Type keyType = dictionaryType.GenericTypeArguments[0];
            Type valueType = dictionaryType.GenericTypeArguments[1];
            bool keyTypeIsEnum = keyType.IsEnum;
            bool keyTypeIsInteger =
                keyType == typeof(sbyte) ||
                keyType == typeof(byte) ||
                keyType == typeof(short) ||
                keyType == typeof(ushort) ||
                keyType == typeof(int) ||
                keyType == typeof(uint) ||
                keyType == typeof(long) ||
                keyType == typeof(ulong);

            if (keyType != typeof(string) && !keyTypeIsEnum && !keyTypeIsInteger)
            {
                // We only support string, enum and integer (except nint-IntPtr and nuint-UIntPtr) keys
                return null;
            }

            MethodInfo method = CopyDictionaryInterfaceMethod.MakeGenericMethod(keyType, valueType);

            object dictionary = method.Invoke(null, new object?[] { source })!;

            BindDictionary(dictionary, dictionary.GetType(), config, options);

            return dictionary;
        }

        private static object CopyDictionaryInterface<TKey, TValue>(IDictionary<TKey, TValue> source)
            where TKey : notnull
            => source != null
                ? new Dictionary<TKey, TValue>(source)
                : new Dictionary<TKey, TValue>();

        private static readonly MethodInfo CopyDictionaryInterfaceMethod =
            typeof(ConfigurationBinder)
                .GetMethod(nameof(CopyDictionaryInterface), DeclaredOnlyLookup)!;

        [RequiresDynamicCode(DynamicCodeWarningMessage)]
        [RequiresUnreferencedCode("Cannot statically analyze what the element type is of the value objects in the dictionary so its members may be trimmed.")]
        public static object? BindDictionaryInterface_Factory(
            object? source,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties)]
            Type dictionaryType,
            IConfiguration config, BinderOptions options)
        {
            // IDictionary<K,V> is guaranteed to have exactly two parameters
            Type keyType = dictionaryType.GenericTypeArguments[0];
            Type valueType = dictionaryType.GenericTypeArguments[1];
            bool keyTypeIsEnum = keyType.IsEnum;
            bool keyTypeIsInteger =
                keyType == typeof(sbyte) ||
                keyType == typeof(byte) ||
                keyType == typeof(short) ||
                keyType == typeof(ushort) ||
                keyType == typeof(int) ||
                keyType == typeof(uint) ||
                keyType == typeof(long) ||
                keyType == typeof(ulong);

            if (keyType != typeof(string) && !keyTypeIsEnum && !keyTypeIsInteger)
            {
                // We only support string, enum and integer (except nint-IntPtr and nuint-UIntPtr) keys
                return null;
            }

            Type factoryType = typeof(DictionaryInterfaceFactory<,>).MakeGenericType(keyType, valueType);
            DictionaryInterfaceFactory factory = (DictionaryInterfaceFactory)Activator.CreateInstance(factoryType)!;

            object dictionary = factory.Copy(source);

            BindDictionary(dictionary, dictionary.GetType(), config, options);

            return dictionary;
        }

        [RequiresDynamicCode(DynamicCodeWarningMessage)]
        [RequiresUnreferencedCode("Cannot statically analyze what the element type is of the value objects in the dictionary so its members may be trimmed.")]
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Method is intentionally left empty to keep modifications to the benchmarked code minimal.")]
        private static void BindDictionary(
            object dictionary,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties)]
            Type dictionaryType,
            IConfiguration config, BinderOptions options)
        {

        }
    }
}
