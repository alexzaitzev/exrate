using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using AutoMapper;
using Exrate.Infrastructure;
using Exrate.Models;
using Exrate.Repos;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Text;
using NLog;
using Newtonsoft.Json;

namespace Exrate.Services
{
	public class RateService : IRateService
	{
		private readonly IRateRepository _rateRepository;
		private readonly Logger _logger;
		private readonly List<RateUpdateModel> _rates;
		private readonly ExrateEntities _db;
		private const string DATE_REGEX = @"\d{2}.\d{2}.(\d{4}|\d{2})";

		public RateService(IRateRepository rateRepository)
		{
			_rateRepository = rateRepository;
			_logger = LogManager.GetCurrentClassLogger();
			_rates = new List<RateUpdateModel>();
			_db = new ExrateEntities();
		}

		public List<RateViewModel> GetTopRates(RateForm form)
		{
			var topRates = _rateRepository.GetTopRates(form);

			return topRates;
		}
		public List<StatViewModel> GetAllRates(RateForm form)
		{
			var rates = _rateRepository.GetTopRates(form);

			Mapper.CreateMap<RateViewModel, StatViewModel>();
			var stats = Mapper.Map<List<RateViewModel>, List<StatViewModel>>(rates);

			_rateRepository.AddChartInfo(stats, form);

			return stats;
		}
		public List<CityViewModel> GetCities()
		{
			var cities = _rateRepository.GetCities();

			return cities;
		}
		public List<CbRateViewModel> GetCbRates()
		{
			try
			{
				var reader = XmlReader.Create(BuildCbUrl(Currencies.USD, Enums.Days.Week));
				var list = new List<CbRateViewModel>();
				var items = ((CbRateListModel)new XmlSerializer(typeof(CbRateListModel)).Deserialize(reader)).Items;
				items.RemoveRange(0, items.Count - 2);
				var todayValue = DecimalEx.Parse(items.Last().Value);
				list.Add(new CbRateViewModel
					{
						Currency = "$",
						Value = todayValue,
						Diff = todayValue - DecimalEx.Parse(items.First().Value)
					});
				reader = XmlReader.Create(BuildCbUrl(Currencies.EUR, Enums.Days.Week));
				items = ((CbRateListModel)new XmlSerializer(typeof(CbRateListModel)).Deserialize(reader)).Items;
				items.RemoveRange(0, items.Count - 2);
				todayValue = DecimalEx.Parse(items.Last().Value);
				list.Add(new CbRateViewModel
				{
					Currency = "€",
					Value = todayValue,
					Diff = todayValue - DecimalEx.Parse(items.First().Value)
				});

				return list;
			}
			catch (Exception ex)
			{
				_logger.Trace("[RateService.GetCbRates] " + ex.Message);
				return new List<CbRateViewModel>();
			}
		}
		/// <summary>
		/// Create query to get currency value from the CB site
		/// </summary>
		/// <param name="currency">Type of the currency</param>
		/// <param name="days">Query period</param>
		/// <returns>Query string</returns>
		private static string BuildCbUrl(string currency, Enums.Days days)
		{
			string curCode;
			switch (currency)
			{
				case Currencies.USD: curCode = "R01235";
					break;
				case Currencies.EUR: curCode = "R01239";
					break;
				default: curCode = "R01235";
					break;
			}

			var today = DateTime.Now;
			var tillDate = today.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
			var sinceDate = today.AddDays((int)days).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

			return string.Format(
				"http://www.cbr.ru/scripts/XML_dynamic.asp?date_req1={0}&date_req2={1}&VAL_NM_RQ={2}", sinceDate, tillDate, curCode);
		}
		/// <summary>
		/// Update krasnoyarsk banks rates
		/// </summary>
		/// <returns>Boolean value</returns>
		public bool UpdateKrskRates()
		{
			_rates.Clear();
			GetBankOfMoscowRates();
			GetExpressBankRates("http://www.express-bank.ru/krasnoyarsk/");
			GetLevoberezhniiBankRates();
			GetRosBankRates();
			GetLegionBankRates();
			GetAsianPacificBankRates();
			GetPromsvyazbankBankRates();
			GetAlfaBankRates();
			GetUnitedBankRates();
			GetAkBarsBankRates("http://www.akbars.ru/?set_city=krasnoyarsk");
			GetSberbankRates("http://www.sberbank.ru/krasnoyarsk/ru/quotes/currencies/");

			var banksUpd = _rateRepository.SaveRates(_rates);
			if (banksUpd > 0)
			{
				_logger.Info("Information was successfully updated for " + banksUpd + " banks");
				return true;
			}
			_logger.Info("ERROR! Information wasn't updated. See log file for details");
			return false;
		}

