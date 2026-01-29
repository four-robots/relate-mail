// Runtime configuration loaded from /config.json
export interface AppConfig {
  oidcAuthority: string
  oidcClientId: string
  oidcRedirectUri: string
  oidcScope: string
}

let config: AppConfig | null = null

/**
 * Load configuration from /config.json at runtime
 */
export async function loadConfig(): Promise<AppConfig> {
  if (config) {
    return config
  }

  try {
    const response = await fetch('/config/config.json')
    if (!response.ok) {
      throw new Error(`Failed to load config: ${response.statusText}`)
    }
    const loadedConfig = await response.json() as AppConfig

    // Fallback to build-time env vars if config.json values are empty
    if (!loadedConfig.oidcAuthority) {
      loadedConfig.oidcAuthority = import.meta.env.VITE_OIDC_AUTHORITY || ''
    }
    if (!loadedConfig.oidcClientId) {
      loadedConfig.oidcClientId = import.meta.env.VITE_OIDC_CLIENT_ID || ''
    }
    if (!loadedConfig.oidcRedirectUri) {
      loadedConfig.oidcRedirectUri = import.meta.env.VITE_OIDC_REDIRECT_URI || window.location.origin
    }
    if (!loadedConfig.oidcScope) {
      loadedConfig.oidcScope = import.meta.env.VITE_OIDC_SCOPE || 'openid profile email'
    }

    config = loadedConfig
    return config
  } catch (error) {
    console.error('Failed to load runtime config, falling back to build-time env vars:', error)

    // Fallback to build-time environment variables
    config = {
      oidcAuthority: import.meta.env.VITE_OIDC_AUTHORITY || '',
      oidcClientId: import.meta.env.VITE_OIDC_CLIENT_ID || '',
      oidcRedirectUri: import.meta.env.VITE_OIDC_REDIRECT_URI || window.location.origin,
      oidcScope: import.meta.env.VITE_OIDC_SCOPE || 'openid profile email',
    }

    return config
  }
}

/**
 * Get the loaded configuration
 * Must call loadConfig() first
 */
export function getConfig(): AppConfig {
  if (!config) {
    throw new Error('Configuration not loaded. Call loadConfig() first.')
  }
  return config
}
