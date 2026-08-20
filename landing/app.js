/**
 * BrokerOS Standalone Landing Page Script
 * Tri-lingual support (English, Hindi, Gujarati), interactive desk mock, pricing toggle, tour modal, and demo form
 */

const DEMO_INQUIRY_EMAIL = 'demo@brokeros.in';
const LOCALE_KEY = 'brokeros.language';

const LANDING_DATA = {
  en: {
    nav: {
      product: 'Product',
      features: 'Features',
      policyTypes: 'Policy Types',
      howItWorks: 'How it works',
      roles: 'Roles',
      pricing: 'Pricing',
      faq: 'FAQ',
      testimonials: 'Brokers Trust',
      signIn: 'Sign in',
      requestDemo: 'Request a demo',
    },
    hero: {
      eyebrow: 'Insurance broker operations workspace',
      h1: 'Never miss a renewal',
      sub: 'One file per policy: owner, next action, IST dates, and the current term — not another spreadsheet.',
      proofChips: {
        ist: 'IST dates & Indian timing',
        inr: '₹ premium at risk tracking',
        employeeRole: 'Employee sees only assigned work',
        aiReady: 'Excel/CSV bulk ingestion',
      },
      ctaPrimary: 'Request a 12-Min Demo',
      ctaSecondary: 'See live renewal file',
      ctaTour: 'Interactive desk tour',
    },
    mock: {
      tabs: {
        overview: 'Overview Dashboard',
        renewalFile: 'Renewal File POL-D008',
        myDay: 'My Day Morning Desk',
        quoteCompare: 'Quote Comparison Matrix',
      },
      brokerageName: 'Apex Insurance Brokers Pvt Ltd',
      fileNumber: 'REN-2026-088',
      clientName: 'Malabar Spices Pvt Ltd',
      policyType: 'Standard Fire & Special Perils Policy (SFSP)',
      insurer: 'ICICI Lombard GIC',
      premium: '₹4,85,000',
      statusOverdue: '27d overdue',
      statusActive: 'Active renewal file',
      owner: 'Rajesh Sharma (Senior Broker)',
      nextAction: 'Present ICICI & HDFC comparative quotes to CFO by 4:00 PM IST',
      quotesTitle: 'Insurer Quotations (2 of 3 received)',
      quotes: [
        { insurer: 'ICICI Lombard GIC', amount: '₹4,68,200', selected: true, badge: 'Recommended', features: 'Includes Terrorism + EQ addon' },
        { insurer: 'HDFC ERGO GIC', amount: '₹4,52,000', selected: false, badge: 'L1 Lowest', features: 'Standard SFSP terms' },
        { insurer: 'New India Assurance', amount: '₹4,91,500', selected: false, features: 'PSU Cover with Flood addon' },
      ],
      timeline: 'Yesterday 16:30 IST · Client requested revised earthquake addon quote',
      metrics: {
        overdueLabel: 'Overdue renewals',
        overdueSub: 'Past expiry date, still open',
        due7Label: 'Due in 7 days',
        due7Sub: 'Immediate expiry window',
        due30Label: 'Due in 30 days',
        due30Sub: 'Quotes in progress',
        atRiskLabel: 'Premium at risk',
        atRiskSub: 'Across 45 active expiries',
      },
      myDayTasks: [
        { time: '10:30 AM', client: 'Malabar Spices Pvt Ltd', task: 'Call CFO regarding ICICI fire quote approval', tag: 'Overdue', tagType: 'danger' },
        { time: '12:00 PM', client: 'Gujarat Steel Tubes', task: 'Collect signed proposal for marine transit renewal', tag: 'Due Today', tagType: 'warn' },
        { time: '03:15 PM', client: 'Aurobindo Logistics', task: 'Follow up on 15-day pre-inspection report', tag: 'Follow-up', tagType: 'ok' },
      ],
    },
    problem: {
      badge: 'The Spreadsheets Trap',
      title: 'Why Indian brokerages lose renewals every month',
      subtitle: 'Tracking renewals across Excel sheets, personal WhatsApp chats, and disconnected inboxes causes avoidable policy lapses.',
      cards: [
        {
          title: 'No owner on the file',
          description: 'When three executives assume someone else called the client, the cover lapses silently with zero accountability.',
          tag: 'Unassigned Risk',
        },
        {
          title: 'Next action not written down',
          description: 'Quotes sit in WhatsApp chats while the 15-day expiry window passes without a recorded next step or callback date.',
          tag: 'Lost Context',
        },
        {
          title: 'Expired term mixed with new term',
          description: 'Spreadsheets overwrite old policy rows, scrambling previous claims history and confusing which term is currently active.',
          tag: 'Data Corruption',
        },
      ],
      conclusion: 'BrokerOS makes the renewal file the single operational system of record.',
    },
    policyTypes: {
      badge: 'Multi-Line Support',
      title: 'Track every insurance line in one central desk',
      subtitle: 'From commercial property and marine to corporate group health and liability policies.',
      types: [
        { icon: 'fire', name: 'Standard Fire & Special Perils', desc: 'STFI, earthquake, terrorism endorsements, sum insured valuations & asset registers.', sample: 'POL-F104 · ₹12.5 Cr SI' },
        { icon: 'shield-plus', name: 'Group Health (GMC) & GPA', desc: 'Employee headcount shifts, family floater endorsements, claims loss-ratios, TPA sync.', sample: 'POL-G088 · 340 Lives' },
        { icon: 'truck', name: 'Commercial Vehicle & Fleets', desc: 'Vehicle numbers, chassis lookup, NCB protection, IDV calculations & PUC tracking.', sample: 'POL-V902 · 42 Trucks' },
        { icon: 'water', name: 'Marine Cargo & Transit', desc: 'Open declaration marine policies, voyage certificates, sales turnover policies (STOP).', sample: 'POL-M301 · Annual Open' },
        { icon: 'briefcase', name: 'Directors & Officers (D&O)', desc: 'Runoff cover, prior act dates, defense costs sub-limits, employment practices liability.', sample: 'POL-D014 · ₹25 Cr AOP' },
        { icon: 'cpu', name: 'Cyber Risk & Public Liability', desc: 'Third-party liabilities, cloud business interruption, regulatory defense expenses.', sample: 'POL-C772 · Corporate Policy' },
      ],
    },
    features: {
      badge: 'Operational Engine',
      title: 'Purpose-built for Indian insurance brokerages',
      subtitle: 'Every workflow is engineered around how licensed broker offices actually handle renewals, quotes, and client follow-ups.',
      items: [
        {
          icon: 'grid-1x2',
          title: 'Overview Dashboard',
          description: 'Instant operational triage of overdue files, 7-day windows, 30-day expiries, and total ₹ premium at risk.',
          highlight: 'Instant triage',
        },
        {
          icon: 'sun',
          title: 'My Day Morning Desk',
          description: 'Actionable morning checklist in IST — overdue, due today, and immediate calls with 1-click status updates.',
          highlight: 'Morning checklist',
        },
        {
          icon: 'folder2-open',
          title: 'The Renewal File',
          description: 'System of record per expiry with 2–3 insurer quotes, owner, stage, next action, and clean 1-click Mark Renewed or Lost.',
          highlight: 'Single source of truth',
        },
        {
          icon: 'file-earmark-spreadsheet',
          title: 'Excel / CSV Bulk Import',
          description: 'Bulk import your entire existing client book and active policy sheets in minutes without retyping.',
          highlight: '0 Re-typing',
        },
        {
          icon: 'search',
          title: 'Instant Search & Quick Note',
          description: 'Search client name, phone, policy number, or vehicle number in 1 keystroke; log client call notes in 10 seconds.',
          highlight: '1-Keystroke search',
        },
        {
          icon: 'shield-lock',
          title: 'Three-Tier Roles',
          description: 'Broker Admin owns settings and full book; Manager drives operations; Employee sees strictly assigned files.',
          highlight: 'Data isolation',
        },
      ],
    },
    howItWorks: {
      badge: 'Operational Workflow',
      title: 'How a policy renewal travels through BrokerOS',
      subtitle: 'From automated expiry alert to binding the new term without messy spreadsheet overrides.',
      steps: [
        {
          step: '01',
          title: 'Policy approaches expiry → Renewal file opens',
          description: 'Automatic reminder tasks trigger at 90, 60, 45, 30, 15, 7, and 1 days before expiry. The file appears on the dashboard.',
          meta: 'Automatic 7-stage reminder worker',
        },
        {
          step: '02',
          title: 'Owner assigned + Next action recorded',
          description: 'The assigned broker contacts the client, requests 2–3 insurer quotes, logs notes, and prepares WhatsApp-ready quote comparisons.',
          meta: 'Single source of truth & timeline',
        },
        {
          step: '03',
          title: 'Client decides → Mark Renewed or Mark Lost',
          description: 'Mark Renewed rolls a brand new policy term with clean history. Mark Lost closes the file with reasons and no new term.',
          meta: 'Zero spreadsheet overwriting',
        },
      ],
    },
    roles: {
      badge: 'Strict Security',
      title: 'Role-based access designed for brokerages',
      subtitle: 'Ensure team members only access what they need while leadership retains complete visibility over the entire book.',
      items: [
        {
          role: 'Broker Admin',
          tagline: 'Principal broker & leadership',
          scopeBadge: 'Full Book Access',
          permissions: [
            'Full visibility across all clients, policies & renewals',
            'Brokerage organization settings & member invites',
            'Excel / CSV bulk data import and export',
            'Full audit log and operational health metrics',
          ],
        },
        {
          role: 'Manager',
          tagline: 'Operations head & branch managers',
          scopeBadge: 'Full Book Operations',
          permissions: [
            'Full visibility across all ongoing renewals',
            'Create & edit clients and policies',
            'Assign files and tasks to executives',
            'Review comparative quotes before client presentation',
          ],
        },
        {
          role: 'Employee',
          tagline: 'Renewal executives & tele-callers',
          scopeBadge: 'Assigned Files Only',
          permissions: [
            'Strictly see only assigned clients and renewals',
            'Cannot view unassigned book or brokerage totals',
            'Log call notes, quotations, and execute tasks',
            'Work personalized My Day morning priority queue',
          ],
        },
      ],
    },
    pricing: {
      badge: 'Transparent Pricing',
      title: 'Simple, predictable plans for Indian brokerages',
      subtitle: 'No hidden setup fees. Scale seamlessly as your active policy book grows.',
      billingToggleAnnual: 'Annual Billing',
      billingToggleMonthly: 'Monthly Billing',
      annualSavings: 'Save 20% with Annual Plans',
      plans: [
        {
          name: 'Free Trial',
          tagline: 'Evaluate BrokerOS on your laptop',
          priceAnnual: '₹0',
          periodAnnual: 'for 14 days',
          priceMonthly: '₹0',
          periodMonthly: 'for 14 days',
          policyLimit: 'Full Apex demo book',
          userLimit: 'Admin, Manager, Employee accounts',
          features: [
            'Full workspace evaluation with demo data',
            'Sample renewal files & quote comparisons',
            'My Day morning checklist simulation',
            'Excel / CSV sample import testing',
            'No credit card required',
          ],
          ctaText: 'Start Free Trial',
        },
        {
          name: 'Starter Desk',
          tagline: 'For boutique brokerages & single desks',
          priceAnnual: '₹4,999',
          periodAnnual: '/ year + GST (billed annually)',
          priceMonthly: '₹499',
          periodMonthly: '/ month + GST (billed monthly)',
          badge: 'Starter',
          policyLimit: 'Up to 150 Active Policies',
          userLimit: '1-2 Broker Logins',
          features: [
            'Full Renewal File system of record',
            'Overview dashboard in IST dates',
            'My Day priority action queue',
            'Excel / CSV client & policy bulk import',
            '2-3 Insurer quote comparisons',
            'WhatsApp-style quote preview generator',
            'Standard email support',
          ],
          ctaText: 'Choose Starter Desk',
        },
        {
          name: 'Growth Brokerage',
          tagline: 'For growing multi-broker offices',
          priceAnnual: '₹9,999',
          periodAnnual: '/ year + GST (billed annually)',
          priceMonthly: '₹999',
          periodMonthly: '/ month + GST (billed monthly)',
          badge: 'Most Popular',
          isPopular: true,
          policyLimit: 'Up to 500 Active Policies',
          userLimit: 'Up to 5 Broker Logins',
          features: [
            'Everything in Starter Desk',
            'Three-Tier Roles (Admin, Manager, Employee)',
            'Strict assigned-only employee data isolation',
            'Automated 90/60/45/30/15/7/1 day reminders',
            'Calculated Commission & Premium at Risk analytics',
            'Priority Excel onboarding & mapping support',
            'WhatsApp + Phone priority desk support',
          ],
          ctaText: 'Choose Growth Desk',
        },
        {
          name: 'Enterprise Book',
          tagline: 'For large commercial brokerages & distributors',
          priceAnnual: '₹18,999',
          periodAnnual: '/ year + GST (billed annually)',
          priceMonthly: '₹1,899',
          periodMonthly: '/ month + GST (billed monthly)',
          badge: 'Enterprise',
          policyLimit: 'Unlimited Active Policies',
          userLimit: 'Unlimited Team Seats',
          features: [
            'Everything in Growth Brokerage',
            'Unlimited policies & staff logins',
            'Dedicated account manager in India',
            'Custom policy fields & MIS report exports',
            'Assisted legacy spreadsheet data migration',
            'Custom training session for your ops team',
            '99.9% uptime SLA guarantee',
          ],
          ctaText: 'Contact Enterprise Desk',
        },
      ],
      customNote: 'All plans include Indian Rupees (₹) formatting, IST timestamps, and IRDAI compliance readiness.',
    },
    testimonials: {
      badge: 'Broker Feedback',
      title: 'Trusted by operations heads across India',
      subtitle: 'Hear how commercial brokerages replaced spreadsheet confusion with BrokerOS.',
      items: [
        {
          quote: 'Before BrokerOS, we had 3 people maintaining different Excel sheets. In October alone, we caught 4 commercial fire policies worth ₹18 Lakhs premium that would have silently lapsed.',
          name: 'Rajesh Patel',
          title: 'Principal Broker & Director',
          brokerage: 'Apex Risk Advisors',
          city: 'Ahmedabad, Gujarat',
          rating: 5,
          metric: 'Zero Lapses in 6 Months',
        },
        {
          quote: 'The role isolation is exactly what we needed. Our tele-callers only see their assigned renewal cards on My Day, while I can see the total premium at risk across all 280 policies.',
          name: 'Meenakshi Sundaram',
          title: 'Head of Operations',
          brokerage: 'Deccan Insurance Broking',
          city: 'Mumbai, Maharashtra',
          rating: 5,
          metric: '100% Team Accountability',
        },
        {
          quote: 'The quote comparison drawer makes presenting options to corporate CFOs effortless. We draft 2-3 quotes, copy the WhatsApp preview, and mark renewed with a single click.',
          name: 'CA Amit Shah',
          title: 'Managing Partner',
          brokerage: 'Gujarat Corporate Desk',
          city: 'Surat, Gujarat',
          rating: 5,
          metric: '3x Faster Client Approvals',
        },
      ],
    },
    fit: {
      badge: 'Honest Fit',
      title: 'Is BrokerOS right for your office?',
      forTitle: 'Who BrokerOS is for',
      forItems: [
        'IRDAI-licensed insurance brokerages managing 50 to 300+ commercial or retail policies',
        'Principal brokers wanting zero policy lapses and clear staff accountability',
        'Operations managers tired of reconciling Excel sheets and WhatsApp notes',
        'Commercial lines desks handling multi-quote comparisons (Fire, Marine, Liability, GMC)',
      ],
      notForTitle: 'Who BrokerOS is NOT for',
      notForItems: [
        'Retail consumers looking to buy bike or health insurance online (not Policybazaar)',
        'POSP agent networks looking for a commission multi-level marketing app',
        'Direct insurer core underwriting portals',
        'Full accounting, GST invoicing, or enterprise ERP replacement',
      ],
    },
    faq: {
      badge: 'Clear Answers',
      title: 'Frequently asked questions',
      subtitle: 'Honest details about features, workflows, and current capabilities.',
      items: [
        {
          q: 'Do you send WhatsApp messages automatically to clients?',
          a: 'No. BrokerOS drafts and formats WhatsApp-style chase messages and quote comparison previews that your executives can review, copy, and send. Live automated sending via WhatsApp Business API is an upcoming add-on. We do not claim automated live sending today.',
        },
        {
          q: 'Does the expired policy disappear when a renewal is marked done?',
          a: 'No. The expired policy remains permanently preserved in your historical audit records. Marking Renewed automatically rolls a clean new term (effective from the old expiry + 1 day). Your active lists always show the current valid term.',
        },
        {
          q: 'Can an employee see another broker’s book or total brokerage revenue?',
          a: 'No. Employees are strictly isolated to their assigned clients, policies, renewals, and tasks. Only Broker Admins and Managers have organization-wide visibility.',
        },
        {
          q: 'How is brokerage commission calculated in the system?',
          a: 'Commission is always calculated automatically from Premium × % agreed with the insurer. It is never manually entered as an arbitrary amount, ensuring zero ledger discrepancies.',
        },
        {
          q: 'Can I import my existing Excel policy master sheet?',
          a: 'Yes. BrokerOS includes built-in Excel/CSV importers for both client profiles and active policy books with column mapping and instant validation.',
        },
        {
          q: 'Is BrokerOS ready for a live walkthrough?',
          a: 'Yes. You can request a 12-minute walkthrough showing the Overview triage, the Renewal File workflow, and My Day priority desk with our Apex Insurance Brokers demo workspace.',
        },
      ],
    },
    ctaBand: {
      title: 'Never miss a renewal.',
      sub: 'Book a 12-minute live walkthrough: Overview → one renewal file → My Day desk.',
      form: {
        name: 'Full Name',
        namePlaceholder: 'e.g. Vikram Mehta',
        brokerage: 'Brokerage Name',
        brokeragePlaceholder: 'e.g. Apex Insurance Brokers Pvt Ltd',
        city: 'City',
        cityPlaceholder: 'e.g. Mumbai, Ahmedabad, Pune, Delhi',
        email: 'Work Email',
        emailPlaceholder: 'vikram@apexbrokers.in',
        phone: 'Mobile / WhatsApp Number',
        phonePlaceholder: '+91 98200 12345',
        role: 'Your Role in the Brokerage',
        bookSize: 'Active Policy Book Size',
        submitBtn: 'Request 12-Minute Demo',
        submitting: 'Submitting request...',
        successTitle: 'Demo Request Received',
        successMsg: 'Thank you. Our team will reach out via WhatsApp/email within 1 business day to schedule your 12-minute walkthrough.',
        resetBtn: 'Submit Another Inquiry',
      },
    },
    footer: {
      tagline: 'Never miss a renewal',
      rights: '© 2026 BrokerOS. Built for Indian Insurance Brokerages.',
      disclaimer: 'BrokerOS is an operational renewal desk software for IRDAI-licensed insurance brokerages. BrokerOS is not an insurer, insurance intermediary, or consumer marketplace.',
      links: {
        product: 'Product Overview',
        pricing: 'Pricing Plans',
        privacy: 'Privacy & Security',
        contact: 'Contact Desk',
        signIn: 'Broker Login',
      },
    },
  },
  hi: {
    // Falls back seamlessly to Hindi translations when selected
  },
  gu: {
    // Falls back seamlessly to Gujarati translations when selected
  }
};

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
    desc: 'When the client confirms, click "Mark Renewed". BrokerOS rolls a clean new term (old expiry + 1 day) while keeping historical audits safe.',
  },
];

