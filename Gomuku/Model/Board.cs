using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gomuku.Model;

namespace Gomoku.Model
{
    public enum CellValues { Outside, None, Player1, Player2, Machine }

    public class Board
    {

        #region Biến

        private CellValues CurrentPlayer;
        private CellValues NextPlayer;
        private CellValues[,] BoardCell;
        AutoMovesBoard AMB;
        int MAX_SQUARE;
        private BackgroundWorker bw = new BackgroundWorker();
        List<Node> HighlitghtCell;

        #endregion

        #region Delegate

        public delegate void PlayerWinHandle(CellValues Player);
        public event PlayerWinHandle PlayerWin;
        public delegate void PlayerDickenHandle();
        public event PlayerDickenHandle PlayerDicken;
        public delegate void PaintCellHandle(int row, int col);
        public event PaintCellHandle PaintCellEvent;
        public delegate void HighlightCellWinnerHandle(List<Node> HighlightCellList);
        public event HighlightCellWinnerHandle HighlightCellWinnerEvent;

        #endregion
        
        #region Các hàm dùng chung

        public Board()
        {
            CurrentPlayer = CellValues.Player1;

            NextPlayer = CellValues.Player1;

            HighlitghtCell = new List<Node>();

            MAX_SQUARE = Gomuku.Properties.Settings.Default.MAX_SQUARE;

            BoardCell = new CellValues[MAX_SQUARE + 2, MAX_SQUARE + 2];

            ResetBoard();

            AMB = new AutoMovesBoard();

            AMB.CurrentPlayer = CurrentPlayer;

            bw.WorkerSupportsCancellation = true;

            bw.DoWork += new DoWorkEventHandler(bw_DoWork);

            bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
        }

        public void ResetBoard()
        {
            CurrentPlayer = CellValues.Player1;
            NextPlayer = CellValues.Player1;

            for (int i = 0; i < MAX_SQUARE + 2; i++)
                for (int j = 0; j < MAX_SQUARE + 2; j++)
                {
                    if (i != 0 && j != 0 && i != MAX_SQUARE + 1 && j != MAX_SQUARE + 1)
                        BoardCell[i, j] = CellValues.None;
                }
        }

        public CellValues GetCurrentPlayer()
        {
            return CurrentPlayer;
        }

        public bool IsInBoard(int row, int col)
        {
            return row >= 1 && row <= MAX_SQUARE && col >= 1 && col <= MAX_SQUARE;
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
            for (int i = 1; i <= MAX_SQUARE; i++)
                for (int j = 1; j <= MAX_SQUARE; j++)
                {
                    if (BoardCell[i, j] == CellValues.None)
                        return false;
                }

            return true;
        }

        public void GetHighlightCellList(int row, int col, int increRow, int increCol)
        {
            int cRow = row;
            int cCol = col;

            while (IsInBoard(cRow, cCol))
            {
                if (BoardCell[cRow, cCol] == CurrentPlayer)
                {
                    HighlitghtCell.Add(new Node() { Row = cRow - 1, Column = cCol - 1 });
                    cRow += increRow;
                    cCol += increCol;
                }
                else
                    break;
            }

            cRow = row;
            cCol = col;
            while (IsInBoard(cRow, cCol))
            {
                if (BoardCell[cRow, cCol] == CurrentPlayer)
                {
                    HighlitghtCell.Add(new Node() { Row = cRow - 1, Column = cCol - 1 });
                    cRow -= increRow;
                    cCol -= increCol;
                }
                else
                    break;
            }
        }

        public void GetHighlightCell(int row, int col)
        {
            HighlitghtCell.Clear();

            if (CheckWin(row, col, 1, 0))
            {
                GetHighlightCellList(row, col, 1, 0);
                return;
            }

            if (CheckWin(row, col, 0, 1))
            {
                GetHighlightCellList(row, col, 0, 1);
                return;
            }

            if (CheckWin(row, col, 1, 1))
            {
                GetHighlightCellList(row, col, 1, 1);
                return;
            }

            if (CheckWin(row, col, -1, 1))
            {
                GetHighlightCellList(row, col, -1, 1);
                return;
            }
        }

        #endregion

        #region Chơi với người chơi

        public void PlayerPlayAt(int row, int col)
        {
            row++;
            col++;

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

                PaintCellEvent(row - 1, col - 1);

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
                    GetHighlightCell(row, col);
                    HighlightCellWinnerEvent(HighlitghtCell);
                    PlayerWin(CurrentPlayer);
                }
            }
        }

        #endregion

        #region Chơi với máy

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            Node node = new Node();
            node = AMB.GetMoves(BoardCell);
            e.Result = node;
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Node node = (Node)e.Result;
            NextPlayer = CellValues.Machine;
            PCVsPlayerPlayAt(node.Row - 1, node.Column - 1);
        }

        public void PCVsPlayerPlayAt(int row, int col)
        {
            row++;
            col++;

            if (!IsInBoard(row, col))
                return;

            if (BoardCell[row, col] == CellValues.None)
            {
                if (NextPlayer != CellValues.None)
                {
                    BoardCell[row, col] = NextPlayer;
                    CurrentPlayer = NextPlayer;

                    if (CurrentPlayer == CellValues.Machine)
                        NextPlayer = CellValues.Player1;
                    else
                        NextPlayer = CellValues.None;

                    // Phát sự kiện tô màu
                    PaintCellEvent(row - 1, col - 1);

                    // Kiểm tra hòa
                    if (CheckDicken(row, col))
                    {
                        PlayerDicken();
                        return;
                    }

                    // Kiểm tra thắng
                    if (CheckWin(row, col, 1, 0) || CheckWin(row, col, 0, 1) || CheckWin(row, col, 1, 1) || CheckWin(row, col, -1, 1))
                    {
                        // Phat su kien co nguoi choi thang
                        GetHighlightCell(row, col);
                        HighlightCellWinnerEvent(HighlitghtCell);
                        PlayerWin(CurrentPlayer);
                        return;
                    }

                    if (CurrentPlayer == CellValues.Player1)
                        bw.RunWorkerAsync();
                }
            }
        }

        #endregion
    }
}
