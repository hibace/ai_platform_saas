import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { api, type Agent, type LicenseStatus } from '../api/client';
import { useNotifications } from '../context/NotificationContext';
import Alert from '../components/Alert';
import i18n from '../i18n';

const localeForDate = (lng: string) => (lng === 'en' ? 'en-US' : 'ru-RU');

function Dashboard() {
  const { t } = useTranslation();
  const { addBar, clearBar } = useNotifications();
  const [agents, setAgents] = useState<Agent[]>([]);
  const [license, setLicense] = useState<LicenseStatus | null>(null);

  useEffect(() => {
    api.agents.list().then(setAgents).catch(() => setAgents([]));
    api.controlPlane.license().then(setLicense).catch(() => setLicense(null));
  }, []);

  useEffect(() => {
    if (license && !license.valid) {
      addBar('warning', t('dashboard.licenseBarMessage'), {
        title: t('dashboard.licenseWarningTitle'),
        dismissible: true,
      });
      return clearBar;
    }
    clearBar();
  }, [license, addBar, clearBar, t]);

  return (
    <div className="page">
      <header className="page-header">
        <h1 className="page-title">{t('dashboard.title')}</h1>
      </header>
      <div className="page-body">
        {!license?.valid && license !== null && (
          <Alert type="warning" title={t('dashboard.licenseWarningTitle')}>
            {t('dashboard.licenseWarningBody')}
          </Alert>
        )}
        <div className="card">
          <h3>{t('dashboard.license')}</h3>
          {license ? (
            <p>
              <span className={`badge ${license.valid ? 'badge-success' : 'badge-error'}`}>
                {license.valid ? t('dashboard.licenseValid') : t('dashboard.licenseInvalid')}
              </span>
              {license.edition && ` • ${license.edition}`}
              {license.expiresAt && ` • ${t('dashboard.licenseExpires', { date: new Date(license.expiresAt).toLocaleDateString(localeForDate(i18n.language)) })}`}
            </p>
          ) : (
            <p className="badge badge-muted">{t('dashboard.licenseNotLoaded')}</p>
          )}
        </div>
        <div className="card card-dashboard-agents">
          <h3>{t('dashboard.agentsCount', { count: agents.length })}</h3>
          {agents.length === 0 ? (
            <p className="text-muted">{t('dashboard.noAgents')} <Link to="/agents" className="link-button">{t('dashboard.createAgent')}</Link></p>
          ) : (
            <ul className="dashboard-agent-list">
              {agents.slice(0, 5).map((a) => (
                <li key={a.id}>
                  <Link to={`/agents/${a.id}/run`} className="dashboard-agent-item">
                    <span className="dashboard-agent-name">{a.name}</span>
                    {!a.isEnabled && <span className="badge badge-muted">{t('common.off')}</span>}
                  </Link>
                </li>
              ))}
            </ul>
          )}
        </div>
      </div>
    </div>
  );
}

export default Dashboard;
