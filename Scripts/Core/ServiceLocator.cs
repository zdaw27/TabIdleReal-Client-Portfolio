// Assets/Scripts/Core/ServiceLocator.cs
using System;
using System.Collections.Generic;
using UnityEngine;
namespace TabIdleReal
{
    /// <summary>
    /// 모든 게임 서비스(매니저) 중앙 관리
    /// - 싱글턴 패턴 대체
    /// - 초기화 순서 제어
    /// - 의존성 주입
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, ServiceBase> _services = new();
        private static readonly HashSet<ServiceBase> _initializedServices = new();
        private static bool _isInitialized = false;

        /// <summary>서비스 등록</summary>
        public static void Register<T>(T service) where T : ServiceBase
        {
            var type = typeof(T);
            if (_services.ContainsKey(type))
            {
                Debug.LogWarning($"[ServiceLocator] {type.Name} 이미 등록됨. 덮어씀.");
            }
            _services[type] = service;
            Debug.Log($"[ServiceLocator] ✅ {type.Name} 등록");
        }

        /// <summary>Type으로 등록 (다형성 지원)</summary>
        public static void Register(Type type, ServiceBase service)
        {
            if (_services.ContainsKey(type))
            {
                Debug.LogWarning($"[ServiceLocator] {type.Name} 이미 등록됨. 덮어씀.");
            }
            _services[type] = service;
            Debug.Log($"[ServiceLocator] ✅ {type.Name} 등록");
        }

        /// <summary>서비스 가져오기</summary>
        public static T Get<T>() where T : ServiceBase
        {
            var type = typeof(T);
            if (_services.TryGetValue(type, out var service))
            {
                return (T)service;
            }
            throw new Exception($"[ServiceLocator] ❌ {type.Name} 등록되지 않음! InitializeAll()을 먼저 호출했는지 확인하세요.");
        }

        /// <summary>서비스 가져오기 (안전)</summary>
        public static bool TryGet<T>(out T service) where T : ServiceBase
        {
            var type = typeof(T);
            if (_services.TryGetValue(type, out var s))
            {
                service = (T)s;
                return true;
            }
            service = null;
            return false;
        }

        /// <summary>
        /// 명시적 초기화 순서로 모든 서비스 초기화
        /// - 의존성 순서를 명확하게 정의
        /// - 순서가 중요한 서비스는 여기에 명시적으로 나열
        /// </summary>
        public static void InitializeAll()
        {
            if (_isInitialized)
            {
                Debug.LogWarning("[ServiceLocator] 이미 초기화됨");
                return;
            }

            Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            Debug.Log($"[ServiceLocator] 🚀 {_services.Count}개 서비스 초기화 시작");

            // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
            // 명시적 초기화 순서 (의존성 순서대로)
            // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

            // 1. 데이터 로더 (가장 먼저)
            InitializeService(typeof(GameDataLoader));

            // 2. 핵심 시스템 (다른 서비스들이 의존)
            InitializeService(typeof(GoldBank));

            // 3. 스탯/전투 시스템
            InitializeService(typeof(StatSnapshotHub));
            InitializeService(typeof(FameStatUpgrader));
            InitializeService(typeof(GoldStatUpgrader));

            // 4. 게임 시스템
            InitializeService(typeof(StageManager));
            InitializeService(typeof(WeaponManager));
            InitializeService(typeof(SkillService));

            // 5. 진행도/보상 시스템
            InitializeService(typeof(AchievementManager));
            InitializeService(typeof(BattlePassManager));
            InitializeService(typeof(RebornManager));

            // 6. UI 시스템 (마지막)
            InitializeService(typeof(ToastMessage));
            InitializeService(typeof(RichToastMessage));

            // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
            // 나머지 등록된 서비스들 (순서 무관)
            // ━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
            foreach (var kvp in _services)
            {
                if (_initializedServices.Contains(kvp.Value))
                    continue;

                try
                {
                    kvp.Value.Initialize();
                    _initializedServices.Add(kvp.Value);
                    Debug.Log($"[ServiceLocator] ✅ {kvp.Key.Name} 초기화 완료");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[ServiceLocator] ❌ {kvp.Key.Name} 초기화 실패: {ex}");
                    throw;
                }
            }

            _isInitialized = true;
            Debug.Log($"[ServiceLocator] 🎉 {_services.Count}개 서비스 초기화 완료");
            Debug.Log("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        }

        /// <summary>개별 서비스 초기화 (명시적 순서용)</summary>
        private static void InitializeService(Type type)
        {
            if (!_services.TryGetValue(type, out var service))
            {
                Debug.LogWarning($"[ServiceLocator] {type.Name} 등록되지 않음, 우선 초기화 스킵");
                return;
            }
if (_initializedServices.Contains(service))            {                return;            }

            try
            {
                service.Initialize();
                _initializedServices.Add(service);
                Debug.Log($"[ServiceLocator] 🔸 {type.Name} 우선 초기화 완료");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ServiceLocator] ❌ {type.Name} 초기화 실패: {ex}");
                throw;
            }
        }

        /// <summary>초기화 여부 확인</summary>
        public static bool IsInitialized => _isInitialized;

        /// <summary>등록된 서비스 개수</summary>
        public static int Count => _services.Count;

        /// <summary>테스트용: 모든 서비스 제거</summary>
        public static void Clear()
        {
            _services.Clear();
            _initializedServices.Clear();
            _isInitialized = false;
            Debug.Log("[ServiceLocator] 🗑️ 모든 서비스 제거됨");
        }
    }
}
