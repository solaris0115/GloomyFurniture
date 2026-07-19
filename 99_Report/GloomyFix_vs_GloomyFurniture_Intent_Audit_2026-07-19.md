# GloomyFix vs GloomyFurniture 현재 차이 전수·의도 감사 보고서

> 조사 기준일: 2026-07-19  
> 비교 대상: `GloomyFix`(임의 수정본) / `GloomyFurniture`(현재 원본)  
> 목표: 줄 단위 차이 나열보다 변경 의도, 현재 원본 반영 여부, 채택 가치와 퇴행 위험을 판정한다.

## 0. 최종 결론

**`GloomyFix`를 통째로 원본에 병합하거나 현재 원본과 계속 병용하는 것은 권장하지 않는다.** Fix에는 좋은 아이디어가 남아 있지만, 구현 방식이 같은 `defName` 124개를 뒤에서 전면 교체하는 형태라 현재 원본에 이미 들어간 더 최신의 책장·옷장·벽등·전기 벽난로·벽/울타리·주방 판정 개선도 함께 되돌린다.

권장 방향은 다음 한 문장으로 정리된다.

> **현재 `GloomyFurniture`의 XML 상속 구조와 C# 구현을 기준선으로 유지하고, Fix에서 아직 유효한 의도만 필드 단위로 다시 구현한다.**

### 판정 요약

| 판정 | 핵심 항목 |
|---|---|
| **우선 반영 추천** | 침대 5종 `SleepAccelerator` 연동, 바닥 7종 도색, 연구대의 1.6 연구 보고 Comp·PlaceWorker·`Laboratory` 역할, TV 메뉴 아이콘 보정 |
| **조건부 반영** | 생산설비/가구/러그/구조물 도색, 다재료화, 장식 나무 포장, 연구대·도축대 등 선택 텍스처, 고전 자동문·소형 선반·스탠드 전기 램프·대형 와인 랙, Royal/Hakuro/NewRatkin 호환 의도 |
| **이미 반영 또는 원본 우세** | 드레서 거리 6, Biotech 이유식, 조리대 Kitchen 판정, 책장 10권·동적 표시, 옷장 내부 보관, 전기 벽난로 난방, 벽등, 최신 가로등, 벽/울타리/울타리문 1.6 동작, Bonfire 아이콘 |
| **가져오지 않음** | Fix Def 파일 통복사, Fix Harmony DLL 그대로, Fix 책장/옷장/벽난로/울타리 정의, 백색 조명 즉시 삭제, Royalty/Drape 패치 그대로, 중복 번역 파일, 빌드 캐시와 오래된 IL |

신규 Def 4종은 결함 수정이 아니라 콘텐츠 확장이다. “비슷한 것은 굳이 고치지 않는다”는 기준에서는 우선순위가 낮으며, 실제로 추가하기로 결정했을 때만 완성도를 보강해 가져오는 편이 맞다.

---

## 1. 조사 범위와 전수성

### 1.1 파일 전체 비교

| 항목 | GloomyFix | GloomyFurniture |
|---|---:|---:|
| 전체 파일 | 100 | 558 |
| 전체 크기 | 1,038,889 bytes | 41,123,545 bytes |
| 같은 상대 경로 | 41 | 41 |
| └ 바이트까지 동일 | 3 | 3 |
| └ 내용 변경 | 38 | 38 |
| Fix에만 있는 상대 경로 | 59 | - |
| 원본에만 있는 상대 경로 | - | 517 |

원본-only 517개는 Fix가 삭제하는 파일이 아니다. Fix는 원본 위에 얹히는 동반 모드이므로, 원본의 나머지 Def·코드·사운드·텍스처는 계속 로드된다. 따라서 raw 경로 차이와 실제 런타임 덮어쓰기를 분리해 조사했다.

Fix 100개 파일의 담당 분해는 다음과 같다.

- Def XML 13개: top-level 노드 129개
- 비Def XML 20개: About 1, Patch 8, Language 11
- PNG 50개
- Source 14개
- Assemblies 2개(DLL/PDB)
- `PublishedFileId.txt` 1개

Fix XML 33개와 PNG 50개는 모두 형식 파싱에 성공했다.

### 1.2 Def 전수 매핑

`GloomyFix/1.6/Defs`의 top-level 노드 129개를 원본 `1.5/1.6` 전체에서 `defName`과 abstract `Name`으로 역검색했다.

| 분류 | 노드 수 |
|---|---:|
| 원본 1.6과 매핑됨 | 125 |
| └ 의미상 변경 | 91 |
| └ 의미상 동일 | 34 |
| Fix 신규 | 4 |
| 합계 | **129** |

신규는 `RKGWineRackTall`, `GL_Small_Shelf`, `RGK_lampEM`, `RGK_AutodoorOld` 네 개뿐이다. 나머지는 대부분 기존 Def의 수정판 또는 완전 동일 복제다.

### 1.3 조사 방법과 한계

