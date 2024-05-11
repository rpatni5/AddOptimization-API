namespace AddOptimization.Utilities.Models
{
    public class BackgroundServiceSettings
    {
        public int ExpirationThresholdInDays { get; set; }
        public int RenewLicenseEmailTriggerDurationInSeconds {  get; set; }
        public int FillTimesheetReminderEmailTriggerDurationInSeconds { get; set; }
        
    }
}
