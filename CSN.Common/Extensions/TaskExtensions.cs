namespace CSN.Common.Extensions
{
    using System.Threading.Tasks;

    public static class TaskExtensions
    {
        /// <summary>
        /// Prevent random exceptions with async void
        /// </param>
        public static async void NotAwaited(this Task task)
        {
            try
            {
                await task;
            }
            catch
            {
                // Ignore exception
            }
        }

    }
}
