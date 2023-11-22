using CommandLine;
using Google.Ads.Gax.Examples;
using Google.Ads.GoogleAds;
using Google.Ads.GoogleAds.Lib;
using Google.Ads.GoogleAds.V14.Common;
using Google.Ads.GoogleAds.V14.Errors;
using Google.Ads.GoogleAds.V14.Resources;
using Google.Ads.GoogleAds.V14.Services;
using Google.Ads.GoogleAds.V14.Enums;
using Google.Ads.GoogleAds.Config;
using Google.Api.Gax;
using Google.Ads.Gax.Config;
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Linq;
using System.Web;
using static Google.Ads.GoogleAds.V14.Enums.AdGroupAdStatusEnum.Types;
using static Google.Ads.GoogleAds.V14.Enums.AdGroupStatusEnum.Types;
using static Google.Ads.GoogleAds.V14.Enums.AdGroupTypeEnum.Types;

namespace GoogleLIA.Controllers
{
    public class AdGroupController : Controller
    {
        //private static readonly long customerId = 9894384; // Replace with your customer ID.
        //long campaignId;

        //GoogleAdsConfig config = new GoogleAdsConfig()
        //{
        //    //DeveloperToken = "mNM1JPt50E7kNszwzr50mQ",
        //    //OAuth2Mode = OAuth2Flow.APPLICATION,
        //    //OAuth2ClientId = "740959408131-c20islhvg314i81pa18qim81r53ip55c.apps.googleusercontent.com",
        //    //OAuth2ClientSecret = "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",
        //    //OAuth2RefreshToken = "1//04UB5UgjwL8j3CgYIARAAGAQSNwF-L9Ir5n9MO-na54giYnc44eZHPxX2xfnSgtQUtLHdhCOUBFc9jaK6znSaknMNtcw0Xx3FYkI"
        //};
        //// GET: AdGroup
        //public ActionResult AdGroups()
        //{
        //    return View();
        //}
        //public ActionResult CreateAdGroups()
        //{
        //    // Retrieve campaign names.
        //    //List<(long, string)> campaignList = GetCampaignList();
        //    //SelectList campaignSelectList = new SelectList(campaignList, "Item1", "Item2");
        //    return View();
        //}
        //public List<(long, string)> GetCampaignList()
        //{
        //    GoogleAdsClient googleAdsClient = new GoogleAdsClient(config);

        //    // Get the GoogleAdsService.
        //    GoogleAdsServiceClient googleAdsService = googleAdsClient.GetService(Services.V15.GoogleAdsService);

        //    // Create a query that will retrieve all campaigns.
        //    string query = @"SELECT
        //                    campaign.id,
        //                    campaign.name,
        //                    campaign.network_settings.target_content_network
        //                FROM campaign
        //                ORDER BY campaign.id";

        //    List<(long, string)> campaignList = new List<(long, string)>();

        //    try
        //    {
        //        // Issue a search request.
        //        googleAdsService.SearchStream(customerId.ToString(), query,
        //            delegate (SearchGoogleAdsStreamResponse resp)
        //            {
        //                foreach (GoogleAdsRow googleAdsRow in resp.Results)
        //                {
        //                    Console.WriteLine("Campaign with ID {0} and name '{1}' was found.", googleAdsRow.Campaign.Id, googleAdsRow.Campaign.Name);
        //                    (long, string) campaignInfo = (googleAdsRow.Campaign.Id, googleAdsRow.Campaign.Name);
        //                    campaignList.Add(campaignInfo);
        //                }
        //            }
        //        );

        //        return campaignList;
        //    }
        //    catch (GoogleAdsException e)
        //    {
        //        Console.WriteLine("Failure:");
        //        Console.WriteLine($"Message: {e.Message}");
        //        Console.WriteLine($"Failure: {e.Failure}");
        //        Console.WriteLine($"Request ID: {e.RequestId}");
        //        throw;
        //    }

        //}
        //public void createAdGroup()
        //{
        //    GoogleAdsClient googleAdsClient = new GoogleAdsClient(config);

        //    // Get the AdGroupService.
        //    AdGroupServiceClient adGroupService = googleAdsClient.GetService(Services.V15.AdGroupService);

        //    List<AdGroupOperation> operations = new List<AdGroupOperation>();

        //    // Create the ad group.
        //    AdGroup adGroup = new AdGroup()
        //    {
        //        Name = "Example adGroup",
        //        Status = AdGroupStatusEnum.Types.AdGroupStatus.Enabled,
        //        Campaign = ResourceNames.Campaign(customerId, campaignId),

        //        // Set the ad group bids.
        //        CpcBidMicros = 10000000
        //    };

        //    // Create the operation.
        //    AdGroupOperation operation = new AdGroupOperation()
        //    {
        //        Create = adGroup
        //    };
        //    operations.Add(operation);

        //    try
        //    {
        //        // Create the ad groups.
        //        MutateAdGroupsResponse response = adGroupService.MutateAdGroups(
        //            customerId.ToString(), operations);

        //        // Display the results.
        //        foreach (MutateAdGroupResult newAdGroup in response.Results)
        //        {
        //            Console.WriteLine("Ad group with resource name '{0}' was created.",
        //                newAdGroup.ResourceName);
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
        //}
        //public void getAudience()
        //{
        //    GoogleAdsClient googleAdsClient = new GoogleAdsClient(config);
        //    try
        //    {
        //        GoogleAdsServiceClient googleAdsService = googleAdsClient.GetService(Services.V15.GoogleAdsService);

        //        string query = @"SELECT
        //                            audience.id,
        //                            audience.name,
        //                            audience.description
        //                        FROM
        //                            audience";
        //        PagedEnumerable<SearchGoogleAdsResponse, GoogleAdsRow> response = googleAdsService.Search(customerId: customerId.ToString(), query: query);

        //        foreach (GoogleAdsRow googleAdsRow in response)
        //        {
        //            Audience audience = googleAdsRow.Audience;
        //            Console.WriteLine("Audience ID: {0}", audience.Id);
        //            Console.WriteLine("Audience Name: {0}", audience.Name);
        //            Console.WriteLine("Audience Description: {0}", audience.Description);
        //            Console.WriteLine("");
        //        }
        //    }
        //    catch (GoogleAdsException ex)
        //    {
        //        Console.WriteLine("Google Ads API request failed. Please check your API configuration.");
        //        Console.WriteLine("Error Message: {0}", ex.Message);
        //        Console.WriteLine("Failure: {0}", ex.Failure);
        //        Console.WriteLine("Request ID: {0}", ex.RequestId);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("An error occurred while retrieving audiences. Error Message: {0}", ex.Message);
        //    }
        //}
    }
}