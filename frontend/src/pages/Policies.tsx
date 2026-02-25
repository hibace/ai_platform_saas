import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { api, type Policy } from '../api/client';
import { useNotifications } from '../context/NotificationContext';

export default function Policies() {
  const { t } = useTranslation();
  const { addToast } = useNotifications();
  const [policies, setPolicies] = useState<Policy[]>([]);
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState({ name: '', description: '', rules: [{ action: 'allow', toolPattern: '' }] });

  useEffect(() => {
    api.policies.list().then(setPolicies).catch(() => setPolicies([]));
  }, []);

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await api.policies.save({
        name: form.name,
        description: form.description || undefined,
        rules: form.rules.map((r) => ({ action: r.action, toolPattern: r.toolPattern || undefined })),
      });
      setShowForm(false);
      setForm({ name: '', description: '', rules: [{ action: 'allow', toolPattern: '' }] });
      api.policies.list().then(setPolicies);
      addToast('success', t('policies.created'));
    } catch (err) {
      addToast('error', (err as Error).message, { title: t('common.error') });
    }
  };

  return (
    <div className="page">
      <header className="page-header">
        <h1 className="page-title">{t('policies.title')}</h1>
        <button type="button" className="btn btn-primary" onClick={() => setShowForm(!showForm)}>
          {showForm ? t('common.cancel') : t('policies.createPolicy')}
        </button>
      </header>
      <div className="page-body">
        {showForm && (
          <div className="card">
            <h3>{t('policies.newPolicy')}</h3>
            <form onSubmit={submit}>
            <div className="form-group">
              <label>{t('common.name')}</label>
              <input value={form.name} onChange={(e) => setForm((f) => ({ ...f, name: e.target.value }))} required />
            </div>
            <div className="form-group">
              <label>{t('common.description')}</label>
              <input value={form.description} onChange={(e) => setForm((f) => ({ ...f, description: e.target.value }))} />
            </div>
            <div className="form-group">
              <label>{t('policies.rulesHint')}</label>
              {form.rules.map((r, i) => (
                <div key={i} style={{ display: 'flex', gap: '0.5rem', marginBottom: '0.5rem' }}>
                  <select value={r.action} onChange={(e) => setForm((f) => ({ ...f, rules: f.rules.map((x, j) => j === i ? { ...x, action: e.target.value } : x) }))}>
                    <option value="allow">allow</option>
                    <option value="deny">deny</option>
                  </select>
                  <input
                    placeholder={t('policies.toolPatternPlaceholder')}
                    value={r.toolPattern}
                    onChange={(e) => setForm((f) => ({ ...f, rules: f.rules.map((x, j) => j === i ? { ...x, toolPattern: e.target.value } : x) }))}
                    style={{ flex: 1 }}
                  />
                </div>
              ))}
            </div>
            <button type="submit" className="btn btn-primary">{t('common.create')}</button>
          </form>
        </div>
        )}

        <div className="card">
          <table>
          <thead>
            <tr>
              <th>{t('common.name')}</th>
              <th>{t('common.rules')}</th>
            </tr>
          </thead>
          <tbody>
            {policies.map((p) => (
              <tr key={p.id}>
                <td>{p.name}</td>
                <td>
                  {p.rules?.map((r, i) => (
                    <span key={i} className="badge badge-muted" style={{ marginRight: 4 }}>
                      {r.action} {r.toolPattern || '*'}
                    </span>
                  )) ?? '—'}
                </td>
              </tr>
            ))}
          </tbody>
        </table>
        </div>
      </div>
    </div>
  );
}
