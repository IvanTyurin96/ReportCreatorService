using System;
using System.IO;

namespace ReportCreatorService.Repositories
{
    public class Reporter : IReporter
    {
        private static string directory = Path.Combine(Environment.CurrentDirectory, "Reports");
        public void ReportSuccess(byte[] Data, int Id)
        {
            string report = Path.Combine(directory, $"Report_{Id}.txt");
            File.WriteAllBytesAsync(report, Data);
        }
        public void ReportError(int Id)
        {
            string report = Path.Combine(directory, $"Error_{Id}.txt");
            File.WriteAllTextAsync(report, "Report error.");
        }
        public void ReportTimeout(int Id)
        {
            string report = Path.Combine(directory, $"Timeout_{Id}.txt");
            File.WriteAllTextAsync(report, "Report timeout.");
        }
    }
}