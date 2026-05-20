RimWorld Vanilla Map Rendering MapDrawer SectionLayer DynamicDrawManager Graphics.DrawMesh Unity pipeline plaintext diagram

# 림월드 바닐라 맵 렌더링 구조 보고서

**범위:** 플레이 중 **맵 카메라** 기준(월드 맵 제외). 코드 단위 나열이 아니라 **업데이트 주기·데이터 흐름·드로우 콜 유발 주체** 중심.

---

## 1. 한 줄 요약

맵은 **(A) 섹션 단위로 미리 짜 둔 메시(Map mesh)** 와 **(B) 매 프레임 뷰에 맞춰 골라 그리는 동적 오브젝트(Dynamic things)** 의 **이중 트랙**이고, **터레인·정적 가까운 건물/사물**은 주로 (A), **폰·움직이는 실시간 그래픽**은 주로 (B)다. 공통 드로우 진입은 `Graphics.DrawMesh` 계열이 지배적이다.

---

## 2. 프레임에서 “맵 그림”이 도는 위치

```
Game.UpdatePlay()
  └─ (각 Map) Map.MapUpdate()
        └─ [현재 맵이고 맵 뷰일 때만]
              mapDrawer.MapMeshDrawerUpdate_First()   ← 메시 재생성(일부) 스케줄
              mapDrawer.DrawMapMesh()                 ← 배치 메시 드로우
              dynamicDrawManager.DrawDynamicThings()  ← 등록된 Thing 동적 드로우
              … 이후 오버레이·지정·플렉 등
```

- **주기:** `UpdatePlay`가 돌 때마다 **모든 로드된 맵**에 대해 `MapUpdate`가 호출된다(실제 “무거운” 맵 드로우는 `Find.CurrentMap == this` 이고 맵 뷰일 때만).
- **틱(Tick)과 분리:** 시뮬레이션 틱과 **동일하지 않을 수 있음**. 시각은 **Update 루프**에 붙는다.

---

## 3. 계층 구조 (텍스트 트리)

**전제:** 아래 `Map.MapUpdate` 가지는 **현재 맵 + 맵 카메라**일 때만 돈다. `SectionLayer` 목록은 `typeof(SectionLayer)`의 **비추상 서브클래스를 리플렉션으로 전부 생성**하는 방식이라, **게임/DLC 버전**에 따라 항목이 늘거나 조건(`Visible`, DLC 플래그)으로 빠질 수 있다. (이 문서는 워크스페이스 `RimworldSource` 스냅샷 기준.)

### 3.1 `Map.MapUpdate` 안 “그리기” 순서 (상위)

```
Map.MapUpdate (drawingMap && CurrentMap)
├── mapDrawer.MapMeshDrawerUpdate_First()     … dirty 섹션/글로벌 레이어 메시 재생성(분산)
├── mapDrawer.DrawMapMesh()                   … 아래 3.2 + 3.3
├── dynamicDrawManager.DrawDynamicThings()  … 아래 3.4
├── gameConditionManager.GameConditionManagerDraw() … 날씨·암흑 등 게임조건 전역 이펙트
├── MapEdgeClipDrawer.DrawClippers()        … 맵 가장자리 클리핑 메시
├── designationManager.DrawDesignations()   … 지정(채굴/수확/이동 등) 마킹·미리보기
├── overlayDrawer.DrawAllOverlays()         … 금지·전력·연료 등 Thing 메타 오버레이
├── temporaryThingDrawer.Draw()             … 임시 위치에 Thing을 DrawNowAt로 덧그림
└── flecks.FleckManagerDraw()               … 스파크·먼지 등 짧은 수명 이펙트
```

`MapInterface.MapInterfaceUpdate()` 등은 **별도**로 설계 박스·그리드 오버레이 등을 그린다(Update 경로).

### 3.2 `MapDrawer` — Global `MapDrawLayer` (섹션이 아닌 맵 전역 1인스턴스)

`SectionLayer`를 제외한 `MapDrawLayer` 서브클래스만 여기 등록. **각 타입마다 Mesh/Material 서브메시 여러 개** 가능.