- SHA-256으로 전체 파일을 분류했다.
- XML은 공백·주석뿐인 차이를 걸러 DOM과 leaf 값, 상속 부모, `MayRequire`, Def 참조를 비교했다.
- RimWorld 1.6 로더/DefDatabase 소스로 동일 `defName`의 실제 덮어쓰기 동작을 확인했다.
- 현재 원본 Git 이력을 대조해 Fix 의도가 이미 더 최신 방식으로 반영된 항목을 확인했다.
- PNG는 크기, 알파 영역, 픽셀 배치, `_m` 마스크, XML의 실제 `texPath/uiIconPath` 참조를 확인했다.
- Fix DLL, 배포/obj 해시, IL, 현재 원본 Harmony 코드를 대조했다.
- 실제 게임을 실행한 통합 smoke test는 이번 보고서 범위에 포함하지 않았다. 시각·밸런스 후보는 아래에 별도 QA 필요로 표시했다.

---

## 2. 두 모드의 런타임 관계

`GloomyFix`는 독립 대체 모드가 아니다.

- About 설명에는 GloomyFurniture가 필수라고 쓰였지만 `Solaris.FurnitureBase`의 `modDependencies`가 주석 처리되어 있다.
- `loadAfter`는 설치·활성화를 강제하지 않는다.
- 원본 없이 Fix만 켜면 `Gloomylynx.*` C# 타입, 드롭다운 Def, 대다수 텍스처가 결손된다.
- 원본과 같이 켜면 Fix가 뒤에서 같은 `defName`을 교체한다. RimWorld 1.6 `DefDatabase<T>.AddAllInMods()`는 앞 Def를 제거하고 뒤 Def를 등록하므로 “중복 로그만 나는 것”이 아니라 Fix판이 실제 승자가 된다.
- Fix About는 1.4/1.5/1.6을 선언하지만 기능 Def와 버전별 패치·텍스처는 `1.6`에만 있다. 배포 DLL도 1.6 Assembly-CSharp로 빌드되어 1.4/1.5 지원 근거가 없다.

즉 Fix를 현재 원본과 병용하면 좋은 변경만 추가되는 것이 아니라, 최신 원본을 오래된 전체 정의로 되돌리는 항목이 생긴다.

---

## 3. 우선 반영할 만한 의도

### 3.1 침대 5종의 Sleep Accelerator 연동 — 적극 추천

대상:

`RGK_bedSingle`, `RGK_bedSingleB`, `RGK_bedDouble`, `RGK_bedDoubleB`, `GL_ClassyDoubleBed`

Fix는 각 침대의 `linkableFacilities`에 다음을 일관되게 추가한다.

```xml
<li MayRequire="Ludeon.RimWorld.Ideology">SleepAccelerator</li>
```

Ideology가 없으면 안전하게 무시되고, 현재 원본에는 아직 없다. 변경 범위가 좁고 의도·효과가 분명하다.

**판정: P1 적극 반영.**

### 3.2 바닥 7종 도색 — 적극 추천

Fix는 abstract `WoodFloorBase`, `PebbleBase`에 `isPaintable=true`를 넣어 다음 지형에 상속한다.

`RGK_Terrain`, `GL_Terrain`, `PebbleSandstone`, `PebbleGranite`, `PebbleLimestone`, `PebbleSlate`, `PebbleMarble`

RimWorld 1.6의 정식 `TerrainDef.isPaintable` 필드이고 DLC 의존도 없다.

**판정: P1 적극 반영.**

### 3.3 연구대 1.6 현대화 — 적극 추천, 수치 변경은 분리

대상: `RGK_SimpleResearchBench`

가져올 가치가 큰 부분:

- 부모에서 물려받은 잘못된 작업속도 보고 Comp를 `Inherit="False"`로 끊음
- `CompProperties_ReportWorkSpeed / ResearchSpeedFactor` 적용
- 현 원본의 `GL_Bookshelf` 시설 Comp 유지
- `PlaceWorker_PreventInteractionSpotOverlap`
- `PlaceWorker_DrawLinesToBookcasesInRoom`

Fix와 현 원본 모두 `BenchBase`의 `Workshop` 역할을 그대로 상속하는 누락이 있다. 현대화 시 바닐라 연구대처럼 아래도 같이 넣는 것이 맞다.

```xml
<workTableRoomRole>Laboratory</workTableRoomRole>
<workTableNotInRoomRoleFactor>0.8</workTableNotInRoomRoleFactor>
```

반면 `ResearchSpeedFactor 0.85→0.75`, `pathCost 70→50`, `uiOrder=2600`, 다재료·도색은 밸런스/UX 변경이므로 별도 결정해야 한다. `relatedBuildCommands=GL_Bookshelf`는 기존 시설 명령과 중복될 수 있어 제외하는 편이 낫다.

**판정: Comp·PlaceWorker·Laboratory는 P1, 수치는 조건부.**

### 3.4 TV 메뉴 아이콘 보정 — 적극 추천

대상: `GL_TubeTelevision`

- `uiIconScale 0.64→0.8`
- `uiIconOffset (0,0.1)` 추가

PNG 교체가 아니라 Def UI 값만 바꾸는 좁은 시각 수정이다. Fix About의 TV UI 주장과 실제 구현이 일치한다.

