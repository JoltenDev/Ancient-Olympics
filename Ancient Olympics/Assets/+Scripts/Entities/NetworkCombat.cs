using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkCombat: NetworkBehaviour
{
    enum CombatStates { Melee1, Melee2, Melee3, Frozen }
    [SerializeField] CombatStates state;
    [SerializeField] CombatStates nextState = CombatStates.Melee1;
    [SerializeField] float attackCooldown;
    [SerializeField] float animationTransition;

    // Player
    Controls controls;

    System.Action<float> frozenStateChangeAction;
    System.Action frozenCompletedAction;
    System.Action<float> startAttackCooldownAction;
    System.Action attackCooldownCompleted;
    System.Action<InputAction.CallbackContext> leftClickAction;

    bool canAttack = true;

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;

        controls = new Controls();

        leftClickAction = ctx => ChangeState(nextState);
        controls.Player.OnLeftClick.performed += leftClickAction;
        controls.Player.OnLeftClick.performed += ExecuteState;

        controls.Enable();

        frozenStateChangeAction = (float time) => ChangeState(CombatStates.Frozen);
        ActionEvent.onFrozenStarted += frozenStateChangeAction;

        frozenCompletedAction = () => { state = nextState; };
        ActionEvent.onFrozenCompleted += frozenCompletedAction;

        startAttackCooldownAction = (float amount) => { canAttack = false; };
        ActionEvent.onStartAttackCooldown += startAttackCooldownAction;

        attackCooldownCompleted = () => { canAttack = true; };
        ActionEvent.onAttackCooldownCompleted += attackCooldownCompleted;
    }

    public override void OnDestroy()
    {
        controls.Player.OnLeftClick.performed -= leftClickAction;
        controls.Player.OnLeftClick.performed -= ExecuteState;

        ActionEvent.onFrozenStarted -= frozenStateChangeAction;
        ActionEvent.onFrozenCompleted -= frozenCompletedAction;

        ActionEvent.onStartAttackCooldown -= startAttackCooldownAction;
        ActionEvent.onAttackCooldownCompleted -= attackCooldownCompleted;
    }

    void ChangeState(CombatStates newState)
    {
        if (state == CombatStates.Frozen)
            return;

        state = newState;
    }

    void ExecuteState(InputAction.CallbackContext ctx)
    {
        if (!canAttack) return;

        switch (state)
        {
            case CombatStates.Frozen:
                break;

            case CombatStates.Melee1:
                ActionEvent.onAnimatorMelee?.Invoke("melee1", animationTransition);
                nextState = CombatStates.Melee2;
                break;

            case CombatStates.Melee2:
                ActionEvent.onAnimatorMelee?.Invoke("melee2", animationTransition);
                nextState = CombatStates.Melee3;
                break;

            case CombatStates.Melee3:
                ActionEvent.onAnimatorMelee?.Invoke("melee3", animationTransition);
                ActionEvent.onStartAttackCooldown?.Invoke(attackCooldown);
                nextState = CombatStates.Melee1;
                break;
        }
    }
}