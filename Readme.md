# Exception Performance in .NET: Benchmarks and Best Practices

An *exception* is an event that disrupts the normal flow of a program's execution. When an error occurs within a method, the runtime creates an object and unwinds the stack to find a handler. How much does an exception cost, and how much of that cost comes from reading its stack trace? 

This repository explores the performance cost of exceptions in C# through various benchmarks, comparing returning an error code with throwing and catching an exception across different stack depths. It concludes with a scalable architectural pattern for exception handling in .NET applications.

## Executive Summary (TL;DR)
If you are short on time, here are the key takeaways from the benchmarks and architectural analysis:

1. **`try/catch` Blocks are Virtually Free:** 
   Because modern .NET uses a table-based exception handling mechanism, simply wrapping code in a `try/catch` block costs zero CPU instructions unless an exception is actually thrown. The benchmark confirms this adds negligible overhead (< 1 nanosecond).
   
2. **Throwing is Expensive:** 
   Throwing an exception requires the runtime to allocate an object, capture the thread state, and walk up the call stack to find a handler. This makes throwing an exception significantly slower (microseconds) than returning a simple status code (nanoseconds).
   
3. **The `StackTrace` is the Bottleneck:** 
   The most expensive part of throwing an exception is the CLR asking the operating system to build the stack trace string. The deeper the call stack, the more expensive the exception becomes. At a stack depth of 1024, reading the `StackTrace` is 3.4x slower than just reading the exception `Message`.
   
4. **Do Not Prematurely Optimize Web APIs:** 
   In a real-world ASP.NET Core Web API, network latency, database I/O, and JSON serialization dominate the request lifecycle (taking milliseconds). The overhead of an exception (taking microseconds) is completely dwarfed by these factors. Refactoring a Web API to use status codes everywhere just to save 40 microseconds is a classic case of premature optimization. 
   
**The Architectural Verdict:** For highly repetitive, expected validation failures, returning a `Result` object is preferred. However, for true domain failures (e.g., `UserNotFound`), do not contort your controllers to avoid exceptions. Instead, use a **centralized Middleware + Custom Exception architecture** to keep your code clean, maintainable, and aligned with enterprise best practices.

---

## 1. Historical Context: Naive vs. Rare Exceptions

Before diving into the custom benchmark, it is worth understanding the historical context of exception performance.

