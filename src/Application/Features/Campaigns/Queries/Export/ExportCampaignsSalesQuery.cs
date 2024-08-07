using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using QuestPDF;
using QuestPDF.Fluent;
using Microsoft.EntityFrameworkCore;
using DocumentFormat.OpenXml.InkML;
using QuestPDF.Helpers;
using Document = QuestPDF.Fluent.Document;
using IContainer = QuestPDF.Infrastructure.IContainer;
using AutoMapper;
using Microsoft.AspNetCore.Hosting;
using CleanArchitecture.Blazor.Domain.Entities;
using QuestPDF.Infrastructure;

namespace CleanArchitecture.Blazor.Application.Features.Campaigns.Queries.Export;
public class ExportCampaignsSalesQuery : IRequest<Result<byte[]>>
{
    public int CampaignId { get; set; }
}
public class CampaignSalesReport : IDocument
{
    private readonly Campaign _campaign;

    public CampaignSalesReport(Campaign campaign)
    {
        _campaign = campaign;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container
            .Page(page =>
            {
                page.Margin(10);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontFamily("Arial"));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
    }

    void ComposeHeader(IContainer container)
    {
        var titleStyle = TextStyle.Default.FontSize(24).SemiBold().FontColor(Colors.White);
        var subtitleStyle = TextStyle.Default.FontSize(14).FontColor(Colors.White);

        container.Background(Colors.Blue.Medium)
            .Padding(20)
            .Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("Campaign Sales Report").Style(titleStyle);
                    column.Item().Text($"Generated on: {DateTime.Now:MMMM d, yyyy}").Style(subtitleStyle);
                });
            });
    }

    void ComposeContent(IContainer container)
    {
        container.PaddingVertical(20).Column(column =>
        {
            column.Spacing(20);

            column.Item().Element(ComposeCampaignDetails);
            column.Item().Element(ComposeProductSales);
            column.Item().Element(ComposeTopPerformers);
            column.Item().Element(ComposeSummary);
        });
    }

    void ComposeCampaignDetails(IContainer container)
    {
      
        

        container.Background(Colors.Grey.Lighten3)
            .Padding(10)
            .Column(column =>
            {
                column.Item().Text("Campaign Details").FontSize(18).SemiBold();
                column.Item().Text($"Name: {_campaign.Name}");
                column.Item().Text($"Start Date: {_campaign.StartDate:yyyy-MM-dd} | End Date: {_campaign.EndDate:yyyy-MM-dd} | Status: {_campaign.Status}");
            });
    }

    void ComposeProductSales(IContainer container)
    {
        var sales = _campaign.Sales;

        var groupedSales = sales
            .SelectMany(s => s.SaleItems)
            .GroupBy(si => new { si.Product.Id, si.Product.Name })
            .OrderByDescending(g => g.Sum(si => si.TotalPrice))
            .ToList();

        container.Background(Colors.Grey.Lighten4)
              .Padding(10)
              .Column(column =>
              {
              foreach (var productGroup in groupedSales)
        {
          
                    column.Item().Text(productGroup.Key.Name).FontSize(14).SemiBold().FontColor("#2c3e50");

                    var sellerSales = productGroup
                        .GroupBy(si => si.Sale.User)
                        .OrderByDescending(g => g.Sum(si => si.TotalPrice));

                    foreach (var sellerGroup in sellerSales)
                    {
                        column.Item().PaddingVertical(5).PaddingHorizontal(010).Text($"Seller: {sellerGroup.Key.UserName}").FontSize(13).Italic().FontColor("#34495e");

                        column.Item().PaddingHorizontal(10).PaddingBottom(010).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Quantity");
                                header.Cell().Text("Unit Price");
                                header.Cell().Text("Total Sale");
                                header.Cell().Text("Commission");
                                header.Cell().Text("Customer");
                            });

                            foreach (var sale in sellerGroup)
                            {
                                decimal profit = sale.TotalPrice - (sale.Product.CostPrice * sale.Quantity);
                                table.Cell().Text(sale.Quantity.ToString());
                                table.Cell().Text($"${sale.UnitPrice:F2}");
                                table.Cell().Text($"${sale.TotalPrice:F2}");
                                table.Cell().Text($"${profit:F2}"); // Assuming 32.5% commission
                                table.Cell().Text(sale.Sale.CustomerName);
                            }
                        });
                    }
        }
                });
    }

    void ComposeTopPerformers(IContainer container)
    {
        var topProduct = _campaign.Sales.SelectMany(x=>x.SaleItems)
            .GroupBy(si => si.Product)
            .OrderByDescending(g => g.Sum(si => si.TotalPrice))
            .Select(g => new
            {
                Product = g.Key,
                TotalSold = g.Sum(si => si.Quantity),
                TotalSales = g.Sum(si => si.TotalPrice)
            })
            .FirstOrDefault();

        var topSeller = _campaign.Sales
            .GroupBy(s => s.User)
            .OrderByDescending(g => g.Sum(s => s.TotalAmount))
            .Select(g => new
            {
                Seller = g.Key,
                TotalSales = g.Sum(s => s.TotalAmount),
                Commission = g.Sum(s => s.SaleItems.Sum(si => si.TotalPrice - (si.Product.CostPrice * si.Quantity))),
            })
            .FirstOrDefault();

        container.Background(Colors.Green.Lighten4)
            .Padding(10)
            .Column(column =>
            {
                column.Item().Text("Top Performers").FontSize(18).SemiBold().FontColor("#16a085");
                if (topProduct != null)
                {
                    column.Item().Text($"Top Product: {topProduct.Product.Name} ({topProduct.TotalSold} sold, ${topProduct.TotalSales:F2} in sales)").FontColor("#16a085");
                }
                if (topSeller != null)
                {
                    column.Item().Text($"Top Seller: {topSeller.Seller.UserName} (${topSeller.TotalSales:F2} in sales, ${topSeller.Commission:F2} commission)").FontColor("#16a085");
                }
            });
    }

    void ComposeSummary(IContainer container)
    {
        var totalSales = _campaign.Sales.Sum(s => s.TotalAmount);
        var totalCommision = _campaign.Sales.Sum(s => s.SaleItems.Sum(si => si.TotalPrice - (si.Product.CostPrice * si.Quantity)));
        var totalProductsSold = _campaign.Sales.Sum(s => s.SaleItems.Sum(si => si.Quantity));
        var uniqueCustomers = _campaign.Sales.Select(s => s.CustomerEmail).Distinct().Count();
        var averageSales = uniqueCustomers == 0 ? 0 : (totalSales / uniqueCustomers);
        var averageCommision = totalProductsSold == 0 ? 0 : (totalCommision / totalProductsSold);

        container.Background(Colors.Green.Lighten5)
            .Padding(10)
            .Column(column =>
            {
                column.Item().Text("Summary").FontSize(18).SemiBold().FontColor("#27ae60");
                    column.Item().Text($"Total Sales: ${totalSales:F2}").FontColor("#27ae60");
                    column.Item().Text($"Total Commission: ${totalCommision:F2}").FontColor("#27ae60");
                    column.Item().Text($"Total Products Sold: {totalProductsSold}").FontColor("#27ae60");
                    column.Item().Text($"Average Sale per Customer: ${averageSales:F2}").FontColor("#27ae60");
                    column.Item().Text($"Average Commission per Sale: ${averageCommision:F2}").FontColor("#27ae60");
                    column.Item().Text($"Number of Unique Customers: {uniqueCustomers}").FontColor("#27ae60");
            });
    }

    void ComposeFooter(IContainer container)
    {
        container.AlignCenter()
            .Text(x =>
            {
                x.Span("Page ");
                x.CurrentPageNumber();
                x.Span(" of ");
                x.TotalPages();
            });
    }
}

public class ExportCampaignsSalesQueryHandler : IRequestHandler<ExportCampaignsSalesQuery, Result<byte[]>>
{
    private readonly IApplicationDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ExportCampaignsSalesQueryHandler(IApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }
  

    public async Task<Result<byte[]>> Handle(ExportCampaignsSalesQuery request, CancellationToken cancellationToken)
    {
        var campaign = await _context.Campaigns
           .Include(c => c.Sales)
           .ThenInclude(s => s.SaleItems)
           .ThenInclude(si => si.Product)
           .Include(c => c.Sales)
           .ThenInclude(x=>x.User)
           .FirstOrDefaultAsync(c => c.Id == request.CampaignId);
        QuestPDF.Settings.License = LicenseType.Community;
        var data = new CampaignSalesReport(campaign).GeneratePdf();
        return Result<byte[]>.Success(data);
    }


}