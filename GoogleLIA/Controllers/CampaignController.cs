using System;
using System.Web.Mvc;
using Google.Ads.Gax.Examples;
using Google.Ads.GoogleAds;
using Google.Ads.GoogleAds.Lib;
using Google.Ads.GoogleAds.V15.Resources;
using Google.Ads.GoogleAds.V15.Services;
using static Google.Ads.GoogleAds.V15.Resources.Campaign.Types;
using static Google.Ads.GoogleAds.V15.Enums.AdvertisingChannelTypeEnum.Types;
using static Google.Ads.GoogleAds.V15.Enums.CampaignStatusEnum.Types;
using Google.Ads.GoogleAds.V15.Common;
using Google.Ads.GoogleAds.V15.Errors;
using Google.Ads.GoogleAds.Config;
using GoogleLIA.DBContext;
using System.Linq;
using GoogleLIA.Models;
using System.Data;
using static Google.Ads.GoogleAds.V15.Enums.BudgetDeliveryMethodEnum.Types;
using System.Collections.Generic;
using Google.Ads.Gax.Util;

namespace GoogleLIA.Controllers
{
    public class CampaignController : Controller
    {
        // GET: Campaign
        private GoogleAdsClient client;
        private DBcontext _dbContext;
        private long customerId = 3692349001;

        GoogleAdsConfig config = new GoogleAdsConfig()
        {
            DeveloperToken = "5LICD0jv1UxzLAIIMWgXsQ",
            //OAuth2Mode = "APPLICATION",
            OAuth2ClientId = "1045645606174-ktbh193eo53ofn5m3hvriuv5isk73p47.apps.googleusercontent.com",
            OAuth2ClientSecret = "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",
            OAuth2RefreshToken = "1//04LZiZnpyraTNCgYIARAAGAQSNwF-L9IrLDLpAADns9VFkfS8Yg3DNlNXRhMvVGeJ3YzSlNIzmFwYvlspURVkQ74w37hxZ0y2Tbg",
            //OAuth2AccessToken = "ya29.a0AfB_byASDzAqyq05q5e0Srm8I6GsF_TxkKPMHBDNRL_ZrqO9IFHD-yzcYs9BJkYalbTA1jqE-Muf5Sx_8NqnUHekBBXlOvVV3n7EdovY9KFoYDvtdRHX8rMiEWkLC-PQa3goBkIGpQpuQ_KPBBTkGi47_-051vy2GgaCgYKAewSARESFQGOcNnCL0wmp98WrY_BYqUheq3UQA0169"
            LoginCustomerId = "1026327545"
            //LoginCustomerId = "3692349001"
        };

        public CampaignController()
        {
            // Initialize the Google Ads client.
            _dbContext = new DBcontext();
            client = new GoogleAdsClient(config);
            client.Config.EnableProfiling = true;
        }

        public ActionResult Campaigns()
        {
            GetCampaign(client, 3692349001);
            var data = _dbContext.Campaigns.ToList();
            return View(data);
        }

        public ActionResult CreateCampaigns()
        {
            string[] countrylist = _dbContext.Countries.AsEnumerable().Select(s => s.country_name).Distinct().ToArray();
            ViewBag.CountryList = new SelectList(countrylist);

            ViewBag.LocationList = new SelectList(new List<string>());

            return View();
        }

        public ActionResult Edit(Campaigns item)
        {
            var campaign = _dbContext.Campaigns.FirstOrDefault(x => x.id == item.id);

            item.campaign_name = campaign.campaign_name;
            item.start_date = campaign.start_date;
            item.end_date = campaign.end_date;
            item.budget = campaign.budget;
            item.country = campaign.country;
            item.id = campaign.id;

            return View("CreateCampaigns", campaign);
        }

