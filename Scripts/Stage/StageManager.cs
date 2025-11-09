using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;

public class GameDataNotFoundException : Exception
{
    public GameDataNotFoundException(string msg) : base(msg) { }
}

namespace TabIdleReal
{
    /// <summary>
    /// Stage/Dungeon 파사드 & 호스트.
    /// - 스폰/타이머/입장비/보상/파싱: 컨트롤러에게 위임
    /// - 여기서는 이벤트/상태만 중계 + 컨트롤러 수명관리
    /// </summary>
    public sealed partial class StageManager : ServiceBase
    {
        public static StageManager Instance { get; private set; }

        // ===== 기존 외부 이벤트 (보존) =====
        public event Action<Monster> OnMonsterKilled;
        public event Action<int, bool> OnStageStarted;
        public event Action<int, int> OnStageProgress;
        public event Action<float> OnBossTimerTick;
        public event Action<int> OnStageCleared;

        // ===== 던전 공식 이벤트 (보존) =====
        public enum DungeonEndReason { Cleared, Timeout, Aborted }
        public event Action<DungeonType, int, int, Guid> OnDungeonEntered;
        public event Action<DungeonType, int, int, float, float, Guid> OnDungeonStarted;
        public event Action<DungeonType, int, DungeonEndReason, long, float, Guid> OnDungeonEnded;
        public event Action<DungeonType, int, int, Guid> OnDungeonExitedToMain;

        // ===== 던전 진행도 이벤트 (보존) =====
        public event Action<DungeonType, int> OnDungeonProgressChanged;
        public event Action OnDungeonProgressReset;

        // ===== HUD 경량 이벤트 =====
        public event Action<bool> ModeChanged;                       // true=던전, false=메인
        public event Action<DungeonType, int, float> DungeonEntered; // (type, tier, limitSec)
        public event Action<float> DungeonTimerChanged;              // 남은시간(컨트롤러가 갱신 호출)
        public event Action<DungeonEndReason, long> DungeonEndedHud;
        public event Action<DungeonEndReason, long> DungeonEnded;

        [Header("Prefabs")]
        public Monster mobPrefab;
        public Monster bossPrefab; // (호환 보존)

        [Header("Spawn Point")]
        public Transform spawnPoint;
        public Vector3 fallbackSpawnPosition = Vector3.zero;

        [Header("Legacy Compat (외부 참조 보존)")]
        public int currentStage = 1;            // 메인 현재 스테이지 ID
        public int killsToClear = 10;           // (안 씀) 보존용
        public float respawnDelayAfterDeath = 0.35f;

        // 프로퍼티 래퍼 (컨벤션)
        public int CurrentStage => currentStage;

        [Header("Runtime")]
        public Monster currentTarget;           // 외부 참조용
        public float bossTimer;                 // 현재 남은 시간(컨트롤러가 갱신)

        // 호스팅
        private IStageController _controller;
        private bool _externallyControlled = false;
        private int _returnStageId = 1;

        // 세션
        private DungeonType _curType = DungeonType.Unknown;
        private int _curTier = 0;
        private Guid _runId;
        private System.Diagnostics.Stopwatch _timer;

        // 진행/스폰 상태
        private readonly List<Monster> _alive = new();
        private readonly HashSet<(DungeonType type, int tier)> _cleared = new();

        // ===== 외부 노출 =====
        public bool IsInDungeon { get; private set; }
        public DungeonType CurrentDungeonType => _curType;
        public int CurrentTier => _curTier;

        // ===== 컨트롤러 헬퍼 =====
        public Monster MobPrefab => mobPrefab;
        public Vector3 SpawnPosition => spawnPoint ? spawnPoint.position : fallbackSpawnPosition;
        internal MonstersRow ResolveMonsterRow(int monsterId)
            => GameDataRegistry.MonstersList.FirstOrDefault(r => r != null && r.ID == monsterId);

        public void RegisterSpawned(Monster m)
        {
            if (m == null) return;
            _alive.Add(m);
            currentTarget = m;

            // MonsterProfileUI 업데이트
            var hudUI = FindObjectOfType<HudUI>();
            if (hudUI != null && hudUI.monsterProfileUI != null)
            {
                hudUI.monsterProfileUI.ShowMonster(m.DisplayName, m.isBoss);
                hudUI.monsterProfileUI.UpdateHP((long)m.CurrentHP, (long)m.MaxHP);

                // HP 변경 이벤트 구독
                m.OnHpChanged += (oldHp, curHp, maxHp) =>
                {
                    if (hudUI != null && hudUI.monsterProfileUI != null)
                    {
                        hudUI.monsterProfileUI.UpdateHP((long)curHp, (long)maxHp);
                    }
                };

                Debug.Log($"[StageManager] MonsterProfileUI 표시 - {m.DisplayName}, 보스: {m.isBoss}");
            }
        }

