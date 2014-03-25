namespace Lardite.RefAssistant.Algorithms.Contracts
{
    public interface IField : IMember
    {
        IMemberType FieldType { get; }
    }
}
