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
        List<string> GetLocations(string country, string srchStr);
        List<string> GetGeoTargetCodeList(List<string> locations);
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

        public List<string> GetLocations(string country, string srchStr)
        {
            var locationlist = _dbContext.Locations
                .Where(x => 
                    x.canonical_name.Contains(country) &&
                    (x.canonical_name.Contains(srchStr) ||
                    x.criteria_id.Contains(srchStr) ||
                    x.name.Contains(srchStr) ||
                    x.parent_id.Contains(srchStr) ||
                    x.country_code.Contains(srchStr)))
                .AsEnumerable()
                .Select(s => s.canonical_name)
                .Distinct()
                .Take(30)
                .ToList();

            return locationlist;
        }

        public List<string> GetGeoTargetCodeList(List<string> locations)
        {
            List<string> geoTargetCodeList = new List<string>();

            foreach (var location in locations)
            {
                var geotargetcode = _dbContext.Locations.Where(x => x.canonical_name == location).FirstOrDefault()?.criteria_id;
                if (geotargetcode != null)
                {
                    geoTargetCodeList.Add(geotargetcode);
                }
            }
            return geoTargetCodeList;
        }
    }
}