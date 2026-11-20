import { createContext, useCallback, useContext, useState, type ReactNode } from 'react'
import { CircleAlert, CircleCheck, X } from 'lucide-react'

// Small notifications in the corner, used instead of browser alert() for user feedback.

type Toast = { id: number; type: 'success' | 'error'; message: string }
type ToastApi = { success: (message: string) => void; error: (message: string) => void }

const ToastContext = createContext<ToastApi | null>(null)
let nextId = 1

export function ToastProvider({ children }: { children: ReactNode }) {
  const [toasts, setToasts] = useState<Toast[]>([])

  const remove = useCallback((id: number) => {
    setToasts((list) => list.filter((t) => t.id !== id))
  }, [])

  const show = useCallback(
    (type: Toast['type'], message: string) => {
      const id = nextId++
      setToasts((list) => [...list, { id, type, message }])
      setTimeout(() => remove(id), 4000)
    },
    [remove],
  )

  const toastApi: ToastApi = {
    success: (message) => show('success', message),
    error: (message) => show('error', message),
  }

  return (
    <ToastContext.Provider value={toastApi}>
      {children}
      <div className="toasts" role="status" aria-live="polite">
        {toasts.map((t) => (
          <div key={t.id} className={`toast toast-${t.type}`}>
            {t.type === 'success' ? <CircleCheck size={18} /> : <CircleAlert size={18} />}
            <span>{t.message}</span>
            <button className="toast-close" onClick={() => remove(t.id)} aria-label="Close">
              <X size={14} />
            </button>
          </div>
        ))}
      </div>
    </ToastContext.Provider>
  )
}

export function useToast(): ToastApi {
  const context = useContext(ToastContext)
  if (!context) throw new Error('useToast must be used inside ToastProvider')
  return context
}
