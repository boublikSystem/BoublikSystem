using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BoublikSystem.Entities
{
    public class SaleStorage
    {
        public int Id { get; set; }
        public int SalePointId { get; set; }
        public int ProductId { get; set; }
        public double Count { get; set; }

        public virtual Product Product { get; set; }
    }
}