using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AutoMapper;
using Exrate.Infrastructure;
using Exrate.Models;
using NLog;

namespace Exrate.Repos
{
    public class BankRepository : IBankRepository
    {
        private readonly ExrateEntities _db;
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();

        public BankRepository()
        {
            _db = new ExrateEntities();
        }

        public List<BanksViewModel> GetBanks(BankForm form)
        {
            try
            {
                var dbBanks = _db.GetBanks(form.CityId).ToList();
                Mapper.CreateMap<GetBanks_Result, BanksViewModel>()
                      .ForMember(vm => vm.Logo, opt => opt.MapFrom(src => Constants.BANK_LOGO_PATH + src.Logo));
                var banks = Mapper.Map<List<GetBanks_Result>, List<BanksViewModel>>(dbBanks);

                return banks.OrderBy(x => x.Name).ToList();
            }
            catch (Exception ex)
            {
                _logger.Debug("[BankRepository.GetBanks] " + ex.Message);
                return new List<BanksViewModel>();
            }
        }

        public BankViewModel GetBank(BankForm form)
        {
            try
            {
                //Get bank info
                var cityIdStr = form.CityId.ToString(CultureInfo.InvariantCulture);
                var bank = _db.Bank.Where(x => x.CitiesIds.Contains(cityIdStr) && x.ShortName == form.Bank)
                    .Select(y => new BankViewModel{Name = y.Name, Logo = Constants.BANK_LOGO_PATH + y.Logo_big, Website = y.Website}).SingleOrDefault();

                if(bank == null) return new BankViewModel();

                //Add rates
                var bankRatesDb = _db.GetBankRates(form.Bank, form.CityId).ToList();
                Mapper.CreateMap<GetBankRates_Result, CurrencyModel>();
                bank.Rates = Mapper.Map<List<GetBankRates_Result>, List<CurrencyModel>>(bankRatesDb);

                //Add adresses of departments in the city
                bank.Addresses =_db.Address.Where(x => x.Bank == form.Bank && x.CityId == form.CityId)
                       .Select(y => new AddressModel {Address = y.Address1, Latitude = y.Latitude, Longitude = y.Longitude})
                       .ToList();

                return bank;
            }
            catch (Exception ex)
            {
                _logger.Debug("[BankRepository.GetBank] " + ex.Message);
                return new BankViewModel();
            }
        }
    }
}