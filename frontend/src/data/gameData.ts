// Static game data (items, spells, runes, positions) generated from Riot's Data Dragon.
import catalog from './catalog.json'

export type ItemInfo = { id: number; name: string; gold: number; boots: boolean }

export const ITEMS: ItemInfo[] = catalog.items
export const SUMMONER_SPELLS: { key: string; name: string }[] = catalog.summonerSpells
export const RUNE_TREES: { name: string; keystones: string[] }[] = catalog.runeTrees
export const POSITIONS: string[] = catalog.positions

// Champion portraits that ship with the frontend (in /public/images/champions).
// Used by the champion form so an admin can pick an image instead of typing a URL.
export const CHAMPION_IMAGES: string[] = [
  'Ahri', 'Annie', 'Caitlyn', 'Darius', 'Diana', 'Ezreal', 'Fiora', 'Garen', 'Jhin', 'Jinx', 'Kaisa',
  'Katarina', 'LeeSin', 'Leona', 'Lux', 'Malphite', 'MissFortune', 'Teemo', 'Thresh', 'Viktor', 'Yasuo', 'Zed',
].map((name) => `/images/champions/${name}.png`)

const itemById = new Map(ITEMS.map((item) => [item.id, item]))
const spellByKey = new Map(SUMMONER_SPELLS.map((spell) => [spell.key, spell]))

export function itemName(id: number): string {
  return itemById.get(id)?.name ?? `Item #${id}`
}

export function spellName(key: string): string {
  return spellByKey.get(key)?.name ?? key
}

export const itemImage = (id: number) => `/images/items/${id}.png`
export const spellImage = (key: string) => `/images/spells/${key}.png`
export const runeImage = (tree: string) => `/images/runes/${tree}.png`
export const positionImage = (position: string) => `/images/positions/${position}.png`

export function keystonesOf(tree: string): string[] {
  return RUNE_TREES.find((t) => t.name === tree)?.keystones ?? []
}
