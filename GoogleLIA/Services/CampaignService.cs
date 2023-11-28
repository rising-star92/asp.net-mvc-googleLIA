using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using GoogleLIA.Databases;
using GoogleLIA.Models;

namespace GoogleLIA.Services
{
    public interface ICampaignService
    {
        List<GCampaign> GetCampaignsList();
        GCampaign GetCampaign(int id);
        GCampaign UpdateCampaign(int id, GCampaign item);
        bool DeleteCampaign(int id);
    }

    public class CampaignService : ICampaignService
    {
        private readonly AdsDBContext _context;

        public CampaignService(AdsDBContext context)
        {
            _context = context;
        }

        public List<GCampaign> GetCampaignsList()
        {
            var ret = _context.Campaigns
                .AsNoTracking()
                .ToList();

            return ret;
        }

        public GCampaign GetCampaign(int id)
        {
            var ret = _context.Campaigns
                .AsNoTracking()
                .First(x => x.id == id);

            return ret;
        }

        public GCampaign UpdateCampaign(int id, GCampaign item)
        {
            var _campaign = _context.Campaigns.FirstOrDefault(x => x.id == id);

            if (_campaign != null)
            {
                _campaign.campaign_name = item.campaign_name;
                _campaign.start_date = item.start_date;
                _campaign.end_date = item.end_date;
                _campaign.budget = item.budget;
                _campaign.location = item.location;
                _campaign.status = item.status;
            }

            _context.Entry(_campaign).State = EntityState.Modified;

            _context.SaveChanges();

            return _campaign;
        }

        public bool DeleteCampaign(int id)
        {
            bool ret = false;
            var _campaign = _context.Campaigns.FirstOrDefault(x => x.id == id);

            if (_campaign != null)
            {
                _context.Campaigns.Remove(_campaign);
                _context.SaveChanges();
                ret = true;
            }
            else
            {
                ret = false;
            }

            return ret;
        }


    }
}