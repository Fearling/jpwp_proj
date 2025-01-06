using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

/// <summary>
/// Główna klasa
/// </summary>
public class MagicSchoolGame : Form
{
    /// <summary>
    /// Obiekt reprezentujący gracza w grze.
    /// </summary>
    private PictureBox playerPictureBox;

    /// <summary>
    /// Obiekt reprezentujący tło gry.
    /// </summary>
    private PictureBox backgroundPictureBox;

    /// <summary>
    /// Rozmiar gracza (wysokość i szerokość).
    /// </summary>
    private int playerSize = 20;

    /// <summary>
    /// Aktualna pozycja gracza.
    /// </summary>
    private Point playerPosition = new Point(50, 50);

    /// <summary>
    /// Generator liczb losowych.
    /// </summary>
    private Random random = new Random();

    /// <summary>
    /// Wynik gracza.
    /// </summary>
    private int score = 0;

    /// <summary>
    /// Flaga określająca, czy pytanie jest aktywne.
    /// </summary>
    private bool isQuestionActive = false;

    /// <summary>
    /// Obszar zmiany poziomu.
    /// </summary>
    private Rectangle levelChangeArea = new Rectangle(500, 250, 50, 50);

    /// <summary>
    /// Lista obszarów wywołujących pytania.
    /// </summary>
    private List<Rectangle> questionAreas = new List<Rectangle>();

    /// <summary>
    /// Lista obszarów blokujących ruch gracza.
    /// </summary>
    private List<Rectangle> blockedAreas = new List<Rectangle>();

    /// <summary>
    /// Lista paneli graficznych dla obszarów.
    /// </summary>
    private List<Panel> areaPanels = new List<Panel>();

    /// <summary>
    /// Lista kontrolek graficznych dodanych do tła.
    /// </summary>
    private List<Control> areaGraphix = new List<Control>();

    /// <summary>
    /// Aktualny poziom gry.
    /// </summary>
    private int currentLevel = 1;

    /// <summary>
    /// Liczba żyć gracza.
    /// </summary>
    private int lives = 5;

    /// <summary>
    /// Flaga określająca, czy wszystkie pytania zostały rozwiązane.
    /// </summary>
    private bool allQuestionsAnswered = false;

    /// <summary>
    /// Panel zmiany poziomu.
    /// </summary>
    private Panel levelChangePanel;

    /// <summary>
    /// Lista obszarów z tekstami i ich zawartościami.
    /// </summary>
    private List<(Rectangle Area, string Text)> textAreas = new List<(Rectangle, string)>();

    /// <summary>
    /// Tworzy obszar tekstowy z przypisanym obrazem.
    /// </summary>
    /// <param name="area">Obszar, w którym pojawi się tekst.</param>
    /// <param name="text">Treść tekstu.</param>
    /// <param name="imagePath">Ścieżka do pliku obrazu.</param>
    private void CreateTextArea(Rectangle area, string text, string imagePath)
    {
        textAreas.Add((area, text));

        PictureBox pictureBox = new PictureBox
        {
            Location = new Point(area.X, area.Y),
            Size = new Size(area.Width, area.Height),
            Image = Image.FromFile(imagePath), // Wczytaj grafikę
            SizeMode = PictureBoxSizeMode.StretchImage,
            BackColor = Color.Transparent // Ustawienie przezroczystości
        };

        areaGraphix.Add(pictureBox); // Dodaj do listy paneli
        backgroundPictureBox.Controls.Add(pictureBox); // Dodaj do tła jako kontrolkę
        pictureBox.Parent = backgroundPictureBox; // Ustawienie rodzica dla przezroczystości
    }


    /// <summary>
    /// Konstruktor gry MagicSchoolGame.
    /// </summary>
    public MagicSchoolGame()
    {
        this.Text = "Gra: Magiczna Szkoła";
        this.ClientSize = new Size(640, 320);
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;

        this.KeyDown += new KeyEventHandler(OnKeyDown);

        CreateMainMenuDropdown(); // Dodanie rozwijanego menu
    }

