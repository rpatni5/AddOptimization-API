namespace AddOptimization.Contracts.Dto
{
    public class UserSummaryDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public bool? IsEmailsEnabled { get; set; }
        public string Email { get; set; }
        public string BranchName { get; set; }        
    }
}
