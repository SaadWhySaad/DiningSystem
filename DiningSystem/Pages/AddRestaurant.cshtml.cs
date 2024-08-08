using DiningSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;
using static DiningSystem.Pages.webManagerModel;

namespace DiningSystem.Pages
{
    public class AddRestaurantModel : PageModel
    {
        [BindProperty]
        public Restaurant Restaurant { get; set; }

        private readonly string _connectionString;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AddRestaurantModel(IConfiguration configuration, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _userManager = userManager;
            _roleManager = roleManager;
        }




        public void OnGet()
        {
        }
        public async Task<IActionResult> OnPostAsync()
        {
            
                if (!ModelState.IsValid)
                {
                    return Page();
                }

                var user = await _userManager.FindByIdAsync(Restaurant.AdminUsername);
                if (user == null)
                {
                    ModelState.AddModelError("", "User not found.");
                    return Page();
                }

                if (!await _roleManager.RoleExistsAsync("admin"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("admin"));
                }

                if (!await _userManager.IsInRoleAsync(user, "admin"))
                {
                    await _userManager.AddToRoleAsync(user, "admin");
                }

                // Step 2: Add the restaurant to the database
                using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var query = @"
                    INSERT INTO restaurant (r_name, r_description, r_admin, max_table_servings, open_close_timing)
                    VALUES (@r_name, @r_description, @r_admin, @max_table_servings, @open_close_timings)";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@r_name", Restaurant.r_name);
                    command.Parameters.AddWithValue("@r_description", Restaurant.r_description);
                    command.Parameters.AddWithValue("@r_admin", Restaurant.AdminUsername);
                    command.Parameters.AddWithValue("@max_table_servings", Restaurant.max_table_servings);
                    command.Parameters.AddWithValue("@open_close_timings", Restaurant.open_close_timings);
                    await command.ExecuteNonQueryAsync();
                }
            }

            return RedirectToPage("/Restaurant");
        }
    }
}
