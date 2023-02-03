using BenchmarkDotNet.Running;

namespace ConfigurationBinderBenchmarks
{
    public static class Program
    {
        public static void Main()
        {
            BenchmarkRunner.Run<BindObjectImplementations>();
        }
    }
}
