using System;
using System.Web.Mvc;
using System.Linq;
using GoogleLIA.Models;
using System.Data;
using System.Collections.Generic;
using GoogleLIA.Services;

namespace GoogleLIA.Controllers
{
    public class CampaignController : Controller
    {
        private readonly ICampaignService _campaignService;
        private readonly IGoogleService _googleService;
        private readonly ILocationService _locationService;

        public CampaignController(
            ICampaignService campaignService,
            IGoogleService googleService,
            ILocationService locationService)
        {
            _campaignService = campaignService;
            _googleService = googleService;
            _locationService = locationService;
        }

        public ActionResult List()
        {
            bool ret = _googleService.GetGoolgeCampaigns();

            if (ret)
            {
                var data = _campaignService.GetCampaignsList();
                return View(data);
            }

            return View();
        }

        public ActionResult Edit(int id = -1)
        {
            var countries = _locationService.GetCountries();
            ViewBag.CountryList = new SelectList(countries);

            ViewBag.LocationList = new List<SelectListItem>
            {
                new SelectListItem { Text = "", Value = ""}
            };


            if (id < 0)
            {
                return View();
            }
            else
            {
                GCampaign editItem = _campaignService.GetCampaign(id);
                return View(editItem);
            }
        }

        [HttpPost]
        public ActionResult Create(GCampaign model)
        {
            if (ModelState.IsValid)
            {
                if (model.campaign_id == null)
                {
                    _googleService.CreateGoogleCampaign(model);
                    ViewBag.Message = "Data Insert Successfully";
                }
            }

            return RedirectToAction("List");
        }

        [HttpPut]
        public ActionResult Update(GCampaign model)
        {
            var ret = false;

            if (ModelState.IsValid)
            {
                GCampaign gCampaign = _campaignService.UpdateCampaign(model);
                if (gCampaign != null)
                {
                    ret = _googleService.UpdateGoogleCampaign(gCampaign);
                }
            }

            ViewBag.Message = ret ? "Success Updating Campaign" : "Failed Updating Campaign";

            return View("Edit", model);
        }

        //[HttpGet]
        //public ActionResult Delete(int id)
        //{
        //    var data = _dbContext.Campaigns.FirstOrDefault(x => x.id == id);
        //    var campaign_id = _dbContext.Campaigns.FirstOrDefault(x => x.id == id).campaign_id;

        //    CampaignServiceClient campaignService = client.GetService(Services.V15.CampaignService);
        //    CampaignOperation operation = new CampaignOperation()
        //    {
        //        Remove = ResourceNames.Campaign(customerId, long.Parse(campaign_id))
        //    };

        //    try
        //    {
        //        // Remove the campaign.
        //        MutateCampaignsResponse retVal = campaignService.MutateCampaigns(
        //            customerId.ToString(), new CampaignOperation[] { operation });

        //        // Display the results.
        //        foreach (MutateCampaignResult removedCampaign in retVal.Results)
        //        {
        //            Console.WriteLine($"Campaign with resource name = '{0}' was removed.",
        //                removedCampaign.ResourceName);
        //        }
        //    }
        //    catch (GoogleAdsException e)
        //    {
        //        Console.WriteLine("Failure:");
        //        Console.WriteLine($"Message: {e.Message}");
        //        Console.WriteLine($"Failure: {e.Failure}");
        //        Console.WriteLine($"Request ID: {e.RequestId}");
        //        throw;
        //    }

        //    _dbContext.Campaigns.Remove(data);
        //    _dbContext.SaveChanges();
        //    ViewBag.Messsage = "Record Delete Successfully";
        //    return RedirectToAction("Campaigns");
        //}

        //[HttpGet]
        //public ActionResult Pause(Campaigns item)
        //{
        //    var data = _dbContext.Campaigns.FirstOrDefault(x => x.id == item.id);
        //    var campaign_id = _dbContext.Campaigns.FirstOrDefault(x => x.id == item.id).campaign_id;

        //    CampaignServiceClient campaignService = client.GetService(Services.V15.CampaignService);
        //    Campaign campaignToUpdate = new Campaign()
        //    {
        //        ResourceName = ResourceNames.Campaign(customerId, long.Parse(campaign_id)),
        //        Status = CampaignStatus.Paused
        //    };

        //    CampaignOperation operation = new CampaignOperation()
        //    {
        //        Update = campaignToUpdate,
        //        UpdateMask = FieldMasks.AllSetFieldsOf(campaignToUpdate)
        //    };
        //    try
        //    {
        //        // Update the campaign
        //        MutateCampaignsResponse response = campaignService.MutateCampaigns(
        //            customerId.ToString(), new[] { operation });
            
        //        // Display the results.
        //        foreach (MutateCampaignResult updatedCampaign in response.Results)
        //        {
        //            Console.WriteLine($"Campaign with resource ID = " +
        //                $"'{updatedCampaign.ResourceName}' was updated.");
        //        }
        //    }
        //    catch (GoogleAdsException e)
        //    {
        //        Console.WriteLine("Failure:");
        //        Console.WriteLine($"Message: {e.Message}");
        //        Console.WriteLine($"Failure: {e.Failure}");
        //        Console.WriteLine($"Request ID: {e.RequestId}");
        //        throw;
        //    }

        //    if (data.status == "Enabled")
        //    {
        //        data.status = "Paused";
        //        _dbContext.SaveChanges();
        //    }

        //    //_dbContext.Campaigns.Remove(data);
        //    //_dbContext.SaveChanges();
        //    //ViewBag.Messsage = "Record Delete Successfully";
        //    return RedirectToAction("Campaigns");
        //}
    }
}