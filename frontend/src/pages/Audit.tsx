import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { api, type AuditEvent as AuditEventType } from '../api/client';

export default function Audit() {
  const { t } = useTranslation();
  const [events, setEvents] = useState<AuditEventType[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    setLoading(true);
    api.audit.query({ take: 100 }).then(setEvents).catch(() => setEvents([])).finally(() => setLoading(false));
  }, []);

  return (
    <div className="page">
      <header className="page-header">
        <h1 className="page-title">{t('audit.title')}</h1>
      </header>
      <div className="page-body">
        <div className="card card-table-wrap">
          <table>
          <thead>
            <tr>
              <th>{t('audit.time')}</th>
              <th>{t('audit.type')}</th>
              <th>{t('audit.agent')}</th>
              <th>{t('audit.tool')}</th>
              <th>{t('audit.result')}</th>
            </tr>
          </thead>
          <tbody>
            {loading ? (
              <tr><td colSpan={5}>{t('common.loading')}</td></tr>
            ) : events.length === 0 ? (
              <tr><td colSpan={5}>{t('audit.noRecords')}</td></tr>
            ) : (
              events.map((e) => (
                <tr key={e.id}>
                  <td>{new Date(e.timestamp).toLocaleString()}</td>
                  <td><span className="badge badge-muted">{e.eventType}</span></td>
                  <td>{e.agentId ?? '—'}</td>
                  <td>{e.toolName ?? '—'}</td>
                  <td className="cell-ellipsis" style={{ maxWidth: 200 }} title={e.result ?? e.payload ?? ''}>
                    {e.result ?? e.payload ?? '—'}
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
        </div>
      </div>
    </div>
  );
}
