using ntics;
using ntics.ClassExtensions;
using ntics.DateTimeExtensions;
using sabatex.TaxUA;
using sabatex.V1C77;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;


namespace TaxService.Services
{
    public static class Helpers1C8
    {
        /// <summary>
        /// get all partners with INN Code from reference "Контрагенты"
        /// </summary>
        /// <param name="connection"></param>
        /// <returns></returns>
        static Dictionary<string, string> GetPartners(dynamic connection)
        {
            Dictionary<string, string> Partners = new Dictionary<string, string>();
            Trace.WriteLine(DateTime.Now.ToString() + "Load all partners from 1C8.2");
            dynamic Partners1C = connection.Справочники.Контрагенты.Select();
            while (Partners1C.Next())
            {
                if (Partners1C.IsFolder) continue;
                string CodeINN = Partners1C.ИНН;
                if (CodeINN.Length == 0) continue;
                if (Partners.ContainsKey(CodeINN))
                    Trace.WriteLine(DateTime.Now.ToString() + "Dublicate INN code for " + CodeINN + " " + (string)Partners1C.Description);
                else
                    Partners.Add(CodeINN, Partners1C.Code);
            }
            return Partners;
        }
        /// <summary>
        /// Get dictonary with tax documents 
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public static Dictionary<string, TAXDoccument> GetTAXDoccument1C8(dynamic connection, Period period)
        {
            Trace.WriteLine(DateTime.Now.ToString() + "Load all Inner TAX documents from 1C8");
            Dictionary<string, TAXDoccument> TAXDoccuments = new Dictionary<string, TAXDoccument>();

            dynamic TaxDocs = connection.документы.РегистрацияВходящегоНалоговогоДокумента.Select(period.Begin, period.End);

            while (TaxDocs.Next())
            {
                TAXDoccument td = new TAXDoccument();
                td.Date = TaxDocs.Date;
                td.Number = TaxDocs.Номер;
                td.PartnerINN = TaxDocs.Контрагент.ИНН;
                td.SummDoc = TaxDocs.СуммаДокумента;
                td.VAT = TaxDocs.СуммаНДСДокумента;
                string DocKey = td.PartnerINN + " " + td.Date.ToShortDateString() + " " + td.Number;
                if (TAXDoccuments.ContainsKey(DocKey))
                    Trace.WriteLine("Виявлено повторення документів з реєстрації НДС " + DocKey);
                else
                    TAXDoccuments.Add(DocKey, td);
            }
            return TAXDoccuments;

        }
        public static void PutTAXTo1C8(IOrganization org, string FromDir, Period period)
        {

            List<J1201010> TAXDocs = new List<J1201010>();

            Trace.WriteLine(DateTime.Now.ToString() + "Load TAX documents from XML");
            string[] files = Directory.GetFiles(FromDir, "*.XML");
            XmlDocument doc;
            foreach (string FileName in files)
            {

                //J1201010 tax = (J1201010)TaxDoc.OpenXMLFile(FileName, "J12", "010", org.FirmCodeKODE, period.Begin, period.End, out doc);
                //if (tax != null)
                //{

                //    //J1201010 tax = new J1201010();

                //    //if (!tax.GetFromFile(doc.SelectSingleNode("/DECLAR/DECLARHEAD")))
                //    //{
                //    //    Trace.WriteLine("Error load DECLARHEAD from file " + FileName);
                //    //    continue;
                //    //}

                //    //NTICSTaxLib.J1201009 DB = new NTICSTaxLib.J1201009();
                //    //if (!tax.DECLARBODY.GetFromFile(doc.SelectSingleNode("/DECLAR/DECLARBODY")))
                //    //{
                //    //    Trace.WriteLine("Error load DeclarBody from  file " + FileName);
                //    //    continue;
                //    //}
                //    TAXDocs.Add(tax);
                //}

                //if (TAXDocs.Count != 0)
                //{
                //    Trace.WriteLine(DateTime.Now.ToString() + "Start connection to 1C8!");
                //    Type connector = Type.GetTypeFromProgID("V82.ComConnector");
                //    if (connector == null)
                //        throw new Exception(string.Format("COM Class Object {0} was not created or not registered in the system!", "V82.ComConnector"));

                //    dynamic connectorInstance = Activator.CreateInstance(connector);

                //    if (connectorInstance == null)
                //        throw new Exception(string.Format("Instance of class {0} was not created", "V82.ComConnector"));

                //    //Подключение к базе
                //    //dynamic _connection = connectorInstance.Connect(org.Connections1C.StringConnection);

                //    //Позиционирование Организации
                //    //dynamic Organization = _connection.Справочники.Организации.FindByCode(org.Connections1C.FirmCode);

                //    // Загрузим список контрагентов
                //    //Dictionary<string, string> Partners = GetPartners1C8(_connection);

                //    // Загрузим зарегистрированные налоговые
                //    //Dictionary<string, TAXDoccument> TAXDoccuments = GetTAXDoccument1C8(_connection, period);

                //    // перенесення завантажених документів
                //    //foreach (J1201009 TAXdoc in TAXDocs)
                //    //{
                //    //    // create marker


                //    //}


                //}
                //Trace.WriteLine(DateTime.Now.ToString() + "Look in 1C7.7  VATType 20%");
                //OLE VATType = connection.Global.CreateObject("Справочник.ВидыНДС");
                //if (VATType.Method("FindByDescr", "Платникам ПДВ", 0).ToInt() == 0)
                //{
                //    Trace.WriteLine(DateTime.Now.ToString() + "ERROR NOT Look in 1C7.7  Платникам ПДВ 20%");
                //}

                //XmlNode DH = doc.SelectSingleNode("/DECLAR/DECLARHEAD");
                //if (DH["C_DOC"].InnerText != "J12" || DH["C_DOC_SUB"].InnerText != "010")
                //    continue;
                //XmlNode DB = doc.SelectSingleNode("/DECLAR/DECLARBODY");
                //DateTime HFILL = DB["HFILL"].InnerText.GetTAXDate().Value;
                //if (HFILL < XSD.J12010.StartDate.DateOfBeginDay() || HFILL > XSD.J12010.EndDate.DateOfEndDay())
                //{
                //    continue;
                //}
                //string HKBUY = DB["HKBUY"].InnerText;
                //if (HKBUY != conf.Organization.KODE)
                //{
                //    continue;
                //}


                //string HNUM = DB["HNUM"].InnerText;
                //// номер філії
                //string HNUM2 = DB["HNUM2"].InnerText;
                //if (HNUM2 != "")
                //    HNUM += "//" + HNUM2;
                //string HKSEL = DB["HKSEL"].InnerText;
                //string HNAMESEL = DB["HNAMESEL"].InnerText;
                ////string H02G1S = DB["H02G1S"].InnerText;
                //string R03G11 = DB["R03G11"].InnerText;
                //string R04G11 = DB["R04G11"].InnerText;
                //// check inn firm
                //if (Partners.ContainsKey(HKSEL))
                //{
                //    // check document 
                //    if (TAXDoccuments.ContainsKey(HKSEL + " " + HFILL.ToShortDateString() + " " + HNUM))
                //    {
                //        Trace.WriteLine(DateTime.Now.ToString() + " doc dont add to 1C7.7 " + HKSEL + " " + HNUM + " " + HFILL.ToShortDateString() +
                //                                                " " + R03G11.ToString() + " " + R04G11.ToString());
                //        continue;

                //    }



                //    if (_connection.Справочники.FindByCode(Partners[HKSEL]), 0).ToInt() == 0)
                //    {
                //        Trace.WriteLine(DateTime.Now.ToString() + "ERROR!!! not find INN in 1C7.7 ");
                //        continue;
                //    }



                //    Trace.Write(DateTime.Now.ToString() + " Add doc to 1C7.7 " + HKSEL + " " + HNUM + " " + HFILL.ToShortDateString() +
                //                                                " " + R03G11.ToString() + " " + R04G11.ToString());

                //    OLE FR;

                //    //if (H02G1S == "готівка")
                //    //    FR = connection.Global.EvalExpr("Перечисление.ФормыРасчета.Наличными");
                //    //else if (H02G1S == "чек")
                //    //    FR = connection.Global.EvalExpr("Перечисление.ФормыРасчета.НаличнымиЧерезЭККА");
                //    //else
                //    //    FR = connection.Global.EvalExpr("Перечисление.ФормыРасчета.СРасчетногоСчета");
                //    FR = connection.Global.EvalExpr("Перечисление.ФормыРасчета.СРасчетногоСчета");

                //    OLE VN = connection.Global.EvalExpr("Перечисление.ВидНаловой.ПНЕ");



                //    TaxDocs.Method("New");
                //    TaxDocs.Property("DocDate", HFILL);
                //    TaxDocs.Property("Контрагент", Partners1C77.Method("CurrentItem"));
                //    TaxDocs.Property("Номер", HNUM);
                //    TaxDocs.Property("ВидНДС", VATType.Method("CurrentItem"));
                //    TaxDocs.Property("ФормаРасчета", FR);
                //    TaxDocs.Property("НДС", R03G11);
                //    TaxDocs.Property("Сумма", R04G11);
                //    TaxDocs.Property("ТипПодатковой", 1);
                //    TaxDocs.Property("ДатаВиписки", HFILL);
                //    TaxDocs.Property("ВидНаловой", VN);
                //    TaxDocs.Property("Уточнена", 0);
                //    try
                //    {
                //        TaxDocs.Method("Write");
                //        Trace.WriteLine(" OK");

                //    }
                //    catch (Exception ex)
                //    {
                //        Trace.WriteLine(" ERROR");
                //    }


                //}





            }





        }

