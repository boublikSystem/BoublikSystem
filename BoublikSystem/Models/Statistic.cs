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
        public struct Period
        {
            public DateTime Start { get; set; }
            public DateTime End { get; set; }
        }

        private Period period;

        public List<WayBill> GetCookBills(string userName, string item,
            DateTime periodStart = new DateTime(), DateTime periodEnd = new DateTime())
        {
            List<WayBill> userBills=new List<WayBill>();
            List<WayBill> userBillsByPeriod = new List<WayBill>();
            period = new Period { Start = periodStart, End = periodEnd };
            userBills = GetUserBills(userName).ToList();
             userBills = new List<WayBill>(userBills.OrderByDescending(bill => bill.Id));
            if (item == "1")
            {

                if ((period.Start == Convert.ToDateTime("01.01.0001 0:00:00")) ||
                    (period.End == Convert.ToDateTime("01.01.0001 0:00:00")))
                    return userBills;
                

                if (period.Start == period.End)
                {
                    userBillsByPeriod.AddRange(userBills.Where(bill => bill.Date.Day == period.End.Day));
                    return userBillsByPeriod;
                } //LINQ it's POWER!!!
                userBillsByPeriod.AddRange(userBills.Where(bill => (bill.Date >= period.Start) && (bill.Date <= period.End)));
                return userBillsByPeriod;
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