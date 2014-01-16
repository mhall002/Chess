using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts
{
    public class Board
    {
        private Piece[,] pieces = new Piece[8, 8];

        public Piece Get(int X, int Y)
        {
            return pieces[X - 1, Y - 1];
        }
    }
}