```
Global (예: 본 트리에선 2종)
├── MapDrawLayer_ExteriorLightingOverlay … 맵 바깥(가장자리 밖) 조명 오버레이용 큰 평면(디버그·클리퍼 연동)
└── MapDrawLayer_OrbitalDebris          … 맵 상공 궤도 파편 장식(랜덤 배치 메시, 시각 배경)
```

### 3.3 `MapDrawer` — `Section` + `SectionLayer` (17×17 셀 블록마다 동일 타입 세트)

각 섹션은 **같은 종류의 SectionLayer 인스턴스 묶음**. 대부분 **dirty 시 `Regenerate`로 Mesh 빌드 → 매 프레임 `DrawLayer`에서 `Graphics.DrawMesh`**.

```
Section[,]
├── [지형·바닥·장식]
│   ├── SectionLayer_Terrain           … 바닥 타일(지형Def 머티리얼별로 쿼드 적층)
│   ├── SectionLayer_Watergen          … 수심/물 표현용(터레인과 연동, WaterDepth 서브카메라 레이어로 Draw)
│   ├── SectionLayer_TerrainScatter    … 지형에 붙는 잔디/자갈 등 ScatterableDef 장식
│   ├── SectionLayer_TerrainEdges      … 터레인 모서리/경계 메시(RimWorld)
│   ├── SectionLayer_BridgeProps       … 다리/파운데이션 아래 받침 장식(터레인 연동)
│   └── SectionLayer_SubstructureProps … 서브스트럭처(오딧시) 테두리·받침 장식
│
├── [눈·모래·가스]
│   ├── SectionLayer_Snow              … 눈 깊이에 따른 덮개 메시
│   ├── SectionLayer_Sand              … 모래(바람에 쓸린 모래 등) 덮개
│   ├── SectionLayer_Gas               … 가스 구름 베이스
│   └── SectionLayer_PollutionCloud    … 오염 구름(Biotech, Gas 계열 상속)
│
├── [건물·사물을 “맵 메시에 Print”]
│   ├── SectionLayer_ThingsGeneral     … MapMesh 쓰는 Thing 전반(가구·식물·건물 그래픽 Print)
│   └── SectionLayer_ThingsPowerGrid   … 전력망 오버레이 켰을 때만 Draw; 플레이어 건물 PrintForPowerGrid
│
├── [그림자·실내·조명·안개]
│   ├── SectionLayer_IndoorMask        … 실내/지붕 밖 마스크(그림자·조명 셰이더용, 디버그 마스크 옵션)
│   ├── SectionLayer_SunShadows        … Dynamic: 태양 그림자(뷰 밖 섹션에서도 조건부 DrawDynamic)
│   ├── SectionLayer_EdgeShadows       … Dynamic: 가장자리/접지 그림자
│   ├── SectionLayer_LightingOverlay   … 지면 글로우·지붕 등에 따른 조명 오버레이 색 버텍스
│   └── SectionLayer_Darkness            … 암흑 게임조건 시 어둠 덮개(가시 시)
│
├── [UI에 가까운 월드 오버레이를 메시로]
│   ├── SectionLayer_FogOfWar          … 안개 밝기/가림(안개 그리기 옵션)
│   ├── SectionLayer_Zones               … 구역 색(구역 오버레이·ShouldDrawZones일 때만 Draw)
│   └── SectionLayer_Plans               … 건설 계획 평면(계획 오버레이·스크린샷 모드 예외)
│
├── [피해·특수]
│   └── SectionLayer_BuildingsDamage   … 건물 파손 시 긁힘/크랙 등 데미지 비주얼 Print
│
├── [DLC/특수 맵]
│   ├── SectionLayer_GravshipHull      … 중력선 외곽선·선체 코너 메시
│   ├── SectionLayer_GravshipMask        … 중력선 그림자 마스크 등(정적 플래그 연동)
│   └── SectionLayer_DebugNoise          … 디버그 노이즈(개발용)
│
└── (내부) SectionLayer_Dynamic 서브클래스 … `DrawDynamicSections`에서만 추가로 그릴 수 있음
```

