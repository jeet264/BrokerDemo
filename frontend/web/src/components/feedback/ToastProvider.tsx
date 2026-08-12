import { createContext, useCallback, useContext, useMemo, useState } from 'react'
import { Toast, ToastContainer } from 'react-bootstrap'
import type { ReactNode } from 'react'

interface ToastMessage {
  id: number
  title: string
  body: string
  variant: 'success' | 'danger' | 'info'
}

interface ToastContextValue {
  showToast: (title: string, body: string, variant?: ToastMessage['variant']) => void
}

const ToastContext = createContext<ToastContextValue | null>(null)

export function ToastProvider({ children }: { children: ReactNode }) {
  const [toasts, setToasts] = useState<ToastMessage[]>([])

  const showToast = useCallback((title: string, body: string, variant: ToastMessage['variant'] = 'info') => {
    const id = Date.now()
    setToasts((current) => [...current, { id, title, body, variant }])
  }, [])

  const value = useMemo(() => ({ showToast }), [showToast])

  return (
    <ToastContext.Provider value={value}>
      {children}
      <ToastContainer position="top-end" className="p-3" style={{ zIndex: 1080 }}>
        {toasts.map((toast) => (
          <Toast
            key={toast.id}
            bg={toast.variant}
            onClose={() => setToasts((current) => current.filter((item) => item.id !== toast.id))}
            delay={4000}
            autohide
          >
            <Toast.Header>
              <strong className="me-auto">{toast.title}</strong>
            </Toast.Header>
            <Toast.Body className={toast.variant === 'info' ? '' : 'text-white'}>{toast.body}</Toast.Body>
          </Toast>
        ))}
      </ToastContainer>
    </ToastContext.Provider>
  )
}

export function useToast() {
  const context = useContext(ToastContext)
  if (!context) {
    throw new Error('useToast must be used within ToastProvider')
  }
  return context
}
