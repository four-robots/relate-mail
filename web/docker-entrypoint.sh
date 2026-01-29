#!/bin/sh
set -e

# Generate runtime configuration from environment variables
cat > /usr/share/nginx/html/config.json <<EOF
{
  "oidcAuthority": "${OIDC_AUTHORITY:-}",
  "oidcClientId": "${OIDC_CLIENT_ID:-}",
  "oidcRedirectUri": "${OIDC_REDIRECT_URI:-}",
  "oidcScope": "${OIDC_SCOPE:-openid profile email}"
}
EOF

echo "✓ Generated runtime configuration:"
cat /usr/share/nginx/html/config.json

# Start nginx
exec "$@"
