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

---

## 5. BigNum 시스템

### 5.1 설계 배경
Idle 게임의 특성상 골드/데미지가 기하급수적으로 증가 (1만 → 1억 → 1조 → 1경 → ...). C#의 기본 타입(long, double)으로는 표현 한계와 정밀도 문제 발생.

### 5.2 구조
```csharp
public struct BigNum {
    public double m;  // 가수 (1 ≤ m < 10000)
    public int e4;    // 만 단위 지수 (0=일, 1=만, 2=억, 3=조, 4=경, ...)
    
    // 예시: 1234경 5678조 = m: 1234.5678, e4: 4
}
```

**특징:**
- 만 단위 지수로 한국어 표기와 자연스럽게 매핑
- double 가수로 소수점 이하 정밀도 유지
- 10^(4*e4) 범위까지 표현 가능 (사실상 무한대)

### 5.3 연산
```csharp
// 사칙연산
public static BigNum operator +(BigNum a, BigNum b);
public static BigNum operator -(BigNum a, BigNum b);
public static BigNum operator *(BigNum a, BigNum b);
public static BigNum operator /(BigNum a, BigNum b);

// 비교 연산
public static bool operator >(BigNum a, BigNum b);
public static bool operator <(BigNum a, BigNum b);

// 변환
public static BigNum FromLong(long value);
public static BigNum FromDouble(double value);
public long ToLongSafe(); // 오버플로우 시 long.MaxValue 반환
public string ToKoreanString(); // "123.45경"
```

### 5.4 한국어 표기
```csharp
private static readonly string[] KoreanUnits = {
    "", "만", "억", "조", "경", "해", "자", "양", "구", "간", "정"
};

public string ToKoreanString() {
    if (e4 >= KoreanUnits.Length) return "무량대수";
    return $"{m:F2}{KoreanUnits[e4]}";
}

// 출력 예시:
// 1234만 5678 → "1234.57만"
// 9876경 5432조 → "9876.54경"
```

### 5.5 실전 활용
```csharp
// GoldBank - 재화 관리
public void AddAmount(CurrencyType ct, BigNum amount) {
    var cur = GetAmount(ct);
    SetAmountInternal(ct, cur + amount, amount);
}

// PlayerCombat - 데미지 계산
public BigNum CalculateTapDamage() {
    var baseDmg = _acc.GetAdd(StatDB.Get(StatType.TapDamage), 0f);
    var result = BigNum.FromDouble(baseDmg);
    if (CheckCrit()) result = result * critMultiplier;
    return result;
}

// UI 표시
goldText.text = GoldBank.Instance.Gold.ToKoreanString(); // "1234.56경"
```

### 5.6 장점
1. **무한 확장성**: Idle 게임의 극후반 콘텐츠까지 수용
2. **정밀도 유지**: double 가수로 소수점 계산 정확
3. **직관적 표기**: 한국어 단위와 1:1 매핑
4. **타입 안전성**: 연산자 오버로딩으로 일반 숫자처럼 사용
5. **직렬화 용이**: 구조체로 JSON/Firestore 자동 직렬화

---

## 6. 기술 스택
- **Unity 2022.3.62f1**
- **C# 9.0** (Partial, Record)
- **Firebase Firestore** (클라우드 저장)
- **UniTask** (비동기 처리)
- **12,000+ LOC**

---

**Note**: 이 리포지토리는 전체 프로젝트에서 핵심 시스템 스크립트만 발췌한 포트폴리오입니다.

---

## 7. 데이터 파이프라인 (자동 생성)

### 7.1 설계 목표
- 기획자가 Excel에서 게임 데이터를 편집
- 프로그래머 개입 없이 자동으로 C# 코드 생성
- 컴파일 타임에 타입 검증으로 런타임 에러 방지

### 7.2 파이프라인 구조
```
[Excel (XLSX)]
      ↓ (외부 Python 툴)
[JSON 파일들]
      ↓ (Unity Editor 스크립트)
[C# Generated Code]
      ↓ (컴파일)
[GameDataRegistry 자동 로드]
```

### 7.3 자동 생성 코드

#### Row 클래스 생성
```csharp
// Generated/MonstersRow.cs (자동 생성)
[System.Serializable]
public class MonstersRow {
    public int ID;
    public string Name;
    public int HP;
    public int AttackPower;
    public int GoldReward;
    public string SpriteKey;
}
```

