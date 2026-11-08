using C0BR4ChessEngine.Core;

namespace C0BR4ChessEngine.Evaluation
{
    /// <summary>
    /// C0BR4 v4: literal port of the archived V7P3R v18.3 default fast evaluator.
    /// Scores are side-to-move relative; component scores are White relative.
    /// </summary>
    public sealed class ViperEvaluator
    {
        private static readonly int[] Values = { 0, 100, 300, 325, 500, 900, 0 };
        private static readonly int[][] Tables =
        {
            Array.Empty<int>(),
            new[] {
                0,0,0,0,0,0,0,0, 50,50,50,50,50,50,50,50,
                60,60,70,80,80,70,60,60, 70,70,80,90,90,80,70,70,
                100,100,110,120,120,110,100,100, 200,200,220,250,250,220,200,200,
                400,400,450,500,500,450,400,400, 900,900,900,900,900,900,900,900 },
            new[] {
                200,220,240,250,250,240,220,200, 220,240,260,270,270,260,240,220,
                240,260,300,320,320,300,260,240, 250,270,320,350,350,320,270,250,
                250,270,320,350,350,320,270,250, 240,260,300,320,320,300,260,240,
                220,240,260,270,270,260,240,220, 200,220,240,250,250,240,220,200 },
            new[] {
                250,260,270,280,280,270,260,250, 260,300,290,290,290,290,300,260,
                270,290,320,300,300,320,290,270, 280,290,300,350,350,300,290,280,
                280,290,300,350,350,300,290,280, 270,290,320,300,300,320,290,270,
                260,300,290,290,290,290,300,260, 250,260,270,280,280,270,260,250 },
            new[] {
                400,410,420,430,430,420,410,400, 450,450,450,450,450,450,450,450,
                440,440,440,440,440,440,440,440, 440,440,440,440,440,440,440,440,
                440,440,440,440,440,440,440,440, 450,450,450,450,450,450,450,450,
                500,500,500,500,500,500,500,500, 480,480,480,480,480,480,480,480 },
            new[] {
                700,710,720,730,730,720,710,700, 710,750,750,750,750,750,750,710,
                720,750,800,800,800,800,750,720, 730,750,800,850,850,800,750,730,
                730,750,800,850,850,800,750,730, 720,750,800,800,800,800,750,720,
                710,750,750,750,750,750,750,710, 700,710,720,730,730,720,710,700 },
            new[] {
                50,80,0,0,0,0,80,50, 0,0,0,0,0,0,0,0,
                -50,-50,-50,-50,-50,-50,-50,-50, -100,-100,-100,-100,-100,-100,-100,-100,
                -150,-150,-150,-150,-150,-150,-150,-150, -200,-200,-200,-200,-200,-200,-200,-200,
                -250,-250,-250,-250,-250,-250,-250,-250, -300,-300,-300,-300,-300,-300,-300,-300 }
        };
        private static readonly int[] EndgameKing =
        {
            -50,-40,-30,-20,-20,-30,-40,-50, -30,-20,-10,0,0,-10,-20,-30,
            -30,-10,20,30,30,20,-10,-30, -30,-10,30,40,40,30,-10,-30,
            -30,-10,30,40,40,30,-10,-30, -30,-10,20,30,30,20,-10,-30,
            -30,-30,0,0,0,0,-30,-30, -50,-30,-30,-30,-30,-30,-30,-50
        };

        public int Evaluate(Board board)
        {
            var (material, pst, strategic) = EvaluateComponents(board);
            // Preserve Python int(float) truncation, including its operation order.
            int whiteScore = (int)(pst * 0.6 + material * 0.4 + strategic);
            return board.IsWhiteToMove ? whiteScore : -whiteScore;
        }

        public (int Material, int Pst, int Strategic) EvaluateComponents(Board board)
        {
            int whiteMaterial = 0, blackMaterial = 0, queens = 0, material = 0, pst = 0;
            for (int square = 0; square < 64; square++)
            {
                var piece = board.GetPiece(new Square(square));
                if (piece.IsNull) continue;
                int value = Values[(int)piece.PieceType];
                material += piece.IsWhite ? value : -value;
                if (piece.PieceType == PieceType.Queen) queens++;
                if (piece.PieceType != PieceType.Pawn && piece.PieceType != PieceType.King)
                {
                    if (piece.IsWhite) whiteMaterial += value;
                    else blackMaterial += value;
                }
            }
            bool endgame = queens == 0 || (whiteMaterial < 800 && blackMaterial < 800);
            for (int square = 0; square < 64; square++)
            {
                var piece = board.GetPiece(new Square(square));
                if (piece.IsNull) continue;
                int index = piece.IsWhite ? square : square ^ 56;
                var table = piece.PieceType == PieceType.King && endgame
                    ? EndgameKing : Tables[(int)piece.PieceType];
                pst += piece.IsWhite ? table[index] : -table[index];
            }
            int strategic = !endgame && board.FullMoveNumber >= 10 ? Strategic(board) : 0;
            return (material, pst, strategic);
        }

        private static bool IsPawn(Board board, int file, int rank, bool white)
        {
            var piece = board.GetPiece(new Square(file, rank));
            return piece.PieceType == PieceType.Pawn && piece.IsWhite == white;
        }

        private static int Strategic(Board board)
        {
            int score = 0;
            for (int square = 0; square < 64; square++)
            {
                var piece = board.GetPiece(new Square(square));
                if (piece.IsNull) continue;
                int file = square % 8, rank = square / 8;
                int sign = piece.IsWhite ? 1 : -1;
                if (piece.PieceType == PieceType.Rook)
                {
                    bool ownPawn = false, enemyPawn = false;
                    for (int r = 0; r < 8; r++)
                    {
                        ownPawn |= IsPawn(board, file, r, piece.IsWhite);
                        enemyPawn |= IsPawn(board, file, r, !piece.IsWhite);
                    }
                    if (!ownPawn) score += sign * (enemyPawn ? 10 : 20);
                }
                else if (piece.PieceType == PieceType.King)
                {
                    for (int f = Math.Max(0, file - 1); f <= Math.Min(7, file + 1); f++)
                    {
                        for (int offset = 1; offset <= 2; offset++)
                        {
                            int r = rank + (piece.IsWhite ? offset : -offset);
                            if (r >= 0 && r < 8 && IsPawn(board, f, r, piece.IsWhite))
                            {
                                score += sign * 10;
                                break;
                            }
                        }
                    }
                }
                else if (piece.PieceType == PieceType.Pawn)
                {
                    bool passed = true;
                    for (int f = Math.Max(0, file - 1); f <= Math.Min(7, file + 1); f++)
                        for (int r = rank + (piece.IsWhite ? 1 : -1);
                             r >= 0 && r < 8; r += piece.IsWhite ? 1 : -1)
                            if (IsPawn(board, f, r, !piece.IsWhite)) passed = false;
                    if (passed) score += sign * 30;
                }
            }
            for (int file = 0; file < 8; file++)
            {
                int white = 0, black = 0;
                for (int rank = 0; rank < 8; rank++)
                {
                    if (IsPawn(board, file, rank, true)) white++;
                    if (IsPawn(board, file, rank, false)) black++;
                }
                score -= 20 * Math.Max(0, white - 1);
                score += 20 * Math.Max(0, black - 1);
            }
            return score;
        }
    }
}
