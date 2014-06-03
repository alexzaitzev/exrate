using System.Web.Mvc;
using Exrate.Services;

namespace Exrate.Controllers
{
	public class HomeController : Controller
	{
		private readonly IRateService _rateService;

		public HomeController(IRateService rateService)
		{
			_rateService = rateService;
		}

		public ActionResult Index()
		{
			return View();
		}

		public ActionResult GetCities()
		{
			var cities = _rateService.GetCities();

			return Json(cities);
		}
	}
}
