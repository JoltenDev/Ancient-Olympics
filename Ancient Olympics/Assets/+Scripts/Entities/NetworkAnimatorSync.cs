using Unity.Netcode;
using UnityEngine;

public class NetworkAnimatorSync : NetworkBehaviour
{
    [SerializeField] Animator animator;

    /// <summary>
    /// Sets bool of a given name in the player's animator
    /// </summary>
    /// <param name="name"> Name of boolean to adjust </param>
    /// <param name="condition"> Boolean value to set </param>
    [Rpc(SendTo.Everyone)]
    public void AnimateSetBoolRpc(string name, bool condition)
    {
        animator.SetBool(name, condition);
    }

    /// <summary>
    /// Crossfades into the animation of the given name over a duration
    /// </summary>
    /// <param name="name"> Name of the animation to crossfade into </param>
    /// <param name="transition"> Length of duration for fade </param>
    [Rpc(SendTo.Everyone)]
    public void AnimateCrossFadeRpc(string name, float transition)
    {
        animator.CrossFade(name, transition);
    }

    /// <summary>
    /// Plays the animation of the given name
    /// </summary>
    /// <param name="name"> Name of the animation to be played </param>
    [Rpc(SendTo.Everyone)]
    public void AnimatePlayRpc(string name)
    {
        animator.Play(name);
    }
}
