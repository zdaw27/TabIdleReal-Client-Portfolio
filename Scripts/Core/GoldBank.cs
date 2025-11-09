// Assets/Scripts/TabIdleReal/Core/GoldBank.cs
using System;
using System.Collections.Generic;
using System.Linq;

namespace TabIdleReal
{
    /// <summary>
    /// 통합 재화 뱅크
    /// - CurrencyType enum으로 재화 관리 (enum 값 = Items_EtcRow.ID)
    /// - 외부에서는 enum으로 Add/Spend
    /// - Items_Etc 테이블에 존재하는 ID만 사용 가능 (초기화 시 검증)
    /// - ISessionUnit으로 SessionCoordinator가 초기화 관리
    /// </summary>
    public sealed partial class GoldBank : ServiceBase
    {
        private static GoldBank _instance;
        public static GoldBank Instance => _instance ??= new GoldBank();

        private GoldBank() { }

        public override void Initialize()
        {
            InitializeCurrencies();
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // CurrencyType enum (값 = Items_EtcRow.ID)
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        public enum CurrencyType : int
        {
            Gold = 1000001,
            Diamonds = 1000002,
            StatReroll = 1000003,
            GoldDungeonTicket = 1000004,
            BossRushTicket = 1000005,
            GemRushTicket = 1000006,
            StatRerollLock = 1000007,
            Fame = 1000008,
            Treasure = 1000009,
        }

        // 기본값 (코드에서 하드코딩)
        private const long DEFAULT_GOLD = 0;
        private const int DEFAULT_DIAMONDS = 0;
        private const int DEFAULT_DUNGEON_TICKETS = 2;

        // 재화 저장소: BigNum으로 관리
        private readonly Dictionary<CurrencyType, BigNum> _amounts = new();

        // 편의 프로퍼티
        public BigNum Gold => GetAmount(CurrencyType.Gold);
        public BigNum Diamonds => GetAmount(CurrencyType.Diamonds);

        // 하위 호환 프로퍼티
        public long GoldLong => GetAmount(CurrencyType.Gold).ToLongSafe();
        public int DiamondsInt => GetAmount(CurrencyType.Diamonds).ToIntSafe();

        // 이벤트 (BigNum)
        public event Action<CurrencyType, BigNum, BigNum> OnCurrencyChanged; // (type, newValue, delta)
        public event Action<BigNum, BigNum> OnGoldChanged;                   // (newValue, delta)
        public event Action<BigNum, BigNum> OnDiamondsChanged;               // (newValue, delta)

        // OnInitialize() 제거 - ISessionUnit.InitializeAsync()로 이동

        private void InitializeCurrencies()
        {
            // Items_Etc 테이블에서 유효한 ID만 필터링
            var validIds = new HashSet<int>();
            if (GameDataRegistry.Items_EtcList != null)
            {
                foreach (var row in GameDataRegistry.Items_EtcList)
                {
                    if (row != null && row.ID > 0)
                        validIds.Add(row.ID);
                }
            }

            // enum에 정의된 Currency 중 테이블에 존재하는 것만 초기화
            foreach (CurrencyType ct in Enum.GetValues(typeof(CurrencyType)))
            {
                int id = (int)ct;
                if (validIds.Contains(id))
                {
                    if (!_amounts.ContainsKey(ct))
                        _amounts[ct] = BigNum.Zero;
                }
                else
                {
                    UnityEngine.Debug.LogError($"[GoldBank] CurrencyType.{ct} (ID={id})가 Items_Etc 테이블에 없습니다.");
                }
            }

            // 던전 티켓 기본값 지급 (각 2개)
            if (GetAmount(CurrencyType.GoldDungeonTicket).IsZero)
                AddAmount(CurrencyType.GoldDungeonTicket, DEFAULT_DUNGEON_TICKETS);
            if (GetAmount(CurrencyType.BossRushTicket).IsZero)
                AddAmount(CurrencyType.BossRushTicket, DEFAULT_DUNGEON_TICKETS);
            if (GetAmount(CurrencyType.GemRushTicket).IsZero)
                AddAmount(CurrencyType.GemRushTicket, DEFAULT_DUNGEON_TICKETS);
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 외부 API: enum 기반 (BigNum)
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        /// <summary>재화 수량 조회 (BigNum)</summary>
        public BigNum GetAmount(CurrencyType ct)
        {
            return _amounts.TryGetValue(ct, out var v) ? v : BigNum.Zero;
        }

        /// <summary>재화 추가 (BigNum)</summary>
        public void AddAmount(CurrencyType ct, BigNum amount)
        {
            if (amount.IsZero || amount.IsNegative) return;
            var cur = GetAmount(ct);
            SetAmountInternal(ct, cur + amount, amount);
        }

        /// <summary>재화 소비 (BigNum, 부족하면 false)</summary>
        public bool TrySpend(CurrencyType ct, BigNum amount)
        {
            if (amount.IsZero || amount.IsNegative) return true;
            var cur = GetAmount(ct);
            if (cur < amount) return false;
            SetAmountInternal(ct, cur - amount, -amount);
            return true;
        }

        /// <summary>재화 정확한 값으로 설정 (BigNum)</summary>
        public void SetAmount(CurrencyType ct, BigNum newValue)
        {
            var cur = GetAmount(ct);
            var delta = newValue - cur;
            if (delta.IsZero) return;
            SetAmountInternal(ct, newValue, delta);
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 외부 API: enum 기반 (long 하위 호환)
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        /// <summary>재화 수량 조회 (long 하위 호환)</summary>
        public long GetAmountLong(CurrencyType ct)
        {
            return GetAmount(ct).ToLongSafe();
        }

        /// <summary>재화 추가 (long 하위 호환)</summary>
        public void AddAmount(CurrencyType ct, long amount)
        {
            AddAmount(ct, BigNum.FromLong(amount));
        }

        /// <summary>재화 추가 (int 하위 호환)</summary>
        public void AddAmount(CurrencyType ct, int amount)
        {
            AddAmount(ct, BigNum.FromInt(amount));
        }

        /// <summary>재화 소비 (long 하위 호환)</summary>
        public bool TrySpend(CurrencyType ct, long amount)
        {
            return TrySpend(ct, BigNum.FromLong(amount));
        }

        /// <summary>재화 소비 (int 하위 호환)</summary>
        public bool TrySpend(CurrencyType ct, int amount)
        {
            return TrySpend(ct, BigNum.FromInt(amount));
        }

        /// <summary>재화 설정 (long 하위 호환)</summary>
        public void SetAmount(CurrencyType ct, long newValue)
        {
            SetAmount(ct, BigNum.FromLong(newValue));
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // ID 기반 API (테이블 연동용 - BigNum)
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        /// <summary>테이블 ID로 재화 추가 (BigNum)</summary>
        public bool TryAddById(int itemId, BigNum amount)
        {
            if (amount.IsZero || amount.IsNegative) return true;
            if (TryGetCurrencyType(itemId, out var ct))
            {
                AddAmount(ct, amount);
                return true;
            }
            UnityEngine.Debug.LogWarning($"[GoldBank] TryAddById({itemId}) - CurrencyType에 없는 ID입니다.");
            return false;
        }

        /// <summary>테이블 ID로 재화 소비 (BigNum)</summary>
        public bool TrySpendById(int itemId, BigNum amount)
        {
            if (amount.IsZero || amount.IsNegative) return true;
            if (TryGetCurrencyType(itemId, out var ct))
            {
                return TrySpend(ct, amount);
            }
            UnityEngine.Debug.LogWarning($"[GoldBank] TrySpendById({itemId}) - CurrencyType에 없는 ID입니다.");
            return false;
        }

        /// <summary>테이블 ID로 재화 조회 (BigNum)</summary>
        public BigNum GetAmountById(int itemId)
        {
            if (TryGetCurrencyType(itemId, out var ct))
                return GetAmount(ct);
            return BigNum.Zero;
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // ID 기반 API (하위 호환 - long)
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        /// <summary>테이블 ID로 재화 추가 (long 하위 호환)</summary>
        public bool TryAddById(int itemId, long amount)
        {
            return TryAddById(itemId, BigNum.FromLong(amount));
        }

        /// <summary>테이블 ID로 재화 소비 (long 하위 호환)</summary>
        public bool TrySpendById(int itemId, long amount)
        {
            return TrySpendById(itemId, BigNum.FromLong(amount));
        }

        /// <summary>테이블 ID로 재화 조회 (long 하위 호환)</summary>
        public long GetAmountByIdLong(int itemId)
        {
            return GetAmountById(itemId).ToLongSafe();
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 하위 호환 API (Gold/Diamonds 전용 - BigNum)
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        public void AddGold(BigNum amount) => AddAmount(CurrencyType.Gold, amount);
        public bool TrySpendGold(BigNum amount) => TrySpend(CurrencyType.Gold, amount);

        public void AddDiamonds(BigNum amount) => AddAmount(CurrencyType.Diamonds, amount);
        public bool TrySpendDiamonds(BigNum amount) => TrySpend(CurrencyType.Diamonds, amount);

        // 하위 호환 (long)
        public void AddGold(long amount) => AddAmount(CurrencyType.Gold, amount);
        public bool TrySpendGold(long amount) => TrySpend(CurrencyType.Gold, amount);

        public void AddDiamonds(int amount) => AddAmount(CurrencyType.Diamonds, amount);
        public bool TrySpendDiamonds(int amount) => TrySpend(CurrencyType.Diamonds, amount);

        public void AddReroll(long amount) => AddAmount(CurrencyType.StatReroll, amount);

        // 더 레거시 (float 지원)
        public void Add(float amount) => AddGold((long)Math.Round(Math.Max(0f, amount)));
        public void Add(long amount) => AddGold(amount);
        public bool TrySpend(float amount) => TrySpendGold((long)Math.Round(amount));
        public bool TrySpend(long amount) => TrySpendGold(amount);

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 내부: 값 설정 + 이벤트 발행
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        private void SetAmountInternal(CurrencyType ct, BigNum newValue, BigNum delta)
        {
            _amounts[ct] = newValue;

            // BigNum 이벤트
            GameEvents.Currency.Changed.Invoke(ct, newValue, delta);

            // GameEvents 발생 (BigNum → long 변환)

            // 특화 이벤트
            switch (ct)
            {
                case CurrencyType.Gold:
                    GameEvents.Currency.GoldChanged.Invoke(newValue, delta);
                    break;
                case CurrencyType.Diamonds:
                    GameEvents.Currency.DiamondChanged.Invoke(newValue, delta);
                    break;
            }
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 세이브/로드 (BigNum)
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        [Serializable]
        public struct CurrencySaveEntry
        {
            public string enumName; // "Gold", "Diamonds", ...
            public BigNum amount;   // BigNum으로 저장
        }

        public List<CurrencySaveEntry> ExportForSave()
        {
            var list = new List<CurrencySaveEntry>();
            foreach (CurrencyType ct in Enum.GetValues(typeof(CurrencyType)))
            {
                list.Add(new CurrencySaveEntry
                {
                    enumName = ct.ToString(),
                    amount = GetAmount(ct)
                });
            }
            return list;
        }

        public void ImportFromSave(IEnumerable<CurrencySaveEntry> entries, bool fireEvents = true)
        {
            if (entries == null) return;

            foreach (var e in entries)
            {
                if (!Enum.TryParse<CurrencyType>(e.enumName, true, out var ct)) continue;
                BigNum cur = GetAmount(ct);
                BigNum delta = e.amount - cur;
                if (delta.IsZero) continue;

                if (fireEvents)
                    SetAmountInternal(ct, e.amount, delta);
                else
                    _amounts[ct] = e.amount;
            }
        }

        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
        // 유틸리티
        // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

        /// <summary>enum → itemId</summary>
        public static int GetItemId(CurrencyType ct) => (int)ct;

        /// <summary>itemId → enum (존재하면 true)</summary>
        public static bool TryGetCurrencyType(int itemId, out CurrencyType ct)
        {
            if (Enum.IsDefined(typeof(CurrencyType), itemId))
            {
                ct = (CurrencyType)itemId;
                return true;
            }
            ct = default;
            return false;
        }

#if UNITY_EDITOR
        /// <summary>테스트용: 모든 재화에 +10000</summary>
        [UnityEngine.ContextMenu("Add 10000 to All Currencies")]
        public void AddAllCurrencies_10000()
        {
            foreach (CurrencyType ct in Enum.GetValues(typeof(CurrencyType)))
            {
                AddAmount(ct, 10000);
            }
            UnityEngine.Debug.Log("[GoldBank] 모든 재화에 +10000 추가 완료");
        }
#endif
    }
}
