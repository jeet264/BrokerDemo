import { ConfirmActionModal } from './ConfirmActionModal'
import { useMarkLost } from './useDeskMutations'

export function MarkLostModal({
  show,
  publicId,
  policyNumber,
  onHide,
}: {
  show: boolean
  publicId: string
  policyNumber: string
  onHide: () => void
}) {
  const markLost = useMarkLost()

  return (
    <ConfirmActionModal
      show={show}
      title="Mark lost"
      body={
        <p className="mb-0">
          Cancel <strong>{policyNumber}</strong>? The policy will be marked Cancelled and no new term will be created.
        </p>
      }
      confirmLabel="Mark lost"
      pending={markLost.isPending}
      onHide={onHide}
      onConfirm={() => markLost.mutate({ publicId }, { onSuccess: onHide })}
    />
  )
}
