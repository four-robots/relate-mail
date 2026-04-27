# relate-mail Helm chart

Deploys the Relate Mail platform — REST API, SMTP, POP3, IMAP — to Kubernetes,
with an optional in-chart PostgreSQL.

## TL;DR

```bash
helm install relate-mail ./helm/relate-mail \
  --namespace relate-mail --create-namespace \
  --set postgresql.auth.password=changeme \
  --set smtp.serverName=smtp.example.com \
  --set pop3.serverName=pop3.example.com \
  --set imap.serverName=imap.example.com
```

## Layout

Each protocol host runs as its own Deployment with its own Service so that
SMTP/POP3/IMAP ports can be exposed via separate `LoadBalancer` Services
(typical), while the HTTP API stays internal behind an `Ingress`.

| Component  | Workload     | Default Service type | Ports                                   |
|------------|--------------|----------------------|-----------------------------------------|
| api        | Deployment   | ClusterIP            | 8080 (HTTP)                             |
| smtp       | Deployment   | LoadBalancer         | 587, 465, optionally 25 (MX)            |
| pop3       | Deployment   | LoadBalancer         | 110, 995                                |
| imap       | Deployment   | LoadBalancer         | 143, 993                                |
| web        | Deployment   | ClusterIP            | 8080 (HTTP, disabled by default)        |
| postgresql | StatefulSet  | Headless ClusterIP   | 5432                                    |

The SMTP, POP3, and IMAP hosts each expose an internal HTTP health endpoint
on a separate port (8081, 8082, 8083) used for liveness/readiness probes.

## Database

You can either let the chart deploy PostgreSQL or point at an existing one.

**In-chart (default, dev only):**
```yaml
postgresql:
  enabled: true
  auth:
    password: changeme
```

**External:**
```yaml
postgresql:
  enabled: false
database:
  connectionString: "Host=db.internal;Port=5432;Database=relate_mail;Username=relate;Password=..."
```

Or reference an existing `Secret`:
```yaml
postgresql:
  enabled: false
database:
  existingSecret: relate-mail-db
  existingSecretKey: connectionString
```

## Authentication

OIDC is optional. When `oidc.authority` is empty the API runs in dev mode
with no authentication.

```yaml
oidc:
  authority: https://issuer.example.com/realms/relate
  audience: relate-mail
```

The `internal.apiKey` value is the pre-shared key the SMTP host uses to call
the API's `/internal` notification endpoint. Generate an API key with the
`internal` scope in the database, then provide it inline or via `existingSecret`.

## Images

By default the chart pulls per-component images from GHCR:

```
ghcr.io/four-robots/relate-mail-api:<chart appVersion>
ghcr.io/four-robots/relate-mail-smtp:<chart appVersion>
ghcr.io/four-robots/relate-mail-pop3:<chart appVersion>
ghcr.io/four-robots/relate-mail-imap:<chart appVersion>
```

Override globally:
```yaml
image:
  registry: ghcr.io
  repository: four-robots/relate-mail
  tag: "1.2.3"
```

Or per component:
```yaml
api:
  image:
    repository: my-registry.example.com/relate-mail-api
    tag: "1.2.3"
```

## SMTP MX (inbound internet mail)

Disabled by default. Enable only when this deployment is the authoritative
mail server for one or more domains:

```yaml
smtp:
  mx:
    enabled: true
    port: 25
    hostedDomains:
      - example.com
      - mail.example.com
    validateRecipients: true
```

## Outbound delivery (relay through a smarthost)

Relate Mail's queue processor (`DeliveryQueueProcessor`) is registered in
every host that calls `AddInfrastructure` — so outbound `OutboundMail__*`
config is delivered to all four deployments via a shared `ConfigMap` and
`Secret`. With no relay configured, the SMTP daemon attempts direct-to-MX
delivery, which is rarely accepted from cluster egress IPs (residential or
cloud reputation, missing PTR, etc.). For real deliverability, point the
service at a smarthost such as Resend, SES, SendGrid, Mailgun, or an
internal Postfix relay.

```yaml
outbound:
  enabled: true
  relayHost: smtp.resend.com
  relayPort: 587
  relayUseTls: true
  relayUsername: resend
  relayPassword: re_xxxxxxxxxxxxxxxx
  senderDomain: mail.example.com
```

Or use an existing `Secret` so the password isn't in your values file:

```yaml
outbound:
  enabled: true
  relayHost: email-smtp.us-east-1.amazonaws.com
  relayPort: 587
  relayUsername: AKIA...
  existingSecret: relate-mail-relay
  existingSecretKey: relayPassword
  senderDomain: mail.example.com
```

When `relayHost` is empty, mail is queued and the daemon attempts
direct MX delivery — fine for testing, generally not for production.

## Web SPA

The `web` component is **disabled by default** because the project does
not yet publish a `relate-mail-web` image. Once it does, enable it as a
separate pod so it can roll independently of the API/protocol hosts:

```yaml
web:
  enabled: true
  image:
    # falls back to ghcr.io/four-robots/relate-mail-web:<chart appVersion>
    tag: "1.2.3"
  apiUrl: /api
  oidc:
    clientId: relate-mail-web
    redirectUri: https://relate.example.com
  ingress:
    enabled: true
    className: nginx
    hosts:
      - host: relate.example.com
        paths:
          - path: /
            pathType: Prefix
```

`OIDC_AUTHORITY` is shared with the API via the top-level `oidc.authority`.
`API_URL`, `OIDC_CLIENT_ID`, `OIDC_REDIRECT_URI`, and `OIDC_SCOPE` are passed
as env vars; the image is expected to render them into `config.json` at
container startup (see `docker/.env.example` in the project).

If you want the SPA at `/` and the API at `/api` on the same host, point
`web.ingress` at the user-facing host and add a path-based rule that
forwards `/api` to the API Service — most ingress controllers handle this
with a single Ingress resource (use `nginx.ingress.kubernetes.io/rewrite-target`
or your controller's equivalent), or split it across two Ingress resources
on the same host.

## Ingress

```yaml
api:
  ingress:
    enabled: true
    className: nginx
    hosts:
      - host: api.relate.example.com
        paths:
          - path: /
            pathType: Prefix
    tls:
      - secretName: api-relate-tls
        hosts:
          - api.relate.example.com
```

## Render / lint

```bash
helm lint ./helm/relate-mail
helm template demo ./helm/relate-mail --set postgresql.auth.password=x | less
```
