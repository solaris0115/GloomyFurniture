GloomyFurniture 1.6 Def C# XmlPatch inventory refactor prep Solaris.FurnitureBase

# GloomyFurniture 컨텐츠 인벤토리 (GloomyFix 제외)

**범위**: `GloomyFurniture/` 모드 본편 — `GloomyFix/` 및 별도 fix 패키지는 제외.  
**목적**: 노후화된 XML·데이터·코드 정리 전, 바닐라 친화·개발 친화 개선을 위한 구조 파악.

---

## 1.0 `GloomyFurniture/1.6/Defs` — 파일별 **상속 트리** (`ParentName` / 모드 내 `Name` 추상 → `defName`)

- `[Abstract] 이름` = 해당 XML에서 `Abstract="True"` + `Name`(바닐라 `defName` 아님).
- `(바닐라) …` = 코어 등 외부 패키지 부모.
- `SongDef.xml`의 `SongDef` 한 건은 `<defName>` 없음.

### `Building_Furniture_Base.xml` (추상만)

```
(바닐라) BuildingBase
└── [Abstract] RGKFurnitureBase
    └── [Abstract] RGKFurnitureWithQualityBase
        ├── [Abstract] RGKArtableFurnitureBase
        └── [Abstract] RGKTableBase
            └── [Abstract] RGKTableGatherSpotBase

(바닐라) BedWithQualityBase
└── [Abstract] RGKArtableBedBase
```

### `Building_WindowWall.xml`

```
[Abstract] RKGFence                         ← ParentName 없음
└── [Abstract] Gloomy_RKGFenceWallRelatedCommands
    ├── RGK_VentWall
    ├── GL_VentWall
    ├── RGK_WindowWall
    └── GL_WindowWall

(바닐라) BuildingBase
├── [Abstract] Gloomy_WallRelatedCommandsLayer
│   ├── RGK_Wall
│   ├── GL_Wall
│   └── GL_FlowerWall
├── [Abstract] WallWindowBase
│   └── [Name] WallWindow
│       └── RGK_window
├── RGK_WoodFence
├── GL_DoorFrame
├── GL_BoundaryWood
├── GL_BoundaryStone
├── GL_BoundaryWoodCorner
└── GL_BoundaryStoneCorner

(바닐라) DoorBase
├── RGK_DoorOld
└── RGK_Door
```

### `Fire.xml`

```
(바닐라) BuildingBase
├── RGK_Fireplace
├── RGK_FireplaceE
├── RGK_lamp
├── RGK_LampM
├── RGK_lampE
├── GL_lamp_White
├── GL_LampM_White
├── GL_lampE_White
├── RGK_Street_Lamp_L
├── RGK_Street_Lamp
├── RGK_Street_Lamp_Seven
└── RGK_Street_Lamp_SevenE

(바닐라) LampBase
└── [Abstract] WallLightBase
    ├── GL_Wall_Lamp
    ├── GL_Wall_LampE
    ├── GL_Wall_Lamp_White
    └── GL_Wall_LampE_White

(ParentName 없음 · DesignatorDropdownGroupDef)
├── GL_Lamp_DropDown
├── GL_StreetLamp_DropDown
├── GL_StreetLamp_Seven_DropDown
└── GL_WallLamp_DropDown
```

### `Building_Furnitures.xml`

중간 추상(`RGKArtableBedBase`, `RGKTableBase` 등)은 `Building_Furniture_Base.xml`.

