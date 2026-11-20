import { useEffect, useState, type FormEvent } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { ChevronRight, MessageSquare, Pencil, Send, Trash2 } from 'lucide-react'
import { api, type Build, type Comment, type Paged } from '../api'
import { useAuth } from '../auth'
import { useToast } from '../toast'
import { itemImage, itemName, positionImage, runeImage, spellImage, spellName } from '../data/gameData'
import { BuildForm } from '../components/BuildForm'
import { checkText, useValidation } from '../validation'
import {
  CharCount,
  ConfirmModal,
  ErrorBox,
  FieldLabel,
  FieldMessage,
  FormSummary,
  Modal,
  Pagination,
  Panel,
  Spinner,
  formatDate,
} from '../components/common'

export function BuildPage() {
  const params = useParams()
  const championId = Number(params.championId)
  const buildId = Number(params.buildId)
  const { user, isAdmin } = useAuth()
  const toast = useToast()
  const navigate = useNavigate()

  const [build, setBuild] = useState<Build | null>(null)
  const [error, setError] = useState('')
  const [modal, setModal] = useState<'edit' | 'delete' | null>(null)

  useEffect(() => {
    api
      .getBuild(championId, buildId)
      .then(setBuild)
      .catch((err) => setError(err.message))
  }, [championId, buildId])

  if (error) return <ErrorBox message={error} />
  if (!build) return <Spinner />

  const isAuthor = user?.id === build.authorId

  return (
    <>
      <div className="breadcrumbs">
        <Link to="/">Champions</Link> <ChevronRight size={14} />
        <Link to={`/champions/${championId}`}>{build.championName}</Link> <ChevronRight size={14} />
        <span>{build.title}</span>
      </div>

      <Panel
        title={build.title}
        actions={
          <>
            {isAuthor && (
              <button className="btn btn-small" onClick={() => setModal('edit')}>
                <Pencil size={14} /> Edit
              </button>
            )}
            {(isAuthor || isAdmin) && (
              <button className="btn btn-small btn-danger" onClick={() => setModal('delete')}>
                <Trash2 size={14} /> Delete
              </button>
            )}
          </>
        }
      >
        <div className="build-meta">
          <span className="position-badge">
            <img src={positionImage(build.position)} alt="" width={18} height={18} /> {build.position}
          </span>
          <span>
            {build.championName} build by <strong>{build.authorUsername}</strong>
          </span>
          <span className="muted">
            Posted {formatDate(build.createdAt)}
            {build.updatedAt !== build.createdAt && ` · updated ${formatDate(build.updatedAt)}`}
          </span>
        </div>

        <div className="build-sections">
          <section className="build-section">
            <h3>Summoner spells</h3>
            <div className="icon-row">
              {build.summonerSpells.map((key) => (
                <figure key={key} className="icon-with-label">
                  <img src={spellImage(key)} alt="" width={44} height={44} />
                  <figcaption>{spellName(key)}</figcaption>
                </figure>
              ))}
            </div>
          </section>

          <section className="build-section">
            <h3>Runes</h3>
            <div className="icon-row">
              <figure className="icon-with-label">
                <img src={runeImage(build.primaryRuneTree)} alt="" width={44} height={44} />
                <figcaption>
                  {build.primaryRuneTree}
                  <br />
                  <strong>{build.keystone}</strong>
                </figcaption>
              </figure>
              <figure className="icon-with-label secondary-rune">
                <img src={runeImage(build.secondaryRuneTree)} alt="" width={34} height={34} />
                <figcaption>{build.secondaryRuneTree}</figcaption>
              </figure>
            </div>
          </section>
        </div>

        <section className="build-section">
          <h3>Item build order</h3>
          <ol className="item-order-list">
            {build.items.map((id, index) => (
              <li key={id}>
                <span className="item-order">{index + 1}</span>
                <img src={itemImage(id)} alt="" width={48} height={48} />
                <span>{itemName(id)}</span>
              </li>
            ))}
          </ol>
        </section>

        <section className="build-section">
          <h3>Guide</h3>
          <p className="guide-text">{build.description}</p>
        </section>
      </Panel>

      <Comments championId={championId} buildId={buildId} />

      {modal === 'edit' && (
        <BuildForm
          build={build}
          championName={build.championName}
          onClose={() => setModal(null)}
          onSave={async (input) => {
            setBuild(await api.updateBuild(championId, buildId, input))
            toast.success('Build updated.')
            setModal(null)
          }}
        />
      )}

      {modal === 'delete' && (
        <ConfirmModal
          title="Delete this build?"
          message={
            <p>
              "<strong>{build.title}</strong>" and its {build.commentCount} comments will be deleted.
            </p>
          }
          confirmLabel="Delete build"
          onClose={() => setModal(null)}
          onConfirm={async () => {
            try {
              await api.deleteBuild(championId, buildId)
              toast.success('Build deleted.')
              navigate(`/champions/${championId}`)
            } catch (err) {
              toast.error((err as Error).message)
              setModal(null)
            }
          }}
        />
      )}
    </>
  )
}

// ---------- Comments ----------

