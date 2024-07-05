namespace AddOptimization.Contracts.Dto
{
    public class AuthResponseDto
    {
        public string JWT { get; set; }
        public Guid RefreshToken { get; set; }
        public DateTime Expiry { get; set; }
        public UserSummaryDto User { get; set; }
        public bool? NDASignedRequired { get; set; }
    }
}