**판정: P1, 건설 메뉴 한 번 확인 후 반영.**

### 3.5 NewRatkinPlus 호환 — 대상 모드를 지원한다면 적극 후보

Fix 패치는 로컬 NewRatkinPlus 1.6에서 실제 존재하는 Def를 대상으로 한다.

- `RK_Pulpit`: UI 오프셋
- `RK_HamsterWheelGenerator`: 전용 UI 아이콘·scale
- 두 Ratkin 재봉대: `Make_Patchleather`

현 버전 대상에는 해당 필드가 없어 정상 적용되고, `RK_HamsterWheelGenerator_UI.png`도 실제 참조된다. 다만 GloomyFurniture 핵심 기능과는 별개인 타 모드 호환이다.

**판정: 지원 정책에 포함한다면 P1. `PatchOperationConditional`로 중복 방어 후 별도 호환 패치로 유지.**

---

## 4. 좋은 방향이지만 조건부인 의도

### 4.1 광범위한 다재료화·도색

Fix의 가장 큰 공통 방향이다.

- `<paintable>true</paintable>` literal: Fix 80회, 현재 원본 4회
- `<isPaintable>true</isPaintable>`: Fix 2회, 현재 원본 0회
- 많은 가구가 `Woody` 단독에서 `Woody + Stony + Metallic`으로 확장
- 생산설비 9, 러그 9, JukeBox, 벽/창호/경계, 조명, 가구·오락 다수에 도색 의도

의도는 좋지만 일괄 적용은 위험하다.

- Stuff는 HP·가격·미관·작업량·가연성까지 바꾼다.
- `_m` 마스크와 `CutoutComplex`가 없는 자산은 재료색/도색이 기대대로 보이지 않을 수 있다.
- Fix가 일부 Def에서 오히려 shader를 지워 마스크를 무력화한다.
- 저장 가구·조명처럼 현 원본 C#과 결합된 Def는 paintable 필드만 좁게 넣어야 한다.

**판정: 방향은 채택. 자산별로 `stuffCategories + shader/mask + paintable` 세트를 검증하며 작은 묶음으로 반영.**

우선순위가 높은 범위:

- 러그 9종, JukeBox, 생산설비 9종의 도색
- 환풍벽/창문벽/꽃벽/경계 4종의 도색
- 침대·테이블·좌석·오락 가구의 다재료화
- 일반 창문 `RGK_window`의 다재료화

### 4.2 장식 나무 포장

Fix는 abstract `DecoTree`를 `PlantBase→TreeBase`로 바꾸고 `mustBeWildToSow=false`를 추가한다. 이로써 `GL_TreeA/GL_TreeB`가 진짜 나무처럼 행동하고 `MinifiedTree` 포장도 상속한다.

그러나 `TreeBase`는 질량, 벌목 반응, stump, 홍수, 수명·fertility 등 많은 동작도 함께 바꾼다.

**판정: “진짜 나무화”까지 원하면 조건부 추천. 포장만 목적이면 현재 부모를 유지하고 `minifiedDef=MinifiedTree`만 직접 추가하는 좁은 구현이 안전하다.**

### 4.3 신규 콘텐츠 4종

| 신규 Def | 의도 | 보강할 점 | 판정 |
|---|---|---|---|
| `GL_Small_Shelf` | 1×1 소형 선반 | 마스크/shader 없음, 비용이 매우 낮음, 번역 보강 | 조건부 가치 높음 |
| `RGK_lampEM` | 스탠드 램프의 전기형 짝 | 기존 `RGK_lampE`와 raw label 중복, 다국어 번역 부족 | 조건부 |
| `RKGWineRackTall` | 큰 1×1 와인 랙 | 용량 미지정, 부모/필터 정책 불일치, 관련 건설 없음, 번역 부족 | 조건부·보완 필수 |
| `RGK_AutodoorOld` | 고전 스타일 자동문 | 벽 메뉴에서 바닐라 자동문과 병존/대체 결정, 번역 정리 | 조건부 가치 높음 |

신규 Def를 채택하면 관련 텍스처·번역까지 하나의 기능 단위로 가져와야 한다. Def만 복사하면 완성되지 않는다.

### 4.4 구조물의 바닐라 정렬 수치

조건부 후보:

- 환풍벽/창문벽 4종: HP 300, Work 135, Light affordance, Meditation 0.22
- `RGK_WoodFence`: 바닐라 Fence에 가까운 HP/Work/Beauty 수치
- `RGK_Door`: 바닐라 FenceGate에 가까운 HP/Work/재료비
- `RGK_DoorOld`: 비용·작업량 완화와 다재료화

단, 수치만 현 원본 골격에 이식해야 한다. Fix의 전체 구조 Def는 `drawStyleCategory`, `supportsWallAttachments`, `isFence`, `PenMarker`, fence link/pathing 등 현재 원본의 최신 1.6 의미를 잃는다.

다리 설치 의도도 단순 `terrainAffordanceNeeded=Light` 복사보다 Stuffable 벽에서는 바닐라처럼 `useStuffTerrainAffordance=true`를 검토해야 한다.

