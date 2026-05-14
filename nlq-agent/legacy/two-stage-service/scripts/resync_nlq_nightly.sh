#!/usr/bin/env bash
# Nightly bulk-resync of nlq-agent's Qdrant index from MySQL truth.
#
# Hybrid design (see docs/decisions/.../F3_NET_EVENT_DESIGN.md):
# - The .NET event-bus pushes incremental updates in real time (best-effort).
# - This cron runs at 02:00 host-local as a safety net to reconcile drift.
#
# Crontab entry (host):
#   0 2 * * * /opt/nlq-agent/scripts/resync_nlq_nightly.sh >> /var/log/nlq-resync.log 2>&1
#
# Required env (typically sourced from /etc/nlq-agent.env):
#   NLQ_AGENT_BASE_URL  e.g. http://127.0.0.1:18100
#   SYNC_ADMIN_TOKEN    bearer token (matches nlq-agent's settings.sync_admin_token)
set -euo pipefail

: "${NLQ_AGENT_BASE_URL:?NLQ_AGENT_BASE_URL not set}"
: "${SYNC_ADMIN_TOKEN:?SYNC_ADMIN_TOKEN not set}"

ENDPOINT="${NLQ_AGENT_BASE_URL%/}/api/v1/sync/resync-now"
START_TS="$(date -u +%Y-%m-%dT%H:%M:%SZ)"

echo "[$START_TS] resync-now → $ENDPOINT"

response="$(
  curl --fail --silent --show-error \
       --max-time 300 \
       -X POST \
       -H "Authorization: Bearer ${SYNC_ADMIN_TOKEN}" \
       -H "Content-Type: application/json" \
       "$ENDPOINT"
)"

END_TS="$(date -u +%Y-%m-%dT%H:%M:%SZ)"
echo "[$END_TS] response: $response"
