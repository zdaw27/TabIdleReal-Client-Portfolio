# TabIdleReal - Unity Idle/Tap RPG

Unity 기반 모바일 Idle/Tap RPG 게임의 코어 시스템 스크립트 포트폴리오

## 📋 프로젝트 개요

- **장르**: Idle/Tap RPG
- **플랫폼**: Android/iOS
- **엔진**: Unity 2022.3.62f1
- **언어**: C# 9.0
- **규모**: ~12,000 LOC
- **백엔드**: Firebase Firestore

---

## 📂 폴더 구조

```
Scripts/
├── Core/           # 핵심 시스템 (순수 C# 싱글톤)
│   ├── ServiceBase.cs           # 매니저 기본 클래스 (IDisposable)
│   ├── ServiceLocator.cs        # 서비스 등록/조회
│   ├── DomainManager.cs         # Unity 라이프사이클 관리 (MonoBehaviour)
│   ├── GoldBank.cs              # 재화 관리 (BigNum)
│   └── StatSnapshotHub.cs       # 스탯 스냅샷
├── Combat/         # 전투 시스템
│   ├── PlayerCombat.cs          # 전투 로직
│   ├── StatAccumulator.cs       # Modifier 집계
│   └── IModifierSource.cs       # Modifier 인터페이스
├── Stage/          # 스테이지/던전
│   ├── StageManager.cs          # Facade (순수 C#)
│   └── IStageController.cs      # Strategy 인터페이스
├── Weapon/         # 무기 시스템
│   └── WeaponManager.cs         # GUID 기반 인벤토리 (순수 C#)
├── Skill/          # 스킬 시스템
│   └── SkillService.cs          # 스킬 관리 (순수 C#)
├── Session/        # 세션/세이브
│   ├── SessionCoordinator.cs    # 생명주기 관리
│   └── ISessionUnit.cs          # Save/Load 인터페이스
├── Events/         # 이벤트 버스
│   └── GameEvents.cs            # 강타입 이벤트
└── Generated/      # 자동 생성 코드
    └── GameDataRegistry.cs      # StaticData 로더
```

---

## 1. 설계 철학

### 1.1 핵심 원칙
- **단일 책임**: 각 시스템이 하나의 도메인만 관리 (GoldBank=재화, WeaponManager=무기)
- **확장성 우선**: 신규 기능 추가 시 기존 코드 수정 최소화
- **타입 안정성**: enum과 struct를 활용한 컴파일 타임 검증
- **관심사 분리**: 비즈니스 로직과 직렬화/UI 로직 분리
- **Unity 독립성**: 핵심 로직은 순수 C#으로 작성하여 유닛 테스트 가능

### 1.2 아키텍처 설계 의도

**순수 C# 매니저 시스템**: 모든 핵심 비즈니스 로직 매니저(GoldBank, StageManager, WeaponManager 등 12개)를 MonoBehaviour가 아닌 순수 C# 싱글톤으로 구현. Unity 에디터 의존성을 제거하여 유닛 테스트 가능성을 확보하고, DomainManager(MonoBehaviour)가 Update 루프를 제공하는 하이브리드 구조.

```csharp
// 순수 C# 매니저
public class GoldBank : ServiceBase {
    private static GoldBank _instance;
    public static GoldBank Instance => _instance ??= new GoldBank();
    private GoldBank() { }
}

// Unity 라이프사이클 관리자
public class DomainManager : MonoBehaviour {
    void Awake() {
        ServiceLocator.Register(typeof(GoldBank), GoldBank.Instance);
        ServiceLocator.InitializeAll();
    }

    void Update() {
        StageManager.Instance.Tick(Time.deltaTime);
        SkillService.Instance.Tick(Time.deltaTime);
    }
}
```

### 1.3 기존 설계 의도

**이벤트 기반 통신**: 시스템 간 직접 참조를 제거하고 GameEvents를 통해 느슨하게 결합. Achievement/BattlePass/Quest가 Stage 이벤트만 구독하여 독립적으로 동작.

**Modifier 집계 방식**: 스탯 계산을 중앙화(StatAccumulator)하되, 신규 스탯 소스(무기/아티팩트/스킬/코스튬)는 IModifierSource만 구현하면 자동 통합.

**Strategy 패턴**: 던전 타입별 로직을 IStageController 구현체로 분리. 새 던전 타입 추가 시 기존 StageManager/UI 코드 수정 불필요.

**Partial Class 활용**: WeaponManager의 핵심 로직과 Save/Load를 별도 파일로 분리하여 가독성 향상 및 책임 명확화.

---

## 2. 시스템 아키텍처

### 2.1 전투 시스템
**PlayerCombat → StatAccumulator → 최종 데미지**

```csharp
// 무기/아티팩트/스킬/코스튬의 modifier를 집계
void GatherModifiers() {
    _acc.Clear();
    foreach (var p in modifierProviders.OfType<IModifierSource>())
        foreach (var m in p.GetModifiers(query))
            _acc.Add(m);
}

BigNum CalculateDamage() {
    GatherModifiers();
    var base = _acc[StatDB.Get(StatType.AutoAttackDamage)];
    var crit = CheckCrit() ? _acc[StatDB.Get(StatType.CritDamage)] : 1f;
    return BigNum.FromDouble(base) * crit * SkillMultiplier;
}
```

**의도**: PlayerStats는 기본값만 제공, 최종 계산은 모든 modifier 집계 후 수행. 신규 장비/버프 추가 시 전투 코드 수정 불필요.

---

## 3. 순수 C# 아키텍처 특징

### 3.1 ServiceBase + ServiceLocator
모든 매니저가 상속하는 순수 C# 기본 클래스:
```csharp
public abstract class ServiceBase : IDisposable {
    public abstract void Initialize();
    public virtual void Dispose() { }
}
```

### 3.2 Lazy Singleton 패턴
Unity에 의존하지 않는 싱글톤 구현:
```csharp
private static GoldBank _instance;
public static GoldBank Instance => _instance ??= new GoldBank();
private GoldBank() { }
```

### 3.3 DomainManager - Unity 브리지
- 유일하게 MonoBehaviour를 상속하는 매니저
- Awake()에서 모든 순수 C# 매니저 인스턴스 생성 및 ServiceLocator 등록
- Update()에서 필요한 매니저의 Tick() 메서드 호출
- Unity 라이프사이클과 순수 C# 로직의 브리지 역할

---

## 4. 아키텍처 강점
1. **Unity 독립성**: 핵심 로직을 순수 C#으로 분리하여 유닛 테스트 및 재사용성 확보
2. **확장성**: 신규 기능 추가 시 기존 코드 최소 수정
3. **타입 안정성**: Enum/Struct로 런타임 에러 방지
4. **관심사 분리**: Partial class로 로직/직렬화 격리
5. **느슨한 결합**: 이벤트 기반 통신으로 의존성 제거
6. **대용량 처리**: BigNum으로 Idle 게임 인플레이션 대응

---

**Note**: 이 리포지토리는 전체 프로젝트에서 핵심 시스템 스크립트만 발췌한 포트폴리오입니다.
