"""Read-only heuristic review of tracked files and reachable Git history.

Reports categories and filenames, never matching secret values or email addresses.
Does not prove absence of secrets, resolve copyright, or rewrite history.
"""
import argparse
import json
import re
import subprocess
import zlib
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
PATTERNS = {
    "github_token": rb"\b(?:gh[pousr]_[A-Za-z0-9]{30,}|github_pat_[A-Za-z0-9_]{40,})\b",
    "aws_access_key": rb"\b(?:AKIA|ASIA)[A-Z0-9]{16}\b",
    "private_key": rb"-----BEGIN (?:RSA |EC |OPENSSH |DSA )?PRIVATE KEY-----",
    "assigned_secret_candidate": rb"(?i)(?:password|api[_-]?key|client[_-]?secret|access[_-]?token)\s*[:=]\s*[\"'][A-Za-z0-9_+/=-]{24,}[\"']",
    "personal_windows_path": rb"(?i)[A-Z]:[\\/]Users[\\/][^\\/\s\"']+",
    "machine_name": rb"\b(?:LAPTOP|DESKTOP)-[A-Z0-9]{4,}\b",
    "email_in_content": rb"\b[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,}\b",
    "local_drive_path": rb"(?i)\b[A-Z]:[\\/](?!Users[\\/]|Program Files[\\/])[^\r\n\"<>]{3,}",
}


def git(*args):
    return subprocess.check_output(["git", *args], cwd=ROOT)


def scan(content):
    # PNG image data is compressed pixel noise, not textual metadata. Pattern
    # matching it can invent emails and drive paths that do not exist in the file.
    if content.startswith(b"\x89PNG\r\n\x1a\n"):
        metadata = []
        offset = 8
        while offset + 12 <= len(content):
            size = int.from_bytes(content[offset:offset + 4], "big")
            kind = content[offset + 4:offset + 8]
            data = content[offset + 8:offset + 8 + size]
            if len(data) != size:
                raise ValueError("Truncated PNG chunk")
            if kind == b"tEXt":
                metadata.append(data)
            elif kind == b"zTXt":
                _, compressed = data.split(b"\0", 1)
                if compressed[:1] != b"\0":
                    raise ValueError("Unsupported PNG text compression")
                metadata.append(zlib.decompressobj().decompress(compressed[1:], 1_000_000))
            elif kind == b"iTXt":
                parts = data.split(b"\0", 1)
                rest = parts[1]
                text = rest[2:].split(b"\0", 2)[2]
                metadata.append(zlib.decompressobj().decompress(text, 1_000_000) if rest[0] else text)
            offset += size + 12
        content = b"\n".join(metadata)
    return [name for name, pattern in PATTERNS.items() if re.search(pattern, content)]


def run():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--report", default="PublicationAudit/report.json")
    args = parser.parse_args()
    tracked = [p.decode("utf-8") for p in git("ls-files", "-z").split(b"\0") if p]
    untracked = [p.decode("utf-8") for p in git("ls-files", "--others", "--exclude-standard", "-z").split(b"\0") if p]
    working = tracked + untracked
    current, binary_paths = [], []
    for name in working:
        content = (ROOT / name).read_bytes()
        if b"\0" in content[:8192]:
            binary_paths.append(name)
        hits = scan(content)
        if hits:
            current.append({"path": name, "categories": hits})
    object_names = {}
    for line in git("rev-list", "--objects", "--all").decode("utf-8").splitlines():
        oid, _, name = line.partition(" ")
        object_names[oid] = name
    proc = subprocess.Popen(
        ["git", "cat-file", "--batch"], cwd=ROOT,
        stdin=subprocess.PIPE, stdout=subprocess.PIPE,
    )
    history, blobs, oversized = [], 0, 0
    try:
        for oid, name in object_names.items():
            proc.stdin.write((oid + "\n").encode("ascii"))
            proc.stdin.flush()
            header = proc.stdout.readline().split()
            if len(header) != 3:
                raise RuntimeError("Unexpected Git object response")
            kind, size = header[1], int(header[2])
            # Consume every byte to preserve batch framing, even skipped objects.
            content = proc.stdout.read(size)
            if len(content) != size or proc.stdout.read(1) != b"\n":
                raise RuntimeError("Truncated Git object response")
            if kind != b"blob":
                continue
            blobs += 1
            if size > 8_000_000:
                oversized += 1
                continue
            hits = scan(content)
            if hits:
                history.append({"object": oid[:12], "path": name, "categories": hits})
    finally:
        proc.stdin.close()
        proc.stdout.close()
        proc.wait()
    authors = set(git("log", "--all", "--format=%ae%n%ce").splitlines())
    artifacts = [p for p in tracked if re.search(
        r"(?i)(?:^|/)(?:QAV[^/]*|QA|Library|Builds|UserSettings)/|(?:save[^/]*\.json|\.env|\.pem|\.pfx|\.log)$", p)]
    report = {
        "scope": "tracked and nonignored untracked working files plus unique blobs reachable from local --all refs",
        "head": git("rev-parse", "HEAD").decode().strip(),
        "tracked_files": len(tracked), "working_files_scanned": len(working), "reachable_blobs": blobs,
        "oversized_blobs_not_pattern_scanned": oversized,
        "current_findings": current, "history_findings": history,
        "tracked_sensitive_artifact_candidates": artifacts,
        "distinct_git_author_committer_emails": len(authors - {b""}),
        "binary_files_requiring_metadata_review": binary_paths,
        "limitations": [
            "Heuristics can miss secrets and produce false positives.",
            "Compressed binary metadata and external/remotely unreachable refs are not fully inspected.",
            "PNG pattern checks use text metadata chunks, not compressed pixels; compressed text is limited to 1 MB per chunk.",
            "Email values and matched content are intentionally omitted.",
            "No legal clearance, license adoption, publication or history rewrite is performed.",
        ],
    }
    target = (ROOT / args.report).resolve()
    if not target.is_relative_to(ROOT / "PublicationAudit"):
        raise ValueError("Reports must remain in ignored PublicationAudit directory")
    target.parent.mkdir(parents=True, exist_ok=True)
    target.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps({k: report[k] for k in (
        "tracked_files", "working_files_scanned", "reachable_blobs", "oversized_blobs_not_pattern_scanned",
        "distinct_git_author_committer_emails", "tracked_sensitive_artifact_candidates")}, ensure_ascii=False))
    print("Current categories:", sorted({c for f in current for c in f["categories"]}))
    print("History categories:", sorted({c for f in history for c in f["categories"]}))
    print("Private report saved under PublicationAudit/report.json")


if __name__ == "__main__":
    run()
