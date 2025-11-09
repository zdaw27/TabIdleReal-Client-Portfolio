# TabIdleReal - 스크립트 설계 문서

Unity 기반 Idle/Tap RPG의 확장 가능한 아키텍처

---

## 1. 설계 철학

### 1.1 핵심 원칙
- **단일 책임**: 각 시스템이 하나의 도메인만 관리 (GoldBank=재화, WeaponManager=무기)
- **확장성 우선**: 신규 기능 추가 시 기존 코드 수정 최소화
- **타입 안정성**: enum과 struct를 활용한 컴파일 타임 검증
- **관심사 분리**: 비즈니스 로직과 직렬화/UI 로직 분리

### 1.2 설계 의도
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

### 2.2 스테이지/던전 시스템
**StageManager (Facade) ↔ IStageController (Strategy)**

```csharp
public interface IStageController {
    void Enter(StageManager mgr, int returnStageId);
    void Update(float dt);
    void OnMonsterDied(Monster m);
    void Exit();
}

// 던전 전환
var goldCtrl = new GoldDungeonController(tier);
StageManager.Instance.StartController(goldCtrl, currentStage);

// 종료 시 메인으로 자동 복귀
HandleEndRequested(...) {
    StartController(new MainStageController(returnStage), returnStage);
}
```

**의도**: 컨트롤러 교체만으로 일반/던전 전환. UI는 StageManager 이벤트만 구독하므로 던전 타입 무관.

### 2.3 재화 시스템
**GoldBank - BigNum으로 대용량 숫자 처리**

```csharp
public enum CurrencyType : int {
    Gold = 1000001,
    Diamonds = 1000002,
    // Items_EtcRow.ID와 1:1 매핑
}

public BigNum GetAmount(CurrencyType ct);
public void AddAmount(CurrencyType ct, BigNum amount);
public bool TrySpend(CurrencyType ct, BigNum amount);

// BigNum 구조
public struct BigNum {
    public double m;  // 1 ≤ m < 10000
    public int e4;    // 만 단위 지수 (1=만, 2=억, 3=조)
    public string ToKoreanString(); // "123.45경"
}
```

**의도**: Enum으로 타입 안정성 확보, BigNum으로 기하급수적 성장 지원. 모든 재화 변경이 단일 지점을 거쳐 로깅/디버깅 용이.

### 2.4 무기 시스템
**WeaponManager (인벤토리) + WeaponGenerator (생성)**

```csharp
// 핵심 로직 - WeaponManager.cs
public List<Weapon> Inventory;
public Weapon Equipped;

public void Equip(Weapon w) {
    Equipped = w;
    GameEvents.WeaponEvent.InventoryChanged.Invoke(Inventory);
}

// 저장/로드 - WeaponManager.ISaveUnit.partial.cs
public partial class WeaponManager : ISessionUnit {
    public async UniTask SaveAsync() {
        var dto = Capture(); // 현재 상태 직렬화
        await _doc.SetAsync(dto);
    }
}
```

**의도**: GUID로 동일 무기 구별, Partial로 로직과 직렬화 분리.

### 2.5 스킬 시스템
**SkillService (매니저) + SkillBase (추상 클래스)**

```csharp
public abstract class SkillBase {
    public abstract void Execute(SkillContext ctx);
    public virtual float GetDamageMultiplier() => 1f;
}

// 구현체 예시
public class PowerSlashSkill : SkillBase {
    public override void Execute(SkillContext ctx) {
        ctx.PlayerCombat.DealDamage(ctx.Target, baseDmg * GetDamageMultiplier());
    }
}
```

**의도**: 스킬별 클래스 분리로 if-else 제거, 신규 스킬은 SkillBase 상속만 하면 됨.

### 2.6 세션/세이브 시스템
**SessionCoordinator - 생명주기 관리**

```csharp
public interface ISessionUnit {
    UniTask InitializeAsync(string uid); // Firebase 문서 참조 준비
    UniTask LoadAsync();                 // Firestore에서 로드
    UniTask SaveAsync();                 // Firestore에 저장
    void Reset();                        // 로컬 초기화
}

// 로그인 시 순차 초기화
public async UniTask LoginAsync(string uid) {
    var ordered = units.OfType<ISessionUnit>().ToList();
    foreach (var u in ordered) {
        await u.InitializeAsync(uid);
        await u.LoadAsync();
    }
    GameEvents.Session.DataLoaded.Invoke();
}
```

**의도**: SessionCoordinator가 Inspector 순서대로 초기화/로드, 역순 저장. 주기적 자동 저장(2분) + 중요 이벤트 시 즉시 저장으로 데이터 손실 최소화.

### 2.7 이벤트 버스
**GameEvents - 강타입 이벤트 시스템**

