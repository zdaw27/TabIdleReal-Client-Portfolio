using System.Collections.Generic;
using UnityEngine;

namespace TabIdleReal
{
    /// <summary>��ų �ν��Ͻ� ����/���׷��̵�/��ٿ� ���� + ���� �̺�Ʈ ����</summary>
    public class SkillService : ServiceBase
    {
        public static SkillService Instance { get; private set; }

        // ���� �̺�Ʈ: ���� ����/��ٿ� ���� �� ��ų ���°� �ٲ�� ����
        public event System.Action<SkillType> SkillChanged;
        void NotifyChanged(SkillType t) => SkillChanged?.Invoke(t);

        private SkillContext _ctx;
        private readonly Dictionary<SkillType, SkillBase> _skills = new();
        private int _playerLevel = 1; // Back-compat placeholder

        public override void Initialize()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Def ����
            SkillDefs.EnsureBuilt();

            // Def�� �ö�� ��� Ÿ�� �ڵ� ���
            foreach (var t in SkillDefs.AllTypes)
                Register(t);

            // �ӽ�: ���� ��� (UnlockLevel �÷� �����Ƿ�)
            foreach (var s in _skills.Values) s.Unlocked = true;
        }

        void Update() => Tick(Time.deltaTime);
        public void BindContext(SkillContext ctx) => _ctx = ctx;

        private void Register(SkillType t)
        {
            var s = SkillFactory.Create(t);
            if (s != null)
            {
                _skills[t] = s;
                // �ʱ� ���� �˸�(����)
                NotifyChanged(t);
            }
        }

        public bool Upgrade(SkillType t)
        {
            if (!_skills.TryGetValue(t, out var s)) return false;
            s.Level = Mathf.Clamp(s.Level + 1, 1, 999);
            NotifyChanged(t); // �� ���׷��̵� ��� �˸�
            return true;
        }

        public bool TryActivate(SkillType t)
        {
            if (!_skills.TryGetValue(t, out var s)) return false;
            var ok = s.Activate(in _ctx);
            if (ok) NotifyChanged(t); // �� ��ٿ�/���� ��ȭ �˸�
            return ok;
        }

        public void Tick(float dt)
        {
            foreach (var s in _skills.Values)
            {
                float prevCd = s.GetCooldownLeft();
                int prevLv = s.Level;

                s.Tick(dt);
                s.Update(in _ctx);

                // ��ٿ��� ���߰ų� ������ �ٲ�� �˸�(������ ���)
                if (s.GetCooldownLeft() != prevCd || s.Level != prevLv)
                    NotifyChanged(s.Def.Type);
            }
        }

        public SkillBase Get(SkillType t) => _skills.TryGetValue(t, out var s) ? s : null;

        // === Back-compat helpers expected by existing UI ===
        public float GetAutoAttackMultiplier()
        {
            float mul = 1f;
            foreach (var s in _skills.Values) mul *= Mathf.Max(0f, s.GetAutoAttackMultiplier());
            return mul;
        }

        public void SetPlayerLevel(int lv) { _playerLevel = Mathf.Max(1, lv); }
        public int GetPlayerLevel() => _playerLevel;

        public float GetCooldownLeft(SkillType t) => Get(t)?.GetCooldownLeft() ?? 0f;
        public float GetCooldown(SkillType t) => Get(t)?.Cooldown ?? 0f;

        /// <summary>
        /// 특정 스킬 레벨 초기화 (환생 시 사용)
        /// </summary>
        public void ResetSkillLevel(SkillType t)
        {
            if (!_skills.TryGetValue(t, out var s)) return;
            s.Level = 1;
            NotifyChanged(t);
        }

        /// <summary>
        /// 모든 스킬 레벨 초기화
        /// </summary>
        public void ResetAllSkillLevels()
        {
            foreach (var kv in _skills)
            {
                kv.Value.Level = 1;
                NotifyChanged(kv.Key);
            }
        }
    }
}
