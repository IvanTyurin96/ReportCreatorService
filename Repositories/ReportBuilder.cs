using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Text;

namespace ReportCreatorService.Repositories
{
    public class ReportBuilder : IReportBuilder
    {
        public async Task<byte[]> Build(CancellationToken token)
        {
            
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            Random random = new Random();
            int sleepCount = random.Next(5,46);
            int probability = random.Next(1, 101);

            for(int i = 0; i < sleepCount; i++)
            {
                if(token.IsCancellationRequested)
                    return Array.Empty<byte>();
                       
                if(i == 3 && probability <= 20)
                    throw new Exception("Report failed.");
                   
                Thread.Sleep(1000);
            }

            stopwatch.Stop();

            string elapsedTime = $"Report built in {stopwatch.Elapsed.Seconds} s.";
            return await Task.FromResult(Encoding.UTF8.GetBytes(elapsedTime));
        }
    }
}