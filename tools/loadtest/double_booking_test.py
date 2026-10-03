"""Double-booking load test.

For each round, N distinct users fire a purchase request for the SAME seat at the same
instant (threads released together by a barrier, one pre-opened keep-alive connection each).
Exactly one request must succeed (201); every other one must be rejected (409).

Usage:
    python double_booking_test.py --base http://localhost:5080 --event 1 --section 2 \
        --users 100 --seats 5 --concurrency 50 100

Needs only the Python standard library. Run it against a throw-away database: every round
leaves one sold ticket behind.
"""
import argparse
import http.client
import json
import statistics
import threading
import time
from collections import Counter
from urllib.parse import urlparse

PASSWORD = "Passw0rd!"


def call(conn, method, path, body=None, token=None):
    headers = {"Content-Type": "application/json"}
    if token:
        headers["Authorization"] = f"Bearer {token}"
    conn.request(method, path, json.dumps(body) if body is not None else None, headers)
    resp = conn.getresponse()
    data = resp.read()
    return resp.status, data


def get_token(host, port, i):
    email = f"load{i}@loadtest.example"
    conn = http.client.HTTPConnection(host, port, timeout=60)
    status, data = call(conn, "POST", "/api/auth/register",
                        {"email": email, "fullName": f"Load User {i}", "password": PASSWORD})
    if status != 200:  # already registered on a previous run
        status, data = call(conn, "POST", "/api/auth/login", {"email": email, "password": PASSWORD})
    conn.close()
    assert status == 200, (status, data)
    return json.loads(data)["token"]


def pct(values, p):
    s = sorted(values)
    return s[min(len(s) - 1, max(0, round(p / 100 * len(s)) - 1))]


def run_round(host, port, tokens, event_id, seat_id, n):
    barrier = threading.Barrier(n)
    results = [None] * n

    def worker(k):
        conn = http.client.HTTPConnection(host, port, timeout=120)
        conn.connect()
        barrier.wait()
        t0 = time.perf_counter()
        try:
            status, data = call(conn, "POST", "/api/tickets/purchase/reserved-seat",
                                {"eventId": event_id, "eventSeatId": seat_id}, tokens[k])
        except Exception as ex:  # noqa: BLE001 - count transport failures, don't hide them
            status, data = -1, str(ex).encode()
        results[k] = (status, (time.perf_counter() - t0) * 1000, data)
        conn.close()

    threads = [threading.Thread(target=worker, args=(k,)) for k in range(n)]
    for t in threads:
        t.start()
    for t in threads:
        t.join()
    return results


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--base", default="http://localhost:5080")
    ap.add_argument("--event", type=int, default=1)
    ap.add_argument("--section", type=int, default=2, help="reserved-seating event section")
    ap.add_argument("--users", type=int, default=100)
    ap.add_argument("--seats", type=int, default=5, help="distinct seats per concurrency level")
    ap.add_argument("--concurrency", type=int, nargs="+", default=[50, 100])
    ap.add_argument("--out", default="results.json")
    a = ap.parse_args()

    u = urlparse(a.base)
    host, port = u.hostname, u.port
    conn = http.client.HTTPConnection(host, port, timeout=60)
    status, data = call(conn, "GET", f"/api/events/{a.event}/seats?eventSectionId={a.section}")
    assert status == 200
    free = [s["eventSeatId"] for s in json.loads(data)]
    need = a.seats * len(a.concurrency)
    assert len(free) >= need, f"only {len(free)} free seats, need {need}"
    print(f"{len(free)} free seats; registering/logging in {a.users} users...")
    tokens = [get_token(host, port, i) for i in range(a.users)]

    seat_iter = iter(free)
    rounds, all_ok_latencies, all_latencies = [], [], []
    for n in a.concurrency:
        assert n <= a.users
        for r in range(a.seats):
            seat = next(seat_iter)
            res = run_round(host, port, tokens, a.event, seat, n)
            codes = Counter(s for s, _, _ in res)
            lat = [ms for _, ms, _ in res]
            row = {
                "concurrency": n, "seat": seat, "codes": dict(codes),
                "success_201": codes.get(201, 0), "conflict_409": codes.get(409, 0),
                "other": sum(v for k, v in codes.items() if k not in (201, 409)),
                "avg_ms": round(statistics.mean(lat), 1), "p95_ms": round(pct(lat, 95), 1),
                "max_ms": round(max(lat), 1),
            }
            others = [(s, d[:150]) for s, _, d in res if s not in (201, 409)][:2]
            if others:
                row["other_samples"] = [(s, d.decode(errors="replace")) for s, d in others]
            rounds.append(row)
            all_latencies += lat
            print(f"N={n:>3} seat={seat:>4} -> 201:{row['success_201']} 409:{row['conflict_409']} "
                  f"other:{row['other']}  avg={row['avg_ms']}ms p95={row['p95_ms']}ms max={row['max_ms']}ms")

    total_req = sum(r["concurrency"] for r in rounds)
    summary = {
        "rounds": len(rounds), "total_requests": total_req,
        "rounds_with_exactly_one_success": sum(r["success_201"] == 1 for r in rounds),
        "total_201": sum(r["success_201"] for r in rounds),
        "total_409": sum(r["conflict_409"] for r in rounds),
        "total_other": sum(r["other"] for r in rounds),
        "avg_ms": round(statistics.mean(all_latencies), 1),
        "p95_ms": round(pct(all_latencies, 95), 1),
        "seats_tested": [r["seat"] for r in rounds],
    }
    print(json.dumps(summary, indent=2))
    with open(a.out, "w") as f:
        json.dump({"summary": summary, "rounds": rounds}, f, indent=2)


if __name__ == "__main__":
    main()
