using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Calculator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private enum Operator
        {
            None,
            Add,
            Subtract,
            Multiply,
            Divide,
            Power
        }

        private Operator activeOperator;

        //runningValue represents the value which is being operated on
        //It stores the inital inputted value or the result of the last operation
        private CalculatorValue runningValue;
        //tempValue is the value of the right side operand
        private CalculatorValue tempValue;

        //workingValue is a reference to the currently displayed value, either runningValue or tempValue
        private CalculatorValue workingValue;

        private bool initialEntry = true;

        public MainWindow()
        {
            CClear();

            InitializeComponent();

            UpdateDisplay();
        }

        /// <summary>
        /// Called whenever one of the calculator buttons is clicked.
        /// </summary>
        private void CalcButton_Click(object sender, RoutedEventArgs e)
        {
            //Instead of giving each button its own event method, all buttons subscribe to this method and the button's name is used in a switch case
            //Because all buttons are in the same place and perform similar actions, this greaty reduces boilerplate

            Button button = (Button)sender;

            if (runningValue.IsError && button.Name != "Clear") return;

            switch (button.Name)
            {
                case "OperatorExponent":
                    SetOperator(Operator.Power);
                    break;

                case "OperatorDivision":
                    SetOperator(Operator.Divide);
                    break;

                case "OperatorMultiply":
                    SetOperator(Operator.Multiply);
                    break;

                case "Clear":
                    CClear();
                    break;

                case "Digit7":
                    InputDigit('7');
                    break;

                case "Digit8":
                    InputDigit('8');
                    break;

                case "Digit9":
                    InputDigit('9');
                    break;

                case "OperatorMinus":
                    if (workingValue.IsEmpty || (activeOperator != Operator.None && tempValue.IsEmpty))
                    {
                        InputDigit('-');
                    }
                    else
                    {
                        SetOperator(Operator.Subtract);
                    }
                    break;

                case "Digit4":
                    InputDigit('4');
                    break;

                case "Digit5":
                    InputDigit('5');
                    break;

                case "Digit6":
                    InputDigit('6');
                    break;

                case "OperatorPlus":
                    SetOperator(Operator.Add);
                    break;

                case "Digit1":
                    InputDigit('1');
                    break;

                case "Digit2":
                    InputDigit('2');
                    break;

                case "Digit3":
                    InputDigit('3');
                    break;

                case "Equals":
                    DoOperation();
                    break;

                case "Digit0":
                    InputDigit('0');
                    break;

                case "Decimal":
                    InputDigit('.');
                    break;
            }

            UpdateDisplay();
        }

        /// <summary>
        /// Resets the calculator's temp value
        /// </summary>
        private void ResetTempVal()
        {
            tempValue = new CalculatorValue();
        }

        /// <summary>
        /// Reset the calculator, as per the C button
        /// </summary>
        private void CClear()
        {
            activeOperator = Operator.None;
            ResetTempVal();
            runningValue = new CalculatorValue();
            workingValue = runningValue;
        }

        /// <summary>
        /// Input a digit into the calculator
        /// </summary>
        /// <param name="d">the digit to enter</param>
        private void InputDigit(char d)
        {
            if (tempValue.IsEmpty)
            {
                if (activeOperator != Operator.None)
                {
                    workingValue = tempValue;
                }
            }
            else if (ReferenceEquals(workingValue, runningValue) && !initialEntry)
            {
                CClear();
            }
            workingValue.AddDigit(d);
        }

        /// <summary>
        /// Set the calculator's current operator
        /// </summary>
        /// <param name="o">the operator to be set to</param>
        private void SetOperator(Operator o)
        {
            initialEntry = false;
            if (ReferenceEquals(workingValue, tempValue))
            {
                DoOperation();
            }
            ResetTempVal();
            activeOperator = o;
        }

        /// <summary>
        /// Perform an operation, as per the = button
        /// </summary>
        private void DoOperation()
        {
            if (tempValue.IsEmpty) { return; }

            double operand1 = runningValue.GetArithmeticValue();
            double operand2 = tempValue.GetArithmeticValue();
            double result = 0;

            switch (activeOperator)
            {
                case Operator.Add:
                    result = operand1 + operand2;
                    break;

                case Operator.Subtract:
                    result = operand1 - operand2;
                    break;

                case Operator.Multiply:
                    result = operand1 * operand2;
                    break;

                case Operator.Divide:
                    if (operand2 == 0)
                    {
                        runningValue = new ErrorValue();
                        workingValue = runningValue;
                        return;
                    }
                    result = operand1 / operand2;
                    break;

                case Operator.Power:
                    result = Math.Pow(operand1, operand2);
                    break;
            }

            runningValue = new CalculatorValue(result);
            workingValue = runningValue;
        }

        /// <summary>
        /// Update the digital display with the current working value
        /// </summary>
        private void UpdateDisplay()
        {
            Display.Text = workingValue.Value;
        }
    }

    /// <summary>
    /// Handles the value of the calculator
    /// </summary>
    public class CalculatorValue
    {
        public const string NumericalChars = "1234567890";
        public const string ValidChars = "1234567890.-";

        public const int MaxDigits = 8;

        public bool IsError { get; protected set; } = false;
        public bool IsNegative { get; private set; } = false;
        public bool IsEmpty { get => _value.Length == 0; }

        private string _value = "";
        public string Value
        {
            get
            {
                if (IsError)
                {
                    return "ERROR";
                }

                string val = _value;
                if (IsNegative)
                {
                    val = "-" + val;
                }
                return val;
            }
            private set => _value = value;
        }

        public CalculatorValue()
        {
        }

        public CalculatorValue(double avalue)
        {
            if (avalue < 0)
            {
                IsNegative = true;
            }

            double absValue = Math.Abs(avalue);
            string trialValue = Math.Floor(absValue).ToString();
            int allowedFDigits = -1; //The maximum allowed digits after the decimal point for when the double is formatted to a string
            if (trialValue == "0")
            {
                allowedFDigits += MaxDigits;
            }
            else
            {
                allowedFDigits += MaxDigits - trialValue.Length;
            }
            if (IsNegative)
            {
                allowedFDigits -= 1;
            }
            allowedFDigits = Math.Max(allowedFDigits, 0);
            _value = absValue.ToString("0." + new string('#', allowedFDigits));

            if (Value.Length > MaxDigits)
            {
                AttemptConsolidate();
                if (Value.Length > MaxDigits)
                {
                    IsError = true;
                }
            }
        }

        /// <summary>
        /// Enter a digit into the calculator value
        /// </summary>
        /// <param name="digit">digit character</param>
        public void AddDigit(char digit)
        {
            if (!ValidChars.Contains(digit)) { throw new InvalidCharException("Unknown input character."); }

            if (digit == '.' && Value.Contains('.')) { return; }

            if (Value.Length >= MaxDigits)
            {
                AttemptConsolidate();
                if (Value.Length >= MaxDigits)
                {
                    return;
                }
            }
            if (_value == "0" && NumericalChars.Contains(digit))
            {
                Value = "";
            }

            if (digit == '-')
            {
                if (_value.Length > 0)
                {
                    throw new InvalidCharException("A negative sign is only valid on the first input.");
                }
                else
                {
                    IsNegative = true;
                    return;
                }
            }

            Value = _value + digit;
        }

        /// <summary>
        /// Get the exising value as a double
        /// </summary>
        /// <returns>value as double</returns>
        public double GetArithmeticValue()
        {
            if (IsError)
            {
                throw new CalcErrorException();
            }

            if (IsEmpty)
            {
                return 0;
            }
            try
            {
                return double.Parse(Value);
            }
            catch (FormatException)
            {
                return 0;
            }
        }

        /// <summary>
        /// Try to shorten the value string by removing the front digit if it is a 0.
        /// This may occur before a decimal point.
        /// </summary>
        private void AttemptConsolidate()
        {
            if (_value[0] == '0')
            {
                Value = _value[1..];
            }
        }
    }

    /// <summary>
    /// An error value
    /// </summary>
    public class ErrorValue : CalculatorValue
    {
        public ErrorValue()
        {
            IsError = true;
        }
    }

    /// <summary>
    /// Exception to be thrown when an attempt is made to input an invalid character to a value
    /// </summary>
    public class InvalidCharException : Exception
    {
        public InvalidCharException()
        {
        }

        public InvalidCharException(string message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Exception to be thrown when an attempt is made to get the arithmetic value of an error
    /// </summary>
    public class CalcErrorException : Exception
    {
    }
}
