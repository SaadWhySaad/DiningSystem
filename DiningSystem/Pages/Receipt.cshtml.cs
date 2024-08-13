using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

public class ReceiptModel : PageModel
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IConfiguration _configuration;

    public ReceiptModel(UserManager<ApplicationUser> userManager, IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    public ApplicationUser UserInformation { get; set; }
    public List<MenuItem> MenuItems { get; set; } = new List<MenuItem>();

    public async Task<IActionResult> OnGetAsync()
    {
        // Get the current user
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return NotFound("User not found.");
        }

        UserInformation = user;

        string connectionString = _configuration.GetConnectionString("DefaultConnection");

        using (SqlConnection connection = new SqlConnection(connectionString))
        {
            await connection.OpenAsync();

            // Query to fetch the items in the cart for the user
            string query = @"
                SELECT rm.menu_item, rm.menu_description, rm.menu_item_price, ci.Quantity
                FROM CartItems ci
                JOIN restaurantMenu rm ON ci.ItemId = rm.menu_id
                WHERE ci.UserId = @UserId";

            using (SqlCommand command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@UserId", user.Id);

                using (SqlDataReader reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        MenuItems.Add(new MenuItem
                        {
                            MenuItemName = reader.GetString(0),
                            MenuDescription = reader.GetString(1),
                            MenuItemPrice = reader.GetDecimal(2),
                            Quantity = reader.GetInt32(3)
                        });
                    }
                }
            }
        }

        return Page();
    }
}

public class MenuItem
{
    public string MenuItemName { get; set; }
    public string MenuDescription { get; set; }
    public decimal MenuItemPrice { get; set; }
    public int Quantity { get; set; }
}
