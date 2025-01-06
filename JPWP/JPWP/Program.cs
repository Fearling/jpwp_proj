using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

public class MagicSchoolGame : Form
{
    private PictureBox playerPictureBox;
    private PictureBox backgroundPictureBox;

    private int playerSize = 20;
    private Point playerPosition = new Point(50, 50); // Startowa pozycja gracza
    private Random random = new Random(); // Generator liczb losowych
    private int score = 0; // Wynik gracza

    private bool isQuestionActive = false; // Czy pytanie jest aktywne
    private Rectangle levelChangeArea = new Rectangle(500, 250, 50, 50); // Obszar zmiany poziomu

    private List<Rectangle> questionAreas = new List<Rectangle>(); // Lista obszarów wywołujących pytania
    private List<Rectangle> blockedAreas = new List<Rectangle>(); // Lista obszarów blokujących ruch
    private List<Panel> areaPanels = new List<Panel>(); // Lista paneli graficznych dla obszarów

    private int currentLevel = 1; // Obecny poziom

    private int lives = 5; // Liczba żyć gracza

    private bool allQuestionsAnswered = false; // Czy wszystkie pytania zostały rozwiązane
    private Panel levelChangePanel; // Panel zmiany poziomu

    private List<(Rectangle Area, string Text)> textAreas = new List<(Rectangle, string)>(); // Lista obszarów z tekstami

    private void CreateTextArea(Rectangle area, string text)
    {
        textAreas.Add((area, text));

        Panel panel = new Panel
        {
            Location = new Point(area.X, area.Y),
            Size = new Size(area.Width, area.Height),
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.Green // Kolor obszaru tekstowego
        };

        areaPanels.Add(panel);
        backgroundPictureBox.Controls.Add(panel);
    }


    public MagicSchoolGame()
    {
        this.Text = "Gra: Magiczna Szkoła";
        this.ClientSize = new Size(640, 320);
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;

        this.KeyDown += new KeyEventHandler(OnKeyDown);

        CreateMainMenuDropdown(); // Dodanie rozwijanego menu
    }


    private void LoadLevel(int level)
    {
        playerPosition = new Point(50, 50);
        questionAreas.Clear();
        blockedAreas.Clear();
        textAreas.Clear();
        allQuestionsAnswered = false; // Resetowanie statusu pytań
        areaPanels.ForEach(panel => backgroundPictureBox.Controls.Remove(panel));
        areaPanels.Clear();

        SaveLastVisitedLevel(level);

        if (backgroundPictureBox != null)
        {
            this.Controls.Remove(backgroundPictureBox);
        }

        backgroundPictureBox = new PictureBox();
        backgroundPictureBox.Dock = DockStyle.Fill;

        switch (level)
        {
            case 1:
                backgroundPictureBox.Image = Image.FromFile("C:\\JPWP_projekt\\school_map.jpg");
                CreateQuestionArea(new Rectangle(300, 150, 100, 50));
                CreateQuestionArea(new Rectangle(100, 200, 100, 50));

                CreateTextArea(new Rectangle(400, 100, 100, 50), "Witaj w magicznej szkole!");
                CreateTextArea(new Rectangle(200, 250, 100, 50), "Uważaj na przeszkody!");

                CreateBlockedArea(new Rectangle(200, 100, 100, 50));
                break;
            case 2:
                backgroundPictureBox.Image = Image.FromFile("C:\\JPWP_projekt\\school_map1.jpg");
                CreateQuestionArea(new Rectangle(200, 150, 100, 50));

                CreateTextArea(new Rectangle(350, 250, 100, 50), "Drugi poziom jest trudniejszy!");
                CreateTextArea(new Rectangle(150, 150, 100, 50), "Powodzenia!");

                CreateBlockedArea(new Rectangle(100, 100, 150, 50));
                break;
            default:
                MessageBox.Show("Koniec gry! Gratulacje!", "Koniec");
                Application.Exit();
                return;
        }

        backgroundPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
        this.Controls.Add(backgroundPictureBox);

        playerPictureBox = new PictureBox();
        playerPictureBox.Image = Image.FromFile("C:\\JPWP_projekt\\player_icon.png");
        playerPictureBox.Size = new Size(playerSize, playerSize);
        playerPictureBox.Location = playerPosition;
        backgroundPictureBox.Controls.Add(playerPictureBox);

        levelChangePanel = new Panel
        {
            Location = new Point(levelChangeArea.X, levelChangeArea.Y),
            Size = new Size(levelChangeArea.Width, levelChangeArea.Height),
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.Blue,
            Visible = false // Ukrywamy na początku
        };
        backgroundPictureBox.Controls.Add(levelChangePanel);
    }

