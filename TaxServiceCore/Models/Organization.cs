using sabatex.TaxUA;
using sabatex.V1C77;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace TaxService.Models
{
    [Serializable]
    public class Organization:Connection,IOrganization
    {
        public Organization()
        {
            Name = "NewOrganization";
        }

        public string Name { get; set; }
        public string FullName { get; set; }

        public string Description { get; }

        
        public bool IS_NP { get; set; }
        //public string KODE { get; set; }
        public int C_REG { get; set; }
        public int C_RAJ { get; set; }
        public string FilialNumber { get; set; }
        public string Manager { get; set; }
        public string ManagerIPN { get; set; }
        public string CODE_IN_1C { get; set; }

        public string J12010_OutPath { get; set; }
        public int J12010_StartNumber { get; set; }
        public bool AutoPDV { get; set; }
        public bool IsReadOnly { get; set; } = false;
        /// <summary>
        /// Код фірми в довіднику 1С
        /// </summary>
        public string FirmCode { get; set; }
        /// <summary>
        /// Вказуємо INN як-що він відрізняється від основного 
        /// </summary>
        public string EDRPOUConsolidate { get; set; }

        public void MarkDelete()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return Name;
        }

    }
}
