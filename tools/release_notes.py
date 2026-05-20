#!/usr/bin/env python3
"""
Daily / 릴리즈 노트: 99_ReleaseNote 에 일별 커밋 기록 후, Daily 를 모아 릴리즈 노트 초안 생성.

저장소 루트에서:
  python tools/release_notes.py append-daily    # HEAD 커밋을 해당 일 Daily 파일에 추가
  python tools/release_notes.py create-release    # 모든 *_DAILY.md 를 읽어 실행일 기준 *_RELEASE_NOTES_DRAFT.md 작성 (해시·시각 메타는 초안에 넣지 않음)
"""
from __future__ import annotations

import argparse
import re
import subprocess
import sys
from datetime import datetime
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
NOTE_DIR = ROOT / "99_ReleaseNote"
DAILY_SUFFIX = "_DAILY.md"
# 수동 편집용 *_RELEASE_NOTES.md 와 충돌을 피하기 위해 초안은 별도 접미사를 쓴다.
RELEASE_DRAFT_SUFFIX = "_RELEASE_NOTES_DRAFT.md"


def die(msg: str, code: int = 1) -> None:
    print(msg, file=sys.stderr)
    raise SystemExit(code)


def run_out(cmd: list[str], *, cwd: Path) -> str:
    r = subprocess.run(
        cmd,
        cwd=cwd,
        capture_output=True,
        text=True,
        encoding="utf-8",
        errors="replace",
    )
    if r.returncode != 0:
        die((r.stderr or "").strip() or f"명령 실패: {cmd}")
    return (r.stdout or "").strip()


def git_head_commit_lines(repo: Path) -> tuple[str, str, str, str]:
    """hash, iso date, subject, body (may be empty)."""
    raw = run_out(
        [
            "git",
            "log",
            "-1",
            "--format=%H%n%ci%n%s%n%b",
        ],
        cwd=repo,
    )
    lines = raw.split("\n")
    if len(lines) < 3:
        die("git log -1 결과가 비정상입니다.")
    h, ci, subj = lines[0], lines[1], lines[2]
    body = "\n".join(lines[3:]).rstrip()
    return h, ci, subj, body


def daily_filename_for_commit(repo: Path) -> str:
    """커밋일(커미터 기준, 로컬 표기) YY.MM.DD_DAILY.md."""
    stamp = run_out(
        ["git", "log", "-1", "--format=%cd", "--date=format:%y.%m.%d"],
        cwd=repo,
    )
    if not re.fullmatch(r"\d{2}\.\d{2}\.\d{2}", stamp):
        die(f"날짜 형식 예상과 다름: {stamp!r}")
    return f"{stamp}{DAILY_SUFFIX}"


def yy_mm_dd_today() -> str:
    return datetime.now().strftime("%y.%m.%d")


def append_daily(repo: Path = ROOT) -> Path:
    NOTE_DIR.mkdir(parents=True, exist_ok=True)
    name = daily_filename_for_commit(repo)
    path = NOTE_DIR / name
    _, _, subj, body = git_head_commit_lines(repo)
    block_lines = [
        "---",
        "",
        f"**제목:** {subj}",
        "",
    ]
    if body:
        block_lines.append(body)
        block_lines.append("")
    block = "\n".join(block_lines)
    if path.is_file():
        text = path.read_text(encoding="utf-8", errors="replace").rstrip()
        if text and not text.endswith("\n"):
            text += "\n"
        path.write_text(text + "\n" + block + "\n", encoding="utf-8")
    else:
        day_label = name.replace(DAILY_SUFFIX, "")
        header = f"# Daily — {day_label}\n\n## 커밋 기록\n\n"
        path.write_text(header + block + "\n", encoding="utf-8")
    print(str(path.relative_to(repo)))
    return path


_DAILY_FILE_RE = re.compile(r"^(\d{2}\.\d{2}\.\d{2})" + re.escape(DAILY_SUFFIX) + r"$")
# append-daily 가 예전에 쓰던 `### `7자리` · 커미터일시` 줄 — create-release 시 초안에서 제거
_LEGACY_COMMIT_META_LINE = re.compile(r"^### `[0-9a-f]{7}` · .+$")


