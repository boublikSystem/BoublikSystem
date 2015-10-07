using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using BoublikSystem.Models;

namespace BoublikSystem.Entities
{
    public class WritingOffProduct
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string UserId { get; set; }
        public int SalePointId { get; set; }
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Введите причину списания!")]
        [Display(Name = "Причина списания")]
        public string Message { get; set; }

        [Required(ErrorMessage = "Введите корректное количества продукта для списания")]
        [Display(Name = "Количество")]
        public double Count { get; set; }

        public virtual Product Product { get; set; }
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
        public virtual SalePoint SalePoint { get; set; }
    }
}