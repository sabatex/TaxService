using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaxService.Models;

namespace TaxService.ViewModels
{
    public class OrganizationListViewModel
    {
        public Models.Config Config { get; set; }

        public ObservableCollection<Organization> Organizations { get; set; }
    }
}