    private void RemoveQuestionArea(int index)
    {
        backgroundPictureBox.Controls.Remove(areaPanels[index]);
        areaPanels.RemoveAt(index);
        questionAreas.RemoveAt(index);

        // Sprawdzenie, czy wszystkie pytania zostały rozwiązane
        if (questionAreas.Count == 0)
        {
            allQuestionsAnswered = true;
            levelChangePanel.Visible = true; // Pokazujemy pole zmiany poziomu
        }
    }


    private void ShowLevelIntroduction(int level)
    {
        string introduction;

        switch (level)
        {
            case 1:
                introduction = "Witaj w magicznej szkole! Twoim zadaniem jest rozwiązać zadania matematyczne i zdobyć punkty, unikając przeszkód.";
                break;
            case 2:
                introduction = "Poziom drugi: Uczniowie są bardziej wymagający, a pytania trudniejsze. Przygotuj się na wyzwania!";
                break;
            default:
                introduction = "Rozpoczynasz nowy poziom. Powodzenia!";
                break;
        }

        MessageBox.Show(introduction, $"Poziom {level}");
    }





    private void CreateQuestionArea(Rectangle area)
    {
        questionAreas.Add(area);

        Panel panel = new Panel
        {
            Location = new Point(area.X, area.Y),
            Size = new Size(area.Width, area.Height),
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.FromArgb(128, 128, 128, 128)
        };

        areaPanels.Add(panel);
        backgroundPictureBox.Controls.Add(panel);
    }

    private void CreateBlockedArea(Rectangle area)
    {
        blockedAreas.Add(area);

        Panel panel = new Panel
        {
            Location = new Point(area.X, area.Y),
            Size = new Size(area.Width, area.Height),
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.Red
        };

        areaPanels.Add(panel);
        backgroundPictureBox.Controls.Add(panel);
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (isQuestionActive) return;

        Point newPosition = playerPosition;

        switch (e.KeyCode)
        {
            case Keys.W: newPosition.Offset(0, -playerSize); break;
            case Keys.S: newPosition.Offset(0, playerSize); break;
            case Keys.A: newPosition.Offset(-playerSize, 0); break;
            case Keys.D: newPosition.Offset(playerSize, 0); break;
        }

        // Sprawdzenie granic mapy
        if (newPosition.X < 0 || newPosition.Y < 0 ||
            newPosition.X + playerPictureBox.Width > this.ClientSize.Width ||
            newPosition.Y + playerPictureBox.Height > this.ClientSize.Height)
        {
            return;
        }

        // Sprawdzenie obszarów blokujących ruch
        foreach (Rectangle blockedArea in blockedAreas)
        {
            if (blockedArea.Contains(newPosition) || blockedArea.Contains(newPosition.X + playerSize - 1, newPosition.Y) || blockedArea.Contains(newPosition.X, newPosition.Y + playerSize - 1) || blockedArea.Contains(newPosition.X + playerSize - 1, newPosition.Y + playerSize - 1))
            {
                return;
            }
        }

        playerPosition = newPosition;
        playerPictureBox.Location = playerPosition;

        // Obsługa obszarów tekstowych
        foreach (var textArea in textAreas.ToList()) // Iterujemy po kopii listy
        {
            if (textArea.Area.Contains(playerPosition) || textArea.Area.Contains(newPosition.X + playerSize - 1, newPosition.Y) || textArea.Area.Contains(newPosition.X, newPosition.Y + playerSize - 1) || textArea.Area.Contains(newPosition.X + playerSize - 1, newPosition.Y + playerSize - 1))
            {
                MessageBox.Show(textArea.Text, "Informacja");
                var index = textAreas.IndexOf(textArea);
                HideTextArea(index); // Ukrywamy obszar tekstowy
                return;
            }
        }


        // Obsługa obszarów pytaniowych
        for (int i = 0; i < questionAreas.Count; i++)
        {
            if (questionAreas[i].Contains(playerPosition) || questionAreas[i].Contains(newPosition.X + playerSize - 1, newPosition.Y) || questionAreas[i].Contains(newPosition.X, newPosition.Y + playerSize - 1) || questionAreas[i].Contains(newPosition.X + playerSize - 1, newPosition.Y + playerSize - 1))
            {
                GenerateAndAskQuestion(i);
                return;
            }
        }

        // Obsługa zmiany poziomu
        if (levelChangeArea.Contains(playerPosition) || levelChangeArea.Contains(newPosition.X + playerSize - 1, newPosition.Y) || levelChangeArea.Contains(newPosition.X, newPosition.Y + playerSize - 1) || levelChangeArea.Contains(newPosition.X + playerSize - 1, newPosition.Y + playerSize - 1))
        {
            currentLevel++;
            SaveLastVisitedLevel(currentLevel);
            LoadLevel(currentLevel);
        }
    }



