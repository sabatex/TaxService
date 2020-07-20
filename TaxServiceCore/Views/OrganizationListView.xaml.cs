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
    /// Interaction logic for OrganizationListView.xaml
    /// </summary>
    public partial class OrganizationListView : Window
    {
        
        public OrganizationListView()
        {
            InitializeComponent();
        }
        OrganizationListViewModel viewModel;
        public OrganizationListViewModel ViewModel
        {
            get => viewModel;
            set
            {
                viewModel = value;
                this.DataContext = viewModel;
            }
        }


        private void CommandBinding_New(object sender, ExecutedRoutedEventArgs e)
        {
            OrganizationView organizationView = new OrganizationView();
            organizationView.Owner = this;
            //organizationView.ViewModel = new OrganizationViewModel() {Organization = new Models.Organization(), Config = viewModel.Config };
            var result = organizationView.ShowDialog();
            if (result == true)
            {

            }
        }

        private void CommandBinding_Open(object sender, ExecutedRoutedEventArgs e)
        {
            var organization = OrganizationsList.SelectedItem as Organization;
            if (organization == null)
                throw new Exception("Помилка вибору організації");

            OrganizationView organizationView = new OrganizationView(organization.Id);
            organizationView.Owner = this;
            organizationView.ShowDialog();

        }

        private void Open_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (OrganizationsList?.SelectedItem == null)
                e.CanExecute = false;
            else
                e.CanExecute = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var model = new OrganizationListViewModel();
            using (var context = new TaxServiceDbContext())
            {
                model.Organizations = new System.Collections.ObjectModel.ObservableCollection<Organization>( context.Organizations.ToList());
            }
            ViewModel = model;

        }
    }
}
