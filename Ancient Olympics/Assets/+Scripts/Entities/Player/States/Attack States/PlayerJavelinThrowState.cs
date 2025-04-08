using Unity.Netcode;
using UnityEngine;

public class PlayerJavelinThrowState : PlayerBaseState
{
    public PlayerJavelinThrowState(Player player, PlayerStates states) : base(player, states)
    {
    }

    public override void Enter()
    {
        player.WeaponHandler.onSwingStarted?.Invoke();

        player.onAnimatorCrossFade?.Invoke("throw", 0.25f);
        GameManager.Instance.SetJavelinAddendRpc(GameManager.Instance.JavelinSpeedAddend.Value + 5);

        player.CooldownHandler.StartTimer("Attack Duration", .5f);
        player.onAttackDurationCompleted += SwitchState;
    }

    public override void Update()
    {

    }

    public override void FixedUpdate()
    {

    }
    
    public override void Exit()
    {
        if (FindTarget() != 100)
        {
            RequestSpawnJavelinProjectile(FindTarget());
            player.WeaponHandler.UnequipWeaponRpc();
        }

        player.onAttackDurationCompleted -= SwitchState;
        player.WeaponHandler.onSwingCompleted?.Invoke();
    }

    void SwitchState()
    {
        SwitchState(states.Idle());
    }

    void RequestSpawnJavelinProjectile(ulong targetId)
    {
        player.SpawnJavelinProjectileRpc(player.OwnerClientId, targetId, GameManager.Instance.JavelinSpeedAddend.Value, player.transform.position, player.transform.forward);
    }

    ulong FindTarget()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            if (hit.collider.CompareTag("Player"))
            {
                if (!hit.collider.GetComponent<Player>().enabled) return 100;

                return hit.collider.GetComponentInChildren<NetworkObject>().OwnerClientId;
            }
        }

        return 100;
    }
}
