using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Google.Ads.Gax.Examples;
using Google.Ads.Gax.Util;
using Google.Ads.GoogleAds.Config;
using Google.Ads.GoogleAds.Lib;
using Google.Ads.GoogleAds.V15.Errors;
using Google.Ads.GoogleAds.V15.Services;
using Google.Ads.GoogleAds.V15.Common;
using Google.Ads.GoogleAds.V15.Resources;
using static Google.Ads.GoogleAds.V15.Resources.Campaign.Types;
using static Google.Ads.GoogleAds.V15.Enums.AdvertisingChannelTypeEnum.Types;
using static Google.Ads.GoogleAds.V15.Enums.CampaignStatusEnum.Types;
using static Google.Ads.GoogleAds.V15.Enums.BudgetDeliveryMethodEnum.Types;
using GoogleLIA.Databases;
using GoogleLIA.Models;

namespace GoogleLIA.Services
{
    public interface IGoogleService
    {
        bool GetGoolgeCampaigns();
        GCampaign CreateGoogleCampaign(GCampaign campaign);
        bool UpdateGoogleCampaign(GCampaign campaign);
        bool DeleteGoogleCampaign(string campaignId);
        bool PauseGoogleCampaign(string campaignId);
    }

    public class GoogleService: IGoogleService
    {
        private readonly AdsDBContext _context;
        private readonly GoogleAdsClient _googleAdsClient;

        private readonly long customerId = long.Parse(ConfigurationManager.AppSettings["CustomerId"]);
        private readonly GoogleAdsConfig googleAdsConfig = new GoogleAdsConfig()
        {
            DeveloperToken = ConfigurationManager.AppSettings["DeveloperToken"],
            OAuth2ClientId = ConfigurationManager.AppSettings["OAuth2ClientId"],
            OAuth2ClientSecret = ConfigurationManager.AppSettings["OAuth2ClientSecret"],
            OAuth2RefreshToken = ConfigurationManager.AppSettings["OAuth2RefreshToken"],
            LoginCustomerId = ConfigurationManager.AppSettings["LoginCustomerId"],
            EnableProfiling = true
        };

        public GoogleService(AdsDBContext context)
        {
            _googleAdsClient = new GoogleAdsClient(config: googleAdsConfig);
            _context = context;
        }
    
        public bool GetGoolgeCampaigns()
        {
            bool ret = false;

            GoogleAdsServiceClient googleAdsServiceClient = _googleAdsClient.GetService(Google.Ads.GoogleAds.Services.V15.GoogleAdsService);

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
                googleAdsServiceClient.SearchStream(customerId.ToString(), query1,
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

                            googleAdsServiceClient.SearchStream(customerId.ToString(), query2,
                                delegate (SearchGoogleAdsStreamResponse res)
                                {
                                    foreach (GoogleAdsRow r in res.Results)
                                    {
                                        var loc = r.CampaignCriterion.Location.GeoTargetConstant;
                                        var loc_code = loc.Split('/')[1];
                                        country_code = _context.Locations.FirstOrDefault(x =>
                                            (x.parent_id == loc_code) || (x.criteria_id == loc_code)).country_code;
                                        countryname = _context.Countries.FirstOrDefault(x => x.country_code == country_code).country_name;
                                    }
                                }
                            );

                            var campaigndata = _context.Campaigns.FirstOrDefault(x => x.campaign_id == campaignId);

                            if (campaigndata == null)
                            {
                                var newCampaign = _context.Campaigns.Add(new GCampaign {
                                    campaign_id = googleAdsRow.Campaign.Id.ToString(),
                                    campaign_name = googleAdsRow.Campaign.Name,
                                    campaign_type = googleAdsRow.Campaign.AdvertisingChannelType.ToString(),
                                    start_date = googleAdsRow.Campaign.StartDate,
                                    end_date = googleAdsRow.Campaign.EndDate,
                                    budget = Math.Round(googleAdsRow.CampaignBudget.AmountMicros / 1000000f, 2), // Assuming campaign_budget is of type double
                                    ctr = Math.Round(googleAdsRow.Metrics.Ctr * 100, 2), // Assuming ctr is of type double
                                    cpc = Math.Round(googleAdsRow.Metrics.AverageCpc / 1000000f, 2), // Assuming average_cpc is of type long
                                    clicks = (int)googleAdsRow.Metrics.Clicks, // Assuming clicks is of type long
                                    impressions = (int)googleAdsRow.Metrics.Impressions, // Assuming impressions is of type long
                                    cost = googleAdsRow.Metrics.CostMicros / 1000000f, // Assuming cost_micros is of type long
                                    conversion = (int)googleAdsRow.Metrics.Conversions, // Assuming conversions is of type long
                                    status = googleAdsRow.Campaign.Status.ToString(), // Assuming Status is of type enum
                                    country = countryname
                                });

                                _context.SaveChanges();
                            }
                        }
                    });

