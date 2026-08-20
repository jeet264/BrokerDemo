export type LandingLocale = 'en' | 'hi' | 'gu'

export interface LandingContent {
  nav: {
    product: string
    features: string
    policyTypes: string
    howItWorks: string
    roles: string
    pricing: string
    faq: string
    testimonials: string
    signIn: string
    requestDemo: string
  }
  hero: {
    eyebrow: string
    h1: string
    sub: string
    proofChips: {
      ist: string
      inr: string
      employeeRole: string
      aiReady: string
    }
    ctaPrimary: string
    ctaSecondary: string
    ctaTour: string
  }
  mock: {
    tabs: {
      overview: string
      renewalFile: string
      myDay: string
      quoteCompare: string
    }
    brokerageName: string
    fileNumber: string
    clientName: string
    policyType: string
    insurer: string
    premium: string
    statusOverdue: string
    statusActive: string
    owner: string
    nextAction: string
    quotesTitle: string
    quotes: {
      insurer: string
      amount: string
      selected: boolean
      badge?: string
      features: string
    }[]
    timeline: string
    markRenewed: string
    sharePreview: string
    metrics: {
      overdueLabel: string
      overdueVal: string
      overdueSub: string
      due7Label: string
      due7Val: string
      due7Sub: string
      due30Label: string
      due30Val: string
      due30Sub: string
      atRiskLabel: string
      atRiskVal: string
      atRiskSub: string
    }
    myDayTasks: {
      time: string
      client: string
      task: string
      tag: string
      tagType: 'danger' | 'warn' | 'ok'
    }[]
  }
  problem: {
    badge: string
    title: string
    subtitle: string
    cards: {
      title: string
      description: string
      tag: string
    }[]
    conclusion: string
  }
  policyTypes: {
    badge: string
    title: string
    subtitle: string
    types: {
      icon: string
      name: string
      desc: string
      sample: string
    }[]
  }
  features: {
    badge: string
    title: string
    subtitle: string
    items: {
      icon: string
      title: string
      description: string
      highlight?: string
    }[]
  }
  howItWorks: {
    badge: string
    title: string
    subtitle: string
    steps: {
      step: string
      title: string
      description: string
      meta: string
    }[]
  }
  roles: {
    badge: string
    title: string
    subtitle: string
    items: {
      role: string
      tagline: string
      permissions: string[]
      scopeBadge: string
    }[]
  }
  pricing: {
    badge: string
    title: string
    subtitle: string
    billingToggleAnnual: string
    billingToggleMonthly: string
    annualSavings: string
    plans: {
      name: string
      tagline: string
      priceAnnual: string
      periodAnnual: string
      priceMonthly: string
      periodMonthly: string
      badge?: string
      isPopular?: boolean
      policyLimit: string
      userLimit: string
      features: string[]
      ctaText: string
    }[]
    customNote: string
  }
  testimonials: {
    badge: string
    title: string
    subtitle: string
    items: {
      quote: string
      name: string
      title: string
      brokerage: string
      city: string
      rating: number
      metric: string
    }[]
  }
  fit: {
    badge: string
    title: string
    forTitle: string
    forItems: string[]
    notForTitle: string
    notForItems: string[]
  }
  faq: {
    badge: string
    title: string
    subtitle: string
    items: {
      q: string
      a: string
    }[]
  }
  ctaBand: {
    title: string
    sub: string
    form: {
      name: string
      namePlaceholder: string
      brokerage: string
      brokeragePlaceholder: string
      city: string
      cityPlaceholder: string
      email: string
      emailPlaceholder: string
      phone: string
      phonePlaceholder: string
      role: string
      roleOptions: string[]
      bookSize: string
      bookSizeOptions: string[]
      submitBtn: string
      submitting: string
      successTitle: string
      successMsg: string
      resetBtn: string
    }
  }
  footer: {
    tagline: string
    rights: string
    disclaimer: string
    links: {
      product: string
      privacy: string
      contact: string
      signIn: string
      pricing: string
    }
  }
}

