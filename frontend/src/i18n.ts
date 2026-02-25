import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import ru from './locales/ru.json';
import en from './locales/en.json';

const resources = {
  ru: { translation: ru },
  en: { translation: en },
};

const savedLang = (() => {
  try {
    const l = localStorage.getItem('ai-platform-lang');
    if (l === 'ru' || l === 'en') return l;
  } catch {
    /* ignore */
  }
  return undefined;
})();

i18n.use(initReactI18next).init({
  resources,
  lng: savedLang ?? 'ru',
  fallbackLng: 'ru',
  interpolation: {
    escapeValue: false,
  },
});

export default i18n;
