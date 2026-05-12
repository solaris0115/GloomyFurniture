# RimWorld Bookcase Building_Bookcase ThingOwner storage graphics IThingHolder GloomyFurniture GL_Bookshelf

바닐라 림월드 책장은 **전용 C# 클래스 `Building_Bookcase`** 한 덩어리로 동작한다. `CompThingContainer` 같은 **보관 전용 컴포넌트로 쪼개진 구조가 아니다**. 건물 본체가 `ThingOwner<Book>`를 들고 있고, 운반·저장 UI·그리기는 이 클래스와 인터페이스 구현으로 연결된다.

---

## 1. 적재(보관) 관리

| 요소 | 역할 |
|------|------|
| `ThingOwner<Book> innerContainer` | 실제 책(`Book`)만 담는 컨테이너. 생성자에서 `new ThingOwner<Book>(this, false, LookMode.Deep, true)`로 붙음. |
| `StorageSettings settings` | 플레이어 저장 우선순위·필터(기본은 `Books` 카테고리 등). |
| `StorageGroup storageGroup` | 여러 책장을 하나의 저장 그룹으로 묶을 때 그룹 설정 사용. |
| `ThingDef.building` | `maxItemsInCell`, `fixedStorageSettings`, `defaultStorageSettings`, `storageGroupTag` 등으로 용량·필터 상한 정의. |

- **최대 권수**: `MaximumBooks = def.building.maxItemsInCell * def.size.Area` (칸당 최대 × 건물 면적).
- **수락 조건** (`Accepts`): 권수 상한일 때는 **이미 내부에 있는 그 책**만 재수락 허용(재배치 등). 그 외에는 `GetStoreSettings().AllowedToAccept` + `innerContainer.CanAcceptAnyOf`.
- **남은 칸**: `SpaceRemainingFor`는 단순히 `MaximumBooks - HeldBooks.Count`.
- **직렬화**: `ExposeData`에서 `innerContainer`, `settings`, `storageGroup` 스크라이브.
- **디스폰**: 저장 그룹에서 빠지고, `WillReplace`가 아니면 `innerContainer.TryDropAll`로 책을 바닥에 떨굼.

운반 시스템 연동은 클래스 선언에 나온 인터페이스들로 이루어진다: `IThingHolder`, `IHaulDestination`, `IHaulSource`, `IStoreSettingsParent`, `IStorageGroupMember`, `ISearchableContents`, `IHaulEnroute`, `IThingHolderEvents<Book>` 등. 즉 **“건물이 ThingHolder이면서 저장소”** 패턴이다.

---

## 2. 그래픽은 어디서 그리나

1. **책장 본체**: 일반 `Building` 그래프(`graphicData`의 `Graphic_Multi` 등). `DrawAt`에서 `drawLoc`을 살짝 내린 뒤 `base.DrawAt(drawLoc, flip)`로 그림.
2. **꽂혀 있는 책 한 권씩**: 같은 `DrawAt` 안에서 `HeldBooks`를 순회하며 **`book.VerticalGraphic.Draw(loc, opposite, this, 0f)`** 로 세로 그래픽을 배치. 위치는 회전(`Rot4`), `RotOffsets`(방향별 미세 오프셋), 책 두께 상수(`BookWidthEastWest` / `BookWidthNorthSouth`), 인덱스 `i`로 선형 배치.
3. **북엔드(책 끝 장식)**: `def.building.bookendGraphicEast` / `bookendGraphicNorth`가 있을 때만, 남쪽이 아닌 방향에서 `BookendGraphicEast` 또는 `BookendGraphicNorth`로 추가 드로우. (남쪽(`South`)이면 북엔드 미표시.)

`drawerType`은 바닐라 `BookcaseBase`가 **`RealtimeOnly`** — 매 프레임(또는 실시간 경로)으로 그리기 갱신되는 쪽에 맞춰져 있어, 책 꽂힌 모습이 바로 반영된다.

---

## 3. “컴포넌트 구조”인가

- **핵심 보관 로직**: `Building_Bookcase` **필드 + 인터페이스** (컴프가 아님).
- **ThingDef `comps`**: 바닐라 책장 베이스는 `ShelfBase` 계통에서 오는 공통 컴프만 있고, **보관용 `CompProperties_ThingContainer`류는 없음**.
- **모드 `GL_Bookshelf`**: `thingClass`가 **`Building_Bookcase`** 그대로이므로 위와 동일. 추가로 `CompProperties_Facility`(연구 속도 등)만 얹혀 있음.
- **UI 탭**: `inspectorTabs`에 **`ITab_ContentsBooks`** — 내용물 리스트는 `Bookcase.GetDirectlyHeldThings()`를 그대로 노출.

---

## 4. 방·연구·기타 게임 시스템과의 연결 (참고)

- `RoomStatWorker_ReadingBonus` 등이 `CellsFilledPercentage`(칸별 채움 비율)로 읽기 보너스에 반영.
- `StatWorker_RoomReadingBonus`는 `thingClass`가 `Building_Bookcase` 계열인지로 시설 판별.
- 연구대 `PlaceWorker_DrawLinesToBookcasesInRoom` 등으로 방 안 책장과 시각적 연결.

---

## 5. 이 저장소(GloomyFurniture)와의 차이

`GL_Bookshelf`는 바닐라와 동일하게 **`Building_Bookcase` + `ITab_ContentsBooks` + `building` 저장 설정**을 쓴다. 바닐라 넓은 책장은 `size (2,1)`에 `maxItemsInCell` 5라서 10권인 반면, 모드 정의는 **`size (1,1)`에 `maxItemsInCell` 10**이라 한 칸에 최대 10권이다.

모드 XML에는 **`bookendGraphicEast` / `bookendGraphicNorth` 블록이 없음** — 북엔드 그래픽은 선택적이라 없으면 `DrawAt`에서 해당 분기만 스킵되고, 책장 본체 텍스처 + 책 `VerticalGraphic`만 보인다.

모드 쪽 옷장(`WardrobeC`) 주석에도 적혀 있듯이, **“슬롯 그룹 없이 `ThingOwner`에만 보관 + 저장 필터·저장 그룹·운반 목적지”** 패턴이 책장과 유사하다.

---

## 6. 소스 위치 (로컬 기준)

| 구분 | 경로 |
|------|------|
| 바닐라 클래스 | `RimworldSource/RimWorld/Building_Bookcase.cs` |
| 내용물 탭 | `RimworldSource/RimWorld/ITab_ContentsBooks.cs` |
| 바닐라 ThingDef | `RimworldData/Core/Defs/ThingDefs_Buildings/Buildings_Furniture.xml` (`BookcaseBase`, `Bookcase`, `BookcaseSmall`) |
| 모드 책장 Def | `GloomyFurniture/1.6/Defs/Building_Furnitures.xml` (`GL_Bookshelf`) |
