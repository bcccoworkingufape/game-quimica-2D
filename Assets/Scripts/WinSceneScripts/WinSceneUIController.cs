using Core;
using UnityEngine;

public class WinSceneUI_Controller : MonoBehaviour
{
    public void LoadScene(string nomeDaCena)
    {
        if (string.IsNullOrWhiteSpace(nomeDaCena))
        {
            Debug.LogWarning("[WinSceneUI_Controller] Nome da cena inválido.");
            return;
        }

        if (GameManager.Instance == null)
        {
            Debug.LogError("[WinSceneUI_Controller] GameManager.Instance não está disponível.");
            return;
        }

        GameManager.Instance.LoadScene(nomeDaCena);
    }
}
