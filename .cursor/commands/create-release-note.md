---
description: Daily 노트를 읽고 사용자향 릴리즈 노트 초안 작성 (Gloomy Furniture)
---

# /create-release-note

사용자가 `/create-release-note`를 실행하면, 에이전트가 `99_ReleaseNote/*_DAILY.md`를 **전부 읽고 요약**하여 릴리즈 노트 초안을 작성한다.

## 핵심 원칙

- 릴리즈 노트는 **모드 사용자**가 읽는 문서다.
- 커밋 제목, 구현 세부사항, 코드 변경 내역은 **쓰지 않는다**.
- **무엇이 바뀌었는지 / 무엇이 수정되었는지**만 간결하게 적는다.

## 에이전트 실행 순서

1. `99_ReleaseNote` 폴더의 모든 `*_DAILY.md` 파일을 읽는다.
2. Daily 내용을 분석하여 다음 카테고리로 분류한다:
   - **버그 수정** — 고쳐진 문제
   - **변경** — 밸런스 조정, 동작 변경 등
   - **추가** — 새로 들어간 기능·콘텐츠
   - 카테고리가 비어 있으면 해당 섹션은 생략한다.
3. 각 항목은 **한 줄 요약**(사용자 관점)으로 쓴다.
   - 좋은 예: `벽 뒤 가구가 그림자·클릭 판정에서 잘못 잡히던 현상 수정`
   - 나쁜 예: `[AI] ThingDef comps에 OcclusionOffset 패치 추가 및 Fire.xml 3블록 수정`
4. 결과를 `99_ReleaseNote/YY.MM.DD_RELEASE_NOTES_DRAFT.md`에 저장한다.
   - `YY.MM.DD`는 커맨드 실행일 기준(사용자가 날짜를 지정하면 해당 날짜 사용).
5. 저장 후 경로를 알린다.

## 출력 포맷 예시

```markdown
# Gloomy Furniture 릴리즈 노트 — YY.MM.DD

## 버그 수정
- 특정 조명이 꺼진 뒤에도 밝기 버프가 남던 문제 수정
- 다른 모드 지형과 겹칠 때 바닥 텍스처가 깨지던 경우 일부 수정

## 변경
- 일부 가구 건설 재료·작업량 조정
- 한국어 라벨·설명 문구 다듬기

## 추가
- 새 조명 ThingDef 및 연구 선행 조건 추가
```

## 참고

- Daily 원문·기계 집계는 `tools/release_notes.py`(`append-daily` / `create-release`)와 `99_ReleaseNote/*_DAILY.md`로 관리한다. 이 커맨드는 그걸 바탕으로 **사용자향 문장**을 다시 쓰는 단계다.
- Daily 파일이 하나도 없으면 작성할 내용이 없다고 알린다.
