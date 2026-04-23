using UnityEngine;
using Presentation.Common;

// add GameObject da cena inicial/menu
public class OverlayAnimatorBootstrap : MonoBehaviour
{
    private static bool _bootstrapped;

    private void Awake()
    {
        if (_bootstrapped)
        {
            Destroy(gameObject);
            return;
        }

        _bootstrapped = true;
        DontDestroyOnLoad(gameObject);

        OverlayAnimator.Warmup();
        Debug.Log("[OverlayAnimatorBootstrap] LeanTween warmed up.");
    }
}