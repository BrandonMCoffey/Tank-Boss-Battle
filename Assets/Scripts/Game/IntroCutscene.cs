using System.Collections.Generic;
using System.Linq;
using Audio;
using Level_Systems;
using Mechanics.Boss;
using Mechanics.Player_Systems;
using UnityEngine;
using UnityEngine.UI;
using Utility.GameEvents.Logic;

namespace Game
{
    public enum IntroState
    {
        FadeIn,
        PlayerMoveIn,
        EnableLights,
        BossEnter,
        Pause,
        DimLights,
        StartGame
    }

    public class IntroCutscene : MonoBehaviour
    {
        [Header("Time of Events")]
        [SerializeField] private float _fadeIn = 1;
        [SerializeField] private float _playerEnter = 1;
        [SerializeField] private float _brightenLights = 1;
        [SerializeField] private float _pauseTime = 1;
        [SerializeField] private float _dimLights = 1;
        [Header("Temporary Separate Player Tank Art")]
        [SerializeField] private Transform _tempPlayerArt = null;
        [SerializeField] private Vector3 _artStartPos = Vector3.zero;
        [SerializeField] private Vector3 _artEndPos = Vector3.zero;
        [Header("Lights")]
        [SerializeField] private Light _directionalLight = null;
        [SerializeField] private float _directionalMin = 0.25f;
        [SerializeField] private float _directionalMax = 1;
        [SerializeField] private List<RedLight> _redLights = new List<RedLight>();
        [SerializeField] private List<RedLightHelper> _backingRedLights = new List<RedLightHelper>();
        [Header("References")]
        [SerializeField] private PlayerTank _player;
        [SerializeField] private BossStateMachine _boss;
        [SerializeField] private GameObject _skipCutsceneButton = null;
        [SerializeField] private BossTurret _turret = null;
        [SerializeField] private BossPlatform _bossSpawnPlatform = null;
        [SerializeField] private Image _fadeInPanel = null;
        [SerializeField] private SfxReference _tankSfx = new SfxReference();
        [SerializeField] private SfxReference _sirenSfx = new SfxReference();
        [SerializeField] private GameEvent _startGame = null;

        private IntroState _state = IntroState.FadeIn;
        private float _bossEnterTime;
        private float _timer;
        private bool _finished;
        private bool _hasError;
        private bool _bossIsRising;
        private bool _notStarted = true;

        private AudioSourceController _tankSfxController;
        private AudioSourceController _sirenController;

        private void Start()
        {
            if (_player == null) {
                _player = FindObjectOfType<PlayerTank>();
                if (_player == null) {
                    _hasError = true;
                    throw new MissingComponentException("No assigned Player Tank on " + gameObject);
                }
            }
            if (_boss == null) {
                _boss = FindObjectOfType<BossStateMachine>();
                if (_boss == null) {
                    _hasError = true;
                    throw new MissingComponentException("No assigned Boss Tank on " + gameObject);
                }
            }
            if (_bossSpawnPlatform != null) {
                _bossEnterTime = _bossSpawnPlatform.TotalTime;
            } else {
                _hasError = true;
                throw new MissingComponentException("No assigned Boss Tank Spawn Platform on " + gameObject);
            }
            _boss.SetVisuals(false);
            _player.gameObject.SetActive(false);
            _tempPlayerArt.gameObject.SetActive(true);
            _fadeInPanel.gameObject.SetActive(true);
            _directionalLight.intensity = _directionalMin;
            foreach (var redLight in _redLights.Where(redLight => redLight != null)) {
                redLight.gameObject.SetActive(true);
                redLight.SetDelta(0);
            }
            foreach (var backRedLight in _backingRedLights.Where(backRedLight => backRedLight != null)) {
                backRedLight.gameObject.SetActive(true);
                backRedLight.SetIntensityDelta(0);
            }
        }

