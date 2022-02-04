using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Asgard
{
    public static class TaskExtensions
    {
        public static void AsgardFireAndForget(this Task task)
        {
            if (!task.IsCompleted || task.IsFaulted)
            {
                _ = ForgetAwaited(task);
            }

            static async Task ForgetAwaited(Task task)
            {
                try
                {
                    await task.ConfigureAwait(false);
                }
                catch { }
            }
        }

        public static void AsgardFireAndForget<TException>(this Task task, ILogger? logger, string message)
            where TException : Exception
        {
            if (!task.IsCompleted || task.IsFaulted)
            {
                _ = ForgetAwaited(task, logger, message);
            }

            static async Task ForgetAwaited(Task task, ILogger? logger, string message)
            {
                try
                {
                    await task.ConfigureAwait(false);
                }
                catch (TException ex)
                {
                    logger?.LogWarning(ex, message);
                }
                catch { }
            }
        }

        public static void AsgardFireAndForget<TException>(this Task task, ConcurrentQueue<Exception> queue)
            where TException: Exception
        {
            if (!task.IsCompleted || task.IsFaulted)
            {
                _ = ForgetAwaited(task, queue);
            }

            static async Task ForgetAwaited(Task task, ConcurrentQueue<Exception> queue)
            {
                try
                {
                    await task.ConfigureAwait(false);
                }
                catch (TException ex)
                {
                    queue.Enqueue(ex);
                }
                catch { }
            }
        }
    }
}
