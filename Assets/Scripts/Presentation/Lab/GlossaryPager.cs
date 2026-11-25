using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.Lab
{
    /// <summary>
    /// Navegação por subpáginas (anterior / próxima) dentro de uma aba do glossário.
    /// </summary>
    public sealed class GlossaryPager : MonoBehaviour
    {
        [SerializeField] private GameObject[] pages;
        [SerializeField] private Button previousButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private TMP_Text indicator;
        [SerializeField] private ScrollRect[] scrollsToReset;

        private int _index = -1;

        private void Awake()
        {
            if (previousButton != null) previousButton.onClick.AddListener(() => Show(_index - 1));
            if (nextButton != null) nextButton.onClick.AddListener(() => Show(_index + 1));
            Show(0);
        }

        public void Show(int index)
        {
            if (pages == null || pages.Length == 0) return;
            index = Mathf.Clamp(index, 0, pages.Length - 1);
            if (index == _index) return;
            _index = index;

            for (int i = 0; i < pages.Length; i++)
                if (pages[i] != null) pages[i].SetActive(i == index);

            if (previousButton != null) previousButton.interactable = index > 0;
            if (nextButton != null) nextButton.interactable = index < pages.Length - 1;
            if (indicator != null) indicator.text = $"{index + 1} / {pages.Length}";
        }
    }
}
