using UnityEngine;
using System.Collections.Generic;

public enum EnGameState
{
    Ready = 1,
    Start,
    Pause,
    Over,
}

public class GameMgr : MgrBase
{
    public static GameMgr It;
    void Awake() { It = this; }

    public EnGameState State;

    public override void Init()
    {
        base.Init();

        GameReady();
    }

    public override void UnInit()
    {
        base.UnInit();
    }

    #region 游戏流程控制
    public void GameReady()
    {
        State = EnGameState.Ready;
        UIMgr.It.OpenUI("UIHome");
    }

    public void GameStart()
    {
        State = EnGameState.Start;
    }

    public void GamePause()
    {
        State = EnGameState.Pause;
    }

    public void GameResume()
    {
        State = EnGameState.Start;
    }

    public void GameOver()
    {
        State = EnGameState.Over;
    }


    #endregion
}