		#region Bank's rates updaters
		/// <summary>
		/// Set cookie to the site where it's necessary
		/// </summary>
		/// <param name="request">HTTP request</param>
		/// <returns>Boolean value</returns>
		private static bool SetKrskCookie(HttpWebRequest request)
		{
			var cookie = new Cookie
			{
				Path = "/",
				Expires = DateTime.Now.AddYears(1)
			};

			switch (request.Host)
			{
				case "www.atb.su":
					cookie.Name = "atbGIP_TOWN_CODE";
					cookie.Value = "krasnoyarsk";
					cookie.Domain = ".atb.su";
					break;
			}
			request.CookieContainer.Add(cookie);
			return true;
		}
		/// <summary>
		/// Банк Москвы
		/// </summary>
		private void GetBankOfMoscowRates()
		{
			var htmlWeb = new HtmlWeb();
			htmlWeb.OverrideEncoding = Encoding.GetEncoding("utf-8");
			try
			{
				var htmlPage = htmlWeb.Load("http://www.bm.ru/");
				var nodes = htmlPage.DocumentNode.SelectSingleNode("//table[@class='footer-rates-tbl']//tr").ChildNodes;
				var dateStr = htmlPage.DocumentNode.SelectSingleNode("//div[@class='b-currency-arrow left']").InnerText;
				//convert date from '25 Августа' to 25.08.YYYY
				var dateDayStr = new Regex(@"\d{2}").Match(dateStr).ToString();
				var dateMonthStr = new Regex(@"[^\d\s]+").Match(dateStr).ToString();
				string[] months = { "Января", "Февраля", "Марта", "Апреля", "Мая", "Июня", "Июля", "Августа", "Сентября", "Октября", "Ноября", "Декабря" };
				var dateMonthNumStr = Array.FindIndex(months, el => el.Contains(dateMonthStr)) + 1;
				var date = DateTime.Parse(dateDayStr + "." + dateMonthNumStr + "." + DateTime.UtcNow.Year);
				//end convertion
				var buy = nodes[3].InnerText.Replace(',', '.');
				var sell = nodes[7].InnerText.Replace(',', '.');
				var usd = new CurrencyModel
				{
					ValueName = Currencies.USD,
					Buy = Decimal.Parse(buy, CultureInfo.InvariantCulture),
					Sell = Decimal.Parse(sell, CultureInfo.InvariantCulture)
				};
				nodes = htmlPage.DocumentNode.SelectSingleNode("//table[@class='footer-rates-tbl']//tr[2]").ChildNodes;
				buy = nodes[3].InnerText.Replace(',', '.');
				sell = nodes[7].InnerText.Replace(',', '.');
				var eur = new CurrencyModel
				{
					ValueName = Currencies.EUR,
					Buy = Decimal.Parse(buy, CultureInfo.InvariantCulture),
					Sell = Decimal.Parse(sell, CultureInfo.InvariantCulture)
				};

				_rates.Add(new RateUpdateModel
				{
					Bank = Banks.BankOfMoscow,
					Date = date,
					CityId = (int)Enums.Cities.Krsk,
					USD = usd,
					EUR = eur
				});
			}
			catch (Exception ex)
			{
				_logger.Trace("[RateService.UpdateRates.Bank_of_Moscow] " + ex.Message);
			}
		}
		/// <summary>
		/// Восточный экспресс банк
		/// </summary>
		/// <param name="url">Url with city identifier</param>
		private void GetExpressBankRates(string url)
		{
			var htmlWeb = new HtmlWeb();
			htmlWeb.OverrideEncoding = Encoding.GetEncoding("utf-8");
			try
			{
				var htmlPage = htmlWeb.Load(url);
				var nodes = htmlPage.DocumentNode.SelectSingleNode("//div[@class='cur-table-wrapper']//table//tbody//tr[1]").ChildNodes;
				var dateStr = htmlPage.DocumentNode.SelectSingleNode("//div[@class='info']").InnerText;
				var date = DateTime.Parse(new Regex(DATE_REGEX).Match(dateStr).ToString());
				var buy = nodes[1].InnerText.Replace(',', '.');
				var sell = nodes[2].InnerText.Replace(',', '.');
				var usd = new CurrencyModel
				{
					ValueName = Currencies.USD,
					Buy = Decimal.Parse(buy, CultureInfo.InvariantCulture),
					Sell = Decimal.Parse(sell, CultureInfo.InvariantCulture)
				};
				nodes = htmlPage.DocumentNode.SelectSingleNode("//div[@class='cur-table-wrapper']//table//tbody//tr[2]").ChildNodes;
				buy = nodes[1].InnerText.Replace(',', '.');
				sell = nodes[2].InnerText.Replace(',', '.');
				var eur = new CurrencyModel
				{
					ValueName = Currencies.EUR,
					Buy = Decimal.Parse(buy, CultureInfo.InvariantCulture),
					Sell = Decimal.Parse(sell, CultureInfo.InvariantCulture)
				};

				_rates.Add(new RateUpdateModel
				{
					Bank = Banks.ExpressBank,
					Date = date,
					CityId = (int)Enums.Cities.Krsk,
					USD = usd,
					EUR = eur
				});
			}
			catch (Exception ex)
			{
				_logger.Trace("[RateService.UpdateRates.Express-Bank] " + ex.Message);
			}
		}
		/// <summary>
		/// Левобережный банк
		/// </summary>
		private void GetLevoberezhniiBankRates()
		{
			var htmlWeb = new HtmlWeb();
			htmlWeb.OverrideEncoding = Encoding.GetEncoding("windows-1251");
			try
			{
				var htmlPage = htmlWeb.Load("http://www.nskbl.ru/");
				var nodes = htmlPage.DocumentNode.SelectSingleNode("//table[@class='cursval']//tr[2]").ChildNodes;
				var dateStr = htmlPage.DocumentNode.SelectSingleNode("//div[@class='clCol-R']//h2//span[2]").InnerText;
				var date = DateTime.Parse(new Regex(DATE_REGEX).Match(dateStr).ToString());
				var buy = nodes[4].InnerText;
				var sell = nodes[6].InnerText;
				var usd = new CurrencyModel
				{
					ValueName = Currencies.USD,
					Buy = Decimal.Parse(buy, CultureInfo.InvariantCulture),
					Sell = Decimal.Parse(sell, CultureInfo.InvariantCulture)
				};
				nodes = htmlPage.DocumentNode.SelectSingleNode("//table[@class='cursval']//tr[3]").ChildNodes;
				buy = nodes[4].InnerText;
				sell = nodes[6].InnerText;
				var eur = new CurrencyModel
				{
					ValueName = Currencies.EUR,
					Buy = Decimal.Parse(buy, CultureInfo.InvariantCulture),
					Sell = Decimal.Parse(sell, CultureInfo.InvariantCulture)
				};

				_rates.Add(new RateUpdateModel
				{
					Bank = Banks.Levoberezhnii,
					Date = date,
					CityId = (int)Enums.Cities.Krsk,
					USD = usd,
					EUR = eur
				});
			}
			catch (Exception ex)
			{
				_logger.Trace("[RateService.UpdateRates.Levoberezhnii_Bank] " + ex.Message);
			}
		}
		/// <summary>
		/// Росбанк
		/// </summary>
		private void GetRosBankRates()
		{
			var htmlWeb = new HtmlWeb();
			htmlWeb.OverrideEncoding = Encoding.GetEncoding("utf-8");
			try
			{
				var htmlPage = htmlWeb.Load("http://www.rosbank.ru/ru/");
				var nodes = htmlPage.DocumentNode.SelectSingleNode("//div[@class='currency']//table//tbody//tr[1]").ChildNodes;
				var dateStr = htmlPage.DocumentNode.SelectSingleNode("//p[@class='footnote']").InnerText;
				var date = DateTime.Parse(new Regex(DATE_REGEX).Match(dateStr).ToString());
				var tokens = nodes[3].InnerText.Split('/');
				var buy = tokens[0].Replace(',', '.');
				var sell = tokens[1].Replace(',', '.');
				var usd = new CurrencyModel
				{
					ValueName = Currencies.USD,
					Buy = Decimal.Parse(buy, CultureInfo.InvariantCulture),
					Sell = Decimal.Parse(sell, CultureInfo.InvariantCulture)
				};
				nodes = htmlPage.DocumentNode.SelectSingleNode("//div[@class='currency']//table//tbody//tr[2]").ChildNodes;
				tokens = nodes[3].InnerText.Split('/');
				buy = tokens[0].Replace(',', '.');
				sell = tokens[1].Replace(',', '.');
				var eur = new CurrencyModel
				{
					ValueName = Currencies.EUR,
					Buy = Decimal.Parse(buy, CultureInfo.InvariantCulture),
					Sell = Decimal.Parse(sell, CultureInfo.InvariantCulture)
				};

				_rates.Add(new RateUpdateModel
				{
					Bank = Banks.Rosbank,
					Date = date,
					CityId = (int)Enums.Cities.Krsk,
					USD = usd,
					EUR = eur
				});
			}
			catch (Exception ex)
			{
				_logger.Trace("[RateService.UpdateRates.Rosbank] " + ex.Message);
			}
		}
		/// <summary>
		/// Легион банк
		/// </summary>
		private void GetLegionBankRates()
		{
			var htmlWeb = new HtmlWeb();
			htmlWeb.OverrideEncoding = Encoding.GetEncoding("utf-8");
			try
			{
				var htmlPage = htmlWeb.Load("http://www.lgn.ru/");
				var nodes = htmlPage.DocumentNode.SelectSingleNode("//tr[@class='usd']").ChildNodes;
				var dateStr = htmlPage.DocumentNode.SelectSingleNode("//table[@id='informer']//tr[4]").InnerText;
				var date = DateTime.Parse(new Regex(DATE_REGEX).Match(dateStr).ToString());
				var buy = nodes[3].InnerText;
				var sell = nodes[5].InnerText;
				var usd = new CurrencyModel
				{
					ValueName = Currencies.USD,
					Buy = Decimal.Parse(buy, CultureInfo.InvariantCulture),
					Sell = Decimal.Parse(sell, CultureInfo.InvariantCulture)
				};
				nodes = htmlPage.DocumentNode.SelectSingleNode("//tr[@class='eur']").ChildNodes;
				buy = nodes[3].InnerText;
				sell = nodes[5].InnerText;
				var eur = new CurrencyModel
				{
					ValueName = Currencies.EUR,
					Buy = Decimal.Parse(buy, CultureInfo.InvariantCulture),
					Sell = Decimal.Parse(sell, CultureInfo.InvariantCulture)
				};

				_rates.Add(new RateUpdateModel
				{
					Bank = Banks.Legion,
					Date = date,
					CityId = (int)Enums.Cities.Krsk,
					USD = usd,
					EUR = eur
				});
			}
			catch (Exception ex)
			{
				_logger.Trace("[RateService.UpdateRates.Legion_Bank] " + ex.Message);
			}
		}
		/// <summary>
		/// Азиатско-Тихоокеанский банк
		/// </summary>
		private void GetAsianPacificBankRates()
		{
			var htmlWeb = new HtmlWeb();
			htmlWeb.OverrideEncoding = Encoding.GetEncoding("utf-8");
			try
			{
				htmlWeb.UseCookies = true;
				htmlWeb.PreRequest = SetKrskCookie;
				var htmlPage = htmlWeb.Load("http://www.atb.su/");
				var nodes = htmlPage.DocumentNode.SelectSingleNode("//div[@id='currency_some_atb']//table//tr[1]").ChildNodes;
				var dateStr = htmlPage.DocumentNode.SelectSingleNode("//div[@id='last_time']").InnerText;
				var date = DateTime.Parse(new Regex(DATE_REGEX).Match(dateStr).ToString());
				var buy = nodes[3].InnerText.Replace(',', '.'); ;
				var sell = nodes[5].InnerText.Replace(',', '.'); ;
				var usd = new CurrencyModel
				{
					ValueName = Currencies.USD,
					Buy = Decimal.Parse(buy, CultureInfo.InvariantCulture),
					Sell = Decimal.Parse(sell, CultureInfo.InvariantCulture)
				};
				nodes = htmlPage.DocumentNode.SelectNodes("//div[@id='currency_some_atb']//table//tr[2]")[1].ChildNodes;
				buy = nodes[3].InnerText.Replace(',', '.'); ;
				sell = nodes[5].InnerText.Replace(',', '.'); ;
				var eur = new CurrencyModel
				{
					ValueName = Currencies.EUR,
					Buy = Decimal.Parse(buy, CultureInfo.InvariantCulture),
					Sell = Decimal.Parse(sell, CultureInfo.InvariantCulture)
				};

				_rates.Add(new RateUpdateModel
				{
					Bank = Banks.AsianPacificBank,
					Date = date,
					CityId = (int)Enums.Cities.Krsk,
					USD = usd,
					EUR = eur
				});
			}
			catch (Exception ex)
			{
				_logger.Trace("[RateService.UpdateRates.Asian-Pacific_Bank] " + ex.Message);
			}
		}
		/// <summary>
		/// Промсвязьбанк
		/// </summary>
		private void GetPromsvyazbankBankRates()
		{
			var htmlWeb = new HtmlWeb();
			htmlWeb.OverrideEncoding = Encoding.GetEncoding("utf-8");
			try
			{
				var reqGet = WebRequest.Create(
						@"http://www.psbank.ru/psbservices/SearchService.svc/GetCurrencyRatesSpecified?shortNames=%5B%22USD%22%2C%22EUR%22%5D");
				var resp = reqGet.GetResponse();
				var stream = resp.GetResponseStream();
				var sr = new System.IO.StreamReader(stream);
				var jsonResult = sr.ReadToEnd();
				dynamic resObj = JsonConvert.DeserializeObject(jsonResult);

				var date = DateTime.Parse(DateTime.UtcNow.ToShortDateString());
				var usd = new CurrencyModel
				{
					ValueName = Currencies.USD,
					Buy = DecimalEx.Parse(Convert.ToString(resObj[0].PurchasingRate)),
					Sell = DecimalEx.Parse(Convert.ToString(resObj[0].SellingRate))
				};
				var eur = new CurrencyModel
				{
					ValueName = Currencies.EUR,
					Buy = DecimalEx.Parse(Convert.ToString(resObj[1].PurchasingRate)),
					Sell = DecimalEx.Parse(Convert.ToString(resObj[1].SellingRate))
				};

				_rates.Add(new RateUpdateModel
				{
					Bank = Banks.Promsvyazbank,
					Date = date,
					CityId = (int)Enums.Cities.Krsk,
					USD = usd,
					EUR = eur
				});
			}
			catch (Exception ex)
			{
				_logger.Trace("[RateService.UpdateRates.Promsvyazbank] " + ex.Message);
			}
		}
		/// <summary>
		/// Альфа-Банк
		/// </summary>
		private void GetAlfaBankRates()
		{
			var htmlWeb = new HtmlWeb();
			htmlWeb.OverrideEncoding = Encoding.GetEncoding("windows-1251");
			try
			{
				var htmlPage = htmlWeb.Load("http://www.alfabank.ru/retail/exchangerates/");
				var nodes = htmlPage.DocumentNode.SelectNodes("//table[1]//tr[1]")[1].ChildNodes;
				var dateStr = htmlPage.DocumentNode.SelectSingleNode("//table//caption").InnerText;
				var date = DateTime.Parse(new Regex(DATE_REGEX).Match(dateStr).ToString());
				var buy = nodes[1].InnerText;
				var sell = nodes[2].InnerText;
				var usd = new CurrencyModel
				{
					ValueName = Currencies.USD,
					Buy = Decimal.Parse(buy),
					Sell = Decimal.Parse(sell)
				};
				nodes = htmlPage.DocumentNode.SelectNodes("//table[1]//tr[2]")[1].ChildNodes;
				buy = nodes[1].InnerText;
				sell = nodes[2].InnerText;
				var eur = new CurrencyModel
				{
					ValueName = Currencies.EUR,
					Buy = Decimal.Parse(buy),
					Sell = Decimal.Parse(sell)
				};

				_rates.Add(new RateUpdateModel
				{
					Bank = Banks.AlfaBank,
					Date = date,
					CityId = (int)Enums.Cities.Krsk,
					USD = usd,
					EUR = eur
				});
			}
			catch (Exception ex)
			{
				_logger.Trace("[RateService.UpdateRates.Alfa-bank] " + ex.Message);
			}
		}
		/// <summary>
		/// Енисейский объединенный банк
		/// </summary>
		private void GetUnitedBankRates()
		{
			var htmlWeb = new HtmlWeb();
			htmlWeb.OverrideEncoding = Encoding.GetEncoding("koi8-r");
			try
			{
				var htmlPage = htmlWeb.Load("http://www.united.ru/");
				var nodes = htmlPage.DocumentNode.SelectSingleNode("//div[@class='tb1']//table//tr[2]").ChildNodes;
				var offsetKrskInHours = _db.City.First(x => x.Id == (int)Enums.Cities.Krsk).UTCOffset;
				var date = DateTime.UtcNow.AddHours(offsetKrskInHours);
				var buy = nodes[1].InnerText;
				var sell = nodes[2].InnerText;
				var usd = new CurrencyModel	
				{
					ValueName = Currencies.USD,
					Buy = Decimal.Parse(buy),
					Sell = Decimal.Parse(sell),
				};
				nodes = htmlPage.DocumentNode.SelectSingleNode("//div[@class='tb1']//table//tr[3]").ChildNodes;
				buy = nodes[1].InnerText;
				sell = nodes[2].InnerText;
				var eur = new CurrencyModel
				{
					ValueName = Currencies.EUR,
					Buy = Decimal.Parse(buy),
					Sell = Decimal.Parse(sell),
				};

				_rates.Add(new RateUpdateModel
				{
					Bank = Banks.EniseyUnitedBank,
					Date = date,
					CityId = (int)Enums.Cities.Krsk,
					USD = usd,
					EUR = eur
				});
			}
			catch (Exception ex)
			{
				_logger.Trace("[RateService.UpdateRates.Enisey_united_bank] " + ex.Message);
			}
		}
		/// <summary>
		/// Ак Барс Банк
		/// </summary>
		/// <param name="url">Url with city identifier</param>
		private void GetAkBarsBankRates(string url)
		{
			var htmlWeb = new HtmlWeb();
			htmlWeb.OverrideEncoding = Encoding.GetEncoding("windows-1251");
			try
			{
				var htmlPage = htmlWeb.Load(url);
				var nodes = htmlPage.DocumentNode.SelectSingleNode("//table[@class='kurs']//tr[2]").ChildNodes;
				var dateStr = htmlPage.DocumentNode.SelectSingleNode("//td[@class='oval_inner']//span[2]").InnerText;
				var date = DateTime.Parse(new Regex(DATE_REGEX).Match(dateStr).ToString());
				var buy = nodes[3].InnerText;
				var sell = nodes[5].InnerText;
				var usd = new CurrencyModel
				{
					ValueName = Currencies.USD,
					Buy = Decimal.Parse(buy, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture),
					Sell = Decimal.Parse(sell, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture)
				};
				nodes = htmlPage.DocumentNode.SelectSingleNode("//table[@class='kurs']//tr[3]").ChildNodes;
				buy = nodes[3].InnerText;
				sell = nodes[5].InnerText;
				var eur = new CurrencyModel
				{
					ValueName = Currencies.EUR,
					Buy = Decimal.Parse(buy, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture),
					Sell = Decimal.Parse(sell, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture)
				};

				_rates.Add(new RateUpdateModel
				{
					Bank = Banks.AkBarsBank,
					Date = date,
					CityId = (int)Enums.Cities.Krsk,
					USD = usd,
					EUR = eur
				});
			}
			catch (Exception ex)
			{
				_logger.Trace("[RateService.UpdateRates.Ak_Bars_bank] " + ex.Message);
			}
		}
		/// <summary>
		/// Сбербанк
		/// </summary>
		/// <param name="url">Url with city identifier</param>
		private void GetSberbankRates(string url)
		{
			var htmlWeb = new HtmlWeb();
			htmlWeb.OverrideEncoding = Encoding.GetEncoding("windows-1251");
			try
			{
				var htmlPage = htmlWeb.Load(url);
				var nodes = htmlPage.DocumentNode.SelectSingleNode("//table[@class='table3_eggs4']//tr[2]").ChildNodes;
				var dateStr = htmlPage.DocumentNode.SelectSingleNode("//div[@class='r_g_col5']//h3").InnerText;
				var date = DateTime.Parse(new Regex(DATE_REGEX).Match(dateStr).ToString());

				var buy = nodes[5].InnerText;
				var sell = nodes[9].InnerText;
				var usd = new CurrencyModel
				{
					ValueName = Currencies.USD,
					Buy = Decimal.Parse(buy),
					Sell = Decimal.Parse(sell)
				};
				nodes = htmlPage.DocumentNode.SelectSingleNode("//table[@class='table3_eggs4']//tr[3]").ChildNodes;
				buy = nodes[5].InnerText;
				sell = nodes[9].InnerText;
				var eur = new CurrencyModel
				{
					ValueName = Currencies.EUR,
					Buy = Decimal.Parse(buy),
					Sell = Decimal.Parse(sell)
				};

				_rates.Add(new RateUpdateModel
				{
					Bank = Banks.Cberbank,
					Date = date,
					CityId = (int)Enums.Cities.Krsk,
					USD = usd,
					EUR = eur
				});
			}
			catch (Exception ex)
			{
				_logger.Trace("[RateService.UpdateRates.Cberbank] " + ex.Message);
			}
		}
		#endregion
	}
}