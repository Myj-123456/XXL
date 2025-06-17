using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

// A behaviour that is attached to a playable
public class TimelinePlayableBehaviour : PlayableBehaviour
{
    public PlayableDirector director;
    public float loopStartTime;

    // 当拥有的图形开始播放时调用
    public override void OnGraphStart(Playable playable)
    {
        
    }

    // 当拥有的图形停止播放时调用
    public override void OnGraphStop(Playable playable)
    {
        
    }

    // 当可播放对象的状态设置为播放时调用
    public override void OnBehaviourPlay(Playable playable, FrameData info)
    {
        director.time = loopStartTime;
    }

    // 当播放状态设置为暂停时调用
    public override void OnBehaviourPause(Playable playable, FrameData info)
    {
        
    }

    // 当状态设置为播放时，调用每一帧
    public override void PrepareFrame(Playable playable, FrameData info)
    {

    }
}
