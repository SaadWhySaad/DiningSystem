using DiningSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;

namespace DiningSystem.Pages
{
    public class PendingOrdersModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;

        public PendingOrdersModel(IConfiguration configuration, UserManager<ApplicationUser> userManager)
        {
            _configuration = configuration;
            _userManager = userManager;
        }


        public List<Order> PendingOrders { get; set; } = new List<Order>();

        public async Task OnGetAsync()
        {
            string userId = _userManager.GetUserId(User);
            await LoadPendingOrdersAsync(userId);
        }

        private async Task LoadPendingOrdersAsync(string userId)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string sql = @"
                    SELECT o.orderId, o.OrderType, o.DineInDate, o.DineInTime, o.NumberOfPersons, 
                           o.DeliveryAddress, o.Amount, o.pending_amount
                    FROM Orders o
                    WHERE o.UserId = @UserId AND o.order_status IN ('pending')";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            PendingOrders.Add(new Order
                            {
                                OrderId = reader.GetInt32(0),
                                OrderType = reader.GetString(1),
                                DineInDate = reader.IsDBNull(2) ? (DateTime?)null : reader.GetDateTime(2),
                                DineInTime = reader.IsDBNull(3) ? (TimeSpan?)null : reader.GetTimeSpan(3),
                                NumberOfPersons = reader.IsDBNull(4) ? (int?)null : reader.GetInt32(4),
                                DeliveryAddress = reader.IsDBNull(5) ? null : reader.GetString(5),
                                Amount = reader.GetDecimal(6),
                                pendingAmount = reader.GetDecimal(7)
                            });
                        }
                    }
                }
            }
        }


    }
}