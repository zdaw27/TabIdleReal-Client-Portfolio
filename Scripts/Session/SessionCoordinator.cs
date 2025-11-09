// Assets/Scripts/SessionFlow/SessionCoordinator.cs
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
namespace TabIdleReal
{
    /// 인스펙터 순서대로 Initialize→Load, 역순으로 Save.
    public sealed class SessionCoordinator : MonoBehaviour
    {
        [Tooltip("ISessionUnit 구현 컴포넌트를 드래그&드랍하세요 (인스펙터 순서대로 처리)")]
        public List<MonoBehaviour> units = new();

        [Header("Log")]
        public bool verbose = true;
        public bool stopOnError = true;

        [Header("Auto Save")]
        [Tooltip("자동 저장 주기 (초). 0이면 비활성화")]
        public float autoSaveInterval = 120f; // 2분

        string _uid;
        bool _loggedIn;
        float _lastSaveTime;

        public string CurrentUid => _uid;
        public bool IsLoggedIn => _loggedIn;

        void Update()
        {
            // 주기적 자동 저장
            if (_loggedIn && autoSaveInterval > 0)
            {
                if (Time.time - _lastSaveTime >= autoSaveInterval)
                {
                    _lastSaveTime = Time.time;
                    AutoSaveAsync().Forget();
                }
            }
        }

        async UniTask AutoSaveAsync()
        {
            if (verbose) Debug.Log("[Session] Auto-Save start");

            var rev = units.OfType<ISessionUnit>().ToList();
            rev.Reverse();

            foreach (var u in rev)
            {
                try
                {
                    await u.SaveAsync();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[Session] Auto-Save FAIL: {((MonoBehaviour)u).name} : {ex}");
                }
            }

            if (verbose) Debug.Log("[Session] Auto-Save complete");
        }

        /// <summary>
        /// 중요 데이터 변경 시 전체 저장 (완료 대기)
        /// </summary>
        async void SaveCritical(string source)
        {
            if (!_loggedIn) return;

            if (verbose) Debug.Log($"[Session] 🔒 Critical Save triggered by: {source}");

            var rev = units.OfType<ISessionUnit>().ToList();
            rev.Reverse();

            foreach (var u in rev)
            {
                try
                {
                    await u.SaveAsync();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[Session] Critical Save FAIL: {((MonoBehaviour)u).name} : {ex}");
                    if (stopOnError) throw;
                }
            }

            if (verbose) Debug.Log($"[Session] 🔒 Critical Save complete ({source})");
        }

        public async UniTask LoginAsync(string uid)
        {
            if (string.IsNullOrEmpty(uid)) throw new ArgumentException("uid is null/empty");
            if (_loggedIn) await LogoutAsync();

            _uid = uid;

            var ordered = units.OfType<ISessionUnit>().ToList(); // 인스펙터 순서

            foreach (var u in ordered)
            {
                try
                {
                    if (verbose) Debug.Log($"[Session] Initialize -> {((MonoBehaviour)u).name}");
                    await u.InitializeAsync(uid);

                    if (verbose) Debug.Log($"[Session] Load -> {((MonoBehaviour)u).name}");
                    await u.LoadAsync();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[Session] Login step FAIL: {((MonoBehaviour)u).name} : {ex}");
                    if (stopOnError) throw;
                }
            }

            _loggedIn = true;
            _lastSaveTime = Time.time;

            // 이벤트 구독
            GameEvents.Session.CriticalDataChanged.Subscribe(SaveCritical);

            // 데이터 로드 완료 이벤트 발생 (UI 갱신 트리거)
            GameEvents.Session.DataLoaded.Invoke();

            if (verbose) Debug.Log("[Session] READY");
        }

        public async UniTask LogoutAsync()
        {
            if (!_loggedIn) return;
            if (verbose) Debug.Log("[Session] Logout start");

            var rev = units.OfType<ISessionUnit>().ToList();
            rev.Reverse(); // 역순 저장

            foreach (var u in rev)
            {
                try
                {
                    if (verbose) Debug.Log($"[Session] Save -> {((MonoBehaviour)u).name}");
                    await u.SaveAsync();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[Session] Logout step FAIL: {((MonoBehaviour)u).name} : {ex}");
                    if (stopOnError) throw;
                }
            }

            // 이벤트 구독 해제
            GameEvents.Session.CriticalDataChanged.Unsubscribe(SaveCritical);

            _loggedIn = false;
            _uid = null;
            if (verbose) Debug.Log("[Session] LOGGED OUT");
        }

