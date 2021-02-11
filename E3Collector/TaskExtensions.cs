namespace E3Collector
{
    using System.Threading.Tasks;

    public static class TaskExtensions
    {
        #region Methods

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

        #endregion Methods
    }
}