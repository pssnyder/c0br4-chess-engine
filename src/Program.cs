using C0BR4ChessEngine.UCI;

namespace C0BR4ChessEngine
{
    /// <summary>
    /// Main entry point for the chess engine
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            var engine = new UCIEngine();
            engine.Run();
        }
    }
}
