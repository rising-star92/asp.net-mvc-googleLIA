using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GoogleLIA.Models;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace GoogleLIA.DBContext
{
    public class DBcontext : DbContext
    {
        public DBcontext() : base("name=connectionstring")
        {
        }
        public DbSet<Setting> Settings { get; set; }
        public DbSet<Campaigns> Campaigns { get; set; }
        public DbSet<AdGroup> AdGroups { get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<Countrylist> Countries { get; set; }
    }
}