using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

[System.Serializable]
public class TimelinePlayableAssetLoop : PlayableAsset
{
    public ExposedReference<PlayableDirector> director;
    public float loopStartTime;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
        TimelinePlayableBehaviour behaviour = new TimelinePlayableBehaviour();
        behaviour.director = director.Resolve(graph.GetResolver());
        behaviour.loopStartTime = loopStartTime;

        return ScriptPlayable<TimelinePlayableBehaviour>.Create(graph, behaviour);
    }


    //// Factory method that generates a playable based on this asset
    //public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
    //{
    //    TimelinePlayableBehaviour behaviour = new TimelinePlayableBehaviour();
    //    behaviour.director = director.Resolve(graph.GetResolver());
    //    behaviour.loopStartTime = loopStartTime;

    //    return ScriptPlayable<TimelinePlayableBehaviour>.Create(graph, behaviour);
    //}
}
