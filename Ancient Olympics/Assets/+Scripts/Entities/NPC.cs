using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class NPC : NetworkEntity
{
    [Header("NPC Fields")]
    [SerializeField] NetworkAnimatorSync networkAnimatorSync;

    enum States { Idle, Wander };
    States state = States.Idle;

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

        target = FindNewPosition();
        Idle();
    }

    public override void OnDestroy()
    {
        onWanderCompleted -= Idle;
        onIdleCompleted -= Wander;

        onAnimatorSetBool -= networkAnimatorSync.AnimateSetBoolRpc;
        onAnimatorCrossFade -= networkAnimatorSync.AnimateCrossFadeRpc;
    }

    new void FixedUpdate()
    {
        if (!IsServer) return;

        switch (state)
        {
            case States.Idle:
                onAnimatorSetBool?.Invoke("IsMoving", false);
                break;
            case States.Wander:
                float distance = Vector3.Distance(transform.position, target);
                float stoppingDistance = 0.5f;
                Vector3 direction = (target - transform.position).normalized;
                direction.y = 0;

                if (direction != Vector3.zero)
                {
                    MoveRpc(direction);
                }

                if (distance < stoppingDistance)
                    onAnimatorSetBool?.Invoke("IsMoving", false);
                else
                    onAnimatorSetBool?.Invoke("IsMoving", true);
                break;
        }
    }

    void Idle()
    {
        cooldown.StartTimer("Idle", StateLength());
        state = States.Idle;
    }

    void Wander()
    {
        target = FindNewPosition();
        cooldown.StartTimer("Wander", StateLength());
        state = States.Wander;
    }

    Vector3 FindNewPosition()
    {
        float x = UnityEngine.Random.Range(-3f, 3.25f);
        float z = UnityEngine.Random.Range(-1.5f, 1.5f);
        return new Vector3(x, 0.4f, z);
    }

    float StateLength() => UnityEngine.Random.Range(0f, 5f);

    [Rpc(SendTo.Everyone)]
    void MoveRpc(Vector3 direction)
    {
        rigidBody.MovePosition(Vector3.Lerp(rigidBody.position, transform.position + direction * speed * Time.fixedDeltaTime, 5 * Time.fixedDeltaTime));

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        rigidBody.MoveRotation(Quaternion.Slerp(rigidBody.rotation, targetRotation, 0.1f));
    }
}
