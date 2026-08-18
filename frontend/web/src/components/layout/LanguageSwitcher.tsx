import { LOCALES } from '../../i18n/messages'
import { useLanguage } from '../../i18n/LanguageProvider'

export function LanguageSwitcher({ variant }: { variant: 'sidebar' | 'header' }) {
  const { locale, setLocale, t } = useLanguage()

  return (
    <div className={`lang-switch lang-switch-${variant}`} role="group" aria-label={t('chrome.language')}>
      {LOCALES.map((item) => (
        <button
          key={item}
          type="button"
          lang={item}
          className={item === locale ? 'is-active' : undefined}
          aria-pressed={item === locale}
          onClick={() => setLocale(item)}
        >
          {t(`language.${item}`)}
        </button>
      ))}
    </div>
  )
}
