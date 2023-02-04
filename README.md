# Configuration binding micro-benchmark

This repo contains micro-benchmark of different implementations of routines used to bind configuration to an
IDictionary<TKey, TValue> in Microsoft.Extensions.Configuration.Binder.

## BindDictionaryInterface implementations overview

**Default** - the implemention used in ConfigurationBinder as of 7d4163fc6e4b5c634eef22d0fd8decfc2aaca763.

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

## BindDictionary implementations overview

**Default** - the implemention used in ConfigurationBinder as of 7d4163fc6e4b5c634eef22d0fd8decfc2aaca763.

**LazyReflection** - avoids making Reflection API if call results are not used.

**NewBinder** - instantiates a generic object and bind dictionary and use it as an instance of IDictionary<TKey, TValue>
iterface. This is similar to the Factory implementation of BindDictionaryInterface.

**ExistingBinder** - same as **NewBinder** but a pre-precreate binder instance is passed to the binding method. This is
the case if BindDictionaryInterface has previously created a helper object to copy an existing dictionary.

**NonGeneric** - uses non-generic IDictionay interface if supported by the binding target object, otherwise falls back
to **Default** implementation.

## BindDictionaryInterface benchmark results

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

## BindDictionary benchmark results

Initial is the initial number of elements in the dictionary to bind. Added is the number of elements added to the
dictionary. AccessExistingValue indicates whether dictionary value binding tries to get an existing element from the
dictionary.

