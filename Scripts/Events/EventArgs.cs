// Assets/Scripts/Events/EventArgs.cs
using System;
using System.Collections.Generic;

namespace TabIdleReal
{
    /// <summary>
    /// 모든 이벤트 파라미터 클래스의 기본 클래스
    /// - 이벤트 발생 시간 추적
    /// - 디버깅 및 리플레이 시스템에 활용
    /// </summary>
    public abstract class GameEventArgs
    {
        public DateTime Timestamp { get; } = DateTime.UtcNow;
    }

    // ═══════════════════════════════════════
    // 스테이지 이벤트
    // ═══════════════════════════════════════
    
    public class StageClearedEventArgs : GameEventArgs
    {
        public int StageId { get; set; }
        public bool IsBoss { get; set; }
        public TimeSpan ClearTime { get; set; }
    }

    public class MonsterKilledEventArgs : GameEventArgs
    {
        public Monster Monster { get; set; }
        public int Damage { get; set; }
        public bool IsCritical { get; set; }
        public BigNum GoldReward { get; set; }
    }

    public class StageStartedEventArgs : GameEventArgs
    {
        public int StageId { get; set; }
        public bool IsBoss { get; set; }
        public int MonsterCount { get; set; }
    }

    public class StageProgressEventArgs : GameEventArgs
    {
        public int Current { get; set; }
        public int Max { get; set; }
        public float Percentage => Max > 0 ? (float)Current / Max : 0f;
    }

    // ═══════════════════════════════════════
    // 던전 이벤트
    // ═══════════════════════════════════════

    public class DungeonEnteredEventArgs : GameEventArgs
    {
        public DungeonType Type { get; set; }
        public int Tier { get; set; }
        public int ReturnStageId { get; set; }
        public Guid SessionId { get; set; }
        public float TimeLimit { get; set; }
    }

    public class DungeonStartedEventArgs : GameEventArgs
    {
        public DungeonType Type { get; set; }
        public int Tier { get; set; }
        public int StageId { get; set; }
        public float CurrentHP { get; set; }
        public float MaxHP { get; set; }
        public Guid SessionId { get; set; }
    }

    public class DungeonEndedEventArgs : GameEventArgs
    {
        public DungeonType Type { get; set; }
        public int Tier { get; set; }
        public StageManager.DungeonEndReason Reason { get; set; }
        public BigNum GoldEarned { get; set; }
        public TimeSpan TimeElapsed { get; set; }
        public Guid SessionId { get; set; }
        public bool IsSuccess => Reason == StageManager.DungeonEndReason.Cleared;
    }

    // ═══════════════════════════════════════
    // 재화 이벤트
    // ═══════════════════════════════════════

    public class CurrencyChangedEventArgs : GameEventArgs
    {
        public GoldBank.CurrencyType Type { get; set; }
        public BigNum NewValue { get; set; }
        public BigNum Delta { get; set; }
        public BigNum PreviousValue => NewValue - Delta;
    }

    // ═══════════════════════════════════════
    // 무기 이벤트
    // ═══════════════════════════════════════

    public class WeaponObtainedEventArgs : GameEventArgs
    {
        public Weapon Weapon { get; set; }
        public WeaponRarity Rarity { get; set; }
        public string Source { get; set; } // "Gacha", "Drop", "Quest"
    }

    public class WeaponRerolledEventArgs : GameEventArgs
    {
        public Weapon Weapon { get; set; }
        public List<WeaponOption> OldOptions { get; set; }
        public List<WeaponOption> NewOptions { get; set; }
        public int RerollCost { get; set; }
    }

    // ═══════════════════════════════════════
    // 퀘스트 이벤트
    // ═══════════════════════════════════════

    public class QuestActivatedEventArgs : GameEventArgs
    {
        public BaseQuest Quest { get; set; }
        public int QuestIndex { get; set; }
    }

    public class QuestCompletedEventArgs : GameEventArgs
    {
        public BaseQuest Quest { get; set; }
        public TimeSpan CompletionTime { get; set; }
    }

    public class QuestRewardedEventArgs : GameEventArgs
    {
        public BaseQuest Quest { get; set; }
        public List<ItemReward> Rewards { get; set; }
    }

    public class QuestProgressEventArgs : GameEventArgs
    {
        public BaseQuest Quest { get; set; }
        public int CurrentProgress { get; set; }
        public int RequiredProgress { get; set; }
        public float Percentage => RequiredProgress > 0 ? (float)CurrentProgress / RequiredProgress : 0f;
    }

    // ═══════════════════════════════════════
    // 스킬 이벤트
    // ═══════════════════════════════════════

    public class SkillLevelChangedEventArgs : GameEventArgs
    {
        public SkillBase Skill { get; set; }
        public int NewLevel { get; set; }
        public int PreviousLevel { get; set; }
        public int UpgradeCost { get; set; }
    }

    // ═══════════════════════════════════════
    // 스탯 이벤트
    // ═══════════════════════════════════════

    public class StatLevelChangedEventArgs : GameEventArgs
    {
        public StatType StatType { get; set; }
        public int NewLevel { get; set; }
        public int PreviousLevel { get; set; }
        public BigNum Cost { get; set; }
    }

    public class CombatPowerChangedEventArgs : GameEventArgs
    {
        public long NewCombatPower { get; set; }
        public long PreviousCombatPower { get; set; }
        public long Delta => NewCombatPower - PreviousCombatPower;
    }

    // ═══════════════════════════════════════
    // 환생 이벤트
    // ═══════════════════════════════════════

    public class RebornCompletedEventArgs : GameEventArgs
    {
        public int Fame { get; set; }
        public int Treasure { get; set; }
        public int NewRebornLevel { get; set; }
        public int StageReached { get; set; }
    }

    // ═══════════════════════════════════════
    // 배틀패스 이벤트
    // ═══════════════════════════════════════

    public class BattlePassExpGainedEventArgs : GameEventArgs
    {
        public int Exp { get; set; }
        public int NewTier { get; set; }
        public int PreviousTier { get; set; }
        public string Source { get; set; }
    }

    public class BattlePassRewardClaimedEventArgs : GameEventArgs
    {
        public int Tier { get; set; }
        public bool IsPremium { get; set; }
        public List<ItemReward> Rewards { get; set; }
    }

    public class BattlePassQuestCompletedEventArgs : GameEventArgs
    {
        public int QuestId { get; set; }
        public string QuestName { get; set; }
        public int ExpReward { get; set; }
        public bool IsDaily { get; set; }
    }

    // ═══════════════════════════════════════
    // 세션 이벤트
    // ═══════════════════════════════════════

    public class CriticalDataChangedEventArgs : GameEventArgs
    {
        public string Source { get; set; }
        public string Description { get; set; }
    }

    // ═══════════════════════════════════════
    // 보조 클래스
    // ═══════════════════════════════════════

    public class ItemReward
    {
        public GoldBank.CurrencyType CurrencyType { get; set; }
        public BigNum Amount { get; set; }
        public string Description { get; set; }
    }
}
