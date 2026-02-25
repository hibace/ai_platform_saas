import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { api, type Agent, type Tool } from '../api/client';
import { useNotifications } from '../context/NotificationContext';

export default function Agents() {
  const { t } = useTranslation();
  const { addToast } = useNotifications();
  const [agents, setAgents] = useState<Agent[]>([]);
  const [tools, setTools] = useState<Tool[]>([]);
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState({
    name: '',
    description: '',
    systemPrompt: 'You are a helpful assistant.',
    toolIds: [] as string[],
    llmProvider: '',
    llmModel: 'gpt-4o-mini',
    isEnabled: true,
  });

  useEffect(() => {
    api.agents.list().then(setAgents).catch(() => setAgents([]));
    api.tools.list().then(setTools).catch(() => setTools([]));
  }, []);

  const submit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await api.agents.create({
        name: form.name,
        description: form.description || undefined,
        systemPrompt: form.systemPrompt,
        toolIds: form.toolIds,
        llmProvider: form.llmProvider || undefined,
        llmModel: form.llmModel || undefined,
        isEnabled: form.isEnabled,
      });
      setShowForm(false);
      setForm({ name: '', description: '', systemPrompt: 'You are a helpful assistant.', toolIds: [], llmProvider: '', llmModel: 'gpt-4o-mini', isEnabled: true });
      api.agents.list().then(setAgents);
      addToast('success', t('agents.created'));
    } catch (err) {
      addToast('error', (err as Error).message, { title: t('common.error') });
    }
  };

  const toggleTool = (id: string) => {
    setForm((f) => ({
      ...f,
      toolIds: f.toolIds.includes(id) ? f.toolIds.filter((t) => t !== id) : [...f.toolIds, id],
    }));
  };

  return (
    <div className="page">
      <header className="page-header">
        <h1 className="page-title">{t('agents.title')}</h1>
        <button type="button" className="btn btn-primary" onClick={() => setShowForm(!showForm)}>
          {showForm ? t('common.cancel') : t('agents.createAgent')}
        </button>
      </header>
      <div className="page-body">
        {showForm && (
          <div className="card">
            <h3>{t('agents.newAgent')}</h3>
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
              <label>{t('agents.systemPrompt')}</label>
              <textarea value={form.systemPrompt} onChange={(e) => setForm((f) => ({ ...f, systemPrompt: e.target.value }))} />
            </div>
            <div className="form-group">
              <label>{t('agents.tools')}</label>
              <div style={{ display: 'flex', flexWrap: 'wrap', gap: '0.5rem' }}>
                {tools.map((tool) => (
                  <label key={tool.id} style={{ display: 'flex', alignItems: 'center', gap: '0.35rem' }}>
                    <input type="checkbox" checked={form.toolIds.includes(tool.id)} onChange={() => toggleTool(tool.id)} />
                    {tool.name}
                  </label>
                ))}
              </div>
            </div>
            <div className="form-group">
              <label>{t('agents.llmModel')}</label>
              <input value={form.llmModel} onChange={(e) => setForm((f) => ({ ...f, llmModel: e.target.value }))} placeholder={t('agents.llmModelPlaceholder')} />
            </div>
            <div className="form-group">
              <label>
                <input type="checkbox" checked={form.isEnabled} onChange={(e) => setForm((f) => ({ ...f, isEnabled: e.target.checked }))} />
                {t('agents.enabled')}
              </label>
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
              <th>{t('agents.tools')}</th>
              <th>{t('common.status')}</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            {agents.map((a) => (
              <tr key={a.id}>
                <td>{a.name}</td>
                <td>{(a.toolIds as string[])?.join(', ') || '—'}</td>
                <td>
                  <span className={`badge ${a.isEnabled ? 'badge-success' : 'badge-muted'}`}>
                    {a.isEnabled ? t('common.on') : t('common.off')}
                  </span>
                </td>
                <td>
                  <Link to={`/agents/${a.id}/run`} className="btn btn-ghost">{t('agents.run')}</Link>
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