        /// <summary>
        /// Gets the type associated with the specified 1C COM server.
        /// </summary>
        /// <returns> null if an error is encountered while loading the System.Type</returns>
        static Type GetTypeCOMServer(string COMServerName)
        {
            Trace.TraceInformation("З'єднання з COM Server {0}!", COMServerName);
            Type connector = Type.GetTypeFromProgID(COMServerName);
            if (connector == null)
            {
                Trace.TraceError("Помилка зєднання з COM Class Object {0} !", COMServerName);
                return null;
            }
            return connector;
        }
        static dynamic GetHandleCOMServer(string COMServerName)
        {
            return GetHandleCOMServer(GetTypeCOMServer(COMServerName),COMServerName);
        }
        static dynamic GetHandleCOMServer(Type connector, string COMServerName)
        {
            if (connector == null) return null;
            dynamic connectorInstance = Activator.CreateInstance(connector);
            if (connectorInstance == null)
            {
                string msg = string.Format("Instance of class {0} was not created", COMServerName);
                Trace.WriteLine(msg, "Error");
                throw new Exception(msg);
            }
            return connectorInstance;
        }
        public static void ReleaseComObject(ref object o)
        {
            if ((o != null))
            {
                try
                {
                    Marshal.ReleaseComObject(o);
                }
                catch
                {
                }
                finally
                {
                    o = null;
                }
            }
        }

