using Unity.Netcode;
using UnityEngine;

public class NetworkAnimatorSync : NetworkBehaviour
{
    [SerializeField] Animator animator;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        ActionEvent.onAnimatorMove += AnimateSetBoolRpc;
        ActionEvent.onAnimatorDodge += AnimateCrossFadeRpc;
        ActionEvent.onAnimatorMelee += AnimateCrossFadeRpc;
    }

    public override void OnDestroy()
    {
        if (!IsOwner) return;

        ActionEvent.onAnimatorMove -= AnimateSetBoolRpc;
        ActionEvent.onAnimatorDodge -= AnimateCrossFadeRpc;
        ActionEvent.onAnimatorMelee -= AnimateCrossFadeRpc;
    }

    [Rpc(SendTo.Everyone)]
    void AnimateSetBoolRpc(string name, bool condition)
    {
        animator.SetBool(name, condition);
    }

    [Rpc(SendTo.Everyone)]
    void AnimateCrossFadeRpc(string name, float transition)
    {
        animator.CrossFade(name, transition);
    }

    [Rpc(SendTo.Everyone)]
    void AnimatePlayRpc(string name)
    {
        animator.Play(name);
    }
}
