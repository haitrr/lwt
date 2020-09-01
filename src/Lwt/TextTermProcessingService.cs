namespace Lwt
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Lwt.Creators;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;

    public class TextTermProcessingService : BackgroundService
    {
        private readonly ILogger<TextTermProcessingService> logger;
        private readonly IServiceProvider serviceProvider;

        public TextTermProcessingService(ILogger<TextTermProcessingService> logger, IServiceProvider serviceProvider)
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this.logger.LogInformation("Text processing service running");

            while (!stoppingToken.IsCancellationRequested)
            {
                using (IServiceScope scope = this.serviceProvider.CreateScope())
                {
                    var textTermProcessor = scope.ServiceProvider.GetRequiredService<ITextTermProcessor>();

                    try
                    {
                        await textTermProcessor.ProcessTextTermAsync();
                    }
#pragma warning disable
                    catch (Exception e)
#pragma warning restore
                    {
                        this.logger.LogError("Fail to process text term", e);
                    }

                    await Task.Delay(TimeSpan.FromMilliseconds(500));
                }
            }
        }
    }
}