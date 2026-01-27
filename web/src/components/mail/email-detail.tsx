import { format } from 'date-fns'
import { ArrowLeft, Download, Paperclip, Trash2 } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { Badge } from '@/components/ui/badge'
import type { EmailDetail } from '@/api/types'

interface EmailDetailViewProps {
  email: EmailDetail
  onBack: () => void
  onDelete: () => void
}

export function EmailDetailView({ email, onBack, onDelete }: EmailDetailViewProps) {
  const downloadAttachment = (attachmentId: string, fileName: string) => {
    const url = `/api/emails/${email.id}/attachments/${attachmentId}`
    const a = document.createElement('a')
    a.href = url
    a.download = fileName
    a.click()
  }

  return (
    <div className="flex flex-col h-full">
      <div className="flex items-center gap-2 p-4 border-b">
        <Button variant="ghost" size="icon" onClick={onBack}>
          <ArrowLeft className="h-4 w-4" />
        </Button>
        <div className="flex-1" />
        <Button variant="ghost" size="icon" onClick={onDelete}>
          <Trash2 className="h-4 w-4" />
        </Button>
      </div>

      <div className="flex-1 overflow-auto p-4">
        <h1 className="text-xl font-semibold mb-4">{email.subject}</h1>

        <div className="flex items-start gap-4 mb-4">
          <div className="flex-shrink-0 w-10 h-10 rounded-full bg-primary/10 flex items-center justify-center">
            <span className="text-lg font-semibold text-primary">
              {(email.fromDisplayName || email.fromAddress)[0].toUpperCase()}
            </span>
          </div>
          <div className="flex-1 min-w-0">
            <p className="font-medium">
              {email.fromDisplayName || email.fromAddress}
            </p>
            <p className="text-sm text-muted-foreground">
              {email.fromAddress}
            </p>
            <p className="text-sm text-muted-foreground">
              {format(new Date(email.receivedAt), 'PPpp')}
            </p>
          </div>
        </div>

        <div className="mb-4">
          <div className="flex flex-wrap gap-1">
            {email.recipients.map((recipient) => (
              <Badge key={recipient.id} variant="secondary">
                {recipient.type}: {recipient.displayName || recipient.address}
              </Badge>
            ))}
          </div>
        </div>

        {email.attachments.length > 0 && (
          <div className="mb-4 p-3 bg-muted rounded-lg">
            <div className="flex items-center gap-2 mb-2">
              <Paperclip className="h-4 w-4" />
              <span className="text-sm font-medium">
                {email.attachments.length} Attachment{email.attachments.length > 1 ? 's' : ''}
              </span>
            </div>
            <div className="flex flex-wrap gap-2">
              {email.attachments.map((attachment) => (
                <Button
                  key={attachment.id}
                  variant="outline"
                  size="sm"
                  onClick={() => downloadAttachment(attachment.id, attachment.fileName)}
                >
                  <Download className="h-3 w-3 mr-1" />
                  {attachment.fileName}
                </Button>
              ))}
            </div>
          </div>
        )}

        <div className="prose prose-sm max-w-none">
          {email.htmlBody ? (
            <div
              dangerouslySetInnerHTML={{ __html: email.htmlBody }}
              className="border rounded-lg p-4 bg-white"
            />
          ) : (
            <pre className="whitespace-pre-wrap font-sans">
              {email.textBody || '(No content)'}
            </pre>
          )}
        </div>
      </div>
    </div>
  )
}
