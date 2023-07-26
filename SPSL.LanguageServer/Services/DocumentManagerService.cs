using System.Collections.Concurrent;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using SPSL.LanguageServer.Core;

namespace SPSL.LanguageServer.Services;

public class DocumentManagerService
{
    private readonly ConcurrentDictionary<DocumentUri, Document> _documents = new();

    public event EventHandler<DocumentEventArgs>? DocumentContentChanged;
    public event EventHandler<DocumentEventArgs>? DocumentRemoved;
    
    private readonly Action<DocumentUri> _onDocumentContentChanged;

    public DocumentManagerService()
    {
        _onDocumentContentChanged = ((Action<DocumentUri>)OnDocumentContentChanged).Debounce(TimeSpan.FromMilliseconds(500));
    }

    private void OnDocumentContentChanged(DocumentUri uri)
    {
        DocumentContentChanged?.Invoke(this, new(uri));
    }

    public bool HasDocument(DocumentUri uri) => _documents.ContainsKey(uri);

    public void UpdateDocument(DocumentUri uri, Document document)
    {
        _documents.AddOrUpdate(uri, document, (k, v) => document);
        _onDocumentContentChanged(document.Uri);
    }

    public void UpdateDocument(DocumentUri uri, Container<TextDocumentContentChangeEvent> changes)
    {
        Document doc = GetDocument(uri);
        doc.Update(changes);
        UpdateDocument(uri, doc);
    }

    public void RemoveDocument(DocumentUri uri)
    {
        _documents.Remove(uri, out _);
        DocumentRemoved?.Invoke(this, new(uri));
    }

    public Document GetDocument(DocumentUri uri)
    {
        return _documents.TryGetValue(uri, out Document? buffer) ? buffer : new(uri, "spsl");
    }
}