### 3.4 `DynamicDrawManager` (맵 메시와 별도 파이프)

```
DynamicDrawManager
└── 스폰된 Thing 중 def.drawerType != None 인 것만 리스트 보유
    ├── Cull … 카메라 뷰·안개·눈/모래 깊이 등으로 shouldDraw 결정(Job)
    ├── (옵션) ParallelPreDraw / EnsureInitialized … 폰·그래픽 초기화·병렬 준비
    ├── Draw … Thing.DynamicDrawPhase(Draw) → ThingDef에 따라 Graphic.Draw / PawnRenderer 등
    └── 실루엣 … 하이라이트 가능 시 추가 패스
```

**그리는 유형:** 폰·동물·맵 메시에 안 실리는 실시간 사물·일부 이중 드로우(`MapMeshAndRealTime`) 등. **지형 타일 자체**나 **MapMeshOnly 건물 본체**는 여기가 아니라 위 **SectionLayer** 쪽이 담당.

### 3.5 `Map.MapUpdate` 직후 단계 (요약)

| 구성요소 | 주로 그리는 것 |
|----------|----------------|
| `GameConditionManagerDraw` | 날씨·이온스톰·암흑 등 **조건별 전역** 비주얼 |
| `MapEdgeClipDrawer` | 카메라 경계 클리핑 |
| `DesignationManager` | **지정** 브러시·하이라이트 |
| `OverlayDrawer` | **금지 / 전력 필요 / 연료** 등 Thing 상태 아이콘·메타 오버레이 |
| `TemporaryThingDrawer` | **이동 중 미리보기** 등 임시로 Thing을 다른 좌표에 그림 |
| `FleckManager` | **플렉**(짧은 VFX, 메시/쿼드 기반) |

---

## 4. 개체별로 무엇이 “메시 재생성”이고 무엇이 “매 프레임 드로우”인가

### 4.1 ThingDef.drawerType (의사결정 루트)

| drawerType | Map mesh(섹션 레이어에 Print) | DynamicDrawManager |
|------------|-------------------------------|---------------------|
| None | 안 함 | 안 함 |
| RealtimeOnly | 기본적으로 **안 함** | **함** |
| MapMeshOnly | **함** | **안 함** (`DynamicDrawPhase` 조기 return) |
| MapMeshAndRealTime | **함** | **함** (정지 스냅샷 + 움직임 보정 등 용도) |

- **폰:** 보통 **RealtimeOnly 쪽 패턴**(동적 목록 + `PawnRenderer`)이 중심.
- **바닥·대부분 건물·정적 사물:** **MapMeshOnly / MapMeshAndRealTime** → **지형/사물 SectionLayer**가 `Regenerate` 때 `Thing.Print`로 버텍스를 박아 넣음.

### 4.2 터레인

- **SectionLayer_Terrain** 등: **터레인 변경 플래그**가 올라온 섹션만 `Regenerate`에서 셀 루프 → 서브메시에 사각형 추가.
- **매 프레임:** “전 셀 다시 계산”이 아니라, **Dirty일 때만** 메시를 다시 짠다. **DrawLayer는 보통 매 프레임** `Graphics.DrawMesh`로 **이미 만든 Mesh**를 제출.

### 4.3 빌딩

- **정적 표현:** `SectionLayer_Things*` 계열이 `thingGrid`를 훑어 `TakePrintFrom` → `Graphic.Print`로 **배치 메시**에 합침.
- **트리거:** 건물 스폰/파괴/회전/재질 등으로 `MapMeshDirty` / `DirtyMapMesh`류가 불리면 **해당 섹션(때로 인접)**에 **비트 플래그**가 쌓임 → 이후 `Regenerate`에서만 CPU 비용.

### 4.4 폰

- **등록:** 스폰 시 `DynamicDrawManager` 리스트에 올라감(`drawerType` 필터).
- **매 프레임:** **컬링 Job**(뷰렉트·안개·눈/모래 깊이 등)으로 `shouldDraw` 결정 → 보이는 것만 `DynamicDrawPhase(Draw)` → 내부에서 `Graphic.Draw` / `PawnRenderer`의 `Graphics.DrawMesh` 등 **다건의 DrawMesh 호출**이 나갈 수 있음.

