# Configuration binding micro-benchmark

This repo contains micro-benchmark of different implementations of a routine used to bind configuration to an
IDictionary<TKey, TValue> in Microsoft.Extensions.Configuration.Binder.

## Implementations overview

**Default** - the implemention used in ConfigurationBinder as of version 7.0.2.

**LazyReflection** - avoids making Reflection API if call results are not used.

**AddKvp** - uses ICollection<KeyValuePair<TKey, TValue>>.Add to add elements to the resulting dictionary. This removes
the need to get and possibly box Key and Value from the KeyValuePair.

**MakeMethod** - moves initialization of a new Dictionary to a generic method and calls it using Reflection. This
reduces the number of Reflection API calls used to copy the source object, uses Dictionary constructor for the
initialization if possible and pre-sets the capacity of the new Dictionary. Pre-setting Dictionary capacity allows to
avoid re-sizing and re-allocations of the Dictionary internal data structures and its copy-constructor has
optimizations for copying from another Dictionary object making it even faster than adding items to a Dictionary with
pre-set capacity.

**Factory** - similar to **MakeMethod** but in this one a generic factory type is instantiated and invoked via abstract
base type.

## Benchmark results

ItemCount is the number of items in the source object. -1 denotes case where the source object is null. DictionaryType
is the target type.

