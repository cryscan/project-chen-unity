using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using Unity.Kinematica;
using Unity.SnapshotDebugger;

[RequireComponent(typeof(MovementController))]
public class AbilityRunner : Kinematica
{
    IAbility[] abilities;
    IAbility currentAbility;

    MovementController controller;

    new void Awake()
    {
        abilities = GetComponents<IAbility>();
        controller = GetComponent<MovementController>();

        base.Awake();
    }

    new void Update()
    {
        if (currentAbility != null) currentAbility.OnUpdate(_deltaTime);
        else
        {
            foreach (var ability in abilities)
            {
                var result = ability.OnUpdate(_deltaTime);
                if (result != null)
                {
                    currentAbility = result;
                    break;
                }
            }
        }

        base.Update();
    }
    public override void OnAnimatorMove()
    {
        ref var synthesizer = ref Synthesizer.Ref;

        if (currentAbility is IAbilityAnimatorMove abilityAnimatorMove)
        {
            abilityAnimatorMove.OnAbilityAnimatorMove();
        }

        var controllerPosition = controller.Position;
        var desiredLinearDisplacement = synthesizer.WorldRootTransform.t - controllerPosition;

        controller.Move(desiredLinearDisplacement);
        controller.Tick(Debugger.instance.deltaTime);

        var worldRootTransform = AffineTransform.Create(controller.Position, synthesizer.WorldRootTransform.q);

        synthesizer.SetWorldTransform(worldRootTransform, true);

        transform.position = worldRootTransform.t;
        transform.rotation = worldRootTransform.q;
    }
}
