using EdmGraph;

namespace EdmGraph.Console;

class Program
{
    static void Main(string[] args)
    {
        System.Console.WriteLine("=== EdmGraph Demo Application ===");
        System.Console.WriteLine();

        // Demonstrate Calculator functionality
        var calculator = new Calculator();
        System.Console.WriteLine("Calculator Demo:");
        System.Console.WriteLine($"10 + 5 = {calculator.Add(10, 5)}");
        System.Console.WriteLine($"10 - 5 = {calculator.Subtract(10, 5)}");
        System.Console.WriteLine($"10 * 5 = {calculator.Multiply(10, 5)}");
        System.Console.WriteLine($"10 / 5 = {calculator.Divide(10, 5)}");
        System.Console.WriteLine();

        // Demonstrate StringUtils functionality
        System.Console.WriteLine("StringUtils Demo:");
        string sampleText = "hello world from EdmGraph";
        System.Console.WriteLine($"Original: '{sampleText}'");
        System.Console.WriteLine($"Reversed: '{StringUtils.Reverse(sampleText)}'");
        System.Console.WriteLine($"Word Count: {StringUtils.CountWords(sampleText)}");
        System.Console.WriteLine($"Title Case: '{StringUtils.ToTitleCase(sampleText)}'");
        System.Console.WriteLine();

        // Interactive demo
        System.Console.WriteLine("Try it yourself!");
        System.Console.Write("Enter two numbers to add (separated by space): ");
        string? input = System.Console.ReadLine();

        if (!string.IsNullOrEmpty(input))
        {
            string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2 &&
                int.TryParse(parts[0], out int num1) &&
                int.TryParse(parts[1], out int num2))
            {
                System.Console.WriteLine($"Result: {num1} + {num2} = {calculator.Add(num1, num2)}");
            }
            else
            {
                System.Console.WriteLine("Invalid input. Please enter two numbers separated by a space.");
            }
        }

        System.Console.WriteLine();
        System.Console.WriteLine("Press any key to exit...");
        System.Console.ReadKey();
    }
}
