namespace AddOptimization.Contracts.Dto;

public class AddressCreateDto:BaseDto<Guid?>
{
    public string TargetType { get; set; }
    public int? TargetId { get; set; }
    public string Phone { get; set; }
    public string Address1 { get; set; }
    public string Address2 { get; set; }
    public string City { get; set; }
    public string Zip { get; set; }
    public string Province { get; set; }
    public string Country { get; set; }
    public string CountryCode { get; set; }
    public bool IsDeleted { get; set; }
    public Guid? CustomerId { get; set; }
    public string GPSCoordinates { get; set; }
}
