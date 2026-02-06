#!/bin/sh
set -e

# Generate runtime configuration from environment variables
# Frontend fetches /config/config.json, so we need to create that path
mkdir -p /usr/share/nginx/html/config
cat > /usr/share/nginx/html/config/config.json <<EOF
{
  "oidcAuthority": "${OIDC_AUTHORITY:-}",
  "oidcClientId": "${OIDC_CLIENT_ID:-}",
  "oidcRedirectUri": "${OIDC_REDIRECT_URI:-}",
  "oidcScope": "${OIDC_SCOPE:-openid profile email}"
}
EOF

echo "✓ Generated runtime configuration:"
cat /usr/share/nginx/html/config/config.json

# Start nginx
exec "$@"
