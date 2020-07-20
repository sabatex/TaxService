using sabatex.Extensions;
using sabatex.Extensions.ClassExtensions;
using sabatex.V1C77;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using TaxService.Models;

namespace TaxService.ViewModels
{
    public class OrganizationViewModel:ObservableObject
    {
        public OrganizationViewModel()
        {
            PlatformSource = EnumExtensions.GetEnumListWithDescription(typeof(EPlatform1C));
            BaseTypeSource = EnumExtensions.GetEnumListWithDescription(typeof(EServerLocation));
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


        public IEnumerable<Tuple<Enum,string>> PlatformSource { get; private set; }
        public IEnumerable<Tuple<Enum,string>> ConfigSourсe { get; set; }
        public IEnumerable<Tuple<Enum,string>> BaseTypeSource { get; set; }

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
            if ((PlatformType & Connection.Platform1CV7) != 0)
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
                          .Where(s=>(((E1CConfigType)s.Item1 & Connection.ConfigType1C77) != 0) == oldIs1CV7).ToArray();
        }

        public string LabelShortName { get => "Short name"; }

        Visibility visibilityPlatform1C8;
        Visibility visibilityServer1C8;
        Visibility visibilityFilePath;
    }
}