---

## 5. MapDrawer 한 사이클 안에서의 역할 분리

### 5.1 `MapMeshDrawerUpdate_First` (CPU 메시 작업 분산)

- **글로벌 레이어:** `globalDirtyFlags`에 맞는 레이어를 `Regenerate`.
- **섹션:** `dirtyFlags`가 있는 섹션의 레이어를, **카메라가 겹치는지**에 따라  
  - 겹치면 **즉시 `Regenerate`**하고 한 프레임 일을 줄이거나,  
  - 안 겹치면 **플래그만 남겨** 나중 `DrawSection`에서 처리하는 **지연 재생성** 경로가 있다.
- 목적: **한 프레임에 전 맵 재빌드 방지** (시간 분산).

### 5.2 `DrawMapMesh` (GPU 제출 위주)

1. **Global 레이어:** 여전히 Dirty면 여기서도 `Regenerate` 보정 후 `DrawLayer`.
2. **각 Section:**
   - **뷰와 겹치면 `DrawSection`:** 필요 시 지연된 dirty 레이어 `Regenerate` 후 **모든 SectionLayer `DrawLayer`**.
   - **뷰와 안 겹치면 `DrawDynamicSections`:** `SectionLayer_Dynamic`만 조건부 드로우(예: 그림자 레이어가 화면 밖에서도 의미 있을 때).

---

## 6. “드로잉 콜”을 무엇이 유발하는가 (개념)

| 유발 주체 | 유발 조건 | 결과 형태 |
|-----------|-----------|-----------|
| `MapDrawLayer.DrawLayer` | 레이어 Visible, 서브메시 finalized | **Material별 Mesh**를 `Graphics.DrawMesh(..., renderLayer)`로 **인스턴스마다 1회에 가까운 패턴** |
| `Graphic.DrawMeshInt` | 동적 Thing이 컬 통과 후 Draw 단계 | `Graphics.DrawMesh(mesh, pos, rot, mat, 0)` |
| `PawnRenderer` | 폰이 컬됨 | 아틀라스/메시 조합으로 **추가 DrawMesh** |
| 오버레이/플렉/지정 | 매 Update의 후반 | 라인·GUI·메시 등 **별 파이프** |

**정리:**  
- **배치 맵 메시:** “셀/섹션 단위 변경” 이벤트 → **CPU에서 Mesh 재구성** → **매 프레임 DrawMesh로 제출**.  
- **폰/실시간 Thing:** **매 프레임 컬 + Draw** (내용이 안 바뀌어도 Draw 호출은 간다. 다만 컬로 수 줄임).

---

## 7. Unity API (자주 나오는 수준)

- **`Graphics.DrawMesh`**: 맵 레이어 서브메시, 일반 `Graphic`, 일부 그림자.
- **`Mesh`**: `LayerSubMesh`가 들고 있음; `Regenerate`/`FinalizeMesh`에서 버텍스·트라이 갱신.
- **`Material` / `Shader`**: `ShaderDatabase`, `MaterialPool` 경유가 일반적.
- **Jobs (`IJobParallelFor`, Burst)**: `DynamicDrawManager`의 **컬링·실루엣 행렬** 등 (드로우 본체는 여전히 메인 스레드 쪽이 많음).
- **카메라:** `Find.CameraDriver.CurrentViewRect`로 **가시 섹션·컬** 결정.

---

## 8. 시각화 — 파이프라인

