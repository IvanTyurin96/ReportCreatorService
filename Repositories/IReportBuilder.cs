using System.Threading;
using System.Threading.Tasks;

namespace ReportCreatorService.Repositories
{
    public interface IReportBuilder
    {
        public Task<byte[]> Build(CancellationToken token);
    }
}