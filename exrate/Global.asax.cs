using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Exrate.Infrastructure;
using NLog;
using Quartz;
using Quartz.Impl;

namespace Exrate
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private ISchedulerFactory _sf;
        private IScheduler _sched;

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            DependencyResolver.SetResolver(new NinjectDependencyResolver());
            WebApiConfig.Register(GlobalConfiguration.Configuration);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            //Make a schedule jobs for updating rates
            _sf = new StdSchedulerFactory();
            _sched = _sf.GetScheduler();

            var job = JobBuilder.Create<UpdateJob>().WithIdentity(Job.Update, Job.RatesUpdGroup).Build();
            //Update rates of Krasnoyarsk banks
            job.JobDataMap[Job.City] = Enums.Cities.Krsk;
            var trigger = TriggerBuilder.Create().WithIdentity(Job.KrskTrigger, Job.RatesUpdGroup).StartNow().WithCronSchedule(Job.CronEveryDayAt5).Build();
            _sched.ScheduleJob(job, trigger);
            _sched.TriggerJob(job.Key, job.JobDataMap);
//            _sched.Start();
        }

        protected void Application_Error()
        {
            var lastException = Server.GetLastError();
            var logger = LogManager.GetCurrentClassLogger();
            logger.Fatal(lastException);
        }

        protected void Application_End()
        {
            _sched.Shutdown(true);
        }
    }
}