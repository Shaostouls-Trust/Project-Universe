using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
namespace Artngame.Orion.MiniGames
{
    public class GameController : MonoBehaviour
    {
        public static GameController instance;

        public GameObject[] hazardPrefabs;
        public Vector3 spawnValues;
        public int waveSize = 5;
        public float startDelay = 1.0f;
        public float hazardDelay = 0.5f;
        public float waveDelay = 3.0f;
        public Text scoreText;
        public Text restartText;
        public Text gameOverText;

        private int score;
        private bool gameOver;
        private bool canRestart;

        public void GameOver()
        {
            gameOverText.text = "Game Over";
            gameOver = true;
        }

        public void AddScore(int addition)
        {
            score += addition;
            UpdateScore();
        }

        public void SpawnHazard()
        {
            var spawnPos = new Vector3(Random.Range(-spawnValues.x, spawnValues.x), spawnValues.y, spawnValues.z);
            int i = Random.Range(0, hazardPrefabs.Length);
            GameObject hazardPrefab = hazardPrefabs[i];
            Instantiate(hazardPrefab, spawnPos, Quaternion.identity);
        }

        public IEnumerator SpawnWaves()
        {
            yield return new WaitForSeconds(startDelay);
            while (!gameOver)
            {
                for (var i = 0; i < waveSize; i++)
                {
                    SpawnHazard();
                    yield return new WaitForSeconds(hazardDelay);
                }
                yield return new WaitForSeconds(waveDelay);
            }
            restartText.text = "Press 'R' for Restart";
            canRestart = true;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
            if (canRestart)
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                }
            }
        }

        private void UpdateScore()
        {
            scoreText.text = "Score: " + score;
        }

        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
            StartCoroutine(SpawnWaves());
            score = 0;
            UpdateScore();
            gameOverText.text = "";
            restartText.text = "";
        }
    }
}