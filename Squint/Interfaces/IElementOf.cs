namespace SquintScript.Interfaces
{
    public interface IElementOf<T>
    {
        bool IsElementOf(T SuperSet);
        string SuperSetName { get; }
    }
}
