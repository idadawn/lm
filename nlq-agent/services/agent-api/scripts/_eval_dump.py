"""Read SLA eval JSON report and print case-by-case to stdout in UTF-8."""
import json
import sys

sys.stdout.reconfigure(encoding="utf-8")

with open(sys.argv[1], encoding="utf-8") as f:
    data = json.load(f)

for r in data["results"]:
    print("=" * 80)
    flag = "[PASS]" if r["passed"] else "[FAIL]"
    print(f"{flag}  {r['case_id']}  conf={r['confidence']}")
    print(f"Q: {r['question']}")
    for fail in r.get("failures", []):
        print(f"  - {fail}")
    cite = r.get("citations") or []
    if cite:
        print(f"citations: {cite[:3]}")
    print(f"A: {r['answer_excerpt']}")
    print()
