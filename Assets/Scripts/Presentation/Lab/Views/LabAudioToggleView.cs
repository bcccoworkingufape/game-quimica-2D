using UnityEngine;
using Core.Audio;

namespace Presentation.Lab
{
    /// <summary>
    /// Sub-view POCO para os toggles de musica/SFX da HUD do lab.
    /// Encapsula interação com <see cref="IAudioService"/> e atualiza
    /// objetos visuais on/off do scene.
    /// </summary>
    public class LabAudioToggleView
    {
        private readonly GameObject _musicOn;
        private readonly GameObject _musicOff;
        private readonly GameObject _sfxOn;
        private readonly GameObject _sfxOff;
        private readonly IAudioService _audio;

        public LabAudioToggleView(
            GameObject musicOn, GameObject musicOff,
            GameObject sfxOn, GameObject sfxOff,
            IAudioService audio)
        {
            _musicOn = musicOn;
            _musicOff = musicOff;
            _sfxOn = sfxOn;
            _sfxOff = sfxOff;
            _audio = audio;
        }

        public void EnableMusic()
        {
            _audio?.PlayButtonClick();
            _audio?.EnableMusic();
            RefreshMusicVisual();
        }

        public void DisableMusic()
        {
            _audio?.PlayButtonClick();
            _audio?.DisableMusic();
            RefreshMusicVisual();
        }

        public void RefreshMusicVisual()
        {
            if (_audio == null) return;
            bool enabled = _audio.IsMusicEnabled;
            if (_musicOn != null) _musicOn.SetActive(enabled);
            if (_musicOff != null) _musicOff.SetActive(!enabled);
        }

        public void EnableSfx()
        {
            _audio?.EnableSfx();
            RefreshSfxVisual();
        }

        public void DisableSfx()
        {
            _audio?.DisableSfx();
            RefreshSfxVisual();
        }

        public void RefreshSfxVisual()
        {
            if (_audio == null) return;
            bool enabled = _audio.IsSfxEnabled;
            if (_sfxOn != null) _sfxOn.SetActive(enabled);
            if (_sfxOff != null) _sfxOff.SetActive(!enabled);
        }
    }
}
