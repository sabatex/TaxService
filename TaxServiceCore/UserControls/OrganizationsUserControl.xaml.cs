using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TaxService.Models;
using TaxService.Services;

namespace TaxService.UserControls
{

    public class OrganizationRoutedEventArgs:RoutedEventArgs
    {
        public Organization Organization { get; set; }
    }

    /// <summary>
    /// Interaction logic for OrganizationsUserControl.xaml
    /// </summary>
    public partial class OrganizationsUserControl : UserControl
    {
        public ObservableCollection<Organization> Organizations { get; set; } = new ObservableCollection<Organization>();
        public Guid Id { get; set; }
        public OrganizationsUserControl()
        {
            InitializeComponent();
            
        }

        public static readonly RoutedEvent NewOrgenizationClickEvent = EventManager.RegisterRoutedEvent(nameof(NewOrgenizationClick), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(OrganizationsUserControl));
        public event System.Windows.RoutedEventHandler NewOrgenizationClick
        {
            add { AddHandler(NewOrgenizationClickEvent, value); }
            remove { RemoveHandler(NewOrgenizationClickEvent, value); }
        }
        private void addNewClick(object sender, RoutedEventArgs e)
        {
            RoutedEventArgs args = new RoutedEventArgs();
            args.RoutedEvent = NewOrgenizationClickEvent;
            RaiseEvent(args);
        }

        public static readonly RoutedEvent OpenOrgenizationClickEvent = EventManager.RegisterRoutedEvent(nameof(OpenOrgenizationClick), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(OrganizationsUserControl));
        public event System.Windows.RoutedEventHandler OpenOrgenizationClick
        {
            add { AddHandler(OpenOrgenizationClickEvent, value); }
            remove { RemoveHandler(OpenOrgenizationClickEvent, value); }
        }

        private void openItemClick(object sender, RoutedEventArgs e)
        {
            var organization = organizationsList.SelectedItem as Organization;
            if (organization == null)
                throw new Exception("Помилка вибору організації");
            Id = organization.Id;
            RoutedEventArgs args = new OrganizationRoutedEventArgs() { Organization = organization };
            args.RoutedEvent = OpenOrgenizationClickEvent;
            RaiseEvent(args);
        }

        public void Update(Config config)
        {
            Organizations.Clear();
            foreach (var item in config.Organizations) Organizations.Add(item);
        }

        private void deleteClick(object sender, RoutedEventArgs e)
        {
            var organization = organizationsList.SelectedItem as Organization;
            if (organization == null)
                throw new Exception("Помилка вибору організації");
            ConfigStore.CurrentConfig.Organizations.Remove(organization);
            Update(ConfigStore.CurrentConfig);
        }

        public static readonly RoutedEvent CloseOrgenizationClickEvent = EventManager.RegisterRoutedEvent(nameof(CloseOrgenizationClick), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(OrganizationsUserControl));
        public event System.Windows.RoutedEventHandler CloseOrgenizationClick
        {
            add { AddHandler(CloseOrgenizationClickEvent, value); }
            remove { RemoveHandler(CloseOrgenizationClickEvent, value); }
        }
        private void closeClick(object sender, RoutedEventArgs e)
        {
            RoutedEventArgs args = new RoutedEventArgs();
            args.RoutedEvent = CloseOrgenizationClickEvent;
            RaiseEvent(args);
        }



    }
}
