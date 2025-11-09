// Assets/Scripts/Core/DomainManager.cs
using UnityEngine;

namespace TabIdleReal
{
    /// <summary>
    /// 도메인 매니저 - 모든 서비스 인스턴스 생성 및 등록
    /// - DefaultExecutionOrder(-100)로 다른 컴포넌트보다 먼저 실행
    /// - 씬에 하나만 존재해야 함
    /// - Update에서 Tick이 필요한 서비스들을 호출
    /// </summary>
    [DefaultExecutionOrder(-100)]
    public class DomainManager : MonoBehaviour
    {
        private static bool _isInitialized = false;

        void Awake()
        {
            if (_isInitialized)
            {
                Debug.LogWarning("[DomainManager] 이미 초기화됨. 중복 실행 방지.");
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
            _isInitialized = true;

            RegisterAndInitializeServices();
        }

        void Update()
        {
            float dt = Time.deltaTime;
            
            // Tick이 필요한 서비스들 호출
            StageManager.Instance.Tick(dt);
            DungeonTicketManager.Instance.Tick(dt);
            SkillService.Instance.Tick(dt);
        }

        private void RegisterAndInitializeServices()
        {
            Debug.Log("[DomainManager] 서비스 등록 시작...");

            // 1. 인스턴스 생성 및 등록 (Instance 접근으로 lazy initialization)
            ServiceLocator.Register(typeof(GoldBank), GoldBank.Instance);
            ServiceLocator.Register(typeof(StatSnapshotHub), StatSnapshotHub.Instance);
            ServiceLocator.Register(typeof(StageManager), StageManager.Instance);
            ServiceLocator.Register(typeof(WeaponManager), WeaponManager.Instance);
            ServiceLocator.Register(typeof(SkillService), SkillService.Instance);
            ServiceLocator.Register(typeof(RebornManager), RebornManager.Instance);
            ServiceLocator.Register(typeof(CostumeManager), CostumeManager.Instance);
            ServiceLocator.Register(typeof(AchievementManager), AchievementManager.Instance);
            ServiceLocator.Register(typeof(BattlePassManager), BattlePassManager.Instance);
            ServiceLocator.Register(typeof(DungeonTicketManager), DungeonTicketManager.Instance);
            ServiceLocator.Register(typeof(GuideQuestManager), GuideQuestManager.Instance);
            ServiceLocator.Register(typeof(WeaponGachaManager), WeaponGachaManager.Instance);

            Debug.Log("[DomainManager] 12개 서비스 등록 완료");

            // 2. 초기화
            Debug.Log("[DomainManager] 서비스 초기화 시작...");
            ServiceLocator.InitializeAll();
            Debug.Log("[DomainManager] 서비스 초기화 완료");
        }
    }
}
