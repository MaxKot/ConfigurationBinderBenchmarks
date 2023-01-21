// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.Configuration;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace ConfigurationBinderBenchmarks
{
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

            Type genericType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
            MethodInfo addMethod = genericType.GetMethod("Add", DeclaredOnlyLookup)!;

            Type kvpType = typeof(KeyValuePair<,>).MakeGenericType(keyType, valueType);
            PropertyInfo keyMethod = kvpType.GetProperty("Key", DeclaredOnlyLookup)!;
            PropertyInfo valueMethod = kvpType.GetProperty("Value", DeclaredOnlyLookup)!;

            object dictionary = Activator.CreateInstance(genericType)!;

            var orig = source as IEnumerable;
            object?[] arguments = new object?[2];

            if (orig != null)
            {
                foreach (object? item in orig)
                {
                    object? k = keyMethod.GetMethod!.Invoke(item, null);
                    object? v = valueMethod.GetMethod!.Invoke(item, null);
                    arguments[0] = k;
                    arguments[1] = v;
                    addMethod.Invoke(dictionary, arguments);
                }
            }

            BindDictionary(dictionary, genericType, config, options);

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
