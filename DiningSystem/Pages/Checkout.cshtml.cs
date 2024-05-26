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
        [Required(ErrorMessage = "Please enter the expiration date.")]
        [RegularExpression(@"^(0[1-9]|1[0-2])\/?([0-9]{2})$", ErrorMessage = "Please enter a valid expiration date (MM/YY).")]
        public string ExpirationDate { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Please enter the CVV.")]
        [RegularExpression(@"^[0-9]{3,4}$", ErrorMessage = "Please enter a valid CVV.")]
        public string CVV { get; set; }





        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                foreach (var error in errors)
                {
                    Debug.WriteLine(error);
                }
                return Page();
            }

            // Get the user ID
            string userId = _userManager.GetUserId(User);

            // Process the order
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string sql = @"
                    INSERT INTO Orders (UserId, OrderType, DineInDate, DineInTime, NumberOfPersons, DeliveryAddress, CardNumber, ExpirationDate, CVV, order_status)
                    VALUES (@UserId, @OrderType, @DineInDate, @DineInTime, @NumberOfPersons, @DeliveryAddress, @CardNumber, @ExpirationDate, @CVV, @order_status)";

                using (SqlCommand command = new SqlCommand(sql, connection))
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
                    command.ExecuteNonQuery();
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
