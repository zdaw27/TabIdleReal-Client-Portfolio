# TabIdleReal - Unity Idle/Tap RPG

Unity 기반 모바일 Idle/Tap RPG 게임의 코어 시스템 스크립트 포트폴리오

## 📋 프로젝트 개요

- **장르**: Idle/Tap RPG
- **플랫폼**: Android/iOS
- **엔진**: Unity 2022.3.62f1
- **언어**: C# 9.0
- **규모**: ~12,000 LOC
- **백엔드**: Firebase Firestore

## 🎯 주요 특징

### 확장 가능한 아키텍처
- **이벤트 기반 통신**: GameEvents를 통한 느슨한 결합
- **Strategy 패턴**: 던전 타입별 로직 런타임 교체 (IStageController)
- **Modifier 집계**: 무기/아티팩트/스킬 조합 가능한 스탯 시스템
- **Partial Class**: 비즈니스 로직과 직렬화 분리

### 타입 안정성
- Enum 기반 재화 관리 (CurrencyType)
- 강타입 이벤트 시스템 (GameEvent<T>)
- 컴파일 타임 검증으로 런타임 에러 최소화

### 대용량 숫자 처리
- BigNum 구조체로 만/억/조 단위 지원
- Idle 게임의 기하급수적 성장 대응
- 한국어 단위 표시 ("123.45경")

## 📂 폴더 구조

```
Scripts/
├── Core/           # 핵심 시스템
│   ├── GoldBank.cs             # 재화 관리 (BigNum)
│   ├── StatSnapshotHub.cs      # 스탯 스냅샷
│   └── ServiceLocator.cs       # 의존성 주입
├── Combat/         # 전투 시스템
│   ├── PlayerCombat.cs         # 전투 로직
│   ├── StatAccumulator.cs      # Modifier 집계
│   └── IModifierSource.cs      # Modifier 인터페이스
├── Stage/          # 스테이지/던전
│   ├── StageManager.cs         # Facade
│   └── IStageController.cs     # Strategy 인터페이스
├── Weapon/         # 무기 시스템
│   └── WeaponManager.cs        # GUID 기반 인벤토리
├── Skill/          # 스킬 시스템
│   └── SkillService.cs         # 스킬 관리
├── Session/        # 세션/세이브
│   ├── SessionCoordinator.cs   # 생명주기 관리
│   └── ISessionUnit.cs         # Save/Load 인터페이스
├── Events/         # 이벤트 버스
│   └── GameEvents.cs           # 강타입 이벤트
└── Generated/      # 자동 생성 코드
    └── GameDataRegistry.cs     # StaticData 로더
```

## 🔧 핵심 시스템

### 1. 전투 시스템
```csharp
PlayerCombat → StatAccumulator → 최종 데미지
```
- 무기/아티팩트/스킬의 modifier를 StatAccumulator가 자동 집계
- 신규 스탯 소스 추가 시 IModifierSource만 구현

### 2. 재화 시스템
```csharp
public enum CurrencyType : int {
    Gold = 1000001,
    Diamonds = 1000002,
    // ...
}

GoldBank.Instance.AddAmount(CurrencyType.Gold, BigNum.FromLong(1000));
```
- Enum으로 타입 안정성 확보
- BigNum으로 대용량 숫자 처리
- 단일 클래스를 통한 모든 재화 관리

### 3. 스테이지/던전 시스템
```csharp
public interface IStageController {
    void Enter(StageManager mgr, int returnStageId);
    void Update(float dt);
    void OnMonsterDied(Monster m);
    void Exit();
}
```
- Strategy 패턴으로 던전 타입별 로직 분리
- 컨트롤러 교체만으로 일반/던전 전환
- UI는 StageManager 이벤트만 구독

### 4. 세션/세이브 시스템
```csharp
public interface ISessionUnit {
    UniTask InitializeAsync(string uid);
    UniTask LoadAsync();
    UniTask SaveAsync();
    void Reset();
}
```
- SessionCoordinator가 순서대로 초기화/로드, 역순 저장
- Firestore 비동기 저장
- 주기 저장(2분) + 즉시 저장(중요 이벤트)

### 5. 이벤트 버스
```csharp
public static class GameEvents {
    public static class Stage {
        public static readonly GameEvent<int> Cleared = new("Stage.Cleared");
    }
    public static class Currency {
        public static readonly GameEvent<CurrencyType, BigNum, BigNum> Changed;
    }
}
```
- 시스템 간 직접 참조 제거
- 강타입으로 컴파일 타임 검증
- 카테고리별 구조화

## 🚀 확장 시나리오

### 신규 재화 추가
```csharp
// 1. enum 확장
public enum CurrencyType : int {
    NewCurrency = 1000010
}
// 2. 테이블에 데이터 추가
// 3. 기존 API 그대로 사용
GoldBank.Instance.AddAmount(CurrencyType.NewCurrency, 100);
```

### 신규 던전 타입 추가
```csharp
// 1. IStageController 구현
public class BossRushController : IStageController { ... }

// 2. 사용
StageManager.Instance.StartController(new BossRushController(tier), currentStage);
// UI/Achievement 코드 수정 불필요
```

### 신규 스탯 소스 추가
```csharp
public class BuffModifierSource : IModifierSource {
    public IEnumerable<StatModifier> GetModifiers(CombatQuery q) {
        foreach (var buff in activeBuffs)
            yield return Mod.Add(buff.StatType, buff.Value);
    }
}
// PlayerCombat에 추가만 하면 자동 적용
```

## 📖 상세 문서

[ARCHITECTURE.md](./ARCHITECTURE.md)에서 설계 의도 및 시스템별 상세 설명 확인

## 🛠 기술 스택

- Unity 2022.3.62f1
- C# 9.0 (Partial Class, Record)
- Firebase Firestore (클라우드 저장)
- UniTask (비동기 처리)

## 💡 설계 철학

1. **확장성**: 신규 기능 추가 시 기존 코드 최소 수정
2. **타입 안정성**: Enum/Struct로 런타임 에러 방지
3. **관심사 분리**: Partial class로 로직/직렬화 격리
4. **느슨한 결합**: 이벤트 기반 통신
5. **대용량 처리**: BigNum으로 Idle 게임 인플레이션 대응

## 📊 코드 품질

- **관심사 분리**: 비즈니스 로직 vs 직렬화 (Partial Class)
- **개방-폐쇄 원칙**: 인터페이스 추가로 확장, 기존 코드 수정 최소
- **의존성 역전**: 구체 클래스가 아닌 인터페이스 의존
- **단일 책임**: 각 Manager는 하나의 도메인만 관리

---

**Note**: 이 리포지토리는 전체 프로젝트에서 핵심 시스템 스크립트만 발췌한 포트폴리오입니다.