                ret = true;
            }
            catch (GoogleAdsException e)
            {
                Console.WriteLine("Failure:");
                Console.WriteLine($"Message: {e.Message}");
                Console.WriteLine($"Failure: {e.Failure}");
                Console.WriteLine($"Request ID: {e.RequestId}");

                ret = false;
            }

            return ret;
        }

        public GCampaign CreateGoogleCampaign(GCampaign campaign)
        {
            CampaignServiceClient campaignService = _googleAdsClient.GetService(Google.Ads.GoogleAds.Services.V15.CampaignService);
            CampaignCriterionServiceClient campaignCriterionService = _googleAdsClient.GetService(Google.Ads.GoogleAds.Services.V15.CampaignCriterionService);

            string budget = CreateBudget(campaign.budget);

            List<CampaignOperation> operations = new List<CampaignOperation>();

            // Create the campaign.
            Campaign item = new Campaign()
            {
                Name = campaign.campaign_name,
                AdvertisingChannelType = AdvertisingChannelType.Shopping,
                Status = CampaignStatus.Enabled,
                ManualCpc = new ManualCpc()
                {
                    EnhancedCpcEnabled = false
                },
                CampaignBudget = budget,
                NetworkSettings = new NetworkSettings
                {
                    TargetGoogleSearch = true,
                    TargetSearchNetwork = true,
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
                EndDate = campaign.end_date.Replace("-", ""),
            };

            operations.Add(new CampaignOperation() { Create = item });
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

            return campaign;
        }

        public bool UpdateGoogleCampaign(GCampaign campaign)
        {
            bool ret = false;

            CampaignServiceClient campaignService = _googleAdsClient.GetService(Google.Ads.GoogleAds.Services.V15.CampaignService);
            Campaign campaignToUpdate = new Campaign()
            {
                ResourceName = ResourceNames.Campaign(customerId, long.Parse(campaign.campaign_id)),
                Name = campaign.campaign_name,
                Id = long.Parse(campaign.campaign_id),
                StartDate = campaign.start_date.Replace("-", ""),
                EndDate = campaign.end_date.Replace("-", "")
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

                ret = true;
            }
            catch (GoogleAdsException e)
            {
                Console.WriteLine("Failure:");
                Console.WriteLine($"Message: {e.Message}");
                Console.WriteLine($"Failure: {e.Failure}");
                Console.WriteLine($"Request ID: {e.RequestId}");
                throw;
            }

            return ret;
        }

        public bool DeleteGoogleCampaign(string campaignId)
        {
            bool ret = false;

            CampaignServiceClient campaignService = _googleAdsClient.GetService(Google.Ads.GoogleAds.Services.V15.CampaignService);
            CampaignOperation operation = new CampaignOperation()
            {
                Remove = ResourceNames.Campaign(customerId, long.Parse(campaignId))
            };

            try
            {
                MutateCampaignsResponse retVal = campaignService.MutateCampaigns(
                    customerId.ToString(), new CampaignOperation[] { operation });

                foreach (MutateCampaignResult removedCampaign in retVal.Results)
                {
                    Console.WriteLine($"Campaign with resource name = '{0}' was removed.",
                        removedCampaign.ResourceName);
                }

                ret = true;
            }
            catch (GoogleAdsException e)
            {
                Console.WriteLine("Failure:");
                Console.WriteLine($"Message: {e.Message}");
                Console.WriteLine($"Failure: {e.Failure}");
                Console.WriteLine($"Request ID: {e.RequestId}");
                throw;
            }

            return ret;
        }

        public bool PauseGoogleCampaign(string campaignId)
        {
            bool ret = false;

            CampaignServiceClient campaignService = _googleAdsClient.GetService(Google.Ads.GoogleAds.Services.V15.CampaignService);
            Campaign campaignToUpdate = new Campaign()
            {
                ResourceName = ResourceNames.Campaign(customerId, long.Parse(campaignId)),
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

                ret = true;
            }
            catch (GoogleAdsException e)
            {
                Console.WriteLine("Failure:");
                Console.WriteLine($"Message: {e.Message}");
                Console.WriteLine($"Failure: {e.Failure}");
                Console.WriteLine($"Request ID: {e.RequestId}");
                throw;
            }

            return ret;
        }

        private string CreateBudget(double campaign_budget)
        {
            CampaignBudgetServiceClient budgetService = _googleAdsClient.GetService(Google.Ads.GoogleAds.Services.V15.CampaignBudgetService);

            CampaignBudget budget = new CampaignBudget()
            {
                Name = "Budget #" + ExampleUtilities.GetRandomString(),
                DeliveryMethod = BudgetDeliveryMethod.Standard,
                AmountMicros = Convert.ToInt64(campaign_budget)
            };

            CampaignBudgetOperation budgetOperation = new CampaignBudgetOperation()
            {
                Create = budget
            };

            try
            {
                MutateCampaignBudgetsResponse response = budgetService.MutateCampaignBudgets(customerId.ToString(), new CampaignBudgetOperation[] { budgetOperation });
                return response.Results[0].ResourceName;
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
    }
}