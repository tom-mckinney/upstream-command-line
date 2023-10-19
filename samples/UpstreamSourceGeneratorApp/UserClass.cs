namespace UpstreamSourceGeneratorApp;

public partial class UserClass
{
    public void UserMethod()
    {
        // call into a generated method inside the class
        // Console.WriteLine("WHAT?!");
        // this.GeneratedMethod();
        // Console.WriteLine("WHAT?!");
        
        // Console.WriteLine($"Does this work?: {this.Test()}");
        
        GeneratedNamespace.GeneratedClass.GeneratedMethod();
    }
}

// using GeneratedNamespace;
//
// public partial class UserClass
// {
//     public void UserMethod()
//     {
//         // call into a generated method
//         GeneratedNamespace.GeneratedClass.GeneratedMethod();
//     }
// }