// API Types
export * from './api/types'

// UI Components - explicit re-exports to avoid name conflicts with api/types
// (Label conflicts with api/types Label interface)
// Consumers should prefer subpath imports: @relate/shared/components/ui
export {
  Badge, badgeVariants, type BadgeProps,
  Button, buttonVariants, type ButtonProps,
  Card, CardContent, CardDescription, CardFooter, CardHeader, CardTitle,
  Dialog, DialogPortal, DialogOverlay, DialogClose, DialogContent, DialogDescription,
  DialogFooter, DialogHeader, DialogTitle, DialogTrigger,
  Input, type InputProps,
  Label as UILabel, type LabelProps,
  Popover, PopoverTrigger, PopoverContent,
  Select, SelectContent, SelectItem, SelectTrigger, SelectValue,
  Switch, type SwitchProps,
} from './components/ui'

// Mail Components - explicit re-exports to avoid name conflicts with api/types
// (EmailDetail conflicts with api/types EmailDetail interface)
// Consumers should prefer subpath imports: @relate/shared/components/mail
export {
  EmailList,
  EmailDetail as EmailDetailComponent,
  EmailDetailView,
  SearchBar,
  LabelBadge,
} from './components/mail'

// Utilities
export { cn } from './lib/utils'
export { sanitizeHtml } from './lib/sanitize'
