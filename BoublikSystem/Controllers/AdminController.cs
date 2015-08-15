using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BoublikSystem.Entities;
using BoublikSystem.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;

namespace BoublikSystem.Controllers
{
    public class AdminController : Controller
    {
        private static ApplicationDbContext dbContext = new ApplicationDbContext();
        private static IEnumerable<SelectListItem> adrressList;
        private ApplicationUserManager manager = new ApplicationUserManager(new UserStore<ApplicationUser>(dbContext));
        private RoleManager<IdentityRole> roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(dbContext));

        public AdminController()
        {
            adrressList = CreateAddresList(dbContext.SalePoints.ToList());
        }
        private List<SelectListItem> CreateAddresList(List<SalePoint> salesList)
        {
            List<SelectListItem> answer = new List<SelectListItem>();

            if (salesList != null && salesList.Count > 0)
            {
                for (int i = 0; i < salesList.Count; i++)
                {
                    answer.Add(new SelectListItem
                    {
                        Text = salesList[i].Adress,
                        Value = salesList[i].Id.ToString()
                    });
                }
            }

            return answer;
        }
        /// <summary>
        /// Устанавливает значения группы элементов CheckBox в положение true или false
        /// в зависимости от количества пренадлежащих ролей пользователю.
        /// </summary>
        /// <param name="id">Id пользователя</param>
        private void InitRoleValuesForCheckBoxes(string id = "")
        {
            ApplicationUser user = manager.FindByName(id);
            Dictionary<string, bool> rolesChecked =
                new Dictionary<string, bool>(); //создаем словарь для отображения ролей(CheckBoxes in View)

            foreach (var role in roleManager.Roles) // инициализируем словарь дефолтными значениями
            {
                rolesChecked.Add(role.Name, false);
            }

            if (id != "")//если юзер не новый
            {
                var userRoles = user.Roles.ToList();    //получаем роли текущего пользователя

                for (int i = 0; i < user.Roles.Count; i++)  //если находим имя роли в менеджере - устанавливаем true в значение    
                {                                           //словаря, иначе false. Таким образом устанавливаются положения checkbox-ов
                    if (roleManager.RoleExists(roleManager.FindById(userRoles[i].RoleId).Name))
                    {
                        rolesChecked[roleManager.FindById(userRoles[i].RoleId).Name] = true;

                    }
                    else
                    {
                        rolesChecked[roleManager.FindById(userRoles[i].RoleId).Name] = false;
                    }
                }
            }
            ViewBag.RolesDictionary = rolesChecked; //передаем словарь в представление

        }
        public ActionResult Index()
        {
            return View();
        }

        // GET: Admin
        public ActionResult CrudUser()
        {
            List<ApplicationUser> users = manager.Users.ToList();
            List<UserRoleModels> model = new List<UserRoleModels>();
            IList<string> usersRoles;
            string roles = null;

            for (int i = 0; i < users.Count; i++)
            {
                // Полечение всех ролей для юзера
                usersRoles = manager.GetRoles(users[i].Id);
                for (int j = 0; j < usersRoles.Count; j++)
                {
                    roles += usersRoles[j].ToString() + " ";
                }

                // Создание модели для отображения информации о юзере
                model.Add(new UserRoleModels { User = manager.FindById(users[i].Id), Role = roles });
                roles = null;
            }

            return View(model);
        }

        // GET: Admin/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            //todo : начни отсюда
            ApplicationUser applicationUser = manager.FindByName(id);
            if (applicationUser == null)
            {
                return HttpNotFound();
            }
            return View(applicationUser);
        }

        // GET: Admin/Create
        public ActionResult Create()
        {
            ViewBag.SalePoints = adrressList;
            ViewBag.Roles = roleManager.Roles.ToList();
            InitRoleValuesForCheckBoxes();
            return View();
        }

        // POST: Admin/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include =
            "Id,Email,EmailConfirmed,PasswordHash,SecurityStamp," +
            "PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled," +
            "LockoutEndDateUtc,LockoutEnabled,AccessFailedCount," +
            "UserName,SelectedRole,SallerLocation")] ApplicationUser applicationUser)
        {

            if (ModelState.IsValid)
            {
                //todo: ошибка если нажать создать и не заполнить поля


                if ((applicationUser.UserName == null) | (applicationUser.PasswordHash == null))
                    return View(applicationUser);

                var hasher = new PasswordHasher();

                applicationUser.PasswordHash = hasher.HashPassword(applicationUser.PasswordHash);
                manager.Create(applicationUser);

                List<string> r = new List<string>();
                try
                {
                    foreach (var roleId in Request["selectedRoles"].Split(','))
                    {
                        r.Add(roleManager.FindByName(roleId).Name);

                    }
                }
                catch { }
                manager.AddToRoles(applicationUser.Id,r.ToArray());
                return RedirectToAction("CrudUser");
            }

            return View(applicationUser);
        }
    
        // GET: Admin/Edit/5
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser applicationUser = manager.FindByName(id);
            if (applicationUser == null)
            {
                return HttpNotFound();
            }

            ViewBag.SalePoints = adrressList;
            ViewBag.Roles = roleManager.Roles.ToList();

            InitRoleValuesForCheckBoxes(id);


            return View(applicationUser);
        }

        // POST: Admin/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include =
            "Id,Email,EmailConfirmed,PasswordHash," +
            "SecurityStamp,PhoneNumber,PhoneNumberConfirmed," +
            "TwoFactorEnabled,LockoutEndDateUtc,LockoutEnabled," +
            "AccessFailedCount,UserName,SallerLocation")] ApplicationUser applicationUser)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = manager.FindByName(applicationUser.Id);

                var hasher = new PasswordHasher();



                if ((applicationUser.PasswordHash != null) && (applicationUser.PasswordHash.IndexOf(' ') == -1))
                {
                    user.PasswordHash = hasher.HashPassword(applicationUser.PasswordHash);
                }
                if (applicationUser.SallerLocation != 0)
                {
                    user.SallerLocation = applicationUser.SallerLocation;
                }
                //формирование списка названий ролей по id для добавления ролей юзеру
                List<string> r = new List<string>();
                try
                {
                    foreach (var roleId in Request["selectedRoles"].Split(','))
                    {
                        r.Add(roleManager.FindByName(roleId).Name);

                    }
                }
                catch { }
                List<string> rolesName = new List<string>();
                foreach (var role in roleManager.Roles)
                {
                    foreach (var roleUser in user.Roles)
                    {
                        if (roleUser.RoleId == role.Id)//сравнить если юзер имеет роль то добавить ее имя в список rolesName
                            rolesName.Add(role.Name);

                    }

                }
                manager.RemoveFromRoles(user.Id, rolesName.ToArray()); //delete user from all roles

                manager.AddToRoles(user.Id, r.ToArray());
                manager.Update(user);


                //  manager.AddToRole(user.Id, dbContext.Roles.Find(applicationUser.SelectedRole).Name);
                return RedirectToAction("CrudUser");
            }
            return View(applicationUser);
        }

        // GET: Admin/Delete/5
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ApplicationUser applicationUser = manager.FindByName(id);
            if (applicationUser == null)
            {
                return HttpNotFound();
            }
            return View(applicationUser);
        }

        // POST: Admin/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(string id)
        {
            ApplicationUser applicationUser = manager.FindByName(id);
            manager.Delete(applicationUser);
            return RedirectToAction("CrudUser");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                manager.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
