using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace DigitalLove.Game.SpaceShaping.Grid
{
    // The field the lattice draws: it bends the rest nodes and says how visible each warped node is.
    // Both run as jobs so the owner can keep the work on Burst.
    public interface ILatticeField
    {
        bool IsReady { get; }

        JobHandle ScheduleWarp(NativeArray<Vector3> rest, NativeArray<Vector3> warped, JobHandle dependsOn);

        JobHandle ScheduleFade(NativeArray<Vector3> warped, NativeArray<float> alpha, JobHandle dependsOn);
    }
}
