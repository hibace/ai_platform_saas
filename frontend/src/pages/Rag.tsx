import { useState } from 'react';
import { useTranslation } from 'react-i18next';
import { api, type RagSearchResult } from '../api/client';
import { useNotifications } from '../context/NotificationContext';

export default function Rag() {
  const { t } = useTranslation();
  const { addToast } = useNotifications();
  const [collectionId, setCollectionId] = useState('default');
  const [docId, setDocId] = useState('');
  const [docContent, setDocContent] = useState('');
  const [query, setQuery] = useState('');
  const [results, setResults] = useState<RagSearchResult[]>([]);
  const [loading, setLoading] = useState(false);

  const createCollection = () => {
    api.rag.createCollection(collectionId)
      .then(() => addToast('success', t('rag.collectionCreated')))
      .catch((e) => addToast('error', (e as Error).message, { title: t('common.error') }));
  };

  const indexDoc = () => {
    if (!docId.trim() || !docContent.trim()) return;
    api.rag.index(collectionId, { id: docId, content: docContent })
      .then(() => addToast('success', t('rag.documentIndexed')))
      .catch((e) => addToast('error', (e as Error).message, { title: t('common.error') }));
  };

  const search = async () => {
    if (!query.trim()) return;
    setLoading(true);
    try {
      const r = await api.rag.search(collectionId, query, 5);
      setResults(r);
    } catch (e) {
      setResults([]);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="page">
      <header className="page-header">
        <h1 className="page-title">{t('rag.title')}</h1>
      </header>
      <div className="page-body">
        <div className="card">
        <h3>{t('rag.collection')}</h3>
        <div className="form-group">
          <label>{t('rag.collectionId')}</label>
          <input value={collectionId} onChange={(e) => setCollectionId(e.target.value)} />
        </div>
        <button className="btn btn-primary" onClick={createCollection}>{t('rag.createCollection')}</button>
        </div>
        <div className="card">
        <h3>{t('rag.indexing')}</h3>
        <div className="form-group">
          <label>{t('rag.docId')}</label>
          <input value={docId} onChange={(e) => setDocId(e.target.value)} placeholder={t('rag.docIdPlaceholder')} />
        </div>
        <div className="form-group">
          <label>{t('rag.content')}</label>
          <textarea value={docContent} onChange={(e) => setDocContent(e.target.value)} placeholder={t('rag.contentPlaceholder')} />
        </div>
        <button className="btn btn-primary" onClick={indexDoc}>{t('rag.index')}</button>
      </div>
      <div className="card">
        <h3>{t('rag.search')}</h3>
        <div className="form-group">
          <input value={query} onChange={(e) => setQuery(e.target.value)} placeholder={t('rag.queryPlaceholder')} onKeyDown={(e) => e.key === 'Enter' && search()} />
        </div>
        <button className="btn btn-primary" onClick={search} disabled={loading}>{loading ? t('rag.searching') : t('rag.searchBtn')}</button>
        {results.length > 0 && (
          <ul className="list-bullet" style={{ marginTop: '1rem' }}>
            {results.map((r) => (
              <li key={r.documentId} style={{ marginBottom: '0.5rem' }}>
                <strong>{r.documentId}</strong> (score: {r.score.toFixed(2)}): {r.content.slice(0, 150)}...
              </li>
            ))}
          </ul>
        )}
      </div>
      </div>
    </div>
  );
}
