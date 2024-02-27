namespace AddOptimization.Contracts.Dto;

public class CustomerDetailsDto:CustomerDto
{
    public Guid? BillingAddressId { get; set; }
    public List<AddressDto> Addresses { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? LastOrdered { get; set; }
    public int OrderCount { get; set; }
    public int AverageOrderAmount { get; set; }
}
