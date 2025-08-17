using ChessEngine.Core;

namespace ChessEngine.Search
{
    public interface IChessBot
    {
        Move Think(Board board, TimeSpan timeLimit);
    }
}
