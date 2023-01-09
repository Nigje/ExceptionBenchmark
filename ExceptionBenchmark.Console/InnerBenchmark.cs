using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExceptionBenchmark.Console
{
    public class InnerBenchmark
    {
        [Params(1,2,4,8,16)]
        public static int DepthToThrowException ;

        [Benchmark]
        public async Task AsyncWithStackTrace()
        {
            try
            {
                await AsyncDeepMethodWithThrowException();
            }
            catch (Exception ex)
            {
                var s = ex.StackTrace;
            }
        }

        [Benchmark]
        public async Task AsyncWithoutTrace()
        {
            try
            {
                await AsyncDeepMethodWithThrowException();
            }
            catch (Exception ex)
            {
            }
        }

        [Benchmark]
        public async Task AsyncWithoutException()
        {
            try
            {
                var status = await AsyncDeepMethodWithoutException();
            }
            catch (Exception ex)
            {
            }
        }

        [Benchmark]
        public void NormalWithStackTrace()
        {
            try
            {
                NormalDeepMethodWithThrowException();
            }
            catch (Exception ex)
            {
                var s = ex.StackTrace;
            }
        }

        [Benchmark]
        public void NormalWithoutTrace()
        {
            try
            {
                NormalDeepMethodWithThrowException();
            }
            catch (Exception ex)
            {
            }
        }
        [Benchmark]
        public void NormalWithoutException()
        {
            try
            {
                var status =  NormalDeepMethodWithoutException();
            }
            catch (Exception ex)
            {
            }
        }
        private static int NormalDeepMethodWithoutException(int depth = 0)
        {
            if (depth == DepthToThrowException)
            {
                return depth;
            }
            else
                return NormalDeepMethodWithoutException(depth + 1);
        }
        private static void NormalDeepMethodWithThrowException(int depth = 0)
        {
            if (depth == DepthToThrowException)
            {
                throw new Exception("Deep exception");
            }
            else
                NormalDeepMethodWithThrowException(depth + 1);
        }

        private static async Task AsyncDeepMethodWithThrowException(int depth = 0)
        {
            if (depth == DepthToThrowException)
            {
                throw new Exception("Deep exception");
            }
            else
                await AsyncDeepMethodWithThrowException(depth + 1);
        }

        private static async Task<int> AsyncDeepMethodWithoutException(int depth = 0)
        {
            if (depth == DepthToThrowException)
            {
                return depth;
            }
            else
                return await AsyncDeepMethodWithoutException(depth + 1);
        }
    }
}
