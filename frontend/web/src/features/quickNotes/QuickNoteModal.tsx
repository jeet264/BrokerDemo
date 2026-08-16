import { useCallback, useEffect, useRef, useState } from 'react'
import { Button, Form, Modal } from 'react-bootstrap'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { fetchClient, fetchClients } from '../../api/clients'
import { ApiRequestError } from '../../api/client'
import { createQuickNote } from '../../api/quickNotes'
import { fetchRenewal, fetchRenewals } from '../../api/renewals'
import { useToast } from '../../components/feedback/ToastProvider'
import { deskKeys } from '../actions/deskCache'
import { istDateToUtc, tomorrowIsoDate } from '../renewals/renewalDisplay'
import { SearchablePicker, type PickerOption } from './SearchablePicker'

type SpeechRec = {
  lang: string
  interimResults: boolean
  continuous: boolean
  start: () => void
  stop: () => void
  onresult: ((event: { results: ArrayLike<ArrayLike<{ transcript: string }>> }) => void) | null
  onerror: (() => void) | null
  onend: (() => void) | null
}

function speechCtor(): (new () => SpeechRec) | null {
  const browser = window as Window & {
    SpeechRecognition?: new () => SpeechRec
    webkitSpeechRecognition?: new () => SpeechRec
  }
  return browser.SpeechRecognition ?? browser.webkitSpeechRecognition ?? null
}

/**
 * Between-calls capture. Intentionally no NLP: follow-up tasks are a checkbox, not inferred
 * from the wording. A later version can suggest that checkbox (same family as AI document scanning).
 */
