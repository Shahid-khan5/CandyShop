using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Blazor.Application.Features.Dashboard.DTOs;
public class CampaignRevenueDto
{
    public int CampaignId { get; set; }
    public string CampaignName { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalCommission { get; set; }
}
