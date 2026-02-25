import { useTranslation } from 'react-i18next';
import { useNotifications } from '../context/NotificationContext';
import { IconClose, IconError, IconInfo, IconCheck, IconWarning } from './ui/Icons';

const icons = {
  success: IconCheck,
  error: IconError,
  info: IconInfo,
  warning: IconWarning,
};

export default function NotificationBar() {
  const { t } = useTranslation();
  const { bar, clearBar } = useNotifications();

  if (!bar) return null;

  const Icon = icons[bar.type];

  return (
    <div className={`notification-bar notification-bar-${bar.type}`} role="alert">
      <span className="notification-bar-icon">
        <Icon />
      </span>
      <div className="notification-bar-body">
        {bar.title && <strong className="notification-bar-title">{bar.title}</strong>}
        <span className="notification-bar-message">{bar.message}</span>
      </div>
      {bar.dismissible && (
        <button
          type="button"
          className="notification-bar-dismiss btn btn-ghost"
          onClick={clearBar}
          aria-label={t('common.close')}
        >
          <IconClose />
        </button>
      )}
    </div>
  );
}
