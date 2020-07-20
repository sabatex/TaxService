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
using System.Windows.Navigation;
using System.Windows.Shapes;
using TaxService.Models;
using TaxService.ViewModels;
using System.IO;
using TaxService.Views;
using System.Diagnostics;
using sabatex.WPF.Controls.Diagnostics;
using TaxService.Data;
using Microsoft.EntityFrameworkCore;
using TaxService.Services;

namespace TaxService
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string _configFilePath = Directory.GetCurrentDirectory() + @"\config.json";
        MainWindowViewModel mainWindowViewModel;
        public MainWindow()
        {
           InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            //using (var db = new TaxServiceDbContext())
            //{
            //    db.Database.Migrate();
            //}


            //this.DataContext = new MainWindowViewModel();
            //mainWindowViewModel = new MainWindowViewModel();
            //this.DataContext = mainWindowViewModel;
            Trace.Listeners.Add(new TextBoxTraceListener(TextLog));
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            mainWindowViewModel.SaveState();
        }

        private void ComboBoxWithButton_Click(object sender, RoutedEventArgs e)
        {
            OrganizationListView organizationListView = new OrganizationListView();
            organizationListView.Owner = this;
            organizationListView.ShowDialog();
        }

        public bool IsImportBusy { get; set; }
        private async void CommandBinding_Import(object sender, ExecutedRoutedEventArgs e)
        {
            IsImportBusy = true;
            Trace.WriteLine("Import taxes documents started!");
            await Task.Delay(5);
            try
            {
                var taxes = await TaxService.Services.Helpers1C77.GetTAXES(mainWindowViewModel.Config.Organization, mainWindowViewModel.Config.Organization, mainWindowViewModel.Config.SelectedPeriod);
                Trace.WriteLine("Import taxes documents ended!\n");
                Trace.WriteLine("Розпочато збереження податкових в XML");
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                foreach (var d in taxes)
                {
                    DateTime startDate = DateTime.Now;
                    var text = d.GetAsXML();
                    var en = Encoding.GetEncoding(1251);
                    File.WriteAllText(mainWindowViewModel.Config.TaxStorePath + "\\" + d.GetXMLFileName(), text, en);
                    //using (System.IO.StreamWriter file = new System.IO.StreamWriter(mainWindowViewModel.Config.TaxStorePath + "\\" + d.GetXMLFileName(), false, en))
                    //{
                        
                    //    await file.WriteAsync(text);
                    //}
                   TimeSpan time = DateTime.Now.Subtract(startDate);
                   Trace.TraceInformation("Податкова накладна №{0} від {1} збережена як {2} (час виконання - {3})", d.C_DOC_CNT.ToString(),d.HFILL,d.GetXMLFileName(),time.ToString("c"));
                   await Task.Delay(100); 
                }
                Trace.WriteLine("Закінчено збереження податкових в XML");
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Спроба імпортувати податкові документи не вдалася по причині "+ ex.Message);
            }
            IsImportBusy = false;
            CommandManager.InvalidateRequerySuggested();
        }
        
        private void CommandBinding_Export(object sender, ExecutedRoutedEventArgs e)
        {

        }

        private void CanExecute_ImportTax(object sender, CanExecuteRoutedEventArgs e)
        {
            if (IsImportBusy)
                e.CanExecute = false;
            else
                e.CanExecute = true;

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            mainWindowViewModel = new MainWindowViewModel();
            this.DataContext = mainWindowViewModel;
        }
    }

    public static class Command
    {
        public static readonly RoutedUICommand ImportTax = new RoutedUICommand("Імпорт податкових",nameof(ImportTax),typeof(MainWindow));
        public static readonly RoutedUICommand ExportTax = new RoutedUICommand("Експорт податкових", nameof(ExportTax), typeof(MainWindow));
    }
}
