using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts
{
    public class Controller : MonoBehaviour
    {
        List<HighlightRectangle> rects = new List<HighlightRectangle>();
        private Piece[,] pieces = new Piece[8, 8];

        public GameObject RedHighlightRect;
        public GameObject GreenHighlightRect;
        public GameObject BlueHighlightRect;

        public GameObject WhiteRook;
        public GameObject WhiteKnight, WhiteBishop, WhiteKing, WhiteQueen, WhitePawn;
        public GameObject BlackRook, BlackKnight, BlackBishop, BlackKing, BlackQueen, BlackPawn;

        public Colour turn = Colour.Black;
        bool FoundMove = false;
        Move CPUMove = null;
        public ChessAI ChessAI;

        Piece SelectedPiece = null;

        void PieceClick(int x, int y)
        {
            Piece piece = pieces[x, y];
            if (piece.Colour != turn)
            {
                if (SelectedPiece != null)
                {
                    foreach (HighlightRectangle rect in rects)
                    {
                        if (rect.X == x && rect.Y == y)
                            RectClick(rect);
                    }
                }
                return;
            }
            SelectedPiece = piece;
            ClearRects();
            GetMoves(x, y);
            CreateGreenRect(x, y, null);
            print("Clicked " + piece.Type);
        }

        void Update()
        {
            if (FoundMove)
            {
                FoundMove = false;
                print("Found move");
                CreateBlueRect(CPUMove.oldx, CPUMove.oldy, null);
                CreateBlueRect(CPUMove.newx, CPUMove.newy, CPUMove);
            }
        }

        private void GetMoves(int x, int y)
        {
            //List<Move> moves = new List<Move>();
            List<Move> moves = ChessAI.GetMoves(pieces, x, y);
            
            foreach (Move move in moves)
            {
                if (pieces[move.newx, move.newy] == null)
                {
                    CreateGreenRect(move.newx, move.newy, move);
                }
                else
                {
                    CreateRedRect(move.newx, move.newy, move);
                }
            }
        }

        void RectClick(HighlightRectangle rect)
        {
            ExecuteMove(rect.Move);
        }

        bool inBounds (int x, int y)
        {
            return x >= 0 && x < 8 && y >= 0 && y < 8;
        }

        void CreateRedRect(int x, int y, Move move)
        {
            GameObject gameObject = GameObject.Instantiate(RedHighlightRect) as GameObject;
            HighlightRectangle gamePiece = gameObject.GetComponent<HighlightRectangle>();
            gamePiece.Place(x, y, move);
            rects.Add(gamePiece);
            gamePiece.Clicked += RectClick;
        }

        void CreateGreenRect(int x, int y, Move move)
        {
            GameObject gameObject = GameObject.Instantiate(GreenHighlightRect) as GameObject;
            HighlightRectangle gamePiece = gameObject.GetComponent<HighlightRectangle>();
            gamePiece.Place(x, y, move);
            rects.Add(gamePiece);
            gamePiece.Clicked += RectClick;
        }

        void CreateBlueRect(int x, int y, Move move)
        {
            GameObject gameObject = GameObject.Instantiate(BlueHighlightRect) as GameObject;
            HighlightRectangle gamePiece = gameObject.GetComponent<HighlightRectangle>();
            gamePiece.Place(x, y, move);
            rects.Add(gamePiece);
        }

        void ClearRects()
        {
            foreach (HighlightRectangle rect in rects)
            {
                Destroy(rect.gameObject);
            }
            rects.Clear();
        }

        Move CreateMove(int oldx, int oldy, int newx, int newy)
        {
            Move move = new Move();
            move.oldx = oldx;
            move.oldy = oldy;
            move.newx = newx;
            move.newy = newy;
            move.piece = new NPiece(pieces[oldx, oldy]);
            move.destroyed = new NPiece(pieces[newx, newy]);
            return move;
        }

        // Broken!!
        void UndoMove(Move move)
        {
            throw new NotImplementedException();
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

            Move(pieces[move.oldx, move.oldy], move.oldx, move.oldy);
            //pieces[move.newx, move.newy] = move.destroyed;
        }

        void ExecuteMove(Move move)
        {
            if (move.piece.Type == Type.Pawn && (move.newy == 0 || move.newy == 7))
            {
                if (turn == Colour.Black)
                    AddPiece(turn == Colour.Black ? BlackQueen : WhiteQueen, move.newx, move.newy, turn);
                pieces[move.oldx, move.oldy].Kill();
                pieces[move.oldx, move.oldy] = null;
            }
            else
            {
                if (move.piece.Type == Type.King && System.Math.Abs(move.newx - move.oldx)>1)
                {
                    if (move.newx == 1)
                    {
                        Move(pieces[0, move.oldy], 2, move.oldy);
                    }
                    else
                    {
                        Move(pieces[7, move.oldy], 5, move.oldy);
                    }
                }
                Move(pieces[move.oldx, move.oldy], move.newx, move.newy);
            }

            ClearRects();
            SelectedPiece = null;

            turn = GetOther(turn);

            ChessAI.SetBoard(pieces);

            Thread thread = new Thread(new ThreadStart(
                () =>
                {
                    ChessAI_CPUMoveFound(ChessAI.GetMove(turn));
                }));
            FoundMove = false;
            thread.Start();
            //Move cpuMove = ChessAI.GetMove(turn, pieces);

            
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

                        if (!inBounds(newx, newy))
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
                                foreach (Move move in moves)
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
                        moves.Add(CreateMove(x, y, newx, newy));
                    }
                    break;
                }
                moves.Add(CreateMove(x, y, newx, newy));
            }
        }

        void GetMoves(int x, int y, List<Move> moves)
    {
        Piece piece = pieces[x, y];

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

                foreach (Piece otherPiece in pieces)
                {
                    if (!piece.Moved && otherPiece != null && otherPiece.Type == Type.Rook && otherPiece.Colour == piece.Colour && !otherPiece.Moved)
                    {
                        int newy = piece.Y;
                        int newx = otherPiece.X == 0 ? 1 : 6;
                        if (Check(newx, newy, GetOther(piece.Colour)))
                            continue;
                        if (newx == 1)
                        {
                            if (Check(newx+1, newy, GetOther(piece.Colour)))
                                continue;
                        }
                        else
                        {
                            if (Check(newx - 1, newy, GetOther(piece.Colour)))
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

        void Start()
        {
            ResetBoard();

            ChessAI.SetBoard(pieces);
            Move move = ChessAI.GetMove(turn);

            CreateGreenRect(move.oldx, move.oldy, move);
            CreateGreenRect(move.newx, move.newy, null);

            ChessAI.CPUMoveFound += ChessAI_CPUMoveFound;
        }

        void ChessAI_CPUMoveFound(Move cpuMove)
        {
            FoundMove = true;
            CPUMove = cpuMove;
            print("Setting found to true");
        }

        void AddPiece(GameObject piece, int x, int y, Colour colour)
        {
            GameObject gameObject = GameObject.Instantiate(piece) as GameObject;
            Piece gamePiece = gameObject.GetComponent<Piece>();
            gamePiece.Place(x, y);
            gamePiece.Colour = colour;
            pieces[x, y] = gamePiece;
            gamePiece.Clicked += PieceClick;
        }

        public void ResetBoard()
        {
            pieces = new Piece[8, 8];
            int y = 0;
            AddPiece(WhiteRook, 0, y, Colour.White);
            AddPiece(WhiteRook, 7, y, Colour.White);
            print("Outputting knight now");
            AddPiece(WhiteKnight, 1, y, Colour.White);
            print("Woah thar i did it");
            AddPiece(WhiteKnight, 6, y, Colour.White);
            AddPiece(WhiteBishop, 2, y, Colour.White);
            AddPiece(WhiteBishop, 5, y, Colour.White);
            AddPiece(WhiteKing, 4, y, Colour.White);
            AddPiece(WhiteQueen, 3, y, Colour.White);
            y+=2;
            for (int x = 0; x < 8; x++)
            {
                AddPiece(WhitePawn, x, y, Colour.White);
            }

            y = 7;
            AddPiece(BlackRook, 0, y, Colour.Black);
            AddPiece(BlackRook, 7, y, Colour.Black);
            AddPiece(BlackKnight, 1, y, Colour.Black);
            AddPiece(BlackKnight, 6, y, Colour.Black);
            AddPiece(BlackBishop, 2, y, Colour.Black);
            AddPiece(BlackBishop, 5, y, Colour.Black);
            AddPiece(BlackKing, 4, y, Colour.Black);
            AddPiece(BlackQueen, 3, y, Colour.Black);
            y--;
            for (int x = 0; x < 8; x++)
            {
                AddPiece(BlackPawn, x, y, Colour.Black);
            }
        }

        public void Move(Piece piece, int x, int y)
        {
            if (pieces[x, y] != null)
            {
                pieces[x, y].Kill();
            }

            pieces[piece.X, piece.Y] = null;
            pieces[x, y] = piece;
            piece.Move(x, y);
        }

        Colour GetOther(Colour colour)
        {
            return colour == Colour.White ? Colour.Black : Colour.White;
        }
    }
}
