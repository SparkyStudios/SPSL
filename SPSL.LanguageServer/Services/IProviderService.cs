using OmniSharp.Extensions.LanguageServer.Protocol;

namespace SPSL.LanguageServer.Services;

/// <summary>
/// Event data used when the <see cref="IProviderService{T}"/> has updated its data.
/// </summary>
/// <typeparam name="T">
/// The type of the data provided by the <see cref="IProviderService{T}"/>.
/// </typeparam>
public class ProviderDataUpdatedEventArgs<T> : EventArgs where T : class
{
    /// <summary>
    /// The uri of the document for which the <see cref="IProviderService{T}"/> has updated its data. 
    /// </summary>
    public DocumentUri Uri { get; init; }

    /// <summary>
    /// The updated data.
    /// </summary>
    public T Data { get; init; }

    /// <summary>
    /// Constructs a new <see cref="ProviderDataUpdatedEventArgs{T}"/> instance.
    /// </summary>
    /// <param name="uri">The uri of the document.</param>
    /// <param name="data">The updated data.</param>
    public ProviderDataUpdatedEventArgs(DocumentUri uri, T data)
    {
        Uri = uri;
        Data = data;
    }
}

/// <summary>
/// Interface for a service that provides data for a <see cref="Core.Document"/>.
/// </summary>
/// <typeparam name="T">
/// The type of the data provided by the <see cref="IProviderService{T}"/>.
/// </typeparam>
public interface IProviderService<T> where T : class
{
    /// <summary>
    /// Event that is raised when the <see cref="IProviderService{T}"/> has updated its data
    /// for a <see cref="Core.Document"/>.
    /// </summary>
    public event EventHandler<ProviderDataUpdatedEventArgs<T>>? DataUpdated;

    /// <summary>
    /// Gets the data for a <see cref="Core.Document"/>.
    /// </summary>
    /// <param name="uri">The uri of the document.</param>
    /// <returns>
    /// The data for a <see cref="Core.Document"/>, or <see langword="null"/> if no data is available.
    /// </returns>
    public T? GetData(DocumentUri uri);

    /// <summary>
    /// Updates the data for a <see cref="Core.Document"/>.
    /// </summary>
    /// <param name="uri">The uri of the document.</param>
    /// <param name="data">The new data.</param>
    /// <param name="notify">Whether to notify the <see cref="DataUpdated"/> event.</param>
    public void SetData(DocumentUri uri, T data, bool notify = true);
}