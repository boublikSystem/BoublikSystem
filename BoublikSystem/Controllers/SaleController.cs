using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;
using System.Web.Management;
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
        private static List<Bill> _currentSearchList = new List<Bill>();
        private static int _currentSearchPageIndex = 0;
        private static int _pagesCount = 0;

        // GET: Sale
        public ActionResult Index()
        {
            
            return View();
        }

        public ActionResult CreateBill()
        {
            // [HttpPost]
            if (Request.IsAjaxRequest())
            {
                int salePoint = GetSellerLocation();

                Bill bill = new Bill
                {
                    Amount = CalculateAmount(),
                    DateTime = DateTime.Now,
                    SalePointId = salePoint
                };

                _context.Bills.Add(bill);

                // записать елементы чека (продукт) в БД
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

                    #endregion
                }

                _context.SaveChanges();

                _currentBill.Clear();

                return PartialView("AddProductToBill", _currentBill);
            }

            _recivedProducts = GetProducts();
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
        /// Метод обрабатывает нажатие на ссылку "Добавить",
        /// добавляет продукт в чек
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
        /// Удаляем товар
        /// Обновляет список "Формирование чека"
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
        private Dictionary<Product, double> GetProducts()
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
            int sellerLocation = GetSellerLocation();
            var item = DateTime.Now.Date;
            var list = _context.Bills.OrderByDescending(u => u.DateTime).Where(b => (b.SalePointId == sellerLocation)).Take(1000).ToList();

            _currentSearchList.Clear();

            //Bill for current day
            foreach (var bill in list)
            {
                if (((DateTime)(bill.DateTime)).Date == item.Date)
                    _currentSearchList.Add(bill);
            }

            _pagesCount = GetPageCount(_currentSearchList);

            return View(GetBillSearchModel());
        }

        [HttpPost]
        public ActionResult BillCancellation(BillSearchModel searchModel)
        {
            if (Request.IsAjaxRequest())
            {


                return PartialView();
            }

            int sellerLocation = GetSellerLocation();

            _currentSearchList = _context.Bills.OrderByDescending(u => u.DateTime)
                .Where(b => (b.SalePointId == sellerLocation) & (((DateTime)(b.DateTime)).Date == DateTime.Now.Date)).ToList();

            return View(searchModel);
        }

        public PartialViewResult SearchResult(int pageNumber = 0, string serchByProductName = null)
        {
            int sellerLocation = GetSellerLocation();

            _currentSearchPageIndex = pageNumber;

            var listForSearch = _currentSearchList.Skip(_currentSearchPageIndex * 10).Take(10).Where(b => b.SalePointId == sellerLocation).ToList();

            ViewBag.PageCount = GetPageCount(_currentSearchList);
            ViewBag.CurrentPage = _currentSearchPageIndex;


            return PartialView("_SearchResult", listForSearch);
        }

        public ActionResult CancelBill(int billId)
        {
            var bill = _context.Bills.Find(billId);
            int sellerLocation = GetSellerLocation();

            AddBillToStorage(bill, sellerLocation);

            _currentSearchList.Remove(bill);
            _context.Bills.Remove(bill);
            _context.SaveChanges();

            ViewBag.PageCount = GetPageCount(_currentSearchList);

            if ((_currentSearchList.Count % 10) == 0)
                _currentSearchPageIndex -= 1;

            ViewBag.CurrentPage = _currentSearchPageIndex;

            var list = _currentSearchList.Skip(_currentSearchPageIndex * 10).Take(10).Where(b => b.SalePointId == sellerLocation).ToList();


            return PartialView("_SearchResult", list);
        }

        // таблица с продукцией для списания
        public ActionResult WritingOff()
        {
            int sellerLocation = GetSellerLocation();
            var storage = _context.SalePoints.Find(sellerLocation).Storage.ToList();
            return View(storage);
        }

        // Запрос на списание конкретного товара
        public ActionResult ProductToWritingOff(int productId)
        {
            int sellerLocation = GetSellerLocation();
            var item = _context.SalePoints.Find(sellerLocation).Storage;
            double count = item.First(p => p.ProductId == productId).Count;
            string name = item.First(p => p.ProductId == productId).Product.Name;

            ViewBag.Count = count;
            ViewBag.ProductName = name;

            var woff = new WritingOffProduct { ProductId = productId };

            return View(woff);
        }

        // Получения информации для списания
        [HttpPost]
        public ActionResult ProductToWritingOff(WritingOffProduct product)
        {
            if (ModelState.IsValid)
            {
                ViewBag.ProductName = _context.Products.Find(product.ProductId).Name;
                return View("ConfirmWritingOff", product);
            }

            return View(product);
        }

        // Полученное подтверждение для списания
        // удаления со склада, добавления в таблицу о списании
        [HttpPost]
        public ActionResult ConfirmWritingOff(WritingOffProduct product)
        {
            int sellerLocation = GetSellerLocation();
            var storage = _context.SalePoints.Find(sellerLocation).Storage
                .First(p => p.ProductId == product.ProductId).Count -= product.Count;
            product.Date = DateTime.Now;
            product.SalePointId = sellerLocation;
            product.UserId = _context.Users.First(u => u.UserName == User.Identity.Name).Id;
            _context.WritingOffProducts.Add(product);
            _context.SaveChanges();

            
            var listOfProducts = _context.SalePoints.Find(sellerLocation).Storage.ToList();

            return View("WritingOff",listOfProducts);
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

        private BillSearchModel GetBillSearchModel()
        {
            return new BillSearchModel
            {
                DateTime = DateTime.Now
            };
        }

        private int GetPageCount(List<Bill> bills)
        {
            double x = Math.Truncate(bills.Count / 10.0);
            double result = bills.Count / 10.0;

            result -= x;

            if (result > 0)
                x += 1;

            return (int)x;

        }

        private void ParseSearchModel(BillSearchModel search)
        {
            _currentSearchList = _context.Bills.Where(b => ((DateTime)(b.DateTime)).Date == search.DateTime.Date).ToList();
        }
    }

}