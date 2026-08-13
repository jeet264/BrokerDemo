import { useState } from 'react'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { Button, Modal } from 'react-bootstrap'
import { Navigate } from 'react-router-dom'
import { fetchSystemStatus, resetDemoData } from '../../api/system'
import { useToast } from '../../components/feedback/ToastProvider'
import { isDemoResetUiEnabled } from '../../lib/demoMode'

const CONFIRM_COPY = 'This will erase all current data and reload sample data. Continue?'

export function SettingsPage() {
  const queryClient = useQueryClient()
  const { showToast } = useToast()
  const [confirming, setConfirming] = useState(false)

  const statusQuery = useQuery({
    queryKey: ['system-status'],
    queryFn: fetchSystemStatus,
    enabled: isDemoResetUiEnabled,
  })

  const resetMutation = useMutation({
    mutationFn: resetDemoData,
    onSuccess: async (summary) => {
      setConfirming(false)
      await queryClient.invalidateQueries()
      showToast(
        'Demo data reset',
        `${summary.clients} clients, ${summary.policies} policies, ${summary.renewals} renewals reloaded.`,
        'success',
      )
    },
    onError: (error: Error) => {
      showToast('Could not reset demo data', error.message, 'danger')
    },
  })

  if (!isDemoResetUiEnabled) {
    return <Navigate to="/dashboard" replace />
  }

  const apiAllowsReset = statusQuery.data?.demoResetEnabled === true

  return (
    <div>
      <div className="page-heading">
        <h2>Settings</h2>
        <p>Demo-only workspace tools. This page is hidden unless the frontend demo-reset flag is on.</p>
      </div>

      <section className="content-card">
        <h3 className="h6 text-uppercase text-muted">Reset demo data</h3>
        <p className="mb-3">
          Erases Apex clients, policies, renewals, tasks, and activity, then reloads the sample book. Users and the
          organisation stay in place so you remain signed in. Broker Admin only.
        </p>
        {!apiAllowsReset && !statusQuery.isLoading && (
          <div className="alert alert-warning">
            Reset is not available on this API. It is Development-only and must have demo reset enabled.
          </div>
        )}
        <Button
          className="btn-gold"
          disabled={!apiAllowsReset || resetMutation.isPending}
          onClick={() => setConfirming(true)}
        >
          {resetMutation.isPending ? 'Resetting…' : 'Reset Demo Data'}
        </Button>
      </section>

      <Modal show={confirming} onHide={() => !resetMutation.isPending && setConfirming(false)} centered>
        <Modal.Header closeButton={!resetMutation.isPending}>
          <Modal.Title>Reset Demo Data</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <p className="mb-0">{CONFIRM_COPY}</p>
        </Modal.Body>
        <Modal.Footer>
          <Button variant="outline-secondary" disabled={resetMutation.isPending} onClick={() => setConfirming(false)}>
            Cancel
          </Button>
          <Button
            className="btn-gold"
            disabled={resetMutation.isPending}
            onClick={() => resetMutation.mutate()}
          >
            {resetMutation.isPending ? 'Resetting…' : 'Continue'}
          </Button>
        </Modal.Footer>
      </Modal>
    </div>
  )
}
