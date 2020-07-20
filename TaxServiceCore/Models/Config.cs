using ntics;
using ntics.DateTimeExtensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TaxService.Models
{
    public class Config : ObservableObject
    {
        //public static Config LocalConfig;
        //static string configFilePath;

        //static Config()
        //{
        //    configFilePath = Directory.GetCurrentDirectory() + @"\config.json";
        //    LocalConfig = Load();

        //}


        //public static Config Load()
        //{
        //    if (File.Exists(configFilePath))
        //    {
        //        var result = System.Text.Json.JsonSerializer.Deserialize<Config>(File.ReadAllText(configFilePath));
        //        if (result.Organization == null)
        //        {
        //            if (result.Organizations.Count == 0)
        //            {
        //                Organization org = new Organization();
        //                result.Organizations.Add(org);
        //                result.Organization = org;
        //            }
        //            else
        //            {
        //                result.Organization = result.Organizations[0];
        //            }
        //        }
        //        else
        //        {
        //            if (result.Organizations.Count == 0)
        //            {
        //                result.Organizations.Add(result.Organization);
        //            }
        //        }
        //        return result;
        //    }
        //    else
        //    {
        //        var result = new Config();
        //        Organization org = new Organization();
        //        result.Organizations.Add(org);
        //        result.Organization = org;
        //        return result;
        //    }
        //}

        //public static void Save()
        //{
        //    File.WriteAllText(configFilePath, System.Text.Json.JsonSerializer.Serialize(LocalConfig));
        //}


        public Config()
        {
            TaxStorePath = @"C:\temp";
            SelectedPeriod = new Period();
            //Organizations = new List<Organization>();
        }
        //public Guid Id { get; set; }
        public string TaxStorePath { get; set; }
        public Period SelectedPeriod { get; set; }
        //public List<Organization> Organizations { get; set; }

        public Guid OrganizationId { get; set; }

        public Organization Organization { get; set; }

        //private Organization organization;
        //public Organization Organization
        //{
        //    get => organization;
        //    set => SetProperty(ref organization, value);
        //}

        //public Guid CurrentOrganizationId {get;set;}

    }
}
