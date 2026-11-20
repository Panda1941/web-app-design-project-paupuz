import { useEffect, useState } from 'react'
import { Link, useParams } from 'react-router-dom'
import { ArrowLeft, ChevronRight, MessageSquare } from 'lucide-react'
import { api, type Build, type Comment } from '../api'
import { ErrorBox, Panel, Spinner, formatDate } from '../components/common'

// One comment on its own page. The URL matches the API:
//   page: /champions/1/builds/2/comments/3
//   API:  GET /api/v1/champions/1/builds/2/comments/3
export function CommentPage() {
  const params = useParams()
  const championId = Number(params.championId)
  const buildId = Number(params.buildId)
  const commentId = Number(params.commentId)

  const [comment, setComment] = useState<Comment | null>(null)
  const [build, setBuild] = useState<Build | null>(null)
  const [error, setError] = useState('')

  useEffect(() => {
    api
      .getComment(championId, buildId, commentId)
      .then(setComment)
      .catch((err) => setError(err.message))
    api
      .getBuild(championId, buildId)
      .then(setBuild)
      .catch(() => {})
  }, [championId, buildId, commentId])

  if (error) return <ErrorBox message={error} />
  if (!comment) return <Spinner />

  const buildUrl = `/champions/${championId}/builds/${buildId}`

  return (
    <>
      <div className="breadcrumbs">
        <Link to="/">Champions</Link> <ChevronRight size={14} />
        <Link to={`/champions/${championId}`}>{build?.championName ?? 'Champion'}</Link> <ChevronRight size={14} />
        <Link to={buildUrl}>{build?.title ?? 'Build'}</Link> <ChevronRight size={14} />
        <span>Comment #{comment.id}</span>
      </div>

      <Panel title={`Comment #${comment.id}`} icon={<MessageSquare size={18} />}>
        <div className="comment comment-single">
          <div className="comment-head">
            <span className="comment-avatar">{comment.authorUsername[0].toUpperCase()}</span>
            <strong>{comment.authorUsername}</strong>
            <span className="muted">
              {formatDate(comment.createdAt)}
              {comment.updatedAt !== comment.createdAt && ` · edited ${formatDate(comment.updatedAt)}`}
            </span>
          </div>
          <p className="comment-text">{comment.text}</p>
        </div>
        <p className="muted">
          Posted on <Link to={buildUrl}>{build?.title ?? 'this build'}</Link>
          {build && ` (${build.championName}, ${build.position})`}.
        </p>
        <Link to={buildUrl} className="btn btn-small">
          <ArrowLeft size={14} /> Back to the build
        </Link>
      </Panel>
    </>
  )
}
