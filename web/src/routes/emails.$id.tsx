import { createFileRoute } from '@tanstack/react-router'
import { useEmail } from '../api/hooks'
import { EmailDetail } from '../components/mail/email-detail'
import { ArrowLeft } from 'lucide-react'
import { useNavigate } from '@tanstack/react-router'

export const Route = createFileRoute('/emails/$id')({
  component: EmailDetailPage,
})

function EmailDetailPage() {
  const { id } = Route.useParams()
  const navigate = useNavigate()
  const { data: email, isLoading, error } = useEmail(id)

  if (isLoading) {
    return (
      <div className="container mx-auto p-4">
        <div className="text-center py-8">Loading email...</div>
      </div>
    )
  }

  if (error || !email) {
    return (
      <div className="container mx-auto p-4">
        <button
          onClick={() => navigate({ to: '/' })}
          className="mb-4 flex items-center gap-2 text-blue-600 hover:text-blue-800"
        >
          <ArrowLeft className="h-4 w-4" />
          Back to Inbox
        </button>
        <div className="text-center py-8 text-red-600">
          Email not found or error loading email.
        </div>
      </div>
    )
  }

  return (
    <div className="container mx-auto p-4">
      <button
        onClick={() => navigate({ to: -1 })}
        className="mb-4 flex items-center gap-2 text-blue-600 hover:text-blue-800"
      >
        <ArrowLeft className="h-4 w-4" />
        Back
      </button>
      <EmailDetail email={email} />
    </div>
  )
}
