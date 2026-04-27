#!/bin/sh
# Generates /usr/share/nginx/html/config/config.json from environment variables.
# Installed at /docker-entrypoint.d/30-generate-config-json.sh in the image; the
# nginx base image runs every executable in /docker-entrypoint.d/ before launching
# nginx, so we only have to write the file here, not start the server.
set -e

mkdir -p /usr/share/nginx/html/config
cat > /usr/share/nginx/html/config/config.json <<EOF
{
  "oidcAuthority": "${OIDC_AUTHORITY:-}",
  "oidcClientId": "${OIDC_CLIENT_ID:-}",
  "oidcRedirectUri": "${OIDC_REDIRECT_URI:-}",
  "oidcScope": "${OIDC_SCOPE:-openid profile email}"
}
EOF

echo "[entrypoint] generated /usr/share/nginx/html/config/config.json"
cat /usr/share/nginx/html/config/config.json