    private void GenerateAndAskQuestion(int areaIndex)
    {
        isQuestionActive = true;

        // Generowanie pytania (bez zmian)
        int number1 = random.Next(1, 10);
        int number2 = random.Next(1, 10);
        string[] operators = { "+", "-", "*", "/" };

        int correctAnswer = -1;
        switch (operators[currentLevel - 1])
        {
            case "+":
                number1 = random.Next(1, 20);
                number2 = random.Next(1, 20);
                correctAnswer = number1 + number2;
                break;
            case "-":
                correctAnswer = number1 - number2;
                while (correctAnswer < 0)
                {
                    number1 = random.Next(1, 20);
                    number2 = random.Next(1, 20);
                    correctAnswer = number1 - number2;
                }
                break;
            case "*":
                number1 = random.Next(1, 10);
                number2 = random.Next(1, 10);
                correctAnswer = number1 * number2;
                break;
            case "/":
                while (number2 == 0 || (number1 / number2) * number2 < number1)
                {
                    number2 = random.Next(1, 10);
                    number1 = number2 * random.Next(1, 10);
                }
                correctAnswer = number1 / number2;
                break;
        }

        // Wyświetlenie pytania
        string question = $"Ile to jest {number1} {operators[currentLevel - 1]} {number2}?";
        string input = ShowInputDialog(question);

        if (int.TryParse(input, out int userAnswer) && userAnswer == correctAnswer)
        {
            MessageBox.Show("Dobra odpowiedź! Brawo!", "Sukces");
            score++;
            RemoveQuestionArea(areaIndex);
        }
        else
        {
            lives--; // Odejmowanie życia
            MessageBox.Show($"Zła odpowiedź. Poprawna odpowiedź to {correctAnswer}. Pozostałe życia: {lives}", "Błąd");

            if (lives <= 0)
            {
                MessageBox.Show("Straciłeś wszystkie życia! Poziom zostanie zresetowany.", "Koniec poziomu");
                ResetLevel(); // Resetowanie poziomu
            }
        }

        isQuestionActive = false;
    }

    private void HideTextArea(int areaIndex)
    {
        if (areaIndex >= 0 && areaIndex < textAreas.Count)
        {
            var textArea = textAreas[areaIndex];
            textAreas.RemoveAt(areaIndex);

            var panel = areaPanels.FirstOrDefault(p =>
                p.Location == new Point(textArea.Area.X, textArea.Area.Y));
            if (panel != null)
            {
                backgroundPictureBox.Controls.Remove(panel);
                areaPanels.Remove(panel);
            }
        }
    }