```
(바닐라) BedWithQualityBase
├── RGK_bedSingle
└── RGK_bedSingleB

(바닐라) BedWithQualityBase → [Abstract] RGKArtableBedBase
├── RGK_bedDouble
├── RGK_bedDoubleB
└── GL_ClassyDoubleBed

… → [Abstract] RGKArtableFurnitureBase
├── RGK_Dresser
├── RGK_Chair
├── RGKDiningChair
└── GL_Bench

… → [Abstract] RGKTableBase
├── RGK_EndTable
├── RGK_EndTableWithLamp
├── RGK_EndTableWithLampE
├── RGK_TableA
├── RGK_TableC
├── RGK_TableB
├── RGK_TableD
├── RGK_TableE
├── RGK_TableF
├── RGK_TableG
├── RGK_TableH
├── RGK_MiniTableA
└── RGK_MiniTableB

… → [Abstract] RGKFurnitureWithQualityBase
├── RGK_bucketr
├── RGK_orc
└── RGK_SimpleChair

(바닐라) FurnitureWithQualityBase
├── RKGWineRackB
├── WardrobeA
├── WardrobeB
├── RGK_Manger
├── GL_Shelf
├── RGK_DogBowl
└── GL_Locker

(바닐라) BuildingBase
├── GL_Bookshelf
├── GL_Cupboard
├── GL_TablewareShelf
├── RGK_Flower
├── RGK_Teaset
└── GL_Teddy
```

### `ThingDefs_Buildings/Legacy.xml`

```
(바닐라) BuildingBase
├── GL_Street_Lamp_L_White
├── GL_Street_Lamp_White
├── GL_Street_Lamp_Seven_White
└── GL_Street_Lamp_SevenE_White
```

### `Production.xml`

```
(바닐라) BenchBase
├── RGK_SimpleResearchBench
├── GL_TableButcher
├── RGK_FueledStove
├── GL_ElectricStove
├── GL_HandTailoringBench
├── GL_ElectricTailoringBench
├── GL_FueledSmithy
└── GL_ElectricSmithy

(바닐라) BuildingBase
└── GL_Bonfire
```

### `Building_Joy.xml`

```
(바닐라) FurnitureWithQualityBase
├── GL_ChessTable
├── GL_PokerTable
└── GL_BilliardsTable

(바닐라) BuildingBase
└── GL_TubeTelevision
```

### `Building_Rug.xml`

```
[Abstract] RGKRugBase                    ← ParentName 없음
├── RugA_Small
├── RugB_Small
├── RugC_Small
├── RugA
├── RugB
├── RugC
├── RugA_Large
├── RugB_Large
└── RugC_Large
```

### `Plants_Cultivated_Decorative.xml`

```
(바닐라) PlantBase
└── [Abstract] DecoTree
    ├── GL_TreeA
    └── GL_TreeB
```

### `JukeBox.xml`

```
(바닐라) BuildingBase
└── JukeBox

(ParentName 없음 · 동일 파일)
├── JoyGiverDef   → JoyGiver_ListenSong
├── JoyKindDef    → JoyKind_ListenSong
├── JobDef        → JobDef_ListenSong
└── ConceptDef    → MusicForSickPeople
```

### `Recipes_Meals.xml`

```
(바닐라) CookMealBase
├── CookMealPasteD
├── CookMealSimpleD
├── CookMealFineD
├── CookMealLavishD
└── CookMealSurvivalD
```

### `CategoryDef.xml`

```
(ParentName 없음)
├── ThingCategoryDef       → AnimalFoods
├── ThingCategoryDef       → Alcohol
├── DesignationCategoryDef → OldStyleFurniture
└── DesignationCategoryDef → OldStyleDeco
```

### `TerrainDef.xml`

```
(바닐라) FloorBase
├── [Abstract] WoodFloorBase
│   ├── RGK_Terrain
│   └── GL_Terrain
└── [Abstract] PebbleBase
    ├── PebbleSandstone
    ├── PebbleGranite
    ├── PebbleLimestone
    ├── PebbleSlate
    └── PebbleMarble

(ParentName 없음 · DesignatorDropdownGroupDef)
├── Floor_Wood
└── Floor_Pebble
```

### `SongDef.xml`

```
SongDef (1건, XML에 defName 없음)
```

---

## 1. Def 기준 컨텐츠 트리 (부모·자식 관계 중심)

> **§1.0**에 파일별 ASCII 상속 트리를 전부 두었음. 이 절은 당초 요약용이며, 상세는 **§1.0**을 기준으로 보면 됨.

바닐라 `ParentName`은 *해당 DefType의 추상/구체 Def*를 가리킨다. 아래 `→`는 상속(부모 → 자식), `│` 아래 항목은 **같은 파일·같은 상위 아래의 구체 Def 묶음**이다.

