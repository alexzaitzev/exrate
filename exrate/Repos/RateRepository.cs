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
	public class RateRepository : IRateRepository
	{
		private readonly ExrateEntities _db;
		private readonly Logger _logger = LogManager.GetCurrentClassLogger();

		public RateRepository()
		{
			_db = new ExrateEntities();
		}

		public int SaveRates(List<RateUpdateModel> rateModels)
		{
			try
			{
				var todayDate = DateTime.Now.ToUniversalTime();
				var banksCount = 0;
				foreach (var rateModel in rateModels.Where(rateModel => rateModel.Date >= todayDate.Date))
				{
					if (rateModel.IsUSDSet())
					{
						_db.Rate.Add(new Rate
							{
								Value = rateModel.USD.ValueName,
								Bank = rateModel.Bank,
								CityId = (int)Enums.Cities.Krsk,
								Date = todayDate,
								Buy = rateModel.USD.Buy,
								Sell = rateModel.USD.Sell
							});
					}
					if (rateModel.IsEURSet())
						_db.Rate.Add(new Rate
							{
								Value = rateModel.EUR.ValueName,
								Bank = rateModel.Bank,
								CityId = (int)Enums.Cities.Krsk,
								Date = todayDate,
								Buy = rateModel.EUR.Buy,
								Sell = rateModel.EUR.Sell
							});
					banksCount++;
				}
				_db.SaveChanges();
				return banksCount;
			}
			catch (Exception ex)
			{
				_logger.Debug("[RateRepository.SaveRates] " + ex.Message);
				return 0;
			}
		}
		public List<RateViewModel> GetTopRates(RateForm form)
		{
			try
			{
				var ratesDb = _db.GetTodayRates(form.RecCount, form.Currency, (int)form.City, form.SortBy).ToList();
				Mapper.CreateMap<GetTodayRates_Result, RateViewModel>()
					.ForMember(vm => vm.BuyDiff, opt => opt.MapFrom(src => (src.Buy - src.BuyYday)))
					.ForMember(vm => vm.SellDiff, opt => opt.MapFrom(src => (src.Sell - src.SellYday)))
					.ForMember(vm => vm.Logo, opt => opt.MapFrom(src => Constants.BANK_LOGO_PATH + src.Logo))
					.ForMember(vm => vm.Sell, opt => opt.MapFrom(src => Math.Round(src.Sell.Value, 2)))
					.ForMember(vm => vm.LastUpdDt, opt => opt.MapFrom(src => src.LastUpdDt.Value.ToString("dd.MM.yyyy")))
					.ForMember(vm => vm.IsOutDated, opt => opt.MapFrom(src => src.LastUpdDt.Value.Date != DateTime.UtcNow.Date));
				var rates = Mapper.Map<List<GetTodayRates_Result>, List<RateViewModel>>(ratesDb);

				return rates;
			}
			catch (Exception ex)
			{
				_logger.Debug("[RateRepository.GetTopRates] " + ex.Message);
				return new List<RateViewModel>();
			}
		}
		public List<CityViewModel> GetCities()
		{
			try
			{
				var citiesDb = _db.City.Where(city => city.IsActive).ToList();
				Mapper.CreateMap<City, CityViewModel>()
					.ForMember(ddm => ddm.Name, opt => opt.MapFrom(src => src.NameRUS))
					.ForMember(ddm => ddm.Value, opt => opt.MapFrom(src => src.Id));
				var cities = Mapper.Map<List<City>, List<CityViewModel>>(citiesDb);

				return cities.OrderBy(x => x.Name).ToList();
			}
			catch (Exception ex)
			{
				_logger.Debug("[RateRepository.GetCities] " + ex.Message);
				return new List<CityViewModel>();
			}
		}
		public bool AddChartInfo(List<StatViewModel> list, RateForm form)
		{
			try
			{
				var ratesDb = _db.GetChartsInfo(form.Currency, (int)form.City).ToList();

				foreach (var item in list)
				{
					item.BuyChart = ratesDb.Where(x => x.Bank == item.BankCode)
						.Select(y => new[] { y.Date.Value.ToString(CultureInfo.InvariantCulture), y.Buy.ToString("N2", CultureInfo.InvariantCulture) })
						.ToArray();
					item.SellChart = ratesDb.Where(x => x.Bank == item.BankCode)
						.Select(y => new[] { y.Date.Value.ToString(CultureInfo.InvariantCulture), y.Sell.ToString("N2", CultureInfo.InvariantCulture) })
						.ToArray();
				}
				return true;
			}
			catch (Exception ex)
			{
				_logger.Debug("[RateRepository.AddChartInfo] " + ex.Message);
				return false;
			}
		}
	}
}