> Mermaid 다이어그램은 일부 뷰어(기본 MD 미리보기 등)에서 **코드 블록으로만** 보이므로, 동일 내용을 **텍스트 플로우 + 표**로만 적는다. Mermaid가 필요하면 [mermaid.live](https://mermaid.live)에 아래 표/플로우를 옮겨 그리면 된다.

### 8.1 한 프레임 맵 렌더 상위 흐름

**전제:** `Find.CurrentMap == this` 이고 맵 카메라(`DrawingMap`)일 때만 아래 가지가 실행된다.

`Game.UpdatePlay` → `Map.MapUpdate` →

1. `mapDrawer.MapMeshDrawerUpdate_First()` — dirty 누적 반영, 섹션/글로벌 레이어 메시 재생성(분산)
2. `mapDrawer.DrawMapMesh()` — Global `MapDrawLayer` + 각 `Section`의 `SectionLayer.DrawLayer`
3. `dynamicDrawManager.DrawDynamicThings()` — 등록 Thing 컬링 후 동적 드로우
4. `gameConditionManager.GameConditionManagerDraw` → `MapEdgeClipDrawer` → `designationManager` → `overlayDrawer` → `temporaryThingDrawer` → `flecks` (순서는 위 **3.1절**과 동일)

### 8.2 데이터 소스 → 출력 (트랙 비교)

| 트랙 | 데이터 소스 | CPU 쪽(메시 갱신) | GPU 제출 |
|------|-------------|-------------------|----------|
| **섹션 배치 메시** | `TerrainGrid`, `thingGrid`, 각종 그리드 | `SectionLayer.Regenerate`에서 버텍스 적층 → `FinalizeMesh` | `MapDrawLayer.DrawLayer` → `Graphics.DrawMesh` (서브메시 단위) |
| **동적 Thing** | 스폰된 Thing + `drawThings` 리스트 | (대부분 매 프레임) 컬 Job, PreDraw/EnsureInitialized | `Thing.DynamicDrawPhase` → `Graphic` / `PawnRenderer` → `Graphics.DrawMesh` 등 |

**플로우 요약**

- 배치: `그리드/Thing` → `Regenerate` → `Mesh + Material (subMeshes)` → `DrawMesh`
- 동적: `스폰 시 RegisterDrawable` → `매 프레임 Cull` → `DynamicDrawPhase` → `Graphic / PawnRenderer`

### 8.3 `drawerType` 결정 (요약)

| `ThingDef.drawerType` | Section에 `Print` | `DynamicDrawManager` |
|-----------------------|-------------------|----------------------|
| None | ✕ | ✕ |
| RealtimeOnly | ✕ (ThingsGeneral 기준) | ○ |
| MapMeshOnly | ○ | ✕ (`DynamicDrawPhase` 조기 return) |
| MapMeshAndRealTime | ○ | ○ |

---

## 9. 오해하기 쉬운 점

- **“매 프레임 전부 재계산” 아님:** 터레인·대부분 건물 메시는 **dirty 섹션**에서만 `Regenerate` CPU 비용이 큼. **Draw는 매 프레임**이어도 **메시 내용 갱신은 이벤트 구동**.
- **`SectionLayer_Things`의 `DrawLayer`:** 디버그/컷신 등 특수 조건에서 막히는 경우가 있어, **“메시에만 있고 실제로는 다른 경로가 메인”**인 타입도 있다(버전·설정 의존). 기본 개념은 **Print = 맵 메시 / Draw = 제출**이다.
- **폰 그림자만 뷰 밖:** `DynamicDrawManager`에서 폰만 그림자만 그리는 분기가 있음(컬 결과에 따라).

---

## 10. 참고 소스 위치 (로컬 `RimworldSource` 기준)

- `Verse/Map.cs` — `MapUpdate` 안 드로우 순서
- `Verse/MapDrawer.cs` — 섹션·글로벌 레이어, `DrawMapMesh` / `MapMeshDrawerUpdate_First`
- `Verse/MapDrawLayer.cs` — `Regenerate` / `DrawLayer` → `Graphics.DrawMesh`
- `Verse/Section.cs` — `DrawSection` vs `DrawDynamicSections`
- `Verse/DynamicDrawManager.cs` — 등록 Thing 리스트, 컬, `DrawDynamicThings`
- `Verse/Thing.cs` — `drawerType`, `DynamicDrawPhase`, `Print`, `DirtyMapMesh`
- `Verse/DrawerType.cs` — 열거형 정의

---

*본 문서는 워크스페이스에 포함된 디컴파일/소스 트리 `RimworldSource`의 흐름을 기준으로 정리했다. 게임 패치에 따라 클래스명·분기는 변할 수 있다.*
