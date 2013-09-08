using System.Collections.Generic;
using Exrate.Models;

namespace Exrate.Repos
{
    public interface IRateRepository
    {
        int SaveRates(List<RateUpdateModel> rateModels);
        List<RateViewModel> GetTopRates(RateForm form);
        List<CityViewModel> GetCities();
        bool AddChartInfo(List<StatViewModel> list, RateForm form);
    }
}