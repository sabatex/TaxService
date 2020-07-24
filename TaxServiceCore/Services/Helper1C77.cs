using sabatex.Extensions.ClassExtensions;
using sabatex.Extensions.DateTimeExtensions;
using sabatex.TaxUA;
using sabatex.TaxUA.CommonTypes;
using sabatex.V1C77;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TaxService.Models;

namespace TaxService.Services
{
    public static class Helpers1C77
    {
        /// <summary>
        /// Get tax documment from 1C 7.7
        /// </summary>
        /// <param name="global">handle to global module 1C 7.7</param>
        /// <param name="Doc">Handle to tax document in 1C 7.7</param>
        /// <param name="ConfigType">Configuration 1C 7.7 enum</param>
        /// <param name="org">Organization reference</param>
        public static J1201010 GetFrom1C77(GlobalObject1C77 global, COMObject doc, IOrganization org)
        {
            J1201010 result = org.IS_NP?new J1201010():new F1201010();
            // шапка налоговой
            result.TIN = Get1C77_HKSEL(doc,org);
            //result.C_DOC
            //result.C_DOC_SUB
            //result.C_DOC_VER
            result.C_DOC_TYPE = 0;
            result.C_REG = org.C_REG;
            result.C_RAJ = org.C_RAJ;
            var str = doc.GetPropertyString("DocNum");
            try
            {    
                var str1 = (new string(str.Where(s => Char.IsDigit(s)).ToArray())).TrimStart('0');
                result.C_DOC_CNT = uint.Parse(str1);
            }
            catch
            {
                string error = $"Error parse tax number {str} as uint";
                Trace.TraceError(error);
                throw new Exception(error);
            }
            result.PERIOD_TYPE = 1;
            DateTime date = doc.GetPropertyDateTime("DocDate");
            result.PERIOD_YEAR = date.Year;
            result.PERIOD_MONTH = date.Month;
            result.D_FILL = DateTime.Now;
            

            result.R01G1 = new int?();
            result.R03G10S = Get1C77_R03G10S(doc, org);
            result.HORIG1 = new int?();
            result.HTYPR = new DGPNtypr?();
            result.HKSEL = Get1C77_HKSEL(doc, org);
            result.HNAMESEL = Get1C77_HNAMESEL(doc, org);
            result.HTINSEL = Get1C77_TINSEL(doc, org);
            result.HNUM2 = org.FilialNumber == ""?null: org.FilialNumber;
            result.HFILL = date.DateToOPZFormat();
            result.HNUM = result.C_DOC_CNT;
            result.HNUM1 = new ulong?();
            result.HNAMEBUY = Get1C77_HNAMEBUY(doc, org);
            result.HKBUY = Get1C77_HKBUY(doc, org);
            result.HFBUY = Get1C77_HFBUY(doc, org);
            result.HTINBUY = Get1C77_HTINBUY(doc, org);
           
            result.HBOS = org.Manager;
            result.HKBOS = org.ManagerIPN;

            if (doc.MethodDouble("SelectLines") == 1)
            {
                while (doc.MethodDouble("GetLine") == 1)
                {
                    //DateTime DataDoc = Doc.Property("ДатаДок").ToDateTime();
                    //if (OLE.IsEmtyValue(Doc.Property("ТМЦ"))) continue;
                    J1201010T1 t1 = new J1201010T1();


                    t1.RXXXXG3S = Get1C77_TmcDescription(doc, org); //ТМЦ
                    if (doc.Is1C77_usluga(org))
                    {
                        t1.RXXXXG33 = doc.Get1C77_UKTZED(org);
                    }
                    else
                    {
                        t1.RXXXXG4 = doc.Get1C77_UKTZED(org);
                    }

                    t1.RXXXXG4S = doc.GetPropertyObject("Ед").GetPropertyString("Наименование"); // UnitOfMeasure
                    t1.RXXXXG105_2S = Get1C77_UnitCode(doc, org);

                    t1.RXXXXG5 = Get1C77_quantity(doc, org);

                    t1.RXXXXG6 = Get1C77_CostWithoutVAT(doc, org);

                    t1.RXXXXG008 = Get1C77_StavkaNDS(doc, org);
                    t1.RXXXXG009 = Get1C77_PilgaNDS(doc, org);
                    t1.RXXXXG010 = Get1C77_SumWithoutVAT(doc, org);
                    t1.RXXXXG11_10 = new decimal?( (decimal)doc.Get1C77_VAT(org));
                    result.T1.Add(t1);
               }
            }
            return result;
        }
        //public static bool CheckDoc(Object1C77 Doc, E1CConfigType ConfigType, IOrganization org)
        //{
        //    // check transacted
        //    if (Doc.Method("IsTransacted") == 0) return false;

