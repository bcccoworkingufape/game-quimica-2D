using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Data;
using Domain;
using Presentation.Common;

namespace Presentation.Lab
{
    /// <summary>
    /// Sub-view POCO para a HUD do laboratorio. Recebe referencias \u00e0s
    /// fields serializados de LabUIController e cuida das operações de render.
    /// Não e MonoBehaviour: nada e wired no scene/prefab.
    /// </summary>
    public class LabHudView
    {
        private readonly TextMeshProUGUI _difficultyText;
        private readonly TextMeshProUGUI _livesText;
        private readonly TextMeshProUGUI _modeText;
        private readonly TextMeshProUGUI _percentageText;

        private readonly GameObject[] _heartIcons;
        private readonly GameObject[] _starIcons;

        private readonly Button _treeButton;
        private readonly Graphic[] _treeGraphicsToTint;
        private readonly Color _treeEnabled;
        private readonly Color _treeDisabled;
        private readonly GameObject _treePanel;

        public LabHudView(
            TextMeshProUGUI difficultyText,
            TextMeshProUGUI livesText,
            TextMeshProUGUI modeText,
            TextMeshProUGUI percentageText,
            GameObject[] heartIcons,
            GameObject[] starIcons,
            Button treeButton,
            Graphic[] treeGraphicsToTint,
            Color treeEnabled,
            Color treeDisabled,
            GameObject treePanel)
        {
            _difficultyText = difficultyText;
            _livesText = livesText;
            _modeText = modeText;
            _percentageText = percentageText;
            _heartIcons = heartIcons;
            _starIcons = starIcons;
            _treeButton = treeButton;
            _treeGraphicsToTint = treeGraphicsToTint;
            _treeEnabled = treeEnabled;
            _treeDisabled = treeDisabled;
            _treePanel = treePanel;
        }

        public void RenderDifficulty(DifficultyLevelData data, string modeLabel)
        {
            if (data == null) return;

            if (_difficultyText != null)
                _difficultyText.text = data.difficultyName;
            if (_modeText != null)
                _modeText.text = modeLabel;
        }

        public void RenderLives(int lives, GameMode mode)
        {
            var rules = GameModeRulesFactory.For(mode);

            if (_livesText != null)
                _livesText.text = rules.LivesOverrideText ?? $"Vidas: {lives}";

            const int slots = 3;
            if (_heartIcons != null && _heartIcons.Length > 0 && _heartIcons[0] != null)
            {
                var parent = _heartIcons[0].transform.parent as RectTransform;
                float width = parent != null ? parent.rect.width : 310f;
                if (width <= 0) width = 310f;
                for (int i = 0; i < _heartIcons.Length; i++)
                {
                    if (_heartIcons[i] == null) continue;
                    var rect = _heartIcons[i].transform as RectTransform;
                    if (rect == null) continue;
                    rect.anchoredPosition = new Vector2((i + .5f) * width / slots, -50);
                    float iconWidth = Mathf.Min(99, width / slots - 6);
                    rect.sizeDelta = new Vector2(iconWidth, iconWidth * 91f / 99f);
                }
            }
            // No modo de estudo, os corações continuam visíveis como parte da HUD,
            // mas não representam tentativas: o texto deixa claro que são ilimitadas.
            int visible = mode == GameMode.Estudo_Livre ? slots : Mathf.Min(lives, slots);
            SetIconsActive(_heartIcons, visible);
        }

        public void RenderProgress(int percentage)
        {
            if (_percentageText != null)
                _percentageText.text = $"{percentage}%";
        }

        public void SetTreeAvailable(bool available)
        {
            if (!available && _treePanel != null)
                OverlayAnimator.HideImmediate(_treePanel);

            if (_treeButton != null)
                _treeButton.interactable = available;

            var tint = available ? _treeEnabled : _treeDisabled;

            if (_treeGraphicsToTint != null && _treeGraphicsToTint.Length > 0)
            {
                foreach (var g in _treeGraphicsToTint)
                    if (g != null) g.color = tint;
            }
            else if (_treeButton != null && _treeButton.targetGraphic != null)
            {
                _treeButton.targetGraphic.color = tint;
            }
        }

        public void ClearStars() => SetIconsActive(_starIcons, 0);

        /// <summary>
        /// Exibe uma única estrela como confirmação visual de uma resposta certa.
        /// Não representa nem altera a classificação final da fase.
        /// </summary>
        public void ShowAnswerSuccessStar() => SetIconsActive(_starIcons, 1);

        public static void SetIconsActive(GameObject[] icons, int count)
        {
            if (icons == null) return;
            for (int i = 0; i < icons.Length; i++)
                if (icons[i] != null)
                    icons[i].SetActive(i < count);
        }
    }
}

