using UnityEngine;
using DG.Tweening;

public class EnemyPatrol : MonoBehaviour
{
    private void Start()
    {
        transform.DOMoveX(transform.position.x + 6f, 3f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);
    }
}

