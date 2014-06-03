using System.Web.Mvc;
using Exrate.Models;
using Exrate.Services;

namespace Exrate.Controllers
{
	public class RateController : Controller
	{
		private readonly IRateService _rateService;
		private const int TOP_RECORDS = 10;
		private const int ALL_RECORDS = 1000;

		public RateController(IRateService rateService)
		{
			_rateService = rateService;
		}

		public ActionResult GetTopRates(RateForm form)
		{
			form.RecCount = TOP_RECORDS;
			var topRates = _rateService.GetTopRates(form);
//			_rateService.UpdateKrskRates();

			return Json(topRates);
		}

		public ActionResult GetAllRates(RateForm form)
		{
			form.RecCount = ALL_RECORDS;
			var allRates = _rateService.GetAllRates(form);

			return Json(allRates);
		}

		public ActionResult GetCbRates()
		{
			var cbRates = _rateService.GetCbRates();

			return Json(cbRates);
		}
	}
}
