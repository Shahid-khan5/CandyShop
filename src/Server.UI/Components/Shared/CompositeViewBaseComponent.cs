namespace CleanArchitecture.Blazor.Server.UI.Components.Shared;
public class CompositeViewBaseComponent : ComponentBase
{
    [Parameter]
    public RenderFragment HeaderView { get; set; }

    [Parameter]
    public RenderFragment ChildContent { get; set; }

    [Parameter]
    public RenderFragment BusyView { get; set; }

    [Parameter]
    public RenderFragment FooterView { get; set; }

    [Parameter]
    public bool IsBusy { get; set; }

    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        base.BuildRenderTree(builder);

        builder.AddContent(0, HeaderView);

        if (IsBusy)
        {
            builder.AddContent(1, BusyView);
        }
        else
        {
            builder.AddContent(2, ChildContent);
        }

        builder.AddContent(3, FooterView);
    }
}