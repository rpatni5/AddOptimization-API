using System.ComponentModel.DataAnnotations;

namespace AddOptimization.Contracts.Dto;

public class CustomerCreateDto:BaseDto<Guid?>
{
    [Required]
    public string Email { get; set; }
    public string Company { get; set; }
    public string Phone { get; set; }
    public string Notes { get; set; }
    public Guid? BillingAddressId { get; set; }
    [Required(ErrorMessage ="Tax rate is required")]
    public Guid? TaxRateId { get; set; }
    public Guid CustomerStatusId { get; set; }
    public bool IsDeleted { get; set; }
    public Guid? BillingStatusId { get; set; }
    public List<AddressCreateDto> Addresses { get; set; }
}
