using Timer = System.Threading.Timer;

namespace SpellingMAUI;

[QueryProperty("Spelling", "Spelling")]
public partial class MainPage : ContentPage
{
    int correct = 0;
    int incorrect = 0;
    string currentWord = null;
    Timer threadTimer = null;
    List<string> incorrectWords = new List<string>();
    readonly Random random = new Random();
    private readonly SpellingsDatabase database;
    private readonly Spellcheker spellcheker;
    CancellationTokenSource cts;
    private Spelling spelling;
    private string practiceLabel;

    private List<string> Words { get; set; }

    private int WordsCount;

    public string PracticeLabel
    {
        get => practiceLabel; set
        {
            practiceLabel = value;
            OnPropertyChanged(nameof(PracticeLabel));
        }
    }

    public Spelling Spelling
    {
        get => spelling; set
        {
            spelling = value;
            if (spelling != null)
            {
                LoadSpellings();
                PracticeLabel = $"Practice: {spelling.Name} Total Words: {WordsCount}";
            }
        }
    }

    public MainPage(SpellingsDatabase spellingDatabase, Spellcheker spellcheker)
    {
        InitializeComponent();

        database = spellingDatabase;
        this.spellcheker = spellcheker;
        BindingContext = this;
    }

    private void LoadSpellings()
    {
        Words = Spelling != null ? Spelling.Words?.Split(',').Select(x => x.Trim()).Distinct().ToList() : new();
        WordsCount = Words.Count;
    }

    private async void TxtSpell_Completed(object sender, EventArgs e)
    {
        string text = ((Entry)sender).Text;

        if (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(currentWord))
        {
            return;
        }

        if (spellcheker.Spell(currentWord) == spellcheker.Spell(text))
        {
            correct++;
            AnswerLbl.TextColor = Color.Parse("Green");
        }
        else
        {
            incorrect++;
            AnswerLbl.TextColor = Color.Parse("Red");
            incorrectWords.Add(currentWord);
        }

        AnswerLbl.Text = currentWord;
        ResultLbl.Text = $"Correct: {correct} | Incorrect: {incorrect}  |   Progress: {correct + incorrect}/{WordsCount}";

        TxtSpell.Text = "";

        currentWord = GetRandomWord();
        if (!string.IsNullOrEmpty(currentWord))
        {
            await SpeakNowDefaultSettingsAsync(currentWord);
        }
    }

    private async void SpeakBtn_Clicked(object sender, EventArgs e)
    {
        currentWord = GetRandomWord();
        if (currentWord is null)
            return;

        SpeakBtn.IsEnabled = false;
        StopBtn.IsEnabled = true;
        ResultLbl.IsVisible = true;
        AnswerLbl.IsVisible = true;
        StartTimer();
        await SpeakNowDefaultSettingsAsync(currentWord);
    }

    private string GetRandomWord()
    {
        if (Words.Count == 0)
        {
            StopBtn_Clicked(null, null);
            return null;
        }
        int index = random.Next(0, Words.Count);
        var word = Words[index];
        Words.RemoveAt(index);

        return word;
    }

    private void StartTimer()
    {
        int seconds = 0;

        threadTimer = new Timer(callback =>
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                MainThread.InvokeOnMainThreadAsync(() =>
                {
                    seconds++;
                    TimeSpan t = TimeSpan.FromSeconds(seconds);
                    string answer = string.Format("{0:D2}h:{1:D2}m:{2:D2}s",
                                    t.Hours,
                                    t.Minutes,
                                    t.Seconds);
                    TimerLbl.Text = answer;
                });
            });
        }, null, 0, 1000);
    }

    private void StopTimer()
    {
        if (threadTimer != null)
        {
            threadTimer.Change(Timeout.Infinite, Timeout.Infinite);
            threadTimer.Dispose();
            threadTimer = null;
        }
    }


    public async Task SpeakNowDefaultSettingsAsync(string text = "Hello World")
    {
        cts = new CancellationTokenSource();
        await TextToSpeech.Default.SpeakAsync(text, cancelToken: cts.Token);

    }

    // Cancel speech if a cancellation token exists & hasn't been already requested.
    public void CancelSpeech()
    {
        if (cts?.IsCancellationRequested ?? true)
            return;

        cts.Cancel();
    }

    private async void StopBtn_Clicked(object sender, EventArgs e)
    {
        // save to database
        StopTimer();

        if (correct > 0 || incorrect > 0)
        {
            UserScores scores = new UserScores();
            scores.Correct = correct;
            scores.InCorrect = incorrect;
            scores.Time = TimerLbl.Text;
            scores.IncorrectWords = string.Join(", ", incorrectWords);

            await database.SaveItemAsync(scores);
        }

        SpeakBtn.IsEnabled = true;
        StopBtn.IsEnabled = false;
        currentWord = null;

        // reset stats
        correct = incorrect = WordsCount = 0;
        ResultLbl.Text = "";
        ResultLbl.IsVisible = AnswerLbl.IsVisible = false;
        incorrectWords.Clear();
        AnswerLbl.Text = "";
    }

    private async void BackBtn_Clicked(object sender, EventArgs e)
    {
        StopBtn_Clicked(null, null);
        await Shell.Current.GoToAsync("..");
    }

    private async void ReplayBtn_Clicked(object sender, EventArgs e)
    {
        if (currentWord is null)
            return;

        await SpeakNowDefaultSettingsAsync(currentWord);
    }
}