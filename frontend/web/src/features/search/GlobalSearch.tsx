import { useEffect, useId, useRef, useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { fetchSearch } from '../../api/search'
import type { SearchHit } from '../../types/api'

const DEBOUNCE_MS = 300
const MIN_LENGTH = 2
const EMPTY_HITS: SearchHit[] = []

function hrefFor(hit: SearchHit) {
  return hit.type === 'Client' ? `/clients/${hit.publicId}` : `/policies/${hit.publicId}`
}

function matchedLabel(matchedOn: string) {
  if (matchedOn === 'PolicyNumber') {
    return 'Policy number'
  }
  if (matchedOn === 'VehicleNumber') {
    return 'Vehicle number'
  }
  return matchedOn
}

/**
 * Header search: one box for client name, phone, policy number, and vehicle number.
 */
export function GlobalSearch() {
  const navigate = useNavigate()
  const listId = useId()
  const inputId = useId()
  const rootRef = useRef<HTMLDivElement | null>(null)
  const [term, setTerm] = useState('')
  const [debounced, setDebounced] = useState('')
  const [open, setOpen] = useState(false)
  const [activeIndex, setActiveIndex] = useState(0)

  useEffect(() => {
    const handle = window.setTimeout(() => setDebounced(term.trim()), DEBOUNCE_MS)
    return () => window.clearTimeout(handle)
  }, [term])

  useEffect(() => {
    function onPointerDown(event: MouseEvent) {
      if (!rootRef.current?.contains(event.target as Node)) {
        setOpen(false)
      }
    }

    document.addEventListener('mousedown', onPointerDown)
    return () => document.removeEventListener('mousedown', onPointerDown)
  }, [])

  const enabled = debounced.length >= MIN_LENGTH
  const searchQuery = useQuery({
    queryKey: ['global-search', debounced],
    queryFn: () => fetchSearch(debounced),
    enabled,
    staleTime: 15_000,
  })

  const items = searchQuery.data?.items ?? EMPTY_HITS
  const clients = items.filter((item) => item.type === 'Client')
  const policies = items.filter((item) => item.type === 'Policy')
  const flat = [...clients, ...policies]

  useEffect(() => {
    setActiveIndex(0)
  }, [debounced])

  const showPanel = open && term.trim().length >= MIN_LENGTH
  const showEmpty = showPanel && enabled && !searchQuery.isFetching && items.length === 0
  const showResults = showPanel && flat.length > 0

  const go = (hit: SearchHit) => {
    setOpen(false)
    setTerm('')
    setDebounced('')
    navigate(hrefFor(hit))
  }

  return (
    <div className="global-search" ref={rootRef}>
      <label className="visually-hidden" htmlFor={inputId}>
        Search clients and policies
      </label>
      <i className="bi bi-search global-search-icon" aria-hidden />
      <input
        id={inputId}
        className="global-search-input"
        type="search"
        role="combobox"
        placeholder="Search client, phone, policy, or vehicle…"
        autoComplete="off"
        value={term}
        aria-expanded={showPanel}
        aria-controls={listId}
        aria-activedescendant={showResults ? `${listId}-opt-${activeIndex}` : undefined}
        onChange={(event) => {
          setTerm(event.target.value)
          setOpen(true)
        }}
        onFocus={() => setOpen(true)}
        onKeyDown={(event) => {
          if (!showPanel) {
            return
          }

          if (event.key === 'Escape') {
            event.preventDefault()
            setOpen(false)
            return
          }

          if (!showResults) {
            return
          }

          if (event.key === 'ArrowDown') {
            event.preventDefault()
            setActiveIndex((current) => Math.min(current + 1, flat.length - 1))
            return
          }

          if (event.key === 'ArrowUp') {
            event.preventDefault()
            setActiveIndex((current) => Math.max(current - 1, 0))
            return
          }

          if (event.key === 'Enter') {
            event.preventDefault()
            const hit = flat[activeIndex]
            if (hit) {
              go(hit)
            }
          }
        }}
      />
      {showPanel && (
        <div className="global-search-panel" id={listId} role="listbox">
          {searchQuery.isFetching && items.length === 0 && (
            <div className="global-search-empty">Searching…</div>
          )}
          {showEmpty && (
            <div className="global-search-empty">No matches for '{debounced}'</div>
          )}
          {showResults && (
            <>
              {clients.length > 0 && (
                <section>
                  <div className="global-search-group">Clients</div>
                  {clients.map((hit) => {
                    const index = flat.indexOf(hit)
                    return (
                      <SearchOption
                        key={`client-${hit.publicId}`}
                        id={`${listId}-opt-${index}`}
                        hit={hit}
                        active={index === activeIndex}
                        onSelect={() => go(hit)}
                      />
                    )
                  })}
                </section>
              )}
              {policies.length > 0 && (
                <section>
                  <div className="global-search-group">Policies</div>
                  {policies.map((hit) => {
                    const index = flat.indexOf(hit)
                    return (
                      <SearchOption
                        key={`policy-${hit.publicId}`}
                        id={`${listId}-opt-${index}`}
                        hit={hit}
                        active={index === activeIndex}
                        onSelect={() => go(hit)}
                      />
                    )
                  })}
                </section>
              )}
            </>
          )}
        </div>
      )}
    </div>
  )
}

function SearchOption({
  id,
  hit,
  active,
  onSelect,
}: {
  id: string
  hit: SearchHit
  active: boolean
  onSelect: () => void
}) {
  return (
    <button
      type="button"
      id={id}
      role="option"
      aria-selected={active}
      className={`global-search-option${active ? ' is-active' : ''}`}
      onMouseDown={(event) => event.preventDefault()}
      onClick={onSelect}
    >
      <span className="global-search-option-title">{hit.title}</span>
      <span className="global-search-option-sub">
        {hit.subtitle}
        {hit.subtitle ? ' · ' : ''}
        {matchedLabel(hit.matchedOn)}
      </span>
    </button>
  )
}
