using DiningSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DiningSystem.Services
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options) 
        {
            
        }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            //create admin role
            var admin = new IdentityRole("admin");
            admin.NormalizedName = "admin";

            //create user role
            var customer = new IdentityRole("customer");
            customer.NormalizedName = "customer";

            var webAdmin = new IdentityRole("webAdmin");
            webAdmin.NormalizedName = "webAdmin";

            builder.Entity<IdentityRole>().HasData(admin, customer, webAdmin);
        }
    }
}
