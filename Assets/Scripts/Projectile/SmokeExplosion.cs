using Fusion;
using UnityEngine;

public class SmokeExplosion : NetworkBehaviour, IPredictedSpawnBehaviour {
    // PRIVATE MEMBERS



    [SerializeField]
    private float _despawnDelay = 3f;

    [SerializeField]
    private Transform _effectRoot;


    private TickTimer _despawnTimer;

    // NetworkBehaviour INTERFACE

    public override void Spawned() {
        base.Spawned();



        if (Object.IsPredictedSpawn == false) {
            _despawnTimer = TickTimer.CreateFromSeconds(Runner, _despawnDelay);
        }
    }


    public override void FixedUpdateNetwork() {

        if (HasStateAuthority == false)
            return;
        if (_despawnTimer.Expired(Runner) == false)
            return;

        Runner.Despawn(Object);
    }
    // IPredictedSpawnBehaviour INTERFACE

    void IPredictedSpawnBehaviour.PredictedSpawnSpawned() {
        Spawned();
    }

    void IPredictedSpawnBehaviour.PredictedSpawnUpdate() {
        FixedUpdateNetwork();
    }

    void IPredictedSpawnBehaviour.PredictedSpawnRender() {
        
    }

    void IPredictedSpawnBehaviour.PredictedSpawnFailed() {
        Runner.Despawn(Object, true);
    }

    void IPredictedSpawnBehaviour.PredictedSpawnSuccess() {
        // Nothing special is needed
    }
}