    private void CreateMainMenuDropdown()
    {
        ToolStripDropDownButton menuDropdown = new ToolStripDropDownButton("Menu");
        menuDropdown.DropDownItems.Add("Restart", null, (s, e) => RestartGame());
        menuDropdown.DropDownItems.Add("Wyjdź", null, (s, e) => Application.Exit());

        ToolStrip menuStrip = new ToolStrip();
        menuStrip.Items.Add(menuDropdown);
        this.Controls.Add(menuStrip);
    }

    private void RestartGame()
    {
        currentLevel = 1; // Restart gry od poziomu 1
        lives = 5; // Przywrócenie żyć
        LoadLevel(currentLevel);
    }

    private void ResetLevel()
    {
        lives = 5; // Przywrócenie pełnej liczby żyć
        LoadLevel(currentLevel); // Ponowne załadowanie bieżącego poziomu
    }

    private string ShowInputDialog(string question)
    {
        Form inputForm = new Form();
        Label questionLabel = new Label() { Text = question, Dock = DockStyle.Top };
        TextBox inputBox = new TextBox() { Dock = DockStyle.Top };
        Button submitButton = new Button() { Text = "OK", Dock = DockStyle.Bottom };
        inputForm.Controls.Add(submitButton);
        inputForm.Controls.Add(inputBox);
        inputForm.Controls.Add(questionLabel);
        inputForm.ClientSize = new Size(300, 120);
        inputForm.StartPosition = FormStartPosition.CenterParent;

        string userInput = null;
        submitButton.Click += (s, e) => { userInput = inputBox.Text; inputForm.Close(); };

        inputForm.ShowDialog();
        return userInput;
    }

    private void ShowMainMenu()
    {
        Form menuForm = new Form
        {
            Text = "Menu Główne",
            ClientSize = new Size(300, 200),
            StartPosition = FormStartPosition.CenterScreen
        };

        Button newGameButton = new Button
        {
            Text = "Nowa Gra",
            Dock = DockStyle.Top
        };
        newGameButton.Click += (s, e) =>
        {
            currentLevel = 1; // Nowa gra zawsze zaczyna od poziomu 1
            SaveLastVisitedLevel(currentLevel); // Zapisujemy nową grę
            LoadLevel(currentLevel);
            menuForm.Close();
        };

        Button loadGameButton = new Button
        {
            Text = "Wczytaj",
            Dock = DockStyle.Top
        };
        loadGameButton.Click += (s, e) =>
        {
            currentLevel = LoadLastVisitedLevel(); // Wczytaj zapisany poziom
            LoadLevel(currentLevel);
            menuForm.Close();
        };

        Button exitButton = new Button
        {
            Text = "Wyjdź",
            Dock = DockStyle.Top
        };
        exitButton.Click += (s, e) => Application.Exit();

        menuForm.Controls.Add(exitButton);
        menuForm.Controls.Add(loadGameButton);
        menuForm.Controls.Add(newGameButton);
        menuForm.ShowDialog();
    }


    private void SaveLastVisitedLevel(int level)
    {
        try
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "savegame.txt");
            System.IO.File.WriteAllText(filePath, level.ToString());
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Nie udało się zapisać gry: {ex.Message}");
        }
    }



    private int LoadLastVisitedLevel()
    {
        try
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "savegame.txt");

            if (System.IO.File.Exists(filePath))
            {
                string savedLevel = System.IO.File.ReadAllText(filePath);
                if (int.TryParse(savedLevel, out int level))
                {
                    return level;
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Nie udało się wczytać gry: {ex.Message}");
        }

        return 1; // Domyślnie zaczynamy od poziomu 1
    }


    private void ShowSaveFilePath()
    {
        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "savegame.txt");
        MessageBox.Show($"Ścieżka zapisu pliku: {filePath}");
    }




    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        MagicSchoolGame game = new MagicSchoolGame();
        game.ShowMainMenu();
        Application.Run(game);

    }
}
