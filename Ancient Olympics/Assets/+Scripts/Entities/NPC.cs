using System;
using Unity.Netcode;
using UnityEngine;

public class NPC : NetworkEntity
{
    [Header("NPC Fields")]
    [SerializeField] NetworkAnimatorSync networkAnimatorSync;

    Timer cooldown = new Timer();

    #region Actions
    Action onWanderStarted;
    Action onWanderCompleted;
    Action onIdleStarted;
    Action onIdleCompleted;

    Action<string, bool> onAnimatorSetBool;
    Action<string, float> onAnimatorCrossFade;
    #endregion

    Vector3 target;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        cooldown.RegisterTimer(cooldown.timerStartedEvents, "Wander", () => onWanderStarted?.Invoke());
        cooldown.RegisterTimer(cooldown.timerCompletedEvents, "Wander", () => onWanderCompleted?.Invoke());
        cooldown.RegisterTimer(cooldown.timerStartedEvents, "Idle", () => onIdleStarted?.Invoke());
        cooldown.RegisterTimer(cooldown.timerCompletedEvents, "Idle", () => onIdleCompleted?.Invoke());

        onWanderCompleted += Idle; // Done wandering, idle
        onIdleCompleted += Wander; // Done waiting, wander

        onAnimatorSetBool += networkAnimatorSync.AnimateSetBoolRpc;
        onAnimatorCrossFade += networkAnimatorSync.AnimateCrossFadeRpc;

        Idle();
    }

    public override void OnDestroy()
    {
        onWanderCompleted -= Idle;
        onIdleCompleted -= Wander;

        onAnimatorSetBool -= networkAnimatorSync.AnimateSetBoolRpc;
        onAnimatorCrossFade -= networkAnimatorSync.AnimateCrossFadeRpc;
    }

    void FixedUpdate()
    {
        float distance = Vector3.Distance(transform.position, target);
        float stoppingDistance = 0.5f;
        Vector3 direction = (target - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            SendMove(direction, transform.position);
            SendRotation(direction);
        }

        if (distance < stoppingDistance)
            onAnimatorSetBool?.Invoke("IsMoving", false);
        else
            onAnimatorSetBool?.Invoke("IsMoving", true);
    }

    void Idle()
    {
        cooldown.StartTimer("Idle", StateLength());
    }

    void Wander()
    {
        target = FindNewPosition();
        cooldown.StartTimer("Wander", StateLength());
    }

    Vector3 FindNewPosition()
    {
        float x = UnityEngine.Random.Range(-3f, 3.25f);
        float z = UnityEngine.Random.Range(-1.5f, 1.5f);
        return new Vector3(x, 0.4f, z);
    }

    float StateLength() => UnityEngine.Random.Range(0f, 5f);
}
