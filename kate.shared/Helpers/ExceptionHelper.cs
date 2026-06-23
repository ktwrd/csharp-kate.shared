///////////////////////////////////////////////////////////////////////////////
//   Copyright 2026 Kate Ward (https://kate.pet)                             //
//                                                                           //
//   Licensed under the Apache License, Version 2.0 (the "License");         //
//   you may not use this file except in compliance with the License.        //
//   You may obtain a copy of the License at                                 //
//                                                                           //
//       http://www.apache.org/licenses/LICENSE-2.0                          //
///////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

public static class ExceptionHelper
{
    /// <summary>
    /// Retry <paramref name="callback"/> multiple times (defined by <paramref name="count"/>) if the exception that's thrown is assumed to be a timeout exception (detected by <see cref="IsTimedOut"/>)
    /// </summary>
    /// <param name="callback">Method to call. Will be invoked multiple times, no more than the amount of times defined in <paramref name="count"/></param>
    /// <param name="count">Amount of retries before giving up and throwing <see cref="AggregateException"/> with all the exceptions that were thrown by <paramref name="callback"/>.
    /// If this is &lt;1, then it'll be reset to <c>1</c></param>
    /// <exception cref="AggregateException">
    /// Thrown if the amount of retries exceeds the <paramref name="count"/> defined, but if <paramref name="count"/> is <c>1</c>, then it'll re-throw the captured exception type.
    /// </exception>
    /// <remarks>
    /// If <paramref name="count"/> is set to <c>1</c> and it fails, then the exception that was captured will be re-thrown instead of being wrapped in an <see cref="ArgumentException"/>
    /// </remarks>
    public static async Task RetryOnTimedOut(Func<Task> callback, int count = DefaultCount)
    {
        count = Math.Max(1, count);
        var exceptions = new List<Exception>(count);
        for (int i = 0; i <= count; i++)
        {
            try
            {
                await callback();
                return;
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
                if (IsTimedOut(ex) && i < count) continue;
                // just rethrow if this is the only exception, otherwise throw AggregateException
                if (exceptions.Count == 1) throw;
                throw new AggregateException(exceptions);
            }
        }
    }

    /// <returns>
    /// Result data from a successful attempt of calling the <paramref name="callback"/> provided.
    /// </returns>
    /// <inheritdoc cref="RetryOnTimedOut(Func{Task}, int)"/>
    public static async Task<TResult> RetryOnTimedOut<TResult>(Func<Task<TResult>> callback, int count = DefaultCount)
    {
        count = Math.Max(1, count);
        var exceptions = new List<Exception>(count);
        for (int i = 0; i <= count; i++)
        {
            try
            {
                return await callback();
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
                if (IsTimedOut(ex) && i < count) continue;
                // just rethrow if this is the only exception, otherwise throw AggregateException
                if (exceptions.Count == 1) throw;
                throw new AggregateException(exceptions);
            }
        }
        throw new NotImplementedException();
    }

    /// <inheritdoc cref="RetryOnTimedOut(Func{Task}, int)"/>
    public static void RetryOnTimedOut(Action callback, int count = DefaultCount)
    {
        RetryOnTimedOut(InnerCallback, count).GetAwaiter().GetResult();
        return;

        Task InnerCallback()
        {
            callback();
            return Task.CompletedTask;
        }
    }

    /// <inheritdoc cref="RetryOnTimedOut{TResult}(Func{Task{TResult}}, int)"/>
    public static TResult RetryOnTimedOut<TResult>(Func<TResult> callback, int count = DefaultCount)
    {
        return RetryOnTimedOut(InnerCallback, count).GetAwaiter().GetResult();

        Task<TResult> InnerCallback()
        {
            var result = callback();
            return Task.FromResult(result);
        }
    }

    private const int DefaultCount = 10;

