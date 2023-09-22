namespace SPSL.Language.Core;

public interface ILanguageFeature
{
}

public interface ILanguageFeature<T> : ILanguageFeature
{
    public T Merge(T value);
}