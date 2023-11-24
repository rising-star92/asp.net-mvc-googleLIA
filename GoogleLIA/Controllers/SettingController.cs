using System.Web.Mvc;
using GoogleLIA.Models;
using GoogleLIA.Services;

namespace GoogleLIA.Controllers
{
    public class SettingController : Controller
    {
        private readonly ISettingService _settingService;

        public SettingController(
            ISettingService settingService)
        {
            _settingService = settingService;
        }

        public ActionResult Edit()
        {
            var setting = _settingService.GetSetting();

            return View(setting);
        }

        [HttpPost]
        public ActionResult Update(Setting item)
        {
            var setting = _settingService.UpdateSetting(item);

            ViewBag.Message = setting != null ?
                "Success Updating Setting" :
                "Failed Updating Setting";
            
            return RedirectToAction("Edit");
        }

        //private static void UpdateMerchantCenterLinkStatus(long customerId, MerchantCenterLinkServiceClient merchantCenterLinkService, MerchantCenterLink merchantCenterLink, MerchantCenterLinkStatus status)
        //{
        //    // Enables the pending link.
        //    MerchantCenterLink linkToUpdate = new MerchantCenterLink()
        //    {
        //        ResourceName = merchantCenterLink.ResourceName,
        //        Status = status
        //    };

        //    // Creates an operation.
        //    MerchantCenterLinkOperation operation = new MerchantCenterLinkOperation()
        //    {
        //        Update = linkToUpdate,
        //        UpdateMask = FieldMasks.AllSetFieldsOf(linkToUpdate)
        //    };

        //    // Updates the link.
        //    MutateMerchantCenterLinkResponse mutateResponse =
        //        merchantCenterLinkService.MutateMerchantCenterLink(
        //            customerId.ToString(), operation);

        //    // Displays the result.
        //    Console.WriteLine($"The status of Merchant Center Link with resource name " +
        //        $"'{mutateResponse.Result.ResourceName}' to Google Ads account : " +
        //        $"{customerId} was updated to {status}.");
        //}
    }
}