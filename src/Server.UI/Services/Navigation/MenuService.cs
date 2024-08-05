using CleanArchitecture.Blazor.Infrastructure.Constants.Role;
using CleanArchitecture.Blazor.Server.UI.Models.NavigationMenu;
using RRSTEK.Server.UI.Components;

namespace CleanArchitecture.Blazor.Server.UI.Services.Navigation;


public class MenuService : IMenuService
{
    private readonly List<MenuSectionModel> _features = new()
    {
        new MenuSectionModel
        {
            SectionItems = new List<MenuSectionItemModel>
            {
                new MenuSectionItemModel { Title = "Dashboard", Icon = CustomIcons.Dashboard, Href = "/" },
                new MenuSectionItemModel { Title = "Product", Icon =  CustomIcons.ProductIcon, Href = "/Prduct" },
                new MenuSectionItemModel { Title = "Users", Icon = CustomIcons.User, Href = "/identity/users" },
                new MenuSectionItemModel { Title = "Order", Icon = CustomIcons.Orders,  Href = "/Order" },
                new MenuSectionItemModel { Title = "Request", Icon = CustomIcons.User, Href = "/Request" },
                new MenuSectionItemModel { Title = "Reports", Icon = CustomIcons.User, Href = "/Reports" },
                new MenuSectionItemModel { Title = "Setting", Icon = CustomIcons.setting, Href = "/Setting" },
                new MenuSectionItemModel { Title = "Logout", Icon = CustomIcons.Logout, Href = "/pages/authentication/login" },
            }
        }
    };

    public IEnumerable<MenuSectionModel> Features => _features;
}