        public void GetCampaign(GoogleAdsClient client, long customerId)
        {
            // Get the GoogleAdsService.
            GoogleAdsServiceClient googleAdsService = client.GetService(Services.V15.GoogleAdsService);

            // Create a query that will retrieve all campaigns.
            string query1 = @"SELECT
                campaign.id,
                campaign.name,
                campaign.advertising_channel_type,
                campaign.start_date,
                campaign.end_date,
                campaign_budget.amount_micros,
                metrics.ctr,
                metrics.average_cpc,
                metrics.clicks,
                metrics.impressions,
                metrics.cost_micros,
                metrics.conversions,
                campaign.status
            FROM
                campaign
            ORDER BY
                campaign.name";

            try
            {
                // Issue a search request.
                googleAdsService.SearchStream(customerId.ToString(), query1,
                    delegate (SearchGoogleAdsStreamResponse resp)
                    {
                        foreach (GoogleAdsRow googleAdsRow in resp.Results)
                        {
                            var campaignId = googleAdsRow.Campaign.Id.ToString();

                            string country_code = "";
                            string countryname = "";

                            string query2 = $"SELECT " +
                                $"campaign_criterion.campaign, " +
                                $"campaign_criterion.location.geo_target_constant, " +
                                $"campaign_criterion.proximity.address.country_code, " +
                                $"campaign_criterion.proximity.geo_point.longitude_in_micro_degrees, " +
                                $"campaign_criterion.proximity.geo_point.latitude_in_micro_degrees, " +
                                $"campaign_criterion.proximity.radius, " +
                                $"campaign_criterion.negative " +
                                $"FROM campaign_criterion " +
                                $"WHERE " +
                                $"campaign_criterion.campaign = '{googleAdsRow.Campaign.ResourceName}' " +
                                $"AND campaign_criterion.type IN (LOCATION, PROXIMITY)";

                            googleAdsService.SearchStream(customerId.ToString(), query2,
                                delegate (SearchGoogleAdsStreamResponse res)
                                {
                                    foreach (GoogleAdsRow r in res.Results)
                                    {
                                        var loc = r.CampaignCriterion.Location.GeoTargetConstant;
                                        var loc_code = loc.Split('/')[1];
                                        country_code = _dbContext.Locations.FirstOrDefault(x =>
                                            (x.parent_id == loc_code) || (x.criteria_id == loc_code)).country_code;
                                        countryname = _dbContext.Countries.FirstOrDefault(x => x.country_code == country_code).country_name;
                                    }
                                }
                            );

                            var campaigndata = _dbContext.Campaigns.FirstOrDefault(x => x.campaign_id == campaignId);

                            if (campaigndata == null)
                            {
                                var newCampaign = new Campaigns
                                {
                                    campaign_id = googleAdsRow.Campaign.Id.ToString(),
                                    campaign_name = googleAdsRow.Campaign.Name,
                                    campaign_type = googleAdsRow.Campaign.AdvertisingChannelType.ToString(),
                                    start_date = googleAdsRow.Campaign.StartDate,
                                    end_date = googleAdsRow.Campaign.EndDate,
                                    budget = System.Math.Round(googleAdsRow.CampaignBudget.AmountMicros / 1000000f, 2), // Assuming campaign_budget is of type double
                                    ctr = System.Math.Round(googleAdsRow.Metrics.Ctr * 100, 2), // Assuming ctr is of type double
                                    cpc = System.Math.Round(googleAdsRow.Metrics.AverageCpc / 1000000f, 2), // Assuming average_cpc is of type long
                                    clicks = (int)googleAdsRow.Metrics.Clicks, // Assuming clicks is of type long
                                    impressions = (int)googleAdsRow.Metrics.Impressions, // Assuming impressions is of type long
                                    cost = googleAdsRow.Metrics.CostMicros / 1000000f, // Assuming cost_micros is of type long
                                    conversion = (int)googleAdsRow.Metrics.Conversions, // Assuming conversions is of type long
                                    status = googleAdsRow.Campaign.Status.ToString(), // Assuming Status is of type enum
                                    country = countryname
                                };
                                _dbContext.Campaigns.Add(newCampaign);
                                _dbContext.SaveChanges();
                            }
                        }
                    });
            }
            catch (GoogleAdsException e)
            {
                Console.WriteLine("Failure:");
                Console.WriteLine($"Message: {e.Message}");
                Console.WriteLine($"Failure: {e.Failure}");
                Console.WriteLine($"Request ID: {e.RequestId}");
                //throw;
            }
        }