        //    // check firm selected
        //    if (ConfigType != E1CConfigType.Uaservice && ConfigType != E1CConfigType.Inforce)
        //    {
        //        Object1C77 firm = null;
        //        try
        //        {
        //            firm = Doc.GetPropertyObject("Фирма");
        //            if (firm.GetPropertyString("Код").Trim() != org.Connections1C.FirmCode)
        //            {
        //                return false;
        //            }
        //        }
        //        finally
        //        {
        //            if (firm != null) firm.Dispose();
        //        }
        //    }

        //    return true;
        //}

        private static double Get1C77_VAT(this COMObject doc, IOrganization org)
        {
            switch (doc.GLOBAL.Connection.ConfigType)
            {
                case E1CConfigType.Uaservice:
                case E1CConfigType.PUB:
                case E1CConfigType.Buch:
                    return doc.GetPropertyDouble("НДС");
                case E1CConfigType.Inforce:
                    return doc.GetPropertyDouble("НДС");
                default:
                    return 0;
            }
        }
        private static bool Is1C77_usluga(this COMObject doc, IOrganization org)
        {
            switch (doc.GLOBAL.Connection.ConfigType)
            {
                case E1CConfigType.Uaservice:
                case E1CConfigType.PUB:
                case E1CConfigType.Inforce:
                    return false;
                case E1CConfigType.Buch:
                    return doc.GetPropertyObject("ТМЦ")?.GetPropertyObject("ВидТМЦ")?.MethodString("Идентификатор") == "Услуга";
                default:
                    return false;
            }

        }
        private static string Get1C77_R03G10S(this COMObject doc, IOrganization org)
        {
            switch (doc.GLOBAL.Connection.ConfigType)
            {
                case E1CConfigType.Uaservice:
                case E1CConfigType.PUB:
                case E1CConfigType.Buch:
                case E1CConfigType.Inforce:
                    switch (doc.GetPropertyObject("ВидНДС").GetPropertyString("Code"))
                    {
                        case "БезНДС":
                            return "Без ПДВ";
                        default:
                            return null;
                    }
                default:
                    return null;

            }

        }
        private static string Get1C77_HNAMESEL(this COMObject doc, IOrganization org)
        {
            switch (doc.GLOBAL.Connection.ConfigType)
            {
                case E1CConfigType.Uaservice:
                    return doc.GLOBAL.EvalExpr("Константа.ОсновнаяФирма").GetPropertyString("ПолнНаименование");
                case E1CConfigType.PUB:
                    return doc.GetPropertyObject("Фирма").GetPropertyString("ПолнНаименование");
                case E1CConfigType.Buch:
                    return doc.GetPropertyObject("Фирма").GetPropertyString("ПолнНаименованиеНал");
                case E1CConfigType.Inforce:
                    return doc.GetPropertyObject("Клиент").GetPropertyString("ПолнНаим");
                default:
                    return "";
            }
        }
        private static string Get1C77_HKSEL(this COMObject doc, IOrganization org)
        {
            switch (doc.GLOBAL.Connection.ConfigType)
            {
                case E1CConfigType.Uaservice:
                    return doc.GLOBAL.EvalExpr("Константа.ОсновнаяФирма").GetPropertyString("ИНН");
                case E1CConfigType.PUB:
                case E1CConfigType.Buch:
                    return doc.GetPropertyObject("Фирма").GetPropertyString("ИНН");
                case E1CConfigType.Inforce:
                    return "";
                default:
                    return "";
            }
        }
        private static string Get1C77_TINSEL(this COMObject doc, IOrganization org)
        {
            var organization = org as Organization;
            if (organization != null)
            {
                if (organization.EDRPOUConsolidate != "") return organization.EDRPOUConsolidate;
            }
            switch (doc.GLOBAL.Connection.ConfigType)
            {
                case E1CConfigType.Uaservice:
                    return doc.GLOBAL.EvalExpr("Константа.ОсновнаяФирма").GetPropertyString("ЕДРПОУ");
                case E1CConfigType.PUB:
                case E1CConfigType.Buch:
                    return doc.GetPropertyObject("Фирма").GetPropertyString("ЕДРПОУ");
                case E1CConfigType.Inforce:
                    return null;
                default:
                    return null;
            }
        }

