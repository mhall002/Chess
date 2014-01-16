using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    public class HighlightRectangle : MonoBehaviour
    {
        public event Action<HighlightRectangle> Clicked;

        public int X;
        public int Y;
        public Move Move;

        public void Place(int X, int Y, Move move)
        {
            this.X = X;
            this.Y = Y;
            this.Move = move;
            gameObject.transform.position = new Vector3(Utility.GameBoard.xMin + Utility.width * (X), Utility.GameBoard.yMax - Utility.height * (Y), 0);
        }

        public void OnMouseDown()
        {
            if (Clicked != null)
            {
                Clicked.Invoke(this);
            }
        }
    }
}
