using System.ComponentModel.DataAnnotations;

namespace Garage_3._0.Models
{
    public class StatisticsViewModel
    {
        public List<CustomerRevenue> RevenuePerCustomer { get; set; } = [];
    }

    public class CustomerRevenue(string customerId, double revenueAmount, string firstName, string lastName)
    {
        [Display(Name = "Personal Id")]
        public string CustomerId { get; set; } = customerId;

        [Display(Name = "Revenue Amount")]
        public double RevenueAmount { get; set; } = revenueAmount;

        [Display(Name = "Customer Name")]
        public string CustomerName { get; set; } = string.Format("{0} {1}", firstName, lastName);
    }

}