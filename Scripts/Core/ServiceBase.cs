// Assets/Scripts/Core/ServiceBase.cs
using System;

namespace TabIdleReal
{
    /// <summary>
    /// 모든 게임 서비스(매니저)의 기본 클래스
    /// - DomainManager가 인스턴스 생성 및 등록
    /// - 자식 클래스는 Initialize() 구현 필수
    /// - Dispose()로 리소스 정리 (선택)
    /// </summary>
    public abstract class ServiceBase : IDisposable
    {
        /// <summary>
        /// 서비스 초기화
        /// - ServiceLocator.InitializeAll()에서 호출됨
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// 리소스 정리
        /// - 필요한 경우 자식 클래스에서 override
        /// </summary>
        public virtual void Dispose()
        {
            // 기본 구현은 비어있음
        }
    }
}
