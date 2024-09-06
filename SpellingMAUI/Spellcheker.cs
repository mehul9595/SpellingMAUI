using NHunspell;

namespace SpellingMAUI
{
    public class Spellcheker : IDisposable
    {
        // I want to use NHunspell to check the spelling of words using en-US, and en-GB dictionaries.
        private List<Hunspell> hunspells;

        public Spellcheker()
        {
            string affPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Dictionaries", "en_US.aff");
            string dicPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Dictionaries", "en_US.dic");

            string affPathGB = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Dictionaries", "en_GB.aff");
            string dicPathGB = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Dictionaries", "en_GB.dic");

            var hunspellGB = new Hunspell(affPathGB, dicPathGB);

            var hunspellUS = new Hunspell(affPath, dicPath);

            hunspells = new List<Hunspell>
            {
                hunspellUS,
                hunspellGB
            };
        }

        public void Dispose()
        {
            if (hunspells != null & hunspells.Count > 0)
            {
                hunspells.ForEach(x => x.Dispose());
            }
        }

        public bool Spell(string word)
        {
            return hunspells.Any(x =>x.Spell(word));
        }
    }
}



//// ...

//private void TxtSpell_Completed(object sender, EventArgs e)
//{
//    string text = ((Entry)sender).Text;

//    if (string.IsNullOrEmpty(text))
//    {
//        return;
//    }

//    if (!string.IsNullOrEmpty(currentWord))
//    {
//        bool isCorrect = hunspell.Spell(text);

//        if (isCorrect)
//        {
//            correct++;
//            AnswerLbl.TextColor = Color.Parse("Green");
//        }
//        else
//        {
//            incorrect++;
//            AnswerLbl.TextColor = Color.Parse("Red");
//            incorrectWords.Add(currentWord);
//        }
//    }

//    // ...
//}
