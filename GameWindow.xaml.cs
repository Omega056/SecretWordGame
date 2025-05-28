using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using System.Globalization;

namespace SecretGame
{
    public partial class GameWindow : Window
    {
        private readonly List<string> validWords = new List<string>
        {
            // 100 Kazakh words (5 letters, Cyrillic script)
            "балшы", "көлік", "үйші", "гүлді", "айнал", "өзен", "орман", "шөпті", "жұлды", "құсты",
            "кітап", "ағаш", "көктем", "қыста", "жазда", "күзде", "теңіз", "тауды", "әнші", "биші",
            "алтын", "балақ", "бақыт", "бәйге", "білім", "бірлік", "бөбек", "дәуір", "дерек", "достық",
            "дүкен", "ерік", "ешті", "жалын", "жамбыл", "жақсы", "жарма", "жолда", "жүрек", "жылқы",
            "жылды", "кәсіп", "келін", "керек", "кірме", "көмек", "күнгей", "қадам", "қалай", "қараш",
            "қасқа", "қатты", "қауым", "қолай", "қорқа", "құтты", "қызды", "сәлем", "сауап", "сәуле",
            "сенім", "серті", "сүйкім", "сыйлық", "тәлім", "тәуір", "тәжік", "тілек", "төбеде", "түрік",
            "түске", "тыным", "үміті", "үйрек", "шапан", "шатты", "шебер", "шынар", "ыманы", "ықпал",
            "ырысы", "өтеме", "өзгек", "өткір", "өшір", "салма", "тазай", "тұрғы", "ұлысы", "ұнату",
            "шұғыла", "ыстық", "әлемі", "батыр", "жанар", "күміс", "нұрлы", "сұлуы", "таныс", "шаруа"
        };

        private string targetWord;
        private string hintWord;
        private int currentRow = 0;
        private readonly int MaxRows = 6; // Standard Wordle: 6 attempts
        private readonly int WordLength = 5;
        private int score = 0;
        private int level = 1;
        private string currentUser;
        private string currentGuess = "";

        public GameWindow(string username)
        {
            InitializeComponent();
            currentUser = username;
            Title = $"Жасырын Сөздер Ойыны - {username}";
            InitializeGame();
        }

        private int CountSharedLetters(string word1, string word2)
        {
            var set1 = new HashSet<char>(word1.ToLower(CultureInfo.InvariantCulture));
            var set2 = new HashSet<char>(word2.ToLower(CultureInfo.InvariantCulture));
            set1.IntersectWith(set2);
            return set1.Count;
        }

