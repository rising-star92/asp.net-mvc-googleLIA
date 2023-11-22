using System.Web.Mvc;
using System.Collections.Generic;
using GoogleLIA.Services;
using GoogleLIA.Models;

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
        public ActionResult Update(int id, GCampaign model)
        {
            var ret = false;

            if (ModelState.IsValid)
            {
                GCampaign gCampaign = _campaignService.UpdateCampaign(id, model);
                if (gCampaign != null)
                {
                    ret = _googleService.UpdateGoogleCampaign(gCampaign);
                }
            }

            ViewBag.Message = ret ?
                "Success Updating Campaign" :
                "Failed Updating Campaign";

            return View("Edit", model);
        }

        [HttpDelete]
        public ActionResult Delete(int id)
        {
            GCampaign gCampaign = _campaignService.GetCampaign(id);
            var campaignId = gCampaign.campaign_id;

            var ret = _googleService.DeleteGoogleCampaign(campaignId);

            if (ret)
            {
                ret = _campaignService.DeleteCampaign(id);
            }

            ViewBag.Message = ret ?
                "Success Deleting Campaign" : 
                "Failed Deleting Campaign";

            return RedirectToAction("List");
        }

        [HttpPost]
        public ActionResult Pause(GCampaign gCampaign)
        {
            var ret = _googleService.PauseGoogleCampaign(gCampaign.campaign_id);

            if (ret)
            {
                gCampaign.status = "Paused";
                gCampaign = _campaignService.UpdateCampaign(gCampaign.id, gCampaign);
            }

            ViewBag.Message = ret && gCampaign.status == "Paused" ? 
                "Success Pausing Campaign" : 
                "Failed Pausing Campaign";

            return RedirectToAction("List");
        }
    }
}