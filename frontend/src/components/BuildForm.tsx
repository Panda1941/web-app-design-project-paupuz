import { useState, type FormEvent } from 'react'
import { type Build, type BuildInput } from '../api'
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
import { checkText, useValidation } from '../validation'
import { CharCount, FieldLabel, FieldMessage, FormSummary, Modal, RequiredNote } from './common'

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

  const [saving, setSaving] = useState(false)

  // Same rules as BuildRequest on the backend.
  const v = useValidation({
    title: checkText(title, 'Build title', 3, 80),
    summonerSpells:
      spells.length === 2 ? undefined : `Pick ${2 - spells.length} more summoner spell${spells.length === 1 ? '' : 's'}.`,
    items: items.length === 0 ? 'Pick at least 1 item (up to 6).' : undefined,
    description: checkText(description, 'Guide', 10, 4000),
  })

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
    // Checked in the browser first; the server validates everything again.
    if (!v.trySubmit()) return
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
      v.handleApiError(err)
    } finally {
      setSaving(false)
    }
  }

  const visibleItems = ITEMS.filter((item) => item.name.toLowerCase().includes(itemSearch.toLowerCase()))

  return (
    <Modal title={b ? `Edit build: ${b.title}` : `New ${props.championName} build`} onClose={props.onClose} wide>
      <form onSubmit={submit} className="form" noValidate>
        <RequiredNote />
        <FormSummary messages={v.summary} serverMessage={v.serverMessage} />

        <div className="form-row-split">
          <div className="form-row">
            <FieldLabel field="title" required hint={<CharCount value={title} min={3} max={80} />}>
              Build title
            </FieldLabel>
            <input
              id="field-title"
              className={v.invalid('title')}
              value={title}
              onChange={(e) => setTitle(e.target.value)}
              onBlur={() => v.touch('title')}
              maxLength={80}
              placeholder="e.g. Electrocute burst mid"
            />
            <FieldMessage message={v.errorFor('title')} />
          </div>
          <div className="form-row">
            <FieldLabel field="position" required>
              Position
            </FieldLabel>
            <select id="field-position" value={position} onChange={(e) => setPosition(e.target.value)}>
              {POSITIONS.map((p) => (
                <option key={p} value={p}>
                  {p}
                </option>
              ))}
            </select>
            <FieldMessage message={v.errorFor('position')} />
          </div>
        </div>

        <fieldset className="form-row">
          <legend className="field-label">
            <span>
              Summoner spells <span className="required-mark">*</span>
            </span>
            <span className="field-hint">{spells.length} / 2 picked</span>
          </legend>
          <div className={`choice-grid ${v.invalid('summonerSpells')}`}>
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
          <FieldMessage message={v.errorFor('summonerSpells')} />
        </fieldset>

        <div className="form-row-split">
          <fieldset className="form-row">
            <legend className="field-label">
            <span>
              Primary rune tree <span className="required-mark">*</span>
            </span>
          </legend>
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
            <label htmlFor="field-keystone" className="sub-label">
              Keystone <span className="required-mark">*</span>
            </label>
            <select id="field-keystone" value={keystone} onChange={(e) => setKeystone(e.target.value)}>
              {keystonesOf(primaryTree).map((k) => (
                <option key={k} value={k}>
                  {k}
                </option>
              ))}
            </select>
            <FieldMessage message={v.errorFor('keystone') ?? v.errorFor('primaryRuneTree')} />
          </fieldset>

          <fieldset className="form-row">
            <legend className="field-label">
            <span>
              Secondary rune tree <span className="required-mark">*</span>
            </span>
          </legend>
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
            <FieldMessage message={v.errorFor('secondaryRuneTree')} />
          </fieldset>
        </div>

        <fieldset className="form-row">
          <legend className="field-label">
            <span>
              Items <span className="required-mark">*</span>
            </span>
            <span className="field-hint">
              {items.length} / {MAX_ITEMS} · click in the order you buy them
            </span>
          </legend>
          <div className={`selected-items ${v.invalid('items')}`}>
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
          <FieldMessage message={v.errorFor('items')} />
        </fieldset>

        <div className="form-row">
          <FieldLabel field="description" required hint={<CharCount value={description} min={10} max={4000} />}>
            Guide / description
          </FieldLabel>
          <textarea
            id="field-description"
            className={v.invalid('description')}
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            onBlur={() => v.touch('description')}
            maxLength={4000}
            rows={5}
            placeholder="When should people use this build? What's the game plan?"
          />
          <FieldMessage message={v.errorFor('description')} />
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
