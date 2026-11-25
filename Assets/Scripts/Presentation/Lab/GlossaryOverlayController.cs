using System;
using Core.Audio;
using Presentation.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Presentation.Lab
{
    /// <summary>
    /// Controla o overlay do glossário (livro com abas).
    /// Fica no botão que abre o glossário (sempre ativo na cena); o overlay em si
    /// começa desativado e é aberto/fechado via <see cref="OverlayAnimator"/>.
    /// </summary>
    public sealed class GlossaryOverlayController : MonoBehaviour
    {
        [Serializable]
        public sealed class Tab
        {
            public Button button;
            public Image background;
            public TMP_Text label;
            [Tooltip("Conteúdo exibido quando a aba está ativa (opcional).")]
            public GameObject content;
        }

        [Header("Abrir / Fechar")]
        [SerializeField] private Button openButton;
        [SerializeField] private GameObject overlay;
        [Tooltip("Botões que fecham o overlay (fundo escuro, botão X...).")]
        [SerializeField] private Button[] closeButtons;

        [Header("Abas")]
        [SerializeField] private Tab[] tabs;
        [SerializeField] private Sprite activeTabSprite;
        [SerializeField] private Sprite inactiveTabSprite;
        [SerializeField] private Color activeLabelColor = Color.white;
        [SerializeField] private Color inactiveLabelColor = new Color(0.36f, 0.22f, 0.10f);
        [SerializeField] private int defaultTabIndex;

        private int _currentTab = -1;

        public bool IsOpen => overlay != null && overlay.activeSelf;

        private void Awake()
        {
            if (openButton == null) openButton = GetComponent<Button>();
            if (openButton != null) openButton.onClick.AddListener(Open);

            if (closeButtons != null)
            {
                foreach (var b in closeButtons)
                    if (b != null) b.onClick.AddListener(Close);
            }

            if (tabs != null)
            {
                for (int i = 0; i < tabs.Length; i++)
                {
                    int index = i; // captura por iteração
                    if (tabs[i].button != null)
                        tabs[i].button.onClick.AddListener(() => SelectTab(index, true));
                }
            }

            if (overlay != null) OverlayAnimator.HideImmediate(overlay);
        }

        public void Open()
        {
            if (overlay == null || IsOpen) return;

            SfxManager.Instance?.PlayButtonClick();
            SelectTab(defaultTabIndex, false);
            OverlayAnimator.Show(overlay);
        }

        public void Close()
        {
            if (!IsOpen) return;

            SfxManager.Instance?.PlayButtonClick();
            OverlayAnimator.Hide(overlay);
        }

        public void SelectTab(int index, bool playSfx)
        {
            if (tabs == null || index < 0 || index >= tabs.Length || index == _currentTab) return;

            if (playSfx) SfxManager.Instance?.PlayButtonClick();
            _currentTab = index;

            for (int i = 0; i < tabs.Length; i++)
            {
                bool active = i == index;
                var tab = tabs[i];

                if (tab.background != null)
                {
                    var sprite = active ? activeTabSprite : inactiveTabSprite;
                    if (sprite != null) tab.background.sprite = sprite;
                }

                if (tab.label != null)
                    tab.label.color = active ? activeLabelColor : inactiveLabelColor;

                if (tab.content != null)
                    tab.content.SetActive(active);
            }
        }
    }
}
