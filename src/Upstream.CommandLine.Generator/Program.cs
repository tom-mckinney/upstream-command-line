namespace ConsoleApp;

partial class Program
{
    static void Main(string[] args)
    {
        var test = new UserClass();
        test.UserMethod();
        // Console.WriteLine(Testo.Wumbo());
        HelloFrom("Generated Code");
        
    }

    static partial void HelloFrom(string name);
}