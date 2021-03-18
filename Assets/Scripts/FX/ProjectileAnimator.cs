using System.Collections;
using UnityEngine;

public class ProjectileAnimator : MonoBehaviour
{
    private static ProjectileAnimator _instance;
    public static ProjectileAnimator Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ProjectileAnimator>();
            }

            return _instance;
        }
    }

    [SerializeField]
    private Transform projectile;

    private void Awake()
    {
        projectile.gameObject.SetActive(false);
    }

    public IEnumerator Play(Vector3Int cellStart, Vector3Int cellEnd, AttackTrajectory trajectory)
    {
        TilemapNavigator navigator = TilemapNavigator.Instance;
        Vector3 startWorld = navigator.CellToWorldPos(cellStart);
        Vector3 endWorld = navigator.CellToWorldPos(cellEnd);
        Vector3 midPoint = trajectory == AttackTrajectory.Straight ?
            GetMiddlePoint(startWorld, endWorld) : GetArcMiddlePoint(startWorld, endWorld);

        float movementSpeed = trajectory == AttackTrajectory.Straight ? 20f : 6f;
        float distance = Vector3.Distance(startWorld, endWorld);

        float duration = distance * 1 / movementSpeed;
        float elapsedTime = 0f;

        projectile.gameObject.SetActive(true);
        while (elapsedTime < duration)
        {
            float currentState = elapsedTime / duration;
            Vector3 m1 = Vector3.Lerp(startWorld, midPoint, currentState);
            Vector3 m2 = Vector3.Lerp(midPoint, endWorld, currentState);

            projectile.position = Vector3.Lerp(m1, m2, currentState);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        projectile.gameObject.SetActive(false);

    }

    private Vector3 GetMiddlePoint(Vector3 start, Vector3 end)
    {
        Vector3 directionFromStart = end - start;
        return start + directionFromStart * .5f;
    }

    private Vector3 GetArcMiddlePoint(Vector3 start, Vector3 end)
    {
        Vector3 middlePoint = GetMiddlePoint(start, end);
        float distance = Vector3.Distance(start, end);
        return middlePoint + Vector3.up * distance / 2 ;
    }
}
