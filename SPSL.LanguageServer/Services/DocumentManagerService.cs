using System.Collections.Concurrent;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using SPSL.LanguageServer.Core;

namespace SPSL.LanguageServer.Services;

public class DocumentManagerService : IProviderService<Document>
{
    private readonly ConcurrentDictionary<DocumentUri, Document> _documents = new();

    public event EventHandler<DocumentEventArgs>? DocumentRemoved;

    private readonly Action<ProviderDataUpdatedEventArgs<Document>> _onDocumentContentChanged;

    public DocumentManagerService()
    {
        _onDocumentContentChanged =
            ((Action<ProviderDataUpdatedEventArgs<Document>>)OnDocumentContentChanged)
            .Debounce(TimeSpan.FromMilliseconds(500));
    }

    private void OnDocumentContentChanged(ProviderDataUpdatedEventArgs<Document> e)
    {
        DataUpdated?.Invoke(this, e);
    }

    public bool HasDocument(DocumentUri uri) => _documents.ContainsKey(uri);

    public void UpdateDocument(DocumentUri uri, Container<TextDocumentContentChangeEvent> changes)
    {
        Document doc = GetData(uri);
        doc.Update(changes);
        SetData(uri, doc);
    }

    public void RemoveDocument(DocumentUri uri)
    {
        _documents.Remove(uri, out _);
        DocumentRemoved?.Invoke(this, new(uri));
    }

    #region IProviderService<Document> Implementation

    /// <inheritdoc cref="IProviderService{T}.DataUpdated" />
    public event EventHandler<ProviderDataUpdatedEventArgs<Document>>? DataUpdated;

    /// <inheritdoc cref="IProviderService{T}.GetData(DocumentUri)" />
    public Document GetData(DocumentUri uri)
    {
        return _documents.TryGetValue(uri, out Document? buffer) ? buffer : new(uri, "spsl");
    }

    /// <inheritdoc cref="IProviderService{T}.SetData(DocumentUri, T, bool)" />
    public void SetData(DocumentUri uri, Document data, bool notify = true)
    {
        _documents.AddOrUpdate(uri, data, (_, _) => data);

        if (notify)
            _onDocumentContentChanged(new(data.Uri, data));
    }

    #endregion
}