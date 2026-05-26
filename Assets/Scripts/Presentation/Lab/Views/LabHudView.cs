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

            int visible = rules.VisibleLifeIcons(lives, _heartIcons != null ? _heartIcons.Length : 0);
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

        public void RefreshStars(int lives, GameMode mode)
        {
            int max = _starIcons != null ? _starIcons.Length : 0;
            int visible = GameModeRulesFactory.For(mode).VisibleLifeIcons(lives, max);
            SetIconsActive(_starIcons, visible);
        }

        public void ClearStars() => SetIconsActive(_starIcons, 0);

        public static void SetIconsActive(GameObject[] icons, int count)
        {
            if (icons == null) return;
            for (int i = 0; i < icons.Length; i++)
                if (icons[i] != null)
                    icons[i].SetActive(i < count);
        }
    }
}
