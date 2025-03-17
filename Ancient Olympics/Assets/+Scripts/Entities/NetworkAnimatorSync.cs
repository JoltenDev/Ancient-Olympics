using Unity.Netcode;
using UnityEngine;

public class NetworkAnimatorSync : NetworkBehaviour
{
    [SerializeField] Animator animator;

    public void SyncAnimation(Vector2 moveInput)
    {
        if (IsOwner)
            AnimateMoveRpc(moveInput);
    }

    [Rpc(SendTo.Everyone)]
    private void AnimateMoveRpc(Vector2 moveInput)
    {
        animator.SetBool("IsMoving", moveInput != Vector2.zero);
    }
}
