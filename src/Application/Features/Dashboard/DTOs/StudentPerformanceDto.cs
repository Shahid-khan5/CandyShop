using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Blazor.Application.Features.Dashboard.DTOs;
public class StudentPerformanceDto
{
    public string StudentName { get; set; }
    public decimal SalesAmount { get; set; }
    public int ProductSold { get; set; }
    public decimal Commission { get; set; }
    public string Performance { get; set; }
}
