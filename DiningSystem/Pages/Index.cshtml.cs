using DiningSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Data;
using System.Data.SqlClient;

namespace DiningSystem.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IConfiguration _configuration;
        public List<Review> listReview = new List<Review>();

        public IndexModel(ILogger<IndexModel> logger, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
            this.userManager = userManager;
            listReview = new List<Review>();
        }

        public void OnGet()
        {
            try
            {
                string connection = _configuration.GetConnectionString("DefaultConnection");
                using (SqlConnection conn = new SqlConnection(connection))
                {
                    conn.Open();
                    string sql = @"
                                SELECT 
                                    r.review_id,
                                    u.LastName,
                                    rest.r_name,
                                    r.user_review
                                FROM 
                                    Reviews r
                                JOIN 
                                    AspNetUsers u ON r.userId = u.Id
                                JOIN 
                                    Restaurant rest ON r.r_id = rest.r_id";
					using (SqlCommand cmd = new SqlCommand(sql, conn))
                    {
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Review review = new Review();
                                review.review_id = reader.GetInt32(0);
								review.user_name = reader.GetString(1);
								review.restaurant_name = reader.GetString(2);
								review.user_review = reader.GetString(3);
								listReview.Add(review);
							}
                        }
                    }
                }
            }
            catch(Exception ex) {
                Console.WriteLine(ex.ToString());
            }
        }
    }

    public class Review
    {


        public int review_id {  get; set; }
        public string userId { get; set; }
        public int r_id {  get; set; }
		public string user_name { get; set; }
		public string restaurant_name { get; set; }
		public string user_review { get; set; }
	}
}
