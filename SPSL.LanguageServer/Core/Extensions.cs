using System.Collections.Concurrent;

namespace SPSL.LanguageServer.Core;

public static class Extensions
{
    public static EventHandler<T> Debounce<T>(this EventHandler<T> func, TimeSpan milliseconds)
    {
        CancellationTokenSource? cancelTokenSource = null;

        return (sender, args) =>
        {
            cancelTokenSource?.Cancel();
            cancelTokenSource = new();

            _ = Task.Delay(milliseconds, cancelTokenSource.Token)
                .ContinueWith
                (t =>
                    {
                        if (t.IsCompletedSuccessfully)
                        {
                            func(sender, args);
                        }
                    },
                    TaskScheduler.Default
                );
        };
    }

    public static Action<T> Debounce<T>(this Action<T> func, TimeSpan milliseconds) where T : notnull
    {
        ConcurrentDictionary<T, CancellationTokenSource?> cancelTokenSources = new();

        return arg =>
        {
            CancellationTokenSource? cancelTokenSource = cancelTokenSources.GetOrAdd(arg, _ => null);

            cancelTokenSource?.Cancel();
            cancelTokenSource = cancelTokenSources[arg] = new();

            _ = Task.Delay(milliseconds, cancelTokenSource.Token)
                .ContinueWith
                (t =>
                    {
                        if (!t.IsCompletedSuccessfully) return;

                        func(arg);
                        cancelTokenSources.TryRemove(arg, out _);
                    },
                    TaskScheduler.Default
                );
        };
    }
}