using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Calculator
{
    public partial class Calculator : Form
    {
        private string calculation = ""; // Holds the current calculation expression

        // Allows dragging the window
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        const int WM_NCLBUTTONDOWN = 0xA1;
        const int HT_CAPTION = 0x2;

        private HashSet<char> operators = new HashSet<char>() // Set of supported operators
        {
            '+', '-', '×', '÷'
        };

        public Calculator()
        {
            InitializeComponent();
        }

        // Close button click event
        private void close(object sender, EventArgs e)
        {
            Close();
        }

        // Minimize button click event
        private void minimize(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        // Handle button clicks to capture user input
        private void button_Click(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            if (operators.Contains(button.Text[0]))
            {
                if (inputTextBox.Text.Length > 0)
                {
                    if (isLastCharOP())
                    {
                        calculation = calculation.Remove(calculation.Length - 1, 1);
                    }
                }
            }
            if (button.Text[0] == '.')
            {
                if (inputTextBox.Text.Length > 0)
                {
                    if (isLastCharOP())
                    {
                        calculation = calculation.Remove(calculation.Length - 1, 1);
                    }
                }
            }
            calculation += (button).Text;
            inputTextBox.Text = calculation;
        }

        // Clear all button click event
        private void clearBtn_Click(object sender, EventArgs e)
        {
            inputTextBox.Text = "";
            resultTextBox.Text = "";
            calculation = "";
        }

        // Clear most recent operand button click event
        private void clearEntryBtn_click(object sender, EventArgs e)
        {
            if (calculation.Length > 0) // Ensure calculation string is not empty
            {
                while (calculation.Length > 0 && !operators.Contains(calculation[calculation.Length - 1])) // Remove operand until the last operator
                {
                    calculation = calculation.Remove(calculation.Length - 1, 1);
                }
                if (calculation.Length > 1)
                {
                    if (calculation[calculation.Length-2] == '(')
                    {
                        calculation = calculation.Remove(calculation.Length - 1, 1);
                    }
                    else if (operators.Contains(calculation[calculation.Length - 2]))
                    {
                        calculation = calculation.Remove(calculation.Length - 1, 1);
                    }
                }
                if (calculation.Length == 1) calculation = calculation.Remove(calculation.Length - 1, 1);
                inputTextBox.Text = calculation;
            }
        }

        // Backspace button click event
        private void delBtn_Click(object sender, EventArgs e)
        {
            if (calculation.Length > 0)
            {
                calculation = calculation.Remove(calculation.Length - 1, 1); // Remove the last character
            }
            inputTextBox.Text = calculation;
        }

        // Toggle sign of operand button click event
        private void signBtn_Click(object sender, EventArgs e)
        {
            var temp = new StringBuilder();
            while (calculation.Length > 0 && !operators.Contains(calculation[calculation.Length - 1]))
            {
                temp.Insert(0, calculation[calculation.Length - 1]); // Temporarily hold the operand
                calculation = calculation.Remove(calculation.Length - 1, 1); // Remove operand
            }
            if (calculation.Length > 1 && temp[temp.Length - 1] == ')')
            { // Check if operand is negative
                calculation = calculation.Remove(calculation.Length - 1, 1);
                calculation = calculation.Remove(calculation.Length - 1, 1);
                temp = temp.Replace(")(-", ""); // Remove negative sign
                temp = temp.Remove(temp.Length - 1, 1);
                calculation += temp; // Bring back the operand to the calculation
            }
            else
            {
                calculation += $"(-{temp})";
            }

            inputTextBox.Text = calculation;
        }

        // Calculate the result button click event
        private void equalBtn_Click(object sender, EventArgs e)
        {
            string formattedCalculation = calculation.ToString().Replace("×", "*").ToString().Replace("÷", "/"); // Format for Compute method
            if (operators.Contains(formattedCalculation[formattedCalculation.Length - 1])) // Avoid error if the last character is an operand
            {
                formattedCalculation = formattedCalculation.Remove(calculation.Length - 1, 1);
                calculation = calculation.Remove(calculation.Length - 1, 1);
            }

            try
            {
                double roundedResult = Math.Round(Convert.ToDouble(new DataTable().Compute(formattedCalculation, null)), 4); // Round to 4th decimal places
                inputTextBox.Text = (roundedResult > 0) ? $"{roundedResult}" : $"({roundedResult})";
                resultTextBox.Text = calculation;
                calculation = inputTextBox.Text;

            }
            catch (Exception ex)
            {
                inputTextBox.Text = "";
                calculation = "";
                string errorMessage;

                if (ex is OverflowException)
                {
                    errorMessage = "TOO LARGE";
                }
                else
                {
                    errorMessage = "ERROR";
                }

                resultTextBox.Text = errorMessage;
            }
        }

        // Handle changes in input text box
        private void inputTextBox_Changes(object sender, EventArgs e)
        {
            signBtn.Enabled = !isLastCharOP(); // Enable/disable sign button based on the last character
        }

        // Check if the last character in the input text box is an operator
        private bool isLastCharOP()
        {
            if (inputTextBox.Text.Length > 0)
            {
                if (operators.Contains(inputTextBox.Text[inputTextBox.Text.Length - 1]))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true; // If text box is empty, sign button should be enabled
            }
        }

        // Mouse down event to allow dragging the window
        private void Calculator_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }
    }
}
