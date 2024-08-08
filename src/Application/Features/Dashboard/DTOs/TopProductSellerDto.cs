using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Blazor.Application.Features.Dashboard.DTOs;
public class TopProductSellerDto
{
    public string ProductName { get; set; }
    public int ProductsSold { get; set; }
    public int TotalCapacity { get; set; }
}
