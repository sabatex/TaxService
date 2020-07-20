using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ntics.DateTimeExtensions;
using TaxService.Models;

namespace TaxService.Data
{
    public class TaxServiceDbContext:DbContext
    {
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<ConfigRegister>   Configs  { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            var configFilePath = Directory.GetCurrentDirectory() + @"\TaxService.sqlite";
            optionsBuilder.UseSqlite($"Data Source = {configFilePath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        //    modelBuilder.
        //        Entity<Config>().
        //        Property(p => p.SelectedPeriod).
        //        HasConversion(
        //        v => v.ToString(),
        //        v => DateTimePeriod.Parse(v));
        //
        }
    }
}