    /// <summary> /// Wczytuje poziom gry i ustawia odpowiednie elementy. /// </summary>
    private void LoadLevel(int level)
    {
        playerPosition = new Point(50, 50);
        questionAreas.Clear();
        blockedAreas.Clear();
        textAreas.Clear();
        allQuestionsAnswered = false; // Resetowanie statusu pytań
        areaPanels.ForEach(panel => backgroundPictureBox.Controls.Remove(panel));
        areaPanels.Clear();
        areaGraphix.ForEach(control => backgroundPictureBox.Controls.Remove(control));
        areaGraphix.Clear();
        questionAreas.Clear();


        SaveLastVisitedLevel(level);
        ShowLevelIntroduction(level);

        if (backgroundPictureBox != null)
        {
            this.Controls.Remove(backgroundPictureBox);
        }

        backgroundPictureBox = new PictureBox();
        backgroundPictureBox.Dock = DockStyle.Fill;

        switch (level)
        {
            case 1:
                backgroundPictureBox.Image = Image.FromFile("school_map.png");
                CreateQuestionArea(new Rectangle(450, 30, 40, 40),"goblin.png");
                CreateQuestionArea(new Rectangle(200, 30, 40, 40), "goblin.png");
                CreateQuestionArea(new Rectangle(100, 200, 40, 40), "goblin.png");
                CreateQuestionArea(new Rectangle(500, 200, 40, 40), "goblin.png");

                CreateTextArea(new Rectangle(400, 100, 30, 30), "Jeśli dodajesz liczby, które razem mogą stworzyć pełną dziesiątkę, zrób to najpierw! Na przykład: 8 + 7. Z 7 weź 2, żeby zrobić 10 (8 + 2 = 10), a potem dodaj pozostałe 5 (10 + 5 = 15). Gobliny nigdy cię nie złapią, jeśli będziesz działać szybko!", "player_icon.png");
                CreateTextArea(new Rectangle(200, 250, 30, 30), "1 + 9 = 10, 2 + 8 = 10, 3 + 7 = 10, itd.\r\nJeśli znajdziesz takie pary, łatwiej będzie dodawać większe grupy liczb!\"", "player_icon.png");

                CreateBlockedArea(new Rectangle(50, 90, 540, 10));
                CreateBlockedArea(new Rectangle(45, 200, 360, 10));
                CreateBlockedArea(new Rectangle(500, 200, 100, 10));
                CreateBlockedArea(new Rectangle(340, 10, 10, 80));
                CreateBlockedArea(new Rectangle(400, 200, 10, 160));
                break;
            case 2:
                backgroundPictureBox.Image = Image.FromFile("school_map1.png");
                CreateQuestionArea(new Rectangle(160, 100, 40, 40), "roslinka.png");
                CreateQuestionArea(new Rectangle(80, 60, 40, 40), "roslinka.png");
                CreateQuestionArea(new Rectangle(500, 60, 40, 40), "roslinka.png");
                CreateQuestionArea(new Rectangle(420, 60, 40, 40), "roslinka.png");

                CreateTextArea(new Rectangle(350, 250, 30, 30), "Podziel odejmowanie na części. Na przykład: 14 - 9. Odejmij najpierw, żeby zrobić 10 (14 - 4 = 10), a potem odejmij resztę (10 - 5 = 5). Dzięki temu szybciej pokonasz rośliny!", "player_icon.png");
                CreateTextArea(new Rectangle(150, 150, 30, 30), "Zamiast odejmować, zapytaj siebie: ‘Ile muszę dodać do mniejszej liczby, żeby osiągnąć większą?’ Na przykład: 13 - 7. Dodaj 7 + 3, żeby dojść do 10, a potem jeszcze 3, żeby dojść do 13. Wynik to 6!", "player_icon.png");

                CreateBlockedArea(new Rectangle(70, 100, 100, 50));
                CreateBlockedArea(new Rectangle(130, 0, 50, 150));

                CreateBlockedArea(new Rectangle(450, 100, 100, 50));
                CreateBlockedArea(new Rectangle(450, 0, 50, 150));
                break;
            case 3:
                backgroundPictureBox.Image = Image.FromFile("school_map2.png");
                CreateQuestionArea(new Rectangle(160, 200, 40, 40), "duch.png");
                CreateQuestionArea(new Rectangle(80, 60, 40, 40), "duch.png");
                CreateQuestionArea(new Rectangle(500, 60, 40, 40), "duch.png");
                CreateQuestionArea(new Rectangle(420, 60, 40, 40), "duch.png");

                CreateTextArea(new Rectangle(350, 250, 30, 30), "Jeśli liczby są trudne, pomnóż łatwiejsze części i dodaj. Na przykład: 6 × 4. Zrób to jako (6 × 2) + (6 × 2) = 12 + 12 = 24. Egzorcyzm działa szybciej, gdy dzielisz go na etapy!", "player_icon.png");

                CreateBlockedArea(new Rectangle(120, 80, 400, 140));

                break;
            case 4:
                backgroundPictureBox.Image = Image.FromFile("school_map3.png");
                CreateQuestionArea(new Rectangle(160, 200, 40, 40), "portale.png");
                CreateQuestionArea(new Rectangle(80, 60, 40, 40), "portale.png");
                CreateQuestionArea(new Rectangle(500, 60, 40, 40), "portale.png");
                CreateQuestionArea(new Rectangle(420, 60, 40, 40), "portale.png");

                CreateTextArea(new Rectangle(300, 150, 30, 30), "Wyobraź sobie, że masz liczbę do podziału jako grupy rzeczy. Na przykład: 12 ÷ 3. Podziel 12 na 3 równe grupy – każda ma 4. To jak łatanie 3 dziur w rzeczywistości przy użyciu równej liczby run!", "player_icon.png");


                break;
            default:

                MessageBox.Show("Koniec gry! Gratulacje!", "Koniec");
                Application.Exit();
                return;
        }

        backgroundPictureBox.SizeMode = PictureBoxSizeMode.StretchImage;
        this.Controls.Add(backgroundPictureBox);

        playerPictureBox = new PictureBox();
        playerPictureBox.Image = Image.FromFile("player_icon.png");
        playerPictureBox.Size = new Size(playerSize, playerSize);
        playerPictureBox.Location = playerPosition;
        playerPictureBox.BackColor = Color.Transparent;
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
    /// <summary>
    /// Usuwa obszar pytania i odkrywa obszar przejscia na nstepny poziom
    /// </summary>
    /// <param name="index">Numer obszaru ktory ma zostac usuniety</param>
    private void RemoveQuestionArea(int index)
    {
        backgroundPictureBox.Controls.Remove(areaGraphix[index]);
        areaGraphix.RemoveAt(index);
        questionAreas.RemoveAt(index);

        // Sprawdzenie, czy wszystkie pytania zostały rozwiązane
        if (questionAreas.Count == 0)
        {
            allQuestionsAnswered = true;
            levelChangePanel.Visible = true; // Pokazujemy pole zmiany poziomu
        }
    }

    /// <summary>
    /// Wyswietla tutorial do danego poziomu
    /// </summary>
    /// <param name="level">opisuje na ktory poziom gracz sie dostal</param>
    private void ShowLevelIntroduction(int level)
    {
        string introduction;

        switch (level)
        {
            case 1:
                introduction = "Szkoła jest w chaosie! Po korytarzach rozbiegły się gobliny, które sieją zamęt. Profesor Matemagii właśnie nauczył cię zaklęcia dodawania. Twoim zadaniem jest użycie tej nowej magii, aby pokonać gobliny i przywrócić porządek.\r\n\r\nJak to działa?\r\n\r\n    Każdy goblin rzuca w twoją stronę zagadkę matematyczną związaną z dodawaniem.\r\n    Przykład: Goblin krzyczy: \"Ile to jest 7 + 5?\"\r\n    Aby rzucić zaklęcie, musisz podać poprawną odpowiedź. W tym przypadku odpowiedź to 12.\r\n    Jeśli odpowiedź jest poprawna, goblin znika w magicznym pufie!\r\n    Jeśli się pomylisz, goblin cię zaatakuje i stracisz jedno życie.\r\n\r\nRada:\r\nDodawanie to po prostu zliczanie razem dwóch liczb. Możesz sobie wyobrazić, że dodajesz przedmioty w swojej dłoni, aby uzyskać sumę.";
                break;
            case 2:
                introduction = "Szkolne ogrody zostały opanowane przez złośliwe, mięsożerne rośliny. Na szczęście twoja magia odejmowania jest w stanie je powstrzymać! Musisz pomóc ogrodnikom.\r\n\r\nJak to działa?\r\n\r\n    Każda roślina próbuje zaatakować, ale zamiast tego rzuca zagadkę związaną z odejmowaniem.\r\n    Przykład: Roślina syczy: \"Ile to jest 15 - 8?\"\r\n    Twoje zaklęcie odejmowania wymaga poprawnej odpowiedzi, czyli 7.\r\n    Jeśli odpowiesz dobrze, roślina więdnie i znika.\r\n    Zła odpowiedź powoduje, że roślina ugryzie cię, tracisz jedno życie!\r\n\r\nRada:\r\nOdejmowanie to proces \"zabierania\" jednej liczby od drugiej. Możesz sobie wyobrazić, że oddajesz rzeczy, które już miałeś, aby obliczyć wynik.";
                break;
            case 3:
                introduction = "Szkolne podziemia zostały nawiedzone przez duchy! Aby je wypędzić, musisz wykorzystać magię mnożenia, której właśnie nauczyłeś się na lekcjach. Duchy boją się liczb i mnożenia, więc użyj tego przeciwko nim.\r\n\r\nJak to działa?\r\n\r\n    Duchy wyskakują z cienia i rzucają zagadki z mnożeniem.\r\n    Przykład: Duch jęczy: \"Ile to jest 6 × 4?\"\r\n    Twoje zaklęcie mnożenia wymaga odpowiedzi 24, aby duch wrócił do swojego wymiaru.\r\n    Jeśli odpowiesz źle, duch zabiera część twojej energii.\r\n\r\nRada:\r\nMnożenie to szybkie dodawanie. Jeśli masz 6 grup po 4, to oznacza, że masz 24 rzeczy razem. Wyobraź sobie grupki przedmiotów, aby łatwiej rozwiązać zagadkę.";
                break;
            case 4:
                introduction = "Największe zagrożenie dla naszego świata to odchłań, która próbuje przedostać się przez mroczne portale. Aby je zamknąć, musisz wykorzystać magię dzielenia. Rozkładanie dużych liczb na mniejsze części pomoże ci zablokować każdy portal i uratować świat.\r\n\r\nJak to działa?\r\n\r\n    Każdy portal próbuje otworzyć się szerzej, zadając ci zagadkę związaną z dzieleniem.\r\n    Przykład: Portal wibruje i szepcze: \"Ile to jest 20 ÷ 5?\"\r\n    Twoje zaklęcie dzielenia wymaga odpowiedzi 4, aby portal został zamknięty na zawsze.\r\n    Zła odpowiedź powoduje, że portal się rozszerza, a ty tracisz jedno życie.\r\n\r\nRada:\r\nDzielenie to odwrotność mnożenia. Zastanów się, na ile równych części możesz podzielić liczbę, aby uzyskać wynik. Możesz myśleć o tym, jak dzielenie kawałków ciasta między osoby.";
                break;
            default:
                introduction = "Rozpoczynasz nowy poziom. Powodzenia!";
                break;
        }

        MessageBox.Show(introduction, $"Poziom {level}");
    }




    /// <summary>
    /// tworzy pole z zadaniem
    /// </summary>
    /// <param name="area">rozmiar pola</param>
    /// <param name="imagePath">scierzka do obrazu wykorzystanego w polu zadania</param>
    private void CreateQuestionArea(Rectangle area, string imagePath)
    {
        questionAreas.Add(area);

        PictureBox pictureBox = new PictureBox
        {
            Location = new Point(area.X, area.Y),
            Size = new Size(area.Width, area.Height),
            Image = Image.FromFile(imagePath), // Wczytaj grafikę
            SizeMode = PictureBoxSizeMode.StretchImage,
            BackColor = Color.Transparent // Ustawienie przezroczystości
        };

        areaGraphix.Add(pictureBox); // Dodaj do listy paneli
        backgroundPictureBox.Controls.Add(pictureBox); // Dodaj do tła
        pictureBox.Parent = backgroundPictureBox; // Ustawienie rodzica dla przezroczystości
    }

    /// <summary>
    /// tworzy obszar przez ktory nie da sie przejsc
    /// </summary>
    /// <param name="area">wymiary obszaru</param>
    private void CreateBlockedArea(Rectangle area)
    {
        blockedAreas.Add(area);

        Panel panel = new Panel
        {
            Location = new Point(area.X, area.Y),
            Size = new Size(area.Width, area.Height),
            BackColor = Color.Transparent
        };

        areaPanels.Add(panel);
        backgroundPictureBox.Controls.Add(panel);
    }
    /// <summary>
    /// Obsluga sterowania i wyrywanie interakcji z polami
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e">Wcisniety przycisk</param>
    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        Console.WriteLine($"Pozostałe pytania: {questionAreas.Count}");
        Console.WriteLine($"Pozostałe grafiki: {areaGraphix.Count}");

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
        if (allQuestionsAnswered && (levelChangeArea.Contains(playerPosition) || levelChangeArea.Contains(newPosition.X + playerSize - 1, newPosition.Y) || levelChangeArea.Contains(newPosition.X, newPosition.Y + playerSize - 1) || levelChangeArea.Contains(newPosition.X + playerSize - 1, newPosition.Y + playerSize - 1)))
        {
            currentLevel++;
            SaveLastVisitedLevel(currentLevel);
            LoadLevel(currentLevel);
        }
    }


