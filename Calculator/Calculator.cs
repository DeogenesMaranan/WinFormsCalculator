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
        private string calculation = "";

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        const int WM_NCLBUTTONDOWN = 0xA1;
        const int HT_CAPTION = 0x2;

        private HashSet<char> operators = new HashSet<char>()
        {
            '+', '-', '×', '÷', '.'
        };

        public Calculator()
        {
            InitializeComponent();
        }

        private void close(object sender, EventArgs e)
        {
            Close();
        }

        private void minimize(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

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
            calculation += (button).Text;
            inputTextBox.Text = calculation;
        }
        private void clearBtn_Click(object sender, EventArgs e)
        {
            inputTextBox.Text = "";
            resultTextBox.Text = "";
            calculation = "";
        }

        private void clearEntryBtn_click(object sender, EventArgs e)
        {
            if(calculation.Length > 0)
            {
                while (calculation.Length > 0 && !operators.Contains(calculation[calculation.Length - 1]))
                {
                    calculation = calculation.Remove(calculation.Length - 1, 1);
                }
                if (calculation.Length > 1)
                {
                    if (operators.Contains(calculation[calculation.Length - 2]))
                    {
                        calculation = calculation.Remove(calculation.Length - 1, 1);
                    }
                }
                if (calculation.Length == 1) calculation = calculation.Remove(calculation.Length - 1, 1);
                inputTextBox.Text = calculation;
            }
        }


        private void delBtn_Click(object sender, EventArgs e)
        {
            if (calculation.Length > 0)
            {
                calculation = calculation.Remove(calculation.Length - 1, 1);
            }
            inputTextBox.Text = calculation;
        }

        private void signBtn_Click(object sender, EventArgs e)
        {
            var temp = new StringBuilder();
            while (calculation.Length > 0 && !operators.Contains(calculation[calculation.Length - 1]))
            {
                temp.Insert(0, calculation[calculation.Length - 1]);
                calculation = calculation.Remove(calculation.Length - 1, 1);
            }
            if (calculation.Length > 1 && temp[temp.Length - 1] == ')'){
                calculation = calculation.Remove(calculation.Length - 1, 1);
                calculation = calculation.Remove(calculation.Length - 1, 1);
                temp = temp.Replace(")(-", "");
                temp = temp.Remove(temp.Length - 1, 1);
                calculation += temp;
            }
            else
            {
                calculation += $"(-{temp})";
            }

            inputTextBox.Text = calculation;
        }



        private void equalBtn_Click(object sender, EventArgs e)
        {
            string formattedCalculation = calculation.ToString().Replace("×", "*").ToString().Replace("÷", "/");
            if (operators.Contains(formattedCalculation[formattedCalculation.Length - 1]))
            {
                formattedCalculation = formattedCalculation.Remove(calculation.Length - 1, 1);
                calculation = calculation.Remove(calculation.Length - 1, 1);
            }

            try
            {
                double result = Convert.ToDouble(new DataTable().Compute(formattedCalculation, null));
                string roundedResult = Math.Round(result, 4).ToString();

                inputTextBox.Text = roundedResult;
                resultTextBox.Text = calculation;
                calculation = roundedResult;
            }
            catch (Exception ex)
            {
                inputTextBox.Text = "";
                calculation = "";
                string errorMessage;

                if (ex is OverflowException)
                {
                    errorMessage = "TOO LARGE.";
                }
                else
                {
                    errorMessage = "ERROR";
                }

                resultTextBox.Text = errorMessage;
            }
        }

        private void inputTextBox_Changes(object sender, EventArgs e)
        {
            signBtn.Enabled = !isLastCharOP();
        }

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
                return true;
            }
        }


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
