namespace AddOptimization.Utilities.Interface
{
    public interface IException
    {
        string Code { get; }
        object UserMessage { get; }
        int ViolationCode { get; }
    }
}
