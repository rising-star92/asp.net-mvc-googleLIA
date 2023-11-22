using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GoogleLIA.Models;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace GoogleLIA.Databases
{
    public class AdsDBContext : DbContext
    {
        public AdsDBContext() : base("name=connectionstring")
        {
        }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<GCampaign> Campaigns { get; set; }
        public DbSet<AdGroup> AdGroups { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Country> Countries { get; set; }
    }
}