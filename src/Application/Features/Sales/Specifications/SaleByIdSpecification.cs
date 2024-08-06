namespace CleanArchitecture.Blazor.Application.Features.Sales.Specifications;
#nullable disable warnings
/// <summary>
/// Specification class for filtering Sales by their ID.
/// </summary>
public class SaleByIdSpecification : Specification<Sale>
{
    public SaleByIdSpecification(int id)
    {
       Query.Where(q => q.Id == id);
    }
}