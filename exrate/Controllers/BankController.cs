using System.Web.Mvc;
using Exrate.Models;
using Exrate.Services;

namespace Exrate.Controllers
{
    public class BankController : Controller
    {
        private readonly IBankService _bankService;

        public BankController(IBankService bankService)
        {
            _bankService = bankService;
        }

        public ActionResult GetBanks(BankForm form)
        {
            var banks = _bankService.GetBanks(form);

            return Json(banks);
        }

        public ActionResult GetBank(BankForm form)
        {
            var bank = _bankService.GetBank(form);

            return Json(bank);
        }
    }
}