export const LANDING_DATA: Record<LandingLocale, LandingContent> = {
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
      markRenewed: 'Mark Renewed (Roll New Term)',
      sharePreview: 'WhatsApp Quote Preview',
      metrics: {
        overdueLabel: 'Overdue renewals',
        overdueVal: '14',
        overdueSub: 'Past expiry date, still open',
        due7Label: 'Due in 7 days',
        due7Val: '8',
        due7Sub: 'Immediate expiry window',
        due30Label: 'Due in 30 days',
        due30Val: '23',
        due30Sub: 'Quotes in progress',
        atRiskLabel: 'Premium at risk',
        atRiskVal: '₹38.4 Lakhs',
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
        roleOptions: ['Principal Broker / Director', 'Operations Manager', 'Distributor / Partner', 'Senior Renewal Executive'],
        bookSize: 'Active Policy Book Size',
        bookSizeOptions: ['50 to 150 Policies', '150 to 300 Policies', '300 to 1,000 Policies', '1,000+ Policies'],
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
    nav: {
      product: 'उत्पाद',
      features: 'विशेषताएं',
      policyTypes: 'पॉलिसी प्रकार',
      howItWorks: 'कार्यप्रणाली',
      roles: 'भूमिकाएं',
      pricing: 'मूल्य निर्धारण',
      faq: 'प्रश्नोत्तरी',
      testimonials: 'ब्रोकर अनुभव',
      signIn: 'साइन इन',
      requestDemo: 'डेमो का अनुरोध करें',
    },
    hero: {
      eyebrow: 'बीमा ब्रोकर संचालन कार्यक्षेत्र',
      h1: 'नवीनीकरण कभी न चूकें',
      sub: 'प्रत्येक पॉलिसी की एक फ़ाइल: प्रभारी, अगला कदम, भारतीय समय (IST) और वर्तमान अवधि — कोई स्प्रेडशीट का झंझट नहीं।',
      proofChips: {
        ist: 'IST तिथियां और भारतीय समय',
        inr: '₹ जोखिम में प्रीमियम की ट्रैकिंग',
        employeeRole: 'कर्मचारी केवल अपना सौंपा गया कार्य देखते हैं',
        aiReady: 'Excel/CSV बल्क डेटा इंपोर्ट',
      },
      ctaPrimary: '12 मिनट का डेमो अनुरोध',
      ctaSecondary: 'नवीनीकरण फ़ाइल देखें',
      ctaTour: 'इंटरएक्टिव डेस्क टूर',
    },
    mock: {
      tabs: {
        overview: 'अवलोकन डैशबोर्ड',
        renewalFile: 'नवीनीकरण फ़ाइल POL-D008',
        myDay: 'मेरा दिन (My Day)',
        quoteCompare: 'कोटेशन तुलना मैट्रिक्स',
      },
      brokerageName: 'एपेक्स इंश्योरेंस ब्रोकर्स प्रा. लि.',
      fileNumber: 'REN-2026-088',
      clientName: 'मालाबार स्पाइसेस प्रा. लि.',
      policyType: 'मानक अग्नि एवं विशेष जोखिम पॉलिसी (SFSP)',
      insurer: 'ICICI लोम्बार्ड GIC',
      premium: '₹4,85,000',
      statusOverdue: '27 दिन बकाया (Overdue)',
      statusActive: 'सक्रिय नवीनीकरण फ़ाइल',
      owner: 'राजेश शर्मा (वरिष्ठ ब्रोकर)',
      nextAction: 'शाम 4:00 बजे तक CFO को ICICI और HDFC की कोटेशन प्रस्तुत करें',
      quotesTitle: 'बीमा कंपनी कोटेशन (3 में से 2 प्राप्त)',
      quotes: [
        { insurer: 'ICICI लोम्बार्ड GIC', amount: '₹4,68,200', selected: true, badge: 'अनुशंसित', features: 'आतंकवाद + भूकंप एड-ऑन शामिल' },
        { insurer: 'HDFC ERGO GIC', amount: '₹4,52,000', selected: false, badge: 'न्यूनतम L1', features: 'मानक SFSP शर्तें' },
        { insurer: 'द न्यू इंडिया एश्योरेंस', amount: '₹4,91,500', selected: false, features: 'PSU कवर बाढ़ एड-ऑन सहित' },
      ],
      timeline: 'कल 16:30 IST · ग्राहक ने संशोधित भूकंप एड-ऑन कोट मांगा',
      markRenewed: 'नवीनीकृत चिह्नित करें (नई अवधि बनाएं)',
      sharePreview: 'WhatsApp कोटेशन पूर्वावलोकन',
      metrics: {
        overdueLabel: 'बकाया नवीनीकरण',
        overdueVal: '14',
        overdueSub: 'समाप्ति तिथि बीत चुकी है',
        due7Label: '7 दिनों में देय',
        due7Val: '8',
        due7Sub: 'तत्काल नवीनीकरण समय',
        due30Label: '30 दिनों में देय',
        due30Val: '23',
        due30Sub: 'कोटेशन प्रक्रिया में',
        atRiskLabel: 'जोखिम में कुल प्रीमियम',
        atRiskVal: '₹38.4 लाख',
        atRiskSub: '45 सक्रिय समाप्ति मामलों में',
      },
      myDayTasks: [
        { time: 'सुबह 10:30', client: 'मालाबार स्पाइसेस', task: 'ICICI अग्नि कोटेशन अनुमोदन के लिए CFO को कॉल करें', tag: 'बकाया', tagType: 'danger' },
        { time: 'दोपहर 12:00', client: 'गुजरात स्टील ट्यूब्स', task: 'समुद्री पारगमन नवीनीकरण हेतु हस्ताक्षरित प्रस्ताव लें', tag: 'आज देय', tagType: 'warn' },
        { time: 'दोपहर 03:15', client: 'अरबिंदो लॉजिस्टिक्स', task: '15-दिवसीय पूर्व-निरीक्षण रिपोर्ट पर फॉलो-अप लें', tag: 'फॉलो-अप', tagType: 'ok' },
      ],
    },
    problem: {
      badge: 'स्प्रेडशीट की समस्याएं',
      title: 'भारतीय ब्रोकरेज हर महीने नवीनीकरण क्यों खो देते हैं?',
      subtitle: 'एक्सेल शीट, व्यक्तिगत व्हाट्सएप चैट और बिखरे ईमेल में काम करने से पॉलिसी बिना जानकारी के समाप्त हो जाती है।',
      cards: [
        {
          title: 'फ़ाइल पर कोई जिम्मेदार व्यक्ति नहीं',
          description: 'जब तीन लोग सोचते हैं कि किसी और ने ग्राहक को कॉल किया होगा, तो बिना जवाबदेही के पॉलिसी लैप्स हो जाती है।',
          tag: 'अनसाइंड जोखिम',
        },
        {
          title: 'अगला कदम कहीं दर्ज नहीं',
          description: 'कोटेशन व्हाट्सएप में पड़े रहते हैं और बिना अगली तारीख या फॉलो-अप के 15 दिन की समय-सीमा निकल जाती है।',
          tag: 'खोया हुआ संदर्भ',
        },
        {
          title: 'पुरानी और नई अवधि का घालमेल',
          description: 'स्प्रेडशीट में पुरानी लाइन पर नई जानकारी लिख दी जाती है, जिससे पुराना क्लेम रिकॉर्ड खराब हो जाता है।',
          tag: 'डेटा गड़बड़ी',
        },
      ],
      conclusion: 'BrokerOS में प्रत्येक नवीनीकरण फ़ाइल ही संपूर्ण रिकॉर्ड का केंद्र है।',
    },
    policyTypes: {
      badge: 'सभी प्रकार की पॉलिसियां',
      title: 'प्रत्येक बीमा श्रेणी का एक ही डेस्क पर प्रबंधन',
      subtitle: 'वाणिज्यिक संपत्ति, मरीन से लेकर कॉर्पोरेट ग्रुप मेडिक्लेम और देयता पॉलिसियों तक।',
      types: [
        { icon: 'fire', name: 'मानक अग्नि एवं विशेष जोखिम (SFSP)', desc: 'STFI, भूकंप, आतंकवाद एड-ऑन, बीमित राशि मूल्यांकन और परिसंपत्ति रजिस्टर।', sample: 'POL-F104 · ₹12.5 Cr बीमित राशि' },
        { icon: 'shield-plus', name: 'ग्रुप हेल्थ (GMC) एवं GPA', desc: 'कर्मचारी संख्या बदलाव, परिवार फ्लोटर, क्लेम लॉस-रेशियो और TPA तालमेल।', sample: 'POL-G088 · 340 सदस्य' },
        { icon: 'truck', name: 'वाणिज्यिक वाहन एवं फ्लीट', desc: 'वाहन नंबर, चेसिस सर्च, NCB सुरक्षा, IDV गणना और PUC ट्रैकिंग।', sample: 'POL-V902 · 42 ट्रक' },
        { icon: 'water', name: 'मरीन कार्गो एवं ट्रांजिट', desc: 'ओपन डिक्लेरेशन मरीन पॉलिसियां, यात्रा प्रमाण पत्र, वार्षिक टर्नओवर पॉलिसियां।', sample: 'POL-M301 · वार्षिक ओपन' },
        { icon: 'briefcase', name: 'निदेशक एवं अधिकारी (D&O)', desc: 'रन-ऑफ कवर, पूर्व कार्य तिथियां, रक्षा लागत सब-लिमिट, रोजगार देयता।', sample: 'POL-D014 · ₹25 Cr सीमा' },
        { icon: 'cpu', name: 'साइबर जोखिम एवं सार्वजनिक देयता', desc: 'तृतीय पक्ष देयताएं, क्लाउड रुकावट, कानूनी रक्षा खर्च।', sample: 'POL-C772 · कॉर्पोरेट पॉलिसी' },
      ],
    },
    features: {
      badge: 'संचालन इंजन',
      title: 'भारतीय बीमा ब्रोकरों के लिए विशेष रूप से निर्मित',
      subtitle: 'हर कार्यप्रणाली इस आधार पर बनाई गई है कि एक वास्तविक ब्रोकरेज कार्यालय नवीनीकरण और कोटेशन कैसे संभालता है।',
      items: [
        {
          icon: 'grid-1x2',
          title: 'अवलोकन डैशबोर्ड (Overview)',
          description: 'बकाया फाइलों, 7-दिवसीय और 30-दिवसीय समाप्ति और ₹ प्रीमियम जोखिम का त्वरित जायजा लें।',
          highlight: 'त्वरित जायजा',
        },
        {
          icon: 'sun',
          title: 'मेरा दिन (My Day)',
          description: 'IST में सुबह की कार्यसूची — बकाया, आज देय कार्य और 1-क्लिक में स्थिति अपडेट।',
          highlight: 'सुबह की कार्यसूची',
        },
        {
          icon: 'folder2-open',
          title: 'नवीनीकरण फ़ाइल',
          description: '2–3 कोटेशन, प्रभारी, चरण और 1-क्लिक में नवीनीकृत (Mark Renewed) या निरस्त करने की सुविधा।',
          highlight: 'एकल रिकॉर्ड',
        },
        {
          icon: 'file-earmark-spreadsheet',
          title: 'Excel / CSV आयात',
          description: 'अपने मौजूदा ग्राहक डेटा और सक्रिय पॉलिसियों को बिना दोबारा टाइप किए मिनटों में इंपोर्ट करें।',
          highlight: 'टाइपिंग से मुक्ति',
        },
        {
          icon: 'search',
          title: 'त्वरित खोज और त्वरित नोट',
          description: 'ग्राहक नाम, फोन, पॉलिसी नंबर या वाहन नंबर तुरंत खोजें; 10 सेकंड में कॉल नोट दर्ज करें।',
          highlight: '1-क्लिक खोज',
        },
        {
          icon: 'shield-lock',
          title: 'तीन-स्तरीय भूमिकाएं (Roles)',
          description: 'एडमिन पूरा बहीखाता देखते हैं; मैनेजर संचालन संभालते हैं; कर्मचारी केवल अपने काम तक सीमित रहते हैं।',
          highlight: 'डेटा सुरक्षा',
        },
      ],
    },
    howItWorks: {
      badge: 'सरल कार्यप्रणाली',
      title: 'BrokerOS में नवीनीकरण कैसे पूरा होता है?',
      subtitle: 'स्वचालित समाप्ति चेतावनी से लेकर नई पॉलिसी अवधि शुरू होने तक का व्यवस्थित सफर।',
      steps: [
        {
          step: '01',
          title: 'पॉलिसी समाप्ति के करीब → नवीनीकरण फ़ाइल खुली',
          description: 'समाप्ति से 90, 60, 45, 30, 15, 7 और 1 दिन पहले स्वचालित रिमाइंडर कार्य बनते हैं और फ़ाइल डैशबोर्ड पर आ जाती है।',
          meta: 'स्वचालित 7-स्तरीय रिमाइंडर प्रणाली',
        },
        {
          step: '02',
          title: 'प्रभारी तय + अगला कदम दर्ज',
          description: 'आवंटित ब्रोकर ग्राहक से संपर्क करता है, 2-3 कोटेशन प्राप्त करता है, और तुलनात्मक विवरण तैयार करता है।',
          meta: 'एकल रिकॉर्ड और टाइमलाइन',
        },
        {
          step: '03',
          title: 'ग्राहक का निर्णय → नवीनीकृत या खोया दर्ज करें',
          description: 'नवीनीकृत करने पर पुरानी पॉलिसी का रिकॉर्ड सुरक्षित रखकर नई अवधि शुरू होती है; कोई डेटा ओवरराइट नहीं होता।',
          meta: 'स्प्रेडशीट ओवरराइटिंग से मुक्ति',
        },
      ],
    },
    roles: {
      badge: 'सख्त सुरक्षा',
      title: 'ब्रोकरेज कार्यालयों के लिए भूमिका-आधारित नियंत्रण',
      subtitle: 'कर्मचारियों को केवल उनके काम तक सीमित रखें, जबकि प्रबंधन को पूरे व्यवसाय की पूरी जानकारी मिले।',
      items: [
        {
          role: 'ब्रोकर एडमिन (Broker Admin)',
          tagline: 'प्रमुख ब्रोकर एवं निदेशक',
          scopeBadge: 'संपूर्ण बहीखाता पहुंच',
          permissions: [
            'सभी ग्राहकों, पॉलिसियों और नवीनीकरणों की पूरी दृश्यता',
            'ब्रोकरेज सेटिंग्स और टीम सदस्यों को जोड़ना',
            'Excel / CSV में डेटा का थोक आयात एवं निर्यात',
            'संपूर्ण ऑडिट लॉग और संचालन रिपोर्ट',
          ],
        },
        {
          role: 'मैनेजर (Manager)',
          tagline: 'संचालन प्रमुख एवं शाखा प्रबंधक',
          scopeBadge: 'संचालन पहुंच',
          permissions: [
            'चल रहे सभी नवीनीकरणों की पूरी दृश्यता',
            'नए ग्राहक और पॉलिसी जोड़ना व संशोधित करना',
            'कर्मचारियों को फ़ाइलें और कार्य सौंपना',
            'ग्राहक को भेजने से पहले कोटेशन की समीक्षा',
          ],
        },
        {
          role: 'कर्मचारी (Employee)',
          tagline: 'नवीनीकरण अधिकारी एवं कॉलर',
          scopeBadge: 'केवल सौंपी गई फ़ाइलें',
          permissions: [
            'केवल अपने सौंपे गए ग्राहकों और पॉलिसियों तक सीमित',
            'अन्य ब्रोकरों का डेटा या कुल राजस्व नहीं देख सकते',
            'कॉल नोट्स, कोटेशन दर्ज करना और कार्य पूरा करना',
            'व्यक्तिगत My Day प्राथमिकता सूची पर कार्य करना',
          ],
        },
      ],
    },
    pricing: {
      badge: 'पारदर्शी मूल्य निर्धारण',
      title: 'भारतीय ब्रोकरेज के लिए सरल और स्पष्ट योजनाएं',
      subtitle: 'कोई छिपा हुआ शुल्क नहीं। जैसे-जैसे आपका काम बढ़े, अपनी योजना चुनें।',
      billingToggleAnnual: 'वार्षिक भुगतान',
      billingToggleMonthly: 'मासिक भुगतान',
      annualSavings: 'वार्षिक योजना पर 20% की बचत',
      plans: [
        {
          name: 'निःशुल्क ट्रायल',
          tagline: 'अपने लैपटॉप पर BrokerOS का परीक्षण करें',
          priceAnnual: '₹0',
          periodAnnual: '14 दिनों के लिए',
          priceMonthly: '₹0',
          periodMonthly: '14 दिनों के लिए',
          policyLimit: 'एपेक्स डेमो डेटा शामिल',
          userLimit: 'एडमिन, मैनेजर, कर्मचारी खाते',
          features: [
            'डेमो डेटा के साथ पूर्ण कार्यक्षेत्र का परीक्षण',
            'नवीनीकरण फ़ाइल और कोटेशन तुलना',
            'My Day सुबह की कार्यसूची का अभ्यास',
            'Excel / CSV आयात का परीक्षण',
            'क्रेडिट कार्ड की आवश्यकता नहीं',
          ],
          ctaText: 'मुफ्त ट्रायल शुरू करें',
        },
        {
          name: 'स्टार्टर डेस्क (Starter Desk)',
          tagline: 'छोटे ब्रोकरेज और एकल डेस्क के लिए',
          priceAnnual: '₹4,999',
          periodAnnual: '/ वर्ष + GST (वार्षिक)',
          priceMonthly: '₹499',
          periodMonthly: '/ माह + GST (मासिक)',
          badge: 'स्टार्टर',
          policyLimit: '150 सक्रिय पॉलिसियों तक',
          userLimit: '1-2 ब्रोकर लॉगिन',
          features: [
            'पूर्ण नवीनीकरण फ़ाइल सिस्टम',
            'IST तिथियों में अवलोकन डैशबोर्ड',
            'My Day दैनिक प्राथमिकता सूची',
            'Excel / CSV थोक डेटा आयात',
            '2-3 बीमा कंपनियों की कोटेशन तुलना',
            'WhatsApp कोटेशन पूर्वावलोकन जनरेटर',
            'मानक ईमेल सहायता',
          ],
          ctaText: 'स्टार्टर डेस्क चुनें',
        },
        {
          name: 'ग्रोथ ब्रोकरेज (Growth)',
          tagline: 'बहु-सदस्यीय ब्रोकर कार्यालयों के लिए',
          priceAnnual: '₹9,999',
          periodAnnual: '/ वर्ष + GST (वार्षिक)',
          priceMonthly: '₹999',
          periodMonthly: '/ माह + GST (मासिक)',
          badge: 'सर्वाधिक लोकप्रिय',
          isPopular: true,
          policyLimit: '500 सक्रिय पॉलिसियों तक',
          userLimit: '5 ब्रोकर लॉगिन तक',
          features: [
            'स्टार्टर डेस्क की सभी सुविधाएं',
            'तीन-स्तरीय भूमिकाएं (एडमिन, मैनेजर, कर्मचारी)',
            'कर्मचारियों के लिए सख्त डेटा अलगाव',
            'स्वचालित 90/60/45/30/15/7/1 दिन रिमाइंडर',
            'कमीशन और प्रीमियम-जोखिम विश्लेषण',
            'Excel डेटा मैपिंग में प्राथमिकता सहायता',
            'WhatsApp एवं फोन पर प्राथमिकता सहायता',
          ],
          ctaText: 'ग्रोथ डेस्क चुनें',
        },
        {
          name: 'एंटरप्राइज डेस्क',
          tagline: 'बड़े वाणिज्यिक ब्रोकरों और कॉर्पोरेट्स के लिए',
          priceAnnual: '₹18,999',
          periodAnnual: '/ वर्ष + GST (वार्षिक)',
          priceMonthly: '₹1,899',
          periodMonthly: '/ माह + GST (मासिक)',
          badge: 'एंटरप्राइज',
          policyLimit: 'असीमित पॉलिसियां',
          userLimit: 'असीमित टीम सदस्य',
          features: [
            'ग्रोथ ब्रोकरेज की सभी सुविधाएं',
            'असीमित पॉलिसियां एवं स्टाफ लॉगिन',
            'समर्पित भारतीय खाता प्रबंधक',
            'कस्टम फ़ील्ड्स एवं MIS रिपोर्ट निर्यात',
            'पुरानी एक्सेल फाइलों के माइग्रेशन में सहायता',
            'आपकी टीम के लिए व्यक्तिगत प्रशिक्षण सत्र',
            '99.9% अपटाइम SLA गारंटी',
          ],
          ctaText: 'एंटरप्राइज डेस्क से संपर्क करें',
        },
      ],
      customNote: 'सभी योजनाओं में भारतीय रुपये (₹) और IST समय का पूर्ण समर्थन शामिल है।',
    },
    testimonials: {
      badge: 'ब्रोकर अनुभव',
      title: 'भारत भर के ब्रोकरेज प्रमुखों का विश्वास',
      subtitle: 'जानें कि कैसे भारतीय ब्रोकरों ने एक्सेल की अव्यवस्था को दूर किया।',
      items: [
        {
          quote: 'BrokerOS से पहले हमारे 3 लोग अलग-अलग एक्सेल शीट पर काम करते थे। अकेले अक्टूबर में हमने 4 बड़ी फायर पॉलिसियों (₹18 लाख प्रीमियम) को लैप्स होने से बचाया।',
          name: 'राजेश पटेल',
          title: 'प्रमुख ब्रोकर एवं निदेशक',
          brokerage: 'एपेक्स रिस्क एडवाइजर्स',
          city: 'अहमदाबाद, गुजरात',
          rating: 5,
          metric: '6 महीनों में 0 पॉलिसी लैप्स',
        },
        {
          quote: 'रोल-आधारित सुरक्षा ठीक वही थी जिसकी हमें ज़रूरत थी। हमारे कॉलर्स केवल अपने सौंपे गए कार्ड देखते हैं, जबकि मैं सभी 280 पॉलिसियों का कुल जोखिम देख सकता हूँ।',
          name: 'मीनाक्षी सुंदरम',
          title: 'संचालन प्रमुख',
          brokerage: 'डेक्कन इंश्योरेंस ब्रोकिंग',
          city: 'मुंबई, महाराष्ट्र',
          rating: 5,
          metric: '100% स्टाफ जवाबदेही',
        },
        {
          quote: 'कोटेशन तुलना से कॉर्पोरेट CFO को विकल्प दिखाना बहुत आसान हो गया है। हम 2-3 कोट तैयार करते हैं, WhatsApp पूर्वावलोकन कॉपी करते हैं और 1 क्लिक में नवीनीकृत करते हैं।',
          name: 'सीए अमित शाह',
          title: 'प्रबंध साझेदार',
          brokerage: 'गुजरात कॉर्पोरेट डेस्क',
          city: 'सूरत, गुजरात',
          rating: 5,
          metric: '3 गुना त्वरित ग्राहक स्वीकृति',
        },
      ],
    },
    fit: {
      badge: 'स्पष्ट जानकारी',
      title: 'क्या BrokerOS आपके कार्यालय के लिए उपयुक्त है?',
      forTitle: 'BrokerOS किसके लिए है:',
      forItems: [
        'IRDAI-लाइसेंस प्राप्त बीमा ब्रोकरेज जो 50 से 300+ पॉलिसियों का प्रबंधन करते हैं',
        'प्रमुख ब्रोकर जो पॉलिसी छूटने के जोखिम को शून्य करना चाहते हैं',
        'संचालन प्रबंधक जो एक्सेल और व्हाट्सएप की अव्यवस्था से मुक्त होना चाहते हैं',
        'वाणिज्यिक बीमा (फायर, मरीन, लायबिलिटी, GMC) के बहु-कोटेशन संभालने वाले कार्यालय',
      ],
      notForTitle: 'BrokerOS किसके लिए नहीं है:',
      notForItems: [
        'व्यक्तिगत ग्राहक जो ऑनलाइन बाइक या स्वास्थ्य बीमा खरीदना चाहते हैं',
        'POSP एजेंट नेटवर्क जो कमीशन बांटने वाला ऐप ढूंढ रहे हैं',
        'बीमा कंपनियों का प्रत्यक्ष अंडरराइटिंग पोर्टल',
        'संपूर्ण अकाउंटिंग या GST बिलिंग सॉफ्टवेयर',
      ],
    },
    faq: {
      badge: 'स्पष्ट उत्तर',
      title: 'अक्सर पूछे जाने वाले प्रश्न',
      subtitle: 'सुविधाओं, संचालन और क्षमताओं के बारे में वास्तविक तथ्य।',
      items: [
        {
          q: 'क्या आप ग्राहकों को स्वचालित व्हाट्सएप संदेश भेजते हैं?',
          a: 'नहीं। BrokerOS व्हाट्सएप प्रारूप में संदेश और कोटेशन तुलना का ड्राफ्ट तैयार करता है जिसे आपके कर्मचारी कॉपी करके भेज सकते हैं। सीधे स्वचालित व्हाट्सएप संदेश भेजने की सुविधा भविष्य में आएगी। आज हम इसका दावा नहीं करते।',
        },
        {
          q: 'क्या नवीनीकरण के बाद पुरानी पॉलिसी का रिकॉर्ड मिट जाता है?',
          a: 'नहीं। पुरानी समाप्त पॉलिसी इतिहास के रूप में हमेशा सुरक्षित रहती है। "नवीनीकृत" करने पर अगली अवधि (पुरानी समाप्ति + 1 दिन) की नई पॉलिसी बनती है। सूची में हमेशा वर्तमान अवधि दिखती है।',
        },
        {
          q: 'क्या कोई कर्मचारी दूसरे ब्रोकर का क्लाइंट डेटा देख सकता है?',
          a: 'नहीं। कर्मचारी केवल अपनी सौंपी गई फाइलों, ग्राहकों और कार्यों को देख सकते हैं। केवल एडमिन और मैनेजर को पूरे बहीखाते की जानकारी होती है।',
        },
        {
          q: 'ब्रोकरेज कमीशन की गणना कैसे होती है?',
          a: 'कमीशन की गणना हमेशा प्रीमियम × बीमा कंपनी के प्रतिशत से स्वचालित होती है। इसे कभी भी हाथ से मनचाही राशि के रूप में नहीं लिखा जाता, जिससे हिसाब में कोई गलती न हो।',
        },
        {
          q: 'क्या हम अपनी पुरानी एक्सेल फाइल से डेटा ला सकते हैं?',
          a: 'हाँ। BrokerOS में ग्राहकों और पॉलिसियों को सीधे Excel/CSV फ़ाइल से आयात करने की पूर्ण सुविधा उपलब्ध है।',
        },
        {
          q: 'क्या हम लाइव डेमो देख सकते हैं?',
          a: 'हाँ। आप 12 मिनट के लाइव डेमो का अनुरोध कर सकते हैं जिसमें हम एपेक्स इंश्योरेंस ब्रोकर्स के नमूने के साथ पूरा कार्यप्रवाह दिखाएंगे।',
        },
      ],
    },
    ctaBand: {
      title: 'नवीनीकरण कभी न चूकें।',
      sub: '12 मिनट का लाइव डेमो बुक करें: अवलोकन → एक नवीनीकरण फ़ाइल → My Day डेस्क।',
      form: {
        name: 'पूरा नाम',
        namePlaceholder: 'उदा. विक्रम मेहता',
        brokerage: 'ब्रोकरेज का नाम',
        brokeragePlaceholder: 'उदा. एपेक्स इंश्योरेंस ब्रोकर्स प्रा. लि.',
        city: 'शहर',
        cityPlaceholder: 'उदा. मुंबई, अहमदाबाद, सूरत, दिल्ली, पुणे',
        email: 'कार्य ईमेल',
        emailPlaceholder: 'vikram@apexbrokers.in',
        phone: 'मोबाइल / WhatsApp नंबर',
        phonePlaceholder: '+91 98200 12345',
        role: 'ब्रोकरेज में आपकी भूमिका',
        roleOptions: ['प्रमुख ब्रोकर / निदेशक', 'संचालन प्रबंधक (Ops Manager)', 'वितरक / भागीदार', 'वरिष्ठ नवीनीकरण अधिकारी'],
        bookSize: 'सक्रिय पॉलिसियों की संख्या',
        bookSizeOptions: ['50 से 150 पॉलिसियां', '150 से 300 पॉलिसियां', '300 से 1,000 पॉलिसियां', '1,000+ पॉलिसियां'],
        submitBtn: '12 मिनट का डेमो बुक करें',
        submitting: 'अनुरोध भेजा जा रहा है...',
        successTitle: 'डेमो अनुरोध प्राप्त हुआ',
        successMsg: 'धन्यवाद। हमारी टीम 1 कार्यदिवस के भीतर WhatsApp/ईमेल द्वारा 12 मिनट के वॉकथ्रू का समय तय करेगी।',
        resetBtn: 'दूसरा अनुरोध भेजें',
      },
    },
    footer: {
      tagline: 'नवीनीकरण कभी न चूकें',
      rights: '© 2026 BrokerOS. भारतीय बीमा ब्रोकरेज के लिए विशेष रूप से निर्मित।',
      disclaimer: 'BrokerOS IRDAI-लाइसेंस प्राप्त बीमा ब्रोकरों के लिए एक संचालन डेस्क सॉफ्टवेयर है। BrokerOS कोई बीमा कंपनी या एजेंट नेटवर्क नहीं है।',
      links: {
        product: 'उत्पाद अवलोकन',
        pricing: 'मूल्य निर्धारण',
        privacy: 'गोपनीयता एवं सुरक्षा',
        contact: 'संपर्क करें',
        signIn: 'ब्रोकर लॉगिन',
      },
    },
  },
  gu: {
    nav: {
      product: 'પ્રોડક્ટ',
      features: 'વિશેષતાઓ',
      policyTypes: 'પૉલિસી પ્રકારો',
      howItWorks: 'કેવી રીતે કાર્ય કરે છે',
      roles: 'ભૂમિકાઓ',
      pricing: 'કિંમતો (Pricing)',
      faq: 'પ્રશ્નોત્તરી',
      testimonials: 'બ્રોકર્સ વિશ્વાસ',
      signIn: 'સાઇન ઇન',
      requestDemo: 'ડેમો માટે વિનંતી કરો',
    },
    hero: {
      eyebrow: 'વીમા બ્રોકર ઓપરેશન્સ ડેસ્ક',
      h1: 'રિન્યુઅલ ક્યારેય ન ચૂકો',
      sub: 'દરેક પૉલિસીની એક ફાઇલ: જવાબદાર વ્યક્તિ, આગળનું પગલું, IST તારીખો અને ચાલુ મુદત — એક્સેલ શીટની ઝંઝટ વિના.',
      proofChips: {
        ist: 'IST તારીખો અને ભારતીય સમય',
        inr: '₹ જોખમમાં પ્રીમિયમનું ટ્રેકિંગ',
        employeeRole: 'કર્મચારી ફક્ત સોંપેલ કામ જ જોઈ શકે',
        aiReady: 'Excel/CSV બલ્ક ડેટા ઇમ્પોર્ટ',
      },
      ctaPrimary: '12 મિનિટનો ડેમો બુક કરો',
      ctaSecondary: 'લાઈવ રિન્યુઅલ ફાઇલ જુઓ',
      ctaTour: 'ઇન્ટરેક્ટિવ ડેસ્ક ટૂર',
    },
    mock: {
      tabs: {
        overview: 'અવલોકન ડૅશબોર્ડ',
        renewalFile: 'રિન્યુઅલ ફાઇલ POL-D008',
        myDay: 'મારો દિવસ (My Day)',
        quoteCompare: 'ક્વોટ સરખામણી મેટ્રિક્સ',
      },
      brokerageName: 'એપેક્સ ઇન્સ્યોરન્સ બ્રોકર્સ પ્રા. લિ.',
      fileNumber: 'REN-2026-088',
      clientName: 'મલબાર સ્પાઇસિસ પ્રા. લિ.',
      policyType: 'સ્ટાન્ડર્ડ ફાયર એન્ડ સ્પેશિયલ પેરિલ્સ પૉલિસી (SFSP)',
      insurer: 'ICICI લોમ્બાર્ડ GIC',
      premium: '₹4,85,000',
      statusOverdue: '27 દિવસ બાકી (Overdue)',
      statusActive: 'સક્રિય રિન્યુઅલ ફાઇલ',
      owner: 'રાજેશ શર્મા (સિનિયર બ્રોકર)',
      nextAction: 'સાંજે 4:00 વાગ્યા સુધીમાં CFO ને ICICI અને HDFC ક્વોટેશન રજૂ કરો',
      quotesTitle: 'વીમા કંપની ક્વોટેશન્સ (3 માંથી 2 મળ્યા)',
      quotes: [
        { insurer: 'ICICI લોમ્બાર્ડ GIC', amount: '₹4,68,200', selected: true, badge: 'ભલામણ કરેલ', features: 'આતંકવાદ + ધરતીકંપ એડ-ઓન સામેલ' },
        { insurer: 'HDFC ERGO GIC', amount: '₹4,52,000', selected: false, badge: 'સૌથી ઓછું L1', features: 'સ્ટાન્ડર્ડ SFSP શરતો' },
        { insurer: 'ધ ન્યૂ ઇન્ડિયા એશ્યોરન્સ', amount: '₹4,91,500', selected: false, features: 'PSU કવર પૂર એડ-ઓન સાથે' },
      ],
      timeline: 'ગઈકાલે 16:30 IST · ગ્રાહકે સુધારેલ ધરતીકંપ એડ-ઓન ક્વોટ માંગ્યો',
      markRenewed: 'રિન્યુ થયેલ ચિહ્નિત કરો (નવી મુદત)',
      sharePreview: 'WhatsApp ક્વોટેશન પૂર્વાવલોકન',
      metrics: {
        overdueLabel: 'બાકી રિન્યુઅલ્સ',
        overdueVal: '14',
        overdueSub: 'મુદત વીતી ગઈ છે',
        due7Label: '7 દિવસમાં બાકી',
        due7Val: '8',
        due7Sub: 'તાત્કાલિક મુદત સમયગાળો',
        due30Label: '30 દિવસમાં બાકી',
        due30Val: '23',
        due30Sub: 'ક્વોટેશન પ્રક્રિયામાં',
        atRiskLabel: 'જોખમમાં પ્રીમિયમ',
        atRiskVal: '₹38.4 લાખ',
        atRiskSub: '45 સક્રિય ફાઇલોમાં',
      },
      myDayTasks: [
        { time: 'સવારે 10:30', client: 'મલબાર સ્પાઇસિસ', task: 'ICICI ફાયર ક્વોટ મંજૂરી માટે CFO ને કૉલ કરો', tag: 'બાકી', tagType: 'danger' },
        { time: 'બપોરે 12:00', client: 'ગુજરાત સ્ટીલ ટ્યુબ્સ', task: 'મરીન ટ્રાન્ઝિટ રિન્યુઅલ માટે સહી કરેલ પ્રપોઝલ લો', tag: 'આજે બાકી', tagType: 'warn' },
        { time: 'બપોરે 03:15', client: 'અરબિંદો લોજિસ્ટિક્સ', task: '15-દિવસના પ્રી-ઇન્સ્પેક્શન રિપોર્ટ પર ફોલો-અપ લો', tag: 'ફોલો-અપ', tagType: 'ok' },
      ],
    },
    problem: {
      badge: 'સ્પ્રેડશીટની મુશ્કેલીઓ',
      title: 'ભારતીય બ્રોકરેજ દર મહિને રિન્યુઅલ કેમ ગુમાવે છે?',
      subtitle: 'એક્સેલ શીટ્સ, પર્સનલ વોટ્સએપ ચેટ્સ અને વિખરાયેલ ઇમેઇલ્સને કારણે પૉલિસી અજાણતા લેપ્સ થઈ જાય છે.',
      cards: [
        {
          title: 'ફાઇલ પર કોઈ માલિક નથી',
          description: 'જ્યારે ત્રણ એક્ઝિક્યુટિવ્સ માને છે કે બીજા કોઈએ કૉલ કર્યો હશે, ત્યારે પૉલિસી આપોઆપ લેપ્સ થાય છે.',
          tag: 'બિન-સોંપાયેલ જોખમ',
        },
        {
          title: 'આગળનું પગલું ક્યાંય લખેલું નથી',
          description: 'ક્વોટ્સ વોટ્સએપમાં પડ્યા રહે છે અને ફોલો-અપ વગર 15-દિવસનો સમયગાળો નીકળી જાય છે.',
          tag: 'સંદર્ભ ગુમાવ્યો',
        },
        {
          title: 'જૂની અને નવી મુદતની ભેળસેળ',
          description: 'એક્સેલમાં જૂની પૉલિસી પર નવો ડેટા ઓવરરાઇટ થવાથી જૂનો ક્લેમ ઇતિહાસ બગડી જાય છે.',
          tag: 'ડેટા ખરાબી',
        },
      ],
      conclusion: 'BrokerOS માં દરેક રિન્યુઅલ ફાઇલ જ તમામ કામગીરીનો મુખ્ય આધાર છે.',
    },
    policyTypes: {
      badge: 'તમામ પૉલિસી પ્રકારો',
      title: 'દરેક વીમા પોર્ટફોલિયોનું એક જ ડેસ્ક પર સંચાલન',
      subtitle: 'વાણિજ્યિક પ્રોપર્ટી, મરીનથી લઈને કોર્પોરેટ ગ્રુપ હેલ્થ અને લાયબિલિટી પૉલિસીઓ સુધી.',
      types: [
        { icon: 'fire', name: 'સ્ટાન્ડર્ડ ફાયર એન્ડ સ્પેશિયલ પેરિલ્સ (SFSP)', desc: 'STFI, ધરતીકંપ, આતંકવાદ એડ-ઓન્સ, સમ ઇન્સ્યોર્ડ વેલ્યુએશન અને એસેટ રજિસ્ટર.', sample: 'POL-F104 · ₹12.5 Cr સમ ઇન્સ્યોર્ડ' },
        { icon: 'shield-plus', name: 'ગ્રુપ હેલ્થ (GMC) અને GPA', desc: 'કર્મચારી સંખ્યા ફેરફાર, ફેમિલી ફ્લોટર, ક્લેમ્સ લોસ-રેશિયો અને TPA સિન્ક.', sample: 'POL-G088 · 340 કર્મચારીઓ' },
        { icon: 'truck', name: 'કોમર્શિયલ વ્હીકલ અને ફ્લીટ', desc: 'વાહન નંબર, ચેસિસ સર્ચ, NCB રક્ષણ, IDV ગણતરી અને PUC ટ્રેકિંગ.', sample: 'POL-V902 · 42 ટ્રક' },
        { icon: 'water', name: 'મરીન કાર્ગો અને ટ્રાન્ઝિટ', desc: 'ઓપન ડિક્લેરેશન મરીન પૉલિસીઓ, સફર પ્રમાણપત્રો, વાર્ષિક ટર્નઓવર પૉલિસીઓ.', sample: 'POL-M301 · વાર્ષિક ઓપન' },
        { icon: 'briefcase', name: 'ડિરેક્ટર્સ અને ઓફિસર્સ (D&O)', desc: 'રન-ઓફ કવર, અગાઉની તારીખો, કાનૂની સંરક્ષણ ખર્ચ સબ-લિમિટ્સ.', sample: 'POL-D014 · ₹25 Cr મર્યાદા' },
        { icon: 'cpu', name: 'સાયબર રિસ્ક અને પબ્લિક લાયબિલિટી', desc: 'તૃતીય પક્ષ જવાબદારીઓ, ક્લાઉડ અડચણ, સરકારી નિયમનકારી કાનૂની ખર્ચ.', sample: 'POL-C772 · કોર્પોરેટ પૉલિસી' },
      ],
    },
    features: {
      badge: 'ઓપરેશનલ એન્જિન',
      title: 'ભારતીય ઇન્સ્યોરન્સ બ્રોકર્સ માટે ખાસ બનાવેલ',
      subtitle: 'દરેક પ્રક્રિયા એ રીતે તૈયાર કરવામાં આવી છે જે રીતે લાયસન્સ પ્રાપ્ત બ્રોકર ઓફિસ વાસ્તવમાં કામ કરે છે.',
      items: [
        {
          icon: 'grid-1x2',
          title: 'અવલોકન ડૅશબોર્ડ (Overview)',
          description: 'બાકી ફાઇલો, 7-દિવસ અને 30-દિવસની મુદત અને જોખમમાં રહેલા ₹ પ્રીમિયમનું ત્વરિત વિશ્લેષણ.',
          highlight: 'ત્વરિત વિશ્લેષણ',
        },
        {
          icon: 'sun',
          title: 'મારો દિવસ (My Day)',
          description: 'IST માં સવારની કાર્યસૂચિ — બાકી ફાઇલો, આજના કાર્યો અને 1-ક્લિકમાં સ્ટેટસ અપડેટ.',
          highlight: 'સવારની કાર્યસૂચિ',
        },
        {
          icon: 'folder2-open',
          title: 'રિન્યુઅલ ફાઇલ',
          description: '2–3 ક્વોટેશન્સ, જવાબદાર વ્યક્તિ, સ્ટેજ અને 1-ક્લિકમાં રિન્યુઅલ પૂર્ણ કે રદ કરવાની વ્યવસ્થા.',
          highlight: 'મુખ્ય રેકોર્ડ',
        },
        {
          icon: 'file-earmark-spreadsheet',
          title: 'Excel / CSV આયાત',
          description: 'તમારા હાલના ગ્રાહકો અને સક્રિય પૉલિસીઓનો ચોપડો ફરી ટાઇપ કર્યા વિના મિનિટોમાં ઇમ્પોર્ટ કરો.',
          highlight: 'ફરી ટાઇપિંગ મુક્ત',
        },
        {
          icon: 'search',
          title: 'ઝડપી શોધ અને ક્વિક નોટ',
          description: 'ગ્રાહકનું નામ, ફોન, પૉલિસી નંબર કે વાહન નંબર એક જ સેકન્ડમાં શોધો; 10 સેકન્ડમાં કૉલ નોટ લખો.',
          highlight: '1-સેકન્ડ શોધ',
        },
        {
          icon: 'shield-lock',
          title: 'ત્રણ-સ્તરીય રોલ્સ (Roles)',
          description: 'બ્રોકર એડમિન આખો ચોપડો સંભાળે; મેનેજર કામગીરી ચલાવે; કર્મચારી ફક્ત સોંપેલ ફાઇલ જ જુએ.',
          highlight: 'ડેટા સુરક્ષા',
        },
      ],
    },
    howItWorks: {
      badge: 'સરળ કાર્યપદ્ધતિ',
      title: 'BrokerOS માં રિન્યુઅલ કેવી રીતે આગળ વધે છે?',
      subtitle: 'ઓટોમેટિક રિમાઇન્ડરથી લઈને નવી પૉલિસી ટર્મ શરૂ થવા સુધીનું સ્પષ્ટ આયોજન.',
      steps: [
        {
          step: '01',
          title: 'પૉલિસી મુદત નજીક આવે છે → રિન્યુઅલ ફાઇલ ખૂલે છે',
          description: 'સમાપ્તિ પહેલાં 90, 60, 45, 30, 15, 7 અને 1 દિવસે ઓટોમેટિક રિમાઇન્ડર ટાસ્ક બને છે.',
          meta: 'ઓટોમેટિક 7-તબક્કા રિમાઇન્ડર સિસ્ટમ',
        },
        {
          step: '02',
          title: 'જવાબદાર વ્યક્તિ નિયુક્ત + આગળનું પગલું નોંધાયું',
          description: 'સોંપાયેલ બ્રોકર ગ્રાહકનો સંપર્ક કરે છે, 2-3 ક્વોટ્સ મેળવે છે અને વોટ્સએપ પ્રીવ્યુ તૈયાર કરે છે.',
          meta: 'સિંગલ સોર્સ ઓફ ટ્રુથ અને ટાઇમલાઇન',
        },
        {
          step: '03',
          title: 'ગ્રાહકનો નિર્ણય → રિન્યુ અથવા રદ ચિહ્નિત કરો',
          description: 'રિન્યુ કરતાં જ જૂનો રેકોર્ડ સુરક્ષિત રહીને નવી મુદત આપોઆપ શરૂ થાય છે; કોઈ ઓવરરાઇટિંગ નહીં.',
          meta: 'એક્સેલ ઓવરરાઇટિંગથી મુક્તિ',
        },
      ],
    },
    roles: {
      badge: 'ચોક્કસ સુરક્ષા',
      title: 'બ્રોકરેજ ઓફિસ માટે રોલ-આધારિત એક્સેસ',
      subtitle: 'ટીમના સભ્યો ફક્ત તેમને સોંપેલ કામ જ જોઈ શકે છે, જ્યારે લીડરશીપ પાસે આખા પુસ્તકની વિગત રહે છે.',
      items: [
        {
          role: 'બ્રોકર એડમિન (Broker Admin)',
          tagline: 'મુખ્ય બ્રોકર અને ડિરેક્ટર',
          scopeBadge: 'સંપૂર્ણ પુસ્તક એક્સેસ',
          permissions: [
            'બધા ગ્રાહકો, પૉલિસીઓ અને રિન્યુઅલ્સની સંપૂર્ણ વિગત',
            'બ્રોકરેજ સેટિંગ્સ અને નવી ટીમના સભ્યોને જોડવા',
            'Excel / CSV માં જથ્થાબંધ ડેટા ઇમ્પોર્ટ અને એક્સપોર્ટ',
            'ઓડિટ લોગ્સ અને વ્યવસાયિક મેટ્રિક્સ',
          ],
        },
        {
          role: 'મેનેજર (Manager)',
          tagline: 'ઓપરેશન્સ હેડ અને બ્રાન્ચ મેનેજર્સ',
          scopeBadge: 'ઓપરેશન્સ એક્સેસ',
          permissions: [
            'ચાલુ તમામ રિન્યુઅલ્સની સંપૂર્ણ વિગત',
            'નવા ગ્રાહકો અને પૉલિસીઓ ઉમેરવા અને સુધારવા',
            'ટીમને ફાઇલો અને કાર્યો સોંપવા',
            'ગ્રાહકને મોકલતા પહેલા ક્વોટેશન્સની સમીક્ષા કરવી',
          ],
        },
        {
          role: 'કર્મચારી (Employee)',
          tagline: 'રિન્યુઅલ એક્ઝિક્યુટિવ્સ અને કૉલર્સ',
          scopeBadge: 'ફક્ત સોંપેલ ફાઇલો',
          permissions: [
            'ફક્ત પોતાને સોંપેલા ગ્રાહકો અને પૉલિસીઓ જ જોઈ શકે',
            'અન્ય બ્રોકર્સનો ડેટા કે કુલ આવક જોઈ શકતા નથી',
            'કૉલ નોટ્સ, ક્વોટેશન્સ નોંધવા અને ટાસ્ક પૂરા કરવા',
            'પોતાની My Day સવારની યાદી પર કામ કરવું',
          ],
        },
      ],
    },
    pricing: {
      badge: 'પારદર્શક કિંમતો (Pricing)',
      title: 'ભારતીય બ્રોકરેજ માટે સરળ અને વાજબી પ્લાન્સ',
      subtitle: 'કોઈ છુપો ખર્ચ નથી. જેમ તમારો ચોપડો વધે તેમ પ્લાન અપગ્રેડ કરો.',
      billingToggleAnnual: 'વાર્ષિક બિલિંગ',
      billingToggleMonthly: 'માસિક બિલિંગ',
      annualSavings: 'વાર્ષિક પ્લાન પર 20% ની બચત',
      plans: [
        {
          name: 'ફ્રી ટ્રાયલ',
          tagline: 'તમારા લેપટોપ પર BrokerOS ચલાવી જુઓ',
          priceAnnual: '₹0',
          periodAnnual: '14 દિવસ માટે',
          priceMonthly: '₹0',
          periodMonthly: '14 દિવસ માટે',
          policyLimit: 'એપેક્સ ડેમો ડેટા સામેલ',
          userLimit: 'એડમિન, મેનેજર, સ્ટાફ એકાઉન્ટ્સ',
          features: [
            'ડેમો ડેટા સાથે સંપૂર્ણ વર્કસ્પેસ ટ્રાયલ',
            'રિન્યુઅલ ફાઇલ અને ક્વોટેશન સરખામણી',
            'My Day સવારની યાદીનો અનુભવ',
            'Excel / CSV ઇમ્પોર્ટ ટેસ્ટિંગ',
            'ક્રેડિટ કાર્ડની જરૂર નથી',
          ],
          ctaText: 'ફ્રી ટ્રાયલ શરૂ કરો',
        },
        {
          name: 'સ્ટાર્ટર ડેસ્ક (Starter Desk)',
          tagline: 'નાની બ્રોકરેજ અને સિંગલ ડેસ્ક માટે',
          priceAnnual: '₹4,999',
          periodAnnual: '/ વર્ષ + GST (વાર્ષિક)',
          priceMonthly: '₹499',
          periodMonthly: '/ મહિને + GST (માસિક)',
          badge: 'સ્ટાર્ટર',
          policyLimit: '150 સક્રિય પૉલિસીઓ સુધી',
          userLimit: '1-2 બ્રોકર લૉગિન',
          features: [
            'સંપૂર્ણ રિન્યુઅલ ફાઇલ સિસ્ટમ',
            'IST તારીખો સાથે અવલોકન ડૅશબોર્ડ',
            'My Day પ્રાથમિકતા કાર્યસૂચિ',
            'Excel / CSV જથ્થાબંધ ડેટા આયાત',
            '2-3 વીમા કંપની ક્વોટ્સ સરખામણી',
            'WhatsApp ક્વોટ પ્રીવ્યુ જનરેટર',
            'સ્ટાન્ડર્ડ ઇમેઇલ સપોર્ટ',
          ],
          ctaText: 'સ્ટાર્ટર ડેસ્ક પસંદ કરો',
        },
        {
          name: 'ગ્રોથ બ્રોકરેજ (Growth)',
          tagline: 'મલ્ટી-બ્રોકર ઓફિસ અને ટીમો માટે',
          priceAnnual: '₹9,999',
          periodAnnual: '/ વર્ષ + GST (વાર્ષિક)',
          priceMonthly: '₹999',
          periodMonthly: '/ મહિને + GST (માસિક)',
          badge: 'સૌથી વધુ લોકપ્રિય',
          isPopular: true,
          policyLimit: '500 સક્રિય પૉલિસીઓ સુધી',
          userLimit: '5 બ્રોકર લૉગિન સુધી',
          features: [
            'સ્ટાર્ટર ડેસ્કના તમામ ફીચર્સ',
            'ત્રણ-સ્તરીય રોલ્સ (એડમિન, મેનેજર, કર્મચારી)',
            'કર્મચારીઓ માટે સોંપાયેલ ડેટા સુરક્ષા',
            'ઓટોમેટિક 90/60/45/30/15/7/1 દિવસ રિમાઇન્ડર્સ',
            'કમિશન અને પ્રીમિયમ જોખમ એનાલિટિક્સ',
            'Excel ડેટા મેપિંગમાં પ્રાયોરિટી સહાય',
            'WhatsApp અને ફોન પર પ્રાથમિકતા સપોર્ટ',
          ],
          ctaText: 'ગ્રોથ ડેસ્ક પસંદ કરો',
        },
        {
          name: 'એન્ટરપ્રાઇઝ બુક',
          tagline: 'મોટા કોમર્શિયલ બ્રોકર્સ અને ડિસ્ટ્રિબ્યુટર્સ માટે',
          priceAnnual: '₹18,999',
          periodAnnual: '/ વર્ષ + GST (વાર્ષિક)',
          priceMonthly: '₹1,899',
          periodMonthly: '/ મહિને + GST (માસિક)',
          badge: 'એન્ટરપ્રાઇઝ',
          policyLimit: 'અમર્યાદિત પૉલિસીઓ',
          userLimit: 'અમર્યાદિત ટીમ બેઠકો',
          features: [
            'ગ્રોથ બ્રોકરેજના તમામ ફીચર્સ',
            'અમર્યાદિત પૉલિસીઓ અને સ્ટાફ સીટ્સ',
            'ડેડિકેટેડ ભારતીય એકાઉન્ટ મેનેજર',
            'કસ્ટમ ફિલ્ડ્સ અને MIS રિપોર્ટ એક્સપોર્ટ',
            'જૂની એક્સેલ શીટ્સનું માઇગ્રેશન સપોર્ટ',
            'તમારી ટીમ માટે લાઈવ ટ્રેનિંગ સેશન',
            '99.9% અપટાઇમ SLA ગેરંટી',
          ],
          ctaText: 'એન્ટરપ્રાઇઝ ડેસ્કનો સંપર્ક કરો',
        },
      ],
      customNote: 'તમામ પ્લાન્સમાં ભારતીય રૂપિયા (₹) અને IST તારીખોનું સંપૂર્ણ સમર્થન છે.',
    },
    testimonials: {
      badge: 'બ્રોકર્સ વિશ્વાસ',
      title: 'ભારતભરના ઓપરેશન્સ હેડ્સનો અનુભવ',
      subtitle: 'જાણો કે કેવી રીતે કોમર્શિયલ બ્રોકર્સે એક્સેલની ગૂંચવણ દૂર કરી.',
      items: [
        {
          quote: 'BrokerOS પહેલાં અમારા 3 કર્મચારીઓ અલગ-અલગ એક્સેલ શીટ ચલાવતા હતા. માત્ર ઓક્ટોબરમાં અમે ₹18 લાખના પ્રીમિયમની 4 મોટી ફાયર પૉલિસીઓને છૂટી જતાં બચાવી લીધી.',
          name: 'રાજેશ પટેલ',
          title: 'મુખ્ય બ્રોકર અને ડિરેક્ટર',
          brokerage: 'એપેક્સ રિસ્ક એડવાઇઝર્સ',
          city: 'અમદાવાદ, ગુજરાત',
          rating: 5,
          metric: '6 મહિનામાં 0 પૉલિસી લેપ્સ',
        },
        {
          quote: 'રોલ-આધારિત સુરક્ષા ખરેખર ઉત્તમ છે. અમારા કૉલર્સ ફક્ત My Day પર પોતાનું કામ જ જુએ છે, જ્યારે હું બધી 280 પૉલિસીઓનું કુલ જોખમ જોઈ શકું છું.',
          name: 'મીનાક્ષી સુંદરમ',
          title: 'હેડ ઓફ ઓપરેશન્સ',
          brokerage: 'ડેક્કન ઇન્સ્યોરન્સ બ્રોકિંગ',
          city: 'મુંબઈ, મહારાષ્ટ્ર',
          rating: 5,
          metric: '100% ટીમ જવાબદારી',
        },
        {
          quote: 'ક્વોટ સરખામણી મેટ્રિક્સથી CFO ને વિકલ્પો બતાવવા ખૂબ સરળ બની ગયા છે. અમે 2-3 ક્વોટ્સ બનાવીએ છીએ, WhatsApp પ્રીવ્યુ મોકલીએ છીએ અને 1 ક્લિકમાં રિન્યુ કરીએ છીએ.',
          name: 'સીએ અમિત શાહ',
          title: 'મેનેજિંગ પાર્ટનર',
          brokerage: 'ગુજરાત કોર્પોરેટ ડેસ્ક',
          city: 'સુરત, ગુજરાત',
          rating: 5,
          metric: '3 ગણી ઝડપી મંજૂરી',
        },
      ],
    },
    fit: {
      badge: 'યોગ્ય પસંદગી',
      title: 'શું BrokerOS તમારી ઓફિસ માટે યોગ્ય છે?',
      forTitle: 'BrokerOS કોના માટે છે:',
      forItems: [
        'IRDAI-લાયસન્સ ધરાવતી ઇન્સ્યોરન્સ બ્રોકરેજ જે 50 થી 300+ પૉલિસીઓ સંભાળે છે',
        'મુખ્ય બ્રોકર્સ જે રિન્યુઅલ છૂટી જવાનું જોખમ શૂન્ય કરવા માંગે છે',
        'ઓપરેશન્સ મેનેજર્સ જે એક્સેલ અને વોટ્સએપની અંધાધૂંધીથી મુક્ત થવા માંગે છે',
        'કોમર્શિયલ લાઇન્સ (ફાયર, મરીન, લાયબિલિટી, GMC) ના મલ્ટી-ક્વોટ્સ સંભાળતી ટીમો',
      ],
      notForTitle: 'BrokerOS કોના માટે નથી:',
      notForItems: [
        'ગ્રાહકો જે ઓનલાઇન બાઇક કે હેલ્થ ઇન્સ્યોરન્સ ખરીદવા માંગે છે',
        'POSP એજન્ટ નેટવર્ક જે કમિશન વહેંચતી એપ શોધી રહ્યા છે',
        'વીમા કંપનીઓનું સીધું અંડરરાઇટિંગ પોર્ટલ',
        'સંપૂર્ણ એકાઉન્ટિંગ કે GST બિલિંગ સોફ્ટવેર',
      ],
    },
    faq: {
      badge: 'સ્પષ્ટ જવાબો',
      title: 'વારંવાર પૂછાતા પ્રશ્નો',
      subtitle: 'સુવિધાઓ અને ક્ષમતાઓ વિશે સાચી અને પ્રમાણિક માહિતી.',
      items: [
        {
          q: 'શું તમે ગ્રાહકોને ઓટોમેટિક WhatsApp મેસેજ મોકલો છો?',
          a: 'ના. BrokerOS વોટ્સએપ ફોર્મેટમાં ક્વોટ પ્રીવ્યુ અને મેસેજનો ડ્રાફ્ટ તૈયાર કરે છે જેને તમારા એક્ઝિક્યુટિવ્સ કોપી કરીને મોકલી શકે છે. સીધા ઓટોમેટિક WhatsApp Business API મોકલવાની સુવિધા ભવિષ્યમાં આવશે. અમે આજે ઓટોમેટિક મોકલવાનો દાવો કરતા નથી.',
        },
        {
          q: 'શું રિન્યુઅલ પછી જૂની પૉલિસીનો રેકોર્ડ ભૂંસાઈ જાય છે?',
          a: 'ના. જૂની સમાપ્ત થયેલી પૉલિસી ઇતિહાસ તરીકે કાયમ માટે સાચવવામાં આવે છે. "રિન્યુ થયેલ" કરતાં જ નવી મુદત (જૂની મુદત + 1 દિવસ) ની નવી પૉલિસી બને છે. યાદીમાં હંમેશા ચાલુ મુદત જ દેખાય છે.',
        },
        {
          q: 'શું કોઈ કર્મચારી બીજા બ્રોકરનો ગ્રાહક ડેટા જોઈ શકે છે?',
          a: 'ના. કર્મચારીઓ ફક્ત પોતાને સોંપેલી ફાઇલો, પૉલિસીઓ અને કાર્યો જ જોઈ શકે છે. ફક્ત એડમિન અને મેનેજર પાસે સંપૂર્ણ ચોપડાની વિગત હોય છે.',
        },
        {
          q: 'બ્રોકરેજ કમિશનની ગણતરી કેવી રીતે થાય છે?',
          a: 'કમિશન હંમેશા પ્રીમિયમ × વીમા કંપનીના ટકા પરથી આપોઆપ ગણાય છે. તેને ક્યારેય હાથથી લખવામાં આવતું નથી, જેથી હિસાબમાં કોઈ ભૂલ ન થાય.',
        },
        {
          q: 'શું અમે જૂની એક્સેલ ફાઇલમાંથી ડેટા લાવી શકીએ?',
          a: 'હા. BrokerOS માં ગ્રાહકો અને પૉલિસીઓ સીધા Excel/CSV ફાઇલમાંથી ઇમ્પોર્ટ કરવાની સુવિધા સામેલ છે.',
        },
        {
          q: 'શું અમે લાઈવ ડેમો જોઈ શકીએ?',
          a: 'હા. તમે 12 મિનિટના લાઈવ ડેમો માટે વિનંતી કરી શકો છો, જેમાં અમે એપેક્સ ઇન્સ્યોરન્સ બ્રોકર્સના વર્કસ્પેસ સાથે સંપૂર્ણ ડેસ્ક પ્રક્રિયા બતાવીશું.',
        },
      ],
    },
    ctaBand: {
      title: 'રિન્યુઅલ ક્યારેય ન ચૂકો.',
      sub: '12 મિનિટનો લાઈવ ડેમો બુક કરો: અવલોકન → એક રિન્યુઅલ ફાઇલ → My Day ડેસ્ક.',
      form: {
        name: 'પૂરું નામ',
        namePlaceholder: 'દા.ત. વિક્રમ મહેતા',
        brokerage: 'બ્રોકરેજનું નામ',
        brokeragePlaceholder: 'દા.ત. એપેક્સ ઇન્સ્યોરન્સ બ્રોકર્સ પ્રા. લિ.',
        city: 'શહેર',
        cityPlaceholder: 'દા.ત. અમદાવાદ, સુરત, રાજકોટ, મુંબઈ, વડોદરા',
        email: 'કામનું ઇમેઇલ',
        emailPlaceholder: 'vikram@apexbrokers.in',
        phone: 'મોબાઇલ / WhatsApp નંબર',
        phonePlaceholder: '+91 98200 12345',
        role: 'બ્રોકરેજમાં તમારી ભૂમિકા',
        roleOptions: ['મુખ્ય બ્રોકર / ડિરેક્ટર', 'ઓપરેશન્સ મેનેજર', 'ડિસ્ટ્રિબ્યુટર / ભાગીદાર', 'સિનિયર રિન્યુઅલ એક્ઝિક્યુટિવ'],
        bookSize: 'સક્રિય પૉલિસીઓની સંખ્યા',
        bookSizeOptions: ['50 થી 150 પૉલિસીઓ', '150 થી 300 પૉલિસીઓ', '300 થી 1,000 પૉલિસીઓ', '1,000+ પૉલિસીઓ'],
        submitBtn: '12 મિનિટનો ડેમો બુક કરો',
        submitting: 'વિનંતી મોકલાઈ રહી છે...',
        successTitle: 'ડેમો વિનંતી મળી ગઈ છે',
        successMsg: 'આભાર. અમારી ટીમ 1 કામકાજના દિવસમાં WhatsApp/ઇમેઇલ દ્વારા 12 મિનિટના વોકથ્રૂનો સમય નક્કી કરવા સંપર્ક કરશે.',
        resetBtn: 'બીજી વિનંતી મોકલો',
      },
    },
    footer: {
      tagline: 'રિન્યુઅલ ક્યારેય ન ચૂકો',
      rights: '© 2026 BrokerOS. ભારતીય વીમા બ્રોકરેજ માટે ખાસ બનાવેલ.',
      disclaimer: 'BrokerOS એ IRDAI-લાયસન્સ પ્રાપ્ત વીમા બ્રોકર્સ માટેનું ઓપરેશનલ ડેસ્ક સોફ્ટવેર છે. BrokerOS કોઈ વીમા કંપની કે એજન્ટ નેટવર્ક નથી.',
      links: {
        product: 'પ્રોડક્ટ અવલોકન',
        pricing: 'કિંમતો (Pricing)',
        privacy: 'ગોપનીયતા અને સુરક્ષા',
        contact: 'સંપર્ક કરો',
        signIn: 'બ્રોકર લૉગિન',
      },
    },
  },
}
