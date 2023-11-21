using System;
using System.Linq;
using System.Web.Mvc;
using GoogleLIA.Models;
using GoogleLIA.DBContext;
using Google.Ads.GoogleAds.V14.Services;
using Google.Ads.Gax.Util;
using Google.Ads.GoogleAds.V14.Resources;
using static Google.Ads.GoogleAds.V14.Enums.MerchantCenterLinkStatusEnum.Types;
using Google.Ads.GoogleAds;
using Google.Ads.GoogleAds.Lib;

namespace GoogleLIA.Controllers
{
    public class SettingController : Controller
    {
        private DBcontext _dbContext;
        private object customerId;
        private GoogleAdsClient client;

        public SettingController()
        {
            _dbContext = new DBcontext();
        }

        public ActionResult Update(Setting item)
        {
            //The code example below shows how to use ListMerchantCenterLinks to request all links for customer_id.
            MerchantCenterLinkServiceClient merchantCenterLinkService = client.GetService(Services.V14.MerchantCenterLinkService);
            ListMerchantCenterLinksResponse response = merchantCenterLinkService.ListMerchantCenterLinks(customerId.ToString());
            //UpdateMerchantCenterLinkStatus();

            var setting = _dbContext.Settings.FirstOrDefault();

            if (setting != null)
            {
                setting.Ads_id = item.Ads_id;
                setting.Base_url = item.Base_url;
                setting.Merchant_id = item.Merchant_id;
                setting.Shopping_cart = item.Shopping_cart;
                setting.Status = item.Status;
            }
            else
            {
                _dbContext.Settings.Add(item);                
            }

            _dbContext.SaveChanges();

            return View("Settings", setting);
        }

        // GET: Setting
        public ActionResult Settings()
        {
            var setting = _dbContext.Settings.FirstOrDefault();
            return View(setting);
        }

        private static void UpdateMerchantCenterLinkStatus(long customerId, MerchantCenterLinkServiceClient merchantCenterLinkService, MerchantCenterLink merchantCenterLink, MerchantCenterLinkStatus status)
        {
            // Enables the pending link.
            MerchantCenterLink linkToUpdate = new MerchantCenterLink()
            {
                ResourceName = merchantCenterLink.ResourceName,
                Status = status
            };

            // Creates an operation.
            MerchantCenterLinkOperation operation = new MerchantCenterLinkOperation()
            {
                Update = linkToUpdate,
                UpdateMask = FieldMasks.AllSetFieldsOf(linkToUpdate)
            };

            // Updates the link.
            MutateMerchantCenterLinkResponse mutateResponse =
                merchantCenterLinkService.MutateMerchantCenterLink(
                    customerId.ToString(), operation);

            // Displays the result.
            Console.WriteLine($"The status of Merchant Center Link with resource name " +
                $"'{mutateResponse.Result.ResourceName}' to Google Ads account : " +
                $"{customerId} was updated to {status}.");
        }
    }
}