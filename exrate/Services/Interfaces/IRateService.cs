using System.Collections.Generic;
using Exrate.Models;

namespace Exrate.Services
{
    public interface IRateService
    {
        bool UpdateKrskRates();
        List<RateViewModel> GetTopRates(RateForm form);
        List<CityViewModel> GetCities();
        List<CbRateViewModel> GetCbRates();
        List<StatViewModel> GetAllRates(RateForm form);
    }
}