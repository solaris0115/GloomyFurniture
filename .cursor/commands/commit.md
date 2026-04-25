# commit / !커밋

`commit` 또는 `!커밋` 입력 시 이 커맨드를 실행합니다.

이 **Cursor 채팅 세션**에서 에이전트가 수정·추가한 파일만 스테이징하고 커밋합니다.

## 참조 규칙

- [git-workflow.mdc](../rules/git-workflow.mdc) — Safety, PowerShell, 커밋 메시지 형식

## 에이전트 실행 체크리스트

1. `git status`로 변경 파일 목록 확인
2. **이 세션에서 손댄 경로만** `git add` (세션에 없는 수정이 있으면 사용자에게 포함 여부 확인)
3. 커밋 메시지 작성 후 `git commit`
   - **제목**: `[AI] ` + 사용자 요청·작업 목적 한 줄 요약
   - **본문**(여러 주제/의미 단위일 때): 의도·이슈·배경만. 파일/라인 나열 금지
4. **push 하지 않음** (사용자가 따로 요청할 때만)

## 커밋 메시지 예시 (PowerShell)

워크스페이스 루트에서. 경로는 세션에서 실제로 바꾼 파일로 교체.

```powershell
git status
git add "path/to/file1" "path/to/file2"

$msg = @"
[AI] TerrainDef 카테고리 정리 및 1.6 호환 보강

사용자 요청에 따라 지형 Def 분류를 맞추고 1.6 스키마에 맞게 조정했다.
"@
git commit -m $msg
```

한 줄 제목만 쓸 때:

```powershell
git add "path/to/file"
git commit -m "[AI] JukeBox ThingComp Tick 시그니처 1.6 대응"
```

## 주의

- `git reset --hard`, `git clean -fd` 등 파괴적 작업은 [git-workflow.mdc](../rules/git-workflow.mdc) 금지 목록 준수
