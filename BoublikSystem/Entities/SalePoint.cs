using System.Collections.Generic;

namespace BoublikSystem.Entities
{
    public class SalePoint
    {
        public int Id { get; set; }
        public string Adress { get; set; }

        public virtual ICollection<SaleStorage> Storage { get; set; }
    }
}