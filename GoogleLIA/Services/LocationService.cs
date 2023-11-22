using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GoogleLIA.Models;
using GoogleLIA.Databases;

namespace GoogleLIA.Services
{
    public interface ILocationService
    {
        List<string> GetCountries();
    }

    public class LocationService : ILocationService
    {
        private readonly AdsDBContext _dbContext;

        public LocationService(AdsDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<string> GetCountries()
        {
            var countries = _dbContext.Countries
                .AsEnumerable()
                .Select(s => s.country_name)
                .Distinct()
                .ToList();

            return countries;
        }
    }
}