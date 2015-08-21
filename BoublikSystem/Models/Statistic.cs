using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using BoublikSystem.Entities;


namespace BoublikSystem.Models
{
    public class Statistic
    {
        private static ApplicationDbContext context = new ApplicationDbContext();
        public IEnumerable<WayBill> CookBills { get; set; }
        public List<WayBill> GetCookBills(string userName, string item, string period = "ALL")
        {
            List<WayBill> userBills = new List<WayBill>();
            if (item == "1")
            {
                if (period == "ALL")
                {
                    userBills = GetUserBills(userName).ToList();
                }

            }

            //TODO: Различные запросы

            return userBills;
        }

        /// <summary>
        /// Получает все накладные пользователя
        /// </summary>
        /// <param name="userName">User name</param>
        /// <returns>IEnumerable WayBill </returns>
        private IEnumerable<WayBill> GetUserBills(string userName)
        {
            // string userName = User.Identity.Name; //получаем имя авторизованного пользователя

            int sellerLocation = context.Users.First(u => u.UserName == userName).SallerLocation;

            IEnumerable<WayBill> wayBills = context.WayBills.ToList();//.Where(w => w.SalesPointId == sellerLocation).ToList();
            Dictionary<WayBill, List<Product>> productsInWayBill = new Dictionary<WayBill, List<Product>>();
            foreach (var bill in wayBills)
            {
                var productToWayBills = bill.ProductToWayBills.ToList();

            }

            return wayBills;
        }

        public void GetSellerBills()
        {

        }
    }
}