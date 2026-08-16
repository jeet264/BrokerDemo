import { useEffect, useId, useRef, useState } from 'react'
import { Form, Spinner } from 'react-bootstrap'

export interface PickerOption {
  id: string
  label: string
  detail?: string
  clientId?: string
}

/**
 * Compact typeahead for linking a note. Not a full dropdown of the book — type a few letters.
 */
export function SearchablePicker({
  label,
  placeholder,
  value,
  onChange,
  search,
  disabled,
}: {
  label: string
  placeholder: string
  value: PickerOption | null
  onChange: (value: PickerOption | null) => void
  search: (term: string) => Promise<PickerOption[]>
  disabled?: boolean
}) {
  const listId = useId()
  const [term, setTerm] = useState(value?.label ?? '')
  const [open, setOpen] = useState(false)
  const [loading, setLoading] = useState(false)
  const [options, setOptions] = useState<PickerOption[]>([])
  const blurTimer = useRef(0)

  useEffect(() => {
    if (value) {
      setTerm(value.label)
    }
  }, [value])

  useEffect(() => {
    if (!open || disabled) {
      return
    }

    let cancelled = false
    setLoading(true)
    const handle = window.setTimeout(() => {
      void search(term.trim())
        .then((items) => {
          if (!cancelled) {
            setOptions(items)
          }
        })
        .catch(() => {
          if (!cancelled) {
            setOptions([])
          }
        })
        .finally(() => {
          if (!cancelled) {
            setLoading(false)
          }
        })
    }, 180)

    return () => {
      cancelled = true
      window.clearTimeout(handle)
    }
  }, [open, term, search, disabled])

  return (
    <Form.Group className="searchable-picker mb-3">
      <Form.Label>{label}</Form.Label>
      {value ? (
        <div className="searchable-picker-chip">
          <span>
            {value.label}
            {value.detail ? <span className="text-muted"> · {value.detail}</span> : null}
          </span>
          <button
            type="button"
            className="searchable-picker-clear"
            disabled={disabled}
            onClick={() => {
              onChange(null)
              setTerm('')
              setOpen(false)
            }}
          >
            Clear
          </button>
        </div>
      ) : (
        <div className="position-relative">
          <Form.Control
            role="combobox"
            aria-expanded={open}
            aria-controls={listId}
            autoComplete="off"
            placeholder={placeholder}
            disabled={disabled}
            value={term}
            onChange={(event) => {
              setTerm(event.target.value)
              setOpen(true)
            }}
            onFocus={() => setOpen(true)}
            onBlur={() => {
              window.clearTimeout(blurTimer.current)
              blurTimer.current = window.setTimeout(() => setOpen(false), 120)
            }}
          />
          {open && (
            <ul id={listId} className="searchable-picker-list" role="listbox">
              {loading && (
                <li className="searchable-picker-empty">
                  <Spinner animation="border" size="sm" /> Searching…
                </li>
              )}
              {!loading && options.length === 0 && (
                <li className="searchable-picker-empty">No matches. Leave blank to save unlinked.</li>
              )}
              {!loading &&
                options.map((option) => (
                  <li key={option.id}>
                    <button
                      type="button"
                      role="option"
                      onMouseDown={(event) => event.preventDefault()}
                      onClick={() => {
                        onChange(option)
                        setTerm(option.label)
                        setOpen(false)
                      }}
                    >
                      <span>{option.label}</span>
                      {option.detail ? <small>{option.detail}</small> : null}
                    </button>
                  </li>
                ))}
            </ul>
          )}
        </div>
      )}
    </Form.Group>
  )
}