``` ini

BenchmarkDotNet=v0.13.4, OS=Windows 11 (10.0.22000.1455/21H2)
11th Gen Intel Core i5-11400H 2.70GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK=7.0.102
  [Host]     : .NET 7.0.2 (7.0.222.60605), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.2 (7.0.222.60605), X64 RyuJIT AVX2


```
|         Method | Initial | Added | AccessExistingValue |        Mean |     Error |     StdDev |      Median | Ratio | RatioSD |
|--------------- |-------- |------ |-------------------- |------------:|----------:|-----------:|------------:|------:|--------:|
|        **Default** |       **0** |     **0** |               **False** |    **40.73 μs** |  **0.297 μs** |   **0.278 μs** |    **40.75 μs** |  **1.00** |    **0.00** |
| LazyReflection |       0 |     0 |               False |    35.52 μs |  0.211 μs |   0.187 μs |    35.58 μs |  0.87 |    0.01 |
|      NewBinder |       0 |     0 |               False |    33.79 μs |  0.272 μs |   0.241 μs |    33.70 μs |  0.83 |    0.01 |
| ExistingBinder |       0 |     0 |               False |    33.72 μs |  0.346 μs |   0.324 μs |    33.77 μs |  0.83 |    0.01 |
|     NonGeneric |       0 |     0 |               False |    35.63 μs |  0.183 μs |   0.162 μs |    35.62 μs |  0.88 |    0.01 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |       **0** |     **0** |                **True** |    **39.11 μs** |  **0.180 μs** |   **0.160 μs** |    **39.06 μs** |  **1.00** |    **0.00** |
| LazyReflection |       0 |     0 |                True |    35.45 μs |  0.133 μs |   0.111 μs |    35.47 μs |  0.91 |    0.00 |
|      NewBinder |       0 |     0 |                True |    34.40 μs |  0.521 μs |   0.488 μs |    34.16 μs |  0.88 |    0.01 |
| ExistingBinder |       0 |     0 |                True |    33.59 μs |  0.295 μs |   0.261 μs |    33.59 μs |  0.86 |    0.01 |
|     NonGeneric |       0 |     0 |                True |    35.88 μs |  0.282 μs |   0.250 μs |    35.78 μs |  0.92 |    0.01 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |       **0** |     **1** |               **False** |    **64.64 μs** |  **0.459 μs** |   **0.429 μs** |    **64.39 μs** |  **1.00** |    **0.00** |
| LazyReflection |       0 |     1 |               False |    65.38 μs |  0.222 μs |   0.197 μs |    65.39 μs |  1.01 |    0.01 |
|      NewBinder |       0 |     1 |               False |    79.08 μs |  1.122 μs |   1.049 μs |    78.44 μs |  1.22 |    0.02 |
| ExistingBinder |       0 |     1 |               False |    55.24 μs |  0.285 μs |   0.253 μs |    55.22 μs |  0.85 |    0.01 |
|     NonGeneric |       0 |     1 |               False |    56.08 μs |  0.667 μs |   0.591 μs |    55.85 μs |  0.87 |    0.01 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |       **0** |     **1** |                **True** |    **84.33 μs** |  **1.224 μs** |   **1.145 μs** |    **84.17 μs** |  **1.00** |    **0.00** |
| LazyReflection |       0 |     1 |                True |    84.57 μs |  0.458 μs |   0.406 μs |    84.63 μs |  1.00 |    0.01 |
|      NewBinder |       0 |     1 |                True |    80.39 μs |  0.512 μs |   0.427 μs |    80.30 μs |  0.95 |    0.01 |
| ExistingBinder |       0 |     1 |                True |    57.13 μs |  0.346 μs |   0.324 μs |    57.13 μs |  0.68 |    0.01 |
|     NonGeneric |       0 |     1 |                True |    60.25 μs |  1.162 μs |   1.087 μs |    59.96 μs |  0.71 |    0.02 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |       **0** |     **2** |               **False** |    **89.28 μs** |  **0.464 μs** |   **0.434 μs** |    **89.20 μs** |  **1.00** |    **0.00** |
| LazyReflection |       0 |     2 |               False |    88.97 μs |  0.305 μs |   0.238 μs |    89.00 μs |  1.00 |    0.01 |
|      NewBinder |       0 |     2 |               False |    96.79 μs |  0.425 μs |   0.355 μs |    96.79 μs |  1.08 |    0.01 |
| ExistingBinder |       0 |     2 |               False |    75.16 μs |  0.446 μs |   0.348 μs |    75.10 μs |  0.84 |    0.01 |
|     NonGeneric |       0 |     2 |               False |    74.80 μs |  0.343 μs |   0.321 μs |    74.73 μs |  0.84 |    0.00 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |       **0** |     **2** |                **True** |   **121.00 μs** |  **0.265 μs** |   **0.207 μs** |   **121.04 μs** |  **1.00** |    **0.00** |
| LazyReflection |       0 |     2 |                True |   119.08 μs |  0.518 μs |   0.459 μs |   118.95 μs |  0.98 |    0.00 |
|      NewBinder |       0 |     2 |                True |   100.45 μs |  0.441 μs |   0.368 μs |   100.36 μs |  0.83 |    0.00 |
| ExistingBinder |       0 |     2 |                True |    78.41 μs |  0.403 μs |   0.357 μs |    78.36 μs |  0.65 |    0.00 |
|     NonGeneric |       0 |     2 |                True |    79.64 μs |  0.235 μs |   0.196 μs |    79.62 μs |  0.66 |    0.00 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |       **0** |     **3** |               **False** |   **114.24 μs** |  **1.189 μs** |   **1.112 μs** |   **114.82 μs** |  **1.00** |    **0.00** |
| LazyReflection |       0 |     3 |               False |   114.25 μs |  0.603 μs |   0.503 μs |   114.15 μs |  1.00 |    0.01 |
|      NewBinder |       0 |     3 |               False |   119.65 μs |  0.301 μs |   0.235 μs |   119.62 μs |  1.05 |    0.01 |
| ExistingBinder |       0 |     3 |               False |    97.39 μs |  1.126 μs |   0.998 μs |    97.05 μs |  0.85 |    0.01 |
|     NonGeneric |       0 |     3 |               False |    96.36 μs |  0.438 μs |   0.388 μs |    96.23 μs |  0.84 |    0.01 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |       **0** |     **3** |                **True** |   **159.08 μs** |  **0.759 μs** |   **0.673 μs** |   **158.94 μs** |  **1.00** |    **0.00** |
| LazyReflection |       0 |     3 |                True |   158.79 μs |  0.969 μs |   0.859 μs |   158.37 μs |  1.00 |    0.01 |
|      NewBinder |       0 |     3 |                True |   127.69 μs |  0.552 μs |   0.489 μs |   127.50 μs |  0.80 |    0.00 |
| ExistingBinder |       0 |     3 |                True |   102.35 μs |  0.687 μs |   0.609 μs |   102.09 μs |  0.64 |    0.00 |
|     NonGeneric |       0 |     3 |                True |   102.61 μs |  0.644 μs |   0.537 μs |   102.45 μs |  0.64 |    0.00 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |       **0** |     **5** |               **False** |   **160.81 μs** |  **1.986 μs** |   **1.857 μs** |   **160.96 μs** |  **1.00** |    **0.00** |
| LazyReflection |       0 |     5 |               False |   161.96 μs |  0.765 μs |   0.639 μs |   161.85 μs |  1.01 |    0.01 |
|      NewBinder |       0 |     5 |               False |   155.99 μs |  0.439 μs |   0.367 μs |   156.03 μs |  0.97 |    0.01 |
| ExistingBinder |       0 |     5 |               False |   133.60 μs |  0.842 μs |   0.746 μs |   133.57 μs |  0.83 |    0.01 |
|     NonGeneric |       0 |     5 |               False |   134.11 μs |  2.372 μs |   2.219 μs |   133.09 μs |  0.83 |    0.02 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |       **0** |     **5** |                **True** |   **218.88 μs** |  **1.102 μs** |   **0.977 μs** |   **218.41 μs** |  **1.00** |    **0.00** |
| LazyReflection |       0 |     5 |                True |   218.68 μs |  1.181 μs |   1.105 μs |   218.63 μs |  1.00 |    0.01 |
|      NewBinder |       0 |     5 |                True |   165.17 μs |  0.983 μs |   0.920 μs |   164.90 μs |  0.75 |    0.01 |
| ExistingBinder |       0 |     5 |                True |   141.77 μs |  0.816 μs |   0.681 μs |   141.66 μs |  0.65 |    0.00 |
|     NonGeneric |       0 |     5 |                True |   144.80 μs |  0.606 μs |   0.506 μs |   144.62 μs |  0.66 |    0.00 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |       **0** |    **10** |               **False** |   **263.83 μs** |  **2.337 μs** |   **2.072 μs** |   **262.86 μs** |  **1.00** |    **0.00** |
| LazyReflection |       0 |    10 |               False |   266.11 μs |  1.050 μs |   0.877 μs |   265.78 μs |  1.01 |    0.01 |
|      NewBinder |       0 |    10 |               False |   247.73 μs |  4.889 μs |   4.573 μs |   245.37 μs |  0.94 |    0.02 |
| ExistingBinder |       0 |    10 |               False |   215.45 μs |  0.886 μs |   0.786 μs |   215.27 μs |  0.82 |    0.01 |
|     NonGeneric |       0 |    10 |               False |   216.56 μs |  1.447 μs |   1.353 μs |   216.09 μs |  0.82 |    0.01 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |       **0** |    **10** |                **True** |   **378.15 μs** |  **2.212 μs** |   **1.847 μs** |   **377.42 μs** |  **1.00** |    **0.00** |
| LazyReflection |       0 |    10 |                True |   371.42 μs |  2.054 μs |   1.715 μs |   370.94 μs |  0.98 |    0.01 |
|      NewBinder |       0 |    10 |                True |   260.15 μs |  1.814 μs |   1.697 μs |   259.60 μs |  0.69 |    0.01 |
| ExistingBinder |       0 |    10 |                True |   231.90 μs |  1.119 μs |   1.047 μs |   232.05 μs |  0.61 |    0.00 |
|     NonGeneric |       0 |    10 |                True |   238.61 μs |  1.230 μs |   1.027 μs |   238.85 μs |  0.63 |    0.00 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |       **0** |   **100** |               **False** | **3,681.65 μs** | **11.203 μs** |   **9.355 μs** | **3,682.46 μs** |  **1.00** |    **0.00** |
| LazyReflection |       0 |   100 |               False | 3,765.77 μs | 18.272 μs |  16.198 μs | 3,767.69 μs |  1.02 |    0.01 |
|      NewBinder |       0 |   100 |               False | 3,642.36 μs | 37.266 μs |  34.859 μs | 3,638.26 μs |  0.99 |    0.01 |
| ExistingBinder |       0 |   100 |               False | 3,588.16 μs | 32.167 μs |  28.515 μs | 3,582.50 μs |  0.98 |    0.01 |
|     NonGeneric |       0 |   100 |               False | 3,611.80 μs | 48.003 μs |  40.084 μs | 3,605.50 μs |  0.98 |    0.01 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |       **0** |   **100** |                **True** | **5,299.98 μs** | **63.231 μs** |  **59.146 μs** | **5,309.12 μs** |  **1.00** |    **0.00** |
| LazyReflection |       0 |   100 |                True | 4,989.92 μs | 98.476 μs | 187.362 μs | 4,882.01 μs |  0.99 |    0.02 |
|      NewBinder |       0 |   100 |                True | 3,593.32 μs | 16.692 μs |  15.614 μs | 3,591.36 μs |  0.68 |    0.01 |
| ExistingBinder |       0 |   100 |                True | 3,573.15 μs | 17.490 μs |  16.360 μs | 3,576.08 μs |  0.67 |    0.01 |
|     NonGeneric |       0 |   100 |                True | 3,659.33 μs | 33.083 μs |  30.946 μs | 3,653.06 μs |  0.69 |    0.01 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |     **100** |     **0** |               **False** |    **39.27 μs** |  **0.242 μs** |   **0.227 μs** |    **39.32 μs** |  **1.00** |    **0.00** |
| LazyReflection |     100 |     0 |               False |    36.02 μs |  0.295 μs |   0.262 μs |    36.03 μs |  0.92 |    0.01 |
|      NewBinder |     100 |     0 |               False |    33.95 μs |  0.207 μs |   0.193 μs |    33.95 μs |  0.86 |    0.01 |
| ExistingBinder |     100 |     0 |               False |    34.18 μs |  0.297 μs |   0.278 μs |    34.13 μs |  0.87 |    0.01 |
|     NonGeneric |     100 |     0 |               False |    36.39 μs |  0.586 μs |   0.519 μs |    36.23 μs |  0.93 |    0.01 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |     **100** |     **0** |                **True** |    **39.19 μs** |  **0.160 μs** |   **0.150 μs** |    **39.14 μs** |  **1.00** |    **0.00** |
| LazyReflection |     100 |     0 |                True |    35.99 μs |  0.288 μs |   0.269 μs |    36.03 μs |  0.92 |    0.01 |
|      NewBinder |     100 |     0 |                True |    34.76 μs |  0.262 μs |   0.245 μs |    34.71 μs |  0.89 |    0.01 |
| ExistingBinder |     100 |     0 |                True |    34.08 μs |  0.409 μs |   0.382 μs |    34.12 μs |  0.87 |    0.01 |
|     NonGeneric |     100 |     0 |                True |    36.04 μs |  0.273 μs |   0.255 μs |    36.01 μs |  0.92 |    0.01 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |     **100** |     **1** |               **False** |    **65.56 μs** |  **0.327 μs** |   **0.306 μs** |    **65.47 μs** |  **1.00** |    **0.00** |
| LazyReflection |     100 |     1 |               False |    65.02 μs |  0.494 μs |   0.462 μs |    64.88 μs |  0.99 |    0.01 |
|      NewBinder |     100 |     1 |               False |    81.05 μs |  0.397 μs |   0.352 μs |    80.98 μs |  1.24 |    0.01 |
| ExistingBinder |     100 |     1 |               False |    55.39 μs |  0.208 μs |   0.162 μs |    55.42 μs |  0.84 |    0.01 |
|     NonGeneric |     100 |     1 |               False |    56.44 μs |  0.212 μs |   0.198 μs |    56.40 μs |  0.86 |    0.00 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |     **100** |     **1** |                **True** |    **81.36 μs** |  **0.436 μs** |   **0.408 μs** |    **81.31 μs** |  **1.00** |    **0.00** |
| LazyReflection |     100 |     1 |                True |    82.75 μs |  1.318 μs |   1.233 μs |    82.31 μs |  1.02 |    0.01 |
|      NewBinder |     100 |     1 |                True |    80.80 μs |  0.449 μs |   0.398 μs |    80.81 μs |  0.99 |    0.01 |
| ExistingBinder |     100 |     1 |                True |    57.74 μs |  0.388 μs |   0.363 μs |    57.58 μs |  0.71 |    0.01 |
|     NonGeneric |     100 |     1 |                True |    60.21 μs |  0.563 μs |   0.527 μs |    60.09 μs |  0.74 |    0.01 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |     **100** |     **2** |               **False** |    **94.59 μs** |  **1.875 μs** |   **3.613 μs** |    **96.22 μs** |  **1.00** |    **0.00** |
| LazyReflection |     100 |     2 |               False |    87.07 μs |  0.408 μs |   0.319 μs |    87.05 μs |  0.95 |    0.03 |
|      NewBinder |     100 |     2 |               False |    99.14 μs |  0.450 μs |   0.399 μs |    99.05 μs |  1.07 |    0.04 |
| ExistingBinder |     100 |     2 |               False |    76.67 μs |  0.507 μs |   0.474 μs |    76.58 μs |  0.82 |    0.03 |
|     NonGeneric |     100 |     2 |               False |    77.05 μs |  0.634 μs |   0.562 μs |    76.96 μs |  0.83 |    0.03 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |     **100** |     **2** |                **True** |   **120.53 μs** |  **1.232 μs** |   **1.153 μs** |   **120.13 μs** |  **1.00** |    **0.00** |
| LazyReflection |     100 |     2 |                True |   116.90 μs |  0.494 μs |   0.462 μs |   117.02 μs |  0.97 |    0.01 |
|      NewBinder |     100 |     2 |                True |   101.45 μs |  0.511 μs |   0.478 μs |   101.40 μs |  0.84 |    0.01 |
| ExistingBinder |     100 |     2 |                True |    78.87 μs |  1.051 μs |   0.983 μs |    78.78 μs |  0.65 |    0.01 |
|     NonGeneric |     100 |     2 |                True |    81.80 μs |  0.261 μs |   0.218 μs |    81.77 μs |  0.68 |    0.01 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |     **100** |     **3** |               **False** |   **114.82 μs** |  **0.365 μs** |   **0.341 μs** |   **114.79 μs** |  **1.00** |    **0.00** |
| LazyReflection |     100 |     3 |               False |   116.43 μs |  1.346 μs |   1.259 μs |   115.97 μs |  1.01 |    0.01 |
|      NewBinder |     100 |     3 |               False |   122.10 μs |  0.457 μs |   0.405 μs |   122.18 μs |  1.06 |    0.00 |
| ExistingBinder |     100 |     3 |               False |    99.15 μs |  0.606 μs |   0.537 μs |    99.17 μs |  0.86 |    0.01 |
|     NonGeneric |     100 |     3 |               False |    99.88 μs |  1.395 μs |   1.305 μs |    99.54 μs |  0.87 |    0.01 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |     **100** |     **3** |                **True** |   **154.58 μs** |  **0.930 μs** |   **0.870 μs** |   **154.24 μs** |  **1.00** |    **0.00** |
| LazyReflection |     100 |     3 |                True |   157.78 μs |  1.008 μs |   0.842 μs |   157.90 μs |  1.02 |    0.01 |
|      NewBinder |     100 |     3 |                True |   129.60 μs |  1.007 μs |   0.893 μs |   129.27 μs |  0.84 |    0.01 |
| ExistingBinder |     100 |     3 |                True |   103.80 μs |  1.376 μs |   1.287 μs |   103.77 μs |  0.67 |    0.01 |
|     NonGeneric |     100 |     3 |                True |   106.93 μs |  0.547 μs |   0.457 μs |   106.91 μs |  0.69 |    0.01 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |     **100** |     **5** |               **False** |   **158.53 μs** |  **1.011 μs** |   **0.896 μs** |   **158.42 μs** |  **1.00** |    **0.00** |
| LazyReflection |     100 |     5 |               False |   161.55 μs |  1.018 μs |   0.952 μs |   161.59 μs |  1.02 |    0.01 |
|      NewBinder |     100 |     5 |               False |   159.64 μs |  0.608 μs |   0.508 μs |   159.63 μs |  1.01 |    0.01 |
| ExistingBinder |     100 |     5 |               False |   133.32 μs |  1.351 μs |   1.198 μs |   133.04 μs |  0.84 |    0.01 |
|     NonGeneric |     100 |     5 |               False |   133.65 μs |  1.655 μs |   1.548 μs |   132.91 μs |  0.84 |    0.01 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |     **100** |     **5** |                **True** |   **218.02 μs** |  **1.539 μs** |   **1.364 μs** |   **217.65 μs** |  **1.00** |    **0.00** |
| LazyReflection |     100 |     5 |                True |   218.36 μs |  0.948 μs |   0.887 μs |   218.37 μs |  1.00 |    0.01 |
|      NewBinder |     100 |     5 |                True |   172.69 μs |  0.849 μs |   0.663 μs |   172.77 μs |  0.79 |    0.00 |
| ExistingBinder |     100 |     5 |                True |   140.26 μs |  0.546 μs |   0.484 μs |   140.20 μs |  0.64 |    0.00 |
|     NonGeneric |     100 |     5 |                True |   149.99 μs |  1.606 μs |   1.502 μs |   149.16 μs |  0.69 |    0.01 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |     **100** |    **10** |               **False** |   **261.54 μs** |  **0.997 μs** |   **0.833 μs** |   **261.32 μs** |  **1.00** |    **0.00** |
| LazyReflection |     100 |    10 |               False |   260.41 μs |  1.345 μs |   1.123 μs |   260.06 μs |  1.00 |    0.01 |
|      NewBinder |     100 |    10 |               False |   241.46 μs |  1.258 μs |   1.115 μs |   240.97 μs |  0.92 |    0.01 |
| ExistingBinder |     100 |    10 |               False |   215.52 μs |  2.443 μs |   2.285 μs |   215.84 μs |  0.82 |    0.01 |
|     NonGeneric |     100 |    10 |               False |   217.43 μs |  1.361 μs |   1.273 μs |   217.28 μs |  0.83 |    0.01 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |     **100** |    **10** |                **True** |   **372.64 μs** |  **2.729 μs** |   **2.553 μs** |   **371.67 μs** |  **1.00** |    **0.00** |
| LazyReflection |     100 |    10 |                True |   373.30 μs |  5.361 μs |   5.014 μs |   372.87 μs |  1.00 |    0.02 |
|      NewBinder |     100 |    10 |                True |   267.70 μs |  2.022 μs |   1.892 μs |   267.56 μs |  0.72 |    0.01 |
| ExistingBinder |     100 |    10 |                True |   231.41 μs |  1.228 μs |   1.088 μs |   231.07 μs |  0.62 |    0.00 |
|     NonGeneric |     100 |    10 |                True |   247.96 μs |  3.283 μs |   3.071 μs |   247.35 μs |  0.67 |    0.01 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |     **100** |   **100** |               **False** | **3,802.54 μs** |  **8.767 μs** |   **7.321 μs** | **3,799.05 μs** |  **1.00** |    **0.00** |
| LazyReflection |     100 |   100 |               False | 3,789.23 μs | 22.810 μs |  21.336 μs | 3,781.32 μs |  1.00 |    0.01 |
|      NewBinder |     100 |   100 |               False | 3,375.61 μs | 20.176 μs |  18.872 μs | 3,367.24 μs |  0.89 |    0.01 |
| ExistingBinder |     100 |   100 |               False | 3,433.16 μs | 15.724 μs |  14.708 μs | 3,427.38 μs |  0.90 |    0.00 |
|     NonGeneric |     100 |   100 |               False | 3,365.04 μs | 30.764 μs |  27.272 μs | 3,352.89 μs |  0.88 |    0.01 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |     **100** |   **100** |                **True** | **4,850.15 μs** | **24.839 μs** |  **22.019 μs** | **4,846.58 μs** |  **1.00** |    **0.00** |
| LazyReflection |     100 |   100 |                True | 4,911.73 μs | 69.327 μs |  64.849 μs | 4,889.09 μs |  1.01 |    0.02 |
|      NewBinder |     100 |   100 |                True | 3,637.32 μs | 20.260 μs |  18.951 μs | 3,628.13 μs |  0.75 |    0.01 |
| ExistingBinder |     100 |   100 |                True | 3,608.37 μs | 22.434 μs |  20.985 μs | 3,611.57 μs |  0.74 |    0.01 |
|     NonGeneric |     100 |   100 |                True | 3,655.82 μs | 47.403 μs |  44.341 μs | 3,637.14 μs |  0.75 |    0.01 |
