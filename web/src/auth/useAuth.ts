import { useAuth as useOidcAuth, type AuthContextProps } from 'react-oidc-context'

/**
 * Wraps `react-oidc-context`'s `useAuth` so the rest of the app can call it
 * unconditionally. When `oidc.authority` is empty in runtime config, the
 * `<AuthProvider />` in `auth/AuthProvider.tsx` renders its children without
 * the OIDC context — so the upstream `useAuth()` returns `undefined`, and any
 * downstream `auth.isAuthenticated` access blows up with TypeError.
 *
 * This shim returns a sensible "auth bypassed" stub in that case, matching the
 * intent of the AuthProvider's dev-mode branch: the user is treated as logged
 * in, and the auth-related actions are no-ops.
 */
export function useAuth(): AuthContextProps {
  const auth = useOidcAuth()
  if (auth) return auth
  return DEV_MODE_STUB
}

const DEV_MODE_STUB: AuthContextProps = {
  isAuthenticated: true,
  isLoading: false,
  activeNavigator: undefined,
  error: undefined,
  user: {
    profile: {
      sub: 'dev-user',
      email: 'dev@localhost',
      name: 'Dev User',
      preferred_username: 'dev',
      iss: 'dev',
      aud: 'dev',
      exp: Number.MAX_SAFE_INTEGER,
      iat: 0,
    },
    access_token: '',
    token_type: 'Bearer',
    expires_at: Number.MAX_SAFE_INTEGER,
    scope: 'openid profile email',
    state: undefined,
    session_state: null,
    expires_in: Number.MAX_SAFE_INTEGER,
    expired: false,
    scopes: ['openid', 'profile', 'email'],
    toStorageString: () => '',
  } as unknown as AuthContextProps['user'],
  settings: {} as AuthContextProps['settings'],
  events: {} as AuthContextProps['events'],
  signinRedirect: async () => {},
  signinResourceOwnerCredentials: async () => undefined as never,
  signinPopup: async () => undefined as never,
  signinSilent: async () => null,
  signoutRedirect: async () => {},
  signoutPopup: async () => {},
  signoutSilent: async () => {},
  removeUser: async () => {},
  querySessionStatus: async () => null,
  revokeTokens: async () => {},
  startSilentRenew: () => {},
  stopSilentRenew: () => {},
  clearStaleState: async () => {},
}
