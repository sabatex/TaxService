using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ntics;
using ntics.DateTimeExtensions;
using TaxService.Data;
using TaxService.Models;
using TaxService.Services;

namespace TaxService.ViewModels
{
    public class MainWindowViewModel:ObservableObject
    {
        //TaxServiceDbContext context ;
        //string _configFilePath = Directory.GetCurrentDirectory() + @"\config.json";
        string _configFilePath = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\ntics/TaxService/config.json";
        public MainWindowViewModel()
        {
            using (var context = new TaxServiceDbContext())
            {
                Organizations = context.Organizations.ToList();
                var config = context.Configs.Find("MainConfig");
                if (config == null)
                {
                    Config = new Config();
                    Config.Organization = context.Organizations.FirstOrDefault();
                    var s = System.Text.Json.JsonSerializer.Serialize(Config);
                    context.Configs.Add(new ConfigRegister()
                    {
                        Id = "MainConfig",
                        Value = s
                    });
                    context.SaveChanges();
                }
                else
                {
                    Config = System.Text.Json.JsonSerializer.Deserialize<Config>(config.Value);
                    if (Config.Organization == null)
                    {
                        Config.Organization = context.Organizations.FirstOrDefault();
                    }
                    else
                    {
                        Config.Organization = Organizations.SingleOrDefault(f => f.Id == Config.Organization.Id);
                        if (Config.Organization == null)
                        {
                            // fix error
                            Config.Organization = Organizations.FirstOrDefault();
                        }
                    }
                }
            }
        }

        public List<Organization> Organizations { get; set; }

        public Period SelectedPeriod { 
            get=>ConfigStore.SelectedPeriod;
            set=>ConfigStore.SelectedPeriod = value; }

        public string TaxStorePath { get=>ConfigStore.TaxStorePath; set=>ConfigStore.TaxStorePath = value; }

        public void SaveState() => SaveState("MainConfig");
            

        public void SaveState(string configName)
        {
            var serializedConfig = System.Text.Json.JsonSerializer.Serialize(Config);
            using (var context = new TaxServiceDbContext())
            {
                var config = context.Configs.Find(configName);
                
                if (config == null)
                {
                    // create new config
                    context.Configs.Add(new ConfigRegister()
                    {
                        Id = configName,
                        Value = serializedConfig
                    });
                }
                else
                {
                    config.Value = serializedConfig;
                }
                context.SaveChanges();
            }
       }


        Config config;
        public Config Config {
            get=>config;
            set=>SetProperty(ref config,value);
        }
        


        //public override string TaxStorePath
        //{
        //    get => base.TaxStorePath;
        //    set {
        //        if (base.TaxStorePath != value)
        //        {
        //            base.TaxStorePath = value;
        //            OnPropertyChanged();
        //            //if (PropertyChanged != null)
        //            //    PropertyChanged(this, new PropertyChangedEventArgs(nameof(TaxStorePath)));

        //        };
        //    }
        //}
        


        //public override DateTimePeriod SelectedPeriod
        //{
        //    get => base.SelectedPeriod;
        //    set
        //    {
        //        base.SelectedPeriod = value;
        //        OnPropertyChanged();
        //    }
        //}
    }
}
