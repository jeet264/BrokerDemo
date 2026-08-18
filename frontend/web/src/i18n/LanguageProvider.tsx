import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react'
import { htmlLang, isLocale, LOCALE_STORAGE_KEY, messages, type Locale } from './messages'

type Translate = (key: string) => string

interface LanguageContextValue {
  locale: Locale
  setLocale: (locale: Locale) => void
  t: Translate
}

const LanguageContext = createContext<LanguageContextValue | null>(null)

function readStoredLocale(): Locale {
  if (typeof localStorage === 'undefined') {
    return 'en'
  }
  const stored = localStorage.getItem(LOCALE_STORAGE_KEY)
  return isLocale(stored) ? stored : 'en'
}

function lookup(locale: Locale, key: string): string | undefined {
  const parts = key.split('.')
  let current: unknown = messages[locale]
  for (const part of parts) {
    if (typeof current !== 'object' || current === null || !(part in current)) {
      return undefined
    }
    current = (current as Record<string, unknown>)[part]
  }
  return typeof current === 'string' ? current : undefined
}

export function LanguageProvider({ children }: { children: ReactNode }) {
  const [locale, setLocaleState] = useState<Locale>(readStoredLocale)

  useEffect(() => {
    localStorage.setItem(LOCALE_STORAGE_KEY, locale)
    document.documentElement.lang = htmlLang(locale)
  }, [locale])

  const setLocale = useCallback((next: Locale) => {
    setLocaleState(next)
  }, [])

  const t = useCallback<Translate>(
    (key) => lookup(locale, key) ?? lookup('en', key) ?? key,
    [locale],
  )

  const value = useMemo(() => ({ locale, setLocale, t }), [locale, setLocale, t])

  return <LanguageContext.Provider value={value}>{children}</LanguageContext.Provider>
}

export function useLanguage() {
  const context = useContext(LanguageContext)
  if (!context) {
    throw new Error('useLanguage must be used inside LanguageProvider')
  }
  return context
}
