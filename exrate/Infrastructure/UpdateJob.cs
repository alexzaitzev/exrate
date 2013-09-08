using Exrate.Repos;
using Exrate.Services;
using Quartz;
using NLog;

namespace Exrate.Infrastructure
{
    [DisallowConcurrentExecution]
    public class UpdateJob : IJob
    {
        private readonly IRateService _rateService;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public UpdateJob()
        {
            _rateService = new RateService(new RateRepository());
        }

        public virtual void Execute(IJobExecutionContext context)
        {
            var dataMap = context.JobDetail.JobDataMap;
            var city = (Enums.Cities)dataMap.Get(Job.City);
            switch (city)
            {
                case Enums.Cities.Krsk:
                    _logger.Debug("Krsk banks was updated");
                    _rateService.UpdateKrskRates();
                    break;
                case Enums.Cities.Msc:
                    break;
                default:
                    break;
            }
        }
    }
}