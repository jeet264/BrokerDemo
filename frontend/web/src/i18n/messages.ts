export const LOCALES = ['en', 'hi', 'gu'] as const

export type Locale = (typeof LOCALES)[number]

export const LOCALE_STORAGE_KEY = 'brokeros.language'

type Messages = {
  brandTag: string
  nav: {
    workspace: string
    overview: string
    myDay: string
    clients: string
    policies: string
    renewals: string
    tasks: string
    notifications: string
  }
  chrome: {
    overviewKicker: string
    myDayKicker: string
    clientsKicker: string
    clientFile: string
    importClients: string
    policiesKicker: string
    policyFile: string
    importPolicies: string
    renewalsKicker: string
    renewalFile: string
    tasksKicker: string
    task: string
    notificationsKicker: string
    settings: string
    settingsKicker: string
    excelCsv: string
    quickNote: string
    demo: string
    signOut: string
    language: string
  }
  language: {
    en: string
    hi: string
    gu: string
  }
  roles: {
    BrokerAdmin: string
    BrokerManager: string
    BrokerEmployee: string
  }
}

export const messages: Record<Locale, Messages> = {
  en: {
    brandTag: 'Never miss a renewal',
    nav: {
      workspace: 'Workspace',
      overview: 'Overview',
      myDay: 'My Day',
      clients: 'Clients',
      policies: 'Policies',
      renewals: 'Renewals',
      tasks: 'Tasks',
      notifications: 'Notifications',
    },
    chrome: {
      overviewKicker: 'What is at risk',
      myDayKicker: 'Call these next',
      clientsKicker: 'The book',
      clientFile: 'Client file',
      importClients: 'Import clients',
      policiesKicker: 'Current term',
      policyFile: 'Policy file',
      importPolicies: 'Import policies',
      renewalsKicker: 'Never miss an expiry',
      renewalFile: 'Renewal file',
      tasksKicker: 'Follow-ups',
      task: 'Task',
      notificationsKicker: 'Preview only',
      settings: 'Settings',
      settingsKicker: 'Demo workspace',
      excelCsv: 'Excel / CSV',
      quickNote: 'Quick Note',
      demo: 'Demo',
      signOut: 'Sign out',
      language: 'Language',
    },
    language: {
      en: 'English',
      hi: 'हिन्दी',
      gu: 'ગુજરાતી',
    },
    roles: {
      BrokerAdmin: 'Broker Admin',
      BrokerManager: 'Broker Manager',
      BrokerEmployee: 'Broker Employee',
    },
  },
  hi: {
    brandTag: 'नवीनीकरण न चूकें',
    nav: {
      workspace: 'कार्यक्षेत्र',
      overview: 'अवलोकन',
      myDay: 'मेरा दिन',
      clients: 'ग्राहक',
      policies: 'पॉलिसी',
      renewals: 'नवीनीकरण',
      tasks: 'कार्य',
      notifications: 'सूचनाएँ',
    },
    chrome: {
      overviewKicker: 'जोखिम में क्या है',
      myDayKicker: 'इन्हें अभी कॉल करें',
      clientsKicker: 'बही',
      clientFile: 'ग्राहक फ़ाइल',
      importClients: 'ग्राहक आयात',
      policiesKicker: 'वर्तमान अवधि',
      policyFile: 'पॉलिसी फ़ाइल',
      importPolicies: 'पॉलिसी आयात',
      renewalsKicker: 'समाप्ति न चूकें',
      renewalFile: 'नवीनीकरण फ़ाइल',
      tasksKicker: 'फॉलो-अप',
      task: 'कार्य',
      notificationsKicker: 'केवल पूर्वावलोकन',
      settings: 'सेटिंग्स',
      settingsKicker: 'डेमो कार्यक्षेत्र',
      excelCsv: 'Excel / CSV',
      quickNote: 'त्वरित नोट',
      demo: 'डेमो',
      signOut: 'साइन आउट',
      language: 'भाषा',
    },
    language: {
      en: 'English',
      hi: 'हिन्दी',
      gu: 'ગુજરાતી',
    },
    roles: {
      BrokerAdmin: 'ब्रोकर एडमिन',
      BrokerManager: 'मैनेजर',
      BrokerEmployee: 'कर्मचारी',
    },
  },
  gu: {
    brandTag: 'રિન્યુઅલ ન ચૂકો',
    nav: {
      workspace: 'વર્કસ્પેસ',
      overview: 'અવલોકન',
      myDay: 'મારો દિવસ',
      clients: 'ગ્રાહકો',
      policies: 'પોલિસી',
      renewals: 'રિન્યુઅલ',
      tasks: 'કાર્ય',
      notifications: 'સૂચનાઓ',
    },
    chrome: {
      overviewKicker: 'શું જોખમમાં છે',
      myDayKicker: 'આને હવે કૉલ કરો',
      clientsKicker: 'ચોપડું',
      clientFile: 'ગ્રાહક ફાઇલ',
      importClients: 'ગ્રાહક આયાત',
      policiesKicker: 'વર્તમાન મુદત',
      policyFile: 'પોલિસી ફાઇલ',
      importPolicies: 'પોલિસી આયાત',
      renewalsKicker: 'સમાપ્તિ ન ચૂકો',
      renewalFile: 'રિન્યુઅલ ફાઇલ',
      tasksKicker: 'ફોલો-અપ',
      task: 'કાર્ય',
      notificationsKicker: 'માત્ર પૂર્વાવલોકન',
      settings: 'સેટિંગ્સ',
      settingsKicker: 'ડેમો વર્કસ્પેસ',
      excelCsv: 'Excel / CSV',
      quickNote: 'ઝડપી નોંધ',
      demo: 'ડેમો',
      signOut: 'સાઇન આઉટ',
      language: 'ભાષા',
    },
    language: {
      en: 'English',
      hi: 'हिन्दी',
      gu: 'ગુજરાતી',
    },
    roles: {
      BrokerAdmin: 'બ્રોકર એડમિન',
      BrokerManager: 'મેનેજર',
      BrokerEmployee: 'કર્મચારી',
    },
  },
}

export function isLocale(value: string | null | undefined): value is Locale {
  return value === 'en' || value === 'hi' || value === 'gu'
}

export function htmlLang(locale: Locale) {
  if (locale === 'hi') {
    return 'hi'
  }
  if (locale === 'gu') {
    return 'gu'
  }
  return 'en'
}
