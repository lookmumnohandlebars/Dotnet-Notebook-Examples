namespace OversimplifiedTypes;

public class Option<T>(T? value)
{
    public bool IsSome => value is not null;
}