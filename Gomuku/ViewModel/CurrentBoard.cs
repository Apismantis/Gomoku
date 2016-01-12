using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gomoku.Model;

namespace Gomoku.ViewModel
{
    public class CurrentBoard
    {
        private Board BoardChess;
        private PCBoard PCBoardChess;
        
        public CurrentBoard()
        {
            BoardChess = new Board();
            PCBoardChess = new PCBoard();
            BoardChess.PlayerWin += PlayerWinEvent;
            PCBoardChess.PlayerWin += PlayerWinEvent;
            BoardChess.PlayerDicken += DickenGameEvent;
        }

        public Board GetBoardChess()
        {
            return BoardChess;
        }

        public PCBoard GetPCBoardChess()
        {
            return PCBoardChess;
        }

        public delegate void EndGameHandle();

        public event EndGameHandle EndGame;

        public void PlayerWinEvent()
        {
            // Phat di su kien ket thuc game
            EndGame();
        }

        public delegate void DickenGameHandle();

        public event DickenGameHandle DickenGame;

        public void DickenGameEvent()
        {
            // Phat di su kien hoa game
            DickenGame();
        }

        public void PlayAt(int row, int col, int mode)
        {
            if (mode == 1)
                BoardChess.PlayAt(row, col);
            else
            {
                PCBoardChess.PlayAt(row, col);
            }
        }
    }
}