        public static dynamic Connect(Connection con)
        {
            if ((con.PlatformType & Connection.Platform1CV7) != 0)
            {
                string error = $"Помилка! Сплутані COM Class V8 with Object {con.PlatformType.ToString()} !";
                Trace.TraceError(error);
                throw new Exception(error);
            }
            string COMServerName = con.PlatformType.ToString() + ".ComConnector";

            Trace.TraceInformation("З'єднання з COM Server {0}!", COMServerName);
            Type V1CType = Type.GetTypeFromProgID(COMServerName);
            if (V1CType == null)
            {
                string error = $"Помилка зєднання з COM Class Object {COMServerName} !";
                Trace.TraceError(error);
                throw new Exception(error);
            }

            dynamic Handle = Activator.CreateInstance(V1CType);
            if (Handle == null)
            {
                V1CType = null;
                string error = $"Instance of class {COMServerName} was not created";
                Trace.TraceError(error);
                throw new Exception(error);
            }

            const string ErrorMsg = "Помилка зєднання з базою !!!";
            StringBuilder stringConnection = new StringBuilder();
            if (con.ServerLocation == EServerLocation.Server)
                stringConnection.Append("srvr=\"" + con.ServerAdress + "\";ref=\"" + con.DataBaseName + "\";");
            else
                stringConnection.Append("file=\"" + con.DataBasePath + "\";");
            stringConnection.Append("Usr=\"" + con.UserName + "\";Pwd=\"" + con.UserPass + "\";");
            if (con.UseLocalKey)
                stringConnection.Append("UseHWLicenses=0;");
            else
                stringConnection.Append("UseHWLicenses=1;");

            //Подключение к базе
            var handle = Handle.Connect(stringConnection);
            ReleaseComObject(ref Handle);
            V1CType = null;
            if (handle == null)
            {
                string error = $"Помилка зєднання з базою {stringConnection}!!!";
                Trace.TraceError(error);
                throw new Exception(error);
            }
            return handle;
        }