export function QuickNoteModal({
  show,
  onHide,
  contextClientPublicId,
  contextRenewalPublicId,
}: {
  show: boolean
  onHide: () => void
  contextClientPublicId?: string
  contextRenewalPublicId?: string
}) {
  const { showToast } = useToast()
  const queryClient = useQueryClient()
  const textRef = useRef<HTMLTextAreaElement | null>(null)
  const [text, setText] = useState('')
  const [client, setClient] = useState<PickerOption | null>(null)
  const [renewal, setRenewal] = useState<PickerOption | null>(null)
  const [createTask, setCreateTask] = useState(false)
  const [dueDate, setDueDate] = useState(tomorrowIsoDate())
  const [listening, setListening] = useState(false)
  const recognitionRef = useRef<SpeechRec | null>(null)
  const canDictate = Boolean(speechCtor())

  const reset = useCallback(() => {
    setText('')
    setClient(null)
    setRenewal(null)
    setCreateTask(false)
    setDueDate(tomorrowIsoDate())
    setListening(false)
    recognitionRef.current?.stop()
    recognitionRef.current = null
  }, [])

  useEffect(() => {
    if (!show) {
      return
    }

    reset()
    let cancelled = false

    async function hydrateContext() {
      try {
        if (contextRenewalPublicId) {
          const file = await fetchRenewal(contextRenewalPublicId)
          if (cancelled) {
            return
          }
          setRenewal({
            id: file.publicId,
            label: `${file.clientName} · ${file.policyNumber}`,
            detail: file.status,
          })
          setClient({ id: file.clientPublicId, label: file.clientName })
          return
        }

        if (contextClientPublicId) {
          const record = await fetchClient(contextClientPublicId)
          if (!cancelled) {
            setClient({ id: record.publicId, label: record.companyName })
          }
        }
      } catch {
        // Stay unlinked if the current page is not a readable file.
      }
    }

    void hydrateContext()
    return () => {
      cancelled = true
    }
  }, [show, contextClientPublicId, contextRenewalPublicId, reset])

  const searchClients = useCallback(async (term: string) => {
    const page = await fetchClients({ search: term || undefined, pageSize: 8, isActive: 'true' })
    return page.items.map((item) => ({
      id: item.publicId,
      label: item.companyName,
      detail: item.clientCode,
    }))
  }, [])

  const searchRenewals = useCallback(
    async (term: string) => {
      const page = await fetchRenewals({
        search: term || undefined,
        pageSize: 8,
        clientPublicId: client?.id,
      })
      return page.items.map((item) => ({
        id: item.publicId,
        label: `${item.clientName} · ${item.policyNumber}`,
        detail: item.status,
        clientId: item.clientPublicId ?? undefined,
      }))
    },
    [client?.id],
  )

  const mutation = useMutation({
    mutationFn: () =>
      createQuickNote({
        text: text.trim(),
        clientPublicId: client?.id,
        renewalPublicId: renewal?.id,
        createFollowUpTask: createTask || undefined,
        taskDueDateUtc: createTask && dueDate ? istDateToUtc(dueDate) : undefined,
      }),
    onSuccess: (saved) => {
      void queryClient.invalidateQueries({ queryKey: deskKeys.tasks })
      void queryClient.invalidateQueries({ queryKey: deskKeys.dashboard })
      void queryClient.invalidateQueries({ queryKey: deskKeys.clients })
      void queryClient.invalidateQueries({ queryKey: ['client-activities'] })
      void queryClient.invalidateQueries({ queryKey: ['renewal'] })
      void queryClient.invalidateQueries({ queryKey: ['renewal-tasks'] })
      if (saved.clientPublicId) {
        void queryClient.invalidateQueries({ queryKey: ['client', saved.clientPublicId] })
      }
      const where = saved.clientName
        ? ` on ${saved.clientName}`
        : saved.policyNumber
          ? ` on ${saved.policyNumber}`
          : ''
      const taskBit = saved.followUpTaskCreated ? ' Follow-up task created.' : ''
      showToast('Note saved', `Logged${where}.${taskBit}`.replace('..', '.'), 'success')
      reset()
      onHide()
    },
    onError: (error: Error) => {
      const message = error instanceof ApiRequestError ? error.message : error.message
      showToast('Could not save note', message, 'danger')
    },
  })

  const toggleDictation = () => {
    if (listening) {
      recognitionRef.current?.stop()
      setListening(false)
      return
    }

    const Ctor = speechCtor()
    if (!Ctor) {
      return
    }

    const recognition = new Ctor()
    recognition.lang = 'en-IN'
    recognition.interimResults = false
    recognition.continuous = false
    recognition.onresult = (event) => {
      const spoken = event.results[0]?.[0]?.transcript?.trim()
      if (spoken) {
        setText((current) => (current.trim() ? `${current.trim()} ${spoken}` : spoken))
      }
    }
    recognition.onerror = () => setListening(false)
    recognition.onend = () => setListening(false)
    recognitionRef.current = recognition
    recognition.start()
    setListening(true)
  }

  const save = () => {
    if (!text.trim() || mutation.isPending) {
      return
    }
    mutation.mutate()
  }

  return (
    <Modal
      show={show}
      onHide={() => {
        reset()
        onHide()
      }}
      centered
      onEntered={() => textRef.current?.focus()}
    >
      <Form
        onSubmit={(event) => {
          event.preventDefault()
          save()
        }}
      >
        <Modal.Header closeButton>
          <Modal.Title>Quick note</Modal.Title>
        </Modal.Header>
        <Modal.Body>
          <p className="text-muted small mb-3">
            Jot what just happened. Client and renewal are optional — save first, link later if you need to.
          </p>
          <Form.Group className="mb-3">
            <div className="d-flex justify-content-between align-items-center mb-1">
              <Form.Label className="mb-0" htmlFor="quick-note-text">
                Note
              </Form.Label>
              {canDictate && (
                <Button
                  type="button"
                  size="sm"
                  variant={listening ? 'danger' : 'outline-secondary'}
                  onClick={toggleDictation}
                  aria-pressed={listening}
                >
                  <i className={`bi ${listening ? 'bi-mic-fill' : 'bi-mic'}`} /> {listening ? 'Listening' : 'Dictate'}
                </Button>
              )}
            </div>
            <Form.Control
              as="textarea"
              rows={4}
              id="quick-note-text"
              ref={textRef}
              value={text}
              maxLength={2000}
              placeholder="Called Alpha — send fire quote tomorrow…"
              onChange={(event) => setText(event.target.value)}
              onKeyDown={(event) => {
                if ((event.metaKey || event.ctrlKey) && event.key === 'Enter') {
                  event.preventDefault()
                  save()
                }
              }}
              required
            />
            <Form.Text>Ctrl/⌘ + Enter saves. We do not auto-detect follow-ups from this text.</Form.Text>
          </Form.Group>
          <SearchablePicker
            label="Client (optional)"
            placeholder="Type a company name"
            value={client}
            onChange={(next) => {
              setClient(next)
              if (!next || (renewal?.clientId && next.id !== renewal.clientId)) {
                setRenewal(null)
              }
            }}
            search={searchClients}
          />
          <SearchablePicker
            label="Renewal (optional)"
            placeholder="Type a client or policy number"
            value={renewal}
            onChange={(next) => {
              setRenewal(next)
              if (next?.clientId) {
                const company = next.label.split(' · ')[0]
                setClient({ id: next.clientId, label: company || next.label })
              }
            }}
            search={searchRenewals}
          />
          <Form.Check
            className="mb-2"
            type="checkbox"
            id="quick-note-follow-up"
            label="Also create a follow-up task"
            checked={createTask}
            onChange={(event) => setCreateTask(event.target.checked)}
          />
          {createTask && (
            <Form.Group>
              <Form.Label htmlFor="quick-note-due">Due date (optional)</Form.Label>
              <Form.Control
                id="quick-note-due"
                type="date"
                value={dueDate}
                onChange={(event) => setDueDate(event.target.value)}
              />
            </Form.Group>
          )}
        </Modal.Body>
        <Modal.Footer>
          <Button
            variant="outline-secondary"
            onClick={() => {
              reset()
              onHide()
            }}
          >
            Cancel
          </Button>
          <Button className="btn-gold" type="submit" disabled={!text.trim() || mutation.isPending}>
            {mutation.isPending ? 'Saving…' : 'Save'}
          </Button>
        </Modal.Footer>
      </Form>
    </Modal>
  )
}