        private void InitializeGame()
        {
            if (validWords.Count == 0)
            {
                ResultTextBlock.Text = "Қате: сөздер тізімі бос!";
                return;
            }

            Random rand = new Random();
            targetWord = validWords[rand.Next(validWords.Count)];

            var candidateHints = validWords
                .Where(w => w != targetWord && (CountSharedLetters(w, targetWord) == 2 || CountSharedLetters(w, targetWord) == 3))
                .ToList();
            hintWord = candidateHints.Count > 0
                ? candidateHints[rand.Next(candidateHints.Count)]
                : validWords.First(w => w != targetWord);

            score = 0;
            level = 1;
            currentRow = 0;
            currentGuess = "";
            SubmitButton.IsEnabled = true;

            GuessGrid.RowDefinitions.Clear();
            GuessGrid.ColumnDefinitions.Clear();
            GuessGrid.Children.Clear();

            for (int i = 0; i < MaxRows; i++)
                GuessGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });
            for (int i = 0; i < WordLength; i++)
                GuessGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });

            for (int row = 0; row < MaxRows; row++)
            {
                for (int col = 0; col < WordLength; col++)
                {
                    Border border = new Border
                    {
                        Background = Brushes.White,
                        BorderBrush = Brushes.Gray,
                        BorderThickness = new Thickness(1),
                        Margin = new Thickness(2)
                    };
                    TextBlock textBlock = new TextBlock
                    {
                        FontSize = 20,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    border.Child = textBlock;
                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, col);
                    GuessGrid.Children.Add(border);
                }
            }

            GuessInput.Text = "";
            UpdateUI();
        }

        private void GuessInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SubmitGuess();
            }
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            SubmitGuess();
        }

        private void SubmitGuess()
        {
            currentGuess = GuessInput.Text.ToUpper(CultureInfo.InvariantCulture);
            if (currentGuess.Length != WordLength)
            {
                ResultTextBlock.Text = "Сөз 5 буквтен болуы керек!";
                return;
            }

            string normalizedGuess = currentGuess.ToLower(CultureInfo.InvariantCulture).Normalize(NormalizationForm.FormC);
            if (!validWords.Contains(normalizedGuess))
            {
                ResultTextBlock.Text = $"Бұл сөз тізімде жоқ! Енгізілген сөз: {normalizedGuess}";
                return;
            }

            CheckGuess(normalizedGuess);
            currentGuess = "";
            GuessInput.Text = "";
            UpdateGuessDisplay();
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            InitializeGame();
        }

        private void UpdateGuessDisplay()
        {
            for (int col = 0; col < WordLength; col++)
            {
                Border border = GuessGrid.Children.Cast<UIElement>()
                    .Where(x => Grid.GetRow(x) == currentRow && Grid.GetColumn(x) == col)
                    .First() as Border;
                TextBlock textBlock = border.Child as TextBlock;
                if (col < currentGuess.Length)
                {
                    textBlock.Text = currentGuess[col].ToString();
                    border.Background = Brushes.White;
                }
                else
                {
                    textBlock.Text = "";
                    border.Background = Brushes.White;
                }
            }
        }

        private void CheckGuess(string guess)
        {
            bool isCorrect = guess == targetWord;
            bool[] targetUsed = new bool[WordLength];
            // First pass: Mark correct positions (green)
            for (int i = 0; i < WordLength; i++)
            {
                Border border = GuessGrid.Children.Cast<UIElement>()
                    .Where(x => Grid.GetRow(x) == currentRow && Grid.GetColumn(x) == i)
                    .First() as Border;
                TextBlock textBlock = border.Child as TextBlock;
                textBlock.Text = guess[i].ToString();

                if (guess[i] == targetWord[i])
                {
                    border.Background = Brushes.Green;
                    targetUsed[i] = true;
                }
            }

            // Second pass: Mark correct letters in wrong positions (yellow) and absent letters (gray)
            if (!isCorrect)
            {
                for (int i = 0; i < WordLength; i++)
                {
                    if (guess[i] != targetWord[i])
                    {
                        Border border = GuessGrid.Children.Cast<UIElement>()
                            .Where(x => Grid.GetRow(x) == currentRow && Grid.GetColumn(x) == i)
                            .First() as Border;
                        bool found = false;
                        for (int j = 0; j < WordLength; j++)
                        {
                            if (!targetUsed[j] && guess[i] == targetWord[j])
                            {
                                border.Background = Brushes.Yellow;
                                targetUsed[j] = true;
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                        {
                            border.Background = Brushes.Gray;
                        }
                    }
                }
            }

            if (isCorrect)
            {
                score += 10 * (MaxRows - currentRow);
                ResultTextBlock.Text = "Құттықтаймыз! Сіз сөзді таптыңыз!";
                SubmitButton.IsEnabled = false;
                return;
            }
            else
            {
                currentRow++;
                if (currentRow >= MaxRows)
                {
                    ResultTextBlock.Text = "Сіз ұтылдыңыз!";
                    SubmitButton.IsEnabled = false;
                    return;
                }
                ResultTextBlock.Text = "Қате! Қайтадан көріңіз!";
            }

            UpdateUI();
        }

        private void UpdateUI()
        {
            LevelTextBlock.Text = $"{level}-деңгей";
            ScoreTextBlock.Text = $"Ұпай: {score}";
            HiddenWordHint.Text = "Сөз: " + string.Join(" ", Enumerable.Repeat("_", WordLength));
            HintWordTextBlock.Text = $"Подсказка: {hintWord}";
        }

        // Removed methods no longer needed due to KeyboardGrid removal
        /*
        private void KeyboardButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentGuess.Length < WordLength)
            {
                Button btn = sender as Button;
                currentGuess += btn.Tag.ToString();
                UpdateGuessDisplay();
            }
        }

        private void BackspaceButton_Click(object sender, RoutedEventArgs e)
        {
            if (currentGuess.Length > 0)
            {
                currentGuess = currentGuess.Substring(0, currentGuess.Length - 1);
                UpdateGuessDisplay();
            }
        }
        */
    }
}