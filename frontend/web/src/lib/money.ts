export function formatInr(amount: number) {
  return new Intl.NumberFormat('en-IN', {
    style: 'currency',
    currency: 'INR',
    minimumFractionDigits: 2,
    maximumFractionDigits: 2,
  }).format(amount)
}

export function commissionAmount(premium: number, commissionPercentage: number) {
  if (!Number.isFinite(premium) || !Number.isFinite(commissionPercentage)) {
    return 0
  }

  const amount = (premium * commissionPercentage) / 100
  return Math.round((amount + Number.EPSILON) * 100) / 100
}
