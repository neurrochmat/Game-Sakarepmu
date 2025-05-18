using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameSampah
{
    public class HalamanManager : MonoBehaviour
    {
        public bool isEscapeToExit;
        // Use this for initialization
        void Start()
        {
        }
        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyUp(KeyCode.Escape))
            {
                if (isEscapeToExit)
                {
                    Application.Quit();
                }
                else
                {
                    KembaliKeMenu();
                }
            }
        }
        public void PilihCharacter()
        {
            SceneManager.LoadScene("Character2");
        }

        public void ModePermainan()
        {
            SceneManager.LoadScene("Mode2");
        }

        public void MulaiPermainan()
        {
            SceneManager.LoadScene("Main2");
        }
        public void GamePingpong()
        {
            SceneManager.LoadScene("Menu2");
        }
        public void KembaliKeMenu()
        {
            SceneManager.LoadScene("MainMenu");
        }

        public void GamePengenalan()
        {
            SceneManager.LoadScene("Home1");
        }

        public void GameSampah()
        {
            SceneManager.LoadScene("Menu");
        }

        public void Credit()
        {
            SceneManager.LoadScene("CreditScene");
        }
    }
}