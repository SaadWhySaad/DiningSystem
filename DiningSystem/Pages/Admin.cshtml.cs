    using DiningSystem.Models;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.RazorPages;
    using System.Data.SqlClient;

    namespace DiningSystem.Pages
    {
    [Authorize(Roles = "admin")]
    public class AdminModel : PageModel
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminModel(IConfiguration configuration, UserManager<ApplicationUser> userManager)
        {
            _configuration = configuration;
            _userManager = userManager;
        }

        public List<Order> Orders { get; set; } = new List<Order>();

        public async Task OnGet()
        {
            if (!User.Identity.IsAuthenticated)
            {
                Response.Redirect("/Account/Login");
            }
            string adminId = _userManager.GetUserId(User);
            await LoadOrdersAsync(adminId);
        }

        public async Task<IActionResult> OnPostUpdateOrderStatusAsync([FromBody] UpdateOrderStatusModel model)
        {
            if (model == null || !ModelState.IsValid)
            {
                return new JsonResult(new { success = false });
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string sql = "UPDATE Orders SET order_status = @Status WHERE orderId = @OrderId";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@OrderId", model.OrderId);
                    command.Parameters.AddWithValue("@Status", model.Status);
                    await command.ExecuteNonQueryAsync();
                }
            }
            return new JsonResult(new { success = true });
        }

        public async Task<IActionResult> OnPostEditOrderAsync([FromBody] EditOrderModel model)
        {
            if (model == null || !ModelState.IsValid)
            {
                return new JsonResult(new { success = false });
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string sql = @"
                    UPDATE Orders
                    SET DineInDate = @DineInDate, DineInTime = @DineInTime, NumberOfPersons = @NumberOfPersons
                    WHERE orderId = @OrderId";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@OrderId", model.OrderId);
                    command.Parameters.AddWithValue("@DineInDate", model.DineInDate);
                    command.Parameters.AddWithValue("@DineInTime", model.DineInTime);
                    command.Parameters.AddWithValue("@NumberOfPersons", model.NumberOfPersons);
                    await command.ExecuteNonQueryAsync();
                }
            }
            return new JsonResult(new { success = true });
        }

        private async Task LoadOrdersAsync(string adminId)
        {
            Orders = new List<Order>();
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string sql = @"
            SELECT o.orderId, o.UserId, u.FirstName + ' ' + u.LastName AS UserName, o.OrderType, 
                   o.DineInDate, o.DineInTime, o.NumberOfPersons, o.DeliveryAddress, o.CardNumber, 
                   o.ExpirationDate, o.CVV, o.Amount, o.order_status, o.r_id
            FROM Orders o
            JOIN Restaurant r ON o.r_id = r.r_id
            JOIN AspNetUsers u ON o.UserId = u.Id
            WHERE r.r_admin = @AdminId";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@AdminId", adminId);
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Orders.Add(new Order
                            {
                                OrderId = reader.GetInt32(0),
                                UserId = reader.GetString(1),
                                UserName = reader.GetString(2),
                                OrderType = reader.GetString(3),
                                DineInDate = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
                                DineInTime = reader.IsDBNull(5) ? (TimeSpan?)null : reader.GetTimeSpan(5),
                                NumberOfPersons = reader.IsDBNull(6) ? (int?)null : reader.GetInt32(6),
                                DeliveryAddress = reader.IsDBNull(7) ? null : reader.GetString(7),
                                CardNumber = reader.GetString(8),
                                ExpirationDate = reader.GetString(9),
                                CVV = reader.GetString(10),
                                Amount = reader.GetDecimal(11), // Ensure the Amount is being read correctly
                                OrderStatus = reader.GetString(12),
                                RestaurantId = reader.GetInt32(13)
                            });
                        }
                    }
                }
            }
        }

        public class UpdateOrderStatusModel
        {
            public int OrderId { get; set; }
            public string Status { get; set; }
        }

        public class EditOrderModel
        {
            public int OrderId { get; set; }
            public DateTime DineInDate { get; set; }
            public TimeSpan DineInTime { get; set; }
            public int NumberOfPersons { get; set; }
        }
    }

    public class Order
    {
        public int OrderId { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; } // New property
        public string OrderType { get; set; }
        public DateTime? DineInDate { get; set; }
        public TimeSpan? DineInTime { get; set; }
        public int? NumberOfPersons { get; set; }
        public string? DeliveryAddress { get; set; }
        public string CardNumber { get; set; }
        public string ExpirationDate { get; set; }
        public string CVV { get; set; }
        public decimal Amount { get; set; }
        public string OrderStatus { get; set; }
        public int RestaurantId { get; set; } // r_id
    }

}



    