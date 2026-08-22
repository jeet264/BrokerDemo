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
    editProfile: string
  }
  dashboard: {
    goodMorning: string
    goodAfternoon: string
    goodEvening: string
    startWithOverdueSub: string
    overdueRenewals: string
    pastExpiryStillOpen: string
    dueIn7Days: string
    includingToday: string
    dueIn30Days: string
    currentTerm: string
    premiumAtRisk: string
    openWithin90Days: string
    pendingTasks: string
    workStillOpen: string
    expiringPolicies: string
    upcomingRenewals: string
    startWithCriticalSub: string
    viewAllRenewals: string
    requiredToday: string
    todaysTasks: string
    todaysTasksSub: string
    viewAllTasks: string
    quickActions: string
    pipelineTitle: string
    topInsurers: string
    policyDistribution: string
    healthScorecard: string
    retentionRate: string
    activeClients: string
    activePolicies: string
  }
  table: {
    client: string
    policy: string
    insurer: string
    premium: string
    expiry: string
    daysLeft: string
    status: string
    assignedTo: string
    action: string
    due: string
    priority: string
    task: string
  }
  actions: {
    view: string
    markComplete: string
    markRenewed: string
    markLost: string
    followUp: string
    call: string
    done: string
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
      editProfile: 'View & Edit Profile',
    },
    dashboard: {
      goodMorning: 'Good morning',
      goodAfternoon: 'Good afternoon',
      goodEvening: 'Good evening',
      startWithOverdueSub: 'Start with overdue, then this week. Next calls also live on My Day.',
      overdueRenewals: 'Overdue renewals',
      pastExpiryStillOpen: 'Past expiry, still open',
      dueIn7Days: 'Due in 7 days',
      includingToday: 'Including today',
      dueIn30Days: 'Due in 30 days',
      currentTerm: 'Current term',
      premiumAtRisk: 'Premium at risk',
      openWithin90Days: 'Open within 90 days',
      pendingTasks: 'Pending tasks',
      workStillOpen: 'Work still open',
      expiringPolicies: 'Expiring policies',
      upcomingRenewals: 'Upcoming renewals',
      startWithCriticalSub: 'Start with overdue and critical items, then the nearest expiry.',
      viewAllRenewals: 'View all renewals',
      requiredToday: 'REQUIRED TODAY',
      todaysTasks: "Today's tasks",
      todaysTasksSub: 'Overdue and due today — the work to clear before close of business.',
      viewAllTasks: 'View all tasks',
      quickActions: 'Quick Actions',
      pipelineTitle: 'Renewal Expiry Pipeline',
      topInsurers: 'Top Insurers by Portfolio Volume',
      policyDistribution: 'Line of Business Breakdown',
      healthScorecard: 'Brokerage Book Health',
      retentionRate: 'Retention Rate',
      activeClients: 'Active Clients',
      activePolicies: 'Active Policies',
    },
    table: {
      client: 'CLIENT',
      policy: 'POLICY',
      insurer: 'INSURER',
      premium: 'PREMIUM',
      expiry: 'EXPIRY',
      daysLeft: 'DAYS LEFT',
      status: 'STATUS',
      assignedTo: 'ASSIGNED TO',
      action: 'ACTION',
      due: 'DUE',
      priority: 'PRIORITY',
      task: 'TASK',
    },
    actions: {
      view: 'View',
      markComplete: 'Mark complete',
      markRenewed: 'Mark renewed',
      markLost: 'Mark lost',
      followUp: 'Follow-up',
      call: 'Call',
      done: 'Done',
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
      editProfile: 'प्रोफ़ाइल देखें व संपादित करें',
    },
    dashboard: {
      goodMorning: 'शुभ प्रभात',
      goodAfternoon: 'शुभ दोपहर',
      goodEvening: 'शुभ संध्या',
      startWithOverdueSub: 'अतिदेय से शुरू करें, फिर इस सप्ताह। अगली कॉल भी My Day पर उपलब्ध हैं।',
      overdueRenewals: 'अतिदेय नवीनीकरण',
      pastExpiryStillOpen: 'अतीत समाप्ति, अभी भी खुला',
      dueIn7Days: '7 दिनों में देय',
      includingToday: 'आज सहित',
      dueIn30Days: '30 दिनों में देय',
      currentTerm: 'वर्तमान अवधि',
      premiumAtRisk: 'जोखिम में प्रीमियम',
      openWithin90Days: '90 दिनों में खुला',
      pendingTasks: 'लंबित कार्य',
      workStillOpen: 'कार्य अभी खुला है',
      expiringPolicies: 'समाप्त होने वाली पॉलिसियाँ',
      upcomingRenewals: 'आगामी नवीनीकरण',
      startWithCriticalSub: 'अतिदेय और महत्वपूर्ण मदों से प्रारंभ करें, फिर निकटतम समाप्ति।',
      viewAllRenewals: 'सभी नवीनीकरण देखें',
      requiredToday: 'आज आवश्यक',
      todaysTasks: 'आज के कार्य',
      todaysTasksSub: 'अतिदेय और आज देय कार्य — व्यवसाय बंद होने से पहले निपटाने योग्य काम।',
      viewAllTasks: 'सभी कार्य देखें',
      quickActions: 'त्वरित कार्य',
      pipelineTitle: 'नवीनीकरण समाप्ति पाइपलाइन',
      topInsurers: 'पोर्टफोलियो के अनुसार शीर्ष बीमाकर्ता',
      policyDistribution: 'बीमा श्रेणी विवरण',
      healthScorecard: 'ब्रोकरेज पोर्टफोलियो स्थिति',
      retentionRate: 'प्रतिधारण दर',
      activeClients: 'सक्रिय ग्राहक',
      activePolicies: 'सक्रिय पॉलिसियां',
    },
    table: {
      client: 'ग्राहक',
      policy: 'पॉलिसी',
      insurer: 'बीमाकर्ता',
      premium: 'प्रीमियम',
      expiry: 'समाप्ति',
      daysLeft: 'शेष दिन',
      status: 'स्थिति',
      assignedTo: 'सौंपा गया',
      action: 'कार्रवाई',
      due: 'देय',
      priority: 'प्राथमिकता',
      task: 'कार्य',
    },
    actions: {
      view: 'देखें',
      markComplete: 'पूरा करें',
      markRenewed: 'नवीनीकृत करें',
      markLost: 'लॉस्ट मार्क करें',
      followUp: 'फॉलो-अप',
      call: 'कॉल करें',
      done: 'हो गया',
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
      editProfile: 'પ્રોફાઇલ જુઓ અને સંપાદિત કરો',
    },
    dashboard: {
      goodMorning: 'શુભ સવાર',
      goodAfternoon: 'શુભ બપોર',
      goodEvening: 'શુભ સાંજ',
      startWithOverdueSub: 'બાકી રહેલ રિન્યુઅલથી શરૂઆત કરો. આગળના કૉલ પણ My Day પર ઉપલબ્ધ છે.',
      overdueRenewals: 'અતિશય રિન્યુઅલ',
      pastExpiryStillOpen: 'વીતી ગયેલ સમાપ્તિ, હજુ ખુલ્લું',
      dueIn7Days: '7 દિવસમાં બાકી',
      includingToday: 'આજ સહિત',
      dueIn30Days: '30 દિવસમાં બાકી',
      currentTerm: 'વર્તમાન મુદત',
      premiumAtRisk: 'જોખમમાં પ્રીમિયમ',
      openWithin90Days: '90 દિવસમાં ખુલ્લું',
      pendingTasks: 'બાકી કાર્યો',
      workStillOpen: 'કામ હજુ બાકી',
      expiringPolicies: 'સમાપ્ત થતી પોલિસીઓ',
      upcomingRenewals: 'આગામી રિન્યુઅલ',
      startWithCriticalSub: 'અતિશય બાકી અને મહત્વપૂર્ણ આઇટમ્સથી શરૂઆત કરો, પછી સૌથી નજીકની એક્સપાયરી.',
      viewAllRenewals: 'તમામ રિન્યુઅલ જુઓ',
      requiredToday: 'આજે જરૂરી',
      todaysTasks: 'આજના કાર્યો',
      todaysTasksSub: 'બાકી રહેલ અને આજે ચૂકવવાપાત્ર કાર્યો — દિવસ પૂર્ણ થાય તે પહેલાં પતાવવાનું કામ.',
      viewAllTasks: 'તમામ કાર્યો જુઓ',
      quickActions: 'ઝડપી કાર્યો',
      pipelineTitle: 'રિન્યુઅલ મુદત પાઇપલાઇન',
      topInsurers: 'પોર્ટફોલિયો મુજબ ટોચના વીમાકર્તા',
      policyDistribution: 'વીમા કેટેગરી વર્ગીકરણ',
      healthScorecard: 'બ્રોકરેજ બુક હેલ્થ',
      retentionRate: 'જાળવણી દર',
      activeClients: 'સક્રિય ગ્રાહકો',
      activePolicies: 'સક્રિય પોલિસીઓ',
    },
    table: {
      client: 'ગ્રાહક',
      policy: 'પોલિસી',
      insurer: 'વીમા કંપની',
      premium: 'પ્રીમિયમ',
      expiry: 'સમાપ્તિ',
      daysLeft: 'બાકી દિવસો',
      status: 'સ્થિતિ',
      assignedTo: 'સોંપેલ',
      action: 'પગલું',
      due: 'બાકી',
      priority: 'પ્રાથમિકતા',
      task: 'કાર્ય',
    },
    actions: {
      view: 'જુઓ',
      markComplete: 'પૂર્ણ કરો',
      markRenewed: 'રિન્યૂ કર્યું',
      markLost: 'લોસ્ટ માર્ક કરો',
      followUp: 'ફોલો-અપ',
      call: 'કૉલ કરો',
      done: 'પૂર્ણ',
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
