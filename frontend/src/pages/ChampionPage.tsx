import { useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { ChevronRight, Hammer, MessageSquare, Pencil, Plus, ScrollText, Trash2 } from 'lucide-react'
import { api, type Build, type ChampionSummary, type Paged } from '../api'
import { useAuth } from '../auth'
import { useToast } from '../toast'
import { POSITIONS, itemImage, itemName, positionImage } from '../data/gameData'
import { ChampionForm } from '../components/ChampionForm'
import { BuildForm } from '../components/BuildForm'
import {
  ChampionImage,
  ConfirmModal,
  ErrorBox,
  Pagination,
  Panel,
  Spinner,
  formatDate,
} from '../components/common'

export function ChampionPage() {
  const championId = Number(useParams().championId)
  const { user, isAdmin } = useAuth()
  const toast = useToast()
  const navigate = useNavigate()

  const [summary, setSummary] = useState<ChampionSummary | null>(null)
  const [builds, setBuilds] = useState<Paged<Build> | null>(null)
  const [position, setPosition] = useState('')
  const [page, setPage] = useState(1)
  const [error, setError] = useState('')
  const [reload, setReload] = useState(0)
  const [modal, setModal] = useState<'edit' | 'delete' | 'newBuild' | null>(null)

  useEffect(() => {
    api
      .getChampionSummary(championId)
      .then(setSummary)
      .catch((err) => setError(err.message))
  }, [championId, reload])

  useEffect(() => {
    api
      .getBuilds(championId, position, page)
      .then(setBuilds)
      .catch((err) => setError(err.message))
  }, [championId, position, page, reload])

  if (error) return <ErrorBox message={error} />
  if (!summary) return <Spinner />

  const champion = summary.champion

  return (
    <>
      <div className="breadcrumbs">
        <Link to="/">Champions</Link> <ChevronRight size={14} /> <span>{champion.name}</span>
      </div>

      <Panel
        title={champion.name}
        actions={
          isAdmin && (
            <>
              <button className="btn btn-small" onClick={() => setModal('edit')}>
                <Pencil size={14} /> Edit
              </button>
              <button className="btn btn-small btn-danger" onClick={() => setModal('delete')}>
                <Trash2 size={14} /> Delete
              </button>
            </>
          )
        }
      >
        <div className="champion-header">
          <ChampionImage src={champion.imageUrl} alt={champion.name} className="champion-portrait" />
          <div>
            <p className="champion-subtitle">{champion.title}</p>
            <p>{champion.description}</p>
          </div>
        </div>

        <div className="stats">
          <div className="stat">
            <span className="stat-value">{summary.buildCount}</span>
            <span className="stat-label">Builds</span>
          </div>
          <div className="stat">
            <span className="stat-value">{summary.commentCount}</span>
            <span className="stat-label">Comments</span>
          </div>
          {POSITIONS.map((p) => (
            <div className="stat" key={p}>
              <span className="stat-value">
                <img src={positionImage(p)} alt="" width={18} height={18} /> {summary.buildsPerPosition[p] ?? 0}
              </span>
              <span className="stat-label">{p}</span>
            </div>
          ))}
        </div>
      </Panel>

      <Panel
        title={`${champion.name} builds`}
        icon={<Hammer size={18} />}
        actions={
          user ? (
            <button className="btn btn-primary btn-small" onClick={() => setModal('newBuild')}>
              <Plus size={14} /> New build
            </button>
          ) : (
            <Link to="/login" className="btn btn-small">
              Log in to post a build
            </Link>
          )
        }
      >
        <div className="tabs" role="tablist">
          {['', ...POSITIONS].map((p) => (
            <button
              key={p || 'all'}
              role="tab"
              aria-selected={position === p}
              className={`tab ${position === p ? 'tab-active' : ''}`}
              onClick={() => {
                setPosition(p)
                setPage(1)
              }}
            >
              {p && <img src={positionImage(p)} alt="" width={16} height={16} />}
              {p || 'All'}
            </button>
          ))}
        </div>

        {!builds && <Spinner />}
        {builds && builds.items.length === 0 && (
          <p className="muted empty">No builds here yet. {user ? 'Be the first to post one!' : ''}</p>
        )}

        {builds && builds.items.length > 0 && (
          <ul className="build-list">
            {builds.items.map((build) => (
              <li key={build.id}>
                <Link to={`/champions/${championId}/builds/${build.id}`} className="build-row">
                  <img src={positionImage(build.position)} alt={build.position} className="build-row-pos" />
                  <div className="build-row-main">
                    <span className="build-row-title">{build.title}</span>
                    <span className="build-row-meta">
                      <ScrollText size={12} /> {build.keystone} · by <strong>{build.authorUsername}</strong> ·{' '}
                      {formatDate(build.updatedAt)}
                    </span>
                  </div>
                  <div className="build-row-items">
                    {build.items.map((id) => (
                      <img key={id} src={itemImage(id)} alt={itemName(id)} title={itemName(id)} />
                    ))}
                  </div>
                  <span className="build-row-comments" title="Comments">
                    <MessageSquare size={14} /> {build.commentCount}
                  </span>
                </Link>
              </li>
            ))}
          </ul>
        )}

        {builds && <Pagination page={builds.page} totalPages={builds.totalPages} onChange={setPage} />}
      </Panel>

      {modal === 'edit' && (
        <ChampionForm
          champion={champion}
          onClose={() => setModal(null)}
          onSave={async (input) => {
            await api.updateChampion(championId, input)
            toast.success('Champion updated.')
            setModal(null)
            setReload(reload + 1)
          }}
        />
      )}

      {modal === 'delete' && (
        <ConfirmModal
          title={`Delete ${champion.name}?`}
          message={
            <p>
              This also deletes <strong>{summary.buildCount} builds</strong> and{' '}
              <strong>{summary.commentCount} comments</strong>. This can't be undone.
            </p>
          }
          confirmLabel="Delete champion"
          onClose={() => setModal(null)}
          onConfirm={async () => {
            try {
              await api.deleteChampion(championId)
              toast.success(`${champion.name} was deleted.`)
              navigate('/')
            } catch (err) {
              toast.error((err as Error).message)
              setModal(null)
            }
          }}
        />
      )}

      {modal === 'newBuild' && (
        <BuildForm
          championName={champion.name}
          onClose={() => setModal(null)}
          onSave={async (input) => {
            const created = await api.createBuild(championId, input)
            toast.success('Build published!')
            navigate(`/champions/${championId}/builds/${created.id}`)
          }}
        />
      )}
    </>
  )
}
