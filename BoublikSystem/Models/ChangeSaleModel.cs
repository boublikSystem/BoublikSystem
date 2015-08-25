using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BoublikSystem.Entities;

namespace BoublikSystem.Models
{
    public class ChangeSaleModel
    {
        public Product Product { get; set; }
        public double Count { get; set; }
    }
}