using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Blazor.Application.Features.Sales.DTOs;
public class SaleSummaryDto
{
    [Description("Class Name")]
    public string ClassName { get; set; }
    [Description("Admin Name")]
    public string AdminName { get; set; }
    [Description("Total Candies Sold")]
    public int TotalCandiesSold { get; set; } // SaleItems Count
    [Description("Total Commission")]
    public decimal TotalCommission { get; set; } // Sum of (SalePrice - UnitPrice)
    [Description("Total Revenue")]
    public decimal TotalAmount { get; set; } // Sum of Sale Table Amount
    [Description("No of Orders")]
    public int NumberOfOrders { get; set; } // Count of distinct orders
    [Description("TotalCustomers")]
    public int TotalCustomers { get; set; } // Count of distinct customers
}