        public override void Initialize()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        void Start()
        {
            // LoadAsync()에서 로드된 currentStage로 시작하도록 변경
            // 여기서 시작하면 로드 전 currentStage=1로 시작되는 문제 발생
            // var main = new MainStageController(Mathf.Max(1, currentStage));
            // StartController(main, currentStage);
        }

        void Update()
        {
            if (_externallyControlled)
            {
                _controller?.Update(Time.deltaTime); // ★ 타이머는 컨트롤러가 자체 처리
                if (currentTarget == null || (currentTarget && !currentTarget.IsAlive))
                    currentTarget = null;
            }

            // MonsterProfileUI 보스 타이머 업데이트
            if (currentTarget != null && currentTarget.isBoss && bossTimer > 0f)
            {
                var hudUI = FindObjectOfType<HudUI>();
                if (hudUI != null && hudUI.monsterProfileUI != null)
                {
                    hudUI.monsterProfileUI.UpdateBossTimer(bossTimer);
                }
            }
        }

        // ===== 외부 API =====
        /// <summary>로드된 currentStage로 메인 컨트롤러 재시작</summary>
        public void RestartMainController()
        {
            Debug.Log($"[StageManager] 메인 컨트롤러 재시작: currentStage={currentStage}");
            var main = new MainStageController(Mathf.Max(1, currentStage));
            StartController(main, currentStage);
        }

        public void StartController(IStageController controller, int returnStageId)
        {
            if (controller == null) return;

            if (_controller != null)
            {
                Unsub(_controller);
                _controller.Exit();
            }

            _controller = controller;
            _returnStageId = (returnStageId > 0) ? returnStageId : Mathf.Max(1, currentStage);
            _externallyControlled = true;

            _controller.Entered += HandleEntered;
            _controller.StartRequested += HandleStartRequested;
            _controller.EndRequested += HandleEndRequested;

            ClearAlive();
            _controller.Enter(this, _returnStageId);
        }

        public void OnMonsterDied(Monster m)
        {
            _alive.Remove(m);
            if (m != null)
            {
                GameEvents.Stage.MonsterKilled.Invoke(m);

                // GameEvents 발생 (Achievement, GuideQuest, BattlePass 등에서 사용)
                GameEvents.Stage.MonsterKilled.Invoke(m);
            }
            _controller?.OnMonsterDied(m); // 컨트롤러 진행 위임

            // MonsterProfileUI 숨김
            var hudUI = FindObjectOfType<HudUI>();
            if (hudUI != null && hudUI.monsterProfileUI != null)
            {
                hudUI.monsterProfileUI.HideMonster();
            }
        }

        public bool IsCleared(DungeonType type, int tier) => _cleared.Contains((type, tier));
        public bool CanEnter(DungeonType type, int tier) => tier <= 1 || _cleared.Contains((type, tier - 1));
        public int GetHighestClearedTier(DungeonType type)
            => _cleared.Where(e => e.type == type).Select(e => e.tier).DefaultIfEmpty(0).Max();

        public IEnumerable<string> ExportProgressStrings() => _cleared.Select(e => $"{(int)e.type}:{e.tier}");
        public void ImportProgressStrings(IEnumerable<string> data)
        {
            _cleared.Clear();
            if (data != null)
            {
                foreach (var s in data)
                {
                    var sp = s.Split(':');
                    if (sp.Length != 2) continue;
                    if (!int.TryParse(sp[0], out var ti)) continue;
                    if (!int.TryParse(sp[1], out var t)) continue;
                    _cleared.Add(((DungeonType)ti, t));
                }
            }
            GameEvents.Dungeon.ProgressReset.Invoke();
        }

        public void ResetDungeonInfo(bool resetProgress = true, bool resetSessionState = true, bool restartMain = true)
        {
            if (resetSessionState)
            {
                if (_controller != null) { Unsub(_controller); _controller.Exit(); _controller = null; }
                _externallyControlled = false;
                _curType = DungeonType.Unknown;
                _curTier = 0;
                _runId = default;
                _timer = null;
                IsInDungeon = false;
                bossTimer = 0f;
                try { ClearAlive(); } catch { }
            }

            if (resetProgress)
            {
                _cleared.Clear();
                GameEvents.Dungeon.ProgressReset.Invoke();
            }

            if (restartMain)
            {
                var main = new MainStageController(Mathf.Max(1, currentStage));
                StartController(main, currentStage);
            }
        }

