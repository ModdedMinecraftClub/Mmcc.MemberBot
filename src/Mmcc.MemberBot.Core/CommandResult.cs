namespace Mmcc.MemberBot.Core
{
    public class CommandResult<T>
    {
        public CommandResult(string failureReason)
            => FailureReason = failureReason;

        public CommandResult(T payload)
            => Payload = payload;

        public T? Payload { get; set; }
        public string? FailureReason { get; set; }
        public bool Succeeded => FailureReason is null;
    }
}