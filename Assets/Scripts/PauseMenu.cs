using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    /// <summary>
    /// Глобальный флаг, показывающий, что игра на паузе.
    /// </summary>
    public static bool IsGamePaused = false;

    [SerializeField] private GameObject pauseMenuCanvas; // Canvas/MenuPanel
    [SerializeField] private Button exitButton;          // Кнопка "Выход"
    [SerializeField] private Slider volumeSlider;        // Ползунок громкости
    [SerializeField] private AudioSource musicSource;    // Ссылка на AudioSource (Music Manager)

    private void Start()
    {
        // Скрываем меню при старте
        if (pauseMenuCanvas != null)
            pauseMenuCanvas.SetActive(false);

        // Назначаем обработчик на кнопку Выход
        if (exitButton != null)
            exitButton.onClick.AddListener(ExitGame);

        // Назначаем обработчик на слайдер громкости
        if (volumeSlider != null)
            volumeSlider.onValueChanged.AddListener(SetVolume);

        // Инициализируем ползунок текущей громкостью
        if (musicSource != null && volumeSlider != null)
            volumeSlider.value = musicSource.volume;
    }

    private void Update()
    {
        // Отслеживаем нажатие ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (IsGamePaused)
                Resume();
            else
                Pause();
        }
    }

    /// <summary>
    /// Поставить игру на паузу и отобразить меню.
    /// </summary>
    public void Pause()
    {
        IsGamePaused = true; // устанавливаем флаг
        if (pauseMenuCanvas != null)
            pauseMenuCanvas.SetActive(true);

        // Останавливаем "время" игры
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Снять паузу и скрыть меню.
    /// </summary>
    public void Resume()
    {
        IsGamePaused = false;
        if (pauseMenuCanvas != null)
            pauseMenuCanvas.SetActive(false);

        // Возвращаем "время" игры
        Time.timeScale = 1f;
    }

    /// <summary>
    /// Обработчик кнопки "Выход" из игры.
    /// </summary>
    public void ExitGame()
    {
        // Перед выходом возвращаем "время" игры к норме
        Time.timeScale = 1f;

#if UNITY_EDITOR
        // Останавливаем игру в редакторе
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // Закрываем приложение
        Application.Quit();
#endif
    }

    /// <summary>
    /// Устанавливаем громкость музыки
    /// </summary>
    /// <param name="volume">Значение громкости из слайдера</param>
    public void SetVolume(float volume)
    {
        if (musicSource != null)
            musicSource.volume = volume;
    }
}