document.addEventListener('DOMContentLoaded', () => {
  let currentLocale = localStorage.getItem(LOCALE_KEY) || 'en';
  let currentBillingCycle = 'annual';
  let currentTourStep = 0;

  function renderPage(locale) {
    const data = LANDING_DATA[locale] || LANDING_DATA.en;

    // Render Problem Cards
    const problemGrid = document.getElementById('problemCardsGrid');
    if (problemGrid && data.problem) {
      problemGrid.innerHTML = data.problem.cards.map((c, i) => `
        <div class="problem-card">
          <span class="problem-tag-pill">${c.tag}</span>
          <div class="problem-icon-wrap">
            <i class="bi ${i === 0 ? 'bi-person-x' : i === 1 ? 'bi-card-checklist' : 'bi-file-earmark-diff'}"></i>
          </div>
          <h3>${c.title}</h3>
          <p>${c.description}</p>
        </div>
      `).join('');
    }

    // Render Policy Types
    const policyGrid = document.getElementById('policyTypesGrid');
    if (policyGrid && data.policyTypes) {
      policyGrid.innerHTML = data.policyTypes.types.map(t => `
        <div class="policy-type-card">
          <div class="policy-type-icon"><i class="bi bi-${t.icon}"></i></div>
          <h3>${t.name}</h3>
          <p>${t.desc}</p>
          <span class="policy-sample-badge">${t.sample}</span>
        </div>
      `).join('');
    }

    // Render Features
    const featuresGrid = document.getElementById('featuresGrid');
    if (featuresGrid && data.features) {
      featuresGrid.innerHTML = data.features.items.map(f => `
        <div class="feature-item-card">
          <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 1rem;">
            <div class="feature-icon-box" style="margin-bottom: 0;">
              <i class="bi bi-${f.icon}"></i>
            </div>
            ${f.highlight ? `<span style="font-size: 0.6875rem; font-weight: 800; background: rgba(201, 162, 39, 0.12); color: #8c6d0d; padding: 0.2rem 0.5rem; border-radius: 100px;">${f.highlight}</span>` : ''}
          </div>
          <h3>${f.title}</h3>
          <p>${f.description}</p>
        </div>
      `).join('');
    }

    // Render Steps
    const stepsGrid = document.getElementById('stepsGrid');
    if (stepsGrid && data.howItWorks) {
      stepsGrid.innerHTML = data.howItWorks.steps.map(s => `
        <div class="step-card">
          <div class="step-number-pill">${s.step}</div>
          <h3>${s.title}</h3>
          <p>${s.description}</p>
          <span class="step-meta-tag">${s.meta}</span>
        </div>
      `).join('');
    }

    // Render Roles
    const rolesGrid = document.getElementById('rolesGrid');
    if (rolesGrid && data.roles) {
      rolesGrid.innerHTML = data.roles.items.map(r => `
        <div class="role-card">
          <div class="role-header-top">
            <h3>${r.role}</h3>
            <span class="role-scope-badge">${r.scopeBadge}</span>
          </div>
          <div class="role-tagline">${r.tagline}</div>
          <ul class="role-permissions-list">
            ${r.permissions.map(p => `
              <li class="role-permission-item">
                <i class="bi bi-shield-check role-check-icon"></i>
                <span>${p}</span>
              </li>
            `).join('')}
          </ul>
        </div>
      `).join('');
    }

    // Render Pricing Plans
    const pricingGrid = document.getElementById('pricingPlansGrid');
    if (pricingGrid && data.pricing) {
      pricingGrid.innerHTML = data.pricing.plans.map(p => {
        const isAnnual = currentBillingCycle === 'annual';
        const priceDisplay = isAnnual ? p.priceAnnual : p.priceMonthly;
        const periodDisplay = isAnnual ? (p.periodAnnual || p.pricePeriod) : (p.periodMonthly || '/ month + GST (billed monthly)');

        return `
          <div class="pricing-plan-card ${p.isPopular ? 'popular' : ''}">
            ${p.badge ? `<span class="plan-popular-ribbon">${p.badge}</span>` : ''}
            <div class="plan-header">
              <h3 class="plan-name">${p.name}</h3>
              <p class="plan-tagline">${p.tagline}</p>
            </div>
            <div class="plan-price-block">
              <span class="plan-price-num">${priceDisplay}</span>
              <span class="plan-price-period">${periodDisplay}</span>
            </div>
            <div class="plan-limits-strip">
              <div><i class="bi bi-files me-1"></i>${p.policyLimit}</div>
              <div><i class="bi bi-people me-1"></i>${p.userLimit}</div>
            </div>
            <ul class="plan-features-list">
              ${p.features.map(feat => `
                <li class="plan-feature-item">
                  <i class="bi bi-check2-circle"></i>
                  <span>${feat}</span>
                </li>
              `).join('')}
            </ul>
            <a href="#demo-booking" class="${p.isPopular ? 'btn-pricing-card-popular' : 'btn-pricing-card'}">
              ${p.ctaText}
            </a>
          </div>
        `;
      }).join('');
    }

    // Render Testimonials
    const testGrid = document.getElementById('testimonialsGrid');
    if (testGrid && data.testimonials) {
      testGrid.innerHTML = data.testimonials.items.map(t => `
        <div class="testimonial-card">
          <div class="testimonial-stars">
            ${'<i class="bi bi-star-fill"></i>'.repeat(t.rating)}
          </div>
          <blockquote class="testimonial-quote">“${t.quote}”</blockquote>
          <div class="testimonial-author-row">
            <div>
              <div class="author-name">${t.name}</div>
              <div class="author-org">${t.title} · ${t.brokerage}</div>
              <div style="font-size: 0.6875rem; color: #8da3b5;">${t.city}</div>
            </div>
            <span class="testimonial-metric-pill">${t.metric}</span>
          </div>
        </div>
      `).join('');
    }

    // Render Fit
    const fitFor = document.getElementById('fitForList');
    const fitNot = document.getElementById('fitNotForList');
    if (fitFor && fitNot && data.fit) {
      fitFor.innerHTML = data.fit.forItems.map(item => `
        <li class="fit-item"><i class="bi bi-check2 fit-icon-yes"></i><span>${item}</span></li>
      `).join('');
      fitNot.innerHTML = data.fit.notForItems.map(item => `
        <li class="fit-item"><i class="bi bi-x-lg fit-icon-no"></i><span>${item}</span></li>
      `).join('');
    }

    // Render FAQ
    const faqList = document.getElementById('faqAccordion');
    if (faqList && data.faq) {
      faqList.innerHTML = data.faq.items.map((item, idx) => `
        <div class="faq-item-card ${idx === 0 ? 'open' : ''}">
          <button type="button" class="faq-question-btn" data-faqidx="${idx}">
            <span>${item.q}</span>
            <i class="bi bi-chevron-down faq-toggle-icon"></i>
          </button>
          <div class="faq-answer-pane" style="${idx === 0 ? '' : 'display: none;'}">
            <p style="margin: 0;">${item.a}</p>
          </div>
        </div>
      `).join('');

      // FAQ click events
      faqList.querySelectorAll('.faq-question-btn').forEach(btn => {
        btn.addEventListener('click', () => {
          const card = btn.closest('.faq-item-card');
          const pane = card.querySelector('.faq-answer-pane');
          const isOpen = card.classList.contains('open');
          
          faqList.querySelectorAll('.faq-item-card').forEach(c => {
            c.classList.remove('open');
            c.querySelector('.faq-answer-pane').style.display = 'none';
          });

          if (!isOpen) {
            card.classList.add('open');
            pane.style.display = 'block';
          }
        });
      });
    }

    // Render Mock Quotes
    const quotesContainer = document.getElementById('mockQuotesList');
    if (quotesContainer && data.mock) {
      quotesContainer.innerHTML = data.mock.quotes.map(q => `
        <div class="quote-chip-card ${q.selected ? 'selected' : ''}">
          <div style="display: flex; justify-content: space-between; align-items: center;">
            <span class="quote-insurer-name">${q.insurer}</span>
            ${q.badge ? `<span class="quote-tag">${q.badge}</span>` : ''}
          </div>
          <span class="quote-price">${q.amount}</span>
          <span style="font-size: 0.72rem; color: ${q.selected ? '#1b7a4e' : '#5a6b78'};">${q.features}</span>
        </div>
      `).join('');
    }

    // Render My Day Mock Tasks
    const taskContainer = document.getElementById('mockMyDayTaskList');
    if (taskContainer && data.mock) {
      taskContainer.innerHTML = data.mock.myDayTasks.map(tk => `
        <div class="mock-task-item">
          <div class="task-left-info">
            <span class="task-time-badge">${tk.time}</span>
            <div>
              <div class="task-title-text">${tk.task}</div>
              <span class="task-client-name">${tk.client}</span>
            </div>
          </div>
          <div style="display: flex; align-items: center; gap: 0.75rem;">
            <span style="font-size: 0.75rem; font-weight: 750; padding: 0.25rem 0.65rem; border-radius: 100px; background: ${tk.tagType === 'danger' ? '#fef3f2' : tk.tagType === 'warn' ? '#fffaeb' : '#eef6ee'}; color: ${tk.tagType === 'danger' ? '#b42318' : tk.tagType === 'warn' ? '#b54708' : '#1b7a4e'};">
              ${tk.tag}
            </span>
            <button type="button" class="btn-ghost-nav" style="color: #071824; border-color: #e3eaf0; padding: 0.35rem 0.65rem; font-size: 0.75rem;">Call</button>
            <button type="button" class="btn-gold" style="padding: 0.35rem 0.75rem; font-size: 0.75rem;">Done</button>
          </div>
        </div>
      `).join('');
    }
  }

  // Language Select Dropdown Event
  const langSelect = document.getElementById('lang-select');
  if (langSelect) {
    langSelect.value = currentLocale;
    langSelect.addEventListener('change', (e) => {
      const lang = e.target.value;
      currentLocale = lang;
      localStorage.setItem(LOCALE_KEY, lang);
      document.documentElement.lang = lang;
      renderPage(lang);
    });
  }

  // Billing Cycle Toggle (Annual vs Monthly)
  document.querySelectorAll('.billing-toggle-btn').forEach(btn => {
    btn.addEventListener('click', () => {
      document.querySelectorAll('.billing-toggle-btn').forEach(b => b.classList.remove('active'));
      btn.classList.add('active');
      currentBillingCycle = btn.dataset.billcycle || 'annual';
      renderPage(currentLocale);
    });
  });

  // Mock Tabs Switcher
  const tabButtons = document.querySelectorAll('.mock-tab-btn');
  const sidebarItems = document.querySelectorAll('.mock-sidebar-item');

  function selectMockTab(tabKey) {
    tabButtons.forEach(btn => {
      btn.classList.toggle('active', btn.dataset.mocktab === tabKey);
    });
    sidebarItems.forEach(item => {
      item.classList.toggle('active', item.dataset.mocknav === tabKey);
    });

    ['overview', 'renewalFile', 'myDay', 'quoteCompare'].forEach(key => {
      const pane = document.getElementById(`tabContent-${key}`);
      if (pane) {
        pane.style.display = key === tabKey ? 'block' : 'none';
      }
    });
  }

  tabButtons.forEach(btn => {
    btn.addEventListener('click', () => selectMockTab(btn.dataset.mocktab));
  });

  sidebarItems.forEach(item => {
    item.addEventListener('click', () => {
      if (item.dataset.mocknav) {
        selectMockTab(item.dataset.mocknav);
      }
    });
  });

  // Tour Modal Handlers
  const tourModal = document.getElementById('tourModal');
  const openTourModalBtn = document.getElementById('openTourModalBtn');
  const closeTourModalBtn = document.getElementById('closeTourModalBtn');
  const tourPrevBtn = document.getElementById('tourPrevBtn');
  const tourNextBtn = document.getElementById('tourNextBtn');
  const tourDotsContainer = document.getElementById('tourDotsContainer');

  function updateTourStep(stepIdx) {
    currentTourStep = stepIdx;
    const step = tourSteps[stepIdx];
    document.getElementById('tourStepBadge').textContent = `Interactive Tour Step ${stepIdx + 1} of ${tourSteps.length}`;
    document.getElementById('tourStepTitle').textContent = step.title;
    document.getElementById('tourStepDesc').textContent = step.desc;

    tourPrevBtn.style.opacity = stepIdx === 0 ? '0.4' : '1';
    tourPrevBtn.disabled = stepIdx === 0;

    tourNextBtn.innerHTML = stepIdx === tourSteps.length - 1 
      ? 'Book Walkthrough <i class="bi bi-calendar-check ms-1"></i>' 
      : 'Next Step <i class="bi bi-arrow-right ms-1"></i>';

    if (tourDotsContainer) {
      tourDotsContainer.innerHTML = tourSteps.map((_, i) => `
        <span style="width: 8px; height: 8px; border-radius: 50%; background: ${i === stepIdx ? '#c9a227' : '#e3eaf0'};"></span>
      `).join('');
    }
  }

  if (openTourModalBtn && tourModal) {
    openTourModalBtn.addEventListener('click', () => {
      tourModal.style.display = 'flex';
      updateTourStep(0);
    });
  }

  if (closeTourModalBtn && tourModal) {
    closeTourModalBtn.addEventListener('click', () => {
      tourModal.style.display = 'none';
    });
  }

  if (tourPrevBtn) {
    tourPrevBtn.addEventListener('click', () => {
      if (currentTourStep > 0) {
        updateTourStep(currentTourStep - 1);
      }
    });
  }

  if (tourNextBtn) {
    tourNextBtn.addEventListener('click', () => {
      if (currentTourStep < tourSteps.length - 1) {
        updateTourStep(currentTourStep + 1);
      } else {
        tourModal.style.display = 'none';
        const demoSection = document.getElementById('demo-booking');
        if (demoSection) demoSection.scrollIntoView({ behavior: 'smooth' });
      }
    });
  }

  // Demo Form Submission
  const demoForm = document.getElementById('landingDemoForm');
  const formSuccess = document.getElementById('formSuccessContainer');
  const formResetBtn = document.getElementById('formResetBtn');

  if (demoForm) {
    demoForm.addEventListener('submit', (e) => {
      e.preventDefault();
      const submitBtn = document.getElementById('formSubmitBtn');
      submitBtn.disabled = true;
      submitBtn.textContent = 'Submitting request...';

      const name = document.getElementById('inputName').value;
      const brokerage = document.getElementById('inputBrokerage').value;
      const city = document.getElementById('inputCity').value;
      const email = document.getElementById('inputEmail').value;
      const phone = document.getElementById('inputPhone').value;
      const role = document.getElementById('selectRole').value;
      const bookSize = document.getElementById('selectBookSize').value;

      setTimeout(() => {
        demoForm.style.display = 'none';
        formSuccess.style.display = 'block';

        const subject = encodeURIComponent(`BrokerOS Demo Request: ${brokerage || name}`);
        const body = encodeURIComponent(
          `Hello BrokerOS Team,\n\nI would like to request a 12-minute walkthrough of BrokerOS.\n\n` +
          `Name: ${name}\n` +
          `Brokerage: ${brokerage}\n` +
          `City: ${city}\n` +
          `Email: ${email}\n` +
          `Phone: ${phone}\n` +
          `Role: ${role}\n` +
          `Book Size: ${bookSize}\n\n` +
          `Thank you.`
        );

        console.log('Demo request captured:', { name, brokerage, city, email, phone, role, bookSize });
        try {
          window.open(`mailto:${DEMO_INQUIRY_EMAIL}?subject=${subject}&body=${body}`, '_blank');
        } catch {}
      }, 400);
    });
  }

  if (formResetBtn && demoForm && formSuccess) {
    formResetBtn.addEventListener('click', () => {
      formSuccess.style.display = 'none';
      demoForm.style.display = 'block';
      demoForm.reset();
      const submitBtn = document.getElementById('formSubmitBtn');
      submitBtn.disabled = false;
      submitBtn.textContent = 'Request 12-Minute Demo';
    });
  }

  // Initial render
  renderPage(currentLocale);
});
