using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : Singleton<CameraManager>
{
    [SerializeField] CinemachineImpulseSource _impulseSource;

    public void Shake(float strength = 1f, int count = 1, float interval = 0.05f)
    {
        ShakeAsync(strength, count, interval).Forget();
    }
    async UniTaskVoid ShakeAsync(float strength, int count, float interval)
    {
        for (int i = 0; i < count; i++)
        {
            Vector2 direction = Random.insideUnitCircle.normalized;
            Vector3 velocity = direction * strength;

            _impulseSource.GenerateImpulseWithVelocity(velocity);

            if (i < count - 1)
            {
                await UniTask.Delay(
                    System.TimeSpan.FromSeconds(interval),
                    DelayType.DeltaTime,
                    cancellationToken: destroyCancellationToken
                );
            }
        }
    }
}