### 4.5 조명에서 선별할 항목

조건부로 유효한 의도:

- 연료 가로등의 `fuelConsumptionPerTickInRain`
- `showAllowAutoRefuelToggle`
- 가로등 `Mass=10`
- LampBase 계열로 공통 필드 정리
- 탁상/스탠드 램프의 따뜻한 색과 반경 미세 조정

가져오지 않을 항목:

- 현재 원본 반경 16/24를 일괄 15로 축소
- 전기 가로등 전력 100→50 뒤 `ColoredLights factor=0.5`를 남겨 실소비를 25까지 낮추는 조합
- 현 원본의 색 선택·암광·연구 설계·PlaceWorker 제거

### 4.6 Royal/Hakuro 호환 의도

- 현 원본 `RoyalPatch.xml`은 침대, 엔드테이블, `RGK_Dresser`를 이미 처리한다.
- 남은 유효 의도는 Royal 침실의 dresser 대체 목록에 `WardrobeA/B`를 한 번 추가하는 정도다.
- Hakuro 패치는 해당 작위의 침대 목록에 글루미 침대 4종을 넣는 의도는 좋지만, 로컬에 대상 모드가 없어 현재 XPath는 검증하지 못했다.

**판정: Wardrobe A/B는 기존 RoyalPatch에 병합. Hakuro는 현재 대상 Def 구조를 확인하고 Conditional로 방어한 뒤 채택.**

### 4.7 선택 텍스처 보정

실제 버그 수정 근거가 강한 후보:

- `RGK_reserch_east.png` + `_m`: 동쪽 연구대 방향 재배치/반전
- `GL_TableButcher_east.png` + `_m`: 돌출 그림자와 정렬 정리
- `GL_Cupboard_north.png` + `_m`
- `GL_Shelf_east/south.png`
- `RGK_Chair3_south.png`
- `RGK_orcB_{east,north,south}` + masks: 발효통 외곽·다리·그림자/마스크 정리
- `RKG_WoodFence_Blueprint_Atlas.png`: cyan 청사진 스타일

이들은 본체와 mask를 쌍으로 적용하고 회전 4방향·설치 셀·건설 메뉴를 실게임에서 확인해야 한다.

---

## 5. 이미 반영되었거나 현재 원본이 더 나은 항목

| 의도 | 현재 판정 | 근거 |
|---|---|---|
| 드레서 효과 거리 6 | 이미 반영 | 양쪽 `maxDistance=6`, `Comfort=0.08` |
| 이유식 제작 | 이미 반영 | 연료/전기 조리대와 Bonfire에 Biotech 조건부 레시피 동일 |
| Kitchen 판정 | 원본 우세 | 현 원본은 Kitchen/0.8, Fix는 이를 제거해 Workshop으로 회귀 |
| 책 저장 | 원본 우세 | `Building_GloomyBookshelf`, 실제 10권 내부 보관·동적 그림·연구 연동 |
| 옷장 저장 | 원본 우세 | `Building_GloomyWardrobe`, 내부 의류 컨테이너·정책·UI·용량 유지 |
| 전기 벽난로 난방 | 원본 우세 | `Building_GloomyHeater`가 2×1 실내 탐색 버그를 해결; Fix는 vanilla Heater로 회귀 |
| 벽등 | 원본 우세 | WallLightBase, attachment, 방향 offset, 새 메뉴 아이콘, 색 선택·암광 |
| 가로등 | 원본 우세 | 현 원본이 연료/전기 스펙·ColoredLights·Legacy를 의도적으로 정리 |
| 벽 연관 건설 | 이미 반영·원본 우세 | 공통 helper 상속, DLC 문·Cooler·Vent, `drawStyleCategory=Walls` |
| 울타리/울타리문 | 원본 우세 | 1.6 Fence/FenceGate 의미, 펜 pathing, 링크, 사운드, 관련 명령 유지 |
| Bonfire UI | 원본 우세 | 전용 `GL_Texture_Furnace` 아이콘을 현재 원본이 의도적으로 사용 |
| Category/Recipe/Song | 동일 | Category 4, Recipe 5, 익명 Song 1은 의미상 동일 |

현재 원본의 후속 커밋 `52de481`, `57acb95`, `3464692`, `df4b389`, `c0472fb`, `80a9a85`, `111a71f`, `0c5cf66`, `d4121c2`, `d874915`, `6b064f2`, `62dbcc5`, `485e72a`가 위 판단을 뒷받침한다.

---

## 6. 통째 반영하면 생기는 핵심 퇴행

### 6.1 저장 시스템 퇴행

- `GL_Bookshelf`: 동적 10권 책장 → 정적 3칸 Bookcase
- `WardrobeA/B`: 내부 의류 컨테이너 → 일반 `Building_Storage`
- `RKGWineRackB`: `maxItemsInCell=3` 삭제
- `RGK_Manger`: 현재 용량 2·fixed Foods 삭제
- `RGK_DogBowl`: 용량 3 삭제
- 여러 저장 Def에서 fixed 필터를 없애 설명보다 넓은 물품을 허용

