using sabatex.Extensions;
using sabatex.Extensions.ClassExtensions;
using sabatex.V1C77;
using System;
using System.Collections.Generic;
using System.Linq;
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

    public class OrganizationViewModel : ObservableObject
    {
        public OrganizationViewModel()
        {
            PlatformSource = EnumExtensions.GetEnumListWithDescription(typeof(EPlatform1C));
            BaseTypeSource = EnumExtensions.GetEnumListWithDescription(typeof(EServerLocation));
            _organization = new Organization();
        }

        Organization _organization;
        public Organization Organization
        {
            get => _organization;
            set
            {
                SetProperty(ref _organization, value);
                updateConfigType();
                updateVisibility();
                OnPropertyChanged(nameof(PlatformType));
                OnPropertyChanged(nameof(ServerLocation));
            }
        }

        Config config;
        public Config Config { get => config; set => SetProperty(ref config, value); }


        public IEnumerable<Tuple<Enum, string>> PlatformSource { get; private set; }
        IEnumerable<Tuple<Enum, string>> configSourсe;
        public IEnumerable<Tuple<Enum, string>> ConfigSourсe {get=>configSourсe;set=>SetProperty(ref configSourсe,value); }
        public IEnumerable<Tuple<Enum, string>> BaseTypeSource { get; set; }

        public EPlatform1C PlatformType
        {
            get => Organization.PlatformType;
            set
            {
                bool oldIs1CV7 = (Organization.PlatformType & Connection.Platform1CV7) != 0;
                bool newIs1CV7 = (value & Connection.Platform1CV7) != 0;
                Organization.PlatformType = value;
                if (oldIs1CV7 != newIs1CV7)
                {
                    updateConfigType();
                    if (newIs1CV7)
                    {
                        Organization.ConfigType = E1CConfigType.Buch;
                    }
                    else
                    {
                        Organization.ConfigType = E1CConfigType.UTP;
                    }
                    updateVisibility();
                    OnPropertyChanged(nameof(ConfigSourсe));
                    OnPropertyChanged(nameof(PlatformType));
                }
            }
        }

        public E1CConfigType ConfigType
        {
            get => Organization.ConfigType;
            set
            {
                Organization.ConfigType = value;
                OnPropertyChanged(nameof(ConfigType));
            }
        }

        public EServerLocation ServerLocation
        {
            get => Organization.ServerLocation;
            set
            {
                Organization.ServerLocation = value;
                updateVisibility();
                OnPropertyChanged(nameof(ServerLocation));
            }
        }

        public Visibility VisibilityPlatform1C8
        {
            get => visibilityPlatform1C8;
            set
            {
                SetProperty(ref visibilityPlatform1C8, value);
            }
        }
        public Visibility VisibilityServer1C8
        {
            get => visibilityServer1C8;
            set => SetProperty(ref visibilityServer1C8, value);
        }
        public Visibility VisibilityFilePath
        {
            get => visibilityFilePath;
            internal set => SetProperty(ref visibilityFilePath, value);
        }


        void updateVisibility()
        {
            if ((PlatformType & Organization.Platform1CV7) != 0)
            {
                VisibilityFilePath = Visibility.Visible;
                VisibilityPlatform1C8 = Visibility.Collapsed;
                VisibilityServer1C8 = Visibility.Collapsed;
            }
            else
            {
                VisibilityPlatform1C8 = Visibility.Visible;
                if (ServerLocation == EServerLocation.Server)
                {
                    VisibilityServer1C8 = Visibility.Visible;
                    VisibilityFilePath = Visibility.Collapsed;
                }
                else
                {
                    VisibilityServer1C8 = Visibility.Collapsed;
                    VisibilityFilePath = Visibility.Visible;
                }
            }

        }

        void updateConfigType()
        {
            bool oldIs1CV7 = (PlatformType & Connection.Platform1CV7) != 0;
            ConfigSourсe = EnumExtensions.GetEnumListWithDescription(typeof(E1CConfigType))
                          .Where(s => (((E1CConfigType)s.Item1 & Connection.ConfigType1C77) != 0) == oldIs1CV7).ToArray();
        }

        public string LabelShortName { get => "Short name"; }

        Visibility visibilityPlatform1C8;
        Visibility visibilityServer1C8;
        Visibility visibilityFilePath;
    }

    /// <summary>
    /// Interaction logic for OrganizationUserControl.xaml
    /// </summary>
    public partial class OrganizationUserControl : UserControl
    {
        public OrganizationUserControl()
        {
            InitializeComponent();
            viewModel = new OrganizationViewModel();
            this.DataContext = viewModel;
        }
        OrganizationViewModel viewModel;
        private bool isNew;
        //public Organization Organization { get; set; }

        public void Initialize(Organization organization = null)
        {
            if (organization == null)
            {
                viewModel.Organization = new Organization();
                isNew = true;
            }
            else
            {
                viewModel.Organization = organization;
                isNew = false;
            }
        }


        public static readonly RoutedEvent OkClickEvent = EventManager.RegisterRoutedEvent(nameof(OkClick), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(OrganizationUserControl));
        public event System.Windows.RoutedEventHandler OkClick
        {
            add { AddHandler(OkClickEvent, value); }
            remove { RemoveHandler(OkClickEvent, value); }
        }
        private void okClick(object sender, RoutedEventArgs e)
        {
            if (isNew)
            {
                ConfigStore.CurrentConfig.Organizations.Add(viewModel.Organization);
                ConfigStore.CurrentConfig.OrganizationId = viewModel.Organization.Id;
            }
            ConfigStore.CurrentConfig.SaveToFile();
            RoutedEventArgs args = new RoutedEventArgs();
            args.RoutedEvent = OkClickEvent;
            RaiseEvent(args);
        }


        public static readonly RoutedEvent CancelClickEvent = EventManager.RegisterRoutedEvent(nameof(CancelClick), RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(OrganizationUserControl));
        public event System.Windows.RoutedEventHandler CancelClick
        {
            add { AddHandler(CancelClickEvent, value); }
            remove { RemoveHandler(CancelClickEvent, value); }
        }

        private void cancelClick(object sender, RoutedEventArgs e)
        {
            RoutedEventArgs args = new RoutedEventArgs();
            args.RoutedEvent = CancelClickEvent;
            RaiseEvent(args);
        }

    }
}
