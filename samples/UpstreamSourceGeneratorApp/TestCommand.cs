[AttributeUsage(AttributeTargets.Class)]
public class CommandGeneratorAttribute : Attribute
{
}

[CommandGenerator]
public partial class TestCommand
{
    public void Run()
    {
        this.GeneratedMethod();
        // BuildCommand();
    }
}