        /// <summary>
        /// Выгрузка корректировки к налоговой накладной
        /// </summary>
        /// <param name="_connection"> Соединение с 1С</param>
        /// <param name="Doc">Переданая накладная</param>
        /// <param name="Organization">Организация по которой проводим выгрузку</param>
        public static J1201010 GetJ12010(dynamic _connection, dynamic Doc, dynamic Organization)
        {
            J1201010 dc = new J1201010();
            string DocNumber = Doc.Number;
            //dc.SetDocNum(ulong.Parse(DocNumber.DigitalSubstring().TrimStart('0')));
            //dc.SetDocPeriod((DateTime)Doc.Date, 1);
            //dc.SetOrganization(Organization);



            dc.HFILL = ((DateTime)(Doc.Date)).DateToOPZFormat();

            string s = ((string)(Doc.Number)).Trim();
            s = s.DigitalSubstring().PadLeft(5, '0');
            dc.HNUM = ulong.Parse(s.Substring(s.Length - 5));
            dc.HNUM1 = null;
            dc.HNUM2 = null;
            dc.HNAMESEL = Organization.FULL_NAME;
            dc.HKSEL = Organization.KODE;

            string КодЯзыкаПечать = "uk";
            dynamic СведенияОПродавце = _connection.УправлениеНебольшойФирмойСервер.СведенияОЮрФизЛице(КодЯзыкаПечать, Organization, Doc.Дата);
            dynamic СведенияОПокупателе = _connection.УправлениеНебольшойФирмойСервер.СведенияОЮрФизЛице(КодЯзыкаПечать, Doc.Контрагент, Doc.Дата);

            s = _connection.УправлениеНебольшойФирмойСервер.ОписаниеОрганизации(СведенияОПокупателе, "ПолноеНаименование,", false);
            dc.HNAMEBUY = s.Trim();
            dc.HKBUY = _connection.УправлениеНебольшойФирмойСервер.ОписаниеОрганизации(СведенияОПокупателе, "ИНН,", false);

            dc.HBOS = Organization.Manager;



            // таблична частина запаси
            int Lines = Doc.Запасы.Количество();

            for (int i = 0; i < Lines; i++)
            {
                dynamic СтрокаТаблицы = Doc.Запасы.Get(i);
                J1201010T1 t1 = new J1201010T1();
                t1.RXXXXG3S = СтрокаТаблицы.Номенклатура.НаименованиеПолное; //ТМЦ
                // если запас  
                t1.RXXXXG4 = СтрокаТаблицы.КодУКТВЭД.Код;
                // услуга 
                //t1.RXXXXG33 = СтрокаТаблицы.КодУКТВЭД.Код;

                uint? DGchk = new uint?();
                string DGDKPP = null;

                t1.RXXXXG4S = СтрокаТаблицы.ЕдиницаИзмерения.Наименование;
                t1.RXXXXG105_2S = СтрокаТаблицы.ЕдиницаИзмерения.Code;
                t1.RXXXXG5 = (decimal)СтрокаТаблицы.Количество;

                bool СуммаВключаетНДС = Doc.СуммаВключаетНДС;
                decimal Price = (decimal)(СтрокаТаблицы.Цена);
                t1.RXXXXG6 = decimal.Round(СуммаВключаетНДС ? Price / 1.2m : Price, 2);
                t1.RXXXXG008 = EVAT.Default;
                t1.RXXXXG009 = null;
                t1.RXXXXG010 = (decimal)(СуммаВключаетНДС ? СтрокаТаблицы.Сумма - СтрокаТаблицы.СуммаНДС : СтрокаТаблицы.Сумма);
                t1.RXXXXG11_10 = (decimal)СтрокаТаблицы.НДС;
                dc.T1.Add(t1);
            }

            return dc;
        }

        /// <summary>
        /// Выгрузка корректировки к налоговой накладной
        /// </summary>
        /// <param name="_connection"> Соединение с 1С</param>
        /// <param name="Doc">Переданая накладная</param>
        /// <param name="Organization">Организация по которой проводим выгрузку</param>
        public static J1201010[] GetDocs(DateTime begin, DateTime end, Connection con, IOrganization org)
        {
            dynamic handle = Connect(con);
            
            //Позиционирование Организации
            dynamic Organization = handle.Справочники.Организации.FindByCode(org.FirmCode);

            dynamic Doc = handle.Документы.НалоговаяНакладная.Select(begin.BeginOfDay(), end.EndOfDay());
            var result = new List<J1201010>();

            while (Doc.Next())
            {
                if (Doc.Posted & (Doc.Организация.Код == Organization.Код))
                {
                    result.Add(GetJ12010(handle, Doc, org));
                }
            }
            GC.Collect();
            return result.ToArray();
        }

    }
}