        /// <summary>첫 회원 가입: Initialize→Reset→Save</summary>
        public async UniTask SignUpAsync(string uid)
        {
            if (string.IsNullOrEmpty(uid)) throw new ArgumentException("uid is null/empty");
            if (_loggedIn) await LogoutAsync();

            _uid = uid;

            var ordered = units.OfType<ISessionUnit>().ToList();

            foreach (var u in ordered)
            {
                try
                {
                    if (verbose) Debug.Log($"[Session] Initialize -> {((MonoBehaviour)u).name}");
                    await u.InitializeAsync(uid);

                    if (verbose) Debug.Log($"[Session] Reset -> {((MonoBehaviour)u).name}");
                    u.Reset();

                    if (verbose) Debug.Log($"[Session] Save(initial) -> {((MonoBehaviour)u).name}");
                    await u.SaveAsync();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[Session] SignUp step FAIL: {((MonoBehaviour)u).name} : {ex}");
                    if (stopOnError) throw;
                }
            }

            _loggedIn = true;

            // 이벤트 구독
            GameEvents.Session.CriticalDataChanged.Subscribe(SaveCritical);

            // 데이터 로드 완료 이벤트 발생 (UI 갱신 트리거)
            GameEvents.Session.DataLoaded.Invoke();

            if (verbose) Debug.Log("[Session] SIGNED UP & READY");
        }

        /// <summary>현재 uid 기준, 전 유닛 로컬 상태만 초기화(저장 안 함)</summary>
        public void ResetAllLocal()
        {
            foreach (var u in units.OfType<ISessionUnit>())
            {
                try
                {
                    u.Reset();
                    if (verbose) Debug.Log($"[Session] Local Reset -> {((MonoBehaviour)u).name}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[Session] Local Reset FAIL: {((MonoBehaviour)u).name} : {ex}");
                    if (stopOnError) throw;
                }
            }
        }

        void OnApplicationQuit()
        {
#if UNITY_EDITOR
            // Unity Editor에서는 OnApplicationQuit 시 Firebase가 응답하지 않아 무한 대기 발생
            // OnApplicationPause에서 이미 저장했으므로 Editor에서는 스킵
            Debug.Log("[Session] OnApplicationQuit - Skipped in Editor (already saved in OnApplicationPause)");
            return;
#endif

            // 앱 종료 시 저장 시도 (Unity 종료 타이밍 이슈로 실패할 수 있음)
            if (_loggedIn)
            {
                try
                {
                    Debug.Log("[Session] OnApplicationQuit - Attempting save");

                    // 동기적으로 각 유닛 저장 시도
                    var rev = units.OfType<ISessionUnit>().ToList();
                    rev.Reverse();

                    foreach (var u in rev)
                    {
                        try
                        {
                            // SaveAsync를 동기적으로 실행 시도
                            u.SaveAsync().GetAwaiter().GetResult();
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning($"[Session] OnApplicationQuit Save failed for {((MonoBehaviour)u).name}: {ex.Message}");
                        }
                    }

                    Debug.Log("[Session] OnApplicationQuit - Save completed");
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[Session] OnApplicationQuit - Save error (expected during shutdown): {ex.Message}");
                }
            }
        }

        void OnApplicationPause(bool pauseStatus)
        {
            // 백그라운드로 전환 시 (홈버튼, 전화 등)
            if (pauseStatus && _loggedIn)
            {
                Debug.Log("[Session] ⏸️ OnApplicationPause - Starting Auto Save");

                try
                {
                    // 동기적으로 각 유닛 저장
                    var rev = units.OfType<ISessionUnit>().ToList();
                    rev.Reverse();

                    foreach (var u in rev)
                    {
                        try
                        {
                            var name = ((MonoBehaviour)u).name;
                            Debug.Log($"[Session] ⏸️ Saving {name}...");
                            u.SaveAsync().GetAwaiter().GetResult();
                            Debug.Log($"[Session] ⏸️ ✅ Saved {name}");
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"[Session] ⏸️ ❌ Save failed for {((MonoBehaviour)u).name}: {ex}");
                        }
                    }

                    Debug.Log("[Session] ⏸️ Auto Save Complete!");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[Session] ⏸️ Auto Save Error: {ex}");
                }
            }
        }
    }
}