### 6.2 생산시설 퇴행

- 조리대 2종과 Bonfire의 Kitchen 역할·비주방 패널티 제거
- Bonfire 전용 아이콘 제거
- 연구대 변경에는 좋은 부분도 있으나 연구속도 하향·중복 관련 명령이 한 묶음으로 섞임

### 6.3 구조물 퇴행

- 환풍벽/창문벽/꽃벽에서 최신 `drawStyleCategory=Walls`, attachment 지원, helper 연관 건설 손실
- `RGK_WoodFence`가 `isFence`와 Fences 링크를 잃고 Impassable roof-supporting wall처럼 변함
- `RGK_Door`가 FenceGate 의미를 잃고 generic Door로 회귀
- 기존 Stuffable 목재 창호 일부가 고정 `WoodLog` 비용으로 바뀌어 저장 호환·해체 환급·색상 의미가 변함

### 6.4 조명·그래픽 퇴행

- `RGK_FireplaceE`의 실내 난방 수정 제거
- 벽난로·식기선반의 현재 그래픽 offset 제거
- 벽등 XML과 텍스처 좌표계를 구형 세트로 되돌림
- EndTable 램프의 색 선택·암광 기능 제거
- 현재 가로등 스펙과 연구 설계를 덮음

### 6.5 세이브 호환 위험

Fix의 원본용 패치는 흰색 조명 9종을 직접 제거한다. 색 선택 기능 도입 후 중복 Def를 정리하려는 방향은 타당하지만, 기존 세이브에 해당 건물이 있으면 Def 누락이 발생할 수 있다.

현재 원본처럼 Legacy 파일로 옮기고 건설 메뉴에서만 숨기는 방식이 더 안전하다. 남은 백색 탁상/벽등도 같은 단계적 폐기 방식을 권장한다.

### 6.6 정적 오류와 불완전한 의존 관계

- `RGK_EndTableWithLamp`와 `RGK_TableD`에 `<designationCategory inharit="false"/>`가 있다. 정식 특수 속성은 `Inherit="False"`다. 첫 항목은 현 원본에도 남은 기존 오류이고, `RGK_TableD`는 Fix가 새로 늘린 오류다. 의도한 상속 차단을 보장할 수 없으므로 별도로 수정해야 한다.
- Fix `Buildings_Lighting.xml`은 `GL_Lamp_DropDown`, `GL_StreetLamp_DropDown`, `GL_StreetLamp_Seven_DropDown`을 참조하지만 정의하지 않는다. 원본 `Fire.xml`의 원본-only helper가 같이 로드된다는 전제라 단독 교체가 불가능하다.
- `RKGWineRackTall`은 용량·fixed filter·관련 건설이 미완성이고, `GL_Small_Shelf`는 다재료/도색을 선언하면서 mask/shader가 없다. `RGK_lampEM`은 기존 전기 램프와 raw label이 겹친다.
- 저장 filter에서 “기본 허용”과 “강제 허용”을 혼용해, 와인 랙·여물통·선반이 About 설명보다 훨씬 넓은 물품을 받는 경우가 있다.

이 항목들은 Fix가 의도 모음으로는 유용하지만 그대로 배포 가능한 정답본은 아니라는 근거다.

---

## 7. Patch XML 8개 실제 실행 감사

| 파일 | 현재 실행 판정 | 의도/문제 | 권고 |
|---|---|---|---|
| `1.6/Patches/GloomyFurniture.xml` | 원본 이름이 맞으면 실행 | 흰색 조명 9종 순차 제거 | 직접 삭제 대신 Legacy·비건설화 |
| `1.6/Patches/GloomyFurniture (Continued).xml` | 정확한 이름일 때 실행 | 대상 하나가 없으면 그 뒤 Sequence 중단 | Continued 현 구조 확인, 개별 Conditional 필요 |
| `Patches/Core.xml` | 실행 | FermentingBarrel icon scale/offset; 현 원본 patch의 scale/path와 중복 | 기존 원본 패치 안에 값만 병합 |
| `Patches/Royalty.xml` | 현 1.6에서 첫 Replace 실패 후 중단 | raw Drape에 `altitudeLayer`가 없어 뒤 항목 전부 미실행 | 그대로 채택 금지 |
| `1.6/Patches/Royalty.xml` | match 자체 실패 | `FindMod`가 이름 `Royalty` 대신 packageId를 검사 | 파일 폐기, 필요한 의도만 재작성 |
| `1.6/Patches/Vanilla Expanded.xml` | VEF 이름이 맞으면 실행 가능 | 현 원본 RoyalPatch와 대부분 중복, Wardrobe만 실질 고유 | Wardrobe A/B만 기존 RoyalPatch에 병합 |
| `Patches/Hakuro Xenohuman.xml` | 대상 모드 있을 때 | 글루미 침대 4종을 작위 침대 요건에 추가 | 대상 최신 XPath 검증 후 조건부 |
| `Patches/NewRatkinPlus.xml` | 로컬 1.6에서 정상 대상 확인 | UI 2건, Patchleather 2대 | Conditional로 다듬어 별도 호환 패치 |

