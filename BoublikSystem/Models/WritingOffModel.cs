using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace BoublikSystem.Models
{
    public class WritingOffModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string UserId { get; set; }
        public int SalePointId { get; set; }

        [Required(ErrorMessage = "Введите причину списания!")]
        [Display(Name = "Причина списания")]
        public string Message { get; set; }

        [Required(ErrorMessage = "Введите количества продукта для списания")]
        [Display(Name = "Количество")]
        public double Count { get; set; }
    }
}