    /// <summary>
    /// Is the exception provided assumed to be a timeout exception?
    /// Checks if it's <see cref="TimeoutException"/>, then if it's inner exception is <see cref="TimeoutException"/>
    /// or if the exception as a string matches the following regular expressions;
    /// <c>time(?:d)?\s?out</c>,
    /// <c>bad\s*gateway</c>,
    /// <c>error[\s_:=]*5[0-9]{2}</c>
    /// or if the exception string contains any of the following (case-insensitive):
    /// <c>error 503</c>,
    /// <c>service unavailable</c>,
    /// <c>unable to connect to the remote server</c>,
    /// <c>connection was closed</c>,
    /// <c>connection closed</c>
    /// or the exception type name is equal to <c>GatewayTimeoutException</c>.
    /// </summary>
    /// <remarks>
    /// If <paramref name="checkGlobalExpressions"/> is set to <see langword="true"/>, then this will return true if any expressions added with <see cref="AddTimedOutExpression"/> matches.
    /// Same applies if <paramref name="checkGlobalTypes"/> is set to <see langword="true"/>, and it checks any of the types added with <see cref="AddTimedOutType"/>.
    /// </remarks>
    public static bool IsTimedOut(
        Exception exception,
        bool checkGlobalTypes,
        bool checkGlobalExpressions,
        bool defaultChecks)
    {
        var exceptionType = exception.GetType();
        var exceptionTypeName = exceptionType.Namespace + '.' + exceptionType.Name;
        var exceptionStr = exception.ToString();
        if (exception is TimeoutException || exception?.InnerException is TimeoutException)
            return true;
        
        // discord checks
        if (defaultChecks && string.Equals(exceptionType.Name, "GatewayReconnectException", StringComparison.OrdinalIgnoreCase))
            return true;
        
        bool state = false;
        if (!state && checkGlobalTypes && IsTimedOutTypes.Count > 0)
        {
            IsTimedOutTypesLock.Wait();
            try
            {
                state |= IsTimedOutTypes.Contains(exceptionType);
            }
            finally
            {
                IsTimedOutTypesLock.Release();
            }
        }

        if (!state && checkGlobalExpressions && IsTimedOutExpressions.Count > 0)
        {
            IsTimedOutExpressionsLock.Wait();
            try
            {
                state |= IsTimedOutExpressions.Any(pair => pair.Value.IsMatch(exceptionStr));
            }
            finally
            {
                IsTimedOutExpressionsLock.Release();
            }
        }

        if (!state && defaultChecks)
        {
            exceptionStr = exceptionStr.ToLower();
            state |= exceptionStr.Contains("timed out")
                       || exceptionStr.Contains("time out")
                       || exceptionStr.Contains("timeout")
                       || exceptionStr.Contains("error 503")
                       || exceptionStr.Contains("service unavailable")
                       || (exceptionStr.Contains("502") &&
                          (exceptionStr.Contains("bad gateway") || exceptionStr.Contains("badgateway")))
                       || exceptionStr.Contains("unable to connect to the remote server")
                       || exceptionStr.Contains("connection was closed")
                       || exceptionStr.Contains("connection closed");
            if (!state) state = IsTimedOutRx01.IsMatch(exceptionStr);
        }
        return state;
    }
    private static readonly Regex IsTimedOutRx01 = new Regex(@"error[\s_:=]*5[0-9]{2}", RegexOptions.IgnoreCase);
    
    /// <summary>
    /// Calls <see cref="IsTimedOut(Exception, bool, bool, bool)"/>, while setting all the options to <see langword="true"/>
    /// </summary>
    /// <param name="exception"></param>
    /// <returns></returns>
    public static bool IsTimedOut(Exception exception)
    {
        return IsTimedOut(exception, true, true, true);
    }

    private static readonly List<KeyValuePair<Guid, Regex>> IsTimedOutExpressions = new List<KeyValuePair<Guid, Regex>>();
    private static readonly HashSet<Type> IsTimedOutTypes = new HashSet<Type>();
    private static readonly SemaphoreSlim IsTimedOutExpressionsLock = new SemaphoreSlim(1, 1);
    private static readonly SemaphoreSlim IsTimedOutTypesLock = new SemaphoreSlim(1, 1);

    public static void AddTimedOutType(Type type)
    {
        IsTimedOutTypesLock.Wait();
        try
        {
            IsTimedOutTypes.Add(type);
        }
        finally
        {
            IsTimedOutTypesLock.Release();
        }
    }
    public static void AddTimedOutTypes(Type[] type)
    {
        IsTimedOutTypesLock.Wait();
        try
        {
            foreach (var item in type) IsTimedOutTypes.Add(item);
        }
        finally
        {
            IsTimedOutTypesLock.Release();
        }
    }
    public static void RemoveTimedOutType(Type type)
    {
        IsTimedOutTypesLock.Wait();
        try
        {
            IsTimedOutTypes.Remove(type);
        }
        finally
        {
            IsTimedOutTypesLock.Release();
        }
    }
    public static void RemoveTimedOutTypes(Type[] type)
    {
        IsTimedOutTypesLock.Wait();
        try
        {
            foreach (var item in type) IsTimedOutTypes.Remove(item);
        }
        finally
        {
            IsTimedOutTypesLock.Release();
        }
    }

    public static void AddTimedOutExpression(Guid guid, Regex expression)
    {
        IsTimedOutExpressionsLock.Wait();
        try
        {
            if (IsTimedOutExpressions.Any(e => e.Key == guid))
            {
                throw new ArgumentException($"Expression already exists with Guid: {guid}", nameof(guid));
            }
            IsTimedOutExpressions.Add(new KeyValuePair<Guid, Regex>(guid, expression));
        }
        finally
        {
            IsTimedOutExpressionsLock.Release();
        }
    }
    public static Guid AddTimedOutExpression(Regex expression)
    {
        var key = Guid.NewGuid();
        IsTimedOutExpressionsLock.Wait();
        try
        {
            IsTimedOutExpressions.Add(new KeyValuePair<Guid, Regex>(key, expression));
        }
        finally
        {
            IsTimedOutExpressionsLock.Release();
        }
        return key;
    }
    public static void RemovedTimedOutExpression(Guid guid)
    {
        IsTimedOutExpressionsLock.Wait();
        try
        {
            IsTimedOutExpressions.RemoveAll(e => e.Key == guid);
        }
        finally
        {
            IsTimedOutExpressionsLock.Release();
        }
    }
}