function Comments({ championId, buildId }: { championId: number; buildId: number }) {
  const { user, isAdmin } = useAuth()
  const toast = useToast()
  const [data, setData] = useState<Paged<Comment> | null>(null)
  const [page, setPage] = useState(1)
  const [reload, setReload] = useState(0)
  const [text, setText] = useState('')
  const [posting, setPosting] = useState(false)
  const v = useValidation({ text: checkText(text, 'Comment', 2, 1000) })
  const [editing, setEditing] = useState<Comment | null>(null)
  const [deleting, setDeleting] = useState<Comment | null>(null)

  useEffect(() => {
    api
      .getComments(championId, buildId, page)
      .then(setData)
      .catch((err) => toast.error(err.message))
  }, [championId, buildId, page, reload])

  async function post(e: FormEvent) {
    e.preventDefault()
    if (!v.trySubmit()) return
    setPosting(true)
    try {
      await api.createComment(championId, buildId, text)
      setText('')
      v.reset()
      toast.success('Comment posted.')
      // Jump to the last page so the new comment is visible.
      const last = Math.max(1, Math.ceil(((data?.totalCount ?? 0) + 1) / (data?.pageSize ?? 10)))
      setPage(last)
      setReload(reload + 1)
    } catch (err) {
      v.handleApiError(err)
    } finally {
      setPosting(false)
    }
  }

  return (
    <Panel title={`Comments (${data?.totalCount ?? 0})`} icon={<MessageSquare size={18} />}>
      {!data && <Spinner />}
      {data && data.items.length === 0 && <p className="muted empty">No comments yet.</p>}

      {data && data.items.length > 0 && (
        <ul className="comment-list">
          {data.items.map((comment) => (
            <li key={comment.id} className="comment">
              <div className="comment-head">
                <span className="comment-avatar">{comment.authorUsername[0].toUpperCase()}</span>
                <strong>{comment.authorUsername}</strong>
                <span className="muted">
                  {formatDate(comment.createdAt)}
                  {comment.updatedAt !== comment.createdAt && ' (edited)'}
                </span>
                <span className="comment-actions">
                  {user?.id === comment.authorId && (
                    <button className="icon-button" onClick={() => setEditing(comment)} title="Edit">
                      <Pencil size={14} />
                    </button>
                  )}
                  {(user?.id === comment.authorId || isAdmin) && (
                    <button className="icon-button danger" onClick={() => setDeleting(comment)} title="Delete">
                      <Trash2 size={14} />
                    </button>
                  )}
                </span>
              </div>
              <p className="comment-text">{comment.text}</p>
            </li>
          ))}
        </ul>
      )}

      {data && <Pagination page={data.page} totalPages={data.totalPages} onChange={setPage} />}

      {user ? (
        <form className="comment-form" onSubmit={post} noValidate>
          <FieldLabel field="text" required hint={<CharCount value={text} min={2} max={1000} />}>
            Leave a comment
          </FieldLabel>
          {v.serverMessage && <FormSummary messages={[]} serverMessage={v.serverMessage} />}
          <textarea
            id="field-text"
            className={v.invalid('text')}
            value={text}
            onChange={(e) => setText(e.target.value)}
            onBlur={() => text && v.touch('text')}
            rows={3}
            maxLength={1000}
            placeholder="What do you think about this build?"
          />
          <FieldMessage message={v.errorFor('text')} />
          <button type="submit" className="btn btn-primary" disabled={posting}>
            <Send size={14} /> {posting ? 'Posting…' : 'Post comment'}
          </button>
        </form>
      ) : (
        <p className="muted">
          <Link to="/login">Log in</Link> to leave a comment.
        </p>
      )}

      {editing && (
        <EditCommentModal
          comment={editing}
          onClose={() => setEditing(null)}
          onSave={async (newText) => {
            await api.updateComment(championId, buildId, editing.id, newText)
            toast.success('Comment updated.')
            setEditing(null)
            setReload(reload + 1)
          }}
        />
      )}

      {deleting && (
        <ConfirmModal
          title="Delete comment?"
          message={<p>"{deleting.text}"</p>}
          confirmLabel="Delete comment"
          onClose={() => setDeleting(null)}
          onConfirm={async () => {
            try {
              await api.deleteComment(championId, buildId, deleting.id)
              toast.success('Comment deleted.')
              if (data && data.items.length === 1 && page > 1) setPage(page - 1)
              setReload(reload + 1)
            } catch (err) {
              toast.error((err as Error).message)
            }
            setDeleting(null)
          }}
        />
      )}
    </Panel>
  )
}

function EditCommentModal(props: {
  comment: Comment
  onSave: (text: string) => Promise<void>
  onClose: () => void
}) {
  const [text, setText] = useState(props.comment.text)
  const [saving, setSaving] = useState(false)
  const v = useValidation({ text: checkText(text, 'Comment', 2, 1000) })

  async function submit(e: FormEvent) {
    e.preventDefault()
    if (!v.trySubmit()) return
    setSaving(true)
    try {
      await props.onSave(text)
    } catch (err) {
      v.handleApiError(err)
    } finally {
      setSaving(false)
    }
  }

  return (
    <Modal title="Edit comment" onClose={props.onClose}>
      <form className="form" onSubmit={submit} noValidate>
        <FormSummary messages={v.summary} serverMessage={v.serverMessage} />
        <div className="form-row">
          <FieldLabel field="text" required hint={<CharCount value={text} min={2} max={1000} />}>
            Comment
          </FieldLabel>
          <textarea
            id="field-text"
            className={v.invalid('text')}
            value={text}
            onChange={(e) => setText(e.target.value)}
            onBlur={() => v.touch('text')}
            rows={4}
            maxLength={1000}
          />
          <FieldMessage message={v.errorFor('text')} />
        </div>
        <div className="form-actions">
          <button type="button" className="btn" onClick={props.onClose}>
            Cancel
          </button>
          <button type="submit" className="btn btn-primary" disabled={saving}>
            {saving ? 'Saving…' : 'Save'}
          </button>
        </div>
      </form>
    </Modal>
  )
}
