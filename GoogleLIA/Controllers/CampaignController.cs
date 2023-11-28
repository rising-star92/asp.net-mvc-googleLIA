using System.Web.Mvc;
using System.Collections.Generic;
using GoogleLIA.Services;
using GoogleLIA.Models;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<ActionResult> List()
        {
            bool ret = await _googleService.GetGoolgeCampaignsAsync();

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
        public async Task<ActionResult> Create(GCampaign model)
        {


            ViewBag.CountryList = new SelectList(_locationService.GetCountries());
            ViewBag.LocationList = new List<SelectListItem>
                {
                    new SelectListItem { Text = "", Value = "" }
                };

            if (ModelState.IsValid)
            {
                if (model.campaign_id == null)
                {
                    List<string> GeoTargetlist = TempData["GeoTargetlist"] as List<string>;
                    await _googleService.CreateGoogleCampaignAsync(model, GeoTargetlist);
                    ViewBag.Message = "Data Insert Successfully";
                }
            }
            return RedirectToAction("List");
        }

        [HttpPut]
        public async Task<ActionResult> Update(int id, GCampaign model)
        {
            var ret = false;

            if (ModelState.IsValid)
            {
                GCampaign gCampaign = _campaignService.UpdateCampaign(id, model);
                if (gCampaign != null)
                {
                    ret = await _googleService.UpdateGoogleCampaignAsync(gCampaign);
                }
            }

            ViewBag.Message = ret ?
                "Success Updating Campaign" :
                "Failed Updating Campaign";

            return View("Edit", model);
        }

        public async Task<ActionResult> Delete(int id)
        {
            GCampaign gCampaign = _campaignService.GetCampaign(id);
            var campaignId = gCampaign.campaign_id;

            var ret = await _googleService.DeleteGoogleCampaignAsync(campaignId);

            if (ret)
            {
                ret =  _campaignService.DeleteCampaign(id);
            }

            ViewBag.Message = ret ?
                "Success Deleting Campaign" : 
                "Failed Deleting Campaign";

            return RedirectToAction("List");
        }

        public async Task<ActionResult> Pause(int id )
        {
            var gCampaign = _campaignService.GetCampaign(id);
            var ret = await _googleService.PauseGoogleCampaignAsync(gCampaign.campaign_id);

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

        [HttpPost]
        public ActionResult GetLocations(string country, string srchStr)
        {
            var locationlist = _locationService.GetLocations(country, srchStr);
            var result = locationlist.Select((location, index) => new { id = index, text = location }).ToArray();
            ViewBag.LocationList = result;
            return Json(new { results = result });
        }

        [HttpPost]
        public ActionResult GetGeoTargetCodeList(List<string> locations)
        {
            List<string> GeoTargetlist = _locationService.GetGeoTargetCodeList(locations);
            TempData["GeoTargetlist"] = GeoTargetlist;
            return Json(new { results = GeoTargetlist });
            //return RedirectToAction("Create", new { results = GeoTargetlist });
        }
    }
}