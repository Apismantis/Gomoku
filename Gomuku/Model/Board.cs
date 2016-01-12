using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gomoku.Model
{
    public enum CellValues { Outside, None, Player1, Player2, Machine }

    public struct Node
    {
        public int Row;
        public int Column;
    }

    public class Board
    {
        private CellValues CurrentPlayer;
        private CellValues NextPlayer;
        private CellValues[,] BoardCell;
        int MAX_SQUARE;

        public delegate void PlayerWinHandle();
        public event PlayerWinHandle PlayerWin;
        public delegate void PlayerDickenHandle();
        public event PlayerDickenHandle PlayerDicken;        
        public delegate void PaintCellHandle(int row, int col);
        public event PaintCellHandle PaintCellEvent;

        public Board()
        {
            CurrentPlayer = CellValues.Player1;

            NextPlayer = CellValues.Player1;

            MAX_SQUARE = Gomuku.Properties.Settings.Default.MAX_SQUARE;

            BoardCell = new CellValues[MAX_SQUARE, MAX_SQUARE];

            for (int i = 0; i < MAX_SQUARE; i++)
                for (int j = 0; j < MAX_SQUARE; j++)
                {
                    BoardCell[i, j] = CellValues.None;
                }
        }

        public CellValues GetCurrentPlayer()
        {
            return CurrentPlayer;
        }

        public void PlayAt(int row, int col)
        {
            if (!IsInBoard(row, col))
                return;

            if (BoardCell[row, col] == CellValues.None)
            {
                BoardCell[row, col] = NextPlayer;

                if (NextPlayer == CellValues.Player1)
                {
                    CurrentPlayer = NextPlayer;
                    NextPlayer = CellValues.Player2;
                }
                else
                {
                    CurrentPlayer = NextPlayer;
                    NextPlayer = CellValues.Player1;
                }

                PaintCellEvent(row, col);

                // Kiểm tra hòa nhau
                if (CheckDicken(row, col))
                {
                    PlayerDicken();
                    return;
                }

                // Kiểm tra thắng
                if (CheckWin(row, col, 1, 0) || CheckWin(row, col, 0, 1) || CheckWin(row, col, 1, 1) || CheckWin(row, col, -1, 1))
                {
                    // Phat su kien co nguoi choi thang
                    PlayerWin();
                }
            }
        }

        public bool IsInBoard(int row, int col)
        {
            return row >= 0 && row < MAX_SQUARE && col >= 0 && col < MAX_SQUARE;
        }

        public bool CheckWin(int row, int col, int increRow, int increCol)
        {
            int CountCell = 0;
            int cRow = row;
            int cCol = col;

            while (IsInBoard(cRow, cCol))
            {
                if (BoardCell[cRow, cCol] == CurrentPlayer)
                {
                    CountCell++;
                    cRow += increRow;
                    cCol += increCol;
                }
                else
                    break;
            }

            CountCell--;
            cRow = row;
            cCol = col;
            while (IsInBoard(cRow, cCol))
            {
                if (BoardCell[cRow, cCol] == CurrentPlayer)
                {
                    CountCell++;
                    cRow -= increRow;
                    cCol -= increCol;
                }
                else
                    break;
            }

            if (CountCell >= 5)
                return true;
            return false;
        }

        public bool CheckDicken(int row, int col)
        {
            for (int i = 0; i < MAX_SQUARE; i++)
                for (int j = 0; j < MAX_SQUARE; j++)
                {
                    if (BoardCell[i, j] == CellValues.None)
                        return false;
                }
            
            return true;
        }
    }
}
