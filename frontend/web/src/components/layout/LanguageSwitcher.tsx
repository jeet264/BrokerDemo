import { useState, useRef, useEffect } from 'react'
import { LOCALES } from '../../i18n/messages'
import { useLanguage } from '../../i18n/LanguageProvider'

export function LanguageSwitcher({ variant }: { variant: 'sidebar' | 'header' }) {
  const { locale, setLocale, t } = useLanguage()
  const [isOpen, setIsOpen] = useState(false)
  const containerRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
        setIsOpen(false)
      }
    }
    document.addEventListener('mousedown', handleClickOutside)
    return () => document.removeEventListener('mousedown', handleClickOutside)
  }, [])

  return (
    <div
      ref={containerRef}
      className={`custom-lang-dropdown lang-switch-${variant}`}
      aria-label={t('chrome.language')}
    >
      <button
        type="button"
        className={`custom-lang-trigger ${variant === 'header' ? 'light-trigger' : ''} ${isOpen ? 'open' : ''}`}
        onClick={() => setIsOpen(!isOpen)}
        aria-expanded={isOpen}
      >
        <i className="bi bi-globe2 lang-globe-icon" aria-hidden="true" />
        <span>{t(`language.${locale}`)}</span>
        <i className={`bi bi-chevron-down lang-chevron-icon ${isOpen ? 'rotate' : ''}`} />
      </button>

      {isOpen && (
        <div className={`custom-lang-menu ${variant === 'header' ? 'light-menu' : ''}`}>
          {LOCALES.map((item) => (
            <button
              key={item}
              type="button"
              className={`custom-lang-option ${locale === item ? 'active' : ''}`}
              onClick={() => {
                setLocale(item)
                setIsOpen(false)
              }}
            >
              <span>{t(`language.${item}`)}</span>
              {locale === item && <i className="bi bi-check2 check-icon" />}
            </button>
          ))}
        </div>
      )}
    </div>
  )
}
