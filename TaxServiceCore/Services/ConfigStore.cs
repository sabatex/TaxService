using sabatex.Extensions.DateTimeExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using TaxService.Data;
using TaxService.Models;

namespace TaxService.Services
{
    public static class ConfigStore
    {
        static string defaultConfigFilePath = Directory.GetCurrentDirectory() + @"\config.json";
        public static void SaveToFile(this Config config)
        {
            SaveToFile(config, defaultConfigFilePath);
        }

        public static void SaveToFile(this Config config, string filePath)
        {
            File.WriteAllText(filePath, System.Text.Json.JsonSerializer.Serialize(config));
        }

        public static Config LoadConfigFromFile(string filePath)
        {
            if (File.Exists(filePath))
                return System.Text.Json.JsonSerializer.Deserialize<Config>(File.ReadAllText(filePath));
            else
                return null;
        }

        public static Config LoadConfigFromFile()
        {
            return LoadConfigFromFile(defaultConfigFilePath);
        }

        public static Period SelectedPeriod
        {
            get => GetValueFromStore<Period>();
            set => SetValueToStore(value); 
            //{
            //    using (var context = new TaxServiceDbContext())
            //    {
            //        var str = context.Configs.SingleOrDefault(s => s.Id == nameof(SelectedPeriod));
            //        if (str == null)
            //            return new DateTimePeriod();
            //        else
            //        {
            //            return DateTimePeriod.Parse(str.Value) ?? new DateTimePeriod();
            //        }
            //    }
            //}
            //set
            //{
            //    using (var context = new TaxServiceDbContext())
            //    {
            //        var str = context.Configs.SingleOrDefault(s => s.Id == nameof(SelectedPeriod));
            //        if (str == null)
            //        {
            //            str = new ConfigRegister
            //            {
            //                Id = nameof(SelectedPeriod),
            //                Value = value.ToString()
            //            };
            //            context.Configs.Add(str);
            //        }
            //        else
            //            str.Value = value.ToString();
            //        context.SaveChanges();
            //    }
            //}
        }

        public static Guid SelectedOrganization
        {
            get => GetValueFromStore<Guid>();
            set => SetValueToStore(value);
        }

        public static string TaxStorePath { get => GetValueFromStore<string>(); set => SetValueToStore(value); }

        public static T GetValueFromStore<T>([CallerMemberName]string propertyName = "")
        {
            using (var context = new TaxServiceDbContext())
            {
                var r = context.Configs.SingleOrDefault(s => s.Id == propertyName);
                if (r != null)
                {
                    var converter = TypeDescriptor.GetConverter(typeof(T));
                    if (converter.CanConvertFrom(typeof(string)))
                    {
                        return (T)converter.ConvertFrom(r.Value);
                    }
                    throw new InvalidCastException($"{r.Value} to {propertyName}");
                }
                return default(T);
            }
        }

        public static void SetValueToStore<T>(T value, [CallerMemberName]string propertyName = "")
        {
            using (var context = new TaxServiceDbContext())
            {
                var row = context.Configs.SingleOrDefault(s => s.Id == propertyName);
                if (row == null)
                {
                    row = new ConfigRegister
                    {
                        Id = propertyName,
                        Value = value.ToString()
                    };
                    context.Configs.Add(row);
                }
                else
                    row.Value = value.ToString();

                context.SaveChanges();
            }

        }

    }
}
