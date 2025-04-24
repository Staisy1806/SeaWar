using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace SeaWars
{
    public partial class Form1 : Form
    {
        public const int mapSize = 10;
        public int cellSize = 35;

        public string alphabet = "АБВГДЕЖЗИК";

        public int[,] myMap = new int[mapSize, mapSize];
        public int[,] enemyMap = new int[mapSize, mapSize];

        public Button[,] myButtons = new Button[mapSize, mapSize];
        public Button[,] enemyButtons = new Button[mapSize, mapSize];

        private Queue<int> shipsQueue = new Queue<int>(new int[] { 4, 3, 3, 2, 2, 2, 1, 1, 1, 1 });
        private List<(int, int)> currentShipCells = new List<(int, int)>();
        private int currentShipSize = 0;
        private bool isPlacingShips = true;
        public static Form1 currentForm;
        private Button startButton;
        private Button backButton;
        public Bot bot;

        private Dictionary<int, int> expectedShips = new Dictionary<int, int>()
        {
            { 4, 1 },
            { 3, 2 },
            { 2, 3 },
            { 1, 4 }
        };

        public Form1()
        {
            if (currentForm == null)
            {
                currentForm = this;
            }
            else
            {
                currentForm = this;
            }

            this.Text = "Морський бій";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Paint += new PaintEventHandler(DrawBackground);

            Init(); // 100% викликається
        }

        private void DrawBackground(object sender, PaintEventArgs e)
        {
            using (LinearGradientBrush brush = new LinearGradientBrush(
                new Point(0, 0), new Point(this.Width, 0),  // Початок і кінець лінії градієнту
                Color.DarkKhaki, Color.Firebrick))            // Початковий і кінцевий колір
            {
                e.Graphics.FillRectangle(brush, this.ClientRectangle);
            }
        }

        public void QuickSort(int[] array, int low, int high)
        {
            if (low < high)
            {
                int pi = Partition(array, low, high);
                QuickSort(array, low, pi - 1);
                QuickSort(array, pi + 1, high);
            }
        }

        private int Partition(int[] array, int low, int high)
        {
            int pivot = array[high];
            int i = low - 1;

            for (int j = low; j < high; j++)
            {
                if (array[j] < pivot)
                {
                    i++;
                    Swap(array, i, j);
                }
            }

            Swap(array, i + 1, high);
            return i + 1;
        }

        private void Swap(int[] array, int i, int j)
        {
            int temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }

        public void Init()
        {
            // Очищаємо попередні елементи
            foreach (var btn in myButtons)
            {
                if (btn != null) this.Controls.Remove(btn);
            }
            foreach (var btn in enemyButtons)
            {
                if (btn != null) this.Controls.Remove(btn);
            }

            // Перезапуск всіх змінних і ініціалізація карти
            isPlacingShips = true;
            shipsQueue = new Queue<int>(new int[] { 4, 3, 3, 2, 2, 2, 1, 1, 1, 1 });
            currentShipCells.Clear();
            currentShipSize = shipsQueue.Dequeue();

            myMap = new int[mapSize, mapSize];
            enemyMap = new int[mapSize, mapSize];
            myButtons = new Button[mapSize, mapSize];
            enemyButtons = new Button[mapSize, mapSize];

            CreateMaps();
            CreateControlButtons();
        }
        private void BtnRetry_Click(object sender, EventArgs e)
        {
            // Створюємо нову гру
            Form1 newGameForm = new Form1();
            Form1.currentForm = newGameForm;
            newGameForm.StartPosition = FormStartPosition.CenterScreen;
            newGameForm.Show();
            this.Close();
        }

        private void CreateMaps()
        {
            this.Width = mapSize * 2 * cellSize + 180;
            this.Height = (mapSize + 4) * cellSize + 50;

            for (int i = 0; i <= mapSize; i++)
            {
                for (int j = 0; j <= mapSize; j++)
                {
                    if (i == 0 && j == 0) continue;
                    if (i == 0 || j == 0)
                    {
                        Label label = new Label()
                        {
                            Text = i == 0 ? alphabet[j - 1].ToString() : i.ToString(),
                            Location = new Point(j * cellSize, i * cellSize + 25),
                            Size = new Size(cellSize, cellSize),
                            TextAlign = ContentAlignment.MiddleCenter,
                            Font = new Font("Arial", 10, FontStyle.Bold),
                            ForeColor = Color.White,
                            BackColor = Color.Transparent
                        };
                        this.Controls.Add(label);
                    }
                    else
                    {
                        int rowIndex = i - 1;
                        int colIndex = j - 1;

                        Button myButton = new Button()
                        {
                            Location = new Point(j * cellSize, i * cellSize + 25),
                            Size = new Size(cellSize, cellSize),
                            BackColor = Color.White,
                            FlatStyle = FlatStyle.Flat
                        };
                        myButton.Click += (sender, e) => PlaceShip(rowIndex, colIndex, myButton);
                        myButtons[rowIndex, colIndex] = myButton;
                        this.Controls.Add(myButton);
                    }
                }
            }

            for (int i = 0; i <= mapSize; i++)
            {
                for (int j = 0; j <= mapSize; j++)
                {
                    if (i == 0 && j == 0) continue;
                    if (i == 0 || j == 0)
                    {
                        Label label = new Label()
                        {
                            Text = i == 0 ? alphabet[j - 1].ToString() : i.ToString(),
                            Location = new Point(mapSize * cellSize + 40 + j * cellSize, i * cellSize + 25),
                            Size = new Size(cellSize, cellSize),
                            TextAlign = ContentAlignment.MiddleCenter,
                            Font = new Font("Arial", 10, FontStyle.Bold),
                            ForeColor = Color.White,
                            BackColor = Color.Transparent
                        };
                        this.Controls.Add(label);
                    }
                    else
                    {
                        int rowIndex = i - 1;
                        int colIndex = j - 1;

                        Button enemyButton = new Button()
                        {
                            Location = new Point(mapSize * cellSize + 40 + j * cellSize, i * cellSize + 25),
                            Size = new Size(cellSize, cellSize),
                            BackColor = Color.White,
                            FlatStyle = FlatStyle.Flat
                        };
                        enemyButton.Click += (sender, e) => ShootAtEnemy(rowIndex, colIndex);
                        enemyButtons[rowIndex, colIndex] = enemyButton;
                        this.Controls.Add(enemyButton);
                    }
                }
            }
        }

        private void CreateControlButtons()
        {
            int bottomOffset = this.Height - 80;

            startButton = new Button()
            {
                Text = "Почати гру",
                Location = new Point(30, bottomOffset),
                Size = new Size(120, 35),
                Font = new Font("Arial", 9, FontStyle.Bold),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false
            };
            startButton.Click += (sender, e) => StartGame();
            this.Controls.Add(startButton);

            backButton = new Button()
            {
                Text = "Назад",
                Location = new Point(this.Width - 150, bottomOffset),
                Size = new Size(100, 35),
                Font = new Font("Arial", 9, FontStyle.Bold),
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            backButton.Click += (sender, e) => BackToMainMenu();
            this.Controls.Add(backButton);
        }

        private void PlaceShip(int row, int col, Button button)
        {
            if (!isPlacingShips || myMap[row, col] != 0)
            {
                return;
            }

            myMap[row, col] = 1;
            button.BackColor = Color.FromArgb(0xBF, 0x21, 0x21);
            currentShipCells.Add((row, col));

            if (currentShipCells.Count == currentShipSize)
            {
                currentShipCells.Clear();
                if (shipsQueue.Count > 0)
                {
                    currentShipSize = shipsQueue.Dequeue();
                }
                else
                {
                    isPlacingShips = false;
                    startButton.Enabled = true;
                }
            }
        }

        private bool CheckWin(int[,] map)
        {
            // Перевірка, чи є ще кораблі на полі
            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    if (map[i, j] == 1) // Якщо є хоча б одна клітинка з кораблем
                    {
                        return false; // Якщо є хоча б один корабель, то гра не завершена
                    }
                }
            }
            return true; // Якщо немає кораблів на полі, то гравець виграв
        }
        private void ShootAtEnemy(int row, int col)
        {
            // Перевірка, чи є кораблі на полі
            if (isPlacingShips) return;  // Якщо кораблі ще не розставлені, не проводимо жодної дії

            if (enemyMap[row, col] == -1 || enemyMap[row, col] == -2)
                return;

            bool hit = enemyMap[row, col] == 1;

            if (hit)
            {
                enemyMap[row, col] = -1;
                enemyButtons[row, col].BackColor = Color.FromArgb(0xBF, 0x21, 0x21);
                enemyButtons[row, col].Text = "X";

                if (CheckWin(enemyMap))
                {
                    MessageBox.Show("Ви виграли!");
                    EndGame();  // Завершуємо гру, якщо виграв гравець
                    return;     // Після завершення гри виходимо з методу, щоб не давати боту робити хід
                }
            }
            else
            {
                enemyMap[row, col] = -2;
                enemyButtons[row, col].BackColor = Color.FromArgb(0x11, 0x13, 0x14);
            }

            // Перевіряємо, чи не виграв гравець, перед тим як дати боту шанс зробити хід
            if (CheckWin(myMap))
            {
                MessageBox.Show("Бот виграв!");
                EndGame();
                return;  // Після завершення гри виходимо з методу, щоб не викликати хід бота
            }

            bot.Shoot();
        }

        private void StartGame()
        {
            if (isPlacingShips)
            {
                MessageBox.Show("Розмістіть всі кораблі перед початком гри!");
                return;
            }

            if (!ValidateShipPlacement())
            {
                MessageBox.Show("Невірне розміщення кораблів!");
                Init(); // Скидаємо все і дозволяємо розміщення заново
                return;
            }

            // Додаємо повідомлення про початок гри
            MessageBox.Show("Гра почалась!");

            startButton.Enabled = false;
            bot = new Bot(enemyMap, myMap, enemyButtons, myButtons);
            bot.ConfigureShips();
        }
        private void BackToMainMenu()
        {
            // Логіка для повернення в головне меню
            var homeForm = Application.OpenForms["Home"];
            if (homeForm == null)
            {
                Home home = new Home();
                home.Show();
            }
            else
            {
                homeForm.Show();
            }
        }


        private bool ValidateShipPlacement()
        {
            bool[,] visited = new bool[mapSize, mapSize];
            Dictionary<int, int> foundShips = new Dictionary<int, int>();

            for (int i = 0; i < mapSize; i++)
            {
                for (int j = 0; j < mapSize; j++)
                {
                    if (myMap[i, j] == 1 && !visited[i, j])
                    {
                        List<(int, int)> currentShip = new List<(int, int)>();
                        TraverseShip(i, j, visited, currentShip);

                        int shipSize = currentShip.Count;
                        if (!expectedShips.ContainsKey(shipSize)) return false;

                        foundShips[shipSize] = foundShips.ContainsKey(shipSize) ? foundShips[shipSize] + 1 : 1;
                    }
                }
            }

            foreach (var ship in expectedShips)
            {
                if (foundShips.ContainsKey(ship.Key) && foundShips[ship.Key] != ship.Value)
                {
                    return false;
                }
            }
            return true;
        }
        public void ShellSort(int[] array)
        {
            int n = array.Length;
            int gap = n / 2;

            while (gap > 0)
            {
                for (int i = gap; i < n; i++)
                {
                    int temp = array[i];
                    int j = i;

                    while (j >= gap && array[j - gap] > temp)
                    {
                        array[j] = array[j - gap];
                        j -= gap;
                    }

                    array[j] = temp;
                }

                gap /= 2;
            }
        }
        private void SelectionSort(int[] arr)
        {
            int n = arr.Length;
            for (int i = 0; i < n - 1; i++)
            {
                int minIndex = i;
                for (int j = i + 1; j < n; j++)
                {
                    if (arr[j] < arr[minIndex])
                    {
                        minIndex = j;
                    }
                }
                int temp = arr[minIndex];
                arr[minIndex] = arr[i];
                arr[i] = temp;
            }
        }

        private void TraverseShip(int i, int j, bool[,] visited, List<(int, int)> currentShip)
        {
            if (i < 0 || j < 0 || i >= mapSize || j >= mapSize || visited[i, j] || myMap[i, j] == 0)
                return;

            visited[i, j] = true;
            currentShip.Add((i, j));

            TraverseShip(i + 1, j, visited, currentShip);
            TraverseShip(i - 1, j, visited, currentShip);
            TraverseShip(i, j + 1, visited, currentShip);
            TraverseShip(i, j - 1, visited, currentShip);
        }
        private void EndGame()
        {
            // Зберігаємо посилання на поточну форму, бо після Close() вона буде знищена
            Form current = this;

            // Створюємо форму Vopros перед закриттям Form1
            Vopros voprosForm = new Vopros();

            // Додаємо обробник закриття поточної форми
            this.FormClosed += (s, e) =>
            {
                voprosForm.Show(); // Показуємо Vopros лише після повного закриття Form1
            };

            this.Close(); // Закриває Form1
        }
    }
}
