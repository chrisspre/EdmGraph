namespace EdmGraph;

/// <summary>
/// Simple calculator for demonstration purposes
/// </summary>
public class Calculator
{
    public int Add(int a, int b) => a + b;
    public int Subtract(int a, int b) => a - b;
    public int Multiply(int a, int b) => a * b;
    public double Divide(int a, int b) => b != 0 ? (double)a / b : throw new DivideByZeroException();
}