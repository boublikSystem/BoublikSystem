using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Script;
using BoublikSystem.Entities;
using BoublikSystem.Models;

namespace BoublikSystem.Controllers
{
    [Authorize(Roles = "seller, admin")]
    public class SaleController : Controller
    {
        private static ApplicationDbContext _context = new ApplicationDbContext();// БД
        private static Dictionary<Product, double> _recivedProducts = new Dictionary<Product, double>(); // дабавленые продукты в накладную
        private static Dictionary<Product, double> _currentBill = new Dictionary<Product, double>(); //текущий чек

        // GET: Sale
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult CreateBill()
        {
            if (Request.IsAjaxRequest())
            {
                int salePoint = _context.Users.First(u => u.UserName == User.Identity.Name).SallerLocation;

                Bill bill = new Bill
                {
                    Amount = CalculateAmount(),
                    DataTime = DateTime.Now,
                    SalePointId = salePoint
                };

                _context.Bills.Add(bill);

                ProductToBill productToBill = new ProductToBill();

                foreach (var item in _currentBill)
                {
                    productToBill.BillId = bill.Id;
                    productToBill.ProductId = item.Key.Id;
                    productToBill.Count = item.Value;

                    _context.ProductToBills.Add(productToBill);


                    #region Delete product count from storage

                    _recivedProducts[item.Key] -= item.Value;
                    _context.SalePoints
                        .Find(salePoint).Storage
                        .ToList()
                        .Find(p => p.ProductId == item.Key.Id)
                        .Count -= item.Value;
                    //double rest = item.Value;
                    //foreach (var variable in _context.ProductToWayBills.Where(w => w.ProductId == item.Key.Id))
                    //{
                    //    if (variable.Count >= rest)
                    //    {
                    //        variable.Count -= rest;
                    //        break;
                    //    }
                    //    else
                    //    {
                    //        rest -= variable.Count;
                    //        variable.Count -= variable.Count;
                    //    }
                    //}

                    #endregion
                }

                _context.SaveChanges();


                _currentBill.Clear();

                return PartialView("AddProductToBill", _currentBill);
            }

            _recivedProducts = GetProduct();

            return View(_recivedProducts);
        }

        /// <summary>
        /// Вызов диалогового окнда для уточнения количества
        /// </summary>
        /// <param name="productId">Ид продукта</param>
        /// <returns></returns>
        public PartialViewResult AskForCount(int productId)
        {
            return PartialView(productId);
        }

        /// <summary>
        /// Метод обрабатывает нажатие на ссылку "Добавить"
        /// и добавляет продукт в чек
        /// </summary>
        /// <param name="productId">Ид продукта</param>
        /// <returns></returns>
        public PartialViewResult AddProductToBill(int productId)
        {
            // при инициализации поссылаю 0
            if (productId != 0)
            {
                Product recivedProduct = _recivedProducts.First(p => p.Key.Id == productId).Key;
                string checkForPoint = null;
                double count = 0;

                checkForPoint = Request["countField"].Replace(".", ",");
                count = Convert.ToDouble(checkForPoint);

                if (_currentBill.ContainsKey(recivedProduct))
                {
                    _currentBill[recivedProduct] += count;
                }
                else
                {
                    _currentBill.Add(recivedProduct, count);
                }
            }

            return PartialView(_currentBill);
        }

        public PartialViewResult DeleteProductFromBill(int productId)
        {
            Product productToDelete = _recivedProducts.First(p => p.Key.Id == productId).Key;

            _currentBill.Remove(productToDelete);

            return PartialView("AddProductToBill", _currentBill);
        }

        /// <summary>
        /// Метод нахождения всех накладных и продуктов, которые в них есть
        /// </summary>
        private Dictionary<Product, double> GetProduct()
        {
            string userName = User.Identity.Name;

            int sellerLocation = _context.Users.First(u => u.UserName == userName).SallerLocation;

            IEnumerable<SaleStorage> saleStorage = _context.SalePoints.Find(sellerLocation).Storage;
            Dictionary<Product,double> allProductsWithCount = new Dictionary<Product, double>();

            foreach (var item in saleStorage)
            {
                if (allProductsWithCount.ContainsKey(item.Product))
                {
                    allProductsWithCount[item.Product] += item.Count;
                }
                else
                {
                    allProductsWithCount.Add(item.Product, item.Count);
                }
            }

            //foreach (var wayBill in wayBills)
            //{
            //    foreach (var item in wayBill.ProductToWayBills.ToList())
            //    {
            //        if (allProductsWithCount.ContainsKey(item.Product))
            //        {
            //            allProductsWithCount[item.Product] += item.Count;
            //        }
            //        else
            //        {
            //            allProductsWithCount.Add(item.Product, item.Count);
            //        }

            //    }
            //}

            return allProductsWithCount;
        }

        /// <summary>
        /// Метод изменяет количество (счетчик) полученных продуктов
        /// Обрабатывает добавление
        /// </summary>
        /// <param name="productId">Ид продукта</param>
        /// <param name="productCount">Количество продукта</param>
        /// <returns></returns>
        public ActionResult ChangeCount(int? productId, double? productCount)
        {
            Product product = _recivedProducts.Keys.First(p => p.Id == productId);

            _recivedProducts[product] -= Convert.ToDouble(productCount);

            string strToChange = string.Format("{0} {1}", _recivedProducts[product], product.MeasurePoint);

            return PartialView("_ChangeCountProduct", strToChange);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="productId"></param>
        /// <param name="productCount"></param>
        /// <param name="notNeeded"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ChangeCount(int? productId, double? productCount, int? notNeeded)
        {
            Product product = _recivedProducts.Keys.First(p => p.Id == productId);

            _recivedProducts[product] += Convert.ToDouble(productCount);

            //string strToChange = string.Format("{0} {1}", _recivedProducts[product], product.MeasurePoint);
            string strToChange = _recivedProducts[product].ToString();

            return PartialView("_ChangeCountProduct", strToChange);
        }

        public ActionResult Calculate()
        {
            decimal amount = CalculateAmount();

            return PartialView("_Calculate", amount);
        }

        private decimal CalculateAmount()
        {
            return _currentBill.Sum(item => item.Key.Price * (decimal)item.Value);
        }
    }
}