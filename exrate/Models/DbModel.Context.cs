﻿//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Exrate.Models
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Objects;
    using System.Data.Objects.DataClasses;
    using System.Linq;
    
    public partial class ExrateEntities : DbContext
    {
        public ExrateEntities()
            : base("name=ExrateEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public DbSet<Address> Address { get; set; }
        public DbSet<Bank> Bank { get; set; }
        public DbSet<City> City { get; set; }
        public DbSet<Currency> Currency { get; set; }
        public DbSet<Rate> Rate { get; set; }
    
        public virtual ObjectResult<GetBankRates_Result> GetBankRates(string bank, Nullable<int> cityId)
        {
            var bankParameter = bank != null ?
                new ObjectParameter("Bank", bank) :
                new ObjectParameter("Bank", typeof(string));
    
            var cityIdParameter = cityId.HasValue ?
                new ObjectParameter("CityId", cityId) :
                new ObjectParameter("CityId", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetBankRates_Result>("GetBankRates", bankParameter, cityIdParameter);
        }
    
        public virtual ObjectResult<GetBanks_Result> GetBanks(Nullable<int> cityId)
        {
            var cityIdParameter = cityId.HasValue ?
                new ObjectParameter("CityId", cityId) :
                new ObjectParameter("CityId", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetBanks_Result>("GetBanks", cityIdParameter);
        }
    
        public virtual ObjectResult<GetChartsInfo_Result> GetChartsInfo(string currency, Nullable<int> cityId)
        {
            var currencyParameter = currency != null ?
                new ObjectParameter("Currency", currency) :
                new ObjectParameter("Currency", typeof(string));
    
            var cityIdParameter = cityId.HasValue ?
                new ObjectParameter("CityId", cityId) :
                new ObjectParameter("CityId", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetChartsInfo_Result>("GetChartsInfo", currencyParameter, cityIdParameter);
        }
    
        public virtual ObjectResult<GetTodayRates_Result> GetTodayRates(Nullable<int> limit, string currency, Nullable<int> cityId, Nullable<int> sortBy)
        {
            var limitParameter = limit.HasValue ?
                new ObjectParameter("Limit", limit) :
                new ObjectParameter("Limit", typeof(int));
    
            var currencyParameter = currency != null ?
                new ObjectParameter("Currency", currency) :
                new ObjectParameter("Currency", typeof(string));
    
            var cityIdParameter = cityId.HasValue ?
                new ObjectParameter("CityId", cityId) :
                new ObjectParameter("CityId", typeof(int));
    
            var sortByParameter = sortBy.HasValue ?
                new ObjectParameter("SortBy", sortBy) :
                new ObjectParameter("SortBy", typeof(int));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<GetTodayRates_Result>("GetTodayRates", limitParameter, currencyParameter, cityIdParameter, sortByParameter);
        }
    }
}
