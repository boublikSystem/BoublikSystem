﻿using System.Data.Entity;
using System.Linq;
using System.Net.Cache;
using System.Security.Claims;
using System.Threading.Tasks;
using BoublikSystem.Entities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace BoublikSystem.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {

            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
       // [Required(ErrorMessage = "The {0} must be at least {2} characters long.")]
        public int SallerLocation { get; set; }
        public string SelectedRole { get; set; }
        //[ForeignKey("SallerLocation")]
        //public virtual SalePoint SalePoint { get; set; }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {

        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }


        public DbSet<Product> Products { get; set; }
        public DbSet<SalePoint> SalePoints { get; set; }
        public DbSet<ProductToWayBill> ProductToWayBills { get; set; }
        public DbSet<WayBill> WayBills { get; set; }
        public DbSet<ProductToBill> ProductToBills { get; set; }
        public DbSet<Bill> Bills { get; set; }
        public DbSet<WritingOffProduct> WritingOffProducts { get; set; }

        //public System.Data.Entity.DbSet<BoublikSystem.Models.WritingOffModel> WritingOffModels { get; set; }
        
        //public System.Data.Entity.DbSet<BoublikSystem.Models.ApplicationUser> ApplicationUsers { get; set; }
    }
}