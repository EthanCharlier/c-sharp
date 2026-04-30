// ENTRY POINT
class Program
{
    static void Main(string[] args)
    {
        Console.Write("Enter an operation (e.g. 5+4): ");
        string input = Console.ReadLine();

        ExpressionParser parser = new ExpressionParser();
        Expression expression = parser.Parse(input);

        Calculator calculator = new Calculator();
        double result = calculator.Evaluate(expression);

        Console.WriteLine(expression.ToString() + " = " + result);
    }
}

// MODEL
class Expression
{
    public double LeftOperand { get; set; }
    public double RightOperand { get; set; }
    public char Operator { get; set; }

    public override string ToString()
        => $"{LeftOperand} {Operator} {RightOperand}";
}

// PARSER
class ExpressionParser
{
    private static readonly char[] SupportedOperators = { '+', '-', '*', '/' };

    public Expression Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input cannot be empty.");

        int index = 0;
        string currentNumber = "";
        char foundOperator = '\0';
        List<string> operands = new List<string>();

        while (index < input.Length)
        {
            char currentChar = input[index];

            if (IsOperator(currentChar))
            {
                if (currentNumber != "")
                {
                    operands.Add(currentNumber);
                    currentNumber = "";
                }
                foundOperator = currentChar;
            }
            else
            {
                currentNumber += currentChar;
            }

            index++;
        }

        if (currentNumber != "")
            operands.Add(currentNumber);

        if (operands.Count != 2 || foundOperator == '\0')
            throw new FormatException($"Invalid expression: '{input}'");

        return new Expression
        {
            LeftOperand = double.Parse(operands[0]),
            RightOperand = double.Parse(operands[1]),
            Operator = foundOperator
        };
    }

    private bool IsOperator(char c)
        => Array.Exists(SupportedOperators, op => op == c);
}

// CALCULATOR
class Calculator
{
    public double Evaluate(Expression expression)
    {
        return expression.Operator switch
        {
            '+' => expression.LeftOperand + expression.RightOperand,
            '-' => expression.LeftOperand - expression.RightOperand,
            '*' => expression.LeftOperand * expression.RightOperand,
            '/' => expression.RightOperand != 0
                        ? expression.LeftOperand / expression.RightOperand
                        : throw new DivideByZeroException("Cannot divide by zero!"),
            _ => throw new InvalidOperationException($"Unknown operator: {expression.Operator}")
        };
    }
}
