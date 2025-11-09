// Assets/Scripts/WeaponSystem/WeaponManager.cs
using System;
using System.Collections.Generic;
using UnityEngine;
namespace TabIdleReal
{
    public partial class WeaponManager : ServiceBase 
    {
        public static WeaponManager Instance { get; private set; }

        // 기존 공개 리스트는 그대로 둬서 다른 코드 깨지지 않게 유지
        public List<Weapon> Inventory = new List<Weapon>();
        public Weapon Equipped;

        // 🔔 인벤토리 변경 이벤트 (UI가 구독해서 바로 갱신)
        public event Action<IReadOnlyList<Weapon>> InventoryChanged;

        public override void Initialize()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            // 씬 시작 시 이미 있는 아이템이 있다면 한 번 방송
            if (Inventory != null && Inventory.Count > 0)
                GameEvents.WeaponEvent.InventoryChanged.Invoke(Inventory);
        }

        /// <summary>
        /// 드랍 테이블에서 넘어온 '무기 테이블 ID'로 무기를 생성해서 인벤토리에 추가.
        /// 생성 실패 시 아무 것도 추가하지 않고 로그만 남김(랜덤 폴백 없음).
        /// </summary>
        public void TryAddWeaponById(int weaponsRowId)
        {
            var w = WeaponGenerator.CreateByID(weaponsRowId); // 네가 올린 Generator 사용
            if (w == null)
            {
                Debug.LogWarning($"[Drop] CreateByID({weaponsRowId}) 실패. 스킵.");
                return;
            }
            AddWeapon(w);
        }
        /// <summary>
        /// 외부에서 무기를 추가할 때 사용할 수 있는 안전 메서드
        /// (이벤트 자동 브로드캐스트)
        /// </summary>
        public void AddWeapon(Weapon w)
        {
            if (w == null) return;
            Inventory.Add(w);
            Debug.Log($"[AddWeapon] {w}  (caller)\n{Environment.StackTrace}");
            GameEvents.WeaponEvent.InventoryChanged.Invoke(Inventory);
            GameEvents.WeaponEvent.Obtained.Invoke(w);
        }

        /// <summary>
        /// 초기화/부트스트랩 로직이 끝난 시점에서
        /// 현재 인벤토리를 한 번 브로드캐스트하고 싶을 때 호출
        /// </summary>
        public void NotifyInventoryReady()
        {
            GameEvents.WeaponEvent.InventoryChanged.Invoke(Inventory);   // 🔔 현재 상태 브로드캐스트
        }

        public void Equip(Weapon w)
        {
            if (w == null) return;
            Equipped = w;
            Debug.Log($"[Equip] {w.Name}");
            // (선택) 장비 변경 시에도 필요하면 이벤트를 보낼 수 있음:
            // GameEvents.WeaponEvent.InventoryChanged.Invoke(Inventory);
        }

        public override string ToString()
        {
            string s = "=== Inventory ===\n";
            foreach (var w in Inventory) s += w + "\n";
            s += "=================\n";
            return s;
        }
    }
}
