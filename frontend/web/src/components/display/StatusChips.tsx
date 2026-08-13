import { humanizeEnum, priorityChipClass, statusChipClass } from '../../lib/format'

export function PriorityChip({ priority }: { priority: string }) {
  return <span className={priorityChipClass(priority)}>{priority}</span>
}

export function StatusChip({ status }: { status: string }) {
  return <span className={statusChipClass(status)}>{humanizeEnum(status)}</span>
}
