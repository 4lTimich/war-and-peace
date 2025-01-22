using Ink.Runtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;
using UnityEngine.UI;

public class Dialogues : MonoBehaviour
{
    private Story _currentStory;
    private TextAsset _inkJson;

    private GameObject _dialoguePanel;
    private TextMeshProUGUI _dialogueText;
    private TextMeshProUGUI _nameText;

    [HideInInspector] public GameObject choiceButtonsPanel;
    private GameObject _choiceButton;
    private List<TextMeshProUGUI> _choicesText = new();

    private List<Character> characters = new();
    private float multiplier = 1.1f;

    // Добавляем поле для текста сносок
    [SerializeField] private TextMeshProUGUI _footnoteText;

    public bool DialogPlay { get; private set; }

    [Inject]
    public void Construct(DialoguesInstaller dialoguesInstaller)
    {
        _inkJson = dialoguesInstaller.inkJson;
        _dialoguePanel = dialoguesInstaller.dialoguePanel;
        _dialogueText = dialoguesInstaller.dialogueText;
        _nameText = dialoguesInstaller.nameText;
        choiceButtonsPanel = dialoguesInstaller.choiceButtonsPanel;
        _choiceButton = dialoguesInstaller.choiceButton;
    }

    private void Awake()
    {
        _currentStory = new Story(_inkJson.text);
    }

    void Start()
    {
        foreach (var character in FindObjectsOfType<Character>())
        {
            characters.Add(character);
        }
        StartDialogue();
    }

    public void StartDialogue()
    {
        DialogPlay = true;
        _dialoguePanel.SetActive(true);
        ContinueStory();
    }

    public void ContinueStory(bool choiceBefore = false)
    {
        // --- ВАЖНО: проверяем, не стоит ли пауза ---
        if (PauseMenu.IsGamePaused)
        {
            return;
        }
        // ------------------------------------------

        if (_isTyping)
        {
            // Если текст анимируется, пропустить анимацию и показать полный текст сразу
            StopCoroutine(_typingCoroutine);
            _dialogueText.text = _fullText;
            _isTyping = false;
            return;
        }

        if (_currentStory.canContinue)
        {
            ShowDialogue();
            ShowChoiceButtons();
        }
        else if (!choiceBefore)
        {
            ExitDialogue();
        }
    }

    private string _fullText = "";
    private Coroutine _typingCoroutine;
    private bool _isTyping = false;
    private float _typeSpeed = 0.03f; // Скорость появления символов

    private void ShowDialogue()
    {
        _fullText = _currentStory.Continue();
        List<string> currentTags = _currentStory.currentTags;

        // Получаем сноску из тегов, если есть
        string footnote = GetFootnoteFromTags(currentTags);

        // Отобразим или спрячем сноску
        if (!string.IsNullOrEmpty(footnote))
        {
            _footnoteText.text = footnote;
            _footnoteText.gameObject.SetActive(true);
        }
        else
        {
            _footnoteText.gameObject.SetActive(false);
        }

        _nameText.text = (string)_currentStory.variablesState["characterName"];

        var index = characters.FindIndex(character => character.characterName.Contains(_nameText.text));
        if (index >= 0)
        {
            characters[index].ChangeEmotion((int)_currentStory.variablesState["characterExpression"]);
            ChangeCharacterScale(index);
        }
        else
        {
            characters.ForEach(character => character.ResetScale());
        }

        // Запуск анимации текста
        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
        }
        _typingCoroutine = StartCoroutine(TypeText(_fullText));
    }

    private IEnumerator TypeText(string fullText)
    {
        _isTyping = true;
        _dialogueText.text = "";
        foreach (char c in fullText)
        {
            _dialogueText.text += c;
            yield return new WaitForSeconds(_typeSpeed * multiplier);
        }
        _isTyping = false;
    }

    private void ChangeCharacterScale(int indexCharacter)
    {
        if (indexCharacter >= 0)
        {
            foreach (var character in characters)
            {
                if (character != characters[indexCharacter])
                {
                    character.ResetScale();
                }
                else if (character.DefaultScale == character.transform.localScale)
                {
                    character.ChangeScale(multiplier);
                }
            }
        }
        else
        {
            characters.ForEach(character => character.ResetScale());
        }
    }

    private void ShowChoiceButtons()
    {
        List<Choice> currentChoices = _currentStory.currentChoices;
        choiceButtonsPanel.SetActive(currentChoices.Count != 0);
        if (currentChoices.Count <= 0) { return; }
        choiceButtonsPanel.transform.Cast<Transform>().ToList().ForEach(child => Destroy(child.gameObject));
        _choicesText.Clear();
        for (int i = 0; i < currentChoices.Count; i++)
        {
            GameObject choice = Instantiate(_choiceButton);
            choice.GetComponent<ButtonAction>().index = i;
            choice.transform.SetParent(choiceButtonsPanel.transform, false);

            TextMeshProUGUI choiceText = choice.GetComponentInChildren<TextMeshProUGUI>();
            choiceText.text = currentChoices[i].text;
            _choicesText.Add(choiceText);
        }
    }

    public void ChoiceButtonAction(int choiceIndex)
    {
        // Если текст анимируется, пропустить анимацию и показать полный текст сразу
        if (_isTyping)
        {
            StopCoroutine(_typingCoroutine);
            _dialogueText.text = _fullText;
            _isTyping = false;
            return;
        }

        _currentStory.ChooseChoiceIndex(choiceIndex);
        ContinueStory(true);
    }

    private void ExitDialogue()
    {
        DialogPlay = false;
        _dialoguePanel.SetActive(false);
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        if (nextSceneIndex <= SceneManager.sceneCount)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
    }

    // Метод для получения текста сноски из тегов Ink
    private string GetFootnoteFromTags(List<string> tags)
    {
        foreach (var tag in tags)
        {
            if (tag.StartsWith("footnote:"))
            {
                return tag.Substring("footnote:".Length).Trim();
            }
        }
        return null;
    }
}
