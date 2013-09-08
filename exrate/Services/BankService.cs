using System.Collections.Generic;
using Exrate.Models;
using Exrate.Repos;

namespace Exrate.Services
{
    public class BankService : IBankService
    {
        private readonly IBankRepository _bankRepository;

        public BankService(IBankRepository bankRepository)
        {
            _bankRepository = bankRepository;
        }

        public List<BanksViewModel> GetBanks(BankForm form)
        {
            var allBanks = _bankRepository.GetBanks(form);

            return allBanks;
        }

        public BankViewModel GetBank(BankForm form)
        {
            var bank = _bankRepository.GetBank(form);

            return bank;
        }
    }
}