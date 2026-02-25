import { useEffect, useState } from 'react';
import { useTranslation } from 'react-i18next';
import { api, type LicenseStatus } from '../api/client';
import i18n from '../i18n';

const localeForDate = (lng: string) => (lng === 'en' ? 'en-US' : 'ru-RU');

export default function ControlPlane() {
  const { t } = useTranslation();
  const [license, setLicense] = useState<LicenseStatus | null>(null);

  useEffect(() => {
    api.controlPlane.license().then(setLicense).catch(() => setLicense(null));
  }, []);

  return (
    <div className="page">
      <header className="page-header">
        <h1 className="page-title">{t('controlPlane.title')}</h1>
      </header>
      <div className="page-body">
        <div className="card">
          <h3>{t('controlPlane.license')}</h3>
          {license ? (
            <dl className="dl-grid">
            <dt>{t('common.status')}</dt>
            <dd><span className={`badge ${license.valid ? 'badge-success' : 'badge-error'}`}>{license.valid ? t('dashboard.licenseValid') : t('dashboard.licenseInvalid')}</span></dd>
            <dt>{t('controlPlane.edition')}</dt>
            <dd>{license.edition ?? '—'}</dd>
            <dt>{t('controlPlane.expires')}</dt>
            <dd>{license.expiresAt ? new Date(license.expiresAt).toLocaleDateString(localeForDate(i18n.language)) : '—'}</dd>
            <dt>{t('controlPlane.maxAgents')}</dt>
            <dd>{license.maxAgents ?? '—'}</dd>
            <dt>{t('controlPlane.maxTenants')}</dt>
            <dd>{license.maxTenants ?? '—'}</dd>
            <dt>{t('controlPlane.features')}</dt>
            <dd>{(license.features ?? []).join(', ') || '—'}</dd>
            </dl>
          ) : (
            <p>{t('controlPlane.licenseLoadError')}</p>
          )}
        </div>
        <div className="card">
          <h3>{t('controlPlane.config')}</h3>
          <p className="text-muted">{t('controlPlane.configNote')}</p>
        </div>
      </div>
    </div>
  );
}