        public void SkipCutscene()
        {
            _directionalLight.intensity = _directionalMax;
            foreach (var redLight in _redLights.Where(redLight => redLight != null)) {
                redLight.gameObject.SetActive(false);
            }
            foreach (var backRedLight in _backingRedLights.Where(backRedLight => backRedLight != null)) {
                backRedLight.gameObject.SetActive(true);
                backRedLight.SetIntensityDelta(1);
            }
            if (!_bossIsRising) {
                _boss.SetVisuals(true);
            }
            _player.transform.position = _artEndPos;
            _player.gameObject.SetActive(true);
            _tempPlayerArt.gameObject.SetActive(false);
            _turret.SetLockTurret(false);
            _finished = true;
            _startGame.Invoke();
            _tankSfxController.SetCustomVolume(0);
            _tankSfxController.Stop();
            _sirenController.Stop();
        }

        public void StartCutscene()
        {
            _timer = 0;
            _tankSfxController = AudioManager.Instance.GetController();
            _sirenController = AudioManager.Instance.GetController();
            _notStarted = false;
        }

        private void Update()
        {
            if (_notStarted || _hasError || _finished) return;

            _timer += Time.deltaTime;

            switch (_state) {
                case IntroState.FadeIn:
                    Color col = _fadeInPanel.color;
                    col.a = 0.5f - 0.5f * _timer / _fadeIn;
                    _fadeInPanel.color = col;
                    if (_timer > _fadeIn) {
                        _fadeInPanel.gameObject.SetActive(false);
                        _state = IntroState.PlayerMoveIn;
                        _timer = 0;
                        _tankSfx.Play(_tankSfxController);
                    }
                    break;
                case IntroState.PlayerMoveIn:
                    _tempPlayerArt.position = Vector3.Slerp(_artStartPos, _artEndPos, _timer / _playerEnter);
                    if (_timer > _playerEnter) {
                        _tempPlayerArt.position = _artEndPos;
                        _state = IntroState.EnableLights;
                        _timer = 0;
                        _sirenSfx.Play(_sirenController);
                        _sirenController.SetCustomVolume(0);
                        _tankSfxController.SetCustomVolume(0);
                    }
                    break;
                case IntroState.EnableLights:
                    foreach (var redLight in _redLights.Where(redLight => redLight != null)) {
                        redLight.SetDelta(_timer / _brightenLights);
                    }

                    _sirenController.SetCustomVolume(_timer / _brightenLights);
                    if (_timer > _playerEnter) {
                        foreach (var redLight in _redLights.Where(redLight => redLight != null)) {
                            redLight.SetDelta(1);
                        }
                        _state = IntroState.BossEnter;
                        _bossSpawnPlatform.PrepareToRaise(_boss);
                        _bossIsRising = true;
                        _timer = 0;
                        _tankSfxController.SetCustomVolume(1);
                    }
                    break;
                case IntroState.BossEnter:
                    if (_timer > _bossEnterTime) {
                        _state = IntroState.Pause;
                        _timer = 0;
                    }
                    break;
                case IntroState.Pause:
                    if (_timer > _pauseTime) {
                        _state = IntroState.DimLights;
                        _timer = 0;
                        _tankSfxController.Stop();
                    }
                    break;
                case IntroState.DimLights:
                    float delta = _timer / _brightenLights;
                    foreach (var redLight in _redLights.Where(redLight => redLight != null)) {
                        redLight.SetDelta(1 - delta);
                    }
                    foreach (var backRedLight in _backingRedLights.Where(backRedLight => backRedLight != null)) {
                        backRedLight.SetIntensityDelta(delta);
                    }
                    _directionalLight.intensity = _directionalMax - (_directionalMax - _directionalMin) * (1 - delta);
                    _sirenController.SetCustomVolume(1 - delta);
                    if (_timer > _dimLights) {
                        foreach (var redLight in _redLights.Where(redLight => redLight != null)) {
                            redLight.gameObject.SetActive(false);
                        }
                        foreach (var backRedLight in _backingRedLights.Where(backRedLight => backRedLight != null)) {
                            backRedLight.SetIntensityDelta(1);
                        }
                        _state = IntroState.StartGame;
                        _timer = 0;
                        _sirenController.Stop();
                    }
                    break;
                case IntroState.StartGame:
                    _player.transform.position = _artEndPos;
                    _player.gameObject.SetActive(true);
                    _tempPlayerArt.gameObject.SetActive(false);
                    _turret.SetLockTurret(false);
                    _finished = true;
                    if (_skipCutsceneButton != null) _skipCutsceneButton.SetActive(false);
                    _startGame.Invoke();
                    break;
            }
        }
    }
}