### Royalty/Drape 주장 불일치

Fix About는 Royalty 커튼과 침실 호환을 구현했다고 설명하지만, 현재 1.6에서는 Royalty 패치 두 벌이 모두 죽어 있다.

고쳐서 활성화하더라도 Drape를 `PassThroughOnly→Impassable`로 바꾸고 Gloomylynx의 second-layer Comp와 8개 자산에 결합한다. 이는 단순 텍스처 수정이 아니라 통행·사격·고도에 영향을 주는 게임플레이 변경이다.

**판정: Drape 2-layer 연출은 별도 실험 기능으로만 검토. 원본에 즉시 반영하지 않는다.**

---

## 8. Harmony DLL과 Source 감사

### 8.1 실제 기능

Fix DLL의 런타임 기능은 하나다.

- `GenConstruct.CanPlaceBlueprintOver` Prefix
- `[HarmonyBefore("com.Gloomylynx.rimworld.mod")]`
- 새 Def의 `placeWorkers`에 `Placeworker_AttachedToWall`이 있으면 `__result=true`, `return false`

의도는 벽 부착 조명 청사진 겹침 허용이다. 그러나 `oldDef`가 실제 지원 벽인지, 벽 청사진인지, 다른 충돌 대상인지 전혀 보지 않고 원본과 뒤 Harmony 패치를 모두 건너뛴다.

현재 원본 벽등은 이미 바닐라형 attachment Def로 정리됐고, 현 Prefix에도 비-edifice 부착물을 허용하는 경로가 있어 현재 버그 재현 근거가 약하다. 또한 현 원본 `Tantrum.cs` 자체도 오래된 vanilla 메서드 전체 복제라 1.6 최신 로직과 동기화할 필요가 있다.

**판정: Fix DLL은 가져오지 않는다. 문제가 재현되면 현 Prefix를 최신 vanilla 로직과 동기화하고, `supportsWallAttachments`와 특정 벽등 조건으로 좁혀 해결한다.**

### 8.2 소스·바이너리 상태

- 배포 DLL과 `Source/obj/Debug` DLL은 SHA-256 동일
- 배포 PDB와 obj PDB도 동일
- 실제 DLL: Assembly-CSharp `1.6.9438.37837`, Harmony `2.4.1.0`
- `GloomyWallLampFix.il`: Assembly-CSharp 1.5, Harmony 2.3.3, 다른 assembly name/MVID
- README의 “IL과 현재 DLL이 동일”은 동작 수준에서는 비슷하지만 바이너리·메타데이터 기준으로 틀림

Source 14개 중 실제 소스/프로젝트는 `HarmonyInit.cs`, `CanPlaceBlueprintOver_Patch.cs`, `AssemblyInfo.cs`, csproj, `.gitignore`다. obj 6개, `.res`, stale IL은 기능 이식 대상이 아니다.

---

## 9. 번역 11개 감사

### 유효한 의도

- 영어/한국어 계열에서 `timber/목재`와 일반 벽을 구분
- `RGK_AutodoorOld` 번역 추가
- Korean/RimWaldo에 `RKGWineRackTall`, `GL_Small_Shelf`, `RGK_lampEM` 번역 추가

### 문제

- Korean/RimWaldo `Buildings_WindowWall.xml`은 기존 `Building_WindowWall.xml` 71키를 거의 전부 복제해 교차 파일 중복을 만든다.
- Korean/RimWaldo `Buildings_Lighting.xml`도 기존 `Fire.xml`을 대량 복제하고, 파일 내부에서 램프 label 4키를 다시 중복한다.
- 신규 Def 번역은 한국어 계열 중심이라 영어·기타 언어 fallback 완성도가 낮다.
- 일본어 Autodoor의 Blueprint/Frame 설명은 수동문 문구를 재사용해 부정확하다.
- 한국어에는 `따듯`, `벽난로 입니다`, 선행 공백이 붙은 ` 7자형` 등 교정할 문구가 있다.

**판정: 신규 번역 파일을 복사하지 말고 기존 원본 언어 파일에 필요한 키만 병합한다. 신규 콘텐츠를 채택할 때 영어 raw label/description부터 정리한다.**

---

## 10. PNG 50개 감사

### 수량 분류

| 분류 | 수량 |
|---|---:|
| 동일 경로 내용 변경 | 23 |
| 바이트까지 동일 | 3 |
| Fix-only 경로 | 24 |
| 합계 | **50** |

Fix-only인 `1.6/Textures/.../GL_WallLit_*` 4개 중 방향 PNG 3개는 원본 root의 동명 자산을 실질적으로 덮어쓴다.

### 판정 그룹

