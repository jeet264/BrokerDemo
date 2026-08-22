import { useEffect, useRef, useState } from 'react'

export interface CustomSelectOption<T extends string = string> {
  value: T
  label: string
}

export function CustomSelect<T extends string = string>({
  value,
  options,
  onChange,
  ariaLabel,
  placeholder = 'Select…',
  className = '',
}: {
  value: T
  options: CustomSelectOption<T>[]
  onChange: (val: T) => void
  ariaLabel?: string
  placeholder?: string
  className?: string
}) {
  const [isOpen, setIsOpen] = useState(false)
  const containerRef = useRef<HTMLDivElement>(null)

  const selectedOption = options.find((opt) => opt.value === value)
  const displayLabel = selectedOption ? selectedOption.label : placeholder

  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
        setIsOpen(false)
      }
    }
    document.addEventListener('mousedown', handleClickOutside)
    return () => document.removeEventListener('mousedown', handleClickOutside)
  }, [])

  return (
    <div
      ref={containerRef}
      className={`custom-select-container ${className}`}
      aria-label={ariaLabel}
    >
      <button
        type="button"
        className={`custom-select-trigger ${isOpen ? 'open' : ''}`}
        onClick={() => setIsOpen(!isOpen)}
        aria-expanded={isOpen}
      >
        <span className="custom-select-value-text">{displayLabel}</span>
        <i className={`bi bi-chevron-down custom-select-chevron ${isOpen ? 'rotate' : ''}`} />
      </button>

      {isOpen && (
        <div className="custom-select-menu">
          {options.map((option) => (
            <button
              key={option.value}
              type="button"
              className={`custom-select-option ${value === option.value ? 'active' : ''}`}
              onClick={() => {
                onChange(option.value)
                setIsOpen(false)
              }}
            >
              <span>{option.label}</span>
              {value === option.value && <i className="bi bi-check2 check-icon" />}
            </button>
          ))}
        </div>
      )}
    </div>
  )
}
