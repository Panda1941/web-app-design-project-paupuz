import { useState, type FormEvent } from 'react'
import { ApiError, type Build, type BuildInput } from '../api'
import {
  ITEMS,
  POSITIONS,
  RUNE_TREES,
  SUMMONER_SPELLS,
  itemImage,
  itemName,
  keystonesOf,
  runeImage,
  spellImage,
} from '../data/gameData'
import { FieldError, Modal, normalizeErrors } from './common'

const MAX_ITEMS = 6

// Create / edit build form, shown in a wide modal.
// Uses several input types: text, select, textarea, checkboxes (spells, items) and radio buttons (rune trees).
export function BuildForm(props: {
  build?: Build
  championName: string
  onSave: (input: BuildInput) => Promise<void>
  onClose: () => void
}) {
  const b = props.build
  const [title, setTitle] = useState(b?.title ?? '')
  const [position, setPosition] = useState(b?.position ?? 'Mid')
  const [description, setDescription] = useState(b?.description ?? '')
  const [spells, setSpells] = useState<string[]>(b?.summonerSpells ?? ['SummonerFlash'])
  const [primaryTree, setPrimaryTree] = useState(b?.primaryRuneTree ?? 'Precision')
  const [keystone, setKeystone] = useState(b?.keystone ?? keystonesOf('Precision')[0])
  const [secondaryTree, setSecondaryTree] = useState(b?.secondaryRuneTree ?? 'Resolve')
  const [items, setItems] = useState<number[]>(b?.items ?? [])
  const [itemSearch, setItemSearch] = useState('')

  const [errors, setErrors] = useState<Record<string, string[]>>({})
  const [formError, setFormError] = useState('')
  const [saving, setSaving] = useState(false)

  function toggleSpell(key: string) {
    if (spells.includes(key)) setSpells(spells.filter((s) => s !== key))
    else if (spells.length < 2) setSpells([...spells, key])
  }

  function toggleItem(id: number) {
    if (items.includes(id)) setItems(items.filter((i) => i !== id))
    else if (items.length < MAX_ITEMS) setItems([...items, id])
  }

  function changePrimaryTree(tree: string) {
    setPrimaryTree(tree)
    setKeystone(keystonesOf(tree)[0])
    if (secondaryTree === tree) setSecondaryTree(RUNE_TREES.find((t) => t.name !== tree)!.name)
  }

  async function submit(e: FormEvent) {
    e.preventDefault()
    setErrors({})
    setFormError('')

    // Quick checks in the browser; the server validates everything again.
    if (spells.length !== 2) {
      setErrors({ summonerspells: ['Pick exactly 2 summoner spells.'] })
      return
    }
    if (items.length === 0) {
      setErrors({ items: ['Pick at least 1 item.'] })
      return
    }

    setSaving(true)
    try {
      await props.onSave({
        title,
        description,
        position,
        items,
        summonerSpells: spells,
        primaryRuneTree: primaryTree,
        keystone,
        secondaryRuneTree: secondaryTree,
      })
    } catch (err) {
      if (err instanceof ApiError) {
        setErrors(normalizeErrors(err.fieldErrors))
        setFormError(err.message)
      }
    } finally {
      setSaving(false)
    }
  }

  const visibleItems = ITEMS.filter((item) => item.name.toLowerCase().includes(itemSearch.toLowerCase()))

  return (
    <Modal title={b ? `Edit build: ${b.title}` : `New ${props.championName} build`} onClose={props.onClose} wide>
      <form onSubmit={submit} className="form">
        {formError && <div className="form-error">{formError}</div>}

        <div className="form-row-split">
          <div className="form-row">
            <label htmlFor="build-title">Build title</label>
            <input
              id="build-title"
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              required
              minLength={3}
              maxLength={80}
              placeholder="e.g. Electrocute burst mid"
            />
            <FieldError errors={errors.title} />
          </div>
          <div className="form-row">
            <label htmlFor="build-position">Position</label>
            <select id="build-position" value={position} onChange={(e) => setPosition(e.target.value)}>
              {POSITIONS.map((p) => (
                <option key={p} value={p}>
                  {p}
                </option>
              ))}
            </select>
            <FieldError errors={errors.position} />
          </div>
        </div>

        <fieldset className="form-row">
          <legend>Summoner spells (pick 2)</legend>
          <div className="choice-grid">
            {SUMMONER_SPELLS.map((spell) => (
              <label key={spell.key} className={`choice ${spells.includes(spell.key) ? 'choice-on' : ''}`}>
                <input
                  type="checkbox"
                  checked={spells.includes(spell.key)}
                  onChange={() => toggleSpell(spell.key)}
                />
                <img src={spellImage(spell.key)} alt="" width={32} height={32} />
                <span>{spell.name}</span>
              </label>
            ))}
          </div>
          <FieldError errors={errors.summonerspells} />
        </fieldset>

        <div className="form-row-split">
          <fieldset className="form-row">
            <legend>Primary rune tree</legend>
            <div className="radio-list">
              {RUNE_TREES.map((tree) => (
                <label key={tree.name} className={`radio ${primaryTree === tree.name ? 'radio-on' : ''}`}>
                  <input
                    type="radio"
                    name="primaryTree"
                    value={tree.name}
                    checked={primaryTree === tree.name}
                    onChange={() => changePrimaryTree(tree.name)}
                  />
                  <img src={runeImage(tree.name)} alt="" width={22} height={22} />
                  {tree.name}
                </label>
              ))}
            </div>
            <label htmlFor="build-keystone" className="sub-label">
              Keystone
            </label>
            <select id="build-keystone" value={keystone} onChange={(e) => setKeystone(e.target.value)}>
              {keystonesOf(primaryTree).map((k) => (
                <option key={k} value={k}>
                  {k}
                </option>
              ))}
            </select>
            <FieldError errors={errors.keystone ?? errors.primaryrunetree} />
          </fieldset>

          <fieldset className="form-row">
            <legend>Secondary rune tree</legend>
            <div className="radio-list">
              {RUNE_TREES.filter((tree) => tree.name !== primaryTree).map((tree) => (
                <label key={tree.name} className={`radio ${secondaryTree === tree.name ? 'radio-on' : ''}`}>
                  <input
                    type="radio"
                    name="secondaryTree"
                    value={tree.name}
                    checked={secondaryTree === tree.name}
                    onChange={() => setSecondaryTree(tree.name)}
                  />
                  <img src={runeImage(tree.name)} alt="" width={22} height={22} />
                  {tree.name}
                </label>
              ))}
            </div>
            <FieldError errors={errors.secondaryrunetree} />
          </fieldset>
        </div>

        <fieldset className="form-row">
          <legend>
            Items ({items.length}/{MAX_ITEMS}) – click in the order you buy them
          </legend>
          <div className="selected-items">
            {items.length === 0 && <span className="muted">No items selected yet.</span>}
            {items.map((id, index) => (
              <button
                type="button"
                key={id}
                className="selected-item"
                onClick={() => toggleItem(id)}
                title={`Remove ${itemName(id)}`}
              >
                <span className="item-order">{index + 1}</span>
                <img src={itemImage(id)} alt={itemName(id)} width={36} height={36} />
              </button>
            ))}
          </div>
          <input
            type="search"
            value={itemSearch}
            onChange={(e) => setItemSearch(e.target.value)}
            placeholder="Search items…"
            aria-label="Search items"
          />
          <div className="item-grid">
            {visibleItems.map((item) => (
              <label
                key={item.id}
                className={`item-choice ${items.includes(item.id) ? 'item-choice-on' : ''}`}
                title={`${item.name} (${item.gold}g)`}
              >
                <input type="checkbox" checked={items.includes(item.id)} onChange={() => toggleItem(item.id)} />
                <img src={itemImage(item.id)} alt={item.name} width={40} height={40} loading="lazy" />
              </label>
            ))}
          </div>
          <FieldError errors={errors.items} />
        </fieldset>

        <div className="form-row">
          <label htmlFor="build-desc">Guide / description</label>
          <textarea
            id="build-desc"
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            required
            minLength={10}
            maxLength={4000}
            rows={5}
            placeholder="When should people use this build? What's the game plan?"
          />
          <FieldError errors={errors.description} />
        </div>

        <div className="form-actions">
          <button type="button" className="btn" onClick={props.onClose}>
            Cancel
          </button>
          <button type="submit" className="btn btn-primary" disabled={saving}>
            {saving ? 'Saving…' : 'Save build'}
          </button>
        </div>
      </form>
    </Modal>
  )
}
