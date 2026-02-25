import { type ReactNode } from 'react';
import { useTranslation } from 'react-i18next';
import { IconCheck, IconError, IconInfo, IconWarning } from './ui/Icons';

export type AlertType = 'success' | 'error' | 'info' | 'warning';

const icons = {
  success: IconCheck,
  error: IconError,
  info: IconInfo,
  warning: IconWarning,
};

type AlertProps = {
  type?: AlertType;
  title?: string;
  children: ReactNode;
  onDismiss?: () => void;
  className?: string;
};

export default function Alert({ type = 'info', title, children, onDismiss, className = '' }: AlertProps) {
  const { t } = useTranslation();
  const Icon = icons[type];

  return (
    <div className={`alert alert-${type} ${className}`.trim()} role="alert">
      <span className="alert-icon">
        <Icon />
      </span>
      <div className="alert-body">
        {title && <div className="alert-title">{title}</div>}
        <div className="alert-content">{children}</div>
      </div>
      {onDismiss && (
        <button
          type="button"
          className="alert-dismiss btn btn-ghost"
          onClick={onDismiss}
          aria-label={t('common.close')}
        >
          ×
        </button>
      )}
    </div>
  );
}
