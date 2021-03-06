namespace NetInterop.Routing
{
    public abstract class InterpretiveHandler : Handler
    {
        public abstract Interpreter Interpreter { get; }

        public object Interpret(object data)
        {
            return Interpreter.Interpret(data);
        }
    }
}