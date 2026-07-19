GloomyFix GloomyFurniture Fix companion mod Xml Patch Def list 1.6 DefInject Royalty VEF GloomyWallLampFix Harmony GenConstruct CanPlaceBlueprintOver

# Gloomy Furniture Fix (`GloomyFix`) — 수정 대상·성격 요약

**대상 모드**: `Solaris.FurnitureBase`(Gloomy Furniture), `Gloomy.Furniture.Continued`(선택) 이후 로드. **Harmony** 의존. 가구 ThingDef 등은 글루미 본모드 **`Gloomylynx.*`**(어셈블리 `Gloomylynx.dll`)을 참조하고, Fix 모드는 **`Assemblies/Gloomy Furniture Fix.dll`**(내부 어셈블리명 `GloomyWallLampFix`)로 **추가 Harmony 패치**를 싣는다.

**방식 요약**: (1) Xml Patch로 원본 일부 Def 제거 (2) 동일 `defName`으로 `1.6/Defs`에서 ThingDef·TerrainDef·RecipeDef 등을 **통째 재정의**해 덮어씀 (3) 타 모드·DLC Def에 Xml Patch로 항목 추가 (4) 다국어 `DefInject` (5) Fix 전용 DLL로 **청사진 겹침** 등 일부 바닐라 로직을 Harmony로 조정.

---

## 1. Xml Patch — 원본 Gloomy에서 **제거**하는 것

- **흰색 조명 ThingDef 일련** (`GL_Wall_Lamp_White`, `GL_lamp_White`, 가로등·가로등7등 `_White` 변형 등) — `GloomyFurniture` / `GloomyFurniture (Continued)` 각각에 대해 동일 제거.

---

## 2. C# — `Assemblies/Gloomy Furniture Fix.dll` (`GloomyWallLampFix`)

레포 복원 소스: `GloomyFix/Source/` (`GloomyWallLampFix.il` = 원본 DLL ildasm 덤프와 동일).

| 항목 | 수정·개입 내용 |
|------|----------------|
| **Harmony ID** | `rabiosus.GloomyWallLampFix` — 시작 시 `PatchAll()`, 로그 `[GloomyWallLampFix]`. |
| **`GenConstruct.CanPlaceBlueprintOver`** | **Prefix** + `[HarmonyBefore("com.Gloomylynx.rimworld.mod")]` → `com.Gloomylynx.rimworld.mod` ID를 쓰는 다른 Harmony보다 **먼저** 실행. |
| **조건** | 새 건축물 `BuildableDef.placeWorkers`에 **`Placeworker_AttachedToWall`**가 포함된 경우. |
| **결과** | `__result = true`로 두고 **원본 메서드 및 이후 패치 체인을 건너뜀** → 벽 부착형 건물(벽 조명 등) **청사진이 다른 것과 겹칠 때 허용**되도록 우회하는 역할. |

---

## 3. Xml Patch — **바닐라 / DLC / 타 모드** Def를 고치는 것

| 대상 | 수정 내용(리스트) |
|------|-------------------|
| **Royalty** (`1.6/Patches/Royalty.xml`) | 커튼 `Drape`: 고도·통과·그리기·뒤집힌 레이어용 `Gloomylynx.CompProperties_SecondLayerFollow` 추가, 사격 관련 설정 추가(1.6). `RoyalTitleDef` 침실 요건에 글루미 엔드테이블·드레서·옷장 계열 `defName` 추가. |
| **Royalty** (`Patches/Royalty.xml`, 구조 유사) | 위와 유사하나 `disableImpassableShotOverConfigError` 없음(구버전용으로 추정). |
| **Vanilla Expanded Framework** | 여러 `RoyalTitleDef`의 침실 요건에 `GL_ClassyDoubleBed`, 엔드테이블·드레서·옷장 계열 추가. |
| **Hakuro Xenohuman** | 해당 모드 작위 침실의 침대 요건에 `RGK_bed*`·`GL_ClassyDoubleBed` 추가. |
| **Core** (`Patches/Core.xml`) | `FermentingBarrel`에 UI 아이콘 스케일·오프셋 추가. |
| **New Ratkin Plus** | 설교단 UI 오프셋, 햄스터 휠 발전기 UI, 재봉대에 `Make_Patchleather` 레시피 추가. |

---

## 4. `1.6/Defs` — 글루미 원본과 동일 `defName`으로 **재정의·대체**하는 범위(파일 단위)

