# !커밋

`!커밋` 입력 시 이 커맨드를 실행합니다.

이 **Cursor 채팅 세션**에서 에이전트가 수정·추가한 파일만 스테이징하고 커밋합니다.

## 참조 규칙

- [git-workflow.mdc](../rules/git-workflow.mdc) — Safety, PowerShell, `!커밋` 메시지 형식
- [tools/release_notes.py](../../tools/release_notes.py) — 커밋 후 Daily 기록(`append-daily`)

## 에이전트 실행 체크리스트

문서에 단계가 있어도 **에이전트가 실행하지 않으면 Daily는 생기지 않는다.** 아래 4번은 **커밋이 성공한 직후 필수**다(생략 금지).

1. `git status`로 변경 파일 목록 확인
2. **이 세션에서 손댄 경로만** `git add` (세션에 없는 수정이 있으면 사용자에게 포함 여부 확인)
3. 커밋 메시지 작성 후 `git commit`
   - **제목**: `[AI] ` + 사용자 요청·작업 목적 한 줄 요약
   - **본문**(여러 주제/의미 단위일 때): 의도·이슈·배경만. 파일/라인 나열 금지
4. **`append-daily` 필수** — 저장소 **루트**(`GloomyFurniture` git 루트)에서 실행한다. 서브폴더에서 돌리면 `ROOT`/git 대상이 어긋난다.

```powershell
Set-Location "d:\GitProject\GloomyFurniture"
python tools/release_notes.py append-daily
```

- 성공 시 stdout에 `99_ReleaseNote\yy.mm.dd_DAILY.md` 같은 **상대 경로 한 줄**이 나온다.
- **실패 시**(`python` 없음, `py`만 있음, venv, git 없음 등): stderr를 읽어 **사용자에게 그대로** 알린다. 커밋은 이미 끝난 상태이므로 **되돌리지 않는다.**
- Windows에서 `python`이 없으면 `py -3 tools/release_notes.py append-daily` 등 **동일 루트**에서 대안을 시도하고, 그래도 실패면 stderr를 넘긴다.
5. **push 하지 않음** (사용자가 따로 요청할 때만)

### Daily 파일을 커밋에 넣을지

`99_ReleaseNote/` 변경이 생기면 [git-workflow.mdc](../rules/git-workflow.mdc)에 맞게 **같은 커밋에 포함할지·별도 커밋할지** 사용자에게 확인한다.

### (선택) 훅으로 기계 보장

에이전트가 4번을 빼먹어도 쌓이게 하려면 `.git/hooks/post-commit`(또는 팀 공유용 `core.hooksPath` + 버전 관리 훅)에서 위와 **같은 명령**을 호출할 수 있다. 정책(같은 커밋 vs 다음 커밋)은 팀이 정한다.

## 수동 확인

문제일 때 루트에서 직접 한 번 실행해 원인을 본다.

```powershell
Set-Location "d:\GitProject\GloomyFurniture"
python tools/release_notes.py append-daily
```

## 커밋 메시지 예시 (PowerShell)

워크스페이스 루트에서. 경로는 세션에서 실제로 바꾼 파일로 교체.

```powershell
git status
git add "path/to/file1" "path/to/file2"

$msg = @"
[AI] Fire.xml 일부 ThingDef 설명·통계 정리

가열·연료 관련 수치가 바닐라와 겹치지 않도록 조정했다. 플레이어 혼선을 줄이기 위함이다.
"@
git commit -m $msg
python tools/release_notes.py append-daily
```

한 줄 제목만 쓸 때:

```powershell
git add "path/to/file"
git commit -m "[AI] 가구 카테고리 DesignationCategoryDef 라벨 보정"
python tools/release_notes.py append-daily
```

## 주의

- `git reset --hard`, `git clean -fd` 등 파괴적 작업은 [git-workflow.mdc](../rules/git-workflow.mdc) 금지 목록 준수