``` ini

BenchmarkDotNet=v0.13.4, OS=Windows 11 (10.0.22000.1455/21H2)
11th Gen Intel Core i5-11400H 2.70GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK=7.0.102
  [Host]     : .NET 7.0.2 (7.0.222.60605), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.2 (7.0.222.60605), X64 RyuJIT AVX2


```
|         Method | ItemCount |                     DictionaryType |      Mean |     Error |    StdDev | Ratio | RatioSD |
|--------------- |---------- |----------------------------------- |----------:|----------:|----------:|------:|--------:|
|        **Default** |        **-1** |         **IDictionary&lt;String, Int32&gt;** |  **35.03 μs** |  **0.390 μs** |  **0.345 μs** |  **1.00** |    **0.00** |
| LazyReflection |        -1 |         IDictionary&lt;String, Int32&gt; |  29.42 μs |  0.313 μs |  0.293 μs |  0.84 |    0.01 |
|         AddKvp |        -1 |         IDictionary&lt;String, Int32&gt; |  35.64 μs |  0.349 μs |  0.309 μs |  1.02 |    0.01 |
|     MakeMethod |        -1 |         IDictionary&lt;String, Int32&gt; |  44.61 μs |  0.576 μs |  0.539 μs |  1.27 |    0.02 |
|        Factory |        -1 |         IDictionary&lt;String, Int32&gt; |  29.11 μs |  0.177 μs |  0.157 μs |  0.83 |    0.01 |
|                |           |                                    |           |           |           |       |         |
|        **Default** |        **-1** | **IReadOnlyDictionary&lt;String, Int32&gt;** |  **33.62 μs** |  **0.268 μs** |  **0.251 μs** |  **1.00** |    **0.00** |
| LazyReflection |        -1 | IReadOnlyDictionary&lt;String, Int32&gt; |  30.03 μs |  0.475 μs |  0.445 μs |  0.89 |    0.01 |
|         AddKvp |        -1 | IReadOnlyDictionary&lt;String, Int32&gt; |  35.52 μs |  0.235 μs |  0.220 μs |  1.06 |    0.01 |
|     MakeMethod |        -1 | IReadOnlyDictionary&lt;String, Int32&gt; |  43.32 μs |  0.254 μs |  0.212 μs |  1.29 |    0.01 |
|        Factory |        -1 | IReadOnlyDictionary&lt;String, Int32&gt; |  29.92 μs |  0.231 μs |  0.216 μs |  0.89 |    0.01 |
|                |           |                                    |           |           |           |       |         |
|        **Default** |         **0** |         **IDictionary&lt;String, Int32&gt;** |  **13.60 μs** |  **0.107 μs** |  **0.100 μs** |  **1.00** |    **0.00** |
| LazyReflection |         0 |         IDictionary&lt;String, Int32&gt; |  12.72 μs |  0.130 μs |  0.121 μs |  0.94 |    0.01 |
|         AddKvp |         0 |         IDictionary&lt;String, Int32&gt; |  12.24 μs |  0.094 μs |  0.083 μs |  0.90 |    0.01 |
|     MakeMethod |         0 |         IDictionary&lt;String, Int32&gt; |  13.38 μs |  0.118 μs |  0.111 μs |  0.98 |    0.01 |
|        Factory |         0 |         IDictionary&lt;String, Int32&gt; |  13.02 μs |  0.062 μs |  0.055 μs |  0.96 |    0.01 |
|                |           |                                    |           |           |           |       |         |
|        **Default** |         **0** | **IReadOnlyDictionary&lt;String, Int32&gt;** |  **56.93 μs** |  **0.273 μs** |  **0.256 μs** |  **1.00** |    **0.00** |
| LazyReflection |         0 | IReadOnlyDictionary&lt;String, Int32&gt; |  33.62 μs |  0.268 μs |  0.238 μs |  0.59 |    0.01 |
|         AddKvp |         0 | IReadOnlyDictionary&lt;String, Int32&gt; |  37.46 μs |  0.271 μs |  0.253 μs |  0.66 |    0.01 |
|     MakeMethod |         0 | IReadOnlyDictionary&lt;String, Int32&gt; |  48.41 μs |  0.289 μs |  0.270 μs |  0.85 |    0.01 |
|        Factory |         0 | IReadOnlyDictionary&lt;String, Int32&gt; |  32.89 μs |  0.228 μs |  0.213 μs |  0.58 |    0.01 |
|                |           |                                    |           |           |           |       |         |
|        **Default** |         **1** |         **IDictionary&lt;String, Int32&gt;** |  **13.33 μs** |  **0.149 μs** |  **0.140 μs** |  **1.00** |    **0.00** |
| LazyReflection |         1 |         IDictionary&lt;String, Int32&gt; |  13.43 μs |  0.084 μs |  0.078 μs |  1.01 |    0.01 |
|         AddKvp |         1 |         IDictionary&lt;String, Int32&gt; |  13.49 μs |  0.267 μs |  0.296 μs |  1.02 |    0.02 |
|     MakeMethod |         1 |         IDictionary&lt;String, Int32&gt; |  13.00 μs |  0.064 μs |  0.056 μs |  0.98 |    0.01 |
|        Factory |         1 |         IDictionary&lt;String, Int32&gt; |  12.55 μs |  0.105 μs |  0.093 μs |  0.94 |    0.01 |
|                |           |                                    |           |           |           |       |         |
|        **Default** |         **1** | **IReadOnlyDictionary&lt;String, Int32&gt;** |  **70.80 μs** |  **0.706 μs** |  **0.660 μs** |  **1.00** |    **0.00** |
| LazyReflection |         1 | IReadOnlyDictionary&lt;String, Int32&gt; |  70.54 μs |  0.367 μs |  0.306 μs |  1.00 |    0.01 |
|         AddKvp |         1 | IReadOnlyDictionary&lt;String, Int32&gt; |  46.39 μs |  0.300 μs |  0.266 μs |  0.65 |    0.01 |
|     MakeMethod |         1 | IReadOnlyDictionary&lt;String, Int32&gt; |  51.35 μs |  0.328 μs |  0.307 μs |  0.73 |    0.01 |
|        Factory |         1 | IReadOnlyDictionary&lt;String, Int32&gt; |  35.80 μs |  0.205 μs |  0.192 μs |  0.51 |    0.00 |
|                |           |                                    |           |           |           |       |         |
|        **Default** |         **2** |         **IDictionary&lt;String, Int32&gt;** |  **13.15 μs** |  **0.122 μs** |  **0.114 μs** |  **1.00** |    **0.00** |
| LazyReflection |         2 |         IDictionary&lt;String, Int32&gt; |  13.19 μs |  0.070 μs |  0.066 μs |  1.00 |    0.01 |
|         AddKvp |         2 |         IDictionary&lt;String, Int32&gt; |  12.80 μs |  0.069 μs |  0.064 μs |  0.97 |    0.01 |
|     MakeMethod |         2 |         IDictionary&lt;String, Int32&gt; |  13.37 μs |  0.176 μs |  0.164 μs |  1.02 |    0.02 |
|        Factory |         2 |         IDictionary&lt;String, Int32&gt; |  12.99 μs |  0.086 μs |  0.076 μs |  0.99 |    0.01 |
|                |           |                                    |           |           |           |       |         |
|        **Default** |         **2** | **IReadOnlyDictionary&lt;String, Int32&gt;** |  **81.06 μs** |  **1.105 μs** |  **0.980 μs** |  **1.00** |    **0.00** |
| LazyReflection |         2 | IReadOnlyDictionary&lt;String, Int32&gt; |  81.16 μs |  0.340 μs |  0.302 μs |  1.00 |    0.01 |
|         AddKvp |         2 | IReadOnlyDictionary&lt;String, Int32&gt; |  51.71 μs |  0.369 μs |  0.308 μs |  0.64 |    0.01 |
|     MakeMethod |         2 | IReadOnlyDictionary&lt;String, Int32&gt; |  52.84 μs |  1.046 μs |  1.163 μs |  0.65 |    0.02 |
|        Factory |         2 | IReadOnlyDictionary&lt;String, Int32&gt; |  36.54 μs |  0.208 μs |  0.174 μs |  0.45 |    0.01 |
|                |           |                                    |           |           |           |       |         |
|        **Default** |         **3** |         **IDictionary&lt;String, Int32&gt;** |  **13.73 μs** |  **0.101 μs** |  **0.095 μs** |  **1.00** |    **0.00** |
| LazyReflection |         3 |         IDictionary&lt;String, Int32&gt; |  12.75 μs |  0.086 μs |  0.080 μs |  0.93 |    0.01 |
|         AddKvp |         3 |         IDictionary&lt;String, Int32&gt; |  13.14 μs |  0.134 μs |  0.125 μs |  0.96 |    0.01 |
|     MakeMethod |         3 |         IDictionary&lt;String, Int32&gt; |  13.16 μs |  0.147 μs |  0.137 μs |  0.96 |    0.01 |
|        Factory |         3 |         IDictionary&lt;String, Int32&gt; |  12.78 μs |  0.113 μs |  0.106 μs |  0.93 |    0.01 |
|                |           |                                    |           |           |           |       |         |
|        **Default** |         **3** | **IReadOnlyDictionary&lt;String, Int32&gt;** |  **88.05 μs** |  **0.438 μs** |  **0.366 μs** |  **1.00** |    **0.00** |
| LazyReflection |         3 | IReadOnlyDictionary&lt;String, Int32&gt; |  89.01 μs |  0.945 μs |  0.884 μs |  1.01 |    0.01 |
|         AddKvp |         3 | IReadOnlyDictionary&lt;String, Int32&gt; |  56.60 μs |  0.308 μs |  0.288 μs |  0.64 |    0.00 |
|     MakeMethod |         3 | IReadOnlyDictionary&lt;String, Int32&gt; |  52.74 μs |  0.295 μs |  0.276 μs |  0.60 |    0.00 |
|        Factory |         3 | IReadOnlyDictionary&lt;String, Int32&gt; |  36.64 μs |  0.131 μs |  0.110 μs |  0.42 |    0.00 |
|                |           |                                    |           |           |           |       |         |
|        **Default** |         **5** |         **IDictionary&lt;String, Int32&gt;** |  **13.47 μs** |  **0.125 μs** |  **0.117 μs** |  **1.00** |    **0.00** |
| LazyReflection |         5 |         IDictionary&lt;String, Int32&gt; |  12.48 μs |  0.103 μs |  0.097 μs |  0.93 |    0.01 |
|         AddKvp |         5 |         IDictionary&lt;String, Int32&gt; |  13.13 μs |  0.066 μs |  0.061 μs |  0.97 |    0.01 |
|     MakeMethod |         5 |         IDictionary&lt;String, Int32&gt; |  12.55 μs |  0.125 μs |  0.117 μs |  0.93 |    0.01 |
|        Factory |         5 |         IDictionary&lt;String, Int32&gt; |  12.56 μs |  0.066 μs |  0.061 μs |  0.93 |    0.01 |
|                |           |                                    |           |           |           |       |         |
|        **Default** |         **5** | **IReadOnlyDictionary&lt;String, Int32&gt;** | **107.55 μs** |  **0.626 μs** |  **0.555 μs** |  **1.00** |    **0.00** |
| LazyReflection |         5 | IReadOnlyDictionary&lt;String, Int32&gt; | 108.65 μs |  1.345 μs |  1.258 μs |  1.01 |    0.01 |
|         AddKvp |         5 | IReadOnlyDictionary&lt;String, Int32&gt; |  70.41 μs |  0.374 μs |  0.331 μs |  0.65 |    0.00 |
|     MakeMethod |         5 | IReadOnlyDictionary&lt;String, Int32&gt; |  53.18 μs |  0.384 μs |  0.341 μs |  0.49 |    0.00 |
|        Factory |         5 | IReadOnlyDictionary&lt;String, Int32&gt; |  38.04 μs |  0.299 μs |  0.250 μs |  0.35 |    0.00 |
|                |           |                                    |           |           |           |       |         |
|        **Default** |        **10** |         **IDictionary&lt;String, Int32&gt;** |  **12.91 μs** |  **0.127 μs** |  **0.119 μs** |  **1.00** |    **0.00** |
| LazyReflection |        10 |         IDictionary&lt;String, Int32&gt; |  13.14 μs |  0.076 μs |  0.067 μs |  1.02 |    0.01 |
|         AddKvp |        10 |         IDictionary&lt;String, Int32&gt; |  12.97 μs |  0.110 μs |  0.097 μs |  1.01 |    0.01 |
|     MakeMethod |        10 |         IDictionary&lt;String, Int32&gt; |  13.13 μs |  0.065 μs |  0.058 μs |  1.02 |    0.01 |
|        Factory |        10 |         IDictionary&lt;String, Int32&gt; |  12.88 μs |  0.137 μs |  0.128 μs |  1.00 |    0.01 |
|                |           |                                    |           |           |           |       |         |
|        **Default** |        **10** | **IReadOnlyDictionary&lt;String, Int32&gt;** | **156.23 μs** |  **2.386 μs** |  **3.103 μs** |  **1.00** |    **0.00** |
| LazyReflection |        10 | IReadOnlyDictionary&lt;String, Int32&gt; | 153.89 μs |  1.222 μs |  1.083 μs |  0.99 |    0.02 |
|         AddKvp |        10 | IReadOnlyDictionary&lt;String, Int32&gt; |  98.48 μs |  0.650 μs |  0.608 μs |  0.63 |    0.01 |
|     MakeMethod |        10 | IReadOnlyDictionary&lt;String, Int32&gt; |  57.03 μs |  0.369 μs |  0.327 μs |  0.37 |    0.01 |
|        Factory |        10 | IReadOnlyDictionary&lt;String, Int32&gt; |  41.52 μs |  0.184 μs |  0.154 μs |  0.27 |    0.01 |
|                |           |                                    |           |           |           |       |         |
|        **Default** |       **100** |         **IDictionary&lt;String, Int32&gt;** |  **13.05 μs** |  **0.070 μs** |  **0.066 μs** |  **1.00** |    **0.00** |
| LazyReflection |       100 |         IDictionary&lt;String, Int32&gt; |  13.24 μs |  0.109 μs |  0.091 μs |  1.01 |    0.01 |
|         AddKvp |       100 |         IDictionary&lt;String, Int32&gt; |  13.13 μs |  0.181 μs |  0.169 μs |  1.01 |    0.01 |
|     MakeMethod |       100 |         IDictionary&lt;String, Int32&gt; |  12.65 μs |  0.118 μs |  0.110 μs |  0.97 |    0.01 |
|        Factory |       100 |         IDictionary&lt;String, Int32&gt; |  13.03 μs |  0.102 μs |  0.095 μs |  1.00 |    0.01 |
|                |           |                                    |           |           |           |       |         |
|        **Default** |       **100** | **IReadOnlyDictionary&lt;String, Int32&gt;** | **979.43 μs** | **14.287 μs** | **13.364 μs** |  **1.00** |    **0.00** |
| LazyReflection |       100 | IReadOnlyDictionary&lt;String, Int32&gt; | 942.09 μs |  6.139 μs |  5.743 μs |  0.96 |    0.02 |
|         AddKvp |       100 | IReadOnlyDictionary&lt;String, Int32&gt; | 639.04 μs |  4.289 μs |  3.582 μs |  0.65 |    0.01 |
|     MakeMethod |       100 | IReadOnlyDictionary&lt;String, Int32&gt; | 121.62 μs |  0.780 μs |  0.730 μs |  0.12 |    0.00 |
|        Factory |       100 | IReadOnlyDictionary&lt;String, Int32&gt; | 108.32 μs |  1.488 μs |  1.392 μs |  0.11 |    0.00 |
