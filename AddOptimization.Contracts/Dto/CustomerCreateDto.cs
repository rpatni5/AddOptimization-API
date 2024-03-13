using System.ComponentModel.DataAnnotations;

namespace AddOptimization.Contracts.Dto;

public class CustomerCreateDto:BaseDto<Guid?>
{
    [Required]
    public string Email { get; set; }
    public string Company { get; set; }
    public string Phone { get; set; }
    public string Notes { get; set; }
    public DateTime Birthday { get; set; }
    public string ContactInfo { get; set; }
    public Guid? BillingAddressId { get; set; }
    public bool IsDeleted { get; set; }

    public Guid? CustomerStatusId { get; set; }
    public List<AddressCreateDto> Addresses { get; set; }
}
