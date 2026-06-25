# 전우치 : 도사열전

> Unity 기반 팀 개발 수집형 RPG 출시 프로젝트

Unity와 C#으로 개발한 모바일 수집형 RPG로, 10인 덱 편성과 다양한 던전 콘텐츠를 핵심으로 하는 한국 배경 판타지 게임입니다. Google Play와 App Store에 출시하여 라이브 서비스 운영 경험을 쌓았으며, Firebase 분석·광고·IAP·어드레서블 리소스 스트리밍 등 모바일 게임 개발 부터 운영까지 사이클 전체를 직접 경험했습니다. 

## Store / Play Video

| 구분 | 링크 |
|------|------|
| Google Play Store | [전우치 : 도사열전](https://play.google.com/store/apps/details?id=com.root3.ktf&pcampaignid=web_share) |
| App Store | [전우치 : 도사 열전](https://apps.apple.com/kr/app/전우치-도사-열전/id6752601495) |
| Play Video | [YouTube Playlist](https://youtube.com/playlist?list=PLrjbH_QJp7ALeTfFHhBUgF8_da-_p4iej&si=NICIk1MXh82QM4Hp) |

<br/>

## Project Overview

| 항목 | 내용 |
|------|------|
| 프로젝트명 | 전우치 : 도사열전 |
| 장르 | 수집형 RPG |
| 개발 형태 | 팀 개발 |
| 플랫폼 | Android / iOS (모바일) |
| 출시 여부 | Google Play · App Store 출시 |
| 엔진 | Unity 2022 LTS |
| 개발 언어 | C# |
| 주요 구현 | 가챠, 길드, 인벤토리, 장비, 훈련, 별자리, 패스, 미니맵, 다중 던전 콘텐츠,  IAP, Addressables |

<br/>

## Tech Stack

### Engine / Language
![Unity](https://img.shields.io/badge/Unity-000000?style=flat-square&logo=unity&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120?style=flat-square&logo=csharp&logoColor=white)

- Unity 2022 LTS 기반 Android / iOS 플랫폼 빌드

### Data / Save
![Firebase](https://img.shields.io/badge/Firebase-FFCA28?style=flat-square&logo=firebase&logoColor=black)

- `ClientLocalDB_Simple` 커스텀 로컬 DB로 JSON 기반 테이블 데이터 추상화
- BestHttp 라이브러리를 통한 서버 API 연동 (로그인, 유저 데이터 동기화)
- Firebase Crashlytics / Analytics로 실시간 크래시 추적 및 지표 수집

### Release / Monetization
![GooglePlay](https://img.shields.io/badge/Google_Play-414141?style=flat-square&logo=googleplay&logoColor=white)
![AppStore](https://img.shields.io/badge/App_Store-0D96F6?style=flat-square&logo=appstore&logoColor=white)
![AdMob](https://img.shields.io/badge/AdMob-EA4335?style=flat-square&logo=google&logoColor=white)

- Unity IAP + Google Play Billing / Apple StoreKit으로 인앱 결제 구현
- AdMob 보상형 광고 연동, 광고 제거 상품 처리

### Resource / Optimization
![Addressables](https://img.shields.io/badge/Addressables-000000?style=flat-square&logo=unity&logoColor=white)

- Unity Addressables로 번들 분리 및 런타임 스트리밍 다운로드
- Object Pooling으로 전투 이펙트·유닛 생성 비용 절감
- Spine 2D 스켈레탈 애니메이션 (`SkeletonGraphic`) 다중 파츠 렌더링

### UI / Localization
- uGUI 기반 복잡한 UI 계층 구조 설계 (75+ 아이템 데이터 클래스)
- `RedDotManager`로 전역 알림 뱃지 상태 관리
- 온보딩 튜토리얼 마스크 시스템 (UI-Onboarding-Tutorial-Mask)

### Build / Dependency
- AppGuard 보안 플러그인 통합
- Google Sheets 연동으로 기획 데이터 자동 갱신 (`GSpreadSheets`)
- 조건부 컴파일 심볼(`IAP`, `CHAT`, `SINGULAR`, `CRASHLYTICS`, `ANALYTICS`)로 빌드 변형 관리

<br/>

## Main Features

| 기능명 | 설명 |
|--------|------|
| Deck System | 최대 10인 캐릭터 덱 편성, 슬롯 해금 및 진형 배치 |
| Multi-Dungeon Contents | 골드·장비·별자리·타워·각성·타임어택 등 6종 이상 던전 |
| Inventory | 보유 캐릭터·장비·아이템 목록 조회 및 관리 |
| Equipment System | 장비 장착·강화·세트 효과 적용 |
| Training System | 캐릭터 스탯 성장을 위한 반복 훈련 콘텐츠 |
| Constellation System | 별자리 해금·조합으로 캐릭터 능력치 강화 |
| Guild System | 길드 보스 레이드, 가입·승인·랭킹 관리 |
| Pass System | 배틀패스·월정액 보상 단계별 수령 및 진행도 표시 |
| Gacha System | 소환 연출(Spine 애니메이션), 위시리스트, 천장 보장 |
| Shop | 패키지, 한정 상점 등 다중 상점 구조 |
| Minimap | 필드 탐색용 미니맵 및 현재 위치 표시 |
| Fishing Mini-Game | 타이밍 기반 낚시 미니게임 별도 씬 구성 |
| Field Exploration | 타일맵 기반 월드 탐색, 워프 포인트, NPC 대화 |
| Object Pooling | 전투 오브젝트 풀링으로 스폰 GC 비용 최소화 |
| Addressable Patch | 앱 업데이트 없이 리소스 핫픽스 가능한 패치 씬 구성 |

<br/>

## Project Structure

```text
KTF/
├── Assets/
│   ├── 3rdParty/            # 외부 SDK (GSpreadSheets, AppGuard 등)
│   ├── AddressableAssetsData/ # Addressables 번들 설정
│   ├── Arts/                # 비주얼 에셋 (Animation, Effects, Fonts, Shaders, Sprites)
│   ├── Bundles/             # 런타임 에셋 번들 (Shared, Skills)
│   ├── Firebase/            # Firebase 설정 파일
│   ├── Plugins/             # 네이티브 플러그인 (AppleAuth, AdMob 등)
│   ├── Resources/
│   │   └── Prefabs/         # 런타임 로드 프리팹 (UI, Squad, Pool, SpawnObject)
│   ├── Scenes/              # 씬 파일 (Title, Field, Dungeon, WorldMap 등)
│   └── Scripts/
│       ├── Battle/          # 전투 시스템 (Dungeon, Field, Squad, Skills, State)
│       ├── DB/              # ClientLocalDB_Simple — 로컬 데이터 테이블
│       ├── ItemData/        # UI 아이템 데이터 클래스 (75종+)
│       ├── Manager/         # 싱글턴 매니저 (UI, Sound, Shop, IAP, RedDot 등)
│       ├── MiniGames/       # 낚시 미니게임
│       ├── Scene/           # 씬 전환 및 로딩
│       ├── System/          # 서버 API, 네트워크 (BestHttp_GameManager)
│       ├── Tile/            # 타일맵 유닛, 입구, 대화 타일
│       ├── UI/              # UI 패널 및 컴포넌트
│       ├── Unit/            # 캐릭터/유닛 정의
│       └── User/            # UserInfoData — 플레이어 계정 데이터
├── ProjectSettings/
└── Packages/
```

<br/>

## Core Implementation

## Core Implementation

### 1. 다중 던전 콘텐츠 구조

골드·장비·별자리·타워·각성·길드보스·타임어택 등 다양한 던전을 하나의 `DungeonScene`에서 파라미터로 분기하여 관리했다.

**구현 내용**
- 던전 타입별 `DBKey`(`Dungeon`, `TowerDungeon` 등) 복합키 설계로 레벨별 수치 테이블화
- `FieldDungeon`, `GuildBoss`, `Ranking`, `TimeAttack`, `Awake` 각 던전 클래스 상속 계층 분리
- 타워 던전은 Human·Guardian·Crusher·Celestial 4개 팩션별 독립 진행 상태 관리
- 입장 조건(레벨, 티켓, 쿨타임) 검증을 서버 API 호출 전 클라이언트에서 선검증
- 던전 결과 보상 계산을 서버에서 처리하고 클라이언트는 UI 표시만 담당하는 구조 유지

**배운 점**
- 씬을 공유하면서 던전 종류를 파라미터로 분기하는 구조가 씬 로딩 비용을 줄이는 데 효과적이었다.
- 클라이언트 선검증과 서버 최종 검증의 이중 구조가 불필요한 API 호출을 줄이면서도 치팅을 방지한다.

---

### 2. ClientLocalDB_Simple — 로컬 데이터 테이블 추상화

기획 데이터(몬스터 스탯, 던전 수치, 상점 아이템 등) 수백 개의 테이블을 단일 인터페이스로 조회할 수 있도록 커스텀 로컬 DB 레이어를 구현했다.

**구현 내용**
- `GetDB<T>(DBKey)` — 전체 테이블을 `Dictionary<string, T>`로 반환, LINQ 조합 가능
- `GetData<T>(DBKey, id)` — 단일 row 조회, `id`는 `object` 타입으로 int·string·Enum 모두 수용
- `BuildingLevelInfo`, `FieldDetail`, `TowerDungeon` 등 복합키 테이블은 `"id1_id2"` 패턴으로 키 join
- `_dbMetas` 딕셔너리에 DBKey별 `KeyFields` 메타 등록으로 키 생성 로직 중앙화
- Google Sheets 연동(`GSpreadSheets`)으로 기획팀 수정사항을 에디터에서 자동 갱신

**배운 점**
- 복합키 설계를 DB 레이어에 캡슐화하면 호출부 코드가 단순해지고 키 규칙 변경 시 영향 범위가 최소화된다.
- 전체 테이블 조회(`GetDB`)와 단일 row 조회(`GetData`)를 API 수준에서 분리하면 호출 의도가 명확해진다.

---

### 3. 길드 & 소셜 시스템

길드 보스 레이드, 가입·승인, 길드원 랭킹을 포함한 소셜 레이어를 서버 API와 연동하여 구현했다.

**구현 내용**
- 길드 생성·가입 신청·승인/거절 플로우를 API 요청-응답 패턴으로 구현
- `GuildApproval`, `GuildRanking` 아이템 데이터 클래스로 UI 바인딩 분리
- 길드 보스 레이드: 멤버별 기여도 집계, 보상 분배 결과를 서버에서 수신 후 UI 갱신
- 실시간 채팅(`UnityChatClient`)으로 길드 내 소통 채널 지원
- 랭킹 조회는 페이지네이션 방식으로 서버 부하 분산

**배운 점**
- 소셜 기능은 클라이언트 상태와 서버 상태가 자주 어긋나므로, 조작 후 반드시 서버 응답으로 최종 상태를 갱신하는 패턴을 철저히 지켜야 한다.
- 채팅처럼 지속 연결이 필요한 기능은 씬 전환에도 끊기지 않도록 매니저 생명주기를 `DontDestroyOnLoad`로 관리해야 한다.

---

### 4. Addressables 기반 리소스 스트리밍

앱 용량 제한과 라이브 패치 요구를 동시에 충족하기 위해 Unity Addressables로 런타임 번들 다운로드 구조를 설계했다.

**구현 내용**
- 에셋을 로컬 번들과 원격 번들로 분리, 초기 설치 용량 최소화
- `PatchScene`에서 카탈로그 갱신 → 변경된 번들만 선택적 다운로드
- `Bundles/Shared/Skills/` 등 카테고리별 번들 그룹핑으로 세분화된 업데이트 제어
- 다운로드 진행률을 UI 프로그레스바와 연결, 실패 시 재시도 로직 포함
- Spine 스켈레탈 에셋(`SkeletonDataAsset`)도 Addressable 참조로 지연 로드

**배운 점**
- 번들 그룹 설계가 초기에 잘못되면 운영 중 변경이 어려우므로, 업데이트 빈도 단위로 그룹을 나누는 것이 중요하다.
- 카탈로그 갱신과 다운로드를 분리하면 사용자가 업데이트 여부를 사전에 인지하고 선택할 수 있어 UX가 개선된다.

---

### 5. Spine 2D 애니메이션 & 렌더링 이슈 대응

캐릭터·가챠 연출에 Spine `SkeletonGraphic`을 사용하면서 발생한 블렌드모드 및 물리 파츠 렌더링 문제를 진단하고 해결했다.

**구현 내용**
- Screen 블렌드 파츠 누락: `allowMultipleCanvasRenderers = false` 상태에서 Multiple CanvasRenderers 옵션 활성화로 해결
- 가챠 연출에서 망토·긴머리 파츠 미표시: 스케일 0 초기화 시 `PhysicsConstraint` NaN 고착 확인, `Physics.Reset()` 호출로 복구
- Spine 에셋 로드 순서와 애니메이션 트랙 우선순위를 명확히 정의하여 파츠 겹침 오류 예방
- SM-S938N(Samsung S25) 한정 `libunity.so` SIGSEGV 네이티브 크래시 원인 분석 (GPU 워커스레드 의심, 엔진 레벨 이슈로 분류)

**배운 점**
- 렌더러 설정 하나의 차이가 파츠 전체 누락으로 이어지는 만큼, Spine UI 통합 시 블렌드모드·렌더러 옵션을 체크리스트로 관리해야 한다.
- 디바이스 한정 네이티브 크래시는 재현 환경 확보가 핵심이며, 엔진 버전과 드라이버 수준의 원인은 우회책 탐색이 현실적인 대응이다.

<br/>

## Repository Purpose

이 저장소는 포트폴리오 목적으로 공개된 코드 저장소입니다. 실제 출시 빌드에서 사용된 인증 키, 광고 ID, 서버 엔드포인트 등 민감 정보는 모두 제거되었으며, 게임 로직과 아키텍처 구조를 중심으로 코드를 유지했습니다. 아트 에셋 및 대용량 바이너리는 포함되지 않습니다. 본인이 작업한 쪽의 코드와 UI 만 남겨두었습니다.


<br/>

## Security Notice

- Firebase 구성 파일 (`google-services.json`) 제외
- AdMob App ID 및 광고 유닛 ID 제거
- Singular SDK 키 제거
- 서버 API 엔드포인트 및 인증 토큰 제외
- AppGuard 보안 설정 파일 제외
- Google Play 키스토어 및 서명 파일 제외

<br/>

## What I Learned

- State Machine 기반 오토배틀 설계로 상태 전환이 많은 시스템에서 유지보수성과 확장성을 동시에 확보할 수 있다.
- 로컬 DB 추상화 레이어(`ClientLocalDB_Simple`)를 통해 기획 데이터 변경을 코드 수정 없이 흡수하는 구조의 중요성을 체감했다.
- Addressables 번들 그룹 설계는 초기 구성이 운영 전략 전체를 좌우하므로, 업데이트 빈도·의존성·용량을 기준으로 사전 설계해야 한다.
- 클라이언트 선검증과 서버 최종 검증의 이중 구조가 API 비용과 보안 사이의 균형을 맞추는 실용적인 방법임을 배웠다.
- Spine 렌더링 이슈처럼 엔진 레이어와 맞닿은 버그는 재현 환경 구축과 단계적 가설 검증이 디버깅의 핵심이다.
- 실 출시 후 Firebase Crashlytics 데이터와 디바이스 한정 크래시 대응을 통해 개발-QA-운영 사이클 전체를 경험했다.

<br/>

## Notes

이 저장소는 실제 서비스 중인 게임의 클라이언트 코드를 포트폴리오용으로 정리한 것으로, 빌드 산출물 및 민감 정보는 포함되지 않습니다. 코드 구조와 시스템 설계 의도를 확인하는 용도로 활용해 주세요.