        private static string CreateBudget(GoogleAdsClient client, long customerId, double campaign_budget)
        {
            Console.WriteLine(client);
            Console.WriteLine(customerId);
            Console.WriteLine(campaign_budget);


            // Get the BudgetService.
            CampaignBudgetServiceClient budgetService = client.GetService(Services.V15.CampaignBudgetService);

            // Create the campaign budget.
            CampaignBudget budget = new CampaignBudget()
            {
                Name = "Budget #" + ExampleUtilities.GetRandomString(),
                DeliveryMethod = BudgetDeliveryMethod.Standard,
                AmountMicros = Convert.ToInt64(campaign_budget)
            };

            // Create the operation.
            CampaignBudgetOperation budgetOperation = new CampaignBudgetOperation()
            {
                Create = budget
            };

            try
            {
                MutateCampaignBudgetsResponse response = budgetService.MutateCampaignBudgets(
                    customerId.ToString(), new CampaignBudgetOperation[] { budgetOperation });
                                return response.Results[0].ResourceName;
            }
            catch(GoogleAdsException e)
            {
                Console.WriteLine("Failure:");
                Console.WriteLine($"Message: {e.Message}");
                Console.WriteLine($"Failure: {e.Failure}");
                Console.WriteLine($"Request ID: {e.RequestId}");
                throw;
            }

        }