        private static string Get1C77_HNAMEBUY(this COMObject doc, IOrganization org)
        {
            switch (doc.GLOBAL.Connection.ConfigType)
            {
                case E1CConfigType.Uaservice:
                    return doc.GetPropertyObject("Контрагент").GetPropertyString("ПолнНаименование");
                case E1CConfigType.PUB:
                    return doc.GetPropertyObject("Контрагент").GetPropertyString("ПолнНаименование");
                case E1CConfigType.Buch:
                    return doc.GetPropertyObject("Контрагент").GetPropertyString("ПолнНаименованиеНал");
                case E1CConfigType.Inforce:
                    return doc.GetPropertyObject("Клиент").GetPropertyString("ПолнНаим");
                default:
                    return "";
            }
        }
        private static string Get1C77_HKBUY(this COMObject doc, IOrganization org)
        {
            switch (doc.GLOBAL.Connection.ConfigType)
            {
                case E1CConfigType.Uaservice:
                case E1CConfigType.PUB:
                case E1CConfigType.Buch:
                    return doc.GetPropertyObject("Контрагент").GetPropertyString("ИНН");
                case E1CConfigType.Inforce:
                    return doc.GetPropertyObject("Клиент").GetPropertyString("НомерПлательщикаНДС");
                default:
                    return "";
            }
        }
        private static string Get1C77_HFBUY(this COMObject doc, IOrganization org)
        {
            switch (doc.GLOBAL.Connection.ConfigType)
            {
                case E1CConfigType.Uaservice:
                case E1CConfigType.PUB:
                case E1CConfigType.Buch:
                    return doc.GetPropertyObject("Контрагент").GetPropertyString("ИНН");
                case E1CConfigType.Inforce:
                    return doc.GetPropertyObject("Клиент").GetPropertyString("НомерПлательщикаНДС");
                default:
                    return "";
            }
        }

        private static string Get1C77_HTINBUY(this COMObject doc, IOrganization org)
        {
            switch (doc.GLOBAL.Connection.ConfigType)
            {
                case E1CConfigType.Uaservice:
                case E1CConfigType.PUB:
                case E1CConfigType.Buch:
                    return doc.GetPropertyObject("Контрагент").GetPropertyString("ЕДРПОУ");
                case E1CConfigType.Inforce:
                    return doc.GetPropertyObject("Клиент").GetPropertyString("НомерПлательщикаНДС");
                default:
                    return null;
            }
        }


