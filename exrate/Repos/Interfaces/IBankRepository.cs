using System.Collections.Generic;
using Exrate.Models;

namespace Exrate.Repos
{
    public interface IBankRepository
    {
        List<BanksViewModel> GetBanks(BankForm form);
        BankViewModel GetBank(BankForm form);
    }
}