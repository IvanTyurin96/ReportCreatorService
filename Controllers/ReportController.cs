using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ReportCreatorService.Repositories;
using System.Text;
using System.IO;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace ReportCreatorService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ReportController : ControllerBase
    {
        private IReportBuilder reportBuilder;
        private IReporter reporter;
        private static int privateReportId = 0;
        private static ConcurrentDictionary<int, CancellationTokenSource> tokenDictionary = new ConcurrentDictionary<int, CancellationTokenSource>();

        public ReportController(IReportBuilder reportBuilder, IReporter reporter)
        {
            this.reportBuilder = reportBuilder;
            this.reporter = reporter;
        }

        [HttpGet]
        [ProducesResponseType(200)]
        public IActionResult Build()
        {
            int reportId = Interlocked.Increment(ref privateReportId);

            CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
            CancellationToken token = cancelTokenSource.Token;

            Task reportTask = new Task(async() =>
            {
                try
                {
                    byte[] byteArray = await reportBuilder.Build(token);
                    if(token.IsCancellationRequested)
                        return;
                        
                    reporter.ReportSuccess(byteArray, reportId);
                }
                catch(Exception)
                {
                    reporter.ReportError(reportId);
                }
                
            }, token);

            reportTask.ContinueWith(t => DestroyTokenSource(reportId));

            Task reportTimeout = new Task(async() =>
            {
                int timeoutN = 15000;
                reportTask.Start();

                if (await Task.WhenAny(reportTask, Task.Delay(timeoutN)) != reportTask) 
                {
                    cancelTokenSource.Cancel();
                    reporter.ReportTimeout(reportId);
                    return;
                }
            });
            
            tokenDictionary.TryAdd(reportId, cancelTokenSource);

            reportTimeout.Start();

            return Ok(reportId);
        }

        [HttpPost]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public IActionResult Stop([FromBody] int id)
        {
            CancellationTokenSource cancelTokenSource = tokenDictionary.FirstOrDefault(i => i.Key == id).Value;

            if(cancelTokenSource == null)
                return NotFound();
            else
                cancelTokenSource.Cancel();

            return NoContent();
        }

        private void DestroyTokenSource(int id)
        {
            if(tokenDictionary.TryRemove(id, out CancellationTokenSource cancellationTokenSource))
            {
                cancellationTokenSource.Dispose();
            }
        }
    }
}