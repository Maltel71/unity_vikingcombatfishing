using UnityEngine;
using Steamworks;
using System;
using System.Collections.Generic;

/// <summary>
/// Kopplingen mot Steams topplista.
///
/// Startar sig sjalv vid uppstart och overlever scenbyten. Ar Steam inte igang,
/// eller kors spelet utanfor Steam, gor den ingenting alls - da anvander menyn
/// den lokala listan i Highscores istallet. Spelet ska aldrig krascha for att
/// Steam saknas.
///
/// Poangen som skickas upp ar PlayerScript.TotalScore (fiskepoang + blood money).
/// Namnet pa Steam-listan ar spelarens Steam-namn, inte det man skriver in i
/// namnrutan - det galler bara den lokala listan.
/// </summary>
public class SteamLeaderboards : MonoBehaviour
{
    public const string LeaderboardName = "Highscore";
    const uint GameAppId = 5207630;

    public static SteamLeaderboards Instance { get; private set; }

    /// <summary>Sant nar Steam ar igang OCH topplistan ar hittad.</summary>
    public static bool IsReady
    {
        get { return Instance != null && Instance.initialized && Instance.leaderboardFound; }
    }

    /// <summary>Sant nar Steam startade. Topplistan kan fortfarande vara pa vag.</summary>
    public static bool SteamRunning
    {
        get { return Instance != null && Instance.initialized; }
    }

    public struct Entry
    {
        public int rank;
        public string name;
        public int score;
    }

    private bool initialized;
    private bool leaderboardFound;
    private SteamLeaderboard_t leaderboard;

    private CallResult<LeaderboardFindResult_t> findCall;
    private CallResult<LeaderboardScoreUploaded_t> uploadCall;
    private CallResult<LeaderboardScoresDownloaded_t> downloadCall;

    private Action<List<Entry>> pendingDownload;

    // ---------------- Uppstart ----------------

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null) return;

        GameObject go = new GameObject("SteamLeaderboards");
        go.AddComponent<SteamLeaderboards>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

#if !UNITY_EDITOR
        try
        {
            // Startades spelet utan Steam startar vi om det via Steam-klienten
            if (SteamAPI.RestartAppIfNecessary(new AppId_t(GameAppId)))
            {
                Application.Quit();
                return;
            }
        }
        catch (DllNotFoundException e)
        {
            Debug.LogWarning("Steam: steam_api64.dll hittades inte. Topplistan ar av. " + e.Message);
            return;
        }
#endif

        if (!Packsize.Test())
        {
            Debug.LogWarning("Steam: fel plattformspackning - topplistan ar av.");
            return;
        }

        try
        {
            initialized = SteamAPI.Init();
        }
        catch (DllNotFoundException e)
        {
            Debug.LogWarning("Steam: kunde inte ladda biblioteket. Topplistan ar av. " + e.Message);
            return;
        }

        if (!initialized)
        {
            // Helt normalt i editorn utan Steam igang, eller utanfor Steam
            Debug.Log("Steam ar inte igang - spelet anvander den lokala topplistan.");
            return;
        }

        findCall = CallResult<LeaderboardFindResult_t>.Create(OnLeaderboardFound);
        uploadCall = CallResult<LeaderboardScoreUploaded_t>.Create(OnScoreUploaded);
        downloadCall = CallResult<LeaderboardScoresDownloaded_t>.Create(OnScoresDownloaded);

        // Finns listan inte pa Steam-sidan skapas den har - fallande, numerisk
        SteamAPICall_t call = SteamUserStats.FindOrCreateLeaderboard(
            LeaderboardName,
            ELeaderboardSortMethod.k_ELeaderboardSortMethodDescending,
            ELeaderboardDisplayType.k_ELeaderboardDisplayTypeNumeric);

        findCall.Set(call);
    }

    void Update()
    {
        if (initialized)
        {
            SteamAPI.RunCallbacks();
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
            if (initialized)
            {
                SteamAPI.Shutdown();
                initialized = false;
            }
        }
    }

    void OnApplicationQuit()
    {
        if (initialized)
        {
            SteamAPI.Shutdown();
            initialized = false;
        }
    }

    // ---------------- Publika anrop ----------------

    /// <summary>Skickar upp resultatet. Steam behaller bara spelarens basta.</summary>
    public static void UploadScore(int score)
    {
        if (!IsReady || score <= 0) return;

        SteamAPICall_t call = SteamUserStats.UploadLeaderboardScore(
            Instance.leaderboard,
            ELeaderboardUploadScoreMethod.k_ELeaderboardUploadScoreMethodKeepBest,
            score, null, 0);

        Instance.uploadCall.Set(call);
    }

    /// <summary>
    /// Hamtar de basta placeringarna. onDone anropas nar svaret kommit.
    /// Returnerar false direkt om Steam inte ar tillgangligt - anvand lokala listan da.
    /// </summary>
    public static bool RequestTopEntries(int count, Action<List<Entry>> onDone)
    {
        if (!IsReady || onDone == null) return false;

        Instance.pendingDownload = onDone;

        SteamAPICall_t call = SteamUserStats.DownloadLeaderboardEntries(
            Instance.leaderboard,
            ELeaderboardDataRequest.k_ELeaderboardDataRequestGlobal,
            1, Mathf.Max(1, count));

        Instance.downloadCall.Set(call);
        return true;
    }

    // ---------------- Svar fran Steam ----------------

    void OnLeaderboardFound(LeaderboardFindResult_t result, bool ioFailure)
    {
        if (ioFailure || result.m_bLeaderboardFound == 0)
        {
            Debug.LogWarning("Steam: hittade inte topplistan \"" + LeaderboardName + "\".");
            return;
        }

        leaderboard = result.m_hSteamLeaderboard;
        leaderboardFound = true;
    }

    void OnScoreUploaded(LeaderboardScoreUploaded_t result, bool ioFailure)
    {
        if (ioFailure || result.m_bSuccess == 0)
        {
            Debug.LogWarning("Steam: kunde inte skicka upp poangen.");
            return;
        }

        if (result.m_bScoreChanged != 0)
        {
            Debug.Log("Steam: nytt personbasta, plats " + result.m_nGlobalRankNew + ".");
        }
    }

    void OnScoresDownloaded(LeaderboardScoresDownloaded_t result, bool ioFailure)
    {
        List<Entry> entries = new List<Entry>();

        if (!ioFailure)
        {
            for (int i = 0; i < result.m_cEntryCount; i++)
            {
                LeaderboardEntry_t raw;
                if (!SteamUserStats.GetDownloadedLeaderboardEntry(
                        result.m_hSteamLeaderboardEntries, i, out raw, null, 0))
                {
                    continue;
                }

                Entry entry = new Entry();
                entry.rank = raw.m_nGlobalRank;
                entry.score = raw.m_nScore;
                entry.name = SteamFriends.GetFriendPersonaName(raw.m_steamIDUser);
                entries.Add(entry);
            }
        }

        Action<List<Entry>> callback = pendingDownload;
        pendingDownload = null;

        if (callback != null)
        {
            callback(entries);
        }
    }
}
