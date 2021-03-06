﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BoublikSystem.Entities
{
    public class ProductToBill
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int BillId { get; set; }
        public double Count { get; set; }

        public virtual Bill Bill { get; set; }
        public virtual Product Product { get; set; }
    }
}