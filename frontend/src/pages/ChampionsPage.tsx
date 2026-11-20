import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { Plus, Search, Users } from 'lucide-react'
import { api, type Champion, type Paged } from '../api'
import { useAuth } from '../auth'
import { useToast } from '../toast'
import { ChampionForm } from '../components/ChampionForm'
import { ChampionImage, ErrorBox, Pagination, Panel, Spinner } from '../components/common'

export function ChampionsPage() {
  const { isAdmin } = useAuth()
  const toast = useToast()
  const [search, setSearch] = useState('')
  const [page, setPage] = useState(1)
  const [data, setData] = useState<Paged<Champion> | null>(null)
  const [error, setError] = useState('')
  const [showForm, setShowForm] = useState(false)
  const [reload, setReload] = useState(0)

  // Load champions whenever the search, page or reload counter changes.
  // The small delay avoids a request on every key press while typing.
  useEffect(() => {
    const timer = setTimeout(() => {
      api
        .getChampions(search, page)
        .then((result) => {
          setData(result)
          setError('')
        })
        .catch((err) => setError(err.message))
    }, 250)
    return () => clearTimeout(timer)
  }, [search, page, reload])

  return (
    <>
      <Panel
        title="Champions"
        icon={<Users size={18} />}
        actions={
          isAdmin && (
            <button className="btn btn-primary btn-small" onClick={() => setShowForm(true)}>
              <Plus size={14} /> Add champion
            </button>
          )
        }
      >
        <div className="search-box">
          <Search size={16} />
          <input
            type="search"
            value={search}
            onChange={(e) => {
              setSearch(e.target.value)
              setPage(1)
            }}
            placeholder="Search champions by name…"
            aria-label="Search champions"
          />
        </div>

        {error && <ErrorBox message={error} />}
        {!data && !error && <Spinner />}

        {data && data.items.length === 0 && <p className="muted">No champions match "{search}".</p>}

        {data && (
          <div className="champion-grid">
            {data.items.map((champion) => (
              <Link key={champion.id} to={`/champions/${champion.id}`} className="champion-card">
                <ChampionImage src={champion.imageUrl} alt={champion.name} className="champion-card-img" />
                <span className="champion-card-name">{champion.name}</span>
                <span className="champion-card-title">{champion.title}</span>
              </Link>
            ))}
          </div>
        )}

        {data && <Pagination page={data.page} totalPages={data.totalPages} onChange={setPage} />}
      </Panel>

      {showForm && (
        <ChampionForm
          onClose={() => setShowForm(false)}
          onSave={async (input) => {
            const created = await api.createChampion(input)
            toast.success(`${created.name} was added.`)
            setShowForm(false)
            setReload(reload + 1)
          }}
        />
      )}
    </>
  )
}
