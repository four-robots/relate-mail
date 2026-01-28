import { createRootRoute, Link, Outlet } from '@tanstack/react-router'
import { Mail, User, LogOut } from 'lucide-react'
import { Button } from '@/components/ui/button'

export const Route = createRootRoute({
  component: RootComponent,
})

function RootComponent() {
  return (
    <div className="min-h-screen flex flex-col">
      <header className="border-b bg-background">
        <div className="container mx-auto px-4">
          <div className="flex h-14 items-center justify-between">
            <div className="flex items-center gap-6">
              <Link to="/" className="flex items-center gap-2 font-semibold">
                <Mail className="h-5 w-5" />
                <span>Relate SMTP</span>
              </Link>
              <nav className="flex items-center gap-4">
                <Link
                  to="/"
                  className="text-sm text-muted-foreground hover:text-foreground transition-colors [&.active]:text-foreground"
                >
                  Inbox
                </Link>
                <Link
                  to="/profile"
                  className="text-sm text-muted-foreground hover:text-foreground transition-colors [&.active]:text-foreground"
                >
                  Profile
                </Link>
                <Link
                  to="/smtp-settings"
                  className="text-sm text-muted-foreground hover:text-foreground transition-colors [&.active]:text-foreground"
                >
                  SMTP Settings
                </Link>
              </nav>
            </div>
            <div className="flex items-center gap-2">
              <Button variant="ghost" size="icon">
                <User className="h-4 w-4" />
              </Button>
              <Button variant="ghost" size="icon">
                <LogOut className="h-4 w-4" />
              </Button>
            </div>
          </div>
        </div>
      </header>
      <main className="flex-1">
        <Outlet />
      </main>
    </div>
  )
}