        private static string Get1C77_TmcDescription(this COMObject doc, IOrganization org)
        {
            switch (doc.GLOBAL.Connection.ConfigType)
            {
                case E1CConfigType.Uaservice:
                case E1CConfigType.PUB:
                case E1CConfigType.Inforce:
                    return doc.GetPropertyObject("ТМЦ").GetPropertyString("Наименование");
                case E1CConfigType.Buch:
                    return doc.GetPropertyObject("ТМЦ").GetPropertyString("ПолнНаименование");
                default:
                    return "";
            }
        }
        private static EVAT Get1C77_StavkaNDS(this COMObject doc, IOrganization org)
        {
            switch (doc.GLOBAL.Connection.ConfigType)
            {
                case E1CConfigType.Uaservice:
                case E1CConfigType.PUB:
                case E1CConfigType.Inforce:
                    return EVAT.Default;
                case E1CConfigType.Buch:
                    switch (doc.GetPropertyObject("ВидНДС").GetPropertyString("Code"))
                    {
                        case "БезНДС":
                            return EVAT.FreeVAT;
                        case "НДС20":
                            return EVAT.Default;
                        case "НДС7":
                            return EVAT.Country;
                        case "НДС0":
                            return EVAT.Export; //902
                        default:
                            return EVAT.FreeVAT;
                    }
                default:
                    return EVAT.Default;
            }
        }
        private static uint? Get1C77_PilgaNDS(this COMObject doc, IOrganization org)
        {
            switch (doc.GLOBAL.Connection.ConfigType)
            {
                case E1CConfigType.Uaservice:
                case E1CConfigType.PUB:
                case E1CConfigType.Inforce:
                    return null;
                case E1CConfigType.Buch:
                    switch (doc.GetPropertyObject("ВидНДС").GetPropertyString("Code"))
                    {
                        case "БезНДС":
                        case "НДС7":
                        case "НДС0":
                            return uint.Parse(doc.GetPropertyString("КодЛьготы"));
                        case "НДС20":
                            return null;
                        default:
                            return null;
                    }
                default:
                    return null;
            }
        }
        private static string Get1C77_UKTZED(this COMObject doc, IOrganization org)
        {
            switch (doc.GLOBAL.Connection.ConfigType)
            {
                case E1CConfigType.Uaservice:
                case E1CConfigType.PUB:
                case E1CConfigType.Inforce:
                case E1CConfigType.Buch:
                    COMObject temp = doc.GetPropertyObject("КодУКТВЭД");
                    if (!doc.GLOBAL.EmptyValue(temp))
                        return temp.GetPropertyString("Код");
                    else
                        return null;
                default:
                    return null;
            }
        }
        private static uint? Get1C77_UnitCode(this COMObject doc, IOrganization org)
        {
            switch (doc.GLOBAL.Connection.ConfigType)
            {
                case E1CConfigType.Uaservice:
                    return doc.GetPropertyObject("Ед").GetPropertyObject("ЕдиницаИзмерения").GetPropertyUint("Код");
                case E1CConfigType.PUB:
                    return doc.GetPropertyObject("Ед").GetPropertyObject("Единица").GetPropertyUint("КодЕдИзмерения");
                case E1CConfigType.Inforce:
                    return doc.GetPropertyObject("Ед").GetPropertyUint("КодЕдИзмерения");
                case E1CConfigType.Buch:
                    return doc.GetPropertyObject("Ед").GetPropertyUint("КодЕдИзмерения");
                default:
                    return new uint?();
            }
        }
        private static Decimal Get1C77_quantity(this COMObject doc, IOrganization org)
        {
            switch (doc.GLOBAL.Connection.ConfigType)
            {
                case E1CConfigType.Uaservice:
                case E1CConfigType.PUB:
                case E1CConfigType.Buch:
                    return (decimal)doc.GetPropertyDouble("Кво");
                case E1CConfigType.Inforce:
                    return (decimal)doc.GetPropertyDouble("Количество");
                default:
                    return 0;
            }
        }
        private static Decimal Get1C77_CostWithoutVAT(this COMObject doc, IOrganization org)
        {
            switch (doc.GLOBAL.Connection.ConfigType)
            {
                case E1CConfigType.Uaservice:
                case E1CConfigType.PUB:
                case E1CConfigType.Buch:
                    return (decimal)doc.GetPropertyDouble("ЦенаБезНДС");
                case E1CConfigType.Inforce:
                    return (decimal)doc.GetPropertyDouble("Цена");
                default:
                    return 0;
            }
        }
        private static Decimal Get1C77_SumWithoutVAT(this COMObject doc, IOrganization org)
        {
            switch (doc.GLOBAL.Connection.ConfigType)
            {
                case E1CConfigType.Uaservice:
                case E1CConfigType.PUB:
                case E1CConfigType.Buch:
                    return (decimal)doc.GetPropertyDouble("СуммаБезНДС");
                case E1CConfigType.Inforce:
                    return (decimal)doc.GetPropertyDouble("Сумма");
                default:
                    return 0;
            }
        }
        public static bool CheckDoc(this COMObject doc, IOrganization org)
        {
            // check transacted
            if (doc.MethodDouble("IsTransacted")==0) return false;

            // check firm selected
            switch (doc.GLOBAL.Connection.ConfigType)
            {
                case E1CConfigType.PUB:
                case E1CConfigType.Buch:
                    COMObject firm = null;
                    try
                    {
                        firm = doc.GetPropertyObject("Фирма");
                        if (firm.GetPropertyString("Код").Trim() != org.FirmCode)
                        {
                            return false;
                        }
                    }
                    finally
                    {
                        if (firm != null) firm.Dispose();
                    }
                    return true;
                default:
                    return true;
            }
        }

