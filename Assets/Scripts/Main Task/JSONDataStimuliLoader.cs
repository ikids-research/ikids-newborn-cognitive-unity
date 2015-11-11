using UnityEngine;
using System.Collections;
namespace JSONDataLoader
{
    public interface IStimuli
    {
        void showStimuli();
        void removeStimuli();
        bool isStimuliLoaded();
    }

    public class VisualStimuli : IStimuli
    {
        private Texture2D _stimuli;
        private GameObject _renderObject;
        private WWW _loaderObject;
        private SpriteRenderer _renderer;
        public VisualStimuli(string fullFileNameAndPathToImage, Vector2 location, Vector2 size)
        {
            string path = "file:///" + fullFileNameAndPathToImage.Replace('\\', '/');
            _loaderObject = new WWW(path);
            _renderObject = new GameObject();
            _renderObject.transform.position = Camera.main.ScreenToWorldPoint(location);
            _renderObject.transform.position = new Vector3(_renderObject.transform.position.x, _renderObject.transform.position.y, 0f);
            _renderer = _renderObject.AddComponent<SpriteRenderer>();
            while (!_loaderObject.isDone)
                Debug.Log("Loading visual asset " + path + " - " + _loaderObject.progress + "%");
            Debug.Log("Done loading visual asset" + path + ".");
            _stimuli = _loaderObject.texture;
            _renderer.sprite = Sprite.Create(_stimuli, new Rect(0f, 0f, _loaderObject.texture.width, _loaderObject.texture.height), Vector2.zero, 1f);
            _renderObject.transform.localScale = new Vector3(size.x / _loaderObject.texture.width, size.y / _loaderObject.texture.height, 0f);
        }

        public void showStimuli()
        {
            _renderObject.SetActive(true);
        }

        public void removeStimuli()
        {
            _renderObject.SetActive(false);
        }

        public bool isStimuliLoaded()
        {
            return _loaderObject.isDone;
        }
    }

    public class AudioStimuli : IStimuli
    {
        private AudioSource _audio;
        private GameObject _audioObject;
        private WWW _loaderObject;

        public AudioStimuli(string fullFileNameAndPathToAudio, bool loop)
        {
            _audioObject = new GameObject();
            _audio = _audioObject.AddComponent<AudioSource>();
            _audio.loop = loop;
            string path = "file:///" + fullFileNameAndPathToAudio.Replace('\\', '/');
            _loaderObject = new WWW(path);
            while (!_loaderObject.isDone)
                Debug.Log("Loading audio asset " + path + " - " + _loaderObject.progress + "%");
            Debug.Log("Done loading audio asset" + path + ".");
            _audio.clip = _loaderObject.audioClip;
        }

        public void showStimuli()
        {
            _audio.Play();
        }

        public void removeStimuli()
        {
            _audio.Stop();
        }

        public bool isStimuliLoaded()
        {
            return _loaderObject.isDone;
        }
    }

    public class MultiImageAnimationStimuli : IStimuli
    {
        private WWW[] _loaderObjects;
        private GameObject[] _renderObjects;
        private Texture2D[] _stimuli;
        private SpriteRenderer[] _renderers;
        private GameObject _rootObject;
        private int _index;
        private float _currentIndexStartTime;
        private float _timePerImage;
        private bool _loop;
        public MultiImageAnimationStimuli(string[] files, Vector2 location, Vector2 size, float timePerImage, bool loop)
        {
            _rootObject = new GameObject();
            _loaderObjects = new WWW[files.Length];
            _renderObjects = new GameObject[files.Length];
            _stimuli = new Texture2D[files.Length];
            _renderers = new SpriteRenderer[files.Length];
            for (int i = 0; i < files.Length; i++)
            {
                string path = "file:///" + files[i].Replace('\\', '/');
                _loaderObjects[i] = new WWW(path);
                _renderObjects[i] = new GameObject();
                _renderObjects[i].transform.position = Camera.main.ScreenToWorldPoint(location);
                _renderObjects[i].transform.position = new Vector3(_renderObjects[i].transform.position.x, _renderObjects[i].transform.position.y, 0f);
                _renderers[i] = _renderObjects[i].AddComponent<SpriteRenderer>();
                while (!_loaderObjects[i].isDone)
                    Debug.Log("Loading visual asset " + path + " - " + _loaderObjects[i].progress + "%");
                Debug.Log("Done loading visual asset" + path + ".");
                _stimuli[i] = _loaderObjects[i].texture;
                _renderers[i].sprite = Sprite.Create(_stimuli[i], new Rect(0f, 0f, _loaderObjects[i].texture.width, _loaderObjects[i].texture.height), Vector2.zero, 1f);
                _renderObjects[i].transform.localScale = new Vector3(size.x / _loaderObjects[i].texture.width, size.y / _loaderObjects[i].texture.height, 0f);
                _renderObjects[i].transform.parent = _rootObject.transform;
                _renderObjects[i].AddComponent<MultiImageAnimationStimuliBehavior>().Script = this;
                _renderObjects[i].SetActive(false);
            }
            _index = 0;
            _timePerImage = timePerImage;
            _loop = loop;
        }

        public void updateStimuli()
        {
            if (_rootObject.activeSelf)
            {
                if (Time.time - _currentIndexStartTime >= _timePerImage)
                {
                    _renderObjects[_index].SetActive(false);
                    _index++;
                    if (_loop) _index %= _renderObjects.Length;
                    if (_index < _renderObjects.Length)
                    {
                        _renderObjects[_index].SetActive(true);
                        _currentIndexStartTime = Time.time;
                    }
                }
            }
        }

        public void showStimuli()
        {
            _rootObject.SetActive(true);
            _index = 0;
            _renderObjects[_index].SetActive(true);
            _currentIndexStartTime = Time.time;
        }

        public void removeStimuli()
        {
            _rootObject.SetActive(false);
        }

        public bool isStimuliLoaded()
        {
            bool isLoaded = true;
            for (int i = 0; i < _loaderObjects.Length; i++)
                isLoaded &= _loaderObjects[i].isDone;
            return isLoaded;
        }
    }

    public class GlobalPauseDisabledStimuli : MonoBehaviour, IStimuli
    {
        private SystemStateMachine _stateMachine;
        private bool _stateMachineGlobalPauseEnabledValue;

        public GlobalPauseDisabledStimuli(bool defaultValue)
        {
            _stateMachine = FindObjectOfType<SystemStateMachine>();
            _stateMachineGlobalPauseEnabledValue = defaultValue;
        }

        public void showStimuli() { _stateMachine.GlobalPauseEnabled = false; }
        public void removeStimuli() { _stateMachine.GlobalPauseEnabled = _stateMachineGlobalPauseEnabledValue; }
        public bool isStimuliLoaded() { return true; }
    }
}