export interface ApiResponse<T> {
  success: boolean
  data: T | null
  message: string | null
  errors: { field?: string; message: string }[] | null
  traceId: string | null
}

export interface SystemStatus {
  productName: string
  tagline: string
  environment: string
  apiVersion: string
  utcNow: string
  databaseConfigured: boolean
  demoResetEnabled: boolean
}

export interface CurrentUser {
  publicUserId: string
  email: string
  fullName: string
  role: string
  organizationPublicId: string
  organizationName: string
  organizationCode: string
}

export interface AuthResponse {
  accessToken: string
  expiresAtUtc: string
  user: CurrentUser
}

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export interface UpcomingRenewal {
  renewalPublicId: string
  clientName: string
  policyNumber: string
  policyType: string
  insurerName: string
  premium: number
  expiryDate: string
  daysRemaining: number
  status: string
  priority: string
  assignedUserName: string | null
}

export interface DashboardTask {
  publicId: string
  title: string
  description: string | null
  dueDateUtc: string
  priority: string
  status: string
  clientName: string | null
  policyNumber: string | null
  renewalPublicId: string | null
  assignedUserName: string | null
}

export interface Dashboard {
  currentUserName: string
  totalClients: number
  activePolicies: number
  renewalsOverdue: number
  renewalsDueToday: number
  renewalsDueWithin7Days: number
  renewalsDueWithin30Days: number
  renewalsDueWithin60Days: number
  premiumAtRisk: number
  pendingTasks: number
  completedTasksToday: number
  pendingFollowUps: number
  upcomingRenewals: UpcomingRenewal[]
  todaysTasks: DashboardTask[]
}

export interface RenewalListItem {
  publicId: string
  policyPublicId: string
  policyNumber: string
  policyType: string
  premium: number
  expiryDate: string
  renewalDate: string
  daysRemaining: number
  status: string
  priority: string
  currentStage: string
  clientName: string
  clientPublicId?: string | null
  insurerName: string
  assignedUserPublicId?: string | null
  assignedUserName?: string | null
  nextPolicyPublicId?: string | null
  nextPolicyNumber?: string | null
  nextPolicyExpiryDate?: string | null
}

export interface RenewalActivity {
  publicId: string
  activityType: string
  description: string
  createdAtUtc: string
  userName: string | null
}

export interface RenewalDetails {
  publicId: string
  policyPublicId: string
  policyNumber: string
  policyType: string
  policyStatus: string
  premium: number
  sumInsured: number
  startDate: string
  expiryDate: string
  renewalDate: string
  daysRemaining: number
  status: string
  priority: string
  currentStage: string
  clientPublicId: string
  clientName: string
  insurerPublicId: string
  insurerName: string
  assignedUserPublicId: string | null
  assignedUserName: string | null
  lastFollowUpAtUtc: string | null
  nextFollowUpAtUtc: string | null
  notes: string | null
  createdAtUtc: string
  nextPolicyPublicId: string | null
  nextPolicyNumber: string | null
  nextPolicyExpiryDate: string | null
  nextRenewalPublicId: string | null
  activities: RenewalActivity[]
}

export interface PolicyListItem {
  publicId: string
  policyNumber: string
  policyType: string
  status: string
  startDate: string
  expiryDate: string
  daysRemaining: number
  premium: number
  sumInsured: number
  clientName: string
  clientPublicId: string
  insurerName: string
  insurerPublicId?: string | null
  assignedUserPublicId?: string | null
  assignedUserName?: string | null
  previousPolicyPublicId?: string | null
  nextPolicyPublicId?: string | null
}

export interface PolicyActivity {
  publicId: string
  activityType: string
  description: string
  createdAtUtc: string
  userName: string | null
}

export interface PolicyDetails {
  publicId: string
  policyNumber: string
  policyType: string
  status: string
  startDate: string
  expiryDate: string
  daysRemaining: number
  premium: number
  sumInsured: number
  commissionPercentage: number
  commissionAmount: number
  clientPublicId: string
  clientName: string
  insurerPublicId: string
  insurerName: string
  assignedUserPublicId: string | null
  assignedUserName: string | null
  renewalPublicId: string | null
  renewalStatus: string | null
  renewalPriority: string | null
  renewalStage: string | null
  notes: string | null
  previousPolicyPublicId: string | null
  nextPolicyPublicId: string | null
  activities: PolicyActivity[]
}

