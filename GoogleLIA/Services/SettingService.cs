using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Web;
using GoogleLIA.Databases;
using GoogleLIA.Models;

namespace GoogleLIA.Services
{
    public interface ISettingService
    {
        Setting GetSetting();
        Setting UpdateSetting(Setting item);
    }
    public class SettingService : ISettingService
    {
        private readonly AdsDBContext _dbContext;

        public SettingService(AdsDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Setting GetSetting()
        {
            return _dbContext.Settings.First();
        }

        public Setting UpdateSetting(Setting item)
        {
            var setting = _dbContext.Settings.First();

            if (setting != null)
            {
                setting.Ads_id = item.Ads_id;
                setting.Base_url = item.Base_url;
                setting.Merchant_id = item.Merchant_id;
                setting.Shopping_cart = item.Shopping_cart;
                setting.Status = item.Status;
            }

            _dbContext.Settings.AddOrUpdate(setting);
            _dbContext.SaveChanges();

            return setting;
        }
    }
}