import { useEffect, useState, type ReactNode } from 'react'
import { ChevronLeft, ChevronRight, Loader, TriangleAlert, X } from 'lucide-react'

// ---------- Panel: the boxed section with a title bar used everywhere ----------

export function Panel(props: { title: ReactNode; icon?: ReactNode; actions?: ReactNode; children: ReactNode }) {
  return (
    <section className="panel">
      <div className="panel-title">
        <h2>
          {props.icon}
          {props.title}
        </h2>
        {props.actions && <div className="panel-actions">{props.actions}</div>}
      </div>
      <div className="panel-body">{props.children}</div>
    </section>
  )
}

// ---------- Modal ----------

export function Modal(props: { title: string; onClose: () => void; children: ReactNode; wide?: boolean }) {
  // Close with the Escape key.
  useEffect(() => {
    function onKey(e: KeyboardEvent) {
      if (e.key === 'Escape') props.onClose()
    }
    window.addEventListener('keydown', onKey)
    return () => window.removeEventListener('keydown', onKey)
  }, [props])

  return (
    <div className="modal-overlay" onMouseDown={props.onClose}>
      <div
        className={`modal ${props.wide ? 'modal-wide' : ''}`}
        role="dialog"
        aria-modal="true"
        aria-label={props.title}
        onMouseDown={(e) => e.stopPropagation()}
      >
        <div className="modal-header">
          <h2>{props.title}</h2>
          <button className="icon-button" onClick={props.onClose} aria-label="Close">
            <X size={18} />
          </button>
        </div>
        <div className="modal-body">{props.children}</div>
      </div>
    </div>
  )
}

// Confirmation dialog for deletes.
export function ConfirmModal(props: {
  title: string
  message: ReactNode
  confirmLabel: string
  onConfirm: () => Promise<void>
  onClose: () => void
}) {
  const [busy, setBusy] = useState(false)

  async function confirm() {
    setBusy(true)
    try {
      await props.onConfirm()
    } finally {
      setBusy(false)
    }
  }

  return (
    <Modal title={props.title} onClose={props.onClose}>
      <div className="confirm">
        <TriangleAlert size={36} className="confirm-icon" />
        <div>{props.message}</div>
      </div>
      <div className="form-actions">
        <button className="btn" onClick={props.onClose} disabled={busy}>
          Cancel
        </button>
        <button className="btn btn-danger" onClick={confirm} disabled={busy}>
          {busy ? 'Deleting…' : props.confirmLabel}
        </button>
      </div>
    </Modal>
  )
}

// ---------- Loading and errors ----------

export function Spinner({ text = 'Loading…' }: { text?: string }) {
  return (
    <div className="spinner">
      <Loader size={22} className="spin" /> {text}
    </div>
  )
}

export function ErrorBox({ message }: { message: string }) {
  return (
    <div className="error-box">
      <TriangleAlert size={18} /> {message}
    </div>
  )
}

// ---------- Pagination ----------

export function Pagination(props: { page: number; totalPages: number; onChange: (page: number) => void }) {
  if (props.totalPages <= 1) return null
  const pages = Array.from({ length: props.totalPages }, (_, i) => i + 1)

  return (
    <nav className="pagination" aria-label="Pages">
      <button className="btn btn-small" disabled={props.page <= 1} onClick={() => props.onChange(props.page - 1)}>
        <ChevronLeft size={14} /> Prev
      </button>
      {pages.map((p) => (
        <button
          key={p}
          className={`btn btn-small ${p === props.page ? 'btn-active' : ''}`}
          onClick={() => props.onChange(p)}
        >
          {p}
        </button>
      ))}
      <button
        className="btn btn-small"
        disabled={props.page >= props.totalPages}
        onClick={() => props.onChange(props.page + 1)}
      >
        Next <ChevronRight size={14} />
      </button>
    </nav>
  )
}

// ---------- Small helpers ----------

export function ChampionImage(props: { src: string; alt: string; className?: string }) {
  return (
    <img
      src={props.src}
      alt={props.alt}
      className={props.className}
      onError={(e) => {
        e.currentTarget.onerror = null
        e.currentTarget.src = '/images/champion-placeholder.svg'
      }}
    />
  )
}

// ---------- Form helpers used together with useValidation ----------

// Label with a red * for required fields and an optional hint on the right.
export function FieldLabel(props: { field: string; required?: boolean; hint?: ReactNode; children: ReactNode }) {
  return (
    <label htmlFor={`field-${props.field}`} className="field-label">
      <span>
        {props.children}
        {props.required && (
          <span className="required-mark" title="Required">
            {' '}
            *
          </span>
        )}
      </span>
      {props.hint && <span className="field-hint">{props.hint}</span>}
    </label>
  )
}

// "12 / 80" counter that turns red when the text is too short or too long.
export function CharCount(props: { value: string; min: number; max: number }) {
  const length = props.value.trim().length
  const bad = length > 0 && (length < props.min || length > props.max)
  return (
    <span className={bad ? 'char-count char-count-bad' : 'char-count'}>
      {length} / {props.max}
    </span>
  )
}

export function FieldMessage({ message }: { message?: string }) {
  if (!message) return null
  return (
    <div className="field-error" role="alert">
      <TriangleAlert size={12} /> {message}
    </div>
  )
}

// Box at the top of a form listing everything that still needs fixing.
export function FormSummary(props: { messages: string[]; serverMessage?: string }) {
  if (props.messages.length > 0) {
    return (
      <div className="form-error form-summary">
        <TriangleAlert size={18} />
        <div>
          <strong>Please fix the following:</strong>
          <ul>
            {props.messages.map((message) => (
              <li key={message}>{message}</li>
            ))}
          </ul>
        </div>
      </div>
    )
  }
  if (props.serverMessage) {
    return (
      <div className="form-error">
        <TriangleAlert size={18} /> {props.serverMessage}
      </div>
    )
  }
  return null
}

export function RequiredNote() {
  return (
    <p className="required-note">
      Fields marked with <span className="required-mark">*</span> are required.
    </p>
  )
}

export function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString(undefined, { year: 'numeric', month: 'short', day: 'numeric' })
}
