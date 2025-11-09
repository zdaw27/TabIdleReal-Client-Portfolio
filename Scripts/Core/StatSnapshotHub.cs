using System;
using System.Collections;
using System.Collections.Generic;


namespace TabIdleReal
{
    /// <summary>
    /// 여러 IModifierSource 의 수정치를 모아 한 번에 StatAccumulator 스냅샷을 만든다.
    /// - LazyManager 상속으로 싱글톤 패턴 자동 적용
    /// - 스탯 집계 및 전투력 계산의 중앙 허브
    /// - Current 는 읽기 전용으로 취급 (외부에서 변경하지 말 것)
    /// </summary>
    
    public class StatSnapshotHub : ServiceBase
    {
        private static StatSnapshotHub _instance;
        public static StatSnapshotHub Instance => _instance ??= new StatSnapshotHub();
        private StatSnapshotHub() { }

        private List<IModifierSource> sources = new();

        

        /// <summary>현재 스냅샷 (읽기 전용)</summary>
        public StatAccumulator Current { get; private set; }

        /// <summary>스냅샷이 재빌드되면 호출됩니다.</summary>
        public event Action<StatAccumulator> OnSnapshotChanged;

        /// <summary>전투력이 변경되면 호출됩니다.</summary>
        public event Action<long> OnCombatPowerChanged;

        // 캐시: Inspector 에서 받은 MonoBehaviour 중 IModifierSource 만 걸러 저장
        private readonly List<IModifierSource> _cache = new();
        private bool _dirty = true;
        private long _cachedCombatPower = 0;
        public override void Initialize()
        {
            UnityEngine.Debug.Log("[StatSnapshotHub] OnInitialize 시작");


            RebuildSourceCache();
            // 최초 빌드
            Rebuild(default);

            UnityEngine.Debug.Log("[StatSnapshotHub] OnInitialize 완료");
        }

        /// <summary>
        /// Inspector 변경 후/런타임에 소스 리스트가 바뀐 경우 호출
        /// </summary>
        public void RebuildSourceCache()
        {
            _cache.Clear();
            foreach (var source in sources)
            {
                if (source == null) continue;
                if (source != null) _cache.Add(source);
                else UnityEngine.Debug.LogWarning($"[StatSnapshotHub] '{source.ToString()}' 는 IModifierSource 가 아님", mb);
            }
        }

        /// <summary>외부에서 스탯이 변경되었음을 알릴 때 호출</summary>
        public void MarkDirty() => _dirty = true;

        /// <summary>현재 스냅샷 반환 (필요시 자동 갱신)</summary>
        public StatAccumulator GetSnapshot()
        {
            if (_dirty) Rebuild(new CombatQuery { Attacker = null, Target = null, DeltaTime = 0f });
            return Current;
        }

        /// <summary>
        /// 쿼리에 맞춰 모든 소스의 Modifiers 를 합산하여 새 스냅샷을 생성한다.
        /// </summary>
        public void Rebuild(CombatQuery q)
        {
            var acc = new StatAccumulator();

            // 네가 쓰는 실제 누적 규칙: acc.Add(StatModifier)
            foreach (var src in _cache)
            {
                IEnumerable<StatModifier> mods;
                try
                {
                    mods = src.GetModifiers(q);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogException(e, (src as object));
                    continue;
                }

                if (mods == null) continue;
                foreach (var m in mods)
                {
                    acc.Add(m);
                }
            }

            Current = acc;
            _dirty = false;
            GameEvents.Stat.SnapshotChanged.Invoke(Current);

            // 전투력 재계산 및 이벤트 발행
            RecalculateCombatPower();
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 전투력 계산
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        /// <summary>현재 전투력 조회 (캐시됨)</summary>
        public long GetCombatPower() => _cachedCombatPower;

        /// <summary>전투력 재계산 및 이벤트 발행 (CombatPowerWeights 기반)</summary>
        private void RecalculateCombatPower()
        {
            var acc = Current;
            if (acc == null)
            {
                _cachedCombatPower = 0;
                return;
            }

            // CombatPowerWeights 데이터 로드
            var weights = GameDataRegistry.CombatPowerWeightsList;
            if (weights == null || weights.Count == 0)
            {
                UnityEngine.Debug.LogWarning("[StatSnapshotHub] CombatPowerWeights 데이터가 없습니다!");
                _cachedCombatPower = 0;
                return;
            }

            float totalPower = 0f;

            // 각 스탯에 가중치를 곱해서 합산
            foreach (var weightRow in weights)
            {
                var statDef = StatDB.Get(weightRow.statType);
                if (statDef == null) continue;

                // 해당 스탯의 현재 값 가져오기
                float statValue = acc.GetAdd(statDef, 0f);

                // 가중치 적용
                float contribution = statValue * weightRow.Weight;
                totalPower += contribution;
            }

            long newPower = (long)UnityEngine.Mathf.Max(0f, totalPower);

            if (newPower != _cachedCombatPower)
            {
                _cachedCombatPower = newPower;
                GameEvents.Stat.CombatPowerChanged.Invoke(_cachedCombatPower);
            }
        }

        // -------- 선택: 런타임에서 소스 추가/제거를 원할 때 --------
        public void AddSource(IModifierSource source)
        {
            if (source == null) return;
            sources.Add(source);
            if (source != null) _cache.Add(source);
            else UnityEngine.Debug.LogWarning($"[StatSnapshotHub] '{source.ToString()}' 는 IModifierSource 가 아님", mb);
        }

        public void RemoveSource(IModifierSource source)
        {
            if (source == null) return;
            sources.Remove(source);
            if (source != null) _cache.Remove(source);
        }
    }
}