```csharp
public static class GameEvents {
    public static class Stage {
        public static readonly GameEvent<int> Cleared = new("Stage.Cleared");
        public static readonly GameEvent<Monster> MonsterKilled = new("Stage.MonsterKilled");
    }

    public static class Currency {
        public static readonly GameEvent<CurrencyType, BigNum, BigNum> Changed
            = new("Currency.Changed");
    }
}

// 사용
GameEvents.Stage.MonsterKilled.Invoke(monster);
GameEvents.Stage.MonsterKilled.Subscribe(OnMonsterKilled);
```

**의도**: 시스템 간 직접 참조 제거, 강타입으로 컴파일 타임 검증, 카테고리별 구조화로 추적 용이.

---

## 3. 데이터 관리

### 3.1 StaticData 로딩
**GameDataRegistry - 자동 로드**

```csharp
public static class GameDataRegistry {
    public static readonly List<MonstersRow> MonstersList = new();
    public static readonly List<WeaponsRow> WeaponsList = new();
    // ... 28개 테이블

    static GameDataRegistry() {
        Load_Monsters(); // JSON에서 자동 로드
        Load_Weapons();
    }
}
```

**파이프라인**: XLSX → JSON (외부 툴) → C# Generated Code → Runtime 자동 로드

**의도**: 정적 생성자로 게임 시작 시 자동 로드, 런타임에서는 간단한 쿼리만 수행.

### 3.2 Save/Load 전략
**Firestore 비동기 + Partial Class**

```csharp
// 중요 데이터 즉시 저장
GameEvents.Session.CriticalDataChanged.Invoke("DungeonProgress");

// SessionCoordinator가 모든 유닛 저장
private void SaveCritical(string source) {
    var rev = units.OfType<ISessionUnit>().Reverse();
    foreach (var u in rev) await u.SaveAsync();
}
```

**의도**: 자동 저장(주기) + 즉시 저장(이벤트)로 데이터 손실 방지, Partial로 직렬화 로직 격리.

---

## 4. 확장 시나리오

### 신규 재화 추가
```csharp
// 1. enum 확장
public enum CurrencyType : int {
    NewCurrency = 1000010
}
// 2. Items_Etc 테이블에 행 추가
// 3. 기존 API 그대로 사용
GoldBank.Instance.AddAmount(CurrencyType.NewCurrency, 100);
```

### 신규 스탯 소스 추가 (버프)
```csharp
public class BuffModifierSource : IModifierSource {
    public IEnumerable<StatModifier> GetModifiers(CombatQuery q) {
        foreach (var buff in activeBuffs)
            yield return Mod.Add(buff.StatType, buff.Value);
    }
}
// PlayerCombat.modifierProviders에 추가만 하면 자동 적용
```

### 신규 던전 타입 추가
```csharp
// 1. IStageController 구현
public class BossRushController : IStageController {
    public void Enter(StageManager mgr, int returnId) { ... }
    public void Update(float dt) { ... }
}
// 2. 사용
var ctrl = new BossRushController(tier);
StageManager.Instance.StartController(ctrl, currentStage);
// UI/Achievement 코드 수정 불필요
```

---

## 5. 코드 품질

### 5.1 관심사 분리
- **비즈니스 vs 직렬화**: Partial class로 WeaponManager 핵심 로직과 Save/Load 분리
- **이벤트 발행 vs 구독**: Manager는 발행만, UI/Quest는 구독만 수행
- **데이터 vs 로직**: GameDataRegistry(정적 데이터) ↔ Generator(동적 생성)

### 5.2 타입 안정성
```csharp
// enum으로 재화 타입 보장 (매직 넘버 제거)
public bool TrySpend(CurrencyType ct, BigNum amount);

// 컴파일 타임 검증
var data = GameDataRegistry.StagesList.First(r => r.ID == stageId);
```

### 5.3 확장성
- **개방-폐쇄 원칙**: IModifierSource, IStageController 추가로 확장, 기존 코드 수정 최소
- **의존성 역전**: 구체 클래스가 아닌 인터페이스에 의존

---

## 6. 기술 스택
- **Unity 2022.3.62f1**
- **C# 9.0** (Partial, Record)
- **Firebase Firestore** (클라우드 저장)
- **UniTask** (비동기 처리)
- **12,000+ LOC**

## 7. 아키텍처 강점
1. **확장성**: 신규 기능 추가 시 기존 코드 최소 수정
2. **타입 안정성**: Enum/Struct로 런타임 에러 방지
3. **관심사 분리**: Partial class로 로직/직렬화 격리
4. **느슨한 결합**: 이벤트 기반 통신으로 의존성 제거
5. **대용량 처리**: BigNum으로 Idle 게임 인플레이션 대응
