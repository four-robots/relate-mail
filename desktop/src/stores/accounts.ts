import { atom } from 'jotai'
import { invoke } from '@tauri-apps/api/core'

export interface Account {
  id: string
  display_name: string
  server_url: string
  user_email: string
  api_key_id: string
  scopes: string[]
  created_at: string
  last_used_at: string
}

export interface AccountsData {
  accounts: Account[]
  active_account_id: string | null
}

// Base atoms
export const accountsAtom = atom<Account[]>([])
export const activeAccountIdAtom = atom<string | null>(null)
export const accountsLoadedAtom = atom<boolean>(false)

// Derived atom for active account
export const activeAccountAtom = atom((get) => {
  const accounts = get(accountsAtom)
  const activeId = get(activeAccountIdAtom)
  return accounts.find((a) => a.id === activeId) ?? null
})

// Derived atom for checking if user has any accounts
export const hasAccountsAtom = atom((get) => {
  const accounts = get(accountsAtom)
  return accounts.length > 0
})

// Action: Load accounts from Tauri backend
export const loadAccountsAtom = atom(null, async (_get, set) => {
  try {
    const data = await invoke<AccountsData>('load_accounts')
    set(accountsAtom, data.accounts)
    set(activeAccountIdAtom, data.active_account_id)
    set(accountsLoadedAtom, true)
    return data
  } catch (err) {
    console.error('Failed to load accounts:', err)
    set(accountsLoadedAtom, true)
    throw err
  }
})

// Action: Add a new account
export const addAccountAtom = atom(
  null,
  async (
    _get,
    set,
    {
      account,
      apiKey,
    }: {
      account: Account
      apiKey: string
    }
  ) => {
    try {
      const data = await invoke<AccountsData>('save_account', {
        account,
        apiKey,
      })
      set(accountsAtom, data.accounts)
      set(activeAccountIdAtom, data.active_account_id)
      return data
    } catch (err) {
      console.error('Failed to save account:', err)
      throw err
    }
  }
)

// Action: Remove an account
export const removeAccountAtom = atom(null, async (_get, set, accountId: string) => {
  try {
    const data = await invoke<AccountsData>('delete_account', { accountId })
    set(accountsAtom, data.accounts)
    set(activeAccountIdAtom, data.active_account_id)
    return data
  } catch (err) {
    console.error('Failed to delete account:', err)
    throw err
  }
})

// Action: Switch active account
export const switchAccountAtom = atom(null, async (_get, set, accountId: string) => {
  try {
    const account = await invoke<Account>('set_active_account', { accountId })
    set(activeAccountIdAtom, accountId)
    // Update the account's last_used_at in the local state
    set(accountsAtom, (accounts) =>
      accounts.map((a) =>
        a.id === accountId ? { ...a, last_used_at: new Date().toISOString() } : a
      )
    )
    return account
  } catch (err) {
    console.error('Failed to switch account:', err)
    throw err
  }
})

// Helper: Generate a new account ID
export async function generateAccountId(): Promise<string> {
  return invoke<string>('generate_account_id')
}

// Helper: Get API key for an account
export async function getAccountApiKey(accountId: string): Promise<string | null> {
  return invoke<string | null>('get_account_api_key', { accountId })
}