        // ===== 내부 연결 =====
        private void Unsub(IStageController c)
        {
            c.Entered -= HandleEntered;
            c.StartRequested -= HandleStartRequested;
            c.EndRequested -= HandleEndRequested;
        }

        private void HandleEntered(DungeonType type, int tier, int returnStage)
        {
            _curType = type;
            _curTier = tier;
            _returnStageId = returnStage;
            _runId = Guid.NewGuid();

            _timer = System.Diagnostics.Stopwatch.StartNew();
            GameEvents.Dungeon.Entered.Invoke(_curType, _curTier, _returnStageId, _runId);

            IsInDungeon = (type != DungeonType.Unknown);
            GameEvents.Dungeon.ModeChanged.Invoke(IsInDungeon);
        }

        private void HandleStartRequested(int monsterId, float hpMul, float limitSec)
        {
            // 타이머/스폰 모두 컨트롤러 소유. HUD 표면 신호만 보냄.
            if (limitSec > 0f)
            {
                bossTimer = limitSec; // 첫 표시용
                GameEvents.Dungeon.EnteredLight.Invoke(_curType, _curTier, limitSec);
                GameEvents.Stage.BossTimerTick.Invoke(bossTimer);
                GameEvents.Dungeon.TimerChanged.Invoke(bossTimer);
            }

            GameEvents.Stage.Started.Invoke(Mathf.Max(1, currentStage), limitSec > 0f);
            GameEvents.Stage.Progress.Invoke(0, 1);
            GameEvents.Dungeon.Started.Invoke(_curType, _curTier, monsterId, Mathf.Max(1f, hpMul), Mathf.Max(0f, limitSec), _runId);
        }

        private void HandleEndRequested(DungeonEndReason reason, long reward)
        {
            EndDungeon(reason, reward);
        }

        private void EndDungeon(DungeonEndReason reason, long reward)
        {
            float elapsed = 0f;
            if (_timer != null) { _timer.Stop(); elapsed = (float)_timer.Elapsed.TotalSeconds; }

            if (reason == DungeonEndReason.Cleared)
                MarkClearedInternal(_curType, _curTier);

            GameEvents.Dungeon.Ended.Invoke(_curType, _curTier, reason, Math.Max(0, reward), Mathf.Max(0f, elapsed), _runId);

            var safeReward = Math.Max(0, reward);
            GameEvents.Dungeon.EndedHud.Invoke(reason, safeReward);
            GameEvents.Dungeon.EndedHud.Invoke(reason, safeReward);

            if (_controller != null) { Unsub(_controller); _controller.Exit(); _controller = null; }
            _externallyControlled = false;

            GameEvents.Dungeon.ExitedToMain.Invoke(_curType, _curTier, _returnStageId, _runId);

            IsInDungeon = false;
            bossTimer = 0f;
            GameEvents.Dungeon.ModeChanged.Invoke(false);

            _curType = DungeonType.Unknown; _curTier = 0; _runId = default; _timer = null;
            ClearAlive();

            // 메인 복귀
            var main = new MainStageController(Mathf.Max(1, _returnStageId));
            StartController(main, _returnStageId);
        }

        private void MarkClearedInternal(DungeonType type, int tier)
        {
            if (type != DungeonType.Unknown && _cleared.Add((type, tier)))
            {
                GameEvents.Dungeon.ProgressChanged.Invoke(type, tier);

                // 배틀패스용 이벤트 발행
                GameEvents.Dungeon.Cleared.Invoke(type);

                // 던전 진행도 저장
                GameEvents.Session.CriticalDataChanged.Invoke("DungeonProgress");
            }
        }

        private void ClearAlive()
        {
            foreach (var m in _alive)
                if (m != null) Destroy(m.gameObject);
            _alive.Clear();
            currentTarget = null;
        }

        // ===== HUD 갱신용 (컨트롤러가 틱마다 호출) =====
        public void NotifyBossTick(float leftSec)
        {
            bossTimer = Mathf.Max(0f, leftSec);
            GameEvents.Stage.BossTimerTick.Invoke(bossTimer);
            GameEvents.Dungeon.TimerChanged.Invoke(bossTimer);
        }

