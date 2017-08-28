using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SocialPlatforms;
using UnityEngine.SocialPlatforms.GameCenter;

public class GameCenterMgr : MgrBase
{
    public static GameCenterMgr It;
    void Awake() { It = this; }

    // 排行榜
    public static readonly string IOS_APP_ID = "1132168350";
    public static readonly string[] RANKS = {"Score"};

    private Dictionary<string, long> m_RankData;

    public override void Init()
    {
        m_RankData = new Dictionary<string, long>();
        Social.localUser.Authenticate(OnAuthenticated);
    }

    public override void UnInit()
    {

    }

    void OnAuthenticated(bool success)
    {
        Debug.Log("[GC]OnAuthenticated: success = " + success);
        if (!success) return;
        Debug.Log(string.Format("[GameCenter]Name:{0},ID:{1}",
            Social.localUser.userName, Social.localUser.id));
        Social.localUser.LoadFriends(OnFriendsLoaded);
        Social.LoadAchievements(OnAchievementsLoaded);
        for (int i = 0; i < RANKS.Length; i++)
        {
            Social.LoadScores(RANKS[i], HandleRankLoaded);
        }
    }

    void HandleRankLoaded(IScore[] scores)
    {
        for (int i = 0; i < scores.Length; i++)
        {
            m_RankData[scores[i].leaderboardID] = scores[i].value;
        }
        Globals.It.DispatchEvent("RankLoaded");
    }

    public void Restore()
    {
    }


    // 加载好友数据
    void OnFriendsLoaded(bool success)
    {
        if (!success) return;
        foreach (IUserProfile friend in Social.localUser.friends)
        {
            Debug.Log(string.Format("[GC]Friend, ID:{0}, State:{1}", friend.id, friend.state));
        }
        Globals.It.DispatchEvent("FriendsLoaded");
    }

    // 加载成就数据
    private void OnAchievementsLoaded(IAchievement[] achievements)
    {
        /*foreach (IAchievement achievement in achievements)
        {
            Debug.Log("* achievement = " + achievement.ToString());
        }*/
        Globals.It.DispatchEvent("AchieveLoaded");
    }

    // 更新成就进度
    public void UpdateAchievement(string achievementId, double progress)
    {
        if (!Social.localUser.authenticated) return;
        Social.ReportProgress(achievementId, progress, null);
    }

    // 显示成就
    public bool ShowAchievements()
    {
        if (!Social.localUser.authenticated) return false;
        Social.ShowAchievementsUI();
        return true;
    }

    // 更新积分
    public bool UpdateRank(string name, long score)
    {
        if (!Social.localUser.authenticated) return false;
        m_RankData[name] = score;
        Social.ReportScore(score, name, null);
        Debug.Log("ReportScore:" + name + "," + score);
        return true;
    }

    // 显示主界面
    public bool ShowLeaderboard()
    {
        if (!Social.localUser.authenticated) return false;
        Social.ShowLeaderboardUI();
        return true;
    }

    // 跳转到AppStore
    public void GotoAppStore()
    {
        var url = string.Format(
            "itms-apps://ax.itunes.apple.com/WebObjects/MZStore.woa/wa/viewContentsUserReviews?type=Purple+Software&id={0}",
            IOS_APP_ID);
        Application.OpenURL(url);
    }
}
