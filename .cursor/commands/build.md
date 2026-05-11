# build

GloomyFurniture **1.6** 모드만 빌드한다. 사용자가 지정한 RimWorld는 Steam 경로 `C:\Program Files (x86)\Steam\steamapps\common\RimWorld`이며, `Gloomylynx.csproj`의 HintPath가 이미 이 경로를 가리키므로 **별도 `/p:RimWorldPath`는 없다**.

## 할 일

1. 워크스페이스 루트에서 아래 **그대로** 실행한다 (경로는 리포지토리 기준 상대 경로로 맞춘다).

```powershell
& "${env:ProgramFiles}\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MSBuild.exe" "GloomyFurniture/1.6/Source/Gloomylynx.csproj" /t:Rebuild /p:Configuration=Debug /restore:false /v:m
```

2. `Community`가 없으면 `Professional` / `Enterprise`로 바꾸거나, `where.exe msbuild`로 찾은 `MSBuild.exe`를 사용한다.

3. 성공 시 출력은 `GloomyFurniture/1.6/Assemblies/Gloomylynx.dll`이다. 실패 시 컴파일 로그 전체를 보고, RimWorld **1.6** API 시그니처 문제면 프로젝트 규칙 `.cursor/rules/rimworld-16-mod-build.mdc`를 따른다.

4. 빌드 결과를 한 줄로 요약한다 (성공/실패, DLL 경로).