        public void CreateCampaign(GoogleAdsClient client, long customerId, Campaigns campaigns)
        {
            CampaignServiceClient campaignService = client.GetService(Services.V15.CampaignService);
            CampaignCriterionServiceClient campaignCriterionService = client.GetService(Services.V15.CampaignCriterionService);
            string budget = CreateBudget(client, customerId, campaigns.budget);
            //var locationId = _dbContext.Locations.FirstOrDefault(x => x.id == id);

            List<CampaignOperation> operations = new List<CampaignOperation>();

            // Create the campaign.
            Campaign campaign = new Campaign()
            {
                Name = campaigns.campaign_name,
                AdvertisingChannelType = AdvertisingChannelType.Shopping,
                Status = CampaignStatus.Enabled,
                ManualCpc = new ManualCpc() {
                    EnhancedCpcEnabled = false
                },
                CampaignBudget = budget,
                NetworkSettings = new NetworkSettings
                {
                    TargetGoogleSearch = true,
                    TargetSearchNetwork = true,
                    // Enable Display Expansion on Search campaigns. See
                    // https://support.google.com/google-ads/answer/7193800 to learn more.
                    TargetContentNetwork = false,
                    TargetPartnerSearchNetwork = false,
                    
                },

                ShoppingSetting = new ShoppingSetting
                {
                    CampaignPriority = 0,
                    MerchantId = 136044241,
                    EnableLocal = true
                },

                StartDate = DateTime.Now.AddDays(1).ToString("yyyyMMdd"),
                EndDate = campaigns.end_date.Replace("-", ""),
            };

            operations.Add(new CampaignOperation() { Create = campaign });
            try
            {
                MutateCampaignsResponse retVal = campaignService.MutateCampaigns(
                    customerId.ToString(), operations);

                if (retVal.Results.Count > 0)
                {
                    foreach (MutateCampaignResult newCampaign in retVal.Results)
                    {
                        string campaignResourceName = newCampaign.ResourceName;

                        CampaignCriterion campaignCriterion = new CampaignCriterion()
                        {
                            Campaign = campaignResourceName,
                            Location = new LocationInfo()
                            {
                                GeoTargetConstant = ResourceNames.GeoTargetConstant(2480)
                            }
                        };

                        CampaignCriterionOperation operation = new CampaignCriterionOperation()
                        {
                            Create = campaignCriterion
                        };

                        try
                        {
                            MutateCampaignCriteriaResponse response = campaignCriterionService.MutateCampaignCriteria(
                                customerId.ToString(), new[] { operation });

                            string campaignCriterionResourceName = response.Results[0].ResourceName;
                            Console.WriteLine($"Set campaign location target with resource name '{campaignCriterionResourceName}'.");
                        }
                        catch (GoogleAdsException e)
                        {
                            Console.WriteLine($"Failed to set campaign location target. Exception says \"{e}\"");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No campaigns were added.");
                }
            }
            catch (GoogleAdsException e)
            {
                Console.WriteLine("Failure:");
                Console.WriteLine($"Message: {e.Message}");
                Console.WriteLine($"Failure: {e.Failure}");
                Console.WriteLine($"Request ID: {e.RequestId}");
                throw;
            }

            // Create the location criterion.
            //CampaignCriterion campaignCriterion = new CampaignCriterion()
            //{
            //    Campaign = ResourceNames.Campaign(customerId, campaign.Id),
            //    Location = new LocationInfo()
            //    {
            //        GeoTargetConstant = ResourceNames.GeoTargetConstant(2840),
            //    },
            //};

            //CampaignCriterionOperation operation = new CampaignCriterionOperation()
            //{
            //    Create = campaignCriterion
            //};

            //try
            //{
            //    // Add the campaign criterion.
            //    MutateCampaignCriteriaResponse response =
            //        campaignCriterionService.MutateCampaignCriteria(customerId.ToString(), new[] { operation });

            //    // Display the results.
            //    foreach (MutateCampaignCriterionResult criterionResult in response.Results)
            //    {
            //        Console.WriteLine("Created campaign criterion with resource name '{0}'.",
            //            criterionResult.ResourceName);
            //    }
            //}
            //catch (GoogleAdsException e)
            //{
            //    Console.WriteLine("Failure:");
            //    Console.WriteLine($"Message: {e.Message}");
            //    Console.WriteLine($"Failure: {e.Failure}");
            //    Console.WriteLine($"Request ID: {e.RequestId}");
            //    throw;
            //}
        }

        public void UpdateCampaign(GoogleAdsClient client, long customerId, Campaigns campaign)
        {
            CampaignServiceClient campaignService = client.GetService(Services.V15.CampaignService);
            Campaign campaignToUpdate = new Campaign()
            {
                ResourceName = ResourceNames.Campaign(customerId, long.Parse(campaign.campaign_id)),
                Name = campaign.campaign_name,
                Id = long.Parse(campaign.campaign_id),
                StartDate = campaign.start_date.Replace("-", ""),   
                EndDate = campaign.end_date.Replace("-", "")
                //Status = model.
            };

            CampaignOperation operation = new CampaignOperation()
            {
                Update = campaignToUpdate,
                UpdateMask = FieldMasks.AllSetFieldsOf(campaignToUpdate)
            };
            try
            {
                // Update the campaign
                MutateCampaignsResponse response = campaignService.MutateCampaigns(
                    customerId.ToString(), new[] { operation });

                // Display the results.
                foreach (MutateCampaignResult updatedCampaign in response.Results)
                {
                    Console.WriteLine($"Campaign with resource ID = " +
                        $"'{updatedCampaign.ResourceName}' was updated.");
                }
            }
            catch (GoogleAdsException e)
            {
                Console.WriteLine("Failure:");
                Console.WriteLine($"Message: {e.Message}");
                Console.WriteLine($"Failure: {e.Failure}");
                Console.WriteLine($"Request ID: {e.RequestId}");
                throw;
            }
        }

        [HttpPost]
        public ActionResult Create(Campaigns model)
        {
            Console.WriteLine(ModelState);
            if (ModelState.IsValid)
            {
                if(model.campaign_id == null)
                {
                    CreateCampaign(client, 3692349001, model);
                    ViewBag.Message = "Data Insert Successfully";
                }
                else
                {
                    var campaign = _dbContext.Campaigns.FirstOrDefault(x => x.id == model.id);

                    if (campaign != null)
                    {
                        campaign.campaign_name = model.campaign_name;
                        campaign.start_date = model.start_date;
                        campaign.end_date = model.end_date;
                        campaign.budget = model.budget;
                        campaign.country = model.country;

                        UpdateCampaign(client, 3692349001, campaign);
                        ViewBag.Message = "Data Update Successfully";
                    }
                }
            }
            return RedirectToAction("Campaigns");
        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            var data = _dbContext.Campaigns.FirstOrDefault(x => x.id == id);
            var campaign_id = _dbContext.Campaigns.FirstOrDefault(x => x.id == id).campaign_id;

            CampaignServiceClient campaignService = client.GetService(Services.V15.CampaignService);
            CampaignOperation operation = new CampaignOperation()
            {
                Remove = ResourceNames.Campaign(customerId, long.Parse(campaign_id))
            };

            try
            {
                // Remove the campaign.
                MutateCampaignsResponse retVal = campaignService.MutateCampaigns(
                    customerId.ToString(), new CampaignOperation[] { operation });

                // Display the results.
                foreach (MutateCampaignResult removedCampaign in retVal.Results)
                {
                    Console.WriteLine($"Campaign with resource name = '{0}' was removed.",
                        removedCampaign.ResourceName);
                }
            }
            catch (GoogleAdsException e)
            {
                Console.WriteLine("Failure:");
                Console.WriteLine($"Message: {e.Message}");
                Console.WriteLine($"Failure: {e.Failure}");
                Console.WriteLine($"Request ID: {e.RequestId}");
                throw;
            }

            _dbContext.Campaigns.Remove(data);
            _dbContext.SaveChanges();
            ViewBag.Messsage = "Record Delete Successfully";
            return RedirectToAction("Campaigns");
        }

        [HttpGet]
        public ActionResult Pause(Campaigns item)
        {
            var data = _dbContext.Campaigns.FirstOrDefault(x => x.id == item.id);
            var campaign_id = _dbContext.Campaigns.FirstOrDefault(x => x.id == item.id).campaign_id;

            CampaignServiceClient campaignService = client.GetService(Services.V15.CampaignService);
            Campaign campaignToUpdate = new Campaign()
            {
                ResourceName = ResourceNames.Campaign(customerId, long.Parse(campaign_id)),
                Status = CampaignStatus.Paused
            };

            CampaignOperation operation = new CampaignOperation()
            {
                Update = campaignToUpdate,
                UpdateMask = FieldMasks.AllSetFieldsOf(campaignToUpdate)
            };
            try
            {
                // Update the campaign
                MutateCampaignsResponse response = campaignService.MutateCampaigns(
                    customerId.ToString(), new[] { operation });
            
                // Display the results.
                foreach (MutateCampaignResult updatedCampaign in response.Results)
                {
                    Console.WriteLine($"Campaign with resource ID = " +
                        $"'{updatedCampaign.ResourceName}' was updated.");
                }
            }
            catch (GoogleAdsException e)
            {
                Console.WriteLine("Failure:");
                Console.WriteLine($"Message: {e.Message}");
                Console.WriteLine($"Failure: {e.Failure}");
                Console.WriteLine($"Request ID: {e.RequestId}");
                throw;
            }

            if (data.status == "Enabled")
            {
                data.status = "Paused";
                _dbContext.SaveChanges();
            }

            //_dbContext.Campaigns.Remove(data);
            //_dbContext.SaveChanges();
            //ViewBag.Messsage = "Record Delete Successfully";
            return RedirectToAction("Campaigns");
        }

        

        [HttpGet]
        public ActionResult GetLocations(string srchStr)
        {
            string[] locationlist = _dbContext.Locations.Where(x => x.canonical_name.Contains(srchStr)).AsEnumerable().Select(s => s.canonical_name).Distinct().ToArray();
            ViewBag.LocationList = new SelectList(locationlist);
            return Json(locationlist);
        }
    }
}