using UnityEngine;
using Presentation.Lab;

public class SolventClickHandler : MonoBehaviour
{
    [Header("Configuração do solvente")]
    [SerializeField] private int solventId;
    [SerializeField] private string solventName;

    [Header("Referência")]
    [SerializeField] private TestManager testManager;

    private void Awake()
    {
        if (testManager == null)
        {
            testManager = FindObjectOfType<TestManager>();
        }
    }

    private void OnMouseDown()
    {
        if (testManager == null)
        {
            Debug.LogWarning("[SolventClickHandler] TestManager não atribuído.");
            return;
        }

        testManager.OnSolventClicked(solventId, solventName);
    }
}
