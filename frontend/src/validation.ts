import { useState } from 'react'
import { ApiError } from './api'

// ---------- Rules ----------
// Each rule returns an error message, or undefined when the value is fine.
// The limits mirror the [Required]/[StringLength] attributes on the backend DTOs.

export function checkText(value: string, label: string, min: number, max: number): string | undefined {
  const length = value.trim().length
  if (length === 0) return `${label} is required.`
  if (length < min) return `${label} must be at least ${min} characters (${min - length} more needed).`
  if (length > max) return `${label} can be at most ${max} characters.`
  return undefined
}

export function checkEmail(value: string): string | undefined {
  if (value.trim() === '') return 'Email is required.'
  if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value.trim())) return 'Enter a valid email address, e.g. name@example.com.'
  return undefined
}

// ---------- Hook ----------
// Keeps track of which fields the user has left (touched) and whether they tried to submit.
// Errors are only shown after that, so an empty form doesn't start out red.
// Server errors (e.g. 409 "username taken" or 422 rules) are shown on the matching field.

export type Errors = Record<string, string | undefined>

export function useValidation(clientErrors: Errors) {
  const [touched, setTouched] = useState<Record<string, boolean>>({})
  const [submitted, setSubmitted] = useState(false)
  const [serverErrors, setServerErrors] = useState<Record<string, string>>({})
  const [serverMessage, setServerMessage] = useState('')

  function errorFor(field: string): string | undefined {
    const client = submitted || touched[field] ? clientErrors[field] : undefined
    return client ?? serverErrors[field]
  }

  // Call when submitting. Returns true if the form can be sent.
  function trySubmit(): boolean {
    setSubmitted(true)
    setServerErrors({})
    setServerMessage('')
    const firstInvalid = Object.keys(clientErrors).find((key) => clientErrors[key])
    if (firstInvalid) {
      document.getElementById(`field-${firstInvalid}`)?.focus()
      return false
    }
    return true
  }

  // Shows an error that came back from the API.
  function handleApiError(err: unknown) {
    if (!(err instanceof ApiError)) {
      setServerMessage('Something went wrong. Please try again.')
      return
    }
    const fields: Record<string, string> = {}
    for (const [key, messages] of Object.entries(err.fieldErrors)) {
      fields[key.charAt(0).toLowerCase() + key.slice(1)] = messages[0]
    }
    setServerErrors(fields)
    setServerMessage(err.message)
  }

  // Messages for the summary box at the top of the form.
  const missing = submitted
    ? Object.values(clientErrors).filter((message): message is string => Boolean(message))
    : []

  return {
    errorFor,
    trySubmit,
    handleApiError,
    touch: (field: string) => setTouched((t) => ({ ...t, [field]: true })),
    reset: () => {
      setSubmitted(false)
      setTouched({})
    },
    invalid: (field: string) => (errorFor(field) ? 'input-invalid' : ''),
    summary: missing,
    serverMessage,
  }
}

export type Validation = ReturnType<typeof useValidation>
