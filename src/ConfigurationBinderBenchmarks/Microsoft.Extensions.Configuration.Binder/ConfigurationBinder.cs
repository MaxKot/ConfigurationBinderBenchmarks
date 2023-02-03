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
        {
            if (source == null)
            {
                return new Dictionary<TKey, TValue>();
            }

            if (source is Dictionary<TKey, TValue> sourceDictionary)
            {
                return new Dictionary<TKey, TValue>(sourceDictionary);
            }

            var sourceCollection = (IReadOnlyCollection<KeyValuePair<TKey, TValue>>)source;
            var result = new Dictionary<TKey, TValue>(sourceCollection.Count);
            foreach (var kvp in sourceCollection)
            {
                result.Add(kvp.Key, kvp.Value);
            }
            return result;
        }
    }

    public static class ConfigurationBinder
    {
        private const BindingFlags DeclaredOnlyLookup = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
        private const string DynamicCodeWarningMessage = "Binding strongly typed objects to configuration values requires generating dynamic code at runtime, for example instantiating generic types.";
        private const string TrimmingWarningMessage = "In case the type is non-primitive, the trimmer cannot statically analyze the object's type so its members may be trimmed.";

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

            BindDictionary_Noop(source, dictionaryType, config, options);

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

            if(source is null)
            {
                dictionaryType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
                source = Activator.CreateInstance(dictionaryType);
            }
            else
            {
                // addMethod can only be null if dictionaryType is IReadOnlyDictionary<TKey, TValue> rather than IDictionary<TKey, TValue>.
                MethodInfo? addMethod = dictionaryType.GetMethod("Add", DeclaredOnlyLookup);
                if (addMethod is null)
                {
                    dictionaryType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
                    var dictionary = Activator.CreateInstance(dictionaryType);

                    var orig = source as IEnumerable;
                    if (orig is not null)
                    {
                        IEnumerator enumerator = orig.GetEnumerator();
                        // This should be the same as the code generated by the compiler for foreach loop
                        // with some extra logic added on the first iteration.
                        try
                        {
                            if (enumerator.MoveNext())
                            {
                                addMethod = dictionaryType.GetMethod("Add", DeclaredOnlyLookup)!;

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

                    source = dictionary;
                }
            }

            Debug.Assert(source is not null);
            Debug.Assert(source.GetType().GetMethod("Add", DeclaredOnlyLookup) is not null);

            BindDictionary_Noop(source, dictionaryType, config, options);

            return source;
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

            // addMethod can only be null if dictionaryType is IReadOnlyDictionary<TKey, TValue> rather than IDictionary<TKey, TValue>.
            MethodInfo? addMethod = dictionaryType.GetMethod("Add", DeclaredOnlyLookup);
            if (addMethod is null || source is null)
            {
                dictionaryType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
                var dictionary = Activator.CreateInstance(dictionaryType);
                // The name of an explicitly implementation method is compiler-specific, but here
                // Dictionary type can be assumed to be compiled with a specific compiler version.
                // Also, C# compiler behavior in this respect does not seem to change in over 10 years.
                const string addMethodName =
                    "System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<TKey,TValue>>.Add";
                addMethod = dictionaryType.GetMethod(addMethodName, DeclaredOnlyLookup)!;

                var orig = source as IEnumerable;
                if (orig is not null)
                {
                    object?[] arguments = new object?[1];

                    foreach (object? item in orig)
                    {
                        arguments[0] = item;
                        addMethod.Invoke(dictionary, arguments);
                    }
                }

                source = dictionary;
            }

            Debug.Assert(source is not null);
            Debug.Assert(addMethod is not null);

            BindDictionary_Noop(source, dictionaryType, config, options);

            return source;
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

            // addMethod can only be null if dictionaryType is IReadOnlyDictionary<TKey, TValue> rather than IDictionary<TKey, TValue>.
            if (source == null || dictionaryType.GetMethod("Add", DeclaredOnlyLookup) == null)
            {
                MethodInfo method = CopyDictionaryInterfaceMethod.MakeGenericMethod(keyType, valueType);
                source = method.Invoke(null, new object?[] { source })!;
            }

            Debug.Assert(source is not null);
            Debug.Assert(source.GetType().GetMethod("Add", DeclaredOnlyLookup) is not null);

            BindDictionary_Noop(source, source.GetType(), config, options);

            return source;
        }

        private static object CopyDictionaryInterface<TKey, TValue>(
            IReadOnlyCollection<KeyValuePair<TKey, TValue>> source)
            where TKey : notnull
        {
            if (source == null)
            {
                return new Dictionary<TKey, TValue>();
            }

            if(source is Dictionary<TKey, TValue> dictionary)
            {
                return new Dictionary<TKey, TValue>(dictionary);
            }

            var result = new Dictionary<TKey, TValue>(source.Count);
            foreach (var kvp in source)
            {
                result.Add(kvp.Key, kvp.Value);
            }
            return result;
        }

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

            // addMethod can only be null if dictionaryType is IReadOnlyDictionary<TKey, TValue> rather than IDictionary<TKey, TValue>.
            if (source == null || dictionaryType.GetMethod("Add", DeclaredOnlyLookup) == null)
            {
                Type factoryType = typeof(DictionaryInterfaceFactory<,>).MakeGenericType(keyType, valueType);
                DictionaryInterfaceFactory factory = (DictionaryInterfaceFactory)Activator.CreateInstance(factoryType)!;

                source = factory.Copy(source);
            }

            Debug.Assert(source is not null);
            Debug.Assert(source.GetType().GetMethod("Add", DeclaredOnlyLookup) is not null);

            BindDictionary_Noop(source, source.GetType(), config, options);

            return source;
        }

        [RequiresDynamicCode(DynamicCodeWarningMessage)]
        [RequiresUnreferencedCode("Cannot statically analyze what the element type is of the value objects in the dictionary so its members may be trimmed.")]
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Method is intentionally left empty to keep modifications to the benchmarked code minimal.")]
        private static void BindDictionary_Noop(
            object dictionary,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties)]
            Type dictionaryType,
            IConfiguration config, BinderOptions options)
        {

        }

        // Binds and potentially overwrites a dictionary object.
        // This differs from BindDictionaryInterface because this method doesn't clone
        // the dictionary; it sets and/or overwrites values directly.
        // When a user specifies a concrete dictionary or a concrete class implementing IDictionary<,>
        // in their config class, then that value is used as-is. When a user specifies an interface (instantiated)
        // in their config class, then it is cloned to a new dictionary, the same way as other collections.
        [RequiresDynamicCode(DynamicCodeWarningMessage)]
        [RequiresUnreferencedCode("Cannot statically analyze what the element type is of the value objects in the dictionary so its members may be trimmed.")]
        public static void BindDictionary(
            object dictionary,
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties)]
            Type dictionaryType,
            IConfiguration config, BinderOptions options)
        {
            Debug.Assert(dictionaryType.IsGenericType &&
                         (dictionaryType.GetGenericTypeDefinition() == typeof(IDictionary<,>) || dictionaryType.GetGenericTypeDefinition() == typeof(Dictionary<,>)));

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
                return;
            }

            MethodInfo tryGetValue = dictionaryType.GetMethod("TryGetValue", DeclaredOnlyLookup)!;
            PropertyInfo indexerProperty = dictionaryType.GetProperty("Item", DeclaredOnlyLookup)!;

            foreach (IConfigurationSection child in config.GetChildren())
            {
                try
                {
                    object key = keyTypeIsEnum ? Enum.Parse(keyType, child.Key, true) :
                        keyTypeIsInteger ? Convert.ChangeType(child.Key, keyType) :
                        child.Key;

                    var valueBindingPoint = new BindingPoint(
                        initialValueProvider: () =>
                        {
                            object?[] tryGetValueArgs = { key, null };
                            return (bool)tryGetValue.Invoke(dictionary, tryGetValueArgs)! ? tryGetValueArgs[1] : null;
                        },
                        isReadOnly: false);
                    BindInstance(
                        type: valueType,
                        bindingPoint: valueBindingPoint,
                        config: child,
                        options: options);
                    if (valueBindingPoint.HasNewValue)
                    {
                        indexerProperty.SetValue(dictionary, valueBindingPoint.Value, new object[] { key });
                    }
                }
                catch (Exception ex)
                {
                    if (options.ErrorOnUnknownConfiguration)
                    {
                        throw new InvalidOperationException(String.Format("Error_GeneralErrorWhenBinding {0}",
                            nameof(options.ErrorOnUnknownConfiguration)), ex);
                    }
                }
            }
        }

        [RequiresDynamicCode(DynamicCodeWarningMessage)]
        [RequiresUnreferencedCode(TrimmingWarningMessage)]
        private static void BindInstance(
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type type,
            BindingPoint bindingPoint,
            IConfiguration config,
            BinderOptions options)
        {

        }
    }
}
