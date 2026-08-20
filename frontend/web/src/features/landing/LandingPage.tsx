import { useState, useEffect, useRef, type FormEvent } from 'react'
import { Link } from 'react-router-dom'
import { LANDING_DATA, type LandingLocale } from './landingData'
import './landing.css'

export const DEMO_INQUIRY_EMAIL = 'demo@insuorg.com'
const LOCALE_KEY = 'brokeros.language'

export function LandingPage() {
  const [locale, setLocale] = useState<LandingLocale>(() => {
    const saved = localStorage.getItem(LOCALE_KEY)
    if (saved === 'hi' || saved === 'gu' || saved === 'en') {
      return saved
    }
    return 'en'
  })

  const [isLangOpen, setIsLangOpen] = useState(false)
  const langDropdownRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (langDropdownRef.current && !langDropdownRef.current.contains(event.target as Node)) {
        setIsLangOpen(false)
      }
    }
    document.addEventListener('mousedown', handleClickOutside)
    return () => document.removeEventListener('mousedown', handleClickOutside)
  }, [])

  const [activeMockTab, setActiveMockTab] = useState<'overview' | 'renewalFile' | 'myDay' | 'quoteCompare'>('renewalFile')
  const [openFaqIndex, setOpenFaqIndex] = useState<number | null>(0)
  const [billingCycle, setBillingCycle] = useState<'annual' | 'monthly'>('annual')
  const [showTourModal, setShowTourModal] = useState(false)
  const [tourStep, setTourStep] = useState(0)

  // Demo Form State
  const [formSubmitted, setFormSubmitted] = useState(false)
  const [isSubmitting, setIsSubmitting] = useState(false)
  const [formData, setFormData] = useState({
    name: '',
    brokerage: '',
    city: '',
    email: '',
    phone: '',
    role: '',
    bookSize: '',
  })

  const t = LANDING_DATA[locale]

  useEffect(() => {
    localStorage.setItem(LOCALE_KEY, locale)
    document.documentElement.lang = locale
    document.title = 'InsuOrg — Never miss a renewal'
  }, [locale])

  const handleLanguageChange = (newLocale: LandingLocale) => {
    setLocale(newLocale)
  }

  const handleFormSubmit = (e: FormEvent) => {
    e.preventDefault()
    setIsSubmitting(true)

    // Simulate clean client-side submission & inquiry routing
    setTimeout(() => {
      setIsSubmitting(false)
      setFormSubmitted(true)
      
      const mailSubject = encodeURIComponent(`InsuOrg Demo Request: ${formData.brokerage || formData.name}`)
      const mailBody = encodeURIComponent(
        `Hello InsuOrg Team,\n\nI would like to request a 12-minute walkthrough of InsuOrg.\n\n` +
        `Name: ${formData.name}\n` +
        `Brokerage: ${formData.brokerage}\n` +
        `City: ${formData.city}\n` +
        `Work Email: ${formData.email}\n` +
        `Phone/WhatsApp: ${formData.phone}\n` +
        `Role: ${formData.role || 'Principal Broker'}\n` +
        `Active Book Size: ${formData.bookSize || '50-150 Policies'}\n\n` +
        `Thank you.`
      )
      
      console.log('InsuOrg Demo Inquiry Received:', formData)
      console.log(`Routing inquiry to: ${DEMO_INQUIRY_EMAIL}`)
      
      try {
        const mailtoLink = `mailto:${DEMO_INQUIRY_EMAIL}?subject=${mailSubject}&body=${mailBody}`
        const win = window.open(mailtoLink, '_blank')
        if (win) {
          win.focus()
        }
      } catch {
        // Fallback gracefully if popup is blocked
      }
    }, 450)
  }

  const scrollToId = (id: string) => {
    const el = document.getElementById(id)
    if (el) {
      el.scrollIntoView({ behavior: 'smooth' })
    }
  }

  const tourSteps = [
    {
      title: '1. Overdue Expiries & IST Triage',
      desc: 'Sign in to see exactly which policies are overdue or due in 7/30 days, with real ₹ premium at risk calculated in Indian Standard Time.',
    },
    {
      title: '2. The Single Renewal File',
      desc: 'Each expiring policy opens a complete file showing assigned broker, policy details, stage, and next action card.',
    },
    {
      title: '3. Comparative Insurer Quotes',
      desc: 'Log 2–3 insurer quotes (ICICI, HDFC, New India), compare addon covers, and copy formatted WhatsApp preview drafts.',
    },
    {
      title: '4. 1-Click Roll New Term',
      desc: 'When the client confirms, click "Mark Renewed". InsuOrg rolls a clean new term (old expiry + 1 day) while keeping historical audits safe.',
    },
  ]

  return (
    <div className="brokeros-landing">
      {/* 1. STICKY HEADER */}
      <header className="landing-nav-sticky">
        <div className="landing-container">
          <div className="nav-content">
            <Link to="/" className="brand-wrapper">
              <img src="/brand/insuorg-horizontal-reverse.png" alt="InsuOrg" style={{ height: '36px', objectFit: 'contain' }} />
            </Link>

            <nav className="nav-jump-links" aria-label="Main Navigation">
              <button type="button" onClick={() => scrollToId('product')} className="nav-link-item" style={{ background: 'none', border: 'none' }}>{t.nav.product}</button>
              <button type="button" onClick={() => scrollToId('policy-types')} className="nav-link-item" style={{ background: 'none', border: 'none' }}>{t.nav.policyTypes}</button>
              <button type="button" onClick={() => scrollToId('features')} className="nav-link-item" style={{ background: 'none', border: 'none' }}>{t.nav.features}</button>
              <button type="button" onClick={() => scrollToId('how-it-works')} className="nav-link-item" style={{ background: 'none', border: 'none' }}>{t.nav.howItWorks}</button>
              <button type="button" onClick={() => scrollToId('roles')} className="nav-link-item" style={{ background: 'none', border: 'none' }}>{t.nav.roles}</button>
              <button type="button" onClick={() => scrollToId('pricing')} className="nav-link-item" style={{ background: 'none', border: 'none' }}>{t.nav.pricing}</button>
              <button type="button" onClick={() => scrollToId('testimonials')} className="nav-link-item" style={{ background: 'none', border: 'none' }}>{t.nav.testimonials}</button>
              <button type="button" onClick={() => scrollToId('faq')} className="nav-link-item" style={{ background: 'none', border: 'none' }}>{t.nav.faq}</button>
            </nav>

            <div className="nav-actions">
              {/* Custom Language Dropdown Overlay */}
              <div ref={langDropdownRef} className="custom-lang-dropdown">
                <button
                  type="button"
                  className={`custom-lang-trigger ${isLangOpen ? 'open' : ''}`}
                  onClick={() => setIsLangOpen(!isLangOpen)}
                  aria-expanded={isLangOpen}
                >
                  <i className="bi bi-globe2 lang-globe-icon" aria-hidden="true"></i>
                  <span>{locale === 'en' ? 'English' : locale === 'hi' ? 'हिन्दी' : 'ગુજરાતી'}</span>
                  <i className={`bi bi-chevron-down lang-chevron-icon ${isLangOpen ? 'rotate' : ''}`}></i>
                </button>

                {isLangOpen && (
                  <div className="custom-lang-menu">
                    <button
                      type="button"
                      className={`custom-lang-option ${locale === 'en' ? 'active' : ''}`}
                      onClick={() => {
                        handleLanguageChange('en')
                        setIsLangOpen(false)
                      }}
                    >
                      <span>English</span>
                      {locale === 'en' && <i className="bi bi-check2 check-icon"></i>}
                    </button>

                    <button
                      type="button"
                      className={`custom-lang-option ${locale === 'hi' ? 'active' : ''}`}
                      onClick={() => {
                        handleLanguageChange('hi')
                        setIsLangOpen(false)
                      }}
                    >
                      <span>हिन्दी</span>
                      {locale === 'hi' && <i className="bi bi-check2 check-icon"></i>}
                    </button>

                    <button
                      type="button"
                      className={`custom-lang-option ${locale === 'gu' ? 'active' : ''}`}
                      onClick={() => {
                        handleLanguageChange('gu')
                        setIsLangOpen(false)
                      }}
                    >
                      <span>ગુજરાતી</span>
                      {locale === 'gu' && <i className="bi bi-check2 check-icon"></i>}
                    </button>
                  </div>
                )}
              </div>

              {/* Sign In & Demo CTA */}
              <Link to="/login" className="btn-ghost-nav">
                <i className="bi bi-box-arrow-in-right"></i>
                {t.nav.signIn}
              </Link>

              <button type="button" onClick={() => scrollToId('demo-booking')} className="btn-gold">
                {t.nav.requestDemo}
              </button>
            </div>
          </div>
        </div>
      </header>

      {/* 2. HERO SECTION */}
      <section className="landing-hero" id="hero">
        <div className="hero-grid-background"></div>
        <div className="landing-container hero-content-wrapper">
          <div className="hero-text-block">
            <div className="badge-kicker badge-kicker-dark">
              <i className="bi bi-shield-check"></i>
              {t.hero.eyebrow}
            </div>
            <h1 className="hero-title">{t.hero.h1}</h1>
            <p className="hero-subtitle">{t.hero.sub}</p>

            {/* Proof Chips */}
            <div className="proof-chips-row">
              <div className="proof-chip">
                <i className="bi bi-clock-history proof-chip-icon"></i>
                <span>{t.hero.proofChips.ist}</span>
              </div>
              <div className="proof-chip">
                <i className="bi bi-currency-rupee proof-chip-icon"></i>
                <span>{t.hero.proofChips.inr}</span>
              </div>
              <div className="proof-chip">
                <i className="bi bi-person-lock proof-chip-icon"></i>
                <span>{t.hero.proofChips.employeeRole}</span>
              </div>
              <div className="proof-chip">
                <i className="bi bi-file-earmark-spreadsheet proof-chip-icon"></i>
                <span>{t.hero.proofChips.aiReady}</span>
              </div>
            </div>

            {/* CTAs */}
            <div className="hero-ctas-row">
              <button type="button" onClick={() => scrollToId('demo-booking')} className="btn-hero-cta btn-hero-gold">
                <i className="bi bi-calendar2-check"></i>
                {t.hero.ctaPrimary}
              </button>
              <button type="button" onClick={() => scrollToId('live-preview')} className="btn-hero-cta btn-hero-outline">
                <i className="bi bi-play-circle"></i>
                {t.hero.ctaSecondary}
              </button>
              <button type="button" onClick={() => setShowTourModal(true)} className="btn-hero-cta btn-hero-ghost">
                <i className="bi bi-compass"></i>
                {t.hero.ctaTour}
              </button>
            </div>
          </div>

          {/* REALISTIC UI MOCK OF BROKEROS DESK */}
          <div className="hero-mock-container" id="live-preview">
            <div className="mock-window-header">
              <div className="window-dots">
                <span className="dot dot-red"></span>
                <span className="dot dot-yellow"></span>
                <span className="dot dot-green"></span>
              </div>
              <div className="mock-window-title">
                <span className="live-indicator"></span>
                <span>{t.mock.brokerageName} · InsuOrg Workspace</span>
              </div>
              <div style={{ width: '40px' }}></div>
            </div>

            {/* Mock Sub-Tabs */}
            <div className="mock-tabs-nav">
              <button
                type="button"
                className={`mock-tab-btn ${activeMockTab === 'overview' ? 'active' : ''}`}
                onClick={() => setActiveMockTab('overview')}
              >
                <i className="bi bi-grid-1x2 me-2"></i>
                {t.mock.tabs.overview}
              </button>
              <button
                type="button"
                className={`mock-tab-btn ${activeMockTab === 'renewalFile' ? 'active' : ''}`}
                onClick={() => setActiveMockTab('renewalFile')}
              >
                <i className="bi bi-folder2-open me-2"></i>
                {t.mock.tabs.renewalFile}
              </button>
              <button
                type="button"
                className={`mock-tab-btn ${activeMockTab === 'myDay' ? 'active' : ''}`}
                onClick={() => setActiveMockTab('myDay')}
              >
                <i className="bi bi-sun me-2"></i>
                {t.mock.tabs.myDay}
              </button>
              <button
                type="button"
                className={`mock-tab-btn ${activeMockTab === 'quoteCompare' ? 'active' : ''}`}
                onClick={() => setActiveMockTab('quoteCompare')}
              >
                <i className="bi bi-calculator me-2"></i>
                {t.mock.tabs.quoteCompare}
              </button>
            </div>

            {/* Mock Workspace Body */}
            <div className="mock-workspace-body">
              {/* Sidebar */}
              <aside className="mock-sidebar">
                <div className={`mock-sidebar-item ${activeMockTab === 'overview' ? 'active' : ''}`} onClick={() => setActiveMockTab('overview')}>
                  <span><i className="bi bi-speedometer2 me-2"></i>Overview</span>
                  <span className="mock-badge">14</span>
                </div>
                <div className={`mock-sidebar-item ${activeMockTab === 'myDay' ? 'active' : ''}`} onClick={() => setActiveMockTab('myDay')}>
                  <span><i className="bi bi-check2-square me-2"></i>My Day</span>
                  <span className="mock-badge mock-badge-warn">3</span>
                </div>
                <div className="mock-sidebar-item">
                  <span><i className="bi bi-people me-2"></i>Clients</span>
                  <span style={{ fontSize: '0.75rem', color: '#8da3b5' }}>184</span>
                </div>
                <div className="mock-sidebar-item">
                  <span><i className="bi bi-file-earmark-text me-2"></i>Policies</span>
                  <span style={{ fontSize: '0.75rem', color: '#8da3b5' }}>240</span>
                </div>
                <div className={`mock-sidebar-item ${activeMockTab === 'renewalFile' ? 'active' : ''}`} onClick={() => setActiveMockTab('renewalFile')}>
                  <span><i className="bi bi-arrow-repeat me-2"></i>Renewals</span>
                  <span className="mock-badge">14</span>
                </div>
                <div className={`mock-sidebar-item ${activeMockTab === 'quoteCompare' ? 'active' : ''}`} onClick={() => setActiveMockTab('quoteCompare')}>
                  <span><i className="bi bi-card-checklist me-2"></i>Quotes</span>
                  <span style={{ fontSize: '0.75rem', color: '#c9a227' }}>3</span>
                </div>
                <div className="mock-sidebar-item">
                  <span><i className="bi bi-list-task me-2"></i>Tasks</span>
                  <span style={{ fontSize: '0.75rem', color: '#8da3b5' }}>9</span>
                </div>
              </aside>

              {/* Main Content Area */}
              <main className="mock-main-content">
                {/* TAB 1: OVERVIEW */}
                {activeMockTab === 'overview' && (
                  <div>
                    <div className="mock-metrics-row">
                      <div className="mock-metric-card danger">
                        <span className="mock-metric-lbl">{t.mock.metrics.overdueLabel}</span>
                        <span className="mock-metric-val">{t.mock.metrics.overdueVal}</span>
                        <span className="mock-metric-sub">{t.mock.metrics.overdueSub}</span>
                      </div>
                      <div className="mock-metric-card warn">
                        <span className="mock-metric-lbl">{t.mock.metrics.due7Label}</span>
                        <span className="mock-metric-val">{t.mock.metrics.due7Val}</span>
                        <span className="mock-metric-sub">{t.mock.metrics.due7Sub}</span>
                      </div>
                      <div className="mock-metric-card gold">
                        <span className="mock-metric-lbl">{t.mock.metrics.due30Label}</span>
                        <span className="mock-metric-val">{t.mock.metrics.due30Val}</span>
                        <span className="mock-metric-sub">{t.mock.metrics.due30Sub}</span>
                      </div>
                      <div className="mock-metric-card ok">
                        <span className="mock-metric-lbl">{t.mock.metrics.atRiskLabel}</span>
                        <span className="mock-metric-val">{t.mock.metrics.atRiskVal}</span>
                        <span className="mock-metric-sub">{t.mock.metrics.atRiskSub}</span>
                      </div>
                    </div>

                    <div className="renewal-file-card" style={{ padding: '1.25rem' }}>
                      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
                        <strong style={{ fontSize: '0.9375rem', color: '#071824' }}>
                          <i className="bi bi-exclamation-triangle-fill text-danger me-2"></i>
                          Urgent Renewal Queue (Top Overdue Expiries)
                        </strong>
                        <span style={{ fontSize: '0.75rem', color: '#5a6b78' }}>Sorted by Expiry Date (IST)</span>
                      </div>
                      <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
                        <div style={{ display: 'flex', justifyContent: 'space-between', padding: '0.75rem', background: '#fef3f2', borderRadius: '8px', border: '1px solid #fee4e2' }}>
                          <div>
                            <strong style={{ color: '#071824' }}>Malabar Spices Pvt Ltd</strong>
                            <div style={{ fontSize: '0.75rem', color: '#5a6b78' }}>POL-D008 · Fire & Special Perils · ICICI Lombard</div>
                          </div>
                          <div style={{ textAlign: 'right' }}>
                            <span className="chip-status-danger">27d overdue</span>
                            <div style={{ fontSize: '0.8125rem', fontWeight: '700', marginTop: '0.2rem' }}>₹4,85,000</div>
                          </div>
                        </div>

                        <div style={{ display: 'flex', justifyContent: 'space-between', padding: '0.75rem', background: '#ffffff', borderRadius: '8px', border: '1px solid #e3eaf0' }}>
                          <div>
                            <strong style={{ color: '#071824' }}>Sterling Polyfilms Ltd</strong>
                            <div style={{ fontSize: '0.75rem', color: '#5a6b78' }}>POL-G042 · Group Health GMC · Star Health</div>
                          </div>
                          <div style={{ textAlign: 'right' }}>
                            <span style={{ background: '#fffaeb', color: '#b54708', fontWeight: '700', padding: '0.2rem 0.5rem', borderRadius: '100px', fontSize: '0.75rem' }}>3d left</span>
                            <div style={{ fontSize: '0.8125rem', fontWeight: '700', marginTop: '0.2rem' }}>₹12,40,000</div>
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                )}

                {/* TAB 2: RENEWAL FILE */}
                {activeMockTab === 'renewalFile' && (
                  <div className="renewal-file-card">
                    <div className="file-header-strip">
                      <div className="file-title-left">
                        <h3>{t.mock.clientName}</h3>
                        <div className="file-meta-row">
                          <span><strong>File:</strong> {t.mock.fileNumber}</span>
                          <span>·</span>
                          <span><strong>Policy:</strong> {t.mock.policyType}</span>
                          <span>·</span>
                          <span><strong>Insurer:</strong> {t.mock.insurer}</span>
                        </div>
                      </div>
                      <div style={{ display: 'flex', gap: '0.5rem', alignItems: 'center' }}>
                        <span className="chip-status-danger">{t.mock.statusOverdue}</span>
                        <span className="chip-status-active">{t.mock.statusActive}</span>
                      </div>
                    </div>

                    <div className="file-details-grid">
                      <div className="detail-col">
                        <label>Current Premium</label>
                        <span>{t.mock.premium}</span>
                      </div>
                      <div className="detail-col">
                        <label>File Owner</label>
                        <span>{t.mock.owner}</span>
                      </div>
                      <div className="detail-col">
                        <label>Current Stage</label>
                        <span style={{ color: '#b54708' }}>Client Decision Pending</span>
                      </div>
                    </div>

                    <div className="next-action-box">
                      <div className="next-action-title">
                        <i className="bi bi-arrow-right-circle me-1"></i>
                        Next Required Action
                      </div>
                      <div className="next-action-desc">{t.mock.nextAction}</div>
                    </div>

                    {/* Quotations comparison */}
                    <div className="quotes-comparison-wrap">
                      <div className="quotes-heading">{t.mock.quotesTitle}</div>
                      <div className="quotes-cards-grid">
                        {t.mock.quotes.map((q, idx) => (
                          <div key={idx} className={`quote-chip-card ${q.selected ? 'selected' : ''}`}>
                            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                              <span className="quote-insurer-name">{q.insurer}</span>
                              {q.badge && <span className="quote-tag">{q.badge}</span>}
                            </div>
                            <span className="quote-price">{q.amount}</span>
                            <span style={{ fontSize: '0.72rem', color: q.selected ? '#1b7a4e' : '#5a6b78' }}>
                              {q.features}
                            </span>
                          </div>
                        ))}
                      </div>
                    </div>

                    <div className="file-actions-footer">
                      <span className="file-timeline-hint">
                        <i className="bi bi-clock-history me-1"></i>
                        {t.mock.timeline}
                      </span>
                      <div style={{ display: 'flex', gap: '0.75rem' }}>
                        <button type="button" className="btn-ghost-nav" style={{ color: '#071824', borderColor: '#cbd8e2', background: '#ffffff', fontSize: '0.8125rem' }}>
                          <i className="bi bi-whatsapp text-success me-1"></i>
                          {t.mock.sharePreview}
                        </button>
                        <button type="button" className="btn-gold" style={{ fontSize: '0.8125rem', padding: '0.45rem 1rem' }}>
                          <i className="bi bi-check2-circle me-1"></i>
                          {t.mock.markRenewed}
                        </button>
                      </div>
                    </div>
                  </div>
                )}

                {/* TAB 3: MY DAY DESK */}
                {activeMockTab === 'myDay' && (
                  <div>
                    <div style={{ background: '#ffffff', padding: '1.25rem', borderRadius: '12px', border: '1px solid #e3eaf0', marginBottom: '1rem' }}>
                      <h4 style={{ fontSize: '1.05rem', fontWeight: '800', color: '#071824', margin: '0 0 0.25rem' }}>
                        Good morning, Rajesh
                      </h4>
                      <p style={{ fontSize: '0.8125rem', color: '#5a6b78', margin: 0 }}>
                        Here is your priority queue for today in Indian Standard Time (IST). Start with overdue callbacks.
                      </p>
                    </div>

                    <div className="mock-tasks-list">
                      {t.mock.myDayTasks.map((tk, idx) => (
                        <div key={idx} className="mock-task-item">
                          <div className="task-left-info">
                            <span className="task-time-badge">{tk.time}</span>
                            <div>
                              <div className="task-title-text">{tk.task}</div>
                              <span className="task-client-name">{tk.client}</span>
                            </div>
                          </div>
                          <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
                            <span
                              style={{
                                fontSize: '0.75rem',
                                fontWeight: '750',
                                padding: '0.25rem 0.65rem',
                                borderRadius: '100px',
                                background: tk.tagType === 'danger' ? '#fef3f2' : tk.tagType === 'warn' ? '#fffaeb' : '#eef6ee',
                                color: tk.tagType === 'danger' ? '#b42318' : tk.tagType === 'warn' ? '#b54708' : '#1b7a4e',
                              }}
                            >
                              {tk.tag}
                            </span>
                            <button type="button" className="btn-ghost-nav" style={{ color: '#071824', borderColor: '#e3eaf0', padding: '0.35rem 0.65rem', fontSize: '0.75rem' }}>
                              Call
                            </button>
                            <button type="button" className="btn-gold" style={{ padding: '0.35rem 0.75rem', fontSize: '0.75rem' }}>
                              Done
                            </button>
                          </div>
                        </div>
                      ))}
                    </div>
                  </div>
                )}

                {/* TAB 4: QUOTE COMPARE MATRIX */}
                {activeMockTab === 'quoteCompare' && (
                  <div className="renewal-file-card">
                    <h4 style={{ fontSize: '1.15rem', fontWeight: '800', color: '#071824', marginBottom: '0.5rem' }}>
                      Insurer Quote Comparison Matrix · POL-D008
                    </h4>
                    <p style={{ fontSize: '0.8125rem', color: '#5a6b78', marginBottom: '1.25rem' }}>
                      Compare cover terms, deductible excess, and net premium across competing insurers before client presentation.
                    </p>
                    <div style={{ overflowX: 'auto' }}>
                      <table style={{ width: '100%', fontSize: '0.8125rem', borderCollapse: 'collapse' }}>
                        <thead>
                          <tr style={{ background: '#0b2b43', color: '#ffffff', textAlign: 'left' }}>
                            <th style={{ padding: '0.75rem' }}>Parameter</th>
                            <th style={{ padding: '0.75rem' }}>ICICI Lombard (Selected)</th>
                            <th style={{ padding: '0.75rem' }}>HDFC ERGO (L1)</th>
                            <th style={{ padding: '0.75rem' }}>New India Assurance</th>
                          </tr>
                        </thead>
                        <tbody>
                          <tr style={{ borderBottom: '1px solid #e3eaf0' }}>
                            <td style={{ padding: '0.75rem', fontWeight: '700' }}>Basic Net Premium</td>
                            <td style={{ padding: '0.75rem', color: '#1b7a4e', fontWeight: '800' }}>₹4,68,200</td>
                            <td style={{ padding: '0.75rem', fontWeight: '700' }}>₹4,52,000</td>
                            <td style={{ padding: '0.75rem' }}>₹4,91,500</td>
                          </tr>
                          <tr style={{ borderBottom: '1px solid #e3eaf0' }}>
                            <td style={{ padding: '0.75rem', fontWeight: '700' }}>Earthquake (Zone II)</td>
                            <td style={{ padding: '0.75rem' }}>✓ Included (₹35k)</td>
                            <td style={{ padding: '0.75rem' }}>✗ Optional Addon</td>
                            <td style={{ padding: '0.75rem' }}>✓ Included</td>
                          </tr>
                          <tr style={{ borderBottom: '1px solid #e3eaf0' }}>
                            <td style={{ padding: '0.75rem', fontWeight: '700' }}>Terrorism Cover</td>
                            <td style={{ padding: '0.75rem' }}>✓ Included</td>
                            <td style={{ padding: '0.75rem' }}>✓ Included</td>
                            <td style={{ padding: '0.75rem' }}>✓ Included</td>
                          </tr>
                          <tr>
                            <td style={{ padding: '0.75rem', fontWeight: '700' }}>Calculated Commission (15%)</td>
                            <td style={{ padding: '0.75rem', color: '#8c6d0d', fontWeight: '800' }}>₹70,230</td>
                            <td style={{ padding: '0.75rem', color: '#8c6d0d', fontWeight: '800' }}>₹67,800</td>
                            <td style={{ padding: '0.75rem', color: '#8c6d0d', fontWeight: '800' }}>₹73,725</td>
                          </tr>
                        </tbody>
                      </table>
                    </div>
                  </div>
                )}
              </main>
            </div>
          </div>
        </div>
      </section>

      {/* 3. PROBLEM STRIP */}
      <section className="landing-problem-section" id="product">
        <div className="landing-container">
          <div className="section-heading-center">
            <div className="badge-kicker">{t.problem.badge}</div>
            <h2>{t.problem.title}</h2>
            <p>{t.problem.subtitle}</p>
          </div>

          <div className="problem-grid-3">
            {t.problem.cards.map((card, idx) => (
              <div key={idx} className="problem-card">
                <span className="problem-tag-pill">{card.tag}</span>
                <div className="problem-icon-wrap">
                  <i className={idx === 0 ? 'bi bi-person-x' : idx === 1 ? 'bi bi-card-checklist' : 'bi bi-file-earmark-diff'}></i>
                </div>
                <h3>{card.title}</h3>
                <p>{card.description}</p>
              </div>
            ))}
          </div>

          <div className="problem-conclusion-banner">
            <div className="conclusion-text">
              <i className="bi bi-check-circle-fill text-warning me-2"></i>
              {t.problem.conclusion}
            </div>
            <span className="conclusion-badge">System of Record</span>
          </div>
        </div>
      </section>

      {/* 4. SUPPORTED POLICY LINES SECTION */}
      <section className="landing-policy-types-section" id="policy-types">
        <div className="landing-container">
          <div className="section-heading-center">
            <div className="badge-kicker">{t.policyTypes.badge}</div>
            <h2>{t.policyTypes.title}</h2>
            <p>{t.policyTypes.subtitle}</p>
          </div>

          <div className="policy-types-grid">
            {t.policyTypes.types.map((type, idx) => (
              <div key={idx} className="policy-type-card">
                <div className="policy-type-icon">
                  <i className={`bi bi-${type.icon}`}></i>
                </div>
                <h3>{type.name}</h3>
                <p>{type.desc}</p>
                <span className="policy-sample-badge">{type.sample}</span>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* 5. FEATURES GRID (6 ITEMS) */}
      <section className="landing-features-section" id="features">
        <div className="landing-container">
          <div className="section-heading-center">
            <div className="badge-kicker">{t.features.badge}</div>
            <h2>{t.features.title}</h2>
            <p>{t.features.subtitle}</p>
          </div>

          <div className="features-grid-6">
            {t.features.items.map((feat, idx) => (
              <div key={idx} className="feature-item-card">
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1rem' }}>
                  <div className="feature-icon-box" style={{ marginBottom: 0 }}>
                    <i className={`bi bi-${feat.icon}`}></i>
                  </div>
                  {feat.highlight && (
                    <span style={{ fontSize: '0.6875rem', fontWeight: '800', background: 'rgba(201, 162, 39, 0.12)', color: '#8c6d0d', padding: '0.2rem 0.5rem', borderRadius: '100px' }}>
                      {feat.highlight}
                    </span>
                  )}
                </div>
                <h3>{feat.title}</h3>
                <p>{feat.description}</p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* 6. HOW IT WORKS (3 STEPS) */}
      <section className="landing-how-section" id="how-it-works">
        <div className="landing-container">
          <div className="section-heading-center">
            <div className="badge-kicker">{t.howItWorks.badge}</div>
            <h2>{t.howItWorks.title}</h2>
            <p>{t.howItWorks.subtitle}</p>
          </div>

          <div className="steps-timeline-grid">
            {t.howItWorks.steps.map((step, idx) => (
              <div key={idx} className="step-card">
                <div className="step-number-pill">{step.step}</div>
                <h3>{step.title}</h3>
                <p>{step.description}</p>
                <span className="step-meta-tag">{step.meta}</span>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* 7. ROLES SECTION */}
      <section className="landing-roles-section" id="roles">
        <div className="landing-container">
          <div className="section-heading-center">
            <div className="badge-kicker">{t.roles.badge}</div>
            <h2>{t.roles.title}</h2>
            <p>{t.roles.subtitle}</p>
          </div>

          <div className="roles-grid-3">
            {t.roles.items.map((r, idx) => (
              <div key={idx} className="role-card">
                <div className="role-header-top">
                  <h3>{r.role}</h3>
                  <span className="role-scope-badge">{r.scopeBadge}</span>
                </div>
                <div className="role-tagline">{r.tagline}</div>
                <ul className="role-permissions-list">
                  {r.permissions.map((perm, pIdx) => (
                    <li key={pIdx} className="role-permission-item">
                      <i className="bi bi-shield-check role-check-icon"></i>
                      <span>{perm}</span>
                    </li>
                  ))}
                </ul>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* 8. TRANSPARENT PRICING SECTION */}
      <section className="landing-pricing-section" id="pricing">
        <div className="landing-container">
          <div className="section-heading-center">
            <div className="badge-kicker">{t.pricing.badge}</div>
            <h2>{t.pricing.title}</h2>
            <p>{t.pricing.subtitle}</p>
          </div>

          <div className="pricing-billing-toggle">
            <div className="billing-toggle-pill">
              <button
                type="button"
                className={`billing-toggle-btn ${billingCycle === 'annual' ? 'active' : ''}`}
                onClick={() => setBillingCycle('annual')}
              >
                {t.pricing.billingToggleAnnual}
              </button>
              <button
                type="button"
                className={`billing-toggle-btn ${billingCycle === 'monthly' ? 'active' : ''}`}
                onClick={() => setBillingCycle('monthly')}
              >
                {t.pricing.billingToggleMonthly}
              </button>
            </div>
            <span className="discount-tag">{t.pricing.annualSavings}</span>
          </div>

          <div className="pricing-plans-grid">
            {t.pricing.plans.map((plan, idx) => {
              const isAnnual = billingCycle === 'annual'
              const priceDisplay = isAnnual ? plan.priceAnnual : plan.priceMonthly
              const periodDisplay = isAnnual ? plan.periodAnnual : plan.periodMonthly

              return (
                <div key={idx} className={`pricing-plan-card ${plan.isPopular ? 'popular' : ''}`}>
                  {plan.badge && <span className="plan-popular-ribbon">{plan.badge}</span>}
                  <div className="plan-header">
                    <h3 className="plan-name">{plan.name}</h3>
                    <p className="plan-tagline">{plan.tagline}</p>
                  </div>

                  <div className="plan-price-block">
                    <span className="plan-price-num">{priceDisplay}</span>
                    <span className="plan-price-period">{periodDisplay}</span>
                  </div>

                <div className="plan-limits-strip">
                  <div><i className="bi bi-files me-1"></i>{plan.policyLimit}</div>
                  <div><i className="bi bi-people me-1"></i>{plan.userLimit}</div>
                </div>

                <ul className="plan-features-list">
                  {plan.features.map((feat, fIdx) => (
                    <li key={fIdx} className="plan-feature-item">
                      <i className="bi bi-check2-circle"></i>
                      <span>{feat}</span>
                    </li>
                  ))}
                </ul>

                <button
                  type="button"
                  onClick={() => scrollToId('demo-booking')}
                  className={plan.isPopular ? 'btn-pricing-card-popular' : 'btn-pricing-card'}
                >
                  {plan.ctaText}
                </button>
              </div>
            )
          })}
          </div>
          <div style={{ textAlign: 'center', marginTop: '2rem', fontSize: '0.8125rem', color: '#5a6b78' }}>
            {t.pricing.customNote}
          </div>
        </div>
      </section>

      {/* 9. TESTIMONIALS / BROKERS TRUST */}
      <section className="landing-testimonials-section" id="testimonials">
        <div className="landing-container">
          <div className="section-heading-center">
            <div className="badge-kicker">{t.testimonials.badge}</div>
            <h2>{t.testimonials.title}</h2>
            <p>{t.testimonials.subtitle}</p>
          </div>

          <div className="testimonials-grid-3">
            {t.testimonials.items.map((testi, idx) => (
              <div key={idx} className="testimonial-card">
                <div className="testimonial-stars">
                  {[...Array(testi.rating)].map((_, sIdx) => (
                    <i key={sIdx} className="bi bi-star-fill"></i>
                  ))}
                </div>
                <blockquote className="testimonial-quote">“{testi.quote}”</blockquote>
                <div className="testimonial-author-row">
                  <div>
                    <div className="author-name">{testi.name}</div>
                    <div className="author-org">{testi.title} · {testi.brokerage}</div>
                    <div style={{ fontSize: '0.6875rem', color: '#8da3b5' }}>{testi.city}</div>
                  </div>
                  <span className="testimonial-metric-pill">{testi.metric}</span>
                </div>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* 10. WHO IT IS FOR / NOT FOR */}
      <section className="landing-fit-section">
        <div className="landing-container">
          <div className="section-heading-center">
            <div className="badge-kicker">{t.fit.badge}</div>
            <h2>{t.fit.title}</h2>
          </div>

          <div className="fit-comparison-grid">
            {/* For */}
            <div className="fit-column-card for-card">
              <div className="fit-column-heading">
                <i className="bi bi-check-circle-fill text-success fs-3"></i>
                <h3>{t.fit.forTitle}</h3>
              </div>
              <ul className="fit-list">
                {t.fit.forItems.map((item, idx) => (
                  <li key={idx} className="fit-item">
                    <i className="bi bi-check2 fit-icon-yes"></i>
                    <span>{item}</span>
                  </li>
                ))}
              </ul>
            </div>

            {/* Not For */}
            <div className="fit-column-card not-card">
              <div className="fit-column-heading">
                <i className="bi bi-x-circle-fill text-danger fs-3"></i>
                <h3>{t.fit.notForTitle}</h3>
              </div>
              <ul className="fit-list">
                {t.fit.notForItems.map((item, idx) => (
                  <li key={idx} className="fit-item">
                    <i className="bi bi-x-lg fit-icon-no"></i>
                    <span>{item}</span>
                  </li>
                ))}
              </ul>
            </div>
          </div>
        </div>
      </section>

      {/* 11. FAQ SECTION */}
      <section className="landing-faq-section" id="faq">
        <div className="landing-container">
          <div className="section-heading-center">
            <div className="badge-kicker">{t.faq.badge}</div>
            <h2>{t.faq.title}</h2>
            <p>{t.faq.subtitle}</p>
          </div>

          <div className="faq-accordion-list">
            {t.faq.items.map((item, idx) => {
              const isOpen = openFaqIndex === idx
              return (
                <div key={idx} className={`faq-item-card ${isOpen ? 'open' : ''}`}>
                  <button
                    type="button"
                    className="faq-question-btn"
                    onClick={() => setOpenFaqIndex(isOpen ? null : idx)}
                    aria-expanded={isOpen}
                  >
                    <span>{item.q}</span>
                    <i className="bi bi-chevron-down faq-toggle-icon"></i>
                  </button>
                  {isOpen && (
                    <div className="faq-answer-pane">
                      <p style={{ margin: 0 }}>{item.a}</p>
                    </div>
                  )}
                </div>
              )
            })}
          </div>
        </div>
      </section>

      {/* 12. FINAL CTA BAND & BOOKING FORM */}
      <section className="landing-cta-band" id="demo-booking">
        <div className="landing-container">
          <div className="cta-band-grid">
            <div className="cta-text-left">
              <div className="badge-kicker badge-kicker-dark">
                <i className="bi bi-calendar-event"></i>
                12-Minute Live Walkthrough
              </div>
              <h2>{t.ctaBand.title}</h2>
              <p>{t.ctaBand.sub}</p>

              <div className="cta-guarantee-points">
                <div className="guarantee-point">
                  <i className="bi bi-check2-circle"></i>
                  <span>No credit card required. Zero sales pressure.</span>
                </div>
                <div className="guarantee-point">
                  <i className="bi bi-check2-circle"></i>
                  <span>Inspect the sample Apex Insurance Brokers book live.</span>
                </div>
                <div className="guarantee-point">
                  <i className="bi bi-check2-circle"></i>
                  <span>See Excel/CSV bulk import mapped in 5 minutes.</span>
                </div>
              </div>
            </div>

            {/* Booking Form Card */}
            <div className="demo-form-card">
              {formSubmitted ? (
                <div className="form-success-box">
                  <div className="success-icon-wrap">
                    <i className="bi bi-check2-circle"></i>
                  </div>
                  <h3>{t.ctaBand.form.successTitle}</h3>
                  <p>{t.ctaBand.form.successMsg}</p>
                  <button
                    type="button"
                    className="btn-gold"
                    onClick={() => {
                      setFormSubmitted(false)
                      setFormData({
                        name: '',
                        brokerage: '',
                        city: '',
                        email: '',
                        phone: '',
                        role: '',
                        bookSize: '',
                      })
                    }}
                  >
                    {t.ctaBand.form.resetBtn}
                  </button>
                </div>
              ) : (
                <form onSubmit={handleFormSubmit}>
                  <div className="demo-form-grid">
                    <div className="form-group-field">
                      <label htmlFor="form-name">{t.ctaBand.form.name} *</label>
                      <input
                        id="form-name"
                        type="text"
                        required
                        placeholder={t.ctaBand.form.namePlaceholder}
                        value={formData.name}
                        onChange={(e) => setFormData({ ...formData, name: e.target.value })}
                      />
                    </div>

                    <div className="form-group-field">
                      <label htmlFor="form-brokerage">{t.ctaBand.form.brokerage} *</label>
                      <input
                        id="form-brokerage"
                        type="text"
                        required
                        placeholder={t.ctaBand.form.brokeragePlaceholder}
                        value={formData.brokerage}
                        onChange={(e) => setFormData({ ...formData, brokerage: e.target.value })}
                      />
                    </div>

                    <div className="form-group-field">
                      <label htmlFor="form-city">{t.ctaBand.form.city} *</label>
                      <input
                        id="form-city"
                        type="text"
                        required
                        placeholder={t.ctaBand.form.cityPlaceholder}
                        value={formData.city}
                        onChange={(e) => setFormData({ ...formData, city: e.target.value })}
                      />
                    </div>

                    <div className="form-group-field">
                      <label htmlFor="form-role">{t.ctaBand.form.role}</label>
                      <select
                        id="form-role"
                        value={formData.role}
                        onChange={(e) => setFormData({ ...formData, role: e.target.value })}
                      >
                        <option value="">Select your role</option>
                        {t.ctaBand.form.roleOptions.map((opt, idx) => (
                          <option key={idx} value={opt}>{opt}</option>
                        ))}
                      </select>
                    </div>

                    <div className="form-group-field">
                      <label htmlFor="form-email">{t.ctaBand.form.email} *</label>
                      <input
                        id="form-email"
                        type="email"
                        required
                        placeholder={t.ctaBand.form.emailPlaceholder}
                        value={formData.email}
                        onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                      />
                    </div>

                    <div className="form-group-field">
                      <label htmlFor="form-phone">{t.ctaBand.form.phone} *</label>
                      <input
                        id="form-phone"
                        type="tel"
                        required
                        placeholder={t.ctaBand.form.phonePlaceholder}
                        value={formData.phone}
                        onChange={(e) => setFormData({ ...formData, phone: e.target.value })}
                      />
                    </div>

                    <div className="form-group-field form-full-width">
                      <label htmlFor="form-book-size">{t.ctaBand.form.bookSize}</label>
                      <select
                        id="form-book-size"
                        value={formData.bookSize}
                        onChange={(e) => setFormData({ ...formData, bookSize: e.target.value })}
                      >
                        <option value="">Select policy book size</option>
                        {t.ctaBand.form.bookSizeOptions.map((opt, idx) => (
                          <option key={idx} value={opt}>{opt}</option>
                        ))}
                      </select>
                    </div>
                  </div>

                  <div className="form-submit-row">
                    <button
                      type="submit"
                      disabled={isSubmitting}
                      className="btn-gold btn-gold-lg"
                      style={{ width: '100%' }}
                    >
                      {isSubmitting ? t.ctaBand.form.submitting : t.ctaBand.form.submitBtn}
                    </button>
                    <span className="form-legal-note">
                      We value confidentiality. Your brokerage client records will never be shared.
                    </span>
                  </div>
                </form>
              )}
            </div>
          </div>
        </div>
      </section>

      {/* 13. FOOTER */}
      <footer className="landing-footer">
        <div className="landing-container">
          <div className="footer-top-row">
            <div className="footer-brand-col">
              <div className="brand-wrapper">
                <img src="/brand/insuorg-horizontal-reverse.png" alt="InsuOrg" style={{ height: '34px', objectFit: 'contain' }} />
              </div>
              <p className="footer-brand-tagline">{t.footer.tagline}</p>
            </div>

            <div className="footer-links-grid">
              <div className="footer-link-group">
                <span>Product</span>
                <button type="button" onClick={() => scrollToId('product')} style={{ background: 'none', border: 'none', color: '#8da3b5', textAlign: 'left', padding: 0, cursor: 'pointer' }}>{t.footer.links.product}</button>
                <button type="button" onClick={() => scrollToId('pricing')} style={{ background: 'none', border: 'none', color: '#8da3b5', textAlign: 'left', padding: 0, cursor: 'pointer' }}>{t.footer.links.pricing}</button>
                <button type="button" onClick={() => scrollToId('how-it-works')} style={{ background: 'none', border: 'none', color: '#8da3b5', textAlign: 'left', padding: 0, cursor: 'pointer' }}>{t.nav.howItWorks}</button>
                <button type="button" onClick={() => scrollToId('roles')} style={{ background: 'none', border: 'none', color: '#8da3b5', textAlign: 'left', padding: 0, cursor: 'pointer' }}>{t.nav.roles}</button>
                <button type="button" onClick={() => scrollToId('faq')} style={{ background: 'none', border: 'none', color: '#8da3b5', textAlign: 'left', padding: 0, cursor: 'pointer' }}>{t.nav.faq}</button>
              </div>

              <div className="footer-link-group">
                <span>Access</span>
                <Link to="/login">{t.footer.links.signIn}</Link>
                <button type="button" onClick={() => scrollToId('demo-booking')} style={{ background: 'none', border: 'none', color: '#8da3b5', textAlign: 'left', padding: 0, cursor: 'pointer' }}>{t.hero.ctaPrimary}</button>
              </div>

              <div className="footer-link-group">
                <span>Trust & Contact</span>
                <button type="button" onClick={() => scrollToId('demo-booking')} style={{ background: 'none', border: 'none', color: '#8da3b5', textAlign: 'left', padding: 0, cursor: 'pointer' }}>{t.footer.links.privacy}</button>
                <a href={`mailto:${DEMO_INQUIRY_EMAIL}`}>{t.footer.links.contact}</a>
              </div>
            </div>
          </div>

          <div className="footer-bottom-bar">
            <div>{t.footer.rights}</div>
            <div className="footer-disclaimer-text">{t.footer.disclaimer}</div>
          </div>
        </div>
      </footer>

      {/* 14. FLOATING QUICK CONSULTATION / WHATSAPP PILL */}
      <a
        href="#demo-booking"
        onClick={(e) => {
          e.preventDefault()
          scrollToId('demo-booking')
        }}
        className="floating-quick-action"
        aria-label="Request Quick Demo"
      >
        <span className="floating-whatsapp-dot"></span>
        <i className="bi bi-chat-dots me-1"></i>
        <span>Request 12-Min Walkthrough</span>
      </a>

      {/* 15. INTERACTIVE TOUR MODAL */}
      {showTourModal && (
        <div
          style={{
            position: 'fixed',
            inset: 0,
            zIndex: 10000,
            background: 'rgba(7, 24, 36, 0.8)',
            backdropFilter: 'blur(8px)',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            padding: '1.5rem',
          }}
        >
          <div
            style={{
              background: '#ffffff',
              borderRadius: '20px',
              maxWidth: '620px',
              width: '100%',
              padding: '2.5rem',
              boxShadow: '0 25px 60px rgba(0,0,0,0.5)',
              position: 'relative',
            }}
          >
            <button
              type="button"
              onClick={() => setShowTourModal(false)}
              style={{
                position: 'absolute',
                top: '1.25rem',
                right: '1.25rem',
                border: 'none',
                background: 'none',
                fontSize: '1.5rem',
                cursor: 'pointer',
                color: '#5a6b78',
              }}
            >
              ×
            </button>
            <div className="badge-kicker">Interactive Tour Step {tourStep + 1} of {tourSteps.length}</div>
            <h3 style={{ fontSize: '1.45rem', fontWeight: '800', color: '#071824', marginBottom: '0.75rem' }}>
              {tourSteps[tourStep].title}
            </h3>
            <p style={{ fontSize: '1rem', color: '#5a6b78', lineHeight: 1.6, marginBottom: '2rem' }}>
              {tourSteps[tourStep].desc}
            </p>

            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <button
                type="button"
                disabled={tourStep === 0}
                onClick={() => setTourStep((s) => Math.max(0, s - 1))}
                className="btn-modal-secondary"
              >
                <i className="bi bi-arrow-left"></i> Previous
              </button>

              <div style={{ display: 'flex', gap: '6px' }}>
                {tourSteps.map((_, i) => (
                  <span
                    key={i}
                    style={{
                      width: '8px',
                      height: '8px',
                      borderRadius: '50%',
                      background: i === tourStep ? '#c9a227' : '#e3eaf0',
                    }}
                  />
                ))}
              </div>

              {tourStep < tourSteps.length - 1 ? (
                <button
                  type="button"
                  onClick={() => setTourStep((s) => s + 1)}
                  className="btn-gold"
                >
                  Next Step <i className="bi bi-arrow-right ms-1"></i>
                </button>
              ) : (
                <button
                  type="button"
                  onClick={() => {
                    setShowTourModal(false)
                    scrollToId('demo-booking')
                  }}
                  className="btn-gold"
                >
                  Book Walkthrough <i className="bi bi-calendar-check ms-1"></i>
                </button>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  )
}
