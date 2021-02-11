namespace E3Collector
{
    using System.Linq;
    using System.Threading.Tasks;

    internal class Program
    {
        #region Methods

        private static async Task Main(string[] args)
        {
            var launchType = args.LastOrDefault();

            switch (launchType)
            {
                case "EnergyCollectorRealtime":
                    await SRUMUtil.CollectRealtime();
                    break;

                case "EnergyCollectorCheckRegistration":
                    await TaskRegistration.CheckRegistration();
                    break;

                case "EnergyCollectorRegisterTask":
                    await TaskRegistration.RegisterTask();
                    break;

                default:
                    if (args.First() == "background")
                    {
                        await PowerCfg.CollectBackground(args.Last());
                    }
                    break;
            }
        }

        #endregion Methods
    }
}