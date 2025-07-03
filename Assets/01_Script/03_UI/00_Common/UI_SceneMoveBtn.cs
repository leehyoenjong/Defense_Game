using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_SceneMoveBtn : MonoBehaviour
{
    [SerializeField] string _scenename;
    public void Btn_SceneMove()
    {
        SceneManager.LoadScene(_scenename);
    }
}