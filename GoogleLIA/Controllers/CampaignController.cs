using System.Web.Mvc;
using System.Collections.Generic;
using GoogleLIA.Services;
using GoogleLIA.Models;
using GoogleLIA.Databases;
using System.Linq;
using System.Dynamic;

namespace GoogleLIA.Controllers
{
    public class CampaignController : Controller
    {
        private readonly ICampaignService _campaignService;
        private readonly IGoogleService _googleService;
        private readonly ILocationService _locationService;
        private readonly AdsDBContext _adsDBContext;

        public CampaignController(
            ICampaignService campaignService,
            IGoogleService googleService,
            ILocationService locationService,
            AdsDBContext adsDBContext)
        {
            _campaignService = campaignService;
            _googleService = googleService;
            _locationService = locationService;
            _adsDBContext = adsDBContext;
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
            //dynamic mymodel = new ExpandoObject(); 
            //mymodel.GCampaign = 
            string[] countrylist = _adsDBContext.Countries.AsEnumerable().Select(s => s.country_name).Distinct().ToArray();
            ViewBag.CountryList = new SelectList(countrylist);
            ViewBag.LocationList = new List<SelectListItem>
                {
                    new SelectListItem { Text = "", Value = "" }
                };

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

        public ActionResult Pause(int id )
        {
            var gCampaign = _adsDBContext.Campaigns.FirstOrDefault(x => x.id == id);
            var campaign_id = _adsDBContext.Campaigns.FirstOrDefault(x => x.id == id).campaign_id;
            var ret = _googleService.PauseGoogleCampaign(campaign_id);

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
        public ActionResult GetLocations(string srchStr, string country)
        {
            var locationlist = _adsDBContext.Locations.Where(x => x.canonical_name.Contains(country) && (x.canonical_name.Contains(srchStr) || x.criteria_id.Contains(srchStr) || x.name.Contains(srchStr) || x.parent_id.Contains(srchStr) || x.country_code.Contains(srchStr))).AsEnumerable().Select(s => s.canonical_name).Distinct().Take(10).ToArray();
            var result = locationlist.Select((location, index) => new { id = index, text = location }).ToArray();
            ViewBag.LocationList = result;
            return Json(new { results = result });
        }

    }
}