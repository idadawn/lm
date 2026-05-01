# Cron Setup — nlq-agent Nightly Bulk-Resync

Half of the F3 hybrid sync design (see `docs/decisions/2026-05-01-nlq-tracer-bullet-r4-r8-final/F3_NET_EVENT_DESIGN.md`). The .NET event-bus pushes incremental updates in real time (best-effort, no retry); this nightly cron reconciles any drift caused by transient HTTP failures or windows where nlq-agent was down.

## Setup

### 1. Set the admin token

In `nlq-agent/.env.production`:

```
SYNC_ADMIN_TOKEN=<rotate quarterly; ≥32 random chars>
```

Generate one:

```bash
python -c 'import secrets; print(secrets.token_urlsafe(32))'
```

### 2. Stage env for the cron job

`/etc/nlq-agent.env` (root-readable only, `chmod 600`):

```bash
NLQ_AGENT_BASE_URL=http://127.0.0.1:18100
SYNC_ADMIN_TOKEN=<same value as nlq-agent/.env.production>
```

### 3. Install crontab entry

As the user that runs nlq-agent (typically `nlq-agent` system user):

```cron
# m h dom mon dow command
0 2 * * * . /etc/nlq-agent.env && /opt/nlq-agent/scripts/resync_nlq_nightly.sh >> /var/log/nlq-resync.log 2>&1
```

The script (`nlq-agent/scripts/resync_nlq_nightly.sh`) is checked into the repo — typically symlinked or copied to `/opt/nlq-agent/scripts/` during deploy.

### 4. Verify

Trigger manually once:

```bash
. /etc/nlq-agent.env && /opt/nlq-agent/scripts/resync_nlq_nightly.sh
# Expected stdout:
#   [2026-05-02T02:00:00Z] resync-now → http://127.0.0.1:18100/api/v1/sync/resync-now
#   [2026-05-02T02:00:01Z] response: {"status":"ok","rules":12,"specs":5,"duration_ms":432}
```

### 5. Logrotate

`/etc/logrotate.d/nlq-resync`:

```
/var/log/nlq-resync.log {
    daily
    rotate 14
    compress
    missingok
    notifempty
    copytruncate
}
```

## Token rotation

Quarterly, in two steps to avoid downtime:

1. Update `nlq-agent/.env.production` with the new token, restart nlq-agent (`docker compose -f docker-compose.production.yml up -d`).
2. Update `/etc/nlq-agent.env` so the next cron run uses the new token.

The window between (1) and (2) only matters if a cron run fires during that minute. If concerned, run (2) immediately after (1) and the next cron will succeed.

## Troubleshooting

| Symptom | Likely cause | Fix |
|---|---|---|
| `curl: (22) The requested URL returned error: 401` | Token mismatch between `.env.production` and `/etc/nlq-agent.env` | Re-sync them; restart nlq-agent if the agent's value was just changed. |
| `Connection refused` | nlq-agent not running | `docker compose -f docker-compose.production.yml ps` |
| Long duration (> 5 min) timeout | Qdrant or MySQL slow | Tune timeout via `--max-time` in the script; check `/metrics` for backpressure. |
| Empty response body | Endpoint returned a streamed error | Check nlq-agent logs (correlation_id is in the structured JSON log lines) for the failed run. |

## Manual trigger (no cron needed)

For "I just edited a rule, sync now":

```bash
curl -fsS -X POST \
  -H "Authorization: Bearer $SYNC_ADMIN_TOKEN" \
  http://127.0.0.1:18100/api/v1/sync/resync-now
```

The endpoint is idempotent — it does a full bulk re-ingest each time. Cost is bounded by the size of `INTERMEDIATE_DATA_FORMULA` + `PRODUCT_SPEC` rows (typically <100 of each).