| 파일 | 무엇을 다루는지 |
|------|----------------|
| `Buildings_Furniture_Base.xml` | 가구 베이스 추상 Def: `BuildingsFurniture` 분류, **미니화** 공통, 테이블/침대 베이스 등. |
| `Buildings_Furniture.xml` | 침대·테이블·의자·소파·드레서·옷장·선반·책장·식기장·장식품 등 대부분의 **가구 본체**. |
| `Buildings_Structure.xml` | **벽**, 창호/문/울타리/환풍 등 **구조물**, `RGK_AutodoorOld` 등. 벽은 `thingClass`가 `Gloomylynx.GL_Building`. |
| `Buildings_Production.xml` | 연구대·조리대·작업대 등 **생산 건물**. |
| `Buildings_Lighting.xml` | 조명류(원본에서 제거한 흰색 조명 대체 포함 가능). |
| `Buildings_Joy.xml` | TV 등 **쾌락** 건물. |
| `Buildings_Rug.xml` | 러그·카펫류. |
| `JukeBox.xml` | 주크박스 ThingDef. |
| `TerrainDef.xml` | 나무·자갈 바닥 등 **지형**: `isPaintable`, 드롭다운 그룹 등. |
| `Recipes_Meals.xml` | 조리대용 **배치 조리** 등 추가 RecipeDef. |
| `Plants_Cultivated_Decorative.xml` | 장식 나무(`GL_TreeA`/`GL_TreeB` 등). |
| `SongDef.xml` | 주크박스 정지용 등 **SongDef** 한 건. |
| `CategoryDef.xml` | `OldStyleFurniture` / `OldStyleDeco` 설계 카테고리, `AnimalFoods`·`Alcohol` ThingCategory(저장 필터용). |

---

## 5. 재정의 Def들의 **공통·반복되는 수정 성격**(기능 리스트)

- **도색**: 가구·벽·생산설비·선반·러그 등 `paintable` / 지형 `isPaintable`.
- **재료**: 다수 건물에 나무·돌·금속 등 `stuffCategories` 확장 및 `costStuffCount` 조정(원본 글루미 Def 대비 밸런스·제작 비용 변경).
- **침대 연동**: `SleepAccelerator`(Ideology)를 침대 `linkableFacilities`에 포함.
- **시설 거리·수치**: 예) `RGK_Dresser` 시설 `maxDistance` **6**(About의 “드레서 사거리” 변경과 일치).
- **선반·책장 저장**: 선반 `maxItemsInCell` 등으로 **칸당 저장 밀도** 변경, `defaultStorageSettings`에 **여러 ThingCategory** 허용(선반 “여러 종류” 수납).
- **책장**: `BookcaseBase` 상속 `GL_Bookshelf`, 칸당 `maxItemsInCell`, 연구대와의 연동(연구대에 책장 연결 표시 PlaceWorker 등은 `Buildings_Production.xml`).
- **조리·바이오텍**: 조리대(연료/전기 등)에 `Make_BabyFood` / `Make_BabyFoodBulk` 레시피 연결(Biotech `MayRequire`).
- **미니화**: 다수 가구·조명·주크박스·일부 구조물에 `MinifiedThing` 등.
- **커스텀 컴포넌트**: `Gloomylynx.CompProperties_SecondLayer` / `SecondLayerOnOffable` / `SecondLayerFollow` — 조명·가구 **이중 레이어 그래픽** 등.
- **관련 건설 UI**: 벽 선택 시 문·자동문·쿨러 등 `relatedBuildCommands`, 울타리와 문·우리 등 연계.
- **자동문**: `RGK_AutodoorOld` 및 연구 `Autodoors` 태그 등(구식 자동문 추가·연동).
- **PlaceWorker**: 시설 연결 표시, 주크박스/일부 조명 등 배치 제한 보조.

---

## 6. 번역

- `Languages/*/DefInjected/ThingDef/` — 가구·창문벽·조명 라벨 등 **번역 오버레이**.

---

## 7. About 설명만 있고, **이 레포 Xml만으로는 근거가 약한** 항목

- **다리 위 벽/울타리 설치**, **장교/휘장 성격 변경**, 일부 **텍스처·그림자** 수정은 `About.xml`에 명시되어 있으나, Xml·Fix DLL만으로는 전부 입증되지 않는다. 벽 등은 **`Gloomylynx.GL_Building`**(글루미 본모드 `Gloomylynx.dll`)과 **에셋**에 더 묶여 있다.
- Fix 전용 **`Gloomy Furniture Fix.dll`**은 위 항목 전체가 아니라, 확인된 범위에서는 **`CanPlaceBlueprintOver` 한 지점**만 건드린다.

---

## 8. 한 줄 결론

`GloomyFix`는 글루미 퍼니처의 **동일 defName 세트를 “Fix판”으로 재작성**해 게임에 올리고, **조명 화이트 변형만 Xml로 원본에서 제거**하며, **`Gloomy Furniture Fix.dll`로 벽 부착 건물 청사진 겹침을 Harmony에서 선처리**하고, **왕실·VEF·하쿠로** 침실 규칙과 **랫킨·발효통** 등 **주변 모드/바닐라**에 소규모 Xml 패치를 더한 **동반(오버라이드) 모드**다.
