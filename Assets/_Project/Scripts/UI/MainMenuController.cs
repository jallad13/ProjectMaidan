using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ProjectMaidan.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button _startMatchButton;

        private void Awake()
        {
            if (_startMatchButton == null)
            {
                _startMatchButton = GetComponentInChildren<Button>(true);
            }

            _startMatchButton?.onClick.AddListener(StartMatch);
        }

        private void OnDestroy()
        {
            _startMatchButton?.onClick.RemoveListener(StartMatch);
        }

        public void StartMatch()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene("Match");
        }
    }
}
