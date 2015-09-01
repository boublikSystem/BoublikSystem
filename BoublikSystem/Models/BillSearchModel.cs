using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BoublikSystem.Models
{
    public class BillSearchModel
    {
        public double Sum { get; set; }
        public string Product { get; set; }
        public DateTime DateTime { get; set; }
    }

    public class SpecificBillSearchModel
    {
        public List<BillSearchModel> Bills { get; set; }
        public int PageCount { get; set; }
    }
}