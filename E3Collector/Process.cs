using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace E3Collector
{
    public class Process
    {
        #region Methods

        public static Task<int> LaunchProcess(string process, string args, string verb, CancellationToken token)
        {
            return Task.Run(() =>
            {
                ProcessStartInfo info = new ProcessStartInfo();
                info.CreateNoWindow = true;
                info.UseShellExecute = true;
                info.FileName = process;
                info.CreateNoWindow = true;
                info.WindowStyle = ProcessWindowStyle.Hidden;
                info.Arguments = args;
                info.Verb = verb;

                if (token.IsCancellationRequested)
                {
                    return -1;
                }

                var p = System.Diagnostics.Process.Start(info);
                token.Register(() => p.Kill());
                p.WaitForExit();

                return p.ExitCode;
            });
        }

        #endregion Methods
    }
}