        // 레거시 외부 이벤트 재사용용
        public void NotifyStageStarted(int stageId, bool isBoss) => GameEvents.Stage.Started.Invoke(stageId, isBoss);
        public void NotifyStageProgress(int kills, int need) => GameEvents.Stage.Progress.Invoke(kills, need);
        public void NotifyStageCleared(int stageId)
        {
            GameEvents.Stage.Cleared.Invoke(stageId);
            GameEvents.Stage.Cleared.Invoke(stageId);
        }
        public void NotifyGiveGold(long amount) => GoldBank.Instance?.AddGold(amount);

#if UNITY_EDITOR
        // ===== 에디터 전용: 던전 강제 클리어 =====
        [UnityEngine.ContextMenu("Force Clear Current Dungeon")]
        public void EditorForceClearDungeon()
        {
            if (!IsInDungeon)
            {
                Debug.LogWarning("[StageManager] 현재 던전이 아닙니다.");
                return;
            }

            if (_controller == null)
            {
                Debug.LogWarning("[StageManager] 컨트롤러가 없습니다.");
                return;
            }

            // 현재 던전 타입에 따라 보상 계산
            long reward = CalculateDungeonReward(_curType, _curTier);

            // 보상 지급
            if (reward > 0)
            {
                var rewardItemId = GetRewardItemId(_curType);
                if (rewardItemId > 0)
                {
                    GoldBank.Instance?.TryAddById(rewardItemId, reward);
                    Debug.Log($"[StageManager] 에디터 강제 클리어 보상: {reward} (ItemID: {rewardItemId})");
                }
            }

            // 현재 살아있는 몬스터 모두 제거
            ClearAlive();

            // 던전 클리어 처리
            Debug.Log($"[StageManager] 에디터 강제 클리어: {_curType} Tier {_curTier}");
            EndDungeon(DungeonEndReason.Cleared, reward);
        }

        private long CalculateDungeonReward(DungeonType type, int tier)
        {
            switch (type)
            {
                case DungeonType.Gold:
                    var goldRow = GameDataRegistry.GoldDungeonList?.Find(r => r != null && r.Tier == tier);
                    if (goldRow == null)
                    {
                        Debug.LogError($"[StageManager] GoldDungeonList에서 Tier {tier} 데이터를 찾을 수 없습니다!");
                        return 0;
                    }
                    return goldRow.RewardAmount;

                case DungeonType.Diamond:
                    var diaRow = GameDataRegistry.DiamondDungeonList?.Find(r => r != null && r.Tier == tier);
                    if (diaRow == null)
                    {
                        Debug.LogError($"[StageManager] DiamondDungeonList에서 Tier {tier} 데이터를 찾을 수 없습니다!");
                        return 0;
                    }
                    return diaRow.RewardAmount;

                case DungeonType.Ruby:
                    // 리롤은 모든 몬스터 처치 보상 합계
                    var rerollRows = GameDataRegistry.StatRerollGemDungeonList;
                    if (rerollRows == null || rerollRows.Count == 0)
                    {
                        Debug.LogError($"[StageManager] StatRerollGemDungeonList가 비어있습니다!");
                        return 0;
                    }
                    long total = 0;
                    foreach (var r in rerollRows)
                        if (r != null) total += r.RewardAmount;
                    return total;

                default:
                    Debug.LogError($"[StageManager] 알 수 없는 던전 타입: {type}");
                    return 0;
            }
        }

        private int GetRewardItemId(DungeonType type)
        {
            switch (type)
            {
                case DungeonType.Gold:
                    var goldRow = GameDataRegistry.GoldDungeonList?.Find(r => r != null);
                    if (goldRow == null)
                    {
                        Debug.LogError($"[StageManager] GoldDungeonList에서 RewardItem을 찾을 수 없습니다!");
                        return 0;
                    }
                    return goldRow.RewardItem;

                case DungeonType.Diamond:
                    var diaRow = GameDataRegistry.DiamondDungeonList?.Find(r => r != null);
                    if (diaRow == null)
                    {
                        Debug.LogError($"[StageManager] DiamondDungeonList에서 RewardItem을 찾을 수 없습니다!");
                        return 0;
                    }
                    return diaRow.RewardItem;

                case DungeonType.Ruby:
                    var rerollRow = GameDataRegistry.StatRerollGemDungeonList?.Find(r => r != null);
                    if (rerollRow == null)
                    {
                        Debug.LogError($"[StageManager] StatRerollGemDungeonList에서 RewardItem을 찾을 수 없습니다!");
                        return 0;
                    }
                    return rerollRow.RewardItem;

                default:
                    Debug.LogError($"[StageManager] 알 수 없는 던전 타입: {type}");
                    return 0;
            }
        }
#endif
    }
}
