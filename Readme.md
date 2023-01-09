# Exception

An *exception* is an event, which occurs during the execution of a program, that disrupts the normal flow of the program's instructions. When an error occurs within a method, the method creates an object and hands it off to the runtime system ([Oracle](https://docs.oracle.com/javase/tutorial/essential/exceptions/definition.html#:~:text=Definition%3A%20An%20exception%20is%20an,off%20to%20the%20runtime%20system.)). Exceptions can be very helpful to developers, but Exception's performance is also a concern. Several benchmarks can be found if you search for expenses of exceptions. The purpose of this article is to discuss three different benchmarks. The conclusions are presented in the following sections:

- Result of console application
- Result of Web API



## The first benchmark

One of the simplest benchmarks is the following code, which has shown **[exceptions are at least 30,000 times slower than return codes](https://stackoverflow.com/questions/891217/how-expensive-are-exceptions-in-c)**. But, in the real world, it is very rare to use a try/catch within a loop in a simple console application by throwing exception in every iterations. 

```c#
    // Test exception
    stopwatch.Start();
    for (int i = 1; i <= 1000000; i++)
    {
        try
        {
            throw new Exception("Failed");
        }
        catch (Exception)
        {
            // Do nothing
        }
    }
    stopwatch.Stop();
```

```c#
    // Test return code
    stopwatch.Start();
    for (int i = 1; i <= 1000000; i++)
    {
        int retcode = ReturnCode();
        if (retcode == 1)
        {
            // Do nothing
        }
    }
	stopwatch.Stop();
```

```c#
static int ReturnCode()
{
    return 1;
}
```



## The second benchmark

[This article](https://mattwarren.org/2016/12/20/Why-Exceptions-should-be-Exceptional/) uses the BenchmarkDotNet library to take a benchmark. It has two approaches, the first is when the exception occurs rarely and the second is when it always occurs (They got the idea from [this article](https://shipilev.net/blog/2014/exceptional-performance/) that discusses exception performance in Java).

### Rare Exception vs. Error Code Handling

In this part, they assumed the throwing exception probability is 1 in 2700 (0.0370). It means the exception occurs once in every 2700 execution. Their conclusion is:

> *Throwing exceptin instead of handelling by code is 15 times slower than using error codes, but it is only 22 nanoseconds different. 22 billionths of a second, you have to be throwing exceptions frequently for it to be noticeable.* 

| Method                        |        Mean |    StdErr |    StdDev |
| ----------------------------- | ----------: | --------: | --------: |
| ErrorCodeWithReturnValue      |   1.4472 ns | 0.0088 ns | 0.0341 ns |
| RareExceptionStackTrace       |  22.0401 ns | 0.0292 ns | 0.1132 ns |
| RareExceptionMediumStackTrace |  61.8835 ns | 0.0609 ns | 0.2279 ns |
| RareExceptionDeepStackTrace   | 115.3692 ns | 0.1795 ns | 0.6953 ns |

- **ErrorCodeWithReturnValue**: It means instead of throwing an exception, they have returned a status code.

- **RareExceptionStackTrace**: In every 2700 executions, an exception is thrown and the stack trace is collected (depth 1).

- **RareExceptionMediumStackTrace**: In every 2700 executions, an exception is thrown and the stack trace is collected (depth 10).

- **RareExceptionDeepStackTrace**: In every 2700 execution, an exception is thrown and the stack trace is collected (depth 20).

Why do they assumed the exception propability 1 in 2700?

> According to the [NASA ‘Near Earth Object Program’](http://neo.jpl.nasa.gov/) asteroid [‘*101955 Bennu (1999 RQ36)*’](http://neo.jpl.nasa.gov/risk/a101955.html) has a Cumulative Impact Probability of 3.7e-04, i.e. there is a **1 in 2,700** (0.0370%) chance of Earth impact, but more reassuringly there is a 99.9630% chance the asteroid will miss the Earth completely!
>
> **So exceptions should be exceptional, unusual or rare, much like a asteroid strike!!**



### Collect Stack Trace vs. Ignore It

In the second part, the article compared expenses when the stack trace is collected versus ignoring this property. the table shows expenses and their conclusion is:

> So we clearly see there is an extra cost for exception handling that increases the deeper the stack trace goes. This is because when an exception is thrown the runtime needs to search up the stack until it hits a method than can handle it. The further it has to look up the stack, the more work it has to do.



```
BenchmarkDotNet=v0.10.1, OS=Microsoft Windows NT 6.1.7601 Service Pack 1
Processor=Intel(R) Core(TM) i7-4800MQ CPU 2.70GHz, ProcessorCount=8
Frequency=2630683 Hz, Resolution=380.1294 ns, Timer=TSC
  [Host]     : Clr 4.0.30319.42000, 32bit LegacyJIT-v4.6.1590.0
  DefaultJob : Clr 4.0.30319.42000, 32bit LegacyJIT-v4.6.1590.0
```

| **Method**                            | **Mean**        | **StdErr**  | **StdDev**  |
| ------------------------------------- | --------------- | ----------- | ----------- |
| Exception-Message                     | 9,187.9417 ns   | 13.4824 ns  | 48.6117 ns  |
| Exception-TryCatch                    | 9,253.0215 ns   | 13.2496 ns  | 51.3154 ns  |
| Exception-StackTrace                  | 30,178.7152 ns  | 41.0362 ns  | 158.9327 ns |
| ExceptionMedium-Message (depth 10)    | 14,911.7999 ns  | 20.2448 ns  | 78.4078 ns  |
| ExceptionMedium-TryCatch (depth 10)   | 15,158.0940 ns  | 147.4210 ns | 737.1049 ns |
| ExceptionMedium-StackTrace (depth 10) | 100,121.7951 ns | 129.0631 ns | 499.8591 ns |
| ExceptionDeep-Message (depth 20)      | 19,166.3524 ns  | 30.0539 ns  | 116.3984 ns |
| ExceptionDeep-TryCatch (depth 20)     | 19,581.6743 ns  | 208.3895 ns | 833.5579 ns |
| ExceptionDeep-StackTrace (depth 20)   | 154,569.3454 ns | 205.2174 ns | 794.8034 ns |





![Exception Handling - Calculating StackTrace](https://mattwarren.org/images/2016/12/Exception%20Handling%20-%20Calculating%20StackTrace.png)



- **Exception-Message**: it means they only returnd the ex.Message.

  ```c#
  public string ExceptionMessage()
  {
      try
      {
          throw Exception("Nothing.");
      }
      catch (InvalidOperationException ioex)
      {
          // Only get the simple message from the Exception (don't trigger a StackTrace collection)
          return ioex.Message;
      }
  }
  ```

- **Exception-TryCatch**: It means they returned the exception.

  ```c#
  public Exception ExceptionDeepTryCatch()
  {
      try
      {
          throw Exception("Nothing.");
      }
      catch (InvalidOperationException ioex)
      {
          return ioex;
      }
  }
  ```
  
- **Exception-StackTrace**: It means they returned the ex.StackTrace.

  ```c#
  public string ExceptionStackTrace()
  {
      try
      {
          throw Exception("Nothing.");
      }
      catch (InvalidOperationException ioex)
      {
          // Force collection of a full StackTrace
          return ioex.StackTrace;
      }
  }
  ```



## The custom benchmark

In this project, .Net 7 was used on Ubuntu 22.04 to make a benchmark.  In the first part of the benchmark, it used a recursive function to create different depths, which made very simple stack traces in console applications, while in the second part it has called a web API to make more complex stack traces. The following functions are used to create different depth stack traces. In the first function, a status code is returned, whereas in the second one, an exception is thrown.

```c#
    private async Task<int> ReturnStatusCodeAsync(int depth = 0)
    {
        if (depth == depthToThrowException)
        {
            return -1;
        }
        else
        {
            return await ReturnStatusCodeAsync(depth + 1);
        }
    }
```

```c#
    private async Task ThrowExceptionInDeepMethodAsync(int depth = 0)
    {
        if (depth == depthToThrowException)
        {
            throw new CustomException("Deep exception", $"Depth: {depth}");
        }
        else
        {
            await ThrowExceptionInDeepMethodAsync(depth + 1);
        }
    }
```



### Exception expenses in console applilcation

This part calls the recursive functions using a console application and NemchmarkDotNet. The following table shows the execution times at different depths. The times are in nanoseconds (1 second is 1,000,000,000 nanoseconds).

```
BenchmarkDotNet=v0.13.2, OS=ubuntu 22.04
11th Gen Intel Core i7-11800H 2.30GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.100
  [Host]     : .NET 7.0.0 (7.0.22.51805), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.0 (7.0.22.51805), X64 RyuJIT AVX2
```

|                                        | Depth 1   | Depth 2   | Depth 4    | Depth 8    | Depth 16   | Depth 32   | Depth 64     | Depth 128    | Depth 256    | Depth 512     | Depth 1024    |
| -------------------------------------- | --------- | --------- | ---------- | ---------- | ---------- | ---------- | ------------ | ------------ | ------------ | ------------- | ------------- |
| Async_ReturnStatusCode_WithoutTryCatch | 44.52     | 65.53     | 124.64     | 244.01     | 476.59     | 933.07     | 1,880.59     | 3,849.64     | 7,536.83     | 14,889.54     | 29,977.95     |
| Async_ReturnStatusCode_WithTryCatch    | 48.74     | 62.10     | 122.64     | 240.31     | 480.69     | 943.88     | 1,880.09     | 3,696.54     | 7,537.03     | 15,006.71     | 29,905.09     |
| Async_Throw_WithoutStackTrace          | 27,048.66 | 34,103.57 | 53,125.79  | 95,790.90  | 175,643.44 | 352,112.27 | 696,451.28   | 1,435,610.08 | 3,511,704.05 | 9,989,830.07  | 43,046,947.53 |
| Async_Throw_WithStackTrace             | 62,684.27 | 83,570.24 | 136,689.25 | 224,752.12 | 414,840.53 | 810,120.60 | 1,567,113.46 | 3,221,837.48 | 6,801,460.83 | 17,198,274.59 | 56,041,531.31 |
| Void_ReturnStatusCode_WithoutTryCatch  | 2.53      | 3.12      | 4.71       | 4.64       | 6.45       | 10.30      | 23.64        | 39.15        | 68.99        | 127.69        | 245.87        |
| Void_ReturnStatusCode_WithTryCatch     | 3.13      | 3.83      | 4.85       | 7.04       | 7.16       | 10.34      | 23.67        | 39.45        | 68.83        | 127.61        | 246.66        |
| Void_Throw_WithoutStackTrace           | 7,816.81  | 8,584.82  | 10,115.83  | 12,781.35  | 20,173.34  | 32,439.54  | 52,855.49    | 100,191.33   | 192,520.16   | 388,601.98    | 772,730.47    |
| Void_Throw_WithStackTrace              | 13,740.81 | 16,048.86 | 21,179.45  | 30,991.91  | 51,546.63  | 89,050.46  | 157,785.87   | 312,501.98   | 662,649.20   | 1,350,348.53  | 2,635,752.13  |



- **Void_ReturnStatusCode_WithoutTryCatch**: A status code is returned instead of an exception, and the method call is not within a try/catch block.

- **Async_ReturnStatusCode_WithoutTryCatch**: Similar to above but the method is called as async.

  ```c#
      public async Task<int> ReturnStatusCodeWithoutTryCatchAsync(int depthToThrowException = 1)
      {
          int statusCode = await ReturnStatusCodeAsync(depthToThrowException);
          return statusCode;
      }
  ```

  

- **Void_ReturnStatusCode_WithTryCatch**: A status code is returned instead of an exception, and the method call is within a try/catch block.

- **Async_ReturnStatusCode_WithTryCatch**: Similar to above but the method is called as async.

  ```c#
      public async Task<int> ReturnStatusCodeWithTryCatchAsync(int depthToThrowException = 1)
      {
          try
          {
              int statusCode = await ReturnStatusCodeAsync(depthToThrowException);
              return statusCode;
          }
          catch (CustomException ex)
          {
              return -1;
          }
      }
  ```

  

- **Void_Throw_WithoutStackTrace**: There is an exception in the depth, but the stack trace collection is not collected.

- **Async_Throw_WithoutStackTrace**: Similar to above but the method is called as async.

  ```c#
      public async Task ThrowExceptionWithoutStackTraceAsync(int depthToThrowException = 1)
      {
          try
          {
              await ThrowExceptionInDeepMethodAsync(depthToThrowException);
          }
          catch (CustomException ex)
          {
              var stackTrace = ex.Message;
          }
      }
  ```

  

  **Void_Throw_WithStackTrace**: There is an exception in the depth, and the stack trace collection is collected.

- **Async_Throw_WithStackTrace**: Similar to above but the method is called as async.

  ```c#
      public async Task ThrowExceptionWithStackTraceAsync(int depthToThrowException = 1)
      {
          try
          {
              await ThrowExceptionInDeepMethodAsync(depthToThrowException);
          }
          catch (CustomException ex)
          {
              var stackTrace = ex.StackTrace;
          }
      }
  ```



#### Result of console application

- **Using try/catch**: The first comparison is between using a try/catch block and not. Based on the result, placing the code within try/catch doesn't affect the execution time (maybe less than 1 nanosecond). This comparison is between `Void_ReturnStatusCode_WithTryCatch` and `Void_ReturnStatusCode_WithoutTryCatch`. Also, it is the same for the async type.
- **Asynchronous vs Synchronous**: Another comparison is between the async and sync methods. such as theory, using the async method has an overhead in task switching that increases execution time but allows scalability. In-depth 1, the difference between `Void_ReturnStatusCode` and `Async_ReturnStatusCode` execution time is about 45 ns which comes from context switching in the tasks layer.

- **StackTrace vs Message**: In this section, it has been supposed an exception occurred in the depth, and compared the cost of collecting stack trace collection and reading only the exception's message. the following charts show the difference between them. As the following chart shows, the cost of reading stack trace in comparison with only reading the exception's message in depth reaches 3 times (in-depth 1024 it reaches 3.4 times). To ignore the task overhead in async methods, the comparison is between `Void_Throw_WithStackTrace` and `Void_Throw_WithoutStackTrace`.

<img src=".\Docs\LineChart_StackTraceCost.svg" alt="LineChart_StackTraceCost" style="zoom:200%;" />



<img src=".\Docs\ColumnChart_StackTrace.svg" alt="ColumnChart_StackTrace" style="zoom:150%;" />

- **Return status code vs. Throw exception**: The comparison between returning status code and throwing exceptions in depth by collecting stack trace collection and only reading the exception's message has been shown in the following table. In-depth 32, the execution time by using the status code is 8613 times faster than throwing an exception by collecting the stack trace collection. It means, in In-depth 32 the execution time by using status code is 10.34 ns but when an exception occurs, by collecting stack trace collection (worst case) the execution time reaches 89,050.46 ns, which means 89,040.12 ns second more or 89 µs (microsecond) or 0.000089 seconds that really it is not too much in normal application. In the game industry or when a trade core or a video render engine is developing these expenses are too much but in most online applications are not too much and the latencies such as network or reading from I/O have more expenses. Also, the frequency of exceptions is very important.

  |                                                     | Depth 1 | Depth 4 | Depth 32 | Depth 64 | Depth 1024 |
  | --------------------------------------------------- | ------- | ------- | -------- | -------- | ---------- |
  | Trigger stack trace collection / error code (times) | 4387    | 4364    | 8613     | 6667     | 10686      |
  | Read only exception message / error code (times)    | 2496    | 2084    | 3138     | 2233     | 3133       |

  - **Trigger stack trace collection / status code**: Comparision between An exception has been thrown by triggering its stack trace collection and handeling the error by returning an error code.
  - **Read only exception message / error code**: Comparision between An exception has been thrown by not triggering its stack trace collection and handeling the error by returning an error code.



### Exception expenses in Web API

In the previous section to compare the values, the task overhead or network latency is ignored. In this part of the benchmark, a simple web application is developed by .Net 7 and ran locally on Ubuntu 22.04. The endpoints are called by BenchmarkDotNet and the following table shows the result (times are in nanoseconds).

```
BenchmarkDotNet=v0.13.2, OS=ubuntu 22.04
11th Gen Intel Core i7-11800H 2.30GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=7.0.100
  [Host]     : .NET 7.0.0 (7.0.22.51805), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.0 (7.0.22.51805), X64 RyuJIT AVX2
```



|                                           | Depth 1   | Depth 2   | Depth 4   | Depth 8   | Depth 16  | Depth 32  | Depth 64  | Depth 128 | Depth 256 | Depth 512 | Depth 1024 |
| ----------------------------------------- | --------- | --------- | --------- | --------- | --------- | --------- | --------- | --------- | --------- | --------- | ---------- |
| API_Get_WhithoutErrorAsync                | 42,161.47 | 38,656.95 | 40,568.02 | 39,053.88 | 38,422.28 | 40,867.41 | 40,048.15 | 39,958.79 | 40,237.61 | 40,054.94 | 40,619.64  |
| API_ReturnStatusCodeAsync_BadRequestAsync | 42,552.86 | 40,655.53 | 40,055.31 | 40,506.38 | 37,822.53 | 41,700.44 | 41,253.92 | 41,962.80 | 39,644.48 | 40,959.34 | 40,242.27  |
| API_ThrowException_WithoutStackTraceAsync | 41,845.55 | 39,551.16 | 39,539.08 | 40,103.79 | 40,402.60 | 42,157.54 | 42,917.57 | 43,287.28 | 38,582.36 | 42,468.83 | 41,882.86  |
| API_ThrowException_WithStackTraceAsync    | 42,478.64 | 39,288.11 | 39,241.53 | 40,747.68 | 40,927.15 | 40,708.47 | 42,728.33 | 43,113.12 | 40,919.05 | 41,538.79 | 38,998.65  |



- **API_Get_WhithoutErrorAsync**: It returns an OK() response and depth doesn't affect it.

  ```c#
      [HttpGet]
      [Route("async-ok")]
  	public async Task<IActionResult> Get()
      {
          return Ok();
      }
  ```

  

- **API_ReturnStatusCodeAsync_BadRequestAsync**: The inner error has been handled by returning a code (not any exception has occurred).

  ```c#
      [HttpGet]
      [Route("async-status-code/{depth}")]
      public async Task<IActionResult> ReturnStatusCodeWithoutTryCatch([FromRoute] int depth)
      {
          var statusCode = await exceptionService.ReturnStatusCodeWithoutTryCatchAsync(depth);
          if (statusCode > 0)
              return Ok();
          return BadRequest("Error in deep");
      }
  ```

  

- **API_ThrowException_WithoutStackTraceAsync**: An exception has occurred but in the logger, the stack trace collection is not collected. In this section, all the exceptions are caught by a middleware.

  ```c#
      [HttpGet]
      [Route("async-throw-exception-no-stack-trace/{depth}")]
      public async Task ThrowExceptionWichoutTryCatchWithoutStackTrace([FromRoute] int depth)
      {
          await exceptionService.ThrowExceptionWichoutTryCatchWithoutStackTraceAsync(depth);
      }
  ```

  

- **API_ThrowException_WithStackTraceAsync**: An exception has occurred and the stack trace is collected. In this section, all the exceptions are caught by a middleware.

  ```c#
      [HttpGet]
      [Route("async-throw-exception-by-stack-trace/{depth}")]
      public async Task ThrowExceptionWichoutTryCatchWithStackTrace([FromRoute] int depth)
      {
          await exceptionService.ThrowExceptionWichoutTryCatchWithStackTraceAsync(depth);
      }
  ```



#### Result of Web API

In this benchmark, the latency of reading from I/O is ignored and the average execution time for all of endpoints is 40,714 ns. The following chart shows more details. So, using status code instead of throwing exceptions doesn't have too much benefit on the usual web API, but throwing exceptions increases the speed of development. There is some best practice regarding the exceptions that are explained in another document.

<img src=".\Docs\ColumnChart_API_Call.svg" alt="ColumnChart_API_Call" style="zoom:150%;" />

## BenchmarkDotNet configuration

The default configuration for BenchmarkDotNet has been used in this project which means one CPU core, 6 warmups, and 15 tries for every benchmark.

