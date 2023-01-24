# Configuration binding micro-benchmark

This repo contains micro-benchmark of different implementations of a routine used to bind configuration to an
IDictionary<TKey, TValue> in Microsoft.Extensions.Configuration.Binder.

## Implementations overview

**Default** - the implemention used in ConfigurationBinder as of version 7.0.2.

**LazyReflection** - avoids making Reflection API if call results are not used.

**AddKvp** - uses ICollection<KeyValuePair<TKey, TValue>>.Add to add elements to the resulting dictionary. This removes
the need to get and possibly box Key and Value from the KeyValuePair.

**Ctor** - calls Dictionary constructor to copy existing values to the result. This reduces the number of Reflection
calls to making generic Dictionary type and looking up the appropriate constructor. The constructor also pre-allocates
internal data structures according to the number of items in the source collection greatly increasing performance with
large number of elements.

**MakeMethod** - reduces the number of Reflection API calls required to call a Dictionary constructor by using a factory
method created from a generic method. Binding using constructor requires constructing a generic Dictionary type, looking
up constructor on that type and invoking it using Reflection API while factory method only requires making the method
and invoking it.

**Factory** - similar to **MakeMethod** but in this one a generic factory type is instantiated and invoked via abstract
base type.

## Benchmark results

``` ini

BenchmarkDotNet=v0.13.4, OS=Windows 11 (10.0.22000.1455/21H2)
11th Gen Intel Core i5-11400H 2.70GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK=7.0.102
  [Host]     : .NET 7.0.2 (7.0.222.60605), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.2 (7.0.222.60605), X64 RyuJIT AVX2


```
|         Method | ItemCount |      Mean |    Error |   StdDev | Ratio |
|--------------- |---------- |----------:|---------:|---------:|------:|
|        **Default** |        **-1** |  **53.84 μs** | **0.293 μs** | **0.274 μs** |  **1.00** |
| LazyReflection |        -1 |  29.37 μs | 0.198 μs | 0.165 μs |  0.55 |
|         AddKvp |        -1 |  32.87 μs | 0.275 μs | 0.244 μs |  0.61 |
|           Ctor |        -1 |  28.69 μs | 0.160 μs | 0.134 μs |  0.53 |
|     MakeMethod |        -1 |  42.82 μs | 0.243 μs | 0.216 μs |  0.80 |
|        Factory |        -1 |  30.26 μs | 0.148 μs | 0.131 μs |  0.56 |
|                |           |           |          |          |       |
|        **Default** |         **0** |  **54.76 μs** | **0.329 μs** | **0.308 μs** |  **1.00** |
| LazyReflection |         0 |  31.82 μs | 0.239 μs | 0.224 μs |  0.58 |
|         AddKvp |         0 |  36.09 μs | 0.252 μs | 0.211 μs |  0.66 |
|           Ctor |         0 |  71.42 μs | 0.543 μs | 0.481 μs |  1.30 |
|     MakeMethod |         0 |  44.24 μs | 0.165 μs | 0.155 μs |  0.81 |
|        Factory |         0 |  30.63 μs | 0.157 μs | 0.131 μs |  0.56 |
|                |           |           |          |          |       |
|        **Default** |         **1** |  **72.35 μs** | **0.413 μs** | **0.366 μs** |  **1.00** |
| LazyReflection |         1 |  72.29 μs | 0.344 μs | 0.305 μs |  1.00 |
|         AddKvp |         1 |  47.05 μs | 0.122 μs | 0.102 μs |  0.65 |
|           Ctor |         1 |  72.73 μs | 0.476 μs | 0.445 μs |  1.00 |
|     MakeMethod |         1 |  47.34 μs | 0.270 μs | 0.253 μs |  0.65 |
|        Factory |         1 |  34.22 μs | 0.232 μs | 0.206 μs |  0.47 |
|                |           |           |          |          |       |
|        **Default** |         **2** |  **80.61 μs** | **0.467 μs** | **0.414 μs** |  **1.00** |
| LazyReflection |         2 |  82.02 μs | 0.515 μs | 0.456 μs |  1.02 |
|         AddKvp |         2 |  52.22 μs | 0.200 μs | 0.156 μs |  0.65 |
|           Ctor |         2 |  73.90 μs | 0.416 μs | 0.369 μs |  0.92 |
|     MakeMethod |         2 |  47.99 μs | 0.183 μs | 0.163 μs |  0.60 |
|        Factory |         2 |  34.62 μs | 0.174 μs | 0.145 μs |  0.43 |
|                |           |           |          |          |       |
|        **Default** |         **3** |  **90.24 μs** | **0.649 μs** | **0.607 μs** |  **1.00** |
| LazyReflection |         3 |  90.76 μs | 0.483 μs | 0.452 μs |  1.01 |
|         AddKvp |         3 |  58.18 μs | 0.355 μs | 0.315 μs |  0.64 |
|           Ctor |         3 |  74.16 μs | 0.473 μs | 0.443 μs |  0.82 |
|     MakeMethod |         3 |  48.53 μs | 0.158 μs | 0.132 μs |  0.54 |
|        Factory |         3 |  34.66 μs | 0.232 μs | 0.217 μs |  0.38 |
|                |           |           |          |          |       |
|        **Default** |         **5** | **109.90 μs** | **0.655 μs** | **0.613 μs** |  **1.00** |
| LazyReflection |         5 | 111.94 μs | 0.733 μs | 0.686 μs |  1.02 |
|         AddKvp |         5 |  70.85 μs | 0.689 μs | 0.644 μs |  0.64 |
|           Ctor |         5 |  78.26 μs | 0.371 μs | 0.290 μs |  0.71 |
|     MakeMethod |         5 |  50.30 μs | 0.263 μs | 0.246 μs |  0.46 |
|        Factory |         5 |  36.65 μs | 0.254 μs | 0.225 μs |  0.33 |
|                |           |           |          |          |       |
|        **Default** |        **10** | **156.25 μs** | **1.002 μs** | **0.888 μs** |  **1.00** |
| LazyReflection |        10 | 158.78 μs | 1.000 μs | 0.935 μs |  1.02 |
|         AddKvp |        10 |  98.73 μs | 0.684 μs | 0.640 μs |  0.63 |
|           Ctor |        10 |  79.36 μs | 0.379 μs | 0.336 μs |  0.51 |
|     MakeMethod |        10 |  55.82 μs | 0.651 μs | 0.609 μs |  0.36 |
|        Factory |        10 |  40.30 μs | 0.251 μs | 0.222 μs |  0.26 |
|                |           |           |          |          |       |
|        **Default** |       **100** | **963.99 μs** | **5.317 μs** | **4.714 μs** |  **1.00** |
| LazyReflection |       100 | 948.41 μs | 4.158 μs | 3.686 μs |  0.98 |
|         AddKvp |       100 | 622.18 μs | 3.909 μs | 3.656 μs |  0.65 |
|           Ctor |       100 | 145.90 μs | 0.930 μs | 0.825 μs |  0.15 |
|     MakeMethod |       100 | 117.32 μs | 0.556 μs | 0.520 μs |  0.12 |
|        Factory |       100 | 104.44 μs | 0.834 μs | 0.780 μs |  0.11 |
