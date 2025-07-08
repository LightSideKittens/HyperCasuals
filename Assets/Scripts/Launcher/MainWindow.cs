using LSCore;
using UnityEngine.SceneManagement;

internal class MainWindow : BaseWindow<MainWindow>
{
    public LSButton playButton;

    private void Awake()
    {
        playButton.Submitted += () =>
        {
            SceneManager.LoadScene(LSMath.WrapIndex(CoreWorld.Level, 10, SceneManager.sceneCountInBuildSettings));
        };
    }
}