### 1.1 가구·생산 공통 추상 (`Building_Furniture_Base.xml`)

```
BuildingBase (바닐라)
└── RGKFurnitureBase [Abstract, Name]
    └── RGKFurnitureWithQualityBase [Abstract]
        ├── RGKArtableFurnitureBase [Abstract] → (예술 가능 가구 계열)
        └── RGKTableBase [Abstract] → 식탁·엔드테이블 등
            └── RGKTableGatherSpotBase [Abstract] → 모닥불 등 모임 지점용

BedWithQualityBase (바닐라)
└── RGKArtableBedBase [Abstract] → 침대류 (예술 탭)
```

**설계 카테고리**: 위 가구·침대류는 `designationCategory`가 대체로 `OldStyleFurniture` (`CategoryDef.xml`).

### 1.2 건물 ThingDef — 파일별 트리 요약

**`Building_Furnitures.xml`** (위 추상 + 바닐라 베이스 직결)

- `BedWithQualityBase` / `RGKArtableBedBase` 하위: 단·더블 침대, `GL_ClassyDoubleBed`
- `RGKArtableFurnitureBase`: `RGK_Dresser`
- `RGKTableBase`: 엔드테이블·램프付 테이블, 의자·벤치, 식탁 A~H, 미니테이블, 와인랙, 옷장, 책장·찬장·식기선반, 먹이통·개밥그릇, 사물함, 장식(꽃·티세트·테디) 등
- `RGKFurnitureWithQualityBase`: `RGK_bucketr`, `RGK_orc` 등 일부 수납/장식

**`Building_WindowWall.xml`** (벽·창·울타리·문·경계 등 — 내부 추상 다수)

```
BuildingBase
├── (Abstract) RKGFence
├── (Abstract) Gloomy_WallRelatedCommandsLayer ← RGK_Wall, GL_Wall, GL_FlowerWall …
├── (Abstract) Gloomy_RKGFenceWallRelatedCommands ← RGK_VentWall, GL_VentWall, RGK_WindowWall, GL_WindowWall
├── (Abstract) WallWindowBase → RGK_window [WallWindow]
├── DoorBase → RGK_DoorOld, RGK_Door
├── BuildingBase (직계) → RGK_WoodFence, GL_DoorFrame, GL_Boundary*, GL_Boundary*Corner
```

- `thingClass`: 벽·창·창문벽 등은 `Gloomylynx.GL_Building`; 통풍구는 `Building_Vent` + `CompFlickableVent`.

**`Fire.xml`** (난로·램프·가로등·벽등 + 드롭다운 그룹)

```
BuildingBase → 벽난로·전기난로·탁상/가로등·화이트 변형 등
LampBase (바닐라) → WallLightBase [Abstract] → GL_Wall_Lamp* (연료/전기/화이트 변형)

DesignatorDropdownGroupDef (동일 파일 하단): GL_Lamp_DropDown, GL_StreetLamp_* , GL_WallLamp_DropDown
```

**`ThingDefs_Buildings/Legacy.xml`**

- XML상 `ParentName`은 **`BuildingBase`** (§1.0 트리 참고). 시각적으로는 `Fire.xml` 가로등 변형과 짝.

**`Production.xml`**

```
BenchBase (바닐라)
├── RGK_SimpleResearchBench (Building_ResearchBench)
├── GL_TableButcher, RGK_FueledStove, GL_ElectricStove, GL_Bonfire
├── GL_HandTailoringBench, GL_ElectricTailoringBench
└── GL_FueledSmithy, GL_ElectricSmithy
```

**`Building_Joy.xml`**

```
FurnitureWithQualityBase (바닐라)
└── GL_ChessTable, GL_PokerTable, GL_BilliardsTable, GL_TubeTelevision
```

**`Building_Rug.xml`**

```
RGKRugBase [Abstract, 바닐라 ParentName 없음]
└── RugA/B/C × (Small / 기본 / Large)
```

**`JukeBox.xml`**

- `ThingDef` `JukeBox` ← `BuildingBase`
- 같은 파일에 `JoyGiverDef`, `JobDef`, `JoyKindDef`, `ConceptDef` (주크박스 플레이 파이프라인)

