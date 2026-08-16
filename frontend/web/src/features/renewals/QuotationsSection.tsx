import { useState } from 'react'
import { Button } from 'react-bootstrap'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import {
  createQuotation,
  deleteQuotation,
  fetchRenewalQuotations,
  selectQuotation,
  shareQuotation,
  shareQuotationComparison,
  updateQuotation,
} from '../../api/quotations'
import { EmptyState, ErrorBanner, LoadingBlock } from '../../components/feedback/PageFeedback'
import { useToast } from '../../components/feedback/ToastProvider'
import { StatusChip } from '../../components/display/StatusChips'
import { formatDateIn } from '../../lib/format'
import { formatInr } from '../../lib/money'
import type { OutboundNotification, Quotation } from '../../types/api'
import { NotificationPreviewModal } from '../notifications/NotificationPreviewModal'
import { QuotationFormModal } from './QuotationFormModal'

export function QuotationsSection({
  renewalPublicId,
  open,
}: {
  renewalPublicId: string
  open: boolean
}) {
  const queryClient = useQueryClient()
  const { showToast } = useToast()
  const [formQuote, setFormQuote] = useState<Quotation | null | undefined>(undefined)
  const [preview, setPreview] = useState<OutboundNotification | null>(null)

  const quotationsQuery = useQuery({
    queryKey: ['renewal-quotations', renewalPublicId],
    queryFn: () => fetchRenewalQuotations(renewalPublicId),
    enabled: Boolean(renewalPublicId),
  })

  const refresh = async () => {
    await queryClient.invalidateQueries({ queryKey: ['renewal-quotations', renewalPublicId] })
    await queryClient.invalidateQueries({ queryKey: ['renewal', renewalPublicId] })
    await queryClient.invalidateQueries({ queryKey: ['renewal-notifications', renewalPublicId] })
  }

  const createMutation = useMutation({
    mutationFn: (body: Parameters<typeof createQuotation>[1]) => createQuotation(renewalPublicId, body),
    onSuccess: async () => {
      await refresh()
      showToast('Quotation added', 'Logged against this renewal.', 'success')
    },
    onError: (error: Error) => showToast('Could not add quotation', error.message, 'danger'),
  })

  const updateMutation = useMutation({
    mutationFn: ({ publicId, body }: { publicId: string; body: Parameters<typeof updateQuotation>[1] }) =>
      updateQuotation(publicId, body),
    onSuccess: async () => {
      await refresh()
      showToast('Quotation updated', 'The comparison list was refreshed.', 'success')
    },
    onError: (error: Error) => showToast('Could not update quotation', error.message, 'danger'),
  })

  const selectMutation = useMutation({
    mutationFn: selectQuotation,
    onSuccess: async (selected) => {
      await refresh()
      showToast('Quote selected', `${selected.insurerName} is the chosen option for this file.`, 'success')
    },
    onError: (error: Error) => showToast('Could not select quotation', error.message, 'danger'),
  })

  const deleteMutation = useMutation({
    mutationFn: deleteQuotation,
    onSuccess: async () => {
      await refresh()
      showToast('Quotation removed', 'It is no longer on this comparison.', 'info')
    },
    onError: (error: Error) => showToast('Could not delete quotation', error.message, 'danger'),
  })

  const shareOneMutation = useMutation({
    mutationFn: shareQuotation,
    onSuccess: async (notification) => {
      await refresh()
      setPreview(notification)
    },
    onError: (error: Error) => showToast('Could not share quotation', error.message, 'danger'),
  })

  const shareAllMutation = useMutation({
    mutationFn: () => shareQuotationComparison(renewalPublicId),
    onSuccess: async (notification) => {
      await refresh()
      setPreview(notification)
    },
    onError: (error: Error) => showToast('Could not share comparison', error.message, 'danger'),
  })

  const quotations = quotationsQuery.data ?? []
  const formOpen = formQuote !== undefined

  return (
    <section className="content-card mt-4">
      <div className="d-flex justify-content-between align-items-center flex-wrap gap-2 mb-3">
        <div>
          <div className="section-kicker">Compare options</div>
          <h3 className="h6 mb-0">Quotations</h3>
        </div>
        <div className="d-flex flex-wrap gap-2">
          {quotations.length > 1 && (
            <Button
              variant="outline-secondary"
              size="sm"
              onClick={() => shareAllMutation.mutate()}
              disabled={shareAllMutation.isPending}
            >
              Share all for comparison
            </Button>
          )}
          {open && (
            <Button className="btn-gold" size="sm" onClick={() => setFormQuote(null)}>
              + Add quotation
            </Button>
          )}
        </div>
      </div>
      {quotationsQuery.isError && <ErrorBanner>Could not load quotations.</ErrorBanner>}
      {quotationsQuery.isLoading && <LoadingBlock label="Loading quotations…" />}
      {!quotationsQuery.isLoading && quotations.length === 0 && (
        <EmptyState
          icon="bi-clipboard-data"
          title="No quotations logged yet"
          description="Call or email two or three insurers, then type the quotes here to compare and share with the client."
        />
      )}
      {quotations.length > 0 && (
        <div className="table-responsive">
          <table className="table align-middle mb-0">
            <thead>
              <tr>
                <th>Insurer</th>
                <th>Premium</th>
                <th>Sum insured</th>
                <th>Valid until</th>
                <th>Status</th>
                <th />
              </tr>
            </thead>
            <tbody>
              {quotations.map((quote) => (
                <tr
                  key={quote.publicId}
                  className={
                    quote.status === 'Selected'
                      ? 'quote-selected'
                      : quote.isLowestPremium
                        ? 'quote-lowest'
                        : undefined
                  }
                >
                  <td>
                    <strong>{quote.insurerName}</strong>
                    {quote.coverageSummary && (
                      <div className="text-muted small">{quote.coverageSummary}</div>
                    )}
                    {quote.notes && <div className="text-muted small">{quote.notes}</div>}
                  </td>
                  <td className="num">
                    {formatInr(quote.premiumAmount)}
                    {quote.isLowestPremium && quote.status !== 'Rejected' && (
                      <div>
                        <span className="badge text-bg-info">Lowest</span>
                      </div>
                    )}
                  </td>
                  <td className="num">{quote.sumInsured == null ? '—' : formatInr(quote.sumInsured)}</td>
                  <td>{formatDateIn(quote.validUntil)}</td>
                  <td>
                    <StatusChip status={quote.status} />
                  </td>
                  <td>
                    <div className="table-actions">
                      <Button
                        variant="outline-secondary"
                        size="sm"
                        onClick={() => shareOneMutation.mutate(quote.publicId)}
                        disabled={shareOneMutation.isPending}
                      >
                        Share
                      </Button>
                      {open && quote.status !== 'Selected' && (
                        <Button
                          className="btn-gold"
                          size="sm"
                          onClick={() => selectMutation.mutate(quote.publicId)}
                          disabled={selectMutation.isPending}
                        >
                          Select this one
                        </Button>
                      )}
                      {open && (
                        <>
                          <Button variant="outline-secondary" size="sm" onClick={() => setFormQuote(quote)}>
                            Edit
                          </Button>
                          <Button
                            variant="outline-danger"
                            size="sm"
                            onClick={() => deleteMutation.mutate(quote.publicId)}
                            disabled={deleteMutation.isPending}
                          >
                            Delete
                          </Button>
                        </>
                      )}
                    </div>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
      <QuotationFormModal
        show={formOpen}
        quotation={formQuote ?? null}
        onHide={() => setFormQuote(undefined)}
        isPending={createMutation.isPending || updateMutation.isPending}
        onSubmit={(body) =>
          formQuote
            ? updateMutation.mutateAsync({ publicId: formQuote.publicId, body })
            : createMutation.mutateAsync(body)
        }
      />
      <NotificationPreviewModal notification={preview} onHide={() => setPreview(null)} />
    </section>
  )
}
