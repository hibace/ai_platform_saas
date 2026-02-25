import { useState, useRef, useEffect } from 'react';
import { useTranslation } from 'react-i18next';
import { useTheme } from '../context/ThemeContext';
import { useNotifications } from '../context/NotificationContext';
import { IconBell, IconHelp, IconSun, IconMoon } from './ui/Icons';
import i18n from '../i18n';

export default function Header() {
  const { t } = useTranslation();
  const { theme, toggleTheme } = useTheme();
  const { toasts, addToast } = useNotifications();
  const [helpOpen, setHelpOpen] = useState(false);
  const [notifOpen, setNotifOpen] = useState(false);
  const helpRef = useRef<HTMLDivElement>(null);
  const notifRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    function handleClick(e: MouseEvent) {
      if (
        helpRef.current && !helpRef.current.contains(e.target as Node) &&
        notifRef.current && !notifRef.current.contains(e.target as Node)
      ) {
        setHelpOpen(false);
        setNotifOpen(false);
      }
    }
    document.addEventListener('click', handleClick);
    return () => document.removeEventListener('click', handleClick);
  }, []);

  const setLang = (lng: 'ru' | 'en') => {
    i18n.changeLanguage(lng);
    try {
      localStorage.setItem('ai-platform-lang', lng);
    } catch {
      /* ignore */
    }
  };

  const randomNotifications: Array<{ type: 'success' | 'error' | 'info' | 'warning'; title?: string; message: string }> = [
    { type: 'success', title: t('header.demoSuccess', { defaultValue: 'Готово' }), message: t('header.demoSuccessMsg', { defaultValue: 'Операция выполнена успешно.' }) },
    { type: 'success', message: t('header.demoSaved', { defaultValue: 'Изменения сохранены.' }) },
    { type: 'error', title: t('header.demoError', { defaultValue: 'Ошибка' }), message: t('header.demoErrorMsg', { defaultValue: 'Не удалось выполнить запрос. Попробуйте позже.' }) },
    { type: 'error', message: t('header.demoErrorNetwork', { defaultValue: 'Ошибка сети. Проверьте подключение.' }) },
    { type: 'warning', title: t('header.demoWarning', { defaultValue: 'Внимание' }), message: t('header.demoWarningMsg', { defaultValue: 'Лимит запросов почти исчерпан.' }) },
    { type: 'warning', message: t('header.demoWarningCache', { defaultValue: 'Данные могут быть устаревшими.' }) },
    { type: 'info', title: t('header.demoInfo', { defaultValue: 'Подсказка' }), message: t('header.demoInfoMsg', { defaultValue: 'Используйте Enter для отправки сообщения.' }) },
    { type: 'info', message: t('header.demoInfoUpdate', { defaultValue: 'Доступно обновление приложения.' }) },
  ];

  const addRandomNotification = () => {
    const item = randomNotifications[Math.floor(Math.random() * randomNotifications.length)];
    addToast(item.type, item.message, { title: item.title });
  };

  return (
    <header className="header">
      <div className="header-brand">
        <span className="header-logo">{t('header.brand')}</span>
      </div>
      <div className="header-actions">
        <div className="header-lang">
          <button
            type="button"
            className={`btn btn-ghost header-lang-btn ${i18n.language === 'ru' ? 'active' : ''}`}
            onClick={() => setLang('ru')}
            aria-label={t('header.langRu')}
          >
            RU
          </button>
          <button
            type="button"
            className={`btn btn-ghost header-lang-btn ${i18n.language === 'en' ? 'active' : ''}`}
            onClick={() => setLang('en')}
            aria-label={t('header.langEn')}
          >
            EN
          </button>
        </div>
        <button
          type="button"
          className="btn btn-ghost header-icon-btn"
          onClick={addRandomNotification}
          title={t('header.randomNotification', { defaultValue: 'Показать случайное уведомление' })}
          aria-label={t('header.randomNotification', { defaultValue: 'Показать случайное уведомление' })}
        >
          🎲
        </button>
        <div className="header-dropdown" ref={notifRef}>
          <button
            type="button"
            className="btn btn-ghost header-icon-btn"
            onClick={() => { setNotifOpen((v) => !v); setHelpOpen(false); }}
            aria-label={t('header.notifications')}
            aria-expanded={notifOpen ? 'true' : 'false'}
          >
            <IconBell />
            {toasts.length > 0 && (
              <span className="header-badge">{toasts.length > 9 ? '9+' : toasts.length}</span>
            )}
          </button>
          {notifOpen && (
            <div className="dropdown-panel notification-panel">
              <div className="dropdown-title">{t('header.notifications')}</div>
              {toasts.length === 0 ? (
                <p className="dropdown-empty">{t('header.notificationsEmpty')}</p>
              ) : (
                <ul className="dropdown-list">
                  {toasts.slice(-5).reverse().map((toast) => (
                    <li key={toast.id} className={`dropdown-item dropdown-item-${toast.type}`}>
                      {toast.title && <strong>{toast.title}</strong>}
                      <span>{toast.message}</span>
                    </li>
                  ))}
                </ul>
              )}
            </div>
          )}
        </div>

        <div className="header-dropdown" ref={helpRef}>
          <button
            type="button"
            className="btn btn-ghost header-icon-btn"
            onClick={() => { setHelpOpen((v) => !v); setNotifOpen(false); }}
            aria-label={t('header.help')}
            aria-expanded={helpOpen ? 'true' : 'false'}
          >
            <IconHelp />
          </button>
          {helpOpen && (
            <div className="dropdown-panel help-panel">
              <div className="dropdown-title">{t('header.help')}</div>
              <nav className="dropdown-nav">
                <a href="#docs" className="dropdown-link" onClick={() => setHelpOpen(false)}>
                  {t('header.docs')}
                </a>
                <a href="#support" className="dropdown-link" onClick={() => setHelpOpen(false)}>
                  {t('header.support')}
                </a>
                <a href="#shortcuts" className="dropdown-link" onClick={() => setHelpOpen(false)}>
                  {t('header.shortcuts')}
                </a>
                <a href="#about" className="dropdown-link" onClick={() => setHelpOpen(false)}>
                  {t('header.about')}
                </a>
              </nav>
              <div className="dropdown-section">
                <div className="dropdown-section-title">{t('header.shortcuts')}</div>
                <div className="shortcut-row"><kbd>?</kbd> — {t('header.shortcutHelp')}</div>
                <div className="shortcut-row"><kbd>Esc</kbd> — {t('header.shortcutClose')}</div>
              </div>
            </div>
          )}
        </div>

        <button
          type="button"
          className="btn btn-ghost header-icon-btn"
          onClick={toggleTheme}
          title={theme === 'dark' ? t('header.themeLight') : t('header.themeDark')}
          aria-label={theme === 'dark' ? t('header.themeLightAria') : t('header.themeDarkAria')}
        >
          {theme === 'dark' ? <IconSun /> : <IconMoon />}
        </button>
      </div>
    </header>
  );
}