**`Plants_Cultivated_Decorative.xml`**

```
PlantBase (바닐라) → DecoTree [Abstract, Name]
└── GL_TreeA, GL_TreeB
```

**`TerrainDef.xml`**

```
FloorBase (바닐라) → WoodFloorBase [Abstract] → RGK_Terrain, GL_Terrain
FloorStoneTile (바닐라) 등 → 자갈 Pebble* , 드롭다운 Floor_Wood / Floor_Pebble
```

**`Recipes_Meals.xml`**

- 산업용 조리대용 **복제 RecipeDef** (`CookMeal*` D 접미) — `RGK_FueledStove` / `GL_ElectricStove` 등 사용자 지정.

**`CategoryDef.xml`**

- `ThingCategoryDef`: `AnimalFoods`, `Alcohol`
- `DesignationCategoryDef`: `OldStyleFurniture`, `OldStyleDeco`

**`SongDef.xml`**

- 주크박스 정지용 클립 등 단일 `SongDef` 엔트리.

---

### 1.3 비 ThingDef / 부가 자산

| 구분 | 위치 |
|------|------|
| 번역·Keyed | `GloomyFurniture/Languages/` (다국어 DefInjected·Keyed 등) |
| 텍스처·사운드 | `GloomyFurniture/Textures/`, `GloomyFurniture/Sounds/` (주크박스 트랙 스캔 경로와 연동) |
| 빌드 산출물 | `GloomyFurniture/1.6/Assemblies/Gloomylynx.dll` |

---

## 2. 코드(C#) 관련 — 무슨 뭉탱이가 무엇을 위한 것인지

**네임스페이스**: `Gloomylynx` — DLL `Gloomylynx.dll`.

| 영역 | 대표 파일 | 역할(개괄) | 바닐라/게임에 붙는 방식 |
|------|-----------|------------|-------------------------|
| 그래픽·레이어 컴포넌트 | `Comp.cs` | 불꽃/램프 오버레이 회전, 이층 그래픽(SecondLayer / Follow / OnOffable), 주크박스용 `CompProperties_JukeBox`, 통풍구용 `CompFlickableVent`(지즈모 숨김), `JoyGiver_ListenSong`, `JobDriver_ListenSong` | ThingDef `comps`의 `compClass` / `CompProperties_*` |
| 벽부착 배치 검사 | `Class1.cs` | `PlaceWorker_WallAttachmentNearby` — 벽 인접 배치 규칙 | ThingDef `placeWorkers` |
| 바닥·벽등 배치 | `WallLamp.cs` | 통과 가능/스탠드 가능 타일 검사 PlaceWorker | `placeWorkers` |
| 주크박스 런타임 | `JukeBox.cs` | `CompJukeBox`: 전원·등록/해제, `SongDef` DB에 커스텀 곡 주입·복구, 파일 스캔(`Sounds/Songs`), `JukeBoxMod`(모드 루트 경로), `SongEntry` | JukeBox ThingDef + `Mod` 서브클래스 |
| 전기 벽난로 온도 | `Building_GloomyHeater.cs` | `Building_Heater` 상속, 다칸 방·보조 셀에서 실내 방 탐색 후 온도 제어 | `RGK_FireplaceE` 등 `thingClass` |
| 옷장 | `GloomyWardrobe.cs` | 바닐라 수납·의류 정책과 맞춘 커스텀 빌딩(의류 전용, 자동 착용 on/off 등) | Wardrobe ThingDef `thingClass` |
| 책장 | `GloomyBookshelf.cs` | `Building_Bookcase` 확장, 남향 2열 책 스파인 배치 등 | GL_Bookshelf `thingClass` |
| 사물함 | `Locker.cs` | `Building_Storage` 빈 서브클래스 + **Harmony**: `Fire.DoFireDamage`에서 사물함 뒤 타일 보호 | Locker ThingDef + 정적 패치 |
| 탄트럼·배치·수면 | `Tantrum.cs` | **Harmony**: (1) 탄트럼 파괴 대상에서 사물함 보호 (2) `GL_ClassyDoubleBed`에서 수면 방해 체크 스킵 (3) `GenConstruct.CanPlaceBlueprintOver` **전면 Prefix** — 벽 위 건축·전선·`GL_Building` 등 레거시 배치 규칙. `GL_Building`은 빈 `Building` 서브클래스(마커). 벽 DefOf | 게임 시작 시 정적 생성자에서 패치 |
| 어셈블리 | `AssemblyInfo.cs` | 메타 | — |