#### Registry 생성
```csharp
// Generated/GameDataRegistry.cs (자동 생성)
public static class GameDataRegistry {
    public static readonly List<MonstersRow> MonstersList = new();
    public static readonly List<WeaponsRow> WeaponsList = new();
    public static readonly List<SkillsRow> SkillsList = new();
    // ... 28개 테이블
    
    static GameDataRegistry() {
        Load_Monsters();
        Load_Weapons();
        Load_Skills();
        // ... 모든 테이블 자동 로드
    }
    
    private static void Load_Monsters() {
        var json = Resources.Load<TextAsset>("CSVDataJson/Monsters");
        var wrapper = JsonUtility.FromJson<MonstersWrapper>(json.text);
        MonstersList.AddRange(wrapper.rows);
    }
}
```

### 7.4 사용 예시

#### 기획 데이터 조회
```csharp
// ID로 몬스터 데이터 찾기
var monsterData = GameDataRegistry.MonstersList
    .First(r => r.ID == monsterId);

// 스테이지별 보상 계산
var stageData = GameDataRegistry.StagesList
    .First(r => r.StageNumber == currentStage);
var reward = stageData.BaseGoldReward * playerLevel;
```

#### Enum 자동 생성
```csharp
// Generated/StatType.cs (자동 생성)
public enum StatType {
    MaxHP = 1,
    AttackPower = 2,
    CritChance = 3,
    CritDamage = 4,
    TapDamage = 5,
    AutoAttackDPS = 6,
    // ... Excel의 Stats 시트에서 자동 생성
}

// Generated/CurrencyType.cs (자동 생성)
public enum CurrencyType : int {
    Gold = 1000001,
    Diamonds = 1000002,
    StatReroll = 1000003,
    // ... Excel의 Items_Etc 시트에서 자동 생성
}
```

### 7.5 자동 생성 툴 구조

#### Python 변환기 (XLSX → JSON)
```python
# tools/xlsx_to_json.py
import pandas as pd
import json

def convert_sheet_to_json(excel_path, sheet_name):
    df = pd.read_excel(excel_path, sheet_name=sheet_name)
    rows = df.to_dict('records')
    
    output = {
        "rows": rows
    }
    
    with open(f'Assets/Resources/CSVDataJson/{sheet_name}.json', 'w', encoding='utf-8') as f:
        json.dump(output, f, ensure_ascii=False, indent=2)
```

#### Unity Editor 코드 생성기
```csharp
// Editor/DataTableCodeGenerator.cs
public class DataTableCodeGenerator : EditorWindow {
    [MenuItem("Tools/Generate Data Tables")]
    static void Generate() {
        var jsonFiles = Directory.GetFiles("Assets/Resources/CSVDataJson", "*.json");
        
        foreach (var jsonPath in jsonFiles) {
            var tableName = Path.GetFileNameWithoutExtension(jsonPath);
            GenerateRowClass(tableName);
            GenerateRegistryEntry(tableName);
        }
        
        AssetDatabase.Refresh();
    }
    
    static void GenerateRowClass(string tableName) {
        var json = File.ReadAllText($"Assets/Resources/CSVDataJson/{tableName}.json");
        var schema = InferSchema(json);
        
        var code = new StringBuilder();
        code.AppendLine($"public class {tableName}Row {{");
        
        foreach (var field in schema) {
            code.AppendLine($"    public {field.Type} {field.Name};");
        }
        
        code.AppendLine("}");
        
        File.WriteAllText($"Assets/Scripts/Generated/{tableName}Row.cs", code.ToString());
    }
}
```

### 7.6 장점

1. **타입 안정성**: Excel 스키마 변경 시 컴파일 에러로 즉시 감지
2. **생산성 향상**: 기획자가 직접 데이터 수정 가능 (프로그래머 불필요)
3. **휴먼 에러 방지**: 수동 타이핑 제거, 자동 검증
4. **일관성**: 모든 테이블이 동일한 구조/네이밍 규칙 준수
5. **확장성**: 신규 테이블 추가 시 자동으로 코드 생성

### 7.7 테이블 목록 (28개)
```
Monsters, Weapons, Skills, Artifacts, Stages, Items_Etc,
Stats, WeaponGacha, GuideQuests, Achievements, BattlePass,
DungeonTiers, CostumeStats, RebornLevels, ItemDrops,
MonsterSpawns, StageRewards, SkillLevels, WeaponOptions,
ArtifactEffects, QuestRewards, PassRewards, StatWeights,
CombatPowerWeights, ...
```

---

## 8. 기술 스택
- **Unity 2022.3.62f1**
- **C# 9.0** (Partial, Record)
- **Firebase Firestore** (클라우드 저장)
- **UniTask** (비동기 처리)
- **Python 3.x** (데이터 파이프라인)
- **12,000+ LOC**

---

**Note**: 이 리포지토리는 전체 프로젝트에서 핵심 시스템 스크립트만 발췌한 포트폴리오입니다.
