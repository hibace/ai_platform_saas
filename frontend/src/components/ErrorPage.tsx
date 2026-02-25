import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import Alert from './Alert';

function IconErrorLarge() {
  return (
    <svg width="80" height="80" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round" aria-hidden>
      <circle cx="12" cy="12" r="10" />
      <path d="M15 9l-6 6M9 9l6 6" />
    </svg>
  );
}

type ErrorPageProps = {
  error: Error;
  onRetry?: () => void;
  fullPage?: boolean;
};

export default function ErrorPage({ error, onRetry, fullPage = true }: ErrorPageProps) {
  const { t } = useTranslation();
  const [showDetails, setShowDetails] = useState(false);

  const message = error?.message || String(error);
  const stack = error?.stack;

  const content = (
    <div className="error-page">
      <div className="error-page-icon" aria-hidden>
        <IconErrorLarge />
      </div>
      <h1 className="error-page-title">{t('errorPage.title')}</h1>
      <p className="error-page-subtitle">{t('errorPage.subtitle')}</p>
      <Alert type="error" title={t('common.error')}>
        <span className="error-page-message">{message}</span>
      </Alert>
      <div className="error-page-actions">
        {onRetry && (
          <button type="button" className="btn btn-primary" onClick={onRetry}>
            {t('errorPage.retry')}
          </button>
        )}
        <Link to="/" className="btn btn-primary">
          {t('errorPage.home')}
        </Link>
      </div>
      {stack && (
        <details className="error-page-details" open={showDetails}>
          <summary onClick={(e) => { e.preventDefault(); setShowDetails((v) => !v); }}>
            {t('errorPage.details')}
          </summary>
          <pre className="error-page-stack">{stack}</pre>
        </details>
      )}
    </div>
  );

  if (fullPage) {
    return (
      <div className="error-page-wrap">
        {content}
      </div>
    );
  }

  return content;
}