def filter_daily_for_release(raw: str) -> str:
    """릴리즈 초안에 붙일 때: 커밋 해시·시각 헤딩 줄만 제거."""
    lines = [ln for ln in raw.splitlines() if not _LEGACY_COMMIT_META_LINE.match(ln)]
    return "\n".join(lines).rstrip()


def list_daily_files() -> list[Path]:
    if not NOTE_DIR.is_dir():
        return []
    out: list[Path] = []
    for p in sorted(NOTE_DIR.iterdir()):
        if p.is_file() and _DAILY_FILE_RE.match(p.name):
            out.append(p)
    return out


def create_release(exec_date: str, repo: Path = ROOT, out_path: Path | None = None) -> Path:
    if not re.fullmatch(r"\d{2}\.\d{2}\.\d{2}", exec_date):
        die("--date 는 YY.MM.DD 형식이어야 합니다.")
    dailies = list_daily_files()
    if not dailies:
        die(f"Daily 파일이 없습니다: {NOTE_DIR}/*{DAILY_SUFFIX}")
    NOTE_DIR.mkdir(parents=True, exist_ok=True)
    if out_path is None:
        out_path = NOTE_DIR / f"{exec_date}{RELEASE_DRAFT_SUFFIX}"
    else:
        out_path = Path(out_path)
        if not out_path.is_absolute():
            out_path = (repo / out_path).resolve()
    parts: list[str] = [
        f"# Ratkin 릴리즈 노트 (집계일: {exec_date})",
        "",
        "> 이 파일은 `*_DAILY.md` 기록을 바탕으로 `tools/release_notes.py create-release` 로 생성·갱신된 초안입니다. 배포 전 문구·분류를 다듬으세요.",
        "",
        "### 일별 상세 (Daily 원문)",
        "",
    ]
    for dp in dailies:
        day = _DAILY_FILE_RE.match(dp.name)
        label = day.group(1) if day else dp.stem
        parts.append(f"#### {label}")
        parts.append("")
        filtered = filter_daily_for_release(
            dp.read_text(encoding="utf-8", errors="replace")
        )
        parts.append(filtered)
        parts.append("")
        parts.append("---")
        parts.append("")
    out_path.write_text("\n".join(parts).rstrip() + "\n", encoding="utf-8")
    print(str(out_path.relative_to(repo)))
    return out_path


def main() -> None:
    ap = argparse.ArgumentParser(description="99_ReleaseNote Daily / 릴리즈 노트 도구")
    sub = ap.add_subparsers(dest="cmd", required=True)

    p_add = sub.add_parser("append-daily", help="HEAD 커밋을 해당 일 Daily 파일에 추가")
    p_add.add_argument(
        "--repo",
        type=Path,
        default=ROOT,
        help="Git 저장소 루트 (기본: 자동)",
    )

    p_rel = sub.add_parser(
        "create-release",
        help="모든 *_DAILY.md 를 읽어 실행일(또는 --date) 기준 RELEASE_NOTES 작성",
    )
    p_rel.add_argument(
        "--date",
        dest="exec_date",
        default=None,
        help="집계·파일명에 쓸 날짜 YY.MM.DD (기본: 오늘)",
    )
    p_rel.add_argument(
        "--repo",
        type=Path,
        default=ROOT,
        help="저장소 루트 (기본: 자동)",
    )
    p_rel.add_argument(
        "-o",
        "--output",
        type=Path,
        default=None,
        help="출력 파일 경로 (상대 시 저장소 루트 기준). 미지정 시 99_ReleaseNote/YY.MM.DD_RELEASE_NOTES_DRAFT.md",
    )

    args = ap.parse_args()
    if args.cmd == "append-daily":
        append_daily(Path(args.repo).resolve())
    elif args.cmd == "create-release":
        d = args.exec_date or yy_mm_dd_today()
        create_release(d, Path(args.repo).resolve(), out_path=args.output)
    else:
        die(f"알 수 없는 서브커맨드: {args.cmd}")


if __name__ == "__main__":
    main()
