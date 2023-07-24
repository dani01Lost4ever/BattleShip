using BattleShip2._0.Properties;
using System.Linq;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace BattleShip2._0
{
    public partial class Form1 : Form
    {
        public int dimShip;
        public int numOfShipPos;
        private Button tempButtom;
        private bool dir = false;
        private List<Button> placedButtons = new List<Button>();
        private List<Button> placedAIButton = new List<Button>();
        private Dictionary<int, List<List<Button>>> aiShipPositions = new Dictionary<int, List<List<Button>>>();
        private Dictionary<int, List<List<Button>>> HumanShipPositions = new Dictionary<int, List<List<Button>>>();
        private int counter = 0;
        private int totalIterations = 0;
        private System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();
        private Random random = new Random();
        private List<Button> hitTargets = new List<Button>();
        private bool ChangeColor = false;
        private bool rotation = true;
        private readonly int ButtonL = 80;
        private string control;

        public Form1()
        {
            InitializeComponent();
            button211.BackgroundImage = Resources.Shield2;
            button211.BackgroundImageLayout = ImageLayout.Stretch;
            button211.BackColor = Color.Transparent;
            button211.ForeColor = Color.White;
            button211.Margin = new Padding(0);
            button211.FlatStyle = FlatStyle.Flat;
            button211.FlatAppearance.BorderSize = 0;

            pictureBox2.BackgroundImage = Resources.TITLE;
            pictureBox2.BackgroundImageLayout = ImageLayout.Stretch;
            tableLayoutPanel1.BackgroundImage = Resources.ocean;
            tableLayoutPanel1.BackgroundImageLayout = ImageLayout.Stretch;
            tableLayoutPanel2.BackgroundImage = Resources.ocean;
            tableLayoutPanel2.BackgroundImageLayout = ImageLayout.Stretch;
            flowLayoutPanel1.BackgroundImage = Resources.frame;
            flowLayoutPanel1.BackgroundImageLayout = ImageLayout.Stretch;
            foreach (Button button in tableLayoutPanel2.Controls)
            {
                button.Enabled = false;
                button.Text = "";
                button.Tag = "0";
                button.Margin = new Padding(0);
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderSize = 1;
                button.FlatAppearance.BorderColor = Color.FromArgb(29, 183, 0);
                button.BackColor = Color.Transparent;
            }
            foreach (Button button in tableLayoutPanel1.Controls)
            {
                button.Text = "";
                button.Margin = new Padding(0);
                button.BackColor = Color.LightBlue;
                int alpha = button.BackColor.A - 100; // Decrease opacity by 10 units
                button.BackColor = Color.FromArgb(alpha, button.BackColor);
            }
            foreach (Button button in flowLayoutPanel1.Controls)
            {
                button.FlatStyle = FlatStyle.Flat;
                button.FlatAppearance.BorderSize = 0;
                button.BackgroundImageLayout = ImageLayout.Stretch;
                button.Text = "";
                if (button == button201) button.BackgroundImage = Properties.Resources.battleship1;
                if (button == button202 || button == button203) button.BackgroundImage = Properties.Resources.submarine;
                if (button == button204 || button == button205 || button == button206) button.BackgroundImage = Properties.Resources.corvette;
                if (button == button207 || button == button208 || button == button209 || button == button210) button.BackgroundImage = Properties.Resources.lance;
            }
            PositionAIShips();
        }

        private void button213_Click(object sender, EventArgs e)
        {
            if (rotation == true)
            {
                if (dir == false)
                {
                    dir = true;
                    button213.Text = "↔";
                }
                else
                {
                    dir = false;
                    button213.Text = "↕";
                }
            }
        }
        private void playerButton_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            PositionShip(button, dimShip, dir); // position a ship with length 3 horizontally
            dimShip = 0;
        }

        private void PositionShip(Button button, int length, bool isHorizontal)
        {
            int row = tableLayoutPanel1.GetRow(button); // get the row of the button in the flow layout panel
            int col = tableLayoutPanel1.GetColumn(button); // get the column of the button in the flow layout panel
            if (length == 0)
            {
                MessageBox.Show("Select a ship");
                return;
            }
            // check if the ship can fit in the grid starting from the clicked button
            if (isHorizontal)
            {
                if (col + length > tableLayoutPanel1.ColumnCount)
                {
                    MessageBox.Show("Ship does not fit in the grid!");
                    return;
                }
            }
            else
            {
                if (row + length > tableLayoutPanel1.RowCount)
                {
                    MessageBox.Show("Ship does not fit in the grid!");
                    return;
                }
            }

            // check if the grid cells are unoccupied
            for (int i = 0; i < length; i++)
            {
                int r = isHorizontal ? row : row + i;
                int c = isHorizontal ? col + i : col;
                Button shipButton = (Button)tableLayoutPanel1.GetControlFromPosition(c, r);
                if (shipButton.Tag != null && shipButton.Tag.ToString() == "occupied")
                {
                    MessageBox.Show("Another ship is occupying this cell!");
                    return;
                }
            }
            // position the ship on the grid
            //List<Button> shipPositions = new List<Button>();
            //for (int i = 0; i < length; i++)
            //{
            //    int r = isHorizontal ? row : row + i;
            //    int c = isHorizontal ? col + i : col;
            //    Button shipButton = (Button)tableLayoutPanel1.GetControlFromPosition(c, r);
            //    shipButton.BackColor = Color.Black;
            //    shipButton.Tag = "occupied";
            //    shipButton.Enabled = false;
            //    shipPositions.Add(shipButton);
            //    placedButtons.Add(shipButton);
            //}
            int partWidth, partHeight;
            Image shipImage = Properties.Resources.lance;
            List<Button> shipPositions = new List<Button>();
            if (length == 4) shipImage = Properties.Resources.battleship1;
            if (length == 3) shipImage = Properties.Resources.submarine;
            if (length == 2) shipImage = Properties.Resources.corvette;
            if (length == 1) shipImage = Properties.Resources.lance;
            if (!isHorizontal)
            {
                shipImage.RotateFlip(RotateFlipType.Rotate90FlipXY);
                partWidth = shipImage.Width;
                partHeight = shipImage.Height / length;
            }
            else
            {
                partWidth = shipImage.Width / length;
                partHeight = shipImage.Height;
            }
            for (int i = 0; i < length; i++)
            {
                int r = isHorizontal ? row : row + i;
                int c = isHorizontal ? col + i : col;
                Button shipButton = (Button)tableLayoutPanel1.GetControlFromPosition(c, r);
                // Create a new Bitmap for each part of the ship image
                Bitmap partImage = new Bitmap(partWidth, partHeight);
                using (Graphics g = Graphics.FromImage(partImage))
                {
                    Rectangle srcRect = isHorizontal
                        ? new Rectangle(i * partWidth, 0, partWidth, partHeight)
                        : new Rectangle(0, i * partHeight, partWidth, partHeight);
                    g.DrawImage(shipImage, new Rectangle(0, 0, partWidth, partHeight), srcRect, GraphicsUnit.Pixel);
                }
                shipButton.BackgroundImage = partImage;
                shipButton.BackgroundImageLayout = ImageLayout.Stretch;
                shipButton.Tag = "occupied";
                shipButton.Enabled = false;
                shipButton.BackColor = Color.LightBlue;
                int alpha = shipButton.BackColor.A - 100; // Decrease opacity by 10 units
                shipButton.BackColor = Color.FromArgb(alpha, shipButton.BackColor);
                //shipButton.BackColor = Color.White;
                shipPositions.Add(shipButton);
                placedButtons.Add(shipButton);
            }
            foreach (Button buttonDisabilita in flowLayoutPanel1.Controls)
            {
                if (buttonDisabilita == tempButtom)
                {
                    buttonDisabilita.Enabled = false;
                    buttonDisabilita.BackColor = Color.FromArgb(30, Color.LightSlateGray);
                    dimShip = 0;
                }
            }
            if (!HumanShipPositions.ContainsKey(length))
            {
                HumanShipPositions[length] = new List<List<Button>>();
            }
            HumanShipPositions[length].Add(shipPositions);

            numOfShipPos++;
        }

        private void button201_Click(object sender, EventArgs e)
        { dimShip = 4; tempButtom = button201; }
        private void button202_Click(object sender, EventArgs e)
        { dimShip = 3; tempButtom = button202; }
        private void button203_Click(object sender, EventArgs e)
        { dimShip = 3; tempButtom = button203; }
        private void button204_Click(object sender, EventArgs e)
        { dimShip = 2; tempButtom = button204; }
        private void button205_Click(object sender, EventArgs e)
        { dimShip = 2; tempButtom = button205; }
        private void button206_Click(object sender, EventArgs e)
        { dimShip = 2; tempButtom = button206; }
        private void button207_Click(object sender, EventArgs e)
        { dimShip = 1; tempButtom = button207; }
        private void button208_Click(object sender, EventArgs e)
        { dimShip = 1; tempButtom = button208; }
        private void button209_Click(object sender, EventArgs e)
        { dimShip = 1; tempButtom = button209; }
        private void button210_Click(object sender, EventArgs e)
        { dimShip = 1; tempButtom = button210; }

        private void button211_Click(object sender, EventArgs e)
        {
            if (numOfShipPos == 10)
            {
                foreach (Button button in tableLayoutPanel1.Controls)
                {
                    button.Enabled = false;
                    button.FlatStyle = FlatStyle.Flat;
                    button.FlatAppearance.BorderSize = 1;
                    button.FlatAppearance.BorderColor = Color.FromArgb(29, 183, 0);
                    button.BackColor = Color.Transparent;
                }
                foreach (Button button in flowLayoutPanel1.Controls)
                {
                    button.BackColor = Color.Transparent;
                }
                Random randomFor = new Random();
                totalIterations = 50;
                counter = 0;
                // Start the timer
                timer.Interval = 100; // Set the delay time (in milliseconds)
                timer.Tick += Timer_Tick;
                timer.Start();
            }
            else MessageBox.Show("Position all the ships, you're gonna need them all!!!");
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (counter < totalIterations)
            {
                // Generate a random number
                Random random = new Random();
                int temp = random.Next(2);
                // Update the label
                if (temp == 0)
                {
                    label4.Text = "HUMAN";
                    label4.BackColor = Color.LightSalmon;
                }
                else
                {
                    label4.Text = "AI";
                    label4.BackColor = Color.LightSeaGreen;
                }
                counter++;
            }
            else
            {
                // Stop the timer
                timer.Stop();
                rotation = false;
                if (label4.Text == "HUMAN")
                {
                    foreach (Button button in tableLayoutPanel2.Controls)
                    {
                        button.Enabled = true;
                    }
                }
                else
                {
                    AIMove();
                    foreach (Button button in tableLayoutPanel2.Controls)
                    {
                        button.Enabled = true;
                    }
                }
                button211.Enabled = false;
                button213.Enabled = false;
            }
        }

        private void Button_MouseEnter(object sender, EventArgs e)
        {
            var button = (Button)sender;
            int row = tableLayoutPanel1.GetRow(button);
            int col = tableLayoutPanel1.GetColumn(button);

            bool canHighlight = true;
            if (dimShip != 0)
            {
                for (int i = 0; i < dimShip; i++)
                {
                    int r = dir ? row : row + i;
                    int c = dir ? col + i : col;
                    if (r >= 0 && r < 10 && c >= 0 && c < 10)
                    {
                        Button shipButton = (Button)tableLayoutPanel1.GetControlFromPosition(c, r);
                        if (placedButtons.Contains(shipButton))
                        {
                            canHighlight = false;
                            break;
                        }
                        shipButton.BackColor = Color.BlueViolet;
                    }
                    else
                    {
                        canHighlight = false;
                        break;
                    }
                }
            }
            else
            {
                canHighlight = false;
            }

            if (!canHighlight)
            {
                for (int i = 0; i < dimShip; i++)
                {
                    int r = dir ? row : row + i;
                    int c = dir ? col + i : col;
                    if (r >= 0 && r < 10 && c >= 0 && c < 10)
                    {
                        Button shipButton = (Button)tableLayoutPanel1.GetControlFromPosition(c, r);
                        if (!placedButtons.Contains(shipButton))
                        {
                            //shipButton.BackColor = Color.White;
                            shipButton.BackColor = Color.LightBlue;
                            int alpha = shipButton.BackColor.A - 100; // Decrease opacity by 10 units
                            shipButton.BackColor = Color.FromArgb(alpha, shipButton.BackColor);
                        }
                    }
                }
            }
        }

        private void Button_MouseLeave(object sender, EventArgs e)
        {
            var button = (Button)sender;
            int row = tableLayoutPanel1.GetRow(button);
            int col = tableLayoutPanel1.GetColumn(button);

            if (dimShip != 0)
            {
                for (int i = 0; i < dimShip; i++)
                {
                    int r = dir ? row : row + i;
                    int c = dir ? col + i : col;
                    if (r >= 0 && r < 10 && c >= 0 && c < 10)
                    {
                        Button shipButton = (Button)tableLayoutPanel1.GetControlFromPosition(c, r);
                        if (!placedButtons.Contains(shipButton))
                        {
                            //shipButton.BackColor = Color.White;
                            shipButton.BackColor = Color.LightBlue;
                            int alpha = shipButton.BackColor.A - 100; // Decrease opacity by 10 units
                            shipButton.BackColor = Color.FromArgb(alpha, shipButton.BackColor);
                        }
                    }
                }
            }
        }

        private void PositionAIShips()
        {
            Random rand = new Random();

            // Define the ship sizes
            int[] shipSizes = { 4, 3, 3, 2, 2, 2, 1, 1, 1, 1 };
            int[] shipSizesDictionary = { 4, 3, 2, 1 };

            // Initialize the aiShipPositions dictionary
            foreach (int shipSize in shipSizesDictionary)
            {
                aiShipPositions.Add(shipSize, new List<List<Button>>());
            }

            // Place each ship
            foreach (int shipSize in shipSizes)
            {
                bool shipPlaced = false;

                // Try to place the ship in a valid location
                while (!shipPlaced)
                {
                    // Choose a random starting location and direction
                    int row = rand.Next(0, 10);
                    int col = rand.Next(0, 10);
                    bool dir = rand.Next(0, 2) == 0;

                    // Check if the ship fits in the chosen direction
                    bool fits = true;

                    for (int i = 0; i < shipSize; i++)
                    {
                        int r = dir ? row : row + i;
                        int c = dir ? col + i : col;
                        Button shipButton = (Button)tableLayoutPanel2.GetControlFromPosition(c, r);
                        if (r >= 10 || c >= 10 || shipButton.Tag != "0")
                        {
                            fits = false;
                            break;
                        }
                    }

                    // If the ship fits, place it and mark the cells as occupied
                    if (fits)
                    {
                        List<Button> buttons = new List<Button>();
                        for (int i = 0; i < shipSize; i++)
                        {
                            int r = dir ? row : row + i;
                            int c = dir ? col + i : col;
                            Button shipButton = (Button)tableLayoutPanel2.GetControlFromPosition(c, r);
                            shipButton.Tag = shipSize;
                            placedAIButton.Add(shipButton);
                            buttons.Add(shipButton);
                        }
                        aiShipPositions[shipSize].Add(buttons);
                        shipPlaced = true;
                    }
                }
            }
        }

        private async void OnClickAIShip(object sender, EventArgs e)
        {
            Button clickedButton = (Button)sender;
            dataGridView1.Rows.Clear();
            foreach (var entry in aiShipPositions)
            {
                int size = entry.Key;
                List<List<Button>> positions = entry.Value;
                dataGridView1.Rows.Add(($"Ship size: {size}"), ($"Positions: {string.Join(", ", positions.Select(ship => string.Join(" ", ship.Select(button => button.Name))))}"));
            }

            // Check if the button is part of a ship
            int shipSize = Convert.ToInt32(clickedButton.Tag);
            if (clickedButton.Tag == "0")
            {
                clickedButton.FlatStyle = FlatStyle.Flat;
                clickedButton.FlatAppearance.BorderSize = 0;
                clickedButton.BackgroundImageLayout = ImageLayout.Stretch;
                clickedButton.Text = "";
                clickedButton.BackgroundImage = Resources.miss2;
                clickedButton.Enabled = false;
                //MessageBox.Show("Miss!");
                AIMove();
                return;
            }

            bool isHit = false;
            foreach (List<Button> ship in aiShipPositions[shipSize])
            {
                if (ship.Contains(clickedButton))
                {
                    isHit = true;
                    //clickedButton.BackColor = Color.Yellow;
                    clickedButton.Enabled = false;
                    clickedButton.BackgroundImageLayout = ImageLayout.Stretch;
                    clickedButton.BackgroundImage = Resources.hit2;
                    //shipSize = ship.Count; // Get the size of the ship
                    ship.Remove(clickedButton);
                    if (ship.Count == 0)
                    {
                        MessageBox.Show($"You sunk the AI's {shipSize}-unit ship!");
                        aiShipPositions[shipSize].Remove(ship);
                    }
                    else
                    {
                        //MessageBox.Show("Hit!");
                    }
                    AIMove();
                    break;
                }
            }

            // Check if all of the AI's ships have been sunk
            bool allShipsSunk = true;
            foreach (List<List<Button>> ship in aiShipPositions.Values)
            {
                if (ship.Count != 0)
                {
                    allShipsSunk = false;
                    break;
                }
            }
            if (allShipsSunk)
            {
                DialogResult result = MessageBox.Show("End", "Congratulations, you have won the game! Do you wanna play again?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                foreach (Button button in tableLayoutPanel2.Controls)
                {
                    button.Enabled = false;
                }
                if (result == DialogResult.Yes)
                {
                    Application.Restart();
                }
            }
        }

        private void AIMove()
        {
            List<Button> availableMoves = GetAvailableMoves();
            Button chosenButton;
            if (hitTargets.Count > 0)
            {
                // Targeting phase
                chosenButton = ChooseTargetMove(availableMoves);
            }
            else
            {
                // Hunting phase
                chosenButton = ChooseHuntMove(availableMoves);
            }
            if (chosenButton != null)
            {
                if (IsShip(chosenButton))
                {
                    chosenButton.Text = "Hit";
                    chosenButton.Font = new Font(chosenButton.Font.FontFamily, 1);
                    chosenButton.BackgroundImageLayout = ImageLayout.Stretch;
                    Image hitImage = Resources.hit2;
                    Bitmap bmp = new Bitmap(chosenButton.BackgroundImage, chosenButton.Width, chosenButton.Height);
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.DrawImage(hitImage, 0, 0, chosenButton.Width, chosenButton.Height);
                    }
                    chosenButton.BackgroundImage = bmp;
                    //chosenButton.BackgroundImage = Resources.hit;
                    hitTargets.Add(chosenButton);
                    UpdateShipState(chosenButton);
                }
                else
                {
                    chosenButton.FlatStyle = FlatStyle.Flat;
                    chosenButton.FlatAppearance.BorderSize = 0;
                    chosenButton.BackgroundImageLayout = ImageLayout.Stretch;
                    chosenButton.BackgroundImage = Resources.miss2;
                    chosenButton.Font = new Font(chosenButton.Font.FontFamily, 1);
                    chosenButton.Text = "Miss";
                }

                int totalLists = 0;
                foreach (var kvp in HumanShipPositions)
                {
                    totalLists += kvp.Value.Count;
                }

                if (totalLists == 0)
                {
                    DialogResult result = MessageBox.Show("End", "EZ, The AI has won the game! Do you wanna play again?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    foreach (Button button in tableLayoutPanel1.Controls)
                    {
                        button.Text = "Finish";
                        button.Font = new Font(button.Font.FontFamily, 1);
                    }
                    if (result == DialogResult.Yes)
                    {
                        Application.Restart();
                    }

                }
            }
        }
        private List<Button> GetAvailableMoves()
        {
            List<Button> availableMoves = new List<Button>();

            foreach (Control control in tableLayoutPanel1.Controls)
            {
                if (control is Button button && button.Text != "Hit" && button.Text != "Miss" && button.Text != "Finish")
                {
                    availableMoves.Add(button);
                }
            }
            return availableMoves;
        }

        private Button ChooseHuntMove(List<Button> availableMoves)
        {
            // Choose a random move from the available moves
            if (availableMoves.Count > 0)
            {
                int index = random.Next(availableMoves.Count);
                return availableMoves[index];
            }
            return null;
        }

        private Button ChooseTargetMove(List<Button> availableMoves)
        {
            List<Button> consecutiveMoves = new List<Button>();

            foreach (Button hitTarget in hitTargets)
            {
                if (hitTargets.Count >= 2)
                {
                    // Check for two or more consecutive hits in a row
                    if (tableLayoutPanel1.GetRow(hitTargets[0]) == tableLayoutPanel1.GetRow(hitTargets[1]))
                    {
                        List<Button> rowButtons = GetConsecutiveButtons(hitTarget, availableMoves, true);
                        if (rowButtons.Count >= 3)
                        {
                            consecutiveMoves.Add(rowButtons[1]);
                        }
                        else if (rowButtons.Count >= 4)
                        {
                            consecutiveMoves.Add(rowButtons[2]);
                        }
                    }

                    if (tableLayoutPanel1.GetColumn(hitTargets[0]) == tableLayoutPanel1.GetColumn(hitTargets[1]))
                    {
                        // Check for two or more consecutive hits in a column
                        List<Button> colButtons = GetConsecutiveButtons(hitTarget, availableMoves, false);
                        if (colButtons.Count >= 3)
                        {
                            consecutiveMoves.Add(colButtons[1]);
                        }
                        else if (colButtons.Count >= 4)
                        {
                            consecutiveMoves.Add(colButtons[2]);
                        }
                    }
                }
            }
            // Prioritize consecutive moves
            if (consecutiveMoves.Count > 0)
            {
                return consecutiveMoves[0];
            }
            // Check adjacent buttons if no consecutive moves found
            foreach (Button hitTarget in hitTargets)
            {
                List<Button> adjacentButtons = GetAdjacentButtons(hitTarget);
                foreach (Button adjacentButton in adjacentButtons)
                {
                    if (availableMoves.Contains(adjacentButton))
                    {
                        return adjacentButton;
                    }
                }
            }

            return null;
        }

        private List<Button> GetConsecutiveButtons(Button startButton, List<Button> availableMoves, bool isRow)
        {
            int startRow = tableLayoutPanel1.GetRow(startButton);
            int startCol = tableLayoutPanel1.GetColumn(startButton);
            List<Button> consecutiveButtons = new List<Button> { startButton };

            for (int i = 1; i < 4; i++)
            {
                int newRow = isRow ? startRow : startRow + i;
                int newCol = isRow ? startCol + i : startCol;

                if (newRow < tableLayoutPanel1.RowCount && newCol < tableLayoutPanel1.ColumnCount)
                {
                    Button nextButton = tableLayoutPanel1.GetControlFromPosition(newCol, newRow) as Button;
                    if (nextButton != null && availableMoves.Contains(nextButton))
                    {
                        consecutiveButtons.Add(nextButton);
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return consecutiveButtons;
        }

        private List<Button> GetAdjacentButtons(Button button)
        {
            // Implement logic to get the adjacent buttons (up, down, left, right) of the given button
            List<Button> adjacentButtons = new List<Button>();
            TableLayoutPanelCellPosition position = tableLayoutPanel1.GetPositionFromControl(button);
            int row = position.Row;
            int column = position.Column;
            // Up
            if (row > 0)
            {
                Button upButton = tableLayoutPanel1.GetControlFromPosition(column, row - 1) as Button;
                if (upButton != null) adjacentButtons.Add(upButton);
            }
            // Down
            if (row < tableLayoutPanel1.RowCount - 1)
            {
                Button downButton = tableLayoutPanel1.GetControlFromPosition(column, row + 1) as Button;
                if (downButton != null) adjacentButtons.Add(downButton);
            }
            // Left
            if (column > 0)
            {
                Button leftButton = tableLayoutPanel1.GetControlFromPosition(column - 1, row) as Button;
                if (leftButton != null) adjacentButtons.Add(leftButton);
            }
            // Right
            if (column < tableLayoutPanel1.ColumnCount - 1)
            {
                Button rightButton = tableLayoutPanel1.GetControlFromPosition(column + 1, row) as Button;
                if (rightButton != null) adjacentButtons.Add(rightButton);
            }
            return adjacentButtons;
        }

        private bool IsShip(Button button)
        {
            foreach (var shipList in HumanShipPositions.Values)
            {
                foreach (var ship in shipList)
                {
                    if (ship.Contains(button))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private void UpdateShipState(Button button)
        {
            foreach (var entry in HumanShipPositions)
            {
                int size = entry.Key;
                List<List<Button>> positions = entry.Value;

                // Loop through each ship of the current size
                foreach (List<Button> ship in positions)
                {
                    // If the current ship contains the clicked button
                    if (ship.Contains(button))
                    {
                        // Check if the ship is destroyed
                        bool isDestroyed = true;
                        foreach (Button shipButton in ship)
                        {
                            if (!hitTargets.Contains(shipButton))
                            {
                                isDestroyed = false;
                                break;
                            }
                        }
                        // If the ship is destroyed, remove its buttons from hitTargets
                        if (isDestroyed)
                        {
                            foreach (Button shipButton in ship)
                            {
                                hitTargets.Remove(shipButton);
                            }
                            HumanShipPositions[size].Remove(ship);
                        }
                        // Exit the method after updating the ship state
                        return;
                    }
                }
            }
        }

        private void button212_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Visible) dataGridView1.Visible = false;
            else dataGridView1.Visible = true;

            if (ChangeColor == false)
            {
                foreach (int shipSize in aiShipPositions.Keys)
                {
                    foreach (List<Button> shipCells in aiShipPositions[shipSize])
                    {
                        foreach (Button cellButton in shipCells)
                        {
                            if (cellButton.Enabled)
                            {
                                // Set the background color of the cell button based on the ship size
                                if (shipSize == 4) cellButton.BackColor = Color.Red;
                                if (shipSize == 3) cellButton.BackColor = Color.Green;
                                if (shipSize == 2) cellButton.BackColor = Color.Purple;
                                if (shipSize == 1) cellButton.BackColor = Color.Pink;
                            }
                        }
                    }
                }
                ChangeColor = true;
            }
            else
            {
                foreach (Button button in tableLayoutPanel2.Controls)
                {
                    button.BackColor = Color.Transparent;
                }
                ChangeColor = false;
            }
        }
    }
}