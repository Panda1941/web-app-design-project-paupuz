import { useState, type ChangeEvent, type FormEvent } from 'react'
import { Upload } from 'lucide-react'
import { ApiError, type Champion, type ChampionInput } from '../api'
import { CHAMPION_IMAGES } from '../data/gameData'
import { ChampionImage, FieldError, Modal, normalizeErrors } from './common'

// Create / edit champion form, shown in a modal. Admin only.
export function ChampionForm(props: {
  champion?: Champion
  onSave: (input: ChampionInput) => Promise<void>
  onClose: () => void
}) {
  const [name, setName] = useState(props.champion?.name ?? '')
  const [title, setTitle] = useState(props.champion?.title ?? '')
  const [description, setDescription] = useState(props.champion?.description ?? '')
  const [imageUrl, setImageUrl] = useState(props.champion?.imageUrl ?? CHAMPION_IMAGES[0])
  const [errors, setErrors] = useState<Record<string, string[]>>({})
  const [formError, setFormError] = useState('')
  const [saving, setSaving] = useState(false)

  // Reads the chosen picture, shrinks it to 128x128 on a canvas and keeps it as a data URL.
  async function handleUpload(e: ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0]
    if (!file) return
    try {
      setImageUrl(await resizeImage(file, 128))
    } catch {
      setFormError('That file could not be read as an image.')
    }
  }

  async function submit(e: FormEvent) {
    e.preventDefault()
    setErrors({})
    setFormError('')
    if (!imageUrl) {
      setFormError('Pick, link or upload a portrait.')
      return
    }
    setSaving(true)
    try {
      await props.onSave({ name, title, description, imageUrl })
    } catch (err) {
      if (err instanceof ApiError) {
        setErrors(normalizeErrors(err.fieldErrors))
        setFormError(err.message)
      }
    } finally {
      setSaving(false)
    }
  }

  return (
    <Modal title={props.champion ? `Edit ${props.champion.name}` : 'Add champion'} onClose={props.onClose}>
      <form onSubmit={submit} className="form">
        {formError && <div className="form-error">{formError}</div>}

        <div className="form-row-split">
          <div className="form-row">
            <label htmlFor="champ-name">Name</label>
            <input
              id="champ-name"
              value={name}
              onChange={(e) => setName(e.target.value)}
              required
              minLength={2}
              maxLength={50}
              placeholder="e.g. Garen"
            />
            <FieldError errors={errors.name} />
          </div>
          <div className="form-row">
            <label htmlFor="champ-title">Title</label>
            <input
              id="champ-title"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              required
              minLength={2}
              maxLength={80}
              placeholder="e.g. the Might of Demacia"
            />
            <FieldError errors={errors.title} />
          </div>
        </div>

        <div className="form-row">
          <label htmlFor="champ-desc">Description</label>
          <textarea
            id="champ-desc"
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            required
            minLength={10}
            maxLength={2000}
            rows={4}
          />
          <FieldError errors={errors.description} />
        </div>

        <div className="form-row">
          <label htmlFor="champ-image">Portrait</label>
          <div className="image-picker">
            <ChampionImage src={imageUrl} alt="Selected portrait" className="image-preview" />
            <div className="image-picker-fields">
              <select
                id="champ-image"
                value={CHAMPION_IMAGES.includes(imageUrl) ? imageUrl : ''}
                onChange={(e) => e.target.value && setImageUrl(e.target.value)}
              >
                <option value="">Custom URL…</option>
                {CHAMPION_IMAGES.map((src) => (
                  <option key={src} value={src}>
                    {src.split('/').pop()?.replace('.png', '')}
                  </option>
                ))}
              </select>
              <input
                type="text"
                value={imageUrl.startsWith('data:') ? '' : imageUrl}
                onChange={(e) => setImageUrl(e.target.value)}
                placeholder={imageUrl.startsWith('data:') ? '(uploaded picture)' : 'Image URL'}
                maxLength={300}
                aria-label="Image URL"
              />
              <label className="btn btn-small file-button">
                <Upload size={14} /> Upload from computer
                <input type="file" accept="image/*" onChange={handleUpload} hidden />
              </label>
            </div>
          </div>
          <FieldError errors={errors.imageurl} />
        </div>

        <div className="form-actions">
          <button type="button" className="btn" onClick={props.onClose}>
            Cancel
          </button>
          <button type="submit" className="btn btn-primary" disabled={saving}>
            {saving ? 'Saving…' : 'Save champion'}
          </button>
        </div>
      </form>
    </Modal>
  )
}

// Draws the image onto a square canvas (cropping the centre) and returns a JPEG data URL.
function resizeImage(file: File, size: number): Promise<string> {
  return new Promise((resolve, reject) => {
    const url = URL.createObjectURL(file)
    const img = new Image()
    img.onload = () => {
      const side = Math.min(img.width, img.height)
      const canvas = document.createElement('canvas')
      canvas.width = size
      canvas.height = size
      const ctx = canvas.getContext('2d')!
      ctx.drawImage(img, (img.width - side) / 2, (img.height - side) / 2, side, side, 0, 0, size, size)
      URL.revokeObjectURL(url)
      resolve(canvas.toDataURL('image/jpeg', 0.85))
    }
    img.onerror = () => {
      URL.revokeObjectURL(url)
      reject(new Error('not an image'))
    }
    img.src = url
  })
}
