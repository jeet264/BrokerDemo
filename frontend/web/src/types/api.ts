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
  insurerName: string
  nextPolicyPublicId?: string | null
  nextPolicyNumber?: string | null
  nextPolicyExpiryDate?: string | null
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
  insurerName: string
  previousPolicyPublicId?: string | null
  nextPolicyPublicId?: string | null
}
