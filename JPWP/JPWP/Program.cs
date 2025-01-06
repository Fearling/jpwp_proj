using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

public class MagicSchoolGame : Form
{
    private PictureBox playerPictureBox;
    private PictureBox backgroundPictureBox;

    private Point playerPosition = new Point(50, 50); // Startowa pozycja gracza
    private Random random = new Random(); // Generator liczb losowych
    private int score = 0; // Wynik gracza

    private bool isQuestionActive = false; // Czy pytanie jest aktywne
    private Rectangle levelChangeArea = new Rectangle(500, 250, 50, 50); // Obszar zmiany poziomu

    private List<Rectangle> questionAreas = new List<Rectangle>(); // Lista obszarów wywołujących pytania
    private List<Panel> areaPanels = new List<Panel>(); // Lista paneli graficznych dla obszarów

    private int currentLevel = 1; // Obecny poziom

    public MagicSchoolGame()
    {
        // Inicjalizacja okna
        this.Text = "Gra: Magiczna Szkoła";
        this.ClientSize = new Size(640, 320);
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;

        // Obsługa klawiatury
        this.KeyDown += new KeyEventHandler(OnKeyDown);

        // Inicjalizacja poziomu
        LoadLevel(1);
    }

    private void LoadLevel(int level)
    {
        // Reset gry
        playerPosition = new Point(50, 50);
        questionAreas.Clear();
        areaPanels.ForEach(panel => backgroundPictureBox.Controls.Remove(panel));
        areaPanels.Clear();

        // Zapis ostatniego poziomu
        SaveLastVisitedLevel(level);

        // Tło
        if (backgroundPictureBox != null)
        {
            this.Controls.Remove(backgroundPictureBox);
        }

        backgroundPictureBox = new PictureBox();
        backgroundPictureBox.Dock = DockStyle.Fill;

        // Ustawienie odpowiedniej mapy dla poziomu
        switch (level)
        {
            case 1:
                backgroundPictureBox.Image = Image.FromFile("C:\\JPWP_projekt\\school_map.jpg");
                CreateQuestionArea(new Rectangle(300, 150, 100, 50));
                CreateQuestionArea(new Rectangle(100, 200, 100, 50));
                CreateQuestionArea(new Rectangle(400, 100, 100, 50));
                break;
            case 2:
                backgroundPictureBox.Image = Image.FromFile("C:\\JPWP_projekt\\school_map1.jpg");
                CreateQuestionArea(new Rectangle(200, 150, 100, 50));
                CreateQuestionArea(new Rectangle(350, 250, 100, 50));
                break;
            default:
                MessageBox.Show("Koniec gry! Gratulacje!", "Koniec");
                Application.Exit();
                return;
        }

        backgroundPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
        this.Controls.Add(backgroundPictureBox);

        // Gracz
        playerPictureBox = new PictureBox();
        playerPictureBox.Image = Image.FromFile("C:\\JPWP_projekt\\player_icon.png");
        playerPictureBox.Size = new Size(30, 30);
        playerPictureBox.Location = playerPosition;
        backgroundPictureBox.Controls.Add(playerPictureBox);

        // Pole zmiany poziomu
        Panel levelChangePanel = new Panel
        {
            Location = new Point(levelChangeArea.X, levelChangeArea.Y),
            Size = new Size(levelChangeArea.Width, levelChangeArea.Height),
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.Blue
        };
        backgroundPictureBox.Controls.Add(levelChangePanel);
    }

    private void CreateQuestionArea(Rectangle area)
    {
        questionAreas.Add(area);

        // Tworzenie wizualnego obszaru
        Panel panel = new Panel
        {
            Location = new Point(area.X, area.Y),
            Size = new Size(area.Width, area.Height),
            BorderStyle = BorderStyle.FixedSingle,
            BackColor = Color.FromArgb(128, 128, 128, 128) // Prawie przezroczysty szary
        };

        areaPanels.Add(panel);
        backgroundPictureBox.Controls.Add(panel);
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (isQuestionActive) return; // Gracz nie może się poruszać podczas pytania

        // Przemieszczanie gracza
        switch (e.KeyCode)
        {
            case Keys.W: MovePlayer(0, -10); break; // W - Góra
            case Keys.S: MovePlayer(0, 10); break;  // S - Dół
            case Keys.A: MovePlayer(-10, 0); break; // A - Lewo
            case Keys.D: MovePlayer(10, 0); break;  // D - Prawo
        }
    }

    private void MovePlayer(int dx, int dy)
    {
        playerPosition.Offset(dx, dy);
        playerPictureBox.Location = playerPosition;

        // Sprawdzenie, czy gracz dotarł do jakiegoś obszaru pytania
        for (int i = 0; i < questionAreas.Count; i++)
        {
            if (questionAreas[i].Contains(playerPosition))
            {
                GenerateAndAskQuestion(i);
                return;
            }
        }

        // Sprawdzenie, czy gracz wszedł na pole zmiany poziomu
        if (levelChangeArea.Contains(playerPosition))
        {
            currentLevel++;
            SaveLastVisitedLevel(currentLevel); // Automatyczne zapisywanie poziomu
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
        string selectedOperator = operators[random.Next(operators.Length)];

        int correctAnswer = -1;
        switch (selectedOperator)
        {
            case "+":
                correctAnswer = number1 + number2;
                break;

            case "-":
                correctAnswer = number1 - number2;
                while (correctAnswer < 0)
                {
                    number1 = random.Next(1, 10);
                    number2 = random.Next(1, 10);
                    correctAnswer = number1 - number2;
                }
                break;

            case "*":
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
        string question = $"Ile to jest {number1} {selectedOperator} {number2}?";
        string input = ShowInputDialog(question);

        if (int.TryParse(input, out int userAnswer) && userAnswer == correctAnswer)
        {
            MessageBox.Show("Dobra odpowiedź! Brawo!", "Sukces");
            score++;
            RemoveQuestionArea(areaIndex);
        }
        else
        {
            MessageBox.Show($"Zła odpowiedź. Poprawna odpowiedź to {correctAnswer}.", "Błąd");
        }

        isQuestionActive = false;
    }

    private void RemoveQuestionArea(int index)
    {
        backgroundPictureBox.Controls.Remove(areaPanels[index]);
        areaPanels.RemoveAt(index);
        questionAreas.RemoveAt(index);
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
        ShowSaveFilePath();
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
