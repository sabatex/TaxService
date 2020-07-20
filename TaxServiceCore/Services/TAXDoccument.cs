using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaxService.Services
{
    public struct TAXDoccument
    {
        public DateTime Date;
        public string Number;
        public string PartnerINN;
        public decimal VAT;
        public decimal SummDoc;
    }
}