| 그룹 | 파일 | 판정 |
|---|---|---|
| About 브랜딩 | Preview, ModIcon | 원본 기능과 무관 |
| WallLit 4 | east/north/south/ui | 현 원본 drawOffset+menuicon 방식이 같은 의도를 더 안전하게 구현; 불필요 |
| Drape 8 | main/back east/south + masks | 패치가 죽어 main만 덮이는 불완전 세트; 위험 |
| 신규 Def 9 | SmallShelf 3, WineRackMedium 6 | 해당 신규 Def 채택 시에만 필요 |
| Ratkin UI 1 | HamsterWheel UI | 호환 패치와 함께 적극 후보 |
| 고아 1 | `WoodFenceGate_Mover_east_Blueprint_Atlas` | 어느 Def도 참조하지 않음 |
| 동일 3 | BookBox north 2, gate mover east 1 | 불필요 |

동일 경로 변경 23개 중 Preview를 제외한 런타임 자산 22개의 판정:

- **가져오지 않음:** `GL_BookBox_south(_m)` — 현 원본 동적 책 그림과 중복
- **현재 원본 유지:** `RKG_WoodGate_Blueprint_Atlas` — Fix는 64×55 비정방, 원본은 최근 64×64로 개선
- **조건부 우선:** 연구대 east+mask, 도축대 east+mask
- **조건부:** Chair3 south, Cupboard north+mask, DoorFrame north/south, Shelf east/south, 발효통 `RGK_orcB` 3방향+mask, DoorOld/Fence blueprint

모든 `_m`은 본체와 함께 움직여야 한다. PNG 한 장만 가져오는 방식은 피한다.

---

## 11. About 설명과 실제 구현의 차이

| About 주장 | 실제 조사 결과 |
|---|---|
| GloomyFurniture 필수 | 맞지만 dependency는 주석 처리되어 강제되지 않음 |
| 1.4/1.5/1.6 지원 | 기능 Def는 1.6만 있고 DLL도 1.6 빌드; 1.4/1.5 근거 부족 |
| 1.5 책장 수납 | Fix의 해당 Def가 1.6 폴더에만 있음 |
| Royalty/Drape 수정 | 현 1.6에서 두 패치 모두 미실행 |
| 연구대 텍스처 수정 | 실제 east+mask 근거 있음 |
| 벽난로/침대 텍스처 수정 | Fix에 해당 PNG 변경 없음 |
| 책장 텍스처 수정 | 변경은 있으나 현 원본 동적 책장과 충돌 |
| 울타리 텍스처 조정 | 실제 blueprint 근거 있음 |
| 울타리문 그림자 제거 | Def shadow 제거는 있으나 본체 방향 PNG 변경은 없음 |
| TV UI 크기 | PNG가 아니라 Def scale/offset 변경 |
| 기타 변경 | NewRatkin, FermentingBarrel, 광범위 Harmony 우회는 설명에서 누락 |

Fix Workshop ID는 `2987876242`, 원본은 `1558635181`이다. `PublishedFileId.txt`는 절대 원본으로 복사하면 안 된다.

---

## 12. 권장 반영 순서

1. **Fix 없이 현재 원본 기준선을 먼저 smoke test**한다.
2. 침대 `SleepAccelerator`, 바닥 도색, 연구대 Comp/PlaceWorker/Laboratory, TV UI를 각각 독립 변경으로 반영한다.
3. 도색·다재료화는 파일 전체 복사가 아니라 기능군별 작은 커밋으로 나눈다.
4. 신규 Def는 하나씩 `Def + 텍스처 + 번역 + relatedBuildCommands + 저장/용량 정책`을 완성한 뒤 추가한다.
5. Royal/Hakuro/NewRatkin은 핵심 Def와 분리된 호환 패치로 유지하고 XPath를 Conditional로 방어한다.
6. 텍스처는 본체/mask 쌍과 XML offset을 함께 검증한다.
7. Fix의 유효 의도를 모두 이식한 뒤 companion Fix 사용을 중단하고, 세이브 호환을 위해 백색 Def는 Legacy로 남긴다.

필수 회귀 테스트:

- XML 로드 오류와 중복 translation key
- 기존 세이브의 책장·옷장·백색 조명
- 벽등 아래 바닥/벽 청사진, 회전 4방향
- 책장 실제 10권 표시와 연구대 연결선
- 옷장 저장·복장 정책·배출 UI
- 조리대/Bonfire의 Kitchen 판정과 이유식
- 전기 벽난로 실내 난방
- 울타리 pen pathing·FenceGate·related commands
- 도색/다재료의 각 방향과 mask
- Ideology/Royalty/Biotech/Anomaly/Odyssey 조합
- 선택 지원 시 Hakuro/NewRatkin

---

## 부록 A. Def 129개 전수 체크리스트

### A-1. 가구·오락·조명 62개

`Buildings_Furniture.xml` 40:

