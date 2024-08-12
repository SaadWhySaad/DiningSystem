using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;

namespace DiningSystem.Pages
{
    public class ReceiptModel : PageModel
    {
        private readonly IConfiguration _configuration;

        public ReceiptModel(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Order Order { get; set; }
        public List<OrderedItem> OrderedItems { get; set; } = new List<OrderedItem>();

        public async Task OnGetAsync(int orderId)
        {
            await LoadOrderAsync(orderId);
            await LoadOrderedItemsAsync(orderId);
        }
        private async Task LoadOrderAsync(int orderId)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                string sql = @"
                    SELECT o.OrderId, u.FirstName + ' ' + u.LastName AS UserName, o.OrderType, 
                           o.DineInDate, o.DineInTime, o.NumberOfPersons, o.DeliveryAddress, o.Amount, o.pending_amount
                    FROM Orders o
                    JOIN AspNetUsers u ON o.UserId = u.Id
                    WHERE o.OrderId = @OrderId";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@OrderId", orderId);
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            Order = new Order
                            {
                                OrderId = reader.GetInt32(0),
                                UserName = reader.GetString(1),
                                OrderType = reader.GetString(2),
                                DineInDate = reader.IsDBNull(3) ? (DateTime?)null : reader.GetDateTime(3),
                                DineInTime = reader.IsDBNull(4) ? (TimeSpan?)null : reader.GetTimeSpan(4),
                                NumberOfPersons = reader.IsDBNull(5) ? (int?)null : reader.GetInt32(5),
                                DeliveryAddress = reader.IsDBNull(6) ? null : reader.GetString(6),
                                Amount = reader.GetDecimal(7),
                                pendingAmount = reader.GetDecimal(8)
                            };
                        }
                    }
                }
            }
        }
        private async Task LoadOrderedItemsAsync(int orderId)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();
                // Fetch items from CartItems based on OrderId
                string sql = @"
                    SELECT rm.menu_item AS ItemName, ci.Quantity, rm.menu_item_price AS Price
                    FROM CartItems ci
                    JOIN restaurantMenu rm ON ci.ItemId = rm.menu_id
                    JOIN Orders o ON o.UserId = ci.UserId
                    WHERE o.OrderId = @OrderId";
                using (SqlCommand command = new SqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@OrderId", orderId);
                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            OrderedItems.Add(new OrderedItem
                            {
                                ItemName = reader.GetString(0),
                                Quantity = reader.GetInt32(1),
                                Price = reader.GetInt32(2)
                            });
                        }
                    }
                }
            }
        }
    }
    public class OrderedItem
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public int Price { get; set; }
    }
}