    /// <summary>
    /// Generuje i obsluguje zadania
    /// </summary>
    /// <param name="areaIndex">Numer pola z zadaniem</param>
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
    /// <summary>
    /// Usuwa pola wyswietlajace tekst
    /// </summary>
    /// <param name="areaIndex">Numer pola z tekstem</param>
    private void HideTextArea(int areaIndex)
    {
        if (areaIndex >= 0 && areaIndex < textAreas.Count)
        {
            var textArea = textAreas[areaIndex];
            textAreas.RemoveAt(areaIndex);

            var pictureBox = backgroundPictureBox.Controls
            .OfType<PictureBox>()
            .FirstOrDefault(p => p.Location == new Point(textArea.Area.X, textArea.Area.Y));

            if (pictureBox != null)
            {
                backgroundPictureBox.Controls.Remove(pictureBox);
            }

        }
    }


    /// <summary>
    /// Tworzy Dropdown menu
    /// </summary>
    private void CreateMainMenuDropdown()
    {
        ToolStripDropDownButton menuDropdown = new ToolStripDropDownButton("Menu");
        menuDropdown.DropDownItems.Add("Restart", null, (s, e) => RestartGame());
        menuDropdown.DropDownItems.Add("Wyjdź", null, (s, e) => Application.Exit());

        ToolStrip menuStrip = new ToolStrip();
        menuStrip.Items.Add(menuDropdown);
        this.Controls.Add(menuStrip);
    }
    /// <summary>
    /// restartuje gre
    /// </summary>
    private void RestartGame()
    {
        lives = 5; // Przywrócenie żyć
        LoadLevel(currentLevel);
    }
    /// <summary>
    /// restartuje gre
    /// </summary>
    private void ResetLevel()
    {
        lives = 5; // Przywrócenie pełnej liczby żyć
        LoadLevel(currentLevel); // Ponowne załadowanie bieżącego poziomu
    }
    /// <summary>
    /// Wyswietla okna z tekstem
    /// </summary>
    /// <param name="question">tekst od wyswietlenia</param>
    /// <returns></returns>
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
    /// <summary>
    /// generuje menu glowne
    /// </summary>
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

    /// <summary>
    /// zapisuje ostatni odwiedony poziom
    /// </summary>
    /// <param name="level">numer ostatni odwiedzonego poziomu</param>
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


    /// <summary>
    /// wczytuje ostatni odwiedzony poziom
    /// </summary>
    /// <returns>na wszelki wypadek gdyby cos nie zadzialalo zostanie uruchomiony 1 poziom</returns>
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



    /// <summary>
    /// Wywoluje program
    /// </summary>
    [STAThread]
    public static void Main()
    {
        Application.EnableVisualStyles();
        MagicSchoolGame game = new MagicSchoolGame();
        game.ShowMainMenu();
        Application.Run(game);

    }
}
