using DiningSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Stripe;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Security.Claims;

namespace DiningSystem.Pages
{
    public class CheckoutModel : PageModel
    {
        

        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;

        public CheckoutModel(IConfiguration configuration, UserManager<ApplicationUser> userManager)
        {
            _configuration = configuration;
            _userManager = userManager;
        }

        [BindProperty]
        [Required(ErrorMessage = "Please choose an option.")]
        public string CheckoutOption { get; set; }

        [BindProperty]
        [DataType(DataType.Date)]
        [RequiredIf(nameof(CheckoutOption), "DineIn", ErrorMessage = "Please select a date.")]
        public DateTime? DineInDate { get; set; }

        [BindProperty]
        [DataType(DataType.Time)]
        [RequiredIf(nameof(CheckoutOption), "DineIn", ErrorMessage = "Please select a time.")]
        public TimeSpan? DineInTime { get; set; }

        [BindProperty]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter a valid number of persons.")]
        [RequiredIf(nameof(CheckoutOption), "DineIn", ErrorMessage = "Please enter the number of persons.")]
        public int? NumberOfPersons { get; set; }

        [BindProperty]
        [RequiredIf(nameof(CheckoutOption), "Delivery", ErrorMessage = "Please enter a delivery address.")]
        public string? DeliveryAddress { get; set; }

        [BindProperty]
        [CreditCard(ErrorMessage = "Please enter a valid card number.")]
        [Required(ErrorMessage = "Please enter your card number.")]
        public string CardNumber { get; set; }

        [BindProperty]
        [Required]
        [RegularExpression(@"\d{2}/\d{2}", ErrorMessage = "Expiration date must be in MM/YY format")]
        [Display(Name = "Expiration Date")]
        public string ExpirationDate { get; set; }

        [BindProperty]
        [Required]
        [RegularExpression(@"\d{3}", ErrorMessage = "CVV must be a 3-digit number")]
        [Display(Name = "CVV")]
        public string CVV { get; set; }





        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Get the user ID
            string userId = _userManager.GetUserId(User);

            // Process the order
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string getRestaurantIdSql = @"
                SELECT DISTINCT r_id 
                FROM restaurantMenu 
                WHERE menu_id IN (SELECT ItemId FROM cartitems WHERE UserId = @UserId)";
                int? restaurantId = null;
                using (SqlCommand getRestaurantIdCommand = new SqlCommand(getRestaurantIdSql, connection))
                {
                    getRestaurantIdCommand.Parameters.AddWithValue("@UserId", userId);
                    using (SqlDataReader reader = getRestaurantIdCommand.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            restaurantId = reader.GetInt32(0);
                        }
                    }
                }

                if (restaurantId == null)
                {
                    // Handle the case where no restaurant ID could be found (e.g., return an error)
                    ModelState.AddModelError(string.Empty, "Unable to determine the restaurant for your order.");
                    return Page();
                }
                string insertOrderSql = @"
                INSERT INTO Orders (UserId, OrderType, DineInDate, DineInTime, NumberOfPersons, DeliveryAddress, CardNumber, ExpirationDate, CVV, order_status, r_id)
                VALUES (@UserId, @OrderType, @DineInDate, @DineInTime, @NumberOfPersons, @DeliveryAddress, @CardNumber, @ExpirationDate, @CVV, @order_status, @RestaurantId)";

                using (SqlCommand command = new SqlCommand(insertOrderSql, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);
                    command.Parameters.AddWithValue("@OrderType", CheckoutOption);
                    command.Parameters.AddWithValue("@DineInDate", (object)DineInDate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@DineInTime", (object)DineInTime ?? DBNull.Value);
                    command.Parameters.AddWithValue("@NumberOfPersons", (object)NumberOfPersons ?? DBNull.Value);
                    command.Parameters.AddWithValue("@DeliveryAddress", (object)DeliveryAddress ?? DBNull.Value);
                    command.Parameters.AddWithValue("@CardNumber", CardNumber);
                    command.Parameters.AddWithValue("@ExpirationDate", ExpirationDate);
                    command.Parameters.AddWithValue("@CVV", CVV);
                    command.Parameters.AddWithValue("@order_status", "pending");
                    command.Parameters.AddWithValue("@RestaurantId", restaurantId);
                    command.ExecuteNonQuery();
                }
                string deleteCartItemsSql = "DELETE FROM cartitems WHERE UserId = @UserId";
                using (SqlCommand deleteCommand = new SqlCommand(deleteCartItemsSql, connection))
                {
                    deleteCommand.Parameters.AddWithValue("@UserId", userId);
                    deleteCommand.ExecuteNonQuery();
                }
            }

            // Redirect to a confirmation page or display a success message
            return RedirectToPage("/OrderConfirmation");
        }






        public void OnGet()
        {
        }





    }

    public class RequiredIfAttribute : ValidationAttribute
    {
        private readonly string _propertyName;
        private readonly string _desiredValue;

        public RequiredIfAttribute(string propertyName, string desiredValue)
        {
            _propertyName = propertyName;
            _desiredValue = desiredValue;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var property = validationContext.ObjectType.GetProperty(_propertyName);
            if (property == null)
                return new ValidationResult($"Unknown property: {_propertyName}");

            var propertyValue = property.GetValue(validationContext.ObjectInstance)?.ToString();
            if (propertyValue == _desiredValue && (value == null || string.IsNullOrEmpty(value.ToString())))
            {
                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;


        }
    }
}