**Harmony ID**: `com.Gloomylynx.rimworld.mod` (`LockerPatch`, `TantrumPatch`).

**정리 관점에서 눈에 띄는 부분**: `GenConstruct.CanPlaceBlueprintOver` 전체를 런타임에 대체하는 패턴, 주크박스의 `DefDatabase<SongDef>` 조작·세이브 로드 시 복구, `JukeBoxCore` 내 `MusicManagerPlay` 할당 코드(호출 경로가 불명확하면 사실상 레거시)는 **바닐라 친화 리팩터링 시 우선 검토 대상**.

---

## 3. Xml Patch (`GloomyFurniture/Patches/`) — 무엇을 고치나

| 파일 | 대상(요지) |
|------|-------------|
| `RecipePatch.xml` | 바닐라·타모드 `ThingDef`/`RecipeDef`의 `recipeUsers`, `WorkGiverDef`의 `fixedBillGiverDefs`에 **글루미 대장간·재봉·도축대** 등을 추가 (전기 대장간/재봉대 등과 동급 작업대로 취급). |
| `GF_JoyGiver.xml` | `JoyGiverDef`의 `thingDefs`에 **GL 체스·포커·당구·튜브 TV**를 바닐라 JoyGiver와 동일 목록에 편입. |
| `RoyalPatch.xml` | **Royalty** `RoyalTitleDef` 침실 요구: 더블침대·엔드테이블·화장대에 모드 가구 허용. |
| `VGP_Patch.xml` | `FueledStove` 사용자에 `RGK_FueledStove`/`GL_ElectricStove` 추가; **VGP** 모드 있을 때 본파이어 등 `recipeUsers` 추가. |
| `RimCuisine_Patch.xml` | **RimCuisine 1.0** 모드 탐지 시 다수 `RecipeDef`에 스토브·본파이어 `recipeUsers` 추가. |
| `TeddyPatch.xml` | 바닐라 `Armchair`에 `CompProperties_AffectedByFacilities`를 넣고 **`GL_Teddy`** 링크. |
| `Manger_Patch.xml` | `Kibble`/`Hay`에 `AnimalFoods` 카테고리; `PsychiteTea` 제작대; `Beer`에 `Alcohol` 카테고리; `DoBillsCook`에 스토브·본파이어. |
| `FermentingBarrel_Patch.xml` | 바닐라 **`FermentingBarrel`** 그래픽·UI 아이콘을 모드 텍스처로 교체; VGP Garden Drinks 등 조건부 확장. |

**패치 로더**: RimWorld **Xml Patch** (`PatchOperation*`). Harmony와 별계.

---

## 4. 리팩터링 방향 메모 (요청 배경 반영)

- **Def**: `RGKRugBase`처럼 바닐라 `ParentName` 없이 출발하는 블록, 주석 처리된 `CompProperties_RoomIdentifier` 다수, `Recipes_Meals`의 이중 RecipeDef 등은 **바닐라 패턴으로 통합·삭제** 여지가 큼.
- **코드**: 배치 규칙은 가능한 한 **PlaceWorker + Def만**으로 끝내고, `GenConstruct` 전역 치환·`SongDef` DB 직접 조작은 **후순위로 치우거나 축소**하는 편이 유지보수에 유리.
- **Xml Patch**: 타 모드·DLC 의존은 `PatchOperationFindMod` / `Test`로 이미 가드된 부분이 많음 — 정리 시 **로드 순서·모드 표기명(구식 이름)** 만 주기적으로 확인.

---

*문서 생성: GloomyFurniture 저장소 `99_Report/` — GloomyFix 제외 인벤토리용.*
