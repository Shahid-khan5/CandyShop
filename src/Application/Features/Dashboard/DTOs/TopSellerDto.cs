using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Blazor.Application.Features.Dashboard.DTOs;
public class TopSellerDto
{
    public string StudentName { get; set; }
    public decimal TotalSales { get; set; }
    public decimal TotalCommission { get; set; }
}
