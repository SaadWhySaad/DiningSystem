using DiningSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data.SqlClient;

namespace DiningSystem.Pages
{
    public class OrderHistoryModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public List<Order> listOrder = new List<Order>();

        // Property to hold grouped orders
        public Dictionary<int, List<Order>> GroupedOrders { get; set; } = new Dictionary<int, List<Order>>();

        // Dictionary to track reviewed restaurants
        public Dictionary<int, bool> ReviewedRestaurants { get; set; } = new Dictionary<int, bool>();

        public OrderHistoryModel(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public void OnGet()
        {
            LoadOrderHistory();
        }

        public IActionResult OnPost()
        {
            var form = Request.Form;
            int restaurantId = int.Parse(form["RestaurantId"]);
            string userReview = form["UserReview"];

            if (string.IsNullOrEmpty(userReview))
            {
                ModelState.AddModelError(string.Empty, "Review cannot be empty.");
                LoadOrderHistory();
                return Page();
            }

            string userId = _userManager.GetUserId(User);
            string userName = _userManager.GetUserName(User);

            string connection = _configuration.GetConnectionString("DefaultConnection");

            try
            {
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    conn.Open();
                    string sql = @"
            INSERT INTO Reviews (userId, r_id, userName, restaurant_name, user_review) 
            VALUES (@userId, @r_id, @userName, 
                    (SELECT r_name FROM Restaurant WHERE r_id = @r_id), @user_review)";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.Parameters.AddWithValue("@r_id", restaurantId);
                        cmd.Parameters.AddWithValue("@userName", userName);
                        cmd.Parameters.AddWithValue("@user_review", userReview);

                        cmd.ExecuteNonQuery();
                    }
                }

                // Mark the restaurant as reviewed
                ReviewedRestaurants[restaurantId] = true;

                // Refresh the page after submitting the review
                return RedirectToPage("/OrderHistory");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return Page();
            }
        }

        private void LoadOrderHistory()
        {
            try
            {
                string userId = _userManager.GetUserId(User);
                string connection = _configuration.GetConnectionString("DefaultConnection");

                using (SqlConnection conn = new SqlConnection(connection))
                {
                    conn.Open();
                    string sql = @"
                    SELECT o.OrderId, o.UserId, o.OrderType, o.DineInDate, o.DineInTime, 
                           o.NumberOfPersons, o.DeliveryAddress, o.CardNumber, o.ExpirationDate, 
                           o.CVV, o.Amount, o.order_status, o.r_id, r.r_name
                    FROM Orders o
                    JOIN Restaurant r ON o.r_id = r.r_id
                    WHERE o.UserId = @userId AND (o.order_status = 'completed' OR o.order_status = 'cancelled')";

                    using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Order order = new Order
                                {
                                    OrderId = reader.GetInt32(0),
                                    UserId = reader.GetString(1),
                                    OrderType = reader.GetString(2),
                                    DineInDate = reader.IsDBNull(3) ? (DateTime?)null : reader.GetDateTime(3),
                                    DineInTime = reader.IsDBNull(4) ? (TimeSpan?)null : reader.GetTimeSpan(4),
                                    NumberOfPersons = reader.IsDBNull(5) ? (int?)null : reader.GetInt32(5),
                                    DeliveryAddress = reader.IsDBNull(6) ? null : reader.GetString(6),
                                    CardNumber = reader.GetString(7),
                                    ExpirationDate = reader.GetString(8),
                                    CVV = reader.GetString(9),
                                    Amount = reader.GetDecimal(10),
                                    order_status = reader.GetString(11),
                                    RestaurantId = reader.GetInt32(12),
                                    RestaurantName = reader.GetString(13)  // Adding restaurant name
                                };

                                listOrder.Add(order);
                            }
                        }
                    }

                    // Group the orders by RestaurantId
                    GroupedOrders = listOrder.GroupBy(o => o.RestaurantId)
                                             .ToDictionary(g => g.Key, g => g.ToList());

                    // Check if the user has reviewed each restaurant
                    foreach (var orderGroup in GroupedOrders)
                    {
                        string checkReviewSql = @"
                            SELECT COUNT(1)
                            FROM Reviews
                            WHERE userId = @userId AND r_id = @r_id";

                        using (SqlCommand checkReviewCmd = new SqlCommand(checkReviewSql, conn))
                        {
                            checkReviewCmd.Parameters.AddWithValue("@userId", userId);
                            checkReviewCmd.Parameters.AddWithValue("@r_id", orderGroup.Key);

                            int reviewCount = (int)checkReviewCmd.ExecuteScalar();
                            ReviewedRestaurants[orderGroup.Key] = reviewCount > 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
