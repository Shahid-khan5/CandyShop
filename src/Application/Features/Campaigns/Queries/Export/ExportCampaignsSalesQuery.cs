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

namespace CleanArchitecture.Blazor.Application.Features.Campaigns.Queries.Export;
public class ExportCampaignsSalesQuery : IRequest<Result<byte[]>>
{
    public int CampaignId { get; set; }
}

public class ExportCampaignsSalesQueryHandler : IRequestHandler<ExportCampaignsSalesQuery, Result<byte[]>>
{
    private readonly IApplicationDbContext _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ExportCampaignsSalesQueryHandler(IApplicationDbContext context,IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }
    public record CampaignData(
    int Id,
    string Name,
    string Description,
    DateTime StartDate,
    DateTime EndDate,
    CampaignStatus Status,
    IEnumerable<SaleData> Sales,
    string TopSalesperson
);

    public record SaleData(
        decimal TotalAmount,
        IEnumerable<SaleItemData> SaleItems
    );

    public record SaleItemData(
        int ProductId,
        string ProductName,
        int Quantity,
        decimal TotalPrice,
        decimal Profit,
        decimal UnitPrice
    );
    public async Task<Result<byte[]>> Handle(ExportCampaignsSalesQuery request, CancellationToken cancellationToken)
    {
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        var campaignData =await _context.Campaigns
            .AsNoTracking()
            .Where(c => c.Id == request.CampaignId)
            .Select(c => new CampaignData(
                c.Id,
                c.Name,
                c.Description,
                c.StartDate,
                c.EndDate,
                c.Status,
                c.Sales.Select(s => new SaleData(
                    s.TotalAmount,
                    s.SaleItems.Select(si => new SaleItemData(
                        si.ProductId,
                        si.Product.Name,
                        si.Quantity,
                    si.TotalPrice,
                    si.TotalPrice - (si.Quantity * si.Product.CostPrice),
                    si.UnitPrice
                    )))),
                c.CampaignUsers
                    .OrderByDescending(cu => cu.User.Sales
                        .Where(s => s.CampaignId == c.Id)
                        .Sum(s => s.TotalAmount))
                    .Select(cu => cu.User.UserName)
                    .FirstOrDefault()
            ))
            .FirstOrDefaultAsync(cancellationToken);

        if (campaignData == null)
        {
            throw new NotFoundException(nameof(Campaign), request.CampaignId);
        }

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(20);
                page.Header().Element(ComposeHeader);
                page.Content().Element(element=>ComposeContent(element, campaignData));
                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                });
            });
        });

        return Result<byte[]>.Success(document.GeneratePdf());
    }

    private void ComposeHeader(IContainer container)
    {
        var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, "img", "candylogo.png");
        container.AlignCenter().AlignMiddle().Row(row =>
        {
            row.RelativeItem().PaddingLeft(150).AlignCenter().AlignMiddle().Column(column =>
            {
                column.Item().Text("Campaign Sales Report").FontSize(20).Bold();
                column.Item().Text(DateTime.Now.ToString("d")).FontSize(13);
            });
            row.ConstantItem(150).Height(120).Image(imagePath);
        });
    }

    private void ComposeContent(IContainer container, CampaignData campaignData)
    {
        container.PaddingVertical(10).Column(column =>
        {
            column.Item().Element(compose => ComposeCampaignDetails(compose, campaignData));
            column.Item().Element(compose => ComposeSalesTable(compose, campaignData));
            column.Item().Element(compose => ComposeTopPerformers(compose, campaignData));
            column.Item().Element(compose => ComposeSummary(compose, campaignData));
        });
    }

    private void ComposeCampaignDetails(IContainer container, CampaignData campaignData)
    {
        container.Background(Colors.Grey.Lighten3).Padding(10).Column(column =>
        {
            column.Item().Padding(3).Text("Campaign Details").Bold();
            column.Item().Padding(3).Text($"Name: {campaignData.Name}");
            column.Item().Padding(3).Text($"Description: {campaignData.Description}");
            column.Item().Padding(3).Text($"Start Date: {campaignData.StartDate:d}");
            column.Item().Padding(3).Text($"End Date: {campaignData.EndDate:d}");
            column.Item().Padding(3).Text($"Status: {campaignData.Status}");
        });
    }

    private void ComposeSalesTable(IContainer container, CampaignData campaignData)
    {
        var salesData = campaignData.Sales.SelectMany(s => s.SaleItems)
            .GroupBy(si => new { si.ProductId, si.ProductName })
            .Select(g => new
            {
                ProductName = g.Key.ProductName,
                g.First().UnitPrice,
                Quantity = g.Sum(si => si.Quantity),
                TotalSale = g.Sum(si => si.TotalPrice),
                Commission = g.Sum(si => si.Profit)
            })
            .OrderByDescending(x => x.TotalSale)
            .ToList();

        container.Padding(10).Table(table =>
        {
            table.ColumnsDefinition(columns =>
            {
                columns.ConstantColumn(200);
                columns.ConstantColumn(80);
                columns.ConstantColumn(80);
                columns.ConstantColumn(80);
                columns.ConstantColumn(80);
            });

            table.Header(header =>
            {
                header.Cell().Element(CellStyle).Text("Product Name");
                header.Cell().Element(CellStyle).Text("Quantity");
                header.Cell().Element(CellStyle).Text("UnitPrice");
                header.Cell().Element(CellStyle).Text("Total Sale");
                header.Cell().Element(CellStyle).Text("Commission");

                static IContainer CellStyle(IContainer container)
                {
                    return container.DefaultTextStyle(x => x.SemiBold()).PaddingVertical(5).BorderBottom(1).BorderColor(Colors.Black);
                }
            });

            foreach (var item in salesData)
            {
                table.Cell().Element(CellStyle).Text(item.ProductName);
                table.Cell().Element(CellStyle).Text(item.Quantity.ToString());
                table.Cell().Element(CellStyle).Text($"${item.TotalSale:N2}");
                table.Cell().Element(CellStyle).Text($"${item.UnitPrice:N2}");
                table.Cell().Element(CellStyle).Text($"${item.Commission:N2}");

                static IContainer CellStyle(IContainer container)
                {
                    return container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingVertical(5);
                }
            }
        });
    }

    private void ComposeTopPerformers(IContainer container, CampaignData campaignData)
    {
        var topProduct = campaignData.Sales.SelectMany(s => s.SaleItems)
            .GroupBy(si => new { si.ProductId, si.ProductName })
            .OrderByDescending(g => g.Sum(si => si.Quantity))
            .Select(g => g.Key.ProductName)
            .FirstOrDefault() ?? "N/A";

        container.Background(Colors.Grey.Lighten3).Padding(10).Column(column =>
        {
            column.Item().Text("Top Performers").Bold();
            column.Item().Text($"Top Product: {topProduct}");
            column.Item().Text($"Top Salesperson: {campaignData.TopSalesperson ?? "N/A"}");
        });
    }

    private void ComposeSummary(IContainer container, CampaignData campaignData)
    {
        var totalSales = campaignData.Sales.Sum(s => s.TotalAmount);
        var totalCommission = campaignData.Sales.Sum(s => s.SaleItems.Sum(x => x.Profit));
        var totalQuantity = campaignData.Sales.SelectMany(s => s.SaleItems).Sum(si => si.Quantity);

        container.Background(Colors.Grey.Lighten3).Padding(10).Column(column =>
        {
            column.Item().Text("Summary").Bold();
            column.Item().Text($"Total Sales: ${totalSales:N2}");
            column.Item().Text($"Total Commission: ${totalCommission:N2}");
            column.Item().Text($"Total Products Sold: {totalQuantity}");
        });
    }
}
