using System;
using System.Collections.Generic;
using System.Text;

namespace Gomoku.Model
{
    public struct Node
    {
        public int Row;
        public int Column;
    }

    public class EValueBoard
    {        
        public int MAX_SQUARE;
        public int[,] Board;

        public EValueBoard()
        {
            MAX_SQUARE = Gomuku.Properties.Settings.Default.MAX_SQUARE;
            Board = new int[MAX_SQUARE + 2, MAX_SQUARE + 2];

            ResetBoard();
        }

        // Reset lại bảng lượng giá
        public void ResetBoard()
        {
            for (int i = 0; i < MAX_SQUARE + 2; i++)
                for (int j = 0; j < MAX_SQUARE + 2; j++)
                    Board[i, j] = 0;
        }
                
        // Lấy thằng Node có giá trị lượng giá cao nhất
        public Node GetMaxNode()
        {
            int Max = Board[1, 1];
            Node node = new Node();

            for (int i = 1; i <= MAX_SQUARE; i++)
                for (int j = 1; j <= MAX_SQUARE; j++)
                    if (Board[i, j] > Max)
                    {
                        node.Row = i; 
                        node.Column = j;
                        Max = Board[i, j];
                    }
            return node;
        }
    }
}