        /// <summary>
        /// Експорт податкових з 1С
        /// </summary>
        /// <param name="org"></param>
        /// <param name="period"></param>
        /// <param name="CallBack"></param>
        public static async Task<List<J1201010>> GetTAXES(IOrganization org, Connection con, Period period)
        {
            List<J1201010> taxes = new List<J1201010>();
            GlobalObject1C77 global;
            try
            {
                global = GlobalObject1C77.GetConnection(con);
            }
            catch
            { 
                return taxes;
            }
            
            COMObject TaxDocs = global.CreateObject("Документ.НалоговаяНакладная");

            var begin = period.Begin?.BeginOfDay() ?? DateTime.Now.BeginOfDay();
            var end = period.End?.EndOfDay() ?? DateTime.Now.EndOfDay(); 
            
            if (TaxDocs.MethodDouble("SelectDocuments",begin, end)==1)
            {
                while (TaxDocs.MethodDouble("GetDocument") == 1)
                {
                    COMObject CurrentDoc = TaxDocs.Method("CurrentDocument") as COMObject;
                    try
                    {
                        if (CheckDoc(CurrentDoc, org))
                        {
                            DateTime startDate = DateTime.Now;
                            taxes.Add(GetFrom1C77(global, CurrentDoc, org));
                            TimeSpan time = DateTime.Now.Subtract(startDate);
                            Trace.TraceInformation("Імпортована податкова №{0} від {1} (час виконання - {2})", CurrentDoc.GetPropertyString("DocNum"),CurrentDoc.GetPropertyDateTime("DocDate"),time.ToString("c"));
                        }
                    }
                    catch (Exception e)
                    {
                        Trace.TraceError("Податкова {0} не завантажена.{1}",CurrentDoc.GetPropertyString("DocNum"),e.Message);
                    }
                    finally
                    {
                        CurrentDoc.Dispose();
                    }
                    await Task.Delay(100);
                }
            }
            TaxDocs.Dispose();
            global.Dispose();
            return taxes;

        }
    
    
        public static async Task PutTaxes(List<XmlDocument> documents, IOrganization org,Connection con,Period period)
        {
            try
            {
                using (GlobalObject1C77 global = GlobalObject1C77.GetConnection(con))
                {
                    COMObject VATType = global.CreateObject("Справочник.ВидыНДС");
                    if (VATType.MethodDouble("НайтиПоНаименованию", "Платникам ПДВ", 0) == 0)
                    {
                        Trace.WriteLine(DateTime.Now.ToString() + "ERROR NOT Look in 1C7.7  Платникам ПДВ 20%");
                    }

                    Dictionary<string, string> Partners = new Dictionary<string, string>();
                    Trace.WriteLine(DateTime.Now.ToString() + "Load all partners from 1C7.7");
                    var Partners1C77 = global.CreateObject("Справочник.Контрагенты");
                    if (Partners1C77.MethodDouble("SelectItems", 0) == 1)
                    {
                        while (Partners1C77.MethodDouble("GetItem") == 1)
                        {
                            if (Partners1C77.MethodDouble("IsGroup") == 0)
                            {
                                string CodeINN = Partners1C77.GetPropertyString("ИНН");
                                if (CodeINN.Length == 0) continue;

                                if (Partners.ContainsKey(Partners1C77.GetPropertyString("ИНН")))
                                    Trace.WriteLine(DateTime.Now.ToString() + "Dublicate INN code for " + Partners1C77.GetPropertyString("ИНН") + " " + Partners1C77.GetPropertyString("Description"));
                                else
                                    Partners.Add(Partners1C77.GetPropertyString("ИНН"), Partners1C77.GetPropertyString("Code"));
                            }
                        }
                    }


                    Trace.WriteLine(DateTime.Now.ToString() + "Load all Inner TAX documents from 1C7.7");
                    Dictionary<string, TAXDoccument> TAXDoccuments = new Dictionary<string, TAXDoccument>();
                    var TaxDocs = global.CreateObject("Документ.РегистрацияПН");

                    if (TaxDocs.MethodDouble("SelectDocuments", period.Begin, period.End) == 1)
                    {
                        while (TaxDocs.MethodDouble("GetDocument") == 1)
                        {
                            TAXDoccument td = new TAXDoccument();
                            td.Date = TaxDocs.GetPropertyDateTime("DocDate");
                            td.Number = TaxDocs.GetPropertyString("Номер");
                            td.PartnerINN = TaxDocs.GetPropertyObject("Контрагент").GetPropertyString("ИНН");
                            td.SummDoc = (decimal)TaxDocs.GetPropertyDouble("Сумма");
                            td.VAT = (decimal)TaxDocs.GetPropertyDouble("НДС");
                            TAXDoccuments.Add(td.PartnerINN + " " + td.Date.ToShortDateString() + " " + td.Number, td);
                        }

                    }


                    foreach (var doc in documents)
                    {
                        XmlNode DB = doc.SelectSingleNode("/DECLAR/DECLARBODY");
                        string HKBUY = DB["HKBUY"].InnerText;
                        if (HKBUY != global.Get1C77_HKSEL(org))
                        {
                            Trace.WriteLine($"Податкова з ІПН {HKBUY }");
                            continue;
                        }

                        string HNUM = DB["HNUM"].InnerText;
                        // номер філії
                        string HNUM2 = DB["HNUM2"].InnerText;
                        if (HNUM2 != "")
                            HNUM += "//" + HNUM2;
                        string HKSEL = DB["HKSEL"].InnerText;
                        string HNAMESEL = DB["HNAMESEL"].InnerText;
                        //string H02G1S = DB["H02G1S"].InnerText;
                        string R03G11 = DB["R03G11"].InnerText;
                        string R04G11 = DB["R04G11"].InnerText;
                        DateTime HFILL = DB["HFILL"].InnerText.GetTAXDate().Value;
                        // check inn firm
                        if (Partners.ContainsKey(HKSEL))
                        {
                            // check document 
                            if (TAXDoccuments.ContainsKey(HKSEL + " " + HFILL.ToShortDateString() + " " + HNUM))
                            {
                                Trace.WriteLine(DateTime.Now.ToString() + " doc dont add to 1C7.7 " + HKSEL + " " + HNUM + " " + HFILL.ToShortDateString() +
                                                                        " " + R03G11.ToString() + " " + R04G11.ToString());
                                continue;

                            }

                            if (Partners1C77.MethodDouble("FindByCode", Partners[HKSEL], 0) == 0)
                            {
                                Trace.WriteLine(DateTime.Now.ToString() + "ERROR!!! not find INN in 1C7.7 ");
                                continue;
                            }
                            Trace.Write(DateTime.Now.ToString() + " Add doc to 1C7.7 " + HKSEL + " " + HNUM + " " + HFILL.ToShortDateString() +
                                                                        " " + R03G11.ToString() + " " + R04G11.ToString());
                            COMObject getФормыРасчета(GlobalObject1C77 global, string value)
                            {
                                switch (value)
                                {
                                    case "готівка": return global.EvalExpr("Перечисление.ФормыРасчета.Наличными");
                                    case "чек": return global.EvalExpr("Перечисление.ФормыРасчета.НаличнымиЧерезЭККА");
                                    default: return global.EvalExpr("Перечисление.ФормыРасчета.СРасчетногоСчета");
                                }

                            }

                            COMObject FR = getФормыРасчета(global, "рахунок");


                            var VN = global.EvalExpr("Перечисление.ВидНаловой.ПНЕ");

                            var TaxDoc = global.CreateObject("Документ.РегистрацияПН");
                            TaxDoc.Method("Новый");
                            TaxDoc.SetProperty("DocDate", HFILL);
                            TaxDoc.SetProperty("Контрагент", Partners1C77.Method("CurrentItem"));
                            TaxDoc.SetProperty("Номер", HNUM);
                            TaxDoc.SetProperty("ВидНДС", VATType.Method("CurrentItem"));
                            TaxDoc.SetProperty("ФормаРасчета", FR);
                            TaxDoc.SetProperty("НДС", R03G11);
                            TaxDoc.SetProperty("Сумма", R04G11);
                            TaxDoc.SetProperty("ТипПодатковой", 1);
                            TaxDoc.SetProperty("ДатаВиписки", HFILL);
                            TaxDoc.SetProperty("ВидНаловой", VN);
                            TaxDoc.SetProperty("Уточнена", 0);
                            try
                            {
                                TaxDoc.Method("Write");
                                Trace.WriteLine(" OK");

                            }
                            catch (Exception ex)
                            {
                                Trace.WriteLine($"ERROR {ex.Message}");
                            }


                        }
                    }
                }
            }
            catch (Exception e)
            {
                Trace.WriteLine($"Помилка: {e.Message}");
                return;
            }
            await Task.Delay(10);
        }
    
    }

}