`RGK_bedSingle`, `RGK_bedSingleB`, `RGK_bedDouble`, `RGK_bedDoubleB`, `GL_ClassyDoubleBed`, `RGK_EndTable`, `RGK_Dresser`, `RGK_EndTableWithLamp`, `RGK_EndTableWithLampE`, `RGK_bucketr`, `RGK_orc`, `RGK_SimpleChair`, `RGK_Chair`, `RGKDiningChair`, `GL_Bench`, `RGK_TableA`, `RGK_TableC`, `RGK_TableB`, `RGK_TableD`, `RGK_TableE`, `RGK_TableF`, `RGK_TableG`, `RGK_TableH`, `RGK_MiniTableA`, `RGK_MiniTableB`, `RKGWineRackB`, `RKGWineRackTall`(신규), `WardrobeA`, `WardrobeB`, `GL_Bookshelf`, `GL_Cupboard`, `GL_TablewareShelf`, `RGK_Manger`, `GL_Shelf`, `GL_Small_Shelf`(신규), `RGK_DogBowl`, `GL_Locker`, `RGK_Flower`, `RGK_Teaset`, `GL_Teddy`

`Buildings_Furniture_Base.xml` 6, 모두 동일:

`RGKFurnitureBase`, `RGKFurnitureWithQualityBase`, `RGKArtableFurnitureBase`, `RGKTableBase`, `RGKTableGatherSpotBase`, `RGKArtableBedBase`

`Buildings_Joy.xml` 4:

`GL_ChessTable`, `GL_PokerTable`, `GL_BilliardsTable`, `GL_TubeTelevision`

`Buildings_Lighting.xml` 12:

`RGK_Fireplace`, `RGK_FireplaceE`, `RGK_LampM`, `RGK_lamp`, `RGK_lampEM`(신규), `RGK_lampE`, `RGK_Street_Lamp_L`, `RGK_Street_Lamp`, `RGK_Street_Lamp_Seven`, `RGK_Street_Lamp_SevenE`, `GL_Wall_Lamp`, `GL_Wall_LampE`

### A-2. 생산·구조·기타 67개

`Buildings_Production.xml` 9:

`RGK_SimpleResearchBench`, `GL_TableButcher`, `RGK_FueledStove`, `GL_ElectricStove`, `GL_Bonfire`, `GL_HandTailoringBench`, `GL_ElectricTailoringBench`, `GL_FueledSmithy`, `GL_ElectricSmithy`

`Buildings_Rug.xml` 10:

`RGKRugBase`, `RugA_Small`, `RugB_Small`, `RugC_Small`, `RugA`, `RugB`, `RugC`, `RugA_Large`, `RugB_Large`, `RugC_Large`

`Buildings_Structure.xml` 19:

`RKGFence`, `RGK_Wall`, `GL_Wall`, `RGK_VentWall`, `GL_VentWall`, `WallWindowBase`, `RGK_window`, `RGK_WindowWall`, `GL_WindowWall`, `RGK_WoodFence`, `RGK_Door`, `GL_FlowerWall`, `RGK_DoorOld`, `RGK_AutodoorOld`(신규), `GL_DoorFrame`, `GL_BoundaryWood`, `GL_BoundaryStone`, `GL_BoundaryWoodCorner`, `GL_BoundaryStoneCorner`

`CategoryDef.xml` 4, 모두 동일:

`AnimalFoods`, `Alcohol`, `OldStyleFurniture`, `OldStyleDeco`

`JukeBox.xml` 5:

`JukeBox`, `JoyGiver_ListenSong`, `JoyKind_ListenSong`, `JobDef_ListenSong`, `MusicForSickPeople` — `JukeBox` 도색 외 동일

`Plants_Cultivated_Decorative.xml` 3:

`DecoTree`, `GL_TreeA`, `GL_TreeB`

`Recipes_Meals.xml` 5, 모두 동일:

`CookMealPasteD`, `CookMealSimpleD`, `CookMealFineD`, `CookMealLavishD`, `CookMealSurvivalD`

`SongDef.xml` 1, 동일:

`defName` 없는 `JukeBoxStop/Stop` SongDef

`TerrainDef.xml` 11:

`WoodFloorBase`, `RGK_Terrain`, `GL_Terrain`, `PebbleBase`, `PebbleSandstone`, `PebbleGranite`, `PebbleLimestone`, `PebbleSlate`, `PebbleMarble`, `Floor_Wood`, `Floor_Pebble`

---

## 부록 B. 비Def 파일 전수 체크리스트

### B-1. 비Def XML 20

- About 1: `About/About.xml`
- Patch 8: 원본/Continued 제거, Royalty 2, VEF, Core, Hakuro, NewRatkin
- Language 11: ChineseSimplified, ChineseTraditional, English, Japanese, Russian의 WindowWall 5; Korean/RimWaldo의 Furniture·Lighting·WindowWall 6

### B-2. PNG 50

- About 2
- WallLit 4
- Drape 8
- SmallShelf 3
- WineRackMedium 6
- NewRatkin UI 1
- 고아 gate blueprint 1
- 바이트 동일 3
- 동일 경로 변경 런타임 자산 22
- 합계 **50**

### B-3. Source 14 / Assemblies 2 / ID 1

- 실제 소스·프로젝트 5
- README/IL/res 역분석 산출물 3
- obj 빌드 캐시·복제 산출물 6
- 배포 DLL/PDB 2
- Workshop PublishedFileId 1

합계와 각 담당 체크리스트가 Fix 전체 100개 파일 및 Def top-level 129개를 모두 덮는다.
