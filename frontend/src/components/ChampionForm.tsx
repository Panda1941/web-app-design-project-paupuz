import { useState, type ChangeEvent, type FormEvent } from 'react'
import { Upload } from 'lucide-react'
import { type Champion, type ChampionInput } from '../api'
import { CHAMPION_IMAGES } from '../data/gameData'
import { checkText, useValidation } from '../validation'
import {
  CharCount,
  ChampionImage,
  FieldLabel,
  FieldMessage,
  FormSummary,
  Modal,
  RequiredNote,
} from './common'

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
  const [saving, setSaving] = useState(false)

  // Same limits as ChampionRequest on the backend.
  const v = useValidation({
    name: checkText(name, 'Name', 2, 50),
    title: checkText(title, 'Title', 2, 80),
    description: checkText(description, 'Description', 10, 2000),
    imageUrl: imageUrl ? undefined : 'Pick, link or upload a portrait.',
  })

  // Reads the chosen picture, shrinks it to 128x128 on a canvas and keeps it as a data URL.
  async function handleUpload(e: ChangeEvent<HTMLInputElement>) {
    const file = e.target.files?.[0]
    if (!file) return
    try {
      setImageUrl(await resizeImage(file, 128))
    } catch {
      v.handleApiError(new Error('not an image'))
    }
  }

  async function submit(e: FormEvent) {
    e.preventDefault()
    if (!v.trySubmit()) return
    setSaving(true)
    try {
      await props.onSave({ name, title, description, imageUrl })
    } catch (err) {
      v.handleApiError(err)
    } finally {
      setSaving(false)
    }
  }

  return (
    <Modal title={props.champion ? `Edit ${props.champion.name}` : 'Add champion'} onClose={props.onClose}>
      <form onSubmit={submit} className="form" noValidate>
        <RequiredNote />
        <FormSummary messages={v.summary} serverMessage={v.serverMessage} />

        <div className="form-row-split">
          <div className="form-row">
            <FieldLabel field="name" required hint={<CharCount value={name} min={2} max={50} />}>
              Name
            </FieldLabel>
            <input
              id="field-name"
              className={v.invalid('name')}
              value={name}
              onChange={(e) => setName(e.target.value)}
              onBlur={() => v.touch('name')}
              maxLength={50}
              placeholder="e.g. Garen"
            />
            <FieldMessage message={v.errorFor('name')} />
          </div>
          <div className="form-row">
            <FieldLabel field="title" required hint={<CharCount value={title} min={2} max={80} />}>
              Title
            </FieldLabel>
            <input
              id="field-title"
              className={v.invalid('title')}
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              onBlur={() => v.touch('title')}
              maxLength={80}
              placeholder="e.g. the Might of Demacia"
            />
            <FieldMessage message={v.errorFor('title')} />
          </div>
        </div>

        <div className="form-row">
          <FieldLabel field="description" required hint={<CharCount value={description} min={10} max={2000} />}>
            Description
          </FieldLabel>
          <textarea
            id="field-description"
            className={v.invalid('description')}
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            onBlur={() => v.touch('description')}
            maxLength={2000}
            rows={4}
            placeholder="Who is this champion? At least 10 characters."
          />
          <FieldMessage message={v.errorFor('description')} />
        </div>

        <div className="form-row">
          <FieldLabel field="imageUrl" required hint="pick one, paste a URL or upload">
            Portrait
          </FieldLabel>
          <div className="image-picker">
            <ChampionImage src={imageUrl || '/images/champion-placeholder.svg'} alt="Selected portrait" className="image-preview" />
            <div className="image-picker-fields">
              <select
                id="field-imageUrl"
                value={CHAMPION_IMAGES.includes(imageUrl) ? imageUrl : ''}
                onChange={(e) => e.target.value && setImageUrl(e.target.value)}
              >
                <option value="">Custom URL / upload…</option>
                {CHAMPION_IMAGES.map((src) => (
                  <option key={src} value={src}>
                    {src.split('/').pop()?.replace('.png', '')}
                  </option>
                ))}
              </select>
              <input
                type="text"
                className={v.invalid('imageUrl')}
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
          <FieldMessage message={v.errorFor('imageUrl')} />
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
