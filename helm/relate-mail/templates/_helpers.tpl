{{/*
Expand the name of the chart.
*/}}
{{- define "relate-mail.name" -}}
{{- default .Chart.Name .Values.nameOverride | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{/*
Create a fully qualified app name (release-name + chart-name unless overridden).
*/}}
{{- define "relate-mail.fullname" -}}
{{- if .Values.fullnameOverride -}}
{{- .Values.fullnameOverride | trunc 63 | trimSuffix "-" -}}
{{- else -}}
{{- $name := default .Chart.Name .Values.nameOverride -}}
{{- if contains $name .Release.Name -}}
{{- .Release.Name | trunc 63 | trimSuffix "-" -}}
{{- else -}}
{{- printf "%s-%s" .Release.Name $name | trunc 63 | trimSuffix "-" -}}
{{- end -}}
{{- end -}}
{{- end -}}

{{/*
Per-component fullname: e.g. <release>-relate-mail-api
*/}}
{{- define "relate-mail.componentFullname" -}}
{{- $top := index . 0 -}}
{{- $component := index . 1 -}}
{{- printf "%s-%s" (include "relate-mail.fullname" $top) $component | trunc 63 | trimSuffix "-" -}}
{{- end -}}

{{/*
Common labels.
*/}}
{{- define "relate-mail.labels" -}}
helm.sh/chart: {{ printf "%s-%s" .Chart.Name .Chart.Version | replace "+" "_" | trunc 63 | trimSuffix "-" }}
{{ include "relate-mail.selectorLabels" . }}
app.kubernetes.io/version: {{ .Chart.AppVersion | quote }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
app.kubernetes.io/part-of: relate-mail
{{- end -}}

{{/*
Selector labels (release-scoped, no component).
*/}}
{{- define "relate-mail.selectorLabels" -}}
app.kubernetes.io/name: {{ include "relate-mail.name" . }}
app.kubernetes.io/instance: {{ .Release.Name }}
{{- end -}}

{{/*
Component labels — call as: include "relate-mail.componentLabels" (list . "api")
*/}}
{{- define "relate-mail.componentLabels" -}}
{{- $top := index . 0 -}}
{{- $component := index . 1 -}}
{{ include "relate-mail.labels" $top }}
app.kubernetes.io/component: {{ $component }}
{{- end -}}

{{- define "relate-mail.componentSelectorLabels" -}}
{{- $top := index . 0 -}}
{{- $component := index . 1 -}}
{{ include "relate-mail.selectorLabels" $top }}
app.kubernetes.io/component: {{ $component }}
{{- end -}}

{{/*
ServiceAccount name to use.
*/}}
{{- define "relate-mail.serviceAccountName" -}}
{{- if .Values.serviceAccount.create -}}
{{- default (include "relate-mail.fullname" .) .Values.serviceAccount.name -}}
{{- else -}}
{{- default "default" .Values.serviceAccount.name -}}
{{- end -}}
{{- end -}}

{{/*
Resolve the image reference for a given component.
Usage: include "relate-mail.image" (dict "top" . "component" "api" "config" .Values.api)
*/}}
{{- define "relate-mail.image" -}}
{{- $top := .top -}}
{{- $component := .component -}}
{{- $config := .config -}}
{{- $repo := $config.image.repository -}}
{{- if not $repo -}}
{{- $repo = printf "%s/%s-%s" $top.Values.image.registry $top.Values.image.repository $component -}}
{{- end -}}
{{- $tag := default (default $top.Chart.AppVersion $top.Values.image.tag) $config.image.tag -}}
{{- printf "%s:%s" $repo $tag -}}
{{- end -}}

{{/*
Name of the Secret holding the database connection string.
Always points at the chart-managed secret unless the user supplied an existingSecret.
*/}}
{{- define "relate-mail.dbSecretName" -}}
{{- if .Values.database.existingSecret -}}
{{- .Values.database.existingSecret -}}
{{- else -}}
{{- printf "%s-db" (include "relate-mail.fullname" .) -}}
{{- end -}}
{{- end -}}

{{- define "relate-mail.dbSecretKey" -}}
{{- if .Values.database.existingSecret -}}
{{- .Values.database.existingSecretKey | default "connectionString" -}}
{{- else -}}
connectionString
{{- end -}}
{{- end -}}

{{/*
Build the database connection string when the chart manages the secret.
Priority:
  1) explicit database.connectionString
  2) host/port/database/username/password (database.* values)
  3) in-chart postgresql with its auth values (when postgresql.enabled)
*/}}
{{- define "relate-mail.dbConnectionString" -}}
{{- if .Values.database.connectionString -}}
{{- .Values.database.connectionString -}}
{{- else if .Values.database.host -}}
{{- printf "Host=%s;Port=%d;Database=%s;Username=%s;Password=%s" .Values.database.host (int .Values.database.port) .Values.database.database .Values.database.username .Values.database.password -}}
{{- else if .Values.postgresql.enabled -}}
{{- $host := printf "%s-postgresql" (include "relate-mail.fullname" .) -}}
{{- printf "Host=%s;Port=%d;Database=%s;Username=%s;Password=%s" $host (int .Values.postgresql.service.port) .Values.postgresql.auth.database .Values.postgresql.auth.username .Values.postgresql.auth.password -}}
{{- else -}}
{{- fail "database.connectionString, database.existingSecret, database.host, or postgresql.enabled must be configured" -}}
{{- end -}}
{{- end -}}

{{/*
Internal API key secret name + key, mirroring the DB pattern.
*/}}
{{- define "relate-mail.internalSecretName" -}}
{{- if .Values.internal.existingSecret -}}
{{- .Values.internal.existingSecret -}}
{{- else -}}
{{- printf "%s-internal" (include "relate-mail.fullname" .) -}}
{{- end -}}
{{- end -}}

{{- define "relate-mail.internalSecretKey" -}}
{{- if .Values.internal.existingSecret -}}
{{- .Values.internal.existingSecretKey | default "internalApiKey" -}}
{{- else -}}
internalApiKey
{{- end -}}
{{- end -}}

{{/*
Security/authentication-salt secret name + key, mirroring the internal pattern.
*/}}
{{- define "relate-mail.securitySecretName" -}}
{{- if .Values.security.existingSecret -}}
{{- .Values.security.existingSecret -}}
{{- else -}}
{{- printf "%s-security" (include "relate-mail.fullname" .) -}}
{{- end -}}
{{- end -}}

{{- define "relate-mail.securitySecretKey" -}}
{{- if .Values.security.existingSecret -}}
{{- .Values.security.existingSecretKey | default "authenticationSalt" -}}
{{- else -}}
authenticationSalt
{{- end -}}
{{- end -}}

{{/*
Resolve the authentication salt. Priority:
  1) explicit security.authenticationSalt
  2) existing chart-managed Secret (preserves the salt across upgrades)
  3) freshly-generated random 32-char alphanum
A regenerated salt only resets rate-limit counters; it doesn't invalidate user
auth or sessions, so an occasional rotation (e.g. on a Pulumi-style fresh
template render) is harmless.
*/}}
{{- define "relate-mail.authenticationSalt" -}}
{{- if .Values.security.authenticationSalt -}}
{{- .Values.security.authenticationSalt -}}
{{- else if .Values.security.existingSecret -}}
{{- /* Caller-supplied secret; we don't read it, the env var binds at runtime. */ -}}
{{- else -}}
{{- $secretName := printf "%s-security" (include "relate-mail.fullname" .) -}}
{{- $existing := lookup "v1" "Secret" .Release.Namespace $secretName -}}
{{- if and $existing $existing.data (index $existing.data "authenticationSalt") -}}
{{- index $existing.data "authenticationSalt" | b64dec -}}
{{- else -}}
{{- randAlphaNum 32 -}}
{{- end -}}
{{- end -}}
{{- end -}}

{{/*
Outbound relay password secret name/key.
*/}}
{{- define "relate-mail.outboundSecretName" -}}
{{- if .Values.outbound.existingSecret -}}
{{- .Values.outbound.existingSecret -}}
{{- else -}}
{{- printf "%s-outbound" (include "relate-mail.fullname" .) -}}
{{- end -}}
{{- end -}}

{{- define "relate-mail.outboundSecretKey" -}}
{{- if .Values.outbound.existingSecret -}}
{{- .Values.outbound.existingSecretKey | default "relayPassword" -}}
{{- else -}}
relayPassword
{{- end -}}
{{- end -}}

{{/*
True when an outbound relay password is reachable (either inline or via existingSecret).
*/}}
{{- define "relate-mail.outboundHasPassword" -}}
{{- if or .Values.outbound.relayPassword .Values.outbound.existingSecret -}}true{{- end -}}
{{- end -}}

{{/*
Shared OutboundMail ConfigMap name.
*/}}
{{- define "relate-mail.outboundConfigName" -}}
{{- printf "%s-outbound" (include "relate-mail.fullname" .) -}}
{{- end -}}

{{/*
Postgres password secret name/key.
*/}}
{{- define "relate-mail.pgSecretName" -}}
{{- if .Values.postgresql.auth.existingSecret -}}
{{- .Values.postgresql.auth.existingSecret -}}
{{- else -}}
{{- printf "%s-postgresql" (include "relate-mail.fullname" .) -}}
{{- end -}}
{{- end -}}

{{- define "relate-mail.pgSecretKey" -}}
{{- if .Values.postgresql.auth.existingSecret -}}
{{- .Values.postgresql.auth.existingSecretKey | default "password" -}}
{{- else -}}
password
{{- end -}}
{{- end -}}