export interface UpsertPolicyRequest {
  policyNumber?: string
  clientPublicId: string
  insurerPublicId: string
  policyType: string
  startDate: string
  expiryDate: string
  premium: number
  sumInsured: number
  commissionPercentage: number
  assignedUserPublicId?: string
  notes?: string
}

export interface InsurerListItem {
  publicId: string
  name: string
  code: string
  isActive: boolean
}

export interface TeamUser {
  publicId: string
  fullName: string
  email: string
  role: string
  isActive: boolean
}

export interface ClientListItem {
  publicId: string
  clientCode: string
  companyName: string
  clientType: string
  industry: string | null
  email: string
  phone: string
  city: string
  state: string
  isActive: boolean
  assignedUserPublicId: string | null
  assignedUserName: string | null
  policyCount: number
  renewalCount: number
}

export interface ClientDetails {
  publicId: string
  clientCode: string
  companyName: string
  clientType: string
  industry: string | null
  email: string
  phone: string
  alternatePhone: string | null
  addressLine1: string
  addressLine2: string | null
  city: string
  state: string
  postalCode: string
  country: string
  assignedUserPublicId: string | null
  assignedUserName: string | null
  notes: string | null
  isActive: boolean
  policyCount: number
  activePolicyCount: number
  upcomingRenewalCount: number
  totalPremium: number
  createdAtUtc: string
  modifiedAtUtc: string | null
}

export interface ClientPolicy {
  publicId: string
  policyNumber: string
  policyType: string
  status: string
  startDate: string
  expiryDate: string
  premium: number
  sumInsured: number
  insurerName: string | null
  assignedUserPublicId: string | null
  assignedUserName: string | null
}

export interface ClientRenewal {
  publicId: string
  policyPublicId: string
  policyNumber: string
  renewalDate: string
  status: string
  priority: string
  currentStage: string
  assignedUserPublicId: string | null
  assignedUserName: string | null
}

export interface ClientActivity {
  publicId: string
  activityType: string
  description: string
  createdAtUtc: string
  userName: string | null
}

export interface CreateClientRequest {
  companyName: string
  clientType: string
  industry?: string
  email: string
  phone: string
  addressLine1: string
  city: string
  state: string
  postalCode: string
  country: string
  assignedUserPublicId?: string
  notes?: string
}

export interface WorkTaskListItem {
  publicId: string
  title: string
  description: string | null
  dueDateUtc: string
  completedAtUtc: string | null
  priority: string
  status: string
  clientPublicId: string | null
  clientName: string | null
  policyPublicId: string | null
  policyNumber: string | null
  renewalPublicId: string | null
  assignedUserPublicId: string | null
  assignedUserName: string | null
}

export interface WorkTaskDetails {
  publicId: string
  title: string
  description: string | null
  dueDateUtc: string
  completedAtUtc: string | null
  priority: string
  status: string
  clientPublicId: string | null
  clientName: string | null
  policyPublicId: string | null
  policyNumber: string | null
  renewalPublicId: string | null
  renewalPolicyNumber: string | null
  assignedUserPublicId: string | null
  assignedUserName: string | null
  createdAtUtc: string
  modifiedAtUtc: string | null
  createdBy: string | null
}

export interface UpdateWorkTaskRequest {
  title: string
  description?: string
  dueDateUtc: string
  priority: string
  clientPublicId?: string
  policyPublicId?: string
  renewalPublicId?: string
}

export interface DemoResetSummary {
  organizationName: string
  organizationCode: string
  clients: number
  policies: number
  renewals: number
  users: number
  insurers: number
  tasks: number
}

export interface RenewalTask {
  publicId: string
  title: string
  description: string | null
  dueDateUtc: string
  completedAtUtc: string | null
  priority: string
  status: string
  reminderMilestoneDays: number | null
  assignedUserPublicId: string | null
  assignedUserName: string | null
  createdAtUtc: string
  createdBy: string | null
}

export interface OutboundNotification {
  publicId: string
  renewalPublicId: string
  clientPublicId: string | null
  clientName: string | null
  policyNumber: string | null
  organizationName: string | null
  recipientType: string
  channel: string
  recipientName: string
  recipientAddress: string | null
  subject: string
  body: string
  status: string
  reminderMilestoneDays: number | null
  createdAtUtc: string
}

export interface QuickNote {
  activityPublicId: string
  taskPublicId: string | null
  clientPublicId: string | null
  clientName: string | null
  renewalPublicId: string | null
  policyNumber: string | null
  text: string
  followUpTaskCreated: boolean
  createdAtUtc: string
}
