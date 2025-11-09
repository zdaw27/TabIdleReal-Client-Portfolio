// Assets/Scripts/TabIdleReal/Player/PlayerCombat.cs
using System.Collections.Generic;
using UnityEngine;
     // StatApplyHelpers, StatAccumulator, IModifierSource, StatDB, StatType
    // StageManager
    // Monster
using DamageNumbersPro;
namespace TabIdleReal
{

     // GoldBank

public class PlayerCombat : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform aimTransform;

    [Header("Auto Attack")]
    [SerializeField] private float baseAutoInterval = 1.0f;
    private float _autoCooldown;

    [SerializeField] private DamageNumberMesh dmnMesh;
    [SerializeField] private bool debugSnapshot = false;

    void Awake()
    {
        if (aimTransform == null) aimTransform = transform;

        // _autoCooldown = GetAutoInterval(); // Start()로 이동
    }
    void Start()
    {
        _autoCooldown = GetAutoInterval();
    }

    void OnEnable()
    {
        if (StageManager.Instance != null)
            StageManager.Instance.OnMonsterKilled += HandleMonsterKilled;
    }

    void OnDisable()
    {
        if (StageManager.Instance != null)
            StageManager.Instance.OnMonsterKilled -= HandleMonsterKilled;
    }

    void Update()
    {
        if (!enabled) return;

        _autoCooldown -= Time.deltaTime;
        if (_autoCooldown <= 0f)
        {
            DoAutoAttack();
            _autoCooldown = GetAutoInterval();
        }

        // 스탯 변경 알림 (StatSnapshotHub)
        if (ServiceLocator.Get<StatSnapshotHub>() != null)
            ServiceLocator.Get<StatSnapshotHub>().MarkDirty();
    }

    float GetAutoInterval()
    {
        var acc = RebuildSnapshot();
        float speedRate = acc.GetAdd(StatDB.Get(StatType.autoAttackSpeedPercent), 0f);
        float intervalMul = 1f / Mathf.Max(0.0001f, 1f + speedRate);
        return Mathf.Max(0.05f, baseAutoInterval * intervalMul);
    }

    // 탭 시스템 제거됨 - 자동 공격만 사용

    void DoAutoAttack()
    {
        var sm = StageManager.Instance;
        if (sm == null) return;

        Monster target = sm.currentTarget;

        if (target == null || !target.IsAlive) return;

        bool isCrit;
        float dmg = ComputeAutoDamage(out isCrit);
        ApplyDamageToMonster(target, dmg, isCrit);
    }

    // 탭 데미지 시스템 제거됨

    float ComputeAutoDamage(out bool isCrit)
    {
        var acc = RebuildSnapshot();
        float nonCrit = acc.CalcFinalAutoDamage(0);
        float critDmg = acc.CalcFinalAutoDamageCritical(0);
        float pCrit = Mathf.Clamp01(acc.GetAdd(StatDB.Get(StatType.criticalProbability), 0f));
        isCrit = Random.value < pCrit;
        return Mathf.Max(0f, isCrit ? critDmg : nonCrit);
    }

    // ── Snapshot (StatSnapshotHub 사용) ──
    StatAccumulator RebuildSnapshot()
    {
        if (ServiceLocator.Get<StatSnapshotHub>() != null)
            return ServiceLocator.Get<StatSnapshotHub>().GetSnapshot();

        Debug.LogWarning("[PlayerCombat] ServiceLocator.Get<StatSnapshotHub>()가 없습니다!");
        return new StatAccumulator();
    }

    // ── Apply Damage ──
    void ApplyDamageToMonster(Monster m, float amount, bool isCritical)
    {
        if (m == null) return;

        m.ReceiveDamage(amount);

        if (dmnMesh != null)
        {
            Vector3 pos = m.transform.position;
            dmnMesh.Spawn(pos, amount, m.transform);
        }
    }

    // ── Reward: 이벤트 구독에서 처리 (10배 없음) ──
    private void HandleMonsterKilled(Monster m)
    {
        if (m == null) return;

        var acc = RebuildSnapshot();
        float baseGold = Mathf.Max(0f, m.goldReward);

        float final = m.isBoss
            ? acc.CalcFinalBossGold(baseGold)
            : acc.CalcFinalNormalGold(baseGold);

        long grant = (long)Mathf.Round(Mathf.Max(0f, final));
        if (grant > 0) GoldBank.Instance?.AddGold(grant);
    }

    // ── 외부(스킬 컨텍스트)에서 사용할 수 있는 "현재 타겟에 데미지 적용" API ──
    public void DealDamageToCurrentTarget(float amount) => DealDamageToCurrentTarget(amount, allowCrit: true);

    public void DealDamageToCurrentTarget(float amount, bool allowCrit)
    {
        var sm = StageManager.Instance;
        if (sm == null) return;

        // 현재 타겟 → 없으면 가장 가까운 살아있는 몬스터
        Monster target = sm.currentTarget;

        if (target == null || !target.IsAlive) return;

        // 스냅샷 갱신 후 치명타 처리(옵션)
        var acc = RebuildSnapshot();
        bool isCrit = false;
        float final = Mathf.Max(0f, amount);

        if (allowCrit)
        {
            float pCrit = Mathf.Clamp01(acc.GetAdd(StatDB.Get(StatType.criticalProbability), 0f));
            if (Random.value < pCrit)
            {
                float critMul = 1f + acc.GetAdd(StatDB.Get(StatType.criticalDamage), 0f);
                final *= Mathf.Max(1f, critMul);
                isCrit = true;
            }
        }

        ApplyDamageToMonster(target, final, isCrit);
    }
}
}
