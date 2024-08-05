using System.Diagnostics;
using System.Reflection;
using Bogus;
using CleanArchitecture.Blazor.Domain.Identity;
using CleanArchitecture.Blazor.Infrastructure.Constants.ClaimTypes;
using CleanArchitecture.Blazor.Infrastructure.Constants.Role;
using CleanArchitecture.Blazor.Infrastructure.Constants.User;
using CleanArchitecture.Blazor.Infrastructure.PermissionSet;

namespace CleanArchitecture.Blazor.Infrastructure.Persistence;

public class ApplicationDbContextInitializer
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ApplicationDbContextInitializer> _logger;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public ApplicationDbContextInitializer(ILogger<ApplicationDbContextInitializer> logger,
        ApplicationDbContext context, UserManager<ApplicationUser> userManager,
        RoleManager<ApplicationRole> roleManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task InitialiseAsync()
    {
        try
        {
            if (_context.Database.IsSqlServer() || _context.Database.IsNpgsql() || _context.Database.IsSqlite())
                await _context.Database.MigrateAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
            _context.ChangeTracker.Clear();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }

    private static IEnumerable<string> GetAllPermissions()
    {
        var allPermissions = new List<string>();
        var modules = typeof(Permissions).GetNestedTypes();

        foreach (var module in modules)
        {
            var moduleName = string.Empty;
            var moduleDescription = string.Empty;

            var fields = module.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            foreach (var fi in fields)
            {
                var propertyValue = fi.GetValue(null);

                if (propertyValue is not null)
                    allPermissions.Add((string)propertyValue);
            }
        }

        return allPermissions;
    }

    private async Task TrySeedAsync()
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Default tenants
            if (!_context.Tenants.Any())
            {
                _context.Tenants.Add(new Tenant { Name = "Super Admin", Description = "Super Admin" });
                _context.Tenants.Add(new Tenant { Name = "Class A", Description = "Class A" });
                await _context.SaveChangesAsync();
            }

            // Default roles
            var superAdmin = new ApplicationRole(RoleName.SuperAdmin) { Description = "Super Admin", TenantId = _context.Tenants.First().Id };
            var admin = new ApplicationRole(RoleName.Admin) { Description = "Class/Team Admin", TenantId = _context.Tenants.First().Id };
            var student = new ApplicationRole(RoleName.Users) { Description = "User", TenantId = _context.Tenants.First().Id };
            var permissions = GetAllPermissions();
            if (_roleManager.Roles.All(r => r.Name != superAdmin.Name))
            {
                await _roleManager.CreateAsync(superAdmin);

                foreach (var permission in permissions)
                    await _roleManager.AddClaimAsync(superAdmin,
                        new Claim(ApplicationClaimTypes.Permission, permission));
            }

            if (_roleManager.Roles.All(r => r.Name != admin.Name))
            {
                await _roleManager.CreateAsync(admin);
                foreach (var permission in permissions)
                    if (!permission.StartsWith("Permissions.Products"))
                        await _roleManager.AddClaimAsync(admin, new Claim(ApplicationClaimTypes.Permission, permission));
            }
            if (_roleManager.Roles.All(r => r.Name != student.Name))
            {
                await _roleManager.CreateAsync(student);
                foreach (var permission in permissions)
                    if (!permission.StartsWith("Permissions.Products"))
                        await _roleManager.AddClaimAsync(student, new Claim(ApplicationClaimTypes.Permission, permission));
            }

            // Default users
            var administrator = new ApplicationUser
            {
                UserName = UserName.Administrator,
                Provider = "Local",
                IsActive = true,
                TenantId = _context.Tenants.First().Id,
                DisplayName = UserName.Administrator, Email = "new163@163.com", EmailConfirmed = true,
                ProfilePictureDataUrl = "https://s.gravatar.com/avatar/78be68221020124c23c665ac54e07074?s=80",
                TwoFactorEnabled = false
            };
            var demo = new ApplicationUser
            {
                UserName = UserName.Demo,
                IsActive = true,
                Provider = "Local",
                TenantId = _context.Tenants.Skip(1).First().Id,
                DisplayName = UserName.Demo, Email = "neozhu@126.com",
                EmailConfirmed = true,
                ProfilePictureDataUrl = "https://s.gravatar.com/avatar/ea753b0b0f357a41491408307ade445e?s=80"
            };

            if (_userManager.Users.All(u => u.UserName != administrator.UserName))
            {
                await _userManager.CreateAsync(administrator, UserName.DefaultPassword);
                await _userManager.AddToRolesAsync(administrator, new[] { superAdmin.Name! });
                //await _userManager.SetTwoFactorEnabledAsync(administrator, true);
            }

            if (_userManager.Users.All(u => u.UserName != demo.UserName))
            {
                await _userManager.CreateAsync(demo, UserName.DefaultPassword);
                await _userManager.AddToRolesAsync(demo, new[] { admin.Name! });
            }

            if (!_context.Campaigns.Any())
            {
                // Generate dummy data
                var random = new Random();
                var userFaker = new Faker<ApplicationUser>()
                    .RuleFor(u => u.UserName, f => f.Internet.UserName())
                    .RuleFor(u => u.Email, f => f.Internet.Email())
                    .RuleFor(u => u.DisplayName, f => f.Name.FullName())
                    .RuleFor(u => u.EmailConfirmed, f => true)
                    .RuleFor(u => u.ProfilePictureDataUrl, f => f.Internet.Avatar())
                    .RuleFor(u => u.TwoFactorEnabled, f => false)
                    .RuleFor(u => u.IsActive, f => true)
                    .RuleFor(u => u.Provider, f => "Local");

                var campaignFaker = new Faker<Campaign>()
                    .RuleFor(c => c.Name, f => f.Company.CompanyName())
                    .RuleFor(c => c.Description, f => f.Lorem.Paragraph())
                    .RuleFor(c => c.StartDate, f => f.Date.Past())
                    .RuleFor(c => c.EndDate, f => f.Date.Future())
                    .RuleFor(c => c.Status, f => f.PickRandom<CompaignStatus>())
                    .RuleFor(c => c.CampaignUsers, f => new List<CampaignUser>())
                    .RuleFor(c => c.Sales, f => new List<Sale>());

                var candyFaker = new Faker<Product>()
                    .RuleFor(p => p.Name, f => f.Commerce.ProductName())
                    .RuleFor(p => p.Description, f => f.Commerce.ProductDescription())
                    .RuleFor(p => p.Brand, f => f.Company.CompanyName())
                    .RuleFor(p => p.Unit, f => "EA")
                    .RuleFor(p => p.Price, f => f.Random.Decimal(1, 20))
                    .RuleFor(p => p.Stock, f => f.Random.Int(0, 100))
                    .RuleFor(p => p.SaleItems, f => new List<SaleItem>())
                    .RuleFor(p => p.Pictures, f => new List<ProductImage>());

                var saleFaker = new Faker<Sale>()
                    .RuleFor(s => s.CustomerName, f => f.Name.FullName())
                    .RuleFor(s => s.CustomerEmail, f => f.Internet.Email())
                    .RuleFor(s => s.TotalAmount, f => 0) // Will be updated later
                    .RuleFor(s => s.SaleDate, f => f.Date.Recent())
                    .RuleFor(s => s.Created, f => DateTime.Now)
                    .RuleFor(s => s.CreatedBy, f => f.Internet.UserName())
                    .RuleFor(s => s.LastModified, f => DateTime.Now)
                    .RuleFor(s => s.LastModifiedBy, f => f.Internet.UserName())
                    .RuleFor(s => s.SaleItems, f => new List<SaleItem>());

                var saleItemFaker = new Faker<SaleItem>()
                    .RuleFor(si => si.Quantity, f => f.Random.Int(1, 10))
                    .RuleFor(si => si.UnitPrice, f => 0) // Will be updated later
                    .RuleFor(si => si.TotalPrice, f => 0); // Will be updated later

                // Create additional admins and campaigns
                var adminUsers = userFaker.Generate(50).DistinctBy(x=>x.NormalizedUserName).ToList();
                var campaigns = campaignFaker.Generate(50);
                var candyProducts = candyFaker.Generate(50); // Generate 10 candy products

                _context.Products.AddRange(candyProducts);
                await _context.SaveChangesAsync();

                for (int i = 0; i < adminUsers.Count; i++)
                {
                    var adminUser = adminUsers[i];
                    adminUser.TenantId = _context.Tenants.First().Id;
                    await _userManager.CreateAsync(adminUser, UserName.DefaultPassword);
                    await _userManager.AddToRolesAsync(adminUser, new[] { admin.Name! });

                    var campaign = campaigns[i];
                    campaign.CampaignUsers.Add(new CampaignUser { UserId = adminUser.Id, Campaign = campaign });

                    if (campaign.Status == CompaignStatus.InProgress || campaign.Status == CompaignStatus.Ended)
                    {
                        var users = userFaker.Generate(random.Next(1, 5));
                        foreach (var user in users)
                        {
                            user.TenantId = _context.Tenants.First().Id;
                            await _userManager.CreateAsync(user, UserName.DefaultPassword);
                            await _userManager.AddToRolesAsync(user, new[] { student.Name! });

                            var campaignUser = new CampaignUser { UserId = user.Id, Campaign = campaign };
                            campaign.CampaignUsers.Add(campaignUser);

                            var sales = saleFaker.Generate(random.Next(1, 3));
                            campaign.Sales = new List<Sale>();
                            foreach (var sale in sales)
                            {
                                sale.UserId = user.Id;
                                sale.CampaignId = campaign.Id;

                                var saleItems = saleItemFaker.Generate(random.Next(1, 5));
                                foreach (var saleItem in saleItems)
                                {
                                    saleItem.ProductId = candyProducts[random.Next(candyProducts.Count)].Id;
                                    saleItem.Sale = sale;
                                    saleItem.UnitPrice = candyProducts.First(p => p.Id == saleItem.ProductId).Price;
                                    saleItem.TotalPrice = saleItem.Quantity * saleItem.UnitPrice;
                                    sale.SaleItems.Add(saleItem);
                                }

                                sale.TotalAmount = sale.SaleItems.Sum(si => si.TotalPrice);
                                campaign.Sales.Add(sale);
                            }
                        }
                    }
                    _context.Campaigns.Add(campaign);
                }
            }
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            await transaction.RollbackAsync();
        }
    }

}