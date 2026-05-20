# !커밋

`!커밋` 입력 시 이 커맨드를 실행합니다.

이 **Cursor 채팅 세션**에서 에이전트가 수정·추가한 파일만 스테이징하고 커밋합니다.

## 참조 규칙

- [git-workflow.mdc](../rules/git-workflow.mdc) — Safety, PowerShell, `!커밋` 메시지 형식

## 에이전트 실행 체크리스트

1. `git status`로 변경 파일 목록 확인
2. **이 세션에서 손댄 경로만** `git add` (세션에 없는 수정이 있으면 사용자에게 포함 여부 확인)
3. 커밋 메시지 작성 후 `git commit`
   - **제목**: `[AI] ` + 사용자 요청·작업 목적 한 줄 요약
   - **본문**(여러 주제/의미 단위일 때): 의도·이슈·배경만. 파일/라인 나열 금지
4. **push 하지 않음** (사용자가 따로 요청할 때만)

## Daily / 릴리즈 노트 (선택)

이 저장소에는 커밋 후 자동으로 Daily를 쓰는 `tools/release_notes.py` **가 없다**. 커밋과 릴리즈 노트를 묶고 싶다면:

- 수동으로 `99_ReleaseNote/YY.MM.DD_DAILY.md`에 요약을 적거나
- `/create-release-note`( [.cursor/commands/create-release-note.md](create-release-note.md) )로 사용자향 초안을 만든다.

Daily·초안 파일을 이번 세션에서 새로 만들었다면, **별도 커밋**으로 묶을지 사용자에게 확인한다(워크플로에 `git-workflow.mdc` 준수).

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
```

한 줄 제목만 쓸 때:

```powershell
git add "path/to/file"
git commit -m "[AI] 가구 카테고리 DesignationCategoryDef 라벨 보정"
```

## 주의

- `git reset --hard`, `git clean -fd` 등 파괴적 작업은 [git-workflow.mdc](../rules/git-workflow.mdc) 금지 목록 준수
