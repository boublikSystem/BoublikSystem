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
    public class ChangeSaleModel
    {
        public Product Product { get; set; }
        public double Count { get; set; }
    }

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
                int salePoint = GetSellerLocation();

                Bill bill = new Bill
                {
                    Amount = CalculateAmount(),
                    DataTime = DateTime.Now,
                    SalePointId = salePoint
                };

                _context.Bills.Add(bill);

                

                foreach (var item in _currentBill)
                {
                    ProductToBill productToBill = new ProductToBill();

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
            _currentBill.Clear();
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

        /// <summary>
        /// Обновляет список товаров добавленный в чек
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
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

            int sellerLocation = GetSellerLocation();

            IEnumerable<SaleStorage> saleStorage = _context.SalePoints.Find(sellerLocation).Storage;
            Dictionary<Product, double> allProductsWithCount = new Dictionary<Product, double>();

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
        /// Метод изменяет количество (отнимает) полученных продуктов
        /// Обрабатывает добавление
        /// </summary>
        /// <param name="productId">Ид продукта</param>
        /// <param name="productCount">Количество продукта</param>
        /// <returns></returns>
        public ActionResult ChangeCount(int? productId, double? productCount)
        {
            Product product = _recivedProducts.Keys.First(p => p.Id == productId);

            _recivedProducts[product] -= Convert.ToDouble(productCount);

            string strToChange = _recivedProducts[product].ToString();

            ChangeSaleModel cahSaleModel = new ChangeSaleModel
            {
                Product = product,
                Count = _recivedProducts[product]
            };

            return PartialView("_ChangeCountProduct", cahSaleModel);
        }

        /// <summary>
        /// Метод изменения количеста (прибавляет) полученный продуктов
        /// Обрабатывает удаление из чека
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

            ChangeSaleModel cahSaleModel = new ChangeSaleModel
            {
                Product = product,
                Count = _recivedProducts[product]
            };

            return PartialView("_ChangeCountProduct", cahSaleModel);
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

        public ActionResult BillCancellation()
        {
            var searchBySum = Request["searchBySum"];
            var searchByProduct = Request["searchByProduct"];
            var searchByDay = Request["searchByDay"];


            if (Request.IsAjaxRequest())
            {
                string str = Request["str"];

                return PartialView("_SearchResult");
            }

            return View();
        }

        public ActionResult DefaultSearchResult()
        {
            int sellerLocation = GetSellerLocation();

            var list = _context.Bills.OrderByDescending(u=>u.DataTime).Take(10).Where(b => b.SalePointId == sellerLocation).ToList();

            return PartialView(list);
        }

        public ActionResult CancelBill(int billId)
        {
            var bill = _context.Bills.Find(billId);
            int sellerLocation = GetSellerLocation();
            
            AddBillToStorage(bill,sellerLocation);
            
            _context.Bills.Remove(bill);
            _context.SaveChanges();

            var list = _context.Bills.OrderByDescending(u => u.DataTime).Take(10).Where(b => b.SalePointId == sellerLocation).ToList();

            return PartialView("DefaultSearchResult",list);
        }

        private int GetSellerLocation()
        {
            return _context.Users.First(u => (u.UserName == User.Identity.Name)).SallerLocation;
        }

        private void AddBillToStorage(Bill bill, int sellerLocation)
        {
            var storage = _context.SalePoints.Find(sellerLocation).Storage;

            foreach (var item in bill.Products)
            {
                SaleStorage saleStorage = new SaleStorage
                {
                    Count = item.Count,
                    ProductId = item.Product.Id,
                    SalePointId = sellerLocation
                };

                storage.Add(saleStorage);
            }
        }
    }

}