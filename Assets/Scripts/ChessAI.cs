using UnityEngine;
using System.Collections;
using Assets.Scripts;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using System.Diagnostics;

public class ChessAI : MonoBehaviour {

    public int NodesSearched = 0;
    public long MaxSearchLength = 20000;
    //public long ReportGranularity = 100;

    public int PawnValue;
    public int RookValue;
    public int KnightValue;
    public int BishopValue;
    public int QueenValue;



    public GUIScript gui;

    public int MaxSearchDepth = 4;

    public event System.Action<Move> CPUMoveFound;

    Colour local = Colour.White;

    NPiece[,] pieces = new NPiece[8, 8];

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        gui.CPUNodesSearched = NodesSearched;
        gui.CPUSearchDepth = MaxSearchDepth;
        gui.CPUGenerateTime = timer.ElapsedMilliseconds / 1000f;
	}

    NPiece[,] PopulatePieces (Piece[,] pieces)
    {
        NPiece[,] newPieces = new NPiece[8, 8];
        for (int x = 0; x < 8; x++)
            for (int y = 0; y < 8; y++)
            {
                if (pieces[x, y] != null)
                {
                    newPieces[x, y] = new NPiece(pieces[x, y]);
                    //print(pieces[x, y].X + " " + pieces[x, y].Y + " " + pieces[x, y].Colour + " " + pieces[x, y].Type + " " + pieces[x, y].Moved);
                    //print(newPieces[x, y].X + " " + newPieces[x, y].Y + " " + newPieces[x, y].Colour + " " + newPieces[x, y].Type + " " + newPieces[x, y].Moved);
                }
            }
        return newPieces;
    }

    public List<Move> GetMoves(Piece[,] pieces, int x, int y)
    {
        this.pieces = PopulatePieces(pieces);
        List<Move> moves = new List<Move>();
        GetMoves(x, y, moves);
        return moves;
    }

    List<Move> GetMoves(Colour colour)
    {
        List<Move> moves = new List<Move>();
        for (int x = 0; x < 8; x++)
            for (int y = 0; y < 8; y++)
            {
                NPiece NPiece = pieces[x, y];
                if (NPiece == null)
                    continue;
                if (NPiece.Colour == colour)
                {
                    GetMoves(x, y, moves);
                }
            }
        return moves;
    }

    Stopwatch timer = new Stopwatch();

    public void SetBoard(Piece[,] pieces)
    {
        this.pieces = PopulatePieces(pieces);
    }

    public Move GetMove(Colour colour)
    {
        local = colour;

        timer.Reset();
        timer.Start();
        NodesSearched = 0;
        int score = alphaBetaMax(-10000, 10000, MaxSearchDepth);
        timer.Stop();
        return chosenMove;
    }

    Move chosenMove = null;

    int alphaBetaMax(int alpha, int beta, int depthRemaining)
    {
        Move localMove = null;
        if (depthRemaining == 0)
        {
            NodesSearched++;
            return Evaluate();
        }
        List<Move> moves = GetMoves(local);
        IEnumerable moves2 = moves;
        if (depthRemaining > 1)
        {
            moves2 = OrderedMoves(moves, false);
        }
        foreach (Move move in moves2)
        {
            ExecuteMove(move);
            int score = alphaBetaMin(alpha, beta, depthRemaining - 1);
            UndoMove(move);
            if (score >= beta)
            {
                return beta;
            }
            if (score > alpha)
            {
                localMove = move;
                alpha = score;
            }
        }
        chosenMove = localMove;
        return alpha;
    }

    Colour GetOther(Colour colour)
    {
        return colour == Colour.White ? Colour.Black : Colour.White;
    }

    IEnumerable<Move> OrderedMoves(List<Move> moves, bool ascending)
    {
        int counter = 0;
        List<KeyValuePair<Move, int>> scores = new List<KeyValuePair<Move, int>>();
        foreach (Move move in moves)
        {
            ExecuteMove(move);
            int score = Evaluate();
            UndoMove(move);
            scores.Add(new KeyValuePair<Move, int>(move, score));
            counter++;
        }

        if (ascending)
            return scores.OrderBy(x => x.Value).Select(x => x.Key);
        else
            return scores.OrderByDescending(x => x.Value).Select(x => x.Key);
    }

    int alphaBetaMin(int alpha, int beta, int depthRemaining)
    {
        if (depthRemaining == 0)
        {
            NodesSearched++;
            return Evaluate();
        }
        List<Move> moves = GetMoves(GetOther(local));
        IEnumerable moves2 = moves;
        if (depthRemaining > 1)
        {
            moves2 = OrderedMoves(moves, true);
        }
        foreach (Move move in moves2)
        {
            ExecuteMove(move);
            int score = alphaBetaMax(alpha, beta, depthRemaining - 1);
            UndoMove(move);
            if (score <= alpha)
            {
                return alpha;
            }
            if (score < beta)
            {
                chosenMove = move;
                beta = score;
            }
        }
        return beta;
    }

    int Evaluate()
    {
        int Score = 0;
        foreach (NPiece piece in pieces)
        {
            if (piece != null)
            {
                int value = 0;
                switch(piece.Type)
                {
                    case Type.Bishop:
                        value = BishopValue;
                        break;
                    case Type.King:
                        value = 2000;
                        break;
                    case Type.Knight:
                        value = KnightValue;
                        break;
                    case Type.Pawn:
                        value = PawnValue;
                        break;
                    case Type.Queen:
                        value = QueenValue;
                        break;
                    case Type.Rook:
                        value = RookValue;
                        break;
                }
                Score += value * (piece.Colour == local ? 1 : -1);
            }
        }
        return Score;
    }

    Move CreateMove(int oldx, int oldy, int newx, int newy)
    {
        Move move = new Move();
        move.oldx = oldx;
        move.oldy = oldy;
        move.newx = newx;
        move.newy = newy;
        move.piece = pieces[oldx, oldy];
        move.destroyed = pieces[newx, newy];
        return move;
    }

    void UndoMove(Move move)
    {
        if (move.piece.Type == Type.King && System.Math.Abs(move.newx - move.oldx) > 1)
        {
            if (move.newx == 1)
            {
                pieces[0, move.newy] = pieces[2, move.newy];
                pieces[2, move.newy] = null;
            }
            else
            {
                pieces[7, move.newy] = pieces[5, move.newy];
                pieces[5, move.newy] = null;
            }
        }

        pieces[move.newx, move.newy] = move.destroyed;
        pieces[move.oldx, move.oldy] = move.piece;
    }

    void ExecuteMove(Move move)
    {
        if (move.piece.Type == Type.Pawn && (move.newy == 0 || move.newy == 7))
        {
            NPiece newQueen = new NPiece();
            newQueen.Type = Type.Queen;
            newQueen.Colour = move.piece.Colour;
            pieces[move.newx, move.newy] = newQueen;
            pieces[move.oldx, move.oldy] = null;
        }
        else
        {
            if (move.piece.Type == Type.King && System.Math.Abs(move.newx - move.oldx)>1)
            {
                if (move.newx == 1)
                {
                    pieces[2, move.newy] = pieces[0, move.newy];
                    pieces[0, move.newy] = null;
                }
                else
                {
                    pieces[5, move.newy] = pieces[7, move.newy];
                    pieces[7, move.newy] = null;
                }
            }
            pieces[move.newx, move.newy] = move.piece;
            pieces[move.oldx, move.oldy] = null;
        }
    }

    bool Check(int x, int y, Colour colour)
    {
        for (int xdirection = 1; xdirection < 2; xdirection++)
        {
            for (int ydirection = 1; ydirection < 2; ydirection++)
            {
                if (xdirection == 0 && ydirection == 0)
                    continue;

                for (int i = 1; i < 9; i++)
                {
                    int newx = x + xdirection * i;
                    int newy = y + ydirection * i;

                    if (!inBounds(newx,newy))
                        break;

                    if (pieces[newx, newy] == null)
                        continue;

                    if (pieces[newx, newy].Colour != colour)
                    {
                        if (pieces[newx, newy].Type == Type.Bishop && xdirection * ydirection != 0)
                        {
                            return true;
                        }
                        if (pieces[newx, newy].Type == Type.King && i == 1)
                        {
                            return true;
                        }
                        if (pieces[newx, newy].Type == Type.Pawn && xdirection * ydirection != 0 && i == 1)
                        {
                            List<Move> moves = new List<Move>();
                            GetMoves(newx, newy, moves);
                            foreach(Move move in moves)
                            {
                                if (move.newx == x && move.newy == y)
                                    return true;
                            }
                        }
                        if (pieces[newx, newy].Type == Type.Queen)
                        {
                            return true;
                        }
                        if (pieces[newx, newy].Type == Type.Rook && xdirection * ydirection == 0)
                        {
                            return true;
                        }
                    }

                    break;
                }
            }
        }

        for (int xdirection = -1; xdirection <= 1; xdirection += 2)
        {
            for (int ydirection = -1; ydirection <= 1; ydirection += 2)
            {
                int newx = x + xdirection * 2;
                int newy = y + ydirection;
                if (inBounds(newx, newy))
                {
                    if (pieces[newx, newy] != null && pieces[newx, newy].Colour == colour && pieces[newx, newy].Type == Type.Knight)
                    {
                        return true;
                    }
                }

                newx = x + xdirection;
                newy = y + ydirection * 2;
                if (inBounds(newx, newy))
                {
                    if (pieces[newx, newy] != null && pieces[newx, newy].Colour == colour && pieces[newx, newy].Type == Type.Knight)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    void GetMoves(int x, int y, int xdirection, int ydirection, List<Move> moves)
    {
        GetMoves(x, y, xdirection, ydirection, 9, moves);
    }

    void GetMoves(int x, int y, int xdirection, int ydirection, int maxDistance, List<Move> moves)
    {
        if (xdirection == 0 && ydirection == 0)
            return;

        for (int distance = 1; distance <= maxDistance; distance++)
        {
            int newx = x + xdirection * distance;
            int newy = y + ydirection * distance;
            if (!inBounds(newx, newy))
            {
                break;
            }
            if (pieces[newx, newy] != null)
            {
                if (pieces[newx, newy].Colour != pieces[x, y].Colour)
                {
                    moves.Add(CreateMove(x,y,newx, newy));
                }
                break;
            }
            moves.Add(CreateMove(x,y,newx, newy));
        }
    }

    void GetMoves(int x, int y, List<Move> moves)
    {
        NPiece piece = pieces[x, y];
        switch (piece.Type)
        {
            case Type.Bishop:
                for (int xdirection = -1; xdirection <= 1; xdirection += 2)
                    for (int ydirection = -1; ydirection <= 1; ydirection += 2)
                        GetMoves(x, y, xdirection, ydirection, moves);
                break;
            case Type.King:
                for (int xdirection = -1; xdirection <= 1; xdirection += 1)
                    for (int ydirection = -1; ydirection <= 1; ydirection += 1)
                        GetMoves(x, y, xdirection, ydirection, 1, moves);

                foreach (NPiece otherPiece in pieces)
                {
                    if (!piece.Moved && otherPiece != null && otherPiece.Type == Type.Rook && otherPiece.Colour == piece.Colour && !otherPiece.Moved)
                    {
                        int newy = piece.Y;
                        int newx = otherPiece.X == 0 ? 1 : 6;
                        if (Check(newx, newy, GetOther(piece.Colour)))
                            continue;
                        if (newx == 1)
                        {
                            if (pieces[newx, newy] != null || pieces[newx+1,newy]!=null || Check(newx+1, newy, GetOther(piece.Colour)))
                                continue;
                        }
                        else
                        {
                            if (pieces[newx, newy] != null || pieces[newx - 1, newy] != null || Check(newx - 1, newy, GetOther(piece.Colour)))
                                continue;
                        }
                        moves.Add(CreateMove(x, y, newx, newy));
                    }
                }
                break;


            case Type.Knight:
                for (int xdirection = -1; xdirection <= 1; xdirection += 2)
                {
                    for (int ydirection = -1; ydirection <= 1; ydirection += 2)
                    {
                        int newx = x + xdirection * 2;
                        int newy = y + ydirection;
                        if (inBounds(newx, newy))
                        {
                            if (pieces[newx, newy] == null)
                            {
                                moves.Add(CreateMove(x,y,newx, newy));
                            }
                            else if (pieces[newx, newy].Colour != piece.Colour)
                            {
                                moves.Add(CreateMove(x, y, newx, newy));
                            }
                        }
                    }
                }

                for (int xdirection = -1; xdirection <= 1; xdirection += 2)
                {
                    for (int ydirection = -1; ydirection <= 1; ydirection += 2)
                    {
                        int newy = y + ydirection * 2;
                        int newx = x + xdirection;
                        if (inBounds(newx, newy))
                        {
                            if (pieces[newx, newy] == null)
                            {
                                moves.Add(CreateMove(x, y, newx, newy));
                            }
                            else if (pieces[newx, newy].Colour != piece.Colour)
                            {
                                moves.Add(CreateMove(x, y, newx, newy));
                            }
                        }
                    }
                }
                break;
            case Type.Pawn:
                int direction = piece.Colour == Colour.White ? 1 : -1;
                if (inBounds(x + 1, y + direction) && pieces[x + 1, y + direction] != null && pieces[x + 1, y + direction].Colour != piece.Colour)
                {
                    moves.Add(CreateMove(x, y, x + 1, y + direction));
                }
                if (inBounds(x - 1, y + direction) && pieces[x - 1, y + direction] != null && pieces[x - 1, y + direction].Colour != piece.Colour)
                {
                    moves.Add(CreateMove(x, y, x - 1, y + direction));
                }
                if (inBounds(x, y + direction) && pieces[x, y + direction] == null)
                {
                    moves.Add(CreateMove(x, y, x, y + direction));

                    if (piece.Colour == Colour.White && y == 1 || piece.Colour == Colour.Black && y == 6)
                    {
                        if (pieces[x, y + direction * 2] == null)
                        {
                            moves.Add(CreateMove(x, y, x, y + direction * 2));
                        }
                    }
                }
                break;
            case Type.Queen:
                for (int xdirection = -1; xdirection <= 1; xdirection += 1)
                    for (int ydirection = -1; ydirection <= 1; ydirection += 1)
                        GetMoves(x, y, xdirection, ydirection, moves);
                break;
            case Type.Rook:
                for (int xdirection = -1; xdirection <= 1; xdirection += 2)
                    GetMoves(x, y, xdirection, 0, moves);
                for (int ydirection = -1; ydirection <= 1; ydirection += 1)
                    GetMoves(x, y, 0, ydirection, moves);
                break;
        }
    }

    bool inBounds(int x, int y)
    {
        return x >= 0 && x < 8 && y >= 0 && y < 8;
    }


}

public class Move
{
    public int oldx, oldy, newx, newy;
    public NPiece destroyed, piece;
}

enum ScoreType
{
    Exact,
    Upper,
    Lower
}

class StoredValue
{
    public long zobristKey;
    public int depth;
    public int score;
    public ScoreType scoreType;
}

class ZobristHash
{
    long CurrentHash = 0;

    long LongRandom(long min, long max, System.Random random)
    {
        byte[] buf = new byte[8];
        random.NextBytes(buf);
        long longRand = System.BitConverter.ToInt64(buf, 0);
        return (System.Math.Abs(longRand % (max - min)) + min);
    }

    const int whitePawn = 0, whiteRook = 1, whiteKnight = 2, whiteBishop = 3, whiteKing = 4, whiteQueen = 5,
        blackPawn = 6, blackRook = 7, blackKnight = 8, blackBishop = 9, blackKing = 10, blackQueen = 11;

    long[,] pieceBitStrings;
    long[] castlingRightBitStrings;
    long[] enPassantBitStrings;
    long blackMoveBitString;
    System.Random random;

    public ZobristHash()
    {
        pieceBitStrings = new long[12, 64];
        for (int x = 0; x < 12; x++)
        {
            for (int y = 0; y < 63; y++)
            {
                pieceBitStrings[x, y] = LongRandom(0, long.MaxValue, random);
            }
        }
        castlingRightBitStrings = new long[4];
        for (int x = 0; x < 4; x++)
        {
            castlingRightBitStrings[x] = LongRandom(0, long.MaxValue, random);
        }
        enPassantBitStrings = new long[8];
        for (int x = 0; x < 8; x++)
        {
            enPassantBitStrings[x] = LongRandom(0, long.MaxValue, random);
        }
        blackMoveBitString = LongRandom(0, long.MaxValue, random);
    }

    public void Hash(NPiece[,] board, bool blackTurn)
    {
        long hash = 0;
        int counter = 0;
        List<NPiece> kings = new List<NPiece>();
        List<NPiece> rooks = new List<NPiece>();
        foreach (NPiece piece in board)
        {
            if (piece != null)
            {
                int pieceCode = 0;
                switch(piece.Type)
                {
                    case Type.Pawn:
                        pieceCode = piece.Colour == Colour.Black ? blackPawn : whitePawn;
                        break;
                    case Type.Rook:
                        rooks.Add(piece);
                        pieceCode = piece.Colour == Colour.Black ? blackRook : whiteRook;
                        break;
                    case Type.Knight:
                        pieceCode = piece.Colour == Colour.Black ? blackKnight : whiteKnight;
                        break;
                    case Type.Bishop:
                        pieceCode = piece.Colour == Colour.Black ? blackBishop : whiteBishop;
                        break;
                    case Type.King:
                        kings.Add(piece);
                        pieceCode = piece.Colour == Colour.Black ? blackKing : whiteKing;
                        break;
                    case Type.Queen:
                        pieceCode = piece.Colour == Colour.Black ? blackQueen : whiteQueen;
                        break;
                }
                hash = hash ^ pieceBitStrings[pieceCode, counter];
            }
            counter++;
        }

        if (board[0,0] != null && board[0,0].Type == Type.Rook && !board[0,0].Moved)
        {
            foreach (NPiece king in kings)
            {
                if (king.Colour == board[0,0].Colour && !king.Moved)
                {
                    hash = hash ^ castlingRightBitStrings[0];
                }
            }
        }
        if (board[0,7] != null && board[0,7].Type == Type.Rook && !board[0,7].Moved)
        {
            foreach (NPiece king in kings)
            {
                if (king.Colour == board[0,7].Colour && !king.Moved)
                {
                    hash = hash ^ castlingRightBitStrings[0];
                }
            }
        }
        if (board[7,7] != null && board[7,7].Type == Type.Rook && !board[7,7].Moved)
        {
            foreach (NPiece king in kings)
            {
                if (king.Colour == board[7,7].Colour && !king.Moved)
                {
                    hash = hash ^ castlingRightBitStrings[0];
                }
            }
        }
        if (board[7,0] != null && board[7,0].Type == Type.Rook && !board[7,0].Moved)
        {
            foreach (NPiece king in kings)
            {
                if (king.Colour == board[7,0].Colour && !king.Moved)
                {
                    hash = hash ^ castlingRightBitStrings[0];
                }
            }
        }
        if (blackTurn)
        {
            hash = hash ^ blackMoveBitString;
        }

        CurrentHash = hash;
    }

    public long GetHash()
    {
        return CurrentHash;
    }

    public void ChangePiece(int x, int y, Type type, Colour colour)
    {
        int pieceCode = 0;
        switch (type)
        {
            case Type.Pawn:
                pieceCode = colour == Colour.Black ? blackPawn : whitePawn;
                break;
            case Type.Rook:
                pieceCode = colour == Colour.Black ? blackRook : whiteRook;
                break;
            case Type.Knight:
                pieceCode = colour == Colour.Black ? blackKnight : whiteKnight;
                break;
            case Type.Bishop:
                pieceCode = colour == Colour.Black ? blackBishop : whiteBishop;
                break;
            case Type.King:
                pieceCode = colour == Colour.Black ? blackKing : whiteKing;
                break;
            case Type.Queen:
                pieceCode = colour == Colour.Black ? blackQueen : whiteQueen;
                break;
        }
        CurrentHash = CurrentHash ^ pieceBitStrings[pieceCode, y * 8 + x];
    }

    public void ChangeTurn()
    {
        CurrentHash = CurrentHash ^ blackMoveBitString;
    }
}