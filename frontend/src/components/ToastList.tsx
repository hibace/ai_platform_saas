import { useTranslation } from 'react-i18next';
import { useNotifications } from '../context/NotificationContext';
import { IconCheck, IconClose, IconError, IconInfo, IconWarning } from './ui/Icons';

const icons = {
  success: IconCheck,
  error: IconError,
  info: IconInfo,
  warning: IconWarning,
};

export default function ToastList() {
  const { t } = useTranslation();
  const { toasts, removeToast } = useNotifications();

  if (toasts.length === 0) return null;

  return (
    <div className="toast-list" role="region" aria-label={t('toast.ariaRegion')}>
      {toasts.map((toast) => {
        const Icon = icons[toast.type];
        return (
          <div
            key={toast.id}
            className={`toast toast-${toast.type}`}
            role="alert"
          >
            <span className="toast-icon">
              <Icon />
            </span>
            <div className="toast-body">
              {toast.title && <div className="toast-title">{toast.title}</div>}
              <div className="toast-message">{toast.message}</div>
            </div>
            <button
              type="button"
              className="toast-dismiss btn btn-ghost"
              onClick={() => removeToast(toast.id)}
              aria-label={t('common.close')}
            >
              <IconClose />
            </button>
          </div>
        );
      })}
    </div>
  );
}