### The Naive Benchmark
One of the simplest synthetic tests involves throwing an exception within a tight loop. As shown in various [StackOverflow discussions](https://stackoverflow.com/questions/891217/how-expensive-are-exceptions-in-c), this approach demonstrates that **exceptions can be at least 30,000 times slower than simple return codes**. However, it is extremely rare for an application to continuously throw exceptions in a tight loop.

### The Realistic Benchmark (Rare Exceptions)
[Matt Warren's analysis](https://mattwarren.org/2016/12/20/Why-Exceptions-should-be-Exceptional/) explores what happens when exceptions are rare (assumed 1 in 2,700 executions, based on NASA's probability of an asteroid impact). 

**The Conclusion:**
> *Throwing an exception instead of returning an error code is 15 times slower, but the absolute difference is only ~20 nanoseconds. You would have to throw exceptions incredibly frequently for this delay to become noticeable.*

Warren also noted that a significant portion of exception overhead comes from collecting the stack trace. The deeper the call stack, the more work the CLR has to do to unwind it.

---

## 2. The Custom Benchmark: Deep Call Stacks

To further analyze stack depth costs and asynchronous context switching, this project implements a custom benchmark. 

### What is measured
- `ExceptionBenchmark.Common/ExceptionService.cs` implements recursive status-code and exception paths.
- `ExceptionBenchmark.Console/Benchmark.cs` runs those paths and makes HTTP requests with BenchmarkDotNet.
- `ExceptionBenchmark.WebApi` exposes the service through a controller and catches exceptions in middleware.

The exception microbenchmarks throw on **every invocation**. They measure the failure path, not an application where most requests succeed. 

| Method name in console benchmark | Work performed |
| --- | --- |
| `ReturnStatusCode_WithoutTryCatch` | Synchronous recursion returning a status code. |
| `ReturnStatusCode_WithTryCatch` | The same path inside a `try/catch`; no exception is thrown. |
| `Throw_WithoutStackTrace` | Throw and catch `CustomException`, then read `Message`. |
| `Throw_WithStackTrace` | Throw and catch `CustomException`, then read `StackTrace`. |
| `Async_*` | Corresponding paths implemented with recursive `Task`/`await` calls. |

### Console Results
The following results were generated using .NET 7 on Ubuntu 22.04 (Intel Core i7-11800H). All means in the table are **nanoseconds per benchmark invocation**.

| Method | Depth 1 | Depth 8 | Depth 32 | Depth 128 | Depth 1024 |
| --- | --- | --- | --- | --- | --- |
| **Async_ReturnStatusCode_WithoutTryCatch** | 44.52 | 244.01 | 933.07 | 3,849.64 | 29,977.95 |
| **Async_ReturnStatusCode_WithTryCatch** | 48.74 | 240.31 | 943.88 | 3,696.54 | 29,905.09 |
| **Async_Throw_WithoutStackTrace** | 27,048.66 | 95,790.90 | 352,112.27 | 1,435,610.08 | 43,046,947.53 |
| **Async_Throw_WithStackTrace** | 62,684.27 | 224,752.12 | 810,120.60 | 3,221,837.48 | 56,041,531.31 |
| **Void_ReturnStatusCode_WithoutTryCatch** | 2.53 | 4.64 | 10.30 | 39.15 | 245.87 |
| **Void_ReturnStatusCode_WithTryCatch** | 3.13 | 7.04 | 10.34 | 39.45 | 246.66 |
| **Void_Throw_WithoutStackTrace** | 7,816.81 | 12,781.35 | 32,439.54 | 100,191.33 | 772,730.47 |
| **Void_Throw_WithStackTrace** | 13,740.81 | 30,991.91 | 89,050.46 | 312,501.98 | 2,635,752.13 |

#### Reading the results:
1. **A non-throwing `try/catch` has a small cost here.** At depth 1, the synchronous status-code path takes 2.53 ns without the block and 3.13 ns with it. This is evidence of a small absolute difference, not proof that `try/catch` is completely free.
2. **Throwing is much more expensive than returning a code.** At depth 32, the synchronous path with `try/catch` takes 10.34 ns when returning a code, but 89,050.46 ns when throwing and reading the `StackTrace`. While the latter is ~8,600 times slower, the absolute difference is about 89 microseconds.
3. **Reading `StackTrace` adds substantial work.** At depth 1024, triggering the stack trace takes roughly 3.4 times longer than simply reading the exception message.
4. **The async measurements include more than context switching.** These methods recursively await tasks, meaning the comparison includes async state machines, task completion, and recursive awaits. At large depths, the async exception path grows particularly expensive.

<img src=".\Docs\LineChart_StackTraceCost.svg" alt="LineChart_StackTraceCost" />
<img src=".\Docs\ColumnChart_StackTrace.svg" alt="ColumnChart_StackTrace" />

---

## 3. Web API Results: Historical & Unverified

The HTTP benchmark intended to measure the real-world impact by sending requests and reading the response body. 

| Method | Depth 1 | Depth 32 | Depth 256 | Depth 1024 |
| --- | --- | --- | --- | --- |
| **API_Get_WhithoutErrorAsync** | 42,161.47 | 40,867.41 | 40,237.61 | 40,619.64 |
| **API_ReturnStatusCodeAsync** | 42,552.86 | 41,700.44 | 39,644.48 | 40,242.27 |
| **API_Throw_WithoutStackTraceAsync** | 41,845.55 | 42,157.54 | 38,582.36 | 41,882.86 |
| **API_Throw_WithStackTraceAsync**| 42,478.64 | 40,708.47 | 40,919.05 | 38,998.65 |

**Critical Analysis of the Historical Data:**
The values cluster around 38–43 microseconds, with little change across depths. However, **these published API results need a new, validated run.** 

Before interpreting the pattern, several configuration flaws in the original benchmark must be resolved:
1. **Route Mismatch:** The controller requires `[Route("api/exception")]`, but the client requested paths like `/async-ok` without the prefix, potentially resulting in 404s.
2. **Unchecked Status Codes:** The client reads response bodies without checking status codes. A 404 can be recorded as a completed, fast benchmark invocation.
3. **Port Mismatch:** The client defaults to IIS Express (`https://localhost:44365/`), while the project HTTPS launch profile uses port `7155`.
4. **Disabled Sweeps:** Only `[Params(1)]` was enabled in the checked-in code.

Due to these issues, the repository does not currently establish which responses produced the historical timings. Even after correcting the configuration, similar end-to-end means would only describe that particular workload.

### Reproducing the Benchmarks Correctly
To execute a valid HTTP run, you must fix the routing and validate the responses:

```sh
dotnet run --project ExceptionBenchmark.WebApi -c Release --launch-profile https
```
Set the client's `baseUrl` to `https://localhost:7155/api/exception`. Add validation to the benchmark client to ensure the expected HTTP status is returned (e.g., `400 BadRequest` for the exception endpoints) rather than `EnsureSuccessStatusCode()`.

---

## 4. Exception Handling Best Practices

Returning a status or an explicit result is a good fit for frequent, expected outcomes on a hot path. Throwing remains useful when an operation cannot fulfill its contract and callers need to unwind to a handler.

If you choose to use exceptions, proper handling in a .NET Web API involves defining a strong internal exception structure and centralizing error catching.

### The Domain Exception Structure
Create a base `CustomException` that all domain-specific exceptions inherit from.

```csharp
public class CustomException : Exception
{
    /// <summary>
    /// A machine-readable error code (e.g., USER_NOT_FOUND).
    /// Used by frontend clients for localization and logic.
    /// </summary>
    public ErrorCodeEnum? ErrorCode { get; protected set; }

    /// <summary>
    /// Internal technical details not meant to be shown to the end-user.
    /// </summary>
    public string? TechnicalMessage { get; protected set; }

    /// <summary>
    /// Indicates the severity of the exception for logging (e.g., Info vs Critical).
    /// </summary>
    public LogSeverityEnum Severity { get; protected set; }

    /// <summary>
    /// Indicates whether the stack trace is valuable enough to be logged.
    /// </summary>
    public bool LogStackTrace { get; protected set; }
}
```
*Note: `LogStackTrace = false` does not disable the runtime's exception mechanics; it simply instructs the logger/middleware to skip reading and formatting the `StackTrace` property.*

### Catching Exceptions via Middleware
Instead of wrapping every controller in a `try/catch` block, use ASP.NET Core Middleware to catch unhandled exceptions globally. When an exception occurs, the middleware should format it into a standardized JSON response:

```json
{
  "ErrorCode": "USER_NOT_FOUND",
  "Message": "Deep exception message",
  "Details": "Available only in non-production environments.",
  "TraceId": "12c61b62-f704-4d2e-9deb-c465a639ef92"
}
```

**Mapping Exceptions to HTTP Status Codes:**
The middleware inspects the exception type to determine the correct HTTP status code:

```csharp
private HttpStatusCode GetHttpStatusCode(Exception exception)
{
    if (exception is CustomNotFoundException)
        return HttpStatusCode.NotFound;
    else if (exception is CustomException)
        return HttpStatusCode.BadRequest;
        
    return HttpStatusCode.InternalServerError;
}
```

### Conclusion
Choose diagnostic detail deliberately. Reading and formatting stack traces has a real cost, but dropping them everywhere can make unexpected failures incredibly difficult to investigate. Measure failure rate, allocations, throughput, and tail latency under representative load before deciding that exception handling is a bottleneck.
