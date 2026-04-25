# GloomyFurniture 1.6 XML 변경 보고서

- **기준**: 로컬 워킹 트리 대비 `HEAD` (`git diff GloomyFurniture/1.6/`)
- **변경 파일 수**: 3개 (`Defs`만 해당)
  - `GloomyFurniture/1.6/Defs/Building_Furnitures.xml`
  - `GloomyFurniture/1.6/Defs/Building_WindowWall.xml`
  - `GloomyFurniture/1.6/Defs/Fire.xml`
- **defName별 요약** 아래 표와 섹션 참고.

---

## 요약 표 (defName → 변경 성격)

| defName | 파일 | 변경 요약 |
|---------|------|-----------|
| `GL_TablewareShelf` | Building_Furnitures.xml | `graphicData`에 남/북 드로우 오프셋 추가 |
| `RGK_Wall` | Building_WindowWall.xml | 바닐라 벽에 가깝게 태그·스탯·건축·명상·피해배율 등 대규모 정렬 |
| `GL_Wall` | Building_WindowWall.xml | `RGK_Wall`과 유사 + 재료 지형·`stuffCategories` 순서 등 |
| `RGK_VentWall` | Building_WindowWall.xml | `placingDraggableDimensions` 제거, `supportsWallAttachments` 추가 |
| `GL_VentWall` | Building_WindowWall.xml | 동일 |
| `RGK_WindowWall` | Building_WindowWall.xml | 동일 |
| `GL_WindowWall` | Building_WindowWall.xml | 동일 |
| `RGK_WoodFence` | Building_WindowWall.xml | 링크 타입/비대칭 링크, `fillPercent`, 펜스 관련 `building` 필드, 설정 오류 억제 플래그 등 |
| `GL_FlowerWall` | Building_WindowWall.xml | `placingDraggableDimensions` 제거, `supportsWallAttachments` 추가 |
| `RGK_Door` | Building_WindowWall.xml | 울타리 연결·수동 문 소리 추가 |
| `GL_BoundaryWood` | Building_WindowWall.xml | `placingDraggableDimensions` 제거 |
| `GL_BoundaryStone` | Building_WindowWall.xml | `placingDraggableDimensions` 제거 |
| `RGK_Fireplace` | Fire.xml | 그래픽 오프셋, `fillPercent`, 불 오버레이 오프셋, 샷오버 설정 플래그 제거 |
| `RGK_FireplaceE` | Fire.xml | `RGK_Fireplace`와 동일 패턴 |

---

## 1. Building_Furnitures.xml

### `GL_TablewareShelf`

- **`graphicData`**
  - 추가: `<drawOffsetNorth>(0,0,0)</drawOffsetNorth>`
  - 추가: `<drawOffsetSouth>(0,-1,0)</drawOffsetSouth>`

---

## 2. Building_WindowWall.xml

### `RGK_Wall`

- **추가**: `noRightClickDraftAttack`, `replaceTags` → `Wall`
- **`statBases`**: `Beauty` 제거. `MaxHitPoints` 250→300, `WorkToBuild` 150→135, `Flammability` 0.5→1.0, `MeditationFocusStrength` 0.22 추가
- **제거/대체**: `constructEffect`, `placingDraggableDimensions`
- **추가**: `drawStyleCategory` → `Walls`, `uiOrder` 2000, `fertility` 0
- **`terrainAffordanceNeeded`**: `Medium` → `Light`
- **`building`**: `paintable`, `isWall`, `isPlaceOverableWall`, `supportsWallAttachments`, `isStuffableAirtight`, 빈 `relatedBuildCommands`
- **제거**: `designationHotKey` (`Misc3`)
- **추가**: `comps` → `CompProperties_MeditationFocus` (Minimal)
- **추가**: `damageMultipliers` → `Bomb`, `Thump` 배율 2

### `GL_Wall`

- `RGK_Wall`과 동일 계열 변경(벽 태그·스탯·명상·피해배율·`drawStyleCategory`·`uiOrder`·`fertility`·`building` 확장·`comps` 등)
- **`stuffCategories`**: 항목 순서 변경 (Woody/Stony/Metallic → Metallic, Woody, Stony)
- **`terrainAffordanceNeeded`**: `useStuffTerrainAffordance` true 추가, 기본값 `Heavy` (이전에는 `Medium`만 명시)
- `constructEffect`, `placingDraggableDimensions`, `designationHotKey` 제거는 `RGK_Wall`과 동일 방향

### `RGK_VentWall` / `GL_VentWall` / `RGK_WindowWall` / `GL_WindowWall`

- **`placingDraggableDimensions`**: 제거 (빈 줄로 대체된 형태)
- **`building.supportsWallAttachments`**: `true` 추가

### `RGK_WoodFence`

- **`graphicData`**: `linkType` `Basic` → `Asymmetric`, `asymmetricLink` 블록 추가 (`linkToDoors` false)
- **`fillPercent`**: 0.1 → 0.25
- **`placingDraggableDimensions`**: 제거
- **`building`**: `isFence`, `isPlaceOverableWall`, `ai_chillDestination`, `ai_neverTrashThis` 등 추가·정리, `isInert` 위치 조정
- **추가**: 루트 `disableImpassableShotOverConfigError` true

### `GL_FlowerWall`

- **`placingDraggableDimensions`**: 제거
- **`building.supportsWallAttachments`**: `true` 추가

### `RGK_Door` (fence door)

- **`building`**: `preferConnectingToFences` true, `soundDoorOpenManual` / `soundDoorCloseManual` → `Door_FenceGateManual`

### `GL_BoundaryWood` / `GL_BoundaryStone`

- **`placingDraggableDimensions`**: 제거

---

## 3. Fire.xml

### `RGK_Fireplace` / `RGK_FireplaceE` (동일 항목)

- **`graphicData`**: `drawOffsetNorth` `(0,-1,0)`, `drawOffsetSouth` `(0,0,0)` 추가
- **`fillPercent`**: 0.20 → 1.0
- **`CompProperties_FireOverlayRotatable` → `offset`**: `(0,0,0.2)` → `(0,1,0.2)`
- **제거**: `disableImpassableShotOverConfigError`

---

## 참고

- 본 보고서는 **현재 스테이징 여부와 무관하게** 워킹 디렉터리와 `HEAD` 차이만 반영합니다.
- `1.6` 이하에서 XML 외 변경(예: `Source/*.cs`, `Assemblies/*.dll`)은 범위에서 제외했습니다.
