using System;
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
		private static ISchedulerFactory _sf;
		private static IScheduler _scheduler;

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
			_scheduler = _sf.GetScheduler();
			_scheduler.Start();

			var job = JobBuilder.Create<UpdateJob>()
				.WithIdentity(Job.Update, Job.RatesUpdGroup).
				Build();

			//Update rates of Krasnoyarsk banks
			job.JobDataMap[Job.City] = Enums.Cities.Krsk;
			var trigger = TriggerBuilder
				.Create()
				.WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(10, 00))
				.StartNow()
				.WithIdentity(Job.KrskTrigger, Job.RatesUpdGroup)
				.Build();
			
			_scheduler.ScheduleJob(job, trigger);
			_scheduler.TriggerJob(job.Key, job.JobDataMap);
		}

		protected void Application_Error()
		{
			var lastException = Server.GetLastError();
			var logger = LogManager.GetCurrentClassLogger();
			logger.Fatal(lastException);
		}

//		protected void Application_End()
//		{
//			_scheduler.Shutdown(true);
//		}
	}
}