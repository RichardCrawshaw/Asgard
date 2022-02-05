using System.Threading.Tasks;

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
    }
}
