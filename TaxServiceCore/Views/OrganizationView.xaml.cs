using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TaxService.Data;
using TaxService.Models;
using TaxService.ViewModels;

namespace TaxService.Views
{
    /// <summary>
    /// Interaction logic for OrganizationView.xaml
    /// </summary>
    public partial class OrganizationView : Window
    {
        private bool isNew;
        private readonly Guid id;
        public Organization Organization { get; set; }
        public OrganizationView()
        {
            InitializeComponent();
            isNew = true;
            id =  Guid.NewGuid();
        }

        public OrganizationView(Guid id):this()
        {
            this.id = id;
            isNew = false;
        }

        OrganizationViewModel viewModel;
        public OrganizationViewModel ViewModel
        {
            get => viewModel;
            set
            {
                viewModel = value;
                this.DataContext = viewModel;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (isNew)
                Organization = new Organization() { Id = id };
            else
            {
                using (Data.TaxServiceDbContext context = new Data.TaxServiceDbContext())
                {
                    Organization = context.Organizations.Find(id);
                    if (Organization == null)
                    {
                        Organization = new Organization();
                        isNew = true;
                    }
                }
            }
            ViewModel = new OrganizationViewModel() { Organization = Organization };
            DataContext = ViewModel;
        }
        /// <summary>
        /// Save config 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            using (var context = new TaxServiceDbContext())
            {
                if (isNew)
                    context.Organizations.Add(viewModel.Organization);
                else
                    context.Organizations.Update(viewModel.Organization);
                context.SaveChanges();
            }
            DialogResult = true;
            Close();
        }
    }
}
