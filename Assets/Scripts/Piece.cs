using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    public enum Colour
    {
        White,
        Black
    }

    public enum Type
    {
        Pawn,
        Rook,
        Knight,
        Bishop,
        Queen,
        King
    }

    public class Position
    {
        public int X;
        public int Y;

        public Position(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
        }
    }

    

    public class Piece : MonoBehaviour
    {
        public int X = 1;
        public int Y = 1;
        public Colour Colour;
        public Type Type;
        public bool Moved = false;

        public event Action<int, int> Clicked;

        // Use this for initialization
        void Start()
        {
            gameObject.transform.position = new Vector3(Utility.GameBoard.xMin + Utility.width * (X), Utility.GameBoard.yMax - Utility.height * (Y), 0);
        }

        public void Move(Position position)
        {
            this.X = position.X;
            this.Y = position.Y;
            gameObject.transform.position = new Vector3(Utility.GameBoard.xMin + Utility.width * (X), Utility.GameBoard.yMax - Utility.height * (Y), 0);
        }

        public void Place(int x, int y)
        {
            this.X = x;
            this.Y = y;
            gameObject.transform.position = new Vector3(Utility.GameBoard.xMin + Utility.width * (X), Utility.GameBoard.yMax - Utility.height * (Y), 0);
        }

        public void Move(int x, int y)
        {
            Moved = true;
            this.X = x;
            this.Y = y;
            gameObject.transform.position = new Vector3(Utility.GameBoard.xMin + Utility.width * (X), Utility.GameBoard.yMax - Utility.height * (Y), 0);
        }

        public void Kill()
        {
            Destroy(gameObject);
        }

        void MouseOver()
        {

        }

        void OnMouseDown()
        {
            if (Clicked != null)
            {
                Clicked.Invoke(X,Y);
            }
        }
    }

    public class NPiece
    {
        public int X = 1;
        public int Y = 1;
        public Colour Colour;
        public Type Type;
        public bool Moved = false;

        public NPiece(Piece piece)
        {
            X = piece.X;
            Y = piece.Y;
            Colour = piece.Colour;
            Type = piece.Type;
            Moved = piece.Moved;
        }

        public NPiece()
        {
            // TODO: Complete member initialization
        }
    }
}
