using System.Collections.Generic;
using Exrate.Models;

namespace Exrate.Services
{
    public interface IBankService
    {
        List<BanksViewModel> GetBanks(BankForm form);
        BankViewModel GetBank(BankForm form);
    }
}