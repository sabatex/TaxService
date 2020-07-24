using sabatex.Extensions;
using sabatex.Extensions.DateTimeExtensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;

namespace TaxService.Models
{
    public class Config : ObservableObject
    {

        public Config()
        {
            TaxStorePath = @"C:\temp\Ixport";
            TaxExportStorePath = @"C:\temp\Export";
            SelectedPeriod = new Period();
            Organizations = new List<Organization>();

        }
        public string TaxStorePath { get; set; }
        public string TaxExportStorePath { get; set; }
        public Period SelectedPeriod { get; set; }
        public Guid? OrganizationId { get; set; }
        
        [JsonIgnore]
        public Organization Organization 
        {
            get=>Organizations.SingleOrDefault(s=>s.Id==OrganizationId);
            set
            {
                OrganizationId = value?.Id;
                OnPropertyChanged(nameof(Organization));
            }
        }
        public List<Organization> Organizations { get; set; }

    }
}
