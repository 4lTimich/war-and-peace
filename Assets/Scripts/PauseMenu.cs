using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    /// <summary>
    /// ���������� ����, ������������, ��� ���� �� �����.
    /// </summary>
    public static bool IsGamePaused = false;

    [SerializeField] private GameObject pauseMenuCanvas; // Canvas/MenuPanel
    [SerializeField] private Button exitButton;          // ������ "�����"
    [SerializeField] private Slider volumeSlider;        // �������� ���������
    [SerializeField] private AudioSource musicSource;    // ������ �� AudioSource (Music Manager)

    private void Start()
    {
        // �������� ���� ��� ������
        if (pauseMenuCanvas != null)
            pauseMenuCanvas.SetActive(false);

        // ��������� ���������� �� ������ �����
        if (exitButton != null)
            exitButton.onClick.AddListener(ExitGame);

        // ��������� ���������� �� ������� ���������
        if (volumeSlider != null)
            volumeSlider.onValueChanged.AddListener(SetVolume);

        // �������������� �������� ������� ����������
        if (musicSource != null && volumeSlider != null)
            volumeSlider.value = musicSource.volume;
    }

    private void Update()
    {
        // ����������� ������� ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsGamePaused)
                Resume();
            else
                Pause();
        }
    }

    /// <summary>
    /// ��������� ���� �� ����� � ���������� ����.
    /// </summary>
    public void Pause()
    {
        IsGamePaused = true; // ������������� ����
        if (pauseMenuCanvas != null)
            pauseMenuCanvas.SetActive(true);

        // ������������� "�����" ����
        Time.timeScale = 0f;
    }

    /// <summary>
    /// ����� ����� � ������ ����.
    /// </summary>
    public void Resume()
    {
        IsGamePaused = false;
        if (pauseMenuCanvas != null)
            pauseMenuCanvas.SetActive(false);

        // ���������� "�����" ����
        Time.timeScale = 1f;
    }

    /// <summary>
    /// ���������� ������ "�����" �� ����.
    /// </summary>
    public void ExitGame()
    {
        // ����� ������� ���������� "�����" ���� � �����
        Time.timeScale = 1f;

#if UNITY_EDITOR
        // ������������� ���� � ���������
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // ��������� ����������
        Application.Quit();
#endif
    }

    /// <summary>
    /// ������������� ��������� ������
    /// </summary>
    /// <param name="volume">�������� ��������� �� ��������</param>
    public void SetVolume(float volume)
    {
        if (musicSource != null)
            musicSource.volume = volume;
    }
}
