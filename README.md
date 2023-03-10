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
|        **Default** |        **-1** |         **IDictionary&lt;String, Int32&gt;** |  **35.03 ??s** |  **0.390 ??s** |  **0.345 ??s** |  **1.00** |    **0.00** |
| LazyReflection |        -1 |         IDictionary&lt;String, Int32&gt; |  29.42 ??s |  0.313 ??s |  0.293 ??s |  0.84 |    0.01 |
|         AddKvp |        -1 |         IDictionary&lt;String, Int32&gt; |  35.64 ??s |  0.349 ??s |  0.309 ??s |  1.02 |    0.01 |
|     MakeMethod |        -1 |         IDictionary&lt;String, Int32&gt; |  44.61 ??s |  0.576 ??s |  0.539 ??s |  1.27 |    0.02 |
|        Factory |        -1 |         IDictionary&lt;String, Int32&gt; |  29.11 ??s |  0.177 ??s |  0.157 ??s |  0.83 |    0.01 |
|                |           |                                    |           |           |           |       |         |
|        **Default** |        **-1** | **IReadOnlyDictionary&lt;String, Int32&gt;** |  **33.62 ??s** |  **0.268 ??s** |  **0.251 ??s** |  **1.00** |    **0.00** |
| LazyReflection |        -1 | IReadOnlyDictionary&lt;String, Int32&gt; |  30.03 ??s |  0.475 ??s |  0.445 ??s |  0.89 |    0.01 |
|         AddKvp |        -1 | IReadOnlyDictionary&lt;String, Int32&gt; |  35.52 ??s |  0.235 ??s |  0.220 ??s |  1.06 |    0.01 |
|     MakeMethod |        -1 | IReadOnlyDictionary&lt;String, Int32&gt; |  43.32 ??s |  0.254 ??s |  0.212 ??s |  1.29 |    0.01 |
|        Factory |        -1 | IReadOnlyDictionary&lt;String, Int32&gt; |  29.92 ??s |  0.231 ??s |  0.216 ??s |  0.89 |    0.01 |
|                |           |                                    |           |           |           |       |         |
|        **Default** |         **0** |         **IDictionary&lt;String, Int32&gt;** |  **13.60 ??s** |  **0.107 ??s** |  **0.100 ??s** |  **1.00** |    **0.00** |
| LazyReflection |         0 |         IDictionary&lt;String, Int32&gt; |  12.72 ??s |  0.130 ??s |  0.121 ??s |  0.94 |    0.01 |
|         AddKvp |         0 |         IDictionary&lt;String, Int32&gt; |  12.24 ??s |  0.094 ??s |  0.083 ??s |  0.90 |    0.01 |
|     MakeMethod |         0 |         IDictionary&lt;String, Int32&gt; |  13.38 ??s |  0.118 ??s |  0.111 ??s |  0.98 |    0.01 |
|        Factory |         0 |         IDictionary&lt;String, Int32&gt; |  13.02 ??s |  0.062 ??s |  0.055 ??s |  0.96 |    0.01 |
|                |           |                                    |           |           |           |       |         |
|        **Default** |         **0** | **IReadOnlyDictionary&lt;String, Int32&gt;** |  **56.93 ??s** |  **0.273 ??s** |  **0.256 ??s** |  **1.00** |    **0.00** |
| LazyReflection |         0 | IReadOnlyDictionary&lt;String, Int32&gt; |  33.62 ??s |  0.268 ??s |  0.238 ??s |  0.59 |    0.01 |
|         AddKvp |         0 | IReadOnlyDictionary&lt;String, Int32&gt; |  37.46 ??s |  0.271 ??s |  0.253 ??s |  0.66 |    0.01 |
|     MakeMethod |         0 | IReadOnlyDictionary&lt;String, Int32&gt; |  48.41 ??s |  0.289 ??s |  0.270 ??s |  0.85 |    0.01 |
|        Factory |         0 | IReadOnlyDictionary&lt;String, Int32&gt; |  32.89 ??s |  0.228 ??s |  0.213 ??s |  0.58 |    0.01 |
|                |           |                                    |           |           |           |       |         |
|        **Default** |         **1** |         **IDictionary&lt;String, Int32&gt;** |  **13.33 ??s** |  **0.149 ??s** |  **0.140 ??s** |  **1.00** |    **0.00** |
| LazyReflection |         1 |         IDictionary&lt;String, Int32&gt; |  13.43 ??s |  0.084 ??s |  0.078 ??s |  1.01 |    0.01 |
|         AddKvp |         1 |         IDictionary&lt;String, Int32&gt; |  13.49 ??s |  0.267 ??s |  0.296 ??s |  1.02 |    0.02 |
|     MakeMethod |         1 |         IDictionary&lt;String, Int32&gt; |  13.00 ??s |  0.064 ??s |  0.056 ??s |  0.98 |    0.01 |
|        Factory |         1 |         IDictionary&lt;String, Int32&gt; |  12.55 ??s |  0.105 ??s |  0.093 ??s |  0.94 |    0.01 |
|                |           |                                    |           |           |           |       |         |
|        **Default** |         **1** | **IReadOnlyDictionary&lt;String, Int32&gt;** |  **70.80 ??s** |  **0.706 ??s** |  **0.660 ??s** |  **1.00** |    **0.00** |
| LazyReflection |         1 | IReadOnlyDictionary&lt;String, Int32&gt; |  70.54 ??s |  0.367 ??s |  0.306 ??s |  1.00 |    0.01 |
|         AddKvp |         1 | IReadOnlyDictionary&lt;String, Int32&gt; |  46.39 ??s |  0.300 ??s |  0.266 ??s |  0.65 |    0.01 |
|     MakeMethod |         1 | IReadOnlyDictionary&lt;String, Int32&gt; |  51.35 ??s |  0.328 ??s |  0.307 ??s |  0.73 |    0.01 |
|        Factory |         1 | IReadOnlyDictionary&lt;String, Int32&gt; |  35.80 ??s |  0.205 ??s |  0.192 ??s |  0.51 |    0.00 |
|                |           |                                    |           |           |           |       |         |
|        **Default** |         **2** |         **IDictionary&lt;String, Int32&gt;** |  **13.15 ??s** |  **0.122 ??s** |  **0.114 ??s** |  **1.00** |    **0.00** |
| LazyReflection |         2 |         IDictionary&lt;String, Int32&gt; |  13.19 ??s |  0.070 ??s |  0.066 ??s |  1.00 |    0.01 |
|         AddKvp |         2 |         IDictionary&lt;String, Int32&gt; |  12.80 ??s |  0.069 ??s |  0.064 ??s |  0.97 |    0.01 |
|     MakeMethod |         2 |         IDictionary&lt;String, Int32&gt; |  13.37 ??s |  0.176 ??s |  0.164 ??s |  1.02 |    0.02 |
|        Factory |         2 |         IDictionary&lt;String, Int32&gt; |  12.99 ??s |  0.086 ??s |  0.076 ??s |  0.99 |    0.01 |
|                |           |                                    |           |           |           |       |         |
|        **Default** |         **2** | **IReadOnlyDictionary&lt;String, Int32&gt;** |  **81.06 ??s** |  **1.105 ??s** |  **0.980 ??s** |  **1.00** |    **0.00** |
| LazyReflection |         2 | IReadOnlyDictionary&lt;String, Int32&gt; |  81.16 ??s |  0.340 ??s |  0.302 ??s |  1.00 |    0.01 |
|         AddKvp |         2 | IReadOnlyDictionary&lt;String, Int32&gt; |  51.71 ??s |  0.369 ??s |  0.308 ??s |  0.64 |    0.01 |
|     MakeMethod |         2 | IReadOnlyDictionary&lt;String, Int32&gt; |  52.84 ??s |  1.046 ??s |  1.163 ??s |  0.65 |    0.02 |
|        Factory |         2 | IReadOnlyDictionary&lt;String, Int32&gt; |  36.54 ??s |  0.208 ??s |  0.174 ??s |  0.45 |    0.01 |
|                |           |                                    |           |           |           |       |         |
|        **Default** |         **3** |         **IDictionary&lt;String, Int32&gt;** |  **13.73 ??s** |  **0.101 ??s** |  **0.095 ??s** |  **1.00** |    **0.00** |
| LazyReflection |         3 |         IDictionary&lt;String, Int32&gt; |  12.75 ??s |  0.086 ??s |  0.080 ??s |  0.93 |    0.01 |
|         AddKvp |         3 |         IDictionary&lt;String, Int32&gt; |  13.14 ??s |  0.134 ??s |  0.125 ??s |  0.96 |    0.01 |
|     MakeMethod |         3 |         IDictionary&lt;String, Int32&gt; |  13.16 ??s |  0.147 ??s |  0.137 ??s |  0.96 |    0.01 |
|        Factory |         3 |         IDictionary&lt;String, Int32&gt; |  12.78 ??s |  0.113 ??s |  0.106 ??s |  0.93 |    0.01 |
|                |           |                                    |           |           |           |       |         |
|        **Default** |         **3** | **IReadOnlyDictionary&lt;String, Int32&gt;** |  **88.05 ??s** |  **0.438 ??s** |  **0.366 ??s** |  **1.00** |    **0.00** |
| LazyReflection |         3 | IReadOnlyDictionary&lt;String, Int32&gt; |  89.01 ??s |  0.945 ??s |  0.884 ??s |  1.01 |    0.01 |
|         AddKvp |         3 | IReadOnlyDictionary&lt;String, Int32&gt; |  56.60 ??s |  0.308 ??s |  0.288 ??s |  0.64 |    0.00 |
|     MakeMethod |         3 | IReadOnlyDictionary&lt;String, Int32&gt; |  52.74 ??s |  0.295 ??s |  0.276 ??s |  0.60 |    0.00 |
|        Factory |         3 | IReadOnlyDictionary&lt;String, Int32&gt; |  36.64 ??s |  0.131 ??s |  0.110 ??s |  0.42 |    0.00 |
|                |           |                                    |           |           |           |       |         |
|        **Default** |         **5** |         **IDictionary&lt;String, Int32&gt;** |  **13.47 ??s** |  **0.125 ??s** |  **0.117 ??s** |  **1.00** |    **0.00** |
| LazyReflection |         5 |         IDictionary&lt;String, Int32&gt; |  12.48 ??s |  0.103 ??s |  0.097 ??s |  0.93 |    0.01 |
|         AddKvp |         5 |         IDictionary&lt;String, Int32&gt; |  13.13 ??s |  0.066 ??s |  0.061 ??s |  0.97 |    0.01 |
|     MakeMethod |         5 |         IDictionary&lt;String, Int32&gt; |  12.55 ??s |  0.125 ??s |  0.117 ??s |  0.93 |    0.01 |
|        Factory |         5 |         IDictionary&lt;String, Int32&gt; |  12.56 ??s |  0.066 ??s |  0.061 ??s |  0.93 |    0.01 |
|                |           |                                    |           |           |           |       |         |
|        **Default** |         **5** | **IReadOnlyDictionary&lt;String, Int32&gt;** | **107.55 ??s** |  **0.626 ??s** |  **0.555 ??s** |  **1.00** |    **0.00** |
| LazyReflection |         5 | IReadOnlyDictionary&lt;String, Int32&gt; | 108.65 ??s |  1.345 ??s |  1.258 ??s |  1.01 |    0.01 |
|         AddKvp |         5 | IReadOnlyDictionary&lt;String, Int32&gt; |  70.41 ??s |  0.374 ??s |  0.331 ??s |  0.65 |    0.00 |
|     MakeMethod |         5 | IReadOnlyDictionary&lt;String, Int32&gt; |  53.18 ??s |  0.384 ??s |  0.341 ??s |  0.49 |    0.00 |
|        Factory |         5 | IReadOnlyDictionary&lt;String, Int32&gt; |  38.04 ??s |  0.299 ??s |  0.250 ??s |  0.35 |    0.00 |
|                |           |                                    |           |           |           |       |         |
|        **Default** |        **10** |         **IDictionary&lt;String, Int32&gt;** |  **12.91 ??s** |  **0.127 ??s** |  **0.119 ??s** |  **1.00** |    **0.00** |
| LazyReflection |        10 |         IDictionary&lt;String, Int32&gt; |  13.14 ??s |  0.076 ??s |  0.067 ??s |  1.02 |    0.01 |
|         AddKvp |        10 |         IDictionary&lt;String, Int32&gt; |  12.97 ??s |  0.110 ??s |  0.097 ??s |  1.01 |    0.01 |
|     MakeMethod |        10 |         IDictionary&lt;String, Int32&gt; |  13.13 ??s |  0.065 ??s |  0.058 ??s |  1.02 |    0.01 |
|        Factory |        10 |         IDictionary&lt;String, Int32&gt; |  12.88 ??s |  0.137 ??s |  0.128 ??s |  1.00 |    0.01 |
|                |           |                                    |           |           |           |       |         |
|        **Default** |        **10** | **IReadOnlyDictionary&lt;String, Int32&gt;** | **156.23 ??s** |  **2.386 ??s** |  **3.103 ??s** |  **1.00** |    **0.00** |
| LazyReflection |        10 | IReadOnlyDictionary&lt;String, Int32&gt; | 153.89 ??s |  1.222 ??s |  1.083 ??s |  0.99 |    0.02 |
|         AddKvp |        10 | IReadOnlyDictionary&lt;String, Int32&gt; |  98.48 ??s |  0.650 ??s |  0.608 ??s |  0.63 |    0.01 |
|     MakeMethod |        10 | IReadOnlyDictionary&lt;String, Int32&gt; |  57.03 ??s |  0.369 ??s |  0.327 ??s |  0.37 |    0.01 |
|        Factory |        10 | IReadOnlyDictionary&lt;String, Int32&gt; |  41.52 ??s |  0.184 ??s |  0.154 ??s |  0.27 |    0.01 |
|                |           |                                    |           |           |           |       |         |
|        **Default** |       **100** |         **IDictionary&lt;String, Int32&gt;** |  **13.05 ??s** |  **0.070 ??s** |  **0.066 ??s** |  **1.00** |    **0.00** |
| LazyReflection |       100 |         IDictionary&lt;String, Int32&gt; |  13.24 ??s |  0.109 ??s |  0.091 ??s |  1.01 |    0.01 |
|         AddKvp |       100 |         IDictionary&lt;String, Int32&gt; |  13.13 ??s |  0.181 ??s |  0.169 ??s |  1.01 |    0.01 |
|     MakeMethod |       100 |         IDictionary&lt;String, Int32&gt; |  12.65 ??s |  0.118 ??s |  0.110 ??s |  0.97 |    0.01 |
|        Factory |       100 |         IDictionary&lt;String, Int32&gt; |  13.03 ??s |  0.102 ??s |  0.095 ??s |  1.00 |    0.01 |
|                |           |                                    |           |           |           |       |         |
|        **Default** |       **100** | **IReadOnlyDictionary&lt;String, Int32&gt;** | **979.43 ??s** | **14.287 ??s** | **13.364 ??s** |  **1.00** |    **0.00** |
| LazyReflection |       100 | IReadOnlyDictionary&lt;String, Int32&gt; | 942.09 ??s |  6.139 ??s |  5.743 ??s |  0.96 |    0.02 |
|         AddKvp |       100 | IReadOnlyDictionary&lt;String, Int32&gt; | 639.04 ??s |  4.289 ??s |  3.582 ??s |  0.65 |    0.01 |
|     MakeMethod |       100 | IReadOnlyDictionary&lt;String, Int32&gt; | 121.62 ??s |  0.780 ??s |  0.730 ??s |  0.12 |    0.00 |
|        Factory |       100 | IReadOnlyDictionary&lt;String, Int32&gt; | 108.32 ??s |  1.488 ??s |  1.392 ??s |  0.11 |    0.00 |

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
|        **Default** |       **0** |     **0** |               **False** |    **40.73 ??s** |  **0.297 ??s** |   **0.278 ??s** |    **40.75 ??s** |  **1.00** |    **0.00** |
| LazyReflection |       0 |     0 |               False |    35.52 ??s |  0.211 ??s |   0.187 ??s |    35.58 ??s |  0.87 |    0.01 |
|      NewBinder |       0 |     0 |               False |    33.79 ??s |  0.272 ??s |   0.241 ??s |    33.70 ??s |  0.83 |    0.01 |
| ExistingBinder |       0 |     0 |               False |    33.72 ??s |  0.346 ??s |   0.324 ??s |    33.77 ??s |  0.83 |    0.01 |
|     NonGeneric |       0 |     0 |               False |    35.63 ??s |  0.183 ??s |   0.162 ??s |    35.62 ??s |  0.88 |    0.01 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |       **0** |     **0** |                **True** |    **39.11 ??s** |  **0.180 ??s** |   **0.160 ??s** |    **39.06 ??s** |  **1.00** |    **0.00** |
| LazyReflection |       0 |     0 |                True |    35.45 ??s |  0.133 ??s |   0.111 ??s |    35.47 ??s |  0.91 |    0.00 |
|      NewBinder |       0 |     0 |                True |    34.40 ??s |  0.521 ??s |   0.488 ??s |    34.16 ??s |  0.88 |    0.01 |
| ExistingBinder |       0 |     0 |                True |    33.59 ??s |  0.295 ??s |   0.261 ??s |    33.59 ??s |  0.86 |    0.01 |
|     NonGeneric |       0 |     0 |                True |    35.88 ??s |  0.282 ??s |   0.250 ??s |    35.78 ??s |  0.92 |    0.01 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |       **0** |     **1** |               **False** |    **64.64 ??s** |  **0.459 ??s** |   **0.429 ??s** |    **64.39 ??s** |  **1.00** |    **0.00** |
| LazyReflection |       0 |     1 |               False |    65.38 ??s |  0.222 ??s |   0.197 ??s |    65.39 ??s |  1.01 |    0.01 |
|      NewBinder |       0 |     1 |               False |    79.08 ??s |  1.122 ??s |   1.049 ??s |    78.44 ??s |  1.22 |    0.02 |
| ExistingBinder |       0 |     1 |               False |    55.24 ??s |  0.285 ??s |   0.253 ??s |    55.22 ??s |  0.85 |    0.01 |
|     NonGeneric |       0 |     1 |               False |    56.08 ??s |  0.667 ??s |   0.591 ??s |    55.85 ??s |  0.87 |    0.01 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |       **0** |     **1** |                **True** |    **84.33 ??s** |  **1.224 ??s** |   **1.145 ??s** |    **84.17 ??s** |  **1.00** |    **0.00** |
| LazyReflection |       0 |     1 |                True |    84.57 ??s |  0.458 ??s |   0.406 ??s |    84.63 ??s |  1.00 |    0.01 |
|      NewBinder |       0 |     1 |                True |    80.39 ??s |  0.512 ??s |   0.427 ??s |    80.30 ??s |  0.95 |    0.01 |
| ExistingBinder |       0 |     1 |                True |    57.13 ??s |  0.346 ??s |   0.324 ??s |    57.13 ??s |  0.68 |    0.01 |
|     NonGeneric |       0 |     1 |                True |    60.25 ??s |  1.162 ??s |   1.087 ??s |    59.96 ??s |  0.71 |    0.02 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |       **0** |     **2** |               **False** |    **89.28 ??s** |  **0.464 ??s** |   **0.434 ??s** |    **89.20 ??s** |  **1.00** |    **0.00** |
| LazyReflection |       0 |     2 |               False |    88.97 ??s |  0.305 ??s |   0.238 ??s |    89.00 ??s |  1.00 |    0.01 |
|      NewBinder |       0 |     2 |               False |    96.79 ??s |  0.425 ??s |   0.355 ??s |    96.79 ??s |  1.08 |    0.01 |
| ExistingBinder |       0 |     2 |               False |    75.16 ??s |  0.446 ??s |   0.348 ??s |    75.10 ??s |  0.84 |    0.01 |
|     NonGeneric |       0 |     2 |               False |    74.80 ??s |  0.343 ??s |   0.321 ??s |    74.73 ??s |  0.84 |    0.00 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |       **0** |     **2** |                **True** |   **121.00 ??s** |  **0.265 ??s** |   **0.207 ??s** |   **121.04 ??s** |  **1.00** |    **0.00** |
| LazyReflection |       0 |     2 |                True |   119.08 ??s |  0.518 ??s |   0.459 ??s |   118.95 ??s |  0.98 |    0.00 |
|      NewBinder |       0 |     2 |                True |   100.45 ??s |  0.441 ??s |   0.368 ??s |   100.36 ??s |  0.83 |    0.00 |
| ExistingBinder |       0 |     2 |                True |    78.41 ??s |  0.403 ??s |   0.357 ??s |    78.36 ??s |  0.65 |    0.00 |
|     NonGeneric |       0 |     2 |                True |    79.64 ??s |  0.235 ??s |   0.196 ??s |    79.62 ??s |  0.66 |    0.00 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |       **0** |     **3** |               **False** |   **114.24 ??s** |  **1.189 ??s** |   **1.112 ??s** |   **114.82 ??s** |  **1.00** |    **0.00** |
| LazyReflection |       0 |     3 |               False |   114.25 ??s |  0.603 ??s |   0.503 ??s |   114.15 ??s |  1.00 |    0.01 |
|      NewBinder |       0 |     3 |               False |   119.65 ??s |  0.301 ??s |   0.235 ??s |   119.62 ??s |  1.05 |    0.01 |
| ExistingBinder |       0 |     3 |               False |    97.39 ??s |  1.126 ??s |   0.998 ??s |    97.05 ??s |  0.85 |    0.01 |
|     NonGeneric |       0 |     3 |               False |    96.36 ??s |  0.438 ??s |   0.388 ??s |    96.23 ??s |  0.84 |    0.01 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |       **0** |     **3** |                **True** |   **159.08 ??s** |  **0.759 ??s** |   **0.673 ??s** |   **158.94 ??s** |  **1.00** |    **0.00** |
| LazyReflection |       0 |     3 |                True |   158.79 ??s |  0.969 ??s |   0.859 ??s |   158.37 ??s |  1.00 |    0.01 |
|      NewBinder |       0 |     3 |                True |   127.69 ??s |  0.552 ??s |   0.489 ??s |   127.50 ??s |  0.80 |    0.00 |
| ExistingBinder |       0 |     3 |                True |   102.35 ??s |  0.687 ??s |   0.609 ??s |   102.09 ??s |  0.64 |    0.00 |
|     NonGeneric |       0 |     3 |                True |   102.61 ??s |  0.644 ??s |   0.537 ??s |   102.45 ??s |  0.64 |    0.00 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |       **0** |     **5** |               **False** |   **160.81 ??s** |  **1.986 ??s** |   **1.857 ??s** |   **160.96 ??s** |  **1.00** |    **0.00** |
| LazyReflection |       0 |     5 |               False |   161.96 ??s |  0.765 ??s |   0.639 ??s |   161.85 ??s |  1.01 |    0.01 |
|      NewBinder |       0 |     5 |               False |   155.99 ??s |  0.439 ??s |   0.367 ??s |   156.03 ??s |  0.97 |    0.01 |
| ExistingBinder |       0 |     5 |               False |   133.60 ??s |  0.842 ??s |   0.746 ??s |   133.57 ??s |  0.83 |    0.01 |
|     NonGeneric |       0 |     5 |               False |   134.11 ??s |  2.372 ??s |   2.219 ??s |   133.09 ??s |  0.83 |    0.02 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |       **0** |     **5** |                **True** |   **218.88 ??s** |  **1.102 ??s** |   **0.977 ??s** |   **218.41 ??s** |  **1.00** |    **0.00** |
| LazyReflection |       0 |     5 |                True |   218.68 ??s |  1.181 ??s |   1.105 ??s |   218.63 ??s |  1.00 |    0.01 |
|      NewBinder |       0 |     5 |                True |   165.17 ??s |  0.983 ??s |   0.920 ??s |   164.90 ??s |  0.75 |    0.01 |
| ExistingBinder |       0 |     5 |                True |   141.77 ??s |  0.816 ??s |   0.681 ??s |   141.66 ??s |  0.65 |    0.00 |
|     NonGeneric |       0 |     5 |                True |   144.80 ??s |  0.606 ??s |   0.506 ??s |   144.62 ??s |  0.66 |    0.00 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |       **0** |    **10** |               **False** |   **263.83 ??s** |  **2.337 ??s** |   **2.072 ??s** |   **262.86 ??s** |  **1.00** |    **0.00** |
| LazyReflection |       0 |    10 |               False |   266.11 ??s |  1.050 ??s |   0.877 ??s |   265.78 ??s |  1.01 |    0.01 |
|      NewBinder |       0 |    10 |               False |   247.73 ??s |  4.889 ??s |   4.573 ??s |   245.37 ??s |  0.94 |    0.02 |
| ExistingBinder |       0 |    10 |               False |   215.45 ??s |  0.886 ??s |   0.786 ??s |   215.27 ??s |  0.82 |    0.01 |
|     NonGeneric |       0 |    10 |               False |   216.56 ??s |  1.447 ??s |   1.353 ??s |   216.09 ??s |  0.82 |    0.01 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |       **0** |    **10** |                **True** |   **378.15 ??s** |  **2.212 ??s** |   **1.847 ??s** |   **377.42 ??s** |  **1.00** |    **0.00** |
| LazyReflection |       0 |    10 |                True |   371.42 ??s |  2.054 ??s |   1.715 ??s |   370.94 ??s |  0.98 |    0.01 |
|      NewBinder |       0 |    10 |                True |   260.15 ??s |  1.814 ??s |   1.697 ??s |   259.60 ??s |  0.69 |    0.01 |
| ExistingBinder |       0 |    10 |                True |   231.90 ??s |  1.119 ??s |   1.047 ??s |   232.05 ??s |  0.61 |    0.00 |
|     NonGeneric |       0 |    10 |                True |   238.61 ??s |  1.230 ??s |   1.027 ??s |   238.85 ??s |  0.63 |    0.00 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |       **0** |   **100** |               **False** | **3,681.65 ??s** | **11.203 ??s** |   **9.355 ??s** | **3,682.46 ??s** |  **1.00** |    **0.00** |
| LazyReflection |       0 |   100 |               False | 3,765.77 ??s | 18.272 ??s |  16.198 ??s | 3,767.69 ??s |  1.02 |    0.01 |
|      NewBinder |       0 |   100 |               False | 3,642.36 ??s | 37.266 ??s |  34.859 ??s | 3,638.26 ??s |  0.99 |    0.01 |
| ExistingBinder |       0 |   100 |               False | 3,588.16 ??s | 32.167 ??s |  28.515 ??s | 3,582.50 ??s |  0.98 |    0.01 |
|     NonGeneric |       0 |   100 |               False | 3,611.80 ??s | 48.003 ??s |  40.084 ??s | 3,605.50 ??s |  0.98 |    0.01 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |       **0** |   **100** |                **True** | **5,299.98 ??s** | **63.231 ??s** |  **59.146 ??s** | **5,309.12 ??s** |  **1.00** |    **0.00** |
| LazyReflection |       0 |   100 |                True | 4,989.92 ??s | 98.476 ??s | 187.362 ??s | 4,882.01 ??s |  0.99 |    0.02 |
|      NewBinder |       0 |   100 |                True | 3,593.32 ??s | 16.692 ??s |  15.614 ??s | 3,591.36 ??s |  0.68 |    0.01 |
| ExistingBinder |       0 |   100 |                True | 3,573.15 ??s | 17.490 ??s |  16.360 ??s | 3,576.08 ??s |  0.67 |    0.01 |
|     NonGeneric |       0 |   100 |                True | 3,659.33 ??s | 33.083 ??s |  30.946 ??s | 3,653.06 ??s |  0.69 |    0.01 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |     **100** |     **0** |               **False** |    **39.27 ??s** |  **0.242 ??s** |   **0.227 ??s** |    **39.32 ??s** |  **1.00** |    **0.00** |
| LazyReflection |     100 |     0 |               False |    36.02 ??s |  0.295 ??s |   0.262 ??s |    36.03 ??s |  0.92 |    0.01 |
|      NewBinder |     100 |     0 |               False |    33.95 ??s |  0.207 ??s |   0.193 ??s |    33.95 ??s |  0.86 |    0.01 |
| ExistingBinder |     100 |     0 |               False |    34.18 ??s |  0.297 ??s |   0.278 ??s |    34.13 ??s |  0.87 |    0.01 |
|     NonGeneric |     100 |     0 |               False |    36.39 ??s |  0.586 ??s |   0.519 ??s |    36.23 ??s |  0.93 |    0.01 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |     **100** |     **0** |                **True** |    **39.19 ??s** |  **0.160 ??s** |   **0.150 ??s** |    **39.14 ??s** |  **1.00** |    **0.00** |
| LazyReflection |     100 |     0 |                True |    35.99 ??s |  0.288 ??s |   0.269 ??s |    36.03 ??s |  0.92 |    0.01 |
|      NewBinder |     100 |     0 |                True |    34.76 ??s |  0.262 ??s |   0.245 ??s |    34.71 ??s |  0.89 |    0.01 |
| ExistingBinder |     100 |     0 |                True |    34.08 ??s |  0.409 ??s |   0.382 ??s |    34.12 ??s |  0.87 |    0.01 |
|     NonGeneric |     100 |     0 |                True |    36.04 ??s |  0.273 ??s |   0.255 ??s |    36.01 ??s |  0.92 |    0.01 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |     **100** |     **1** |               **False** |    **65.56 ??s** |  **0.327 ??s** |   **0.306 ??s** |    **65.47 ??s** |  **1.00** |    **0.00** |
| LazyReflection |     100 |     1 |               False |    65.02 ??s |  0.494 ??s |   0.462 ??s |    64.88 ??s |  0.99 |    0.01 |
|      NewBinder |     100 |     1 |               False |    81.05 ??s |  0.397 ??s |   0.352 ??s |    80.98 ??s |  1.24 |    0.01 |
| ExistingBinder |     100 |     1 |               False |    55.39 ??s |  0.208 ??s |   0.162 ??s |    55.42 ??s |  0.84 |    0.01 |
|     NonGeneric |     100 |     1 |               False |    56.44 ??s |  0.212 ??s |   0.198 ??s |    56.40 ??s |  0.86 |    0.00 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |     **100** |     **1** |                **True** |    **81.36 ??s** |  **0.436 ??s** |   **0.408 ??s** |    **81.31 ??s** |  **1.00** |    **0.00** |
| LazyReflection |     100 |     1 |                True |    82.75 ??s |  1.318 ??s |   1.233 ??s |    82.31 ??s |  1.02 |    0.01 |
|      NewBinder |     100 |     1 |                True |    80.80 ??s |  0.449 ??s |   0.398 ??s |    80.81 ??s |  0.99 |    0.01 |
| ExistingBinder |     100 |     1 |                True |    57.74 ??s |  0.388 ??s |   0.363 ??s |    57.58 ??s |  0.71 |    0.01 |
|     NonGeneric |     100 |     1 |                True |    60.21 ??s |  0.563 ??s |   0.527 ??s |    60.09 ??s |  0.74 |    0.01 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |     **100** |     **2** |               **False** |    **94.59 ??s** |  **1.875 ??s** |   **3.613 ??s** |    **96.22 ??s** |  **1.00** |    **0.00** |
| LazyReflection |     100 |     2 |               False |    87.07 ??s |  0.408 ??s |   0.319 ??s |    87.05 ??s |  0.95 |    0.03 |
|      NewBinder |     100 |     2 |               False |    99.14 ??s |  0.450 ??s |   0.399 ??s |    99.05 ??s |  1.07 |    0.04 |
| ExistingBinder |     100 |     2 |               False |    76.67 ??s |  0.507 ??s |   0.474 ??s |    76.58 ??s |  0.82 |    0.03 |
|     NonGeneric |     100 |     2 |               False |    77.05 ??s |  0.634 ??s |   0.562 ??s |    76.96 ??s |  0.83 |    0.03 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |     **100** |     **2** |                **True** |   **120.53 ??s** |  **1.232 ??s** |   **1.153 ??s** |   **120.13 ??s** |  **1.00** |    **0.00** |
| LazyReflection |     100 |     2 |                True |   116.90 ??s |  0.494 ??s |   0.462 ??s |   117.02 ??s |  0.97 |    0.01 |
|      NewBinder |     100 |     2 |                True |   101.45 ??s |  0.511 ??s |   0.478 ??s |   101.40 ??s |  0.84 |    0.01 |
| ExistingBinder |     100 |     2 |                True |    78.87 ??s |  1.051 ??s |   0.983 ??s |    78.78 ??s |  0.65 |    0.01 |
|     NonGeneric |     100 |     2 |                True |    81.80 ??s |  0.261 ??s |   0.218 ??s |    81.77 ??s |  0.68 |    0.01 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |     **100** |     **3** |               **False** |   **114.82 ??s** |  **0.365 ??s** |   **0.341 ??s** |   **114.79 ??s** |  **1.00** |    **0.00** |
| LazyReflection |     100 |     3 |               False |   116.43 ??s |  1.346 ??s |   1.259 ??s |   115.97 ??s |  1.01 |    0.01 |
|      NewBinder |     100 |     3 |               False |   122.10 ??s |  0.457 ??s |   0.405 ??s |   122.18 ??s |  1.06 |    0.00 |
| ExistingBinder |     100 |     3 |               False |    99.15 ??s |  0.606 ??s |   0.537 ??s |    99.17 ??s |  0.86 |    0.01 |
|     NonGeneric |     100 |     3 |               False |    99.88 ??s |  1.395 ??s |   1.305 ??s |    99.54 ??s |  0.87 |    0.01 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |     **100** |     **3** |                **True** |   **154.58 ??s** |  **0.930 ??s** |   **0.870 ??s** |   **154.24 ??s** |  **1.00** |    **0.00** |
| LazyReflection |     100 |     3 |                True |   157.78 ??s |  1.008 ??s |   0.842 ??s |   157.90 ??s |  1.02 |    0.01 |
|      NewBinder |     100 |     3 |                True |   129.60 ??s |  1.007 ??s |   0.893 ??s |   129.27 ??s |  0.84 |    0.01 |
| ExistingBinder |     100 |     3 |                True |   103.80 ??s |  1.376 ??s |   1.287 ??s |   103.77 ??s |  0.67 |    0.01 |
|     NonGeneric |     100 |     3 |                True |   106.93 ??s |  0.547 ??s |   0.457 ??s |   106.91 ??s |  0.69 |    0.01 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |     **100** |     **5** |               **False** |   **158.53 ??s** |  **1.011 ??s** |   **0.896 ??s** |   **158.42 ??s** |  **1.00** |    **0.00** |
| LazyReflection |     100 |     5 |               False |   161.55 ??s |  1.018 ??s |   0.952 ??s |   161.59 ??s |  1.02 |    0.01 |
|      NewBinder |     100 |     5 |               False |   159.64 ??s |  0.608 ??s |   0.508 ??s |   159.63 ??s |  1.01 |    0.01 |
| ExistingBinder |     100 |     5 |               False |   133.32 ??s |  1.351 ??s |   1.198 ??s |   133.04 ??s |  0.84 |    0.01 |
|     NonGeneric |     100 |     5 |               False |   133.65 ??s |  1.655 ??s |   1.548 ??s |   132.91 ??s |  0.84 |    0.01 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |     **100** |     **5** |                **True** |   **218.02 ??s** |  **1.539 ??s** |   **1.364 ??s** |   **217.65 ??s** |  **1.00** |    **0.00** |
| LazyReflection |     100 |     5 |                True |   218.36 ??s |  0.948 ??s |   0.887 ??s |   218.37 ??s |  1.00 |    0.01 |
|      NewBinder |     100 |     5 |                True |   172.69 ??s |  0.849 ??s |   0.663 ??s |   172.77 ??s |  0.79 |    0.00 |
| ExistingBinder |     100 |     5 |                True |   140.26 ??s |  0.546 ??s |   0.484 ??s |   140.20 ??s |  0.64 |    0.00 |
|     NonGeneric |     100 |     5 |                True |   149.99 ??s |  1.606 ??s |   1.502 ??s |   149.16 ??s |  0.69 |    0.01 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |     **100** |    **10** |               **False** |   **261.54 ??s** |  **0.997 ??s** |   **0.833 ??s** |   **261.32 ??s** |  **1.00** |    **0.00** |
| LazyReflection |     100 |    10 |               False |   260.41 ??s |  1.345 ??s |   1.123 ??s |   260.06 ??s |  1.00 |    0.01 |
|      NewBinder |     100 |    10 |               False |   241.46 ??s |  1.258 ??s |   1.115 ??s |   240.97 ??s |  0.92 |    0.01 |
| ExistingBinder |     100 |    10 |               False |   215.52 ??s |  2.443 ??s |   2.285 ??s |   215.84 ??s |  0.82 |    0.01 |
|     NonGeneric |     100 |    10 |               False |   217.43 ??s |  1.361 ??s |   1.273 ??s |   217.28 ??s |  0.83 |    0.01 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |     **100** |    **10** |                **True** |   **372.64 ??s** |  **2.729 ??s** |   **2.553 ??s** |   **371.67 ??s** |  **1.00** |    **0.00** |
| LazyReflection |     100 |    10 |                True |   373.30 ??s |  5.361 ??s |   5.014 ??s |   372.87 ??s |  1.00 |    0.02 |
|      NewBinder |     100 |    10 |                True |   267.70 ??s |  2.022 ??s |   1.892 ??s |   267.56 ??s |  0.72 |    0.01 |
| ExistingBinder |     100 |    10 |                True |   231.41 ??s |  1.228 ??s |   1.088 ??s |   231.07 ??s |  0.62 |    0.00 |
|     NonGeneric |     100 |    10 |                True |   247.96 ??s |  3.283 ??s |   3.071 ??s |   247.35 ??s |  0.67 |    0.01 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |     **100** |   **100** |               **False** | **3,802.54 ??s** |  **8.767 ??s** |   **7.321 ??s** | **3,799.05 ??s** |  **1.00** |    **0.00** |
| LazyReflection |     100 |   100 |               False | 3,789.23 ??s | 22.810 ??s |  21.336 ??s | 3,781.32 ??s |  1.00 |    0.01 |
|      NewBinder |     100 |   100 |               False | 3,375.61 ??s | 20.176 ??s |  18.872 ??s | 3,367.24 ??s |  0.89 |    0.01 |
| ExistingBinder |     100 |   100 |               False | 3,433.16 ??s | 15.724 ??s |  14.708 ??s | 3,427.38 ??s |  0.90 |    0.00 |
|     NonGeneric |     100 |   100 |               False | 3,365.04 ??s | 30.764 ??s |  27.272 ??s | 3,352.89 ??s |  0.88 |    0.01 |
|                |         |       |                     |             |           |            |             |       |         |
|        **Default** |     **100** |   **100** |                **True** | **4,850.15 ??s** | **24.839 ??s** |  **22.019 ??s** | **4,846.58 ??s** |  **1.00** |    **0.00** |
| LazyReflection |     100 |   100 |                True | 4,911.73 ??s | 69.327 ??s |  64.849 ??s | 4,889.09 ??s |  1.01 |    0.02 |
|      NewBinder |     100 |   100 |                True | 3,637.32 ??s | 20.260 ??s |  18.951 ??s | 3,628.13 ??s |  0.75 |    0.01 |
| ExistingBinder |     100 |   100 |                True | 3,608.37 ??s | 22.434 ??s |  20.985 ??s | 3,611.57 ??s |  0.74 |    0.01 |
|     NonGeneric |     100 |   100 |                True | 3,655.82 ??s | 47.403 ??s |  44.341 ??s | 3,637.14 ??s |  0.75 |    0.01 |
