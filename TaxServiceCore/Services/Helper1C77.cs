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
    }

}

