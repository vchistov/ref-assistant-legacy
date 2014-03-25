namespace Lardite.RefAssistant.Algorithms.Contracts
{
    public interface IEvent : IMember
    {
        IMemberType EventType { get; }
    }
}
