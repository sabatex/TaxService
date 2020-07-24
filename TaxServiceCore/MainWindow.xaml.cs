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
using System.IO;
using System.Diagnostics;
using sabatex.WPF.Controls.Diagnostics;
using TaxService.Services;
using System.Xml;
using sabatex.TaxUA;
using TaxService.UserControls;

namespace TaxService
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void setVisibleFrame(WindowFrames frame)
        {
            organizationsFrame.Visibility = Visibility.Collapsed;
            organizationFrame.Visibility = Visibility.Collapsed;
            mainWindow.Visibility = Visibility.Collapsed;
            switch(frame)
            {
                case WindowFrames.Main:
                    mainWindow.Visibility = Visibility.Visible;
                    break;
                case WindowFrames.Organization:
                    organizationFrame.Visibility = Visibility.Visible;
                    break;
                case WindowFrames.Organizations:
                    organizationsFrame.Visibility = Visibility.Visible;
                    break;
            }

        }

        private void update()
        {
            setVisibleFrame(WindowFrames.Main);
        }


        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            Trace.Listeners.Add(new TextBoxTraceListener(TextLog));
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            ConfigStore.CurrentConfig.SaveToFile();
        }

        private void ComboBoxWithButton_Click(object sender, RoutedEventArgs e)
        {
            setVisibleFrame(WindowFrames.Organizations);
            organizationsFrame.Update(ConfigStore.CurrentConfig);
        }

        #region organizations logic
        private void organizationsClose(object sender, RoutedEventArgs e)
        {
            update();
        }
        private void organizationsAddNew(object sender, RoutedEventArgs e)
        {
            setVisibleFrame(WindowFrames.Organization);
            organizationFrame.Initialize();
        }
        private void organizationsOpenItem(object sender, RoutedEventArgs e)
        {
            setVisibleFrame(WindowFrames.Organization);
            organizationFrame.Initialize((e as OrganizationRoutedEventArgs).Organization);
        }
        #endregion
        // organization logic
        #region organization logic
        private void organizationClose(object sender, RoutedEventArgs e)
        {
            setVisibleFrame(WindowFrames.Organizations);
        }
        private void organizationOk(object sender, RoutedEventArgs e)
        {
            setVisibleFrame(WindowFrames.Organizations);
            organizationsFrame.Update(ConfigStore.CurrentConfig);
        }
        #endregion

        public bool IsImportBusy { get; set; }
        public bool IsExportBusy { get; set; }
        private async void importFrom1C(object sender, RoutedEventArgs e)
        {
            IsImportBusy = true;
            Trace.WriteLine("Вивантаження податкових накладних з 1С.");
            await Task.Delay(5);
            try
            {
                var taxes = await TaxService.Services.Helpers1C77.GetTAXES(ConfigStore.CurrentConfig.Organization, ConfigStore.CurrentConfig.Organization, ConfigStore.CurrentConfig.SelectedPeriod);
                Trace.WriteLine("Import taxes documents ended!\n");
                Trace.WriteLine("Розпочато збереження податкових в XML");
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                foreach (var d in taxes)
                {
                    DateTime startDate = DateTime.Now;
                    var text = d.GetAsXML();
                    var en = Encoding.GetEncoding(1251);
                    File.WriteAllText(ConfigStore.CurrentConfig.TaxStorePath + "\\" + d.GetXMLFileName(), text, en);
                    TimeSpan time = DateTime.Now.Subtract(startDate);
                    Trace.TraceInformation("Податкова накладна №{0} від {1} збережена як {2} (час виконання - {3})", d.C_DOC_CNT.ToString(), d.HFILL, d.GetXMLFileName(), time.ToString("c"));
                    await Task.Delay(100);
                }
                Trace.WriteLine("Закінчено збереження податкових в XML");
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Спроба імпортувати податкові документи не вдалася по причині " + ex.Message);
            }
            IsImportBusy = false;
            CommandManager.InvalidateRequerySuggested();
        }

        private async void exportTo1C(object sender, RoutedEventArgs e)
        {
            IsExportBusy = true;
            Trace.WriteLine("Завантаження податкових в 1С7.7.");
            await Task.Delay(5);
            List<XmlDocument> documents = new List<XmlDocument>();
            Trace.WriteLine(DateTime.Now.ToString() + "Зчитування податкових з XML файлів.");
            string[] files = Directory.GetFiles(ConfigStore.CurrentConfig.TaxExportStorePath, "*.XML");
            foreach (string FileName in files)
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(FileName);
                    XmlNode DH = doc.SelectSingleNode("/DECLAR/DECLARHEAD");
                    var doc_type = DH["C_DOC"].InnerText;
                    var doc_ver = DH["C_DOC_SUB"].InnerText;
                    if (doc_type != "J12" || doc_ver != "010")
                    {
                        Trace.WriteLine($"Файл {FileName}  з C_DOC={doc_type} C_DOC_SUB={doc_ver} не відповідає підтримуваній версії. ");
                        continue;
                    }

                    XmlNode DB = doc.SelectSingleNode("/DECLAR/DECLARBODY");
                    DateTime HFILL = DB["HFILL"].InnerText.GetTAXDate().Value;
                    if (HFILL < ConfigStore.CurrentConfig.SelectedPeriod.Begin || HFILL > ConfigStore.CurrentConfig.SelectedPeriod.End)
                    {
                        Trace.WriteLine($"Файл {FileName}  з HFILL={HFILL} знаходиться за межами визначеного періоду завантаження. ");
                        continue;
                    }
                    documents.Add(doc);
                }
                catch (Exception ex)
                {
                    Trace.WriteLine($"Спроба відкрити документ {FileName} не вдалася по причині " + ex.Message);
                }
            }

            if (documents.Count != 0)
            {
                await Helpers1C77.PutTaxes(documents, ConfigStore.CurrentConfig.Organization, ConfigStore.CurrentConfig.Organization, ConfigStore.CurrentConfig.SelectedPeriod);

            }
            else
            {
                Trace.WriteLine(DateTime.Now.ToString() + "Відсутні файли податкових які відповідають заданим вимогам.");
            }

            IsExportBusy = false;
            CommandManager.InvalidateRequerySuggested();
        }

        private void CanExecute_ImportTax(object sender, CanExecuteRoutedEventArgs e)
        {
            if (IsImportBusy)
                e.CanExecute = false;
            else
                e.CanExecute = true;

        }
        private void CanExecute_ExportTax(object sender, CanExecuteRoutedEventArgs e)
        {
            if (IsExportBusy)
                e.CanExecute = false;
            else
                e.CanExecute = true;

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.DataContext = ConfigStore.CurrentConfig;
        }
    }


}
