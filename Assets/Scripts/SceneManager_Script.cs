using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class SceneManager_Script : MonoBehaviour {

    // Use this for initialization
    void Start() {
        DontDestroyOnLoad(this.gameObject);
    }

    public void GoToBattle()
    {
        SceneManager.LoadScene(1, LoadSceneMode.Single);
    }

    public void BattleWon()
    {

            SceneManager.LoadScene(2, LoadSceneMode.Single);
    }
    public void BattleLost()
    {

        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }
}
