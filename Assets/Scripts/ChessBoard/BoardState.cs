using UnityEngine;
using System.Collections.Generic;

/*
 * The BoardState class stores the internal data values of the board
 * It holds a list of BoardSquare structs that contains info for each square : the type of piece (pawn, king, ... , none) and the team of the piece
 * It also contains methods to get valid moves for each type of piece accoring to the current board configuration
 * It can apply a selected move for a piece and eventually reset its values to default
 */

public partial class ChessGameMgr
{
    public struct BoardPos
    {

        public int X { get; set; }
        public int Y { get; set; }

        //public BoardPos() { X = 0; Y = 0; }
        public BoardPos(int pos) { X = pos % BOARD_SIZE; Y = pos / BOARD_SIZE; }
        public BoardPos(int _x, int _y) { X = _x; Y = _y; }

        public static implicit operator int(BoardPos pos) { return pos.X + pos.Y * BOARD_SIZE; }

        static public int operator +(BoardPos pos1, BoardPos pos2)
        {
            int x = pos1.X + pos2.X;
            int y = pos1.Y + pos2.Y;

            return (x >= 0 && x < BOARD_SIZE && y >= 0 && y < BOARD_SIZE) ? new BoardPos(x, y) : -1;
        }

        public int GetRight()
        {
            return (X == BOARD_SIZE - 1) ? -1 : new BoardPos(X + 1, Y);
        }

        public int GetLeft()
        {
            return (X == 0) ? -1 : new BoardPos(X - 1, Y);
        }

        public int GetTop()
        {
            return (Y == BOARD_SIZE - 1) ? -1 : new BoardPos(X, Y + 1);
        }

        public int GetBottom()
        {
            return (Y == 0) ? -1 : new BoardPos(X, Y - 1);
        }
    }

    public class BoardState
    {
        public List<BoardSquare> Squares = null;
        public BitBoard bitBoard;

        public BoardState Clone()
        {
            BoardState clonedBoard = new BoardState();
            clonedBoard.Squares = Squares;
            clonedBoard.bitBoard = bitBoard;
            return clonedBoard;
        }

        public bool IsValidSquare(int pos, EChessTeam team, int teamFlag)
        {
            if (pos < 0)
                return false;
            PieceData data = bitBoard.GetPieceFromPos(pos);
            bool isTeamValid = (data.team == EChessTeam.None && ((teamFlag & (int) ETeamFlag.None) > 0)) ||
                ((data.team != team && data.team != EChessTeam.None) && ((teamFlag & (int) ETeamFlag.Enemy) > 0));

            return isTeamValid;
        }

        public void AddMoveIfValidSquare(EChessTeam team, int from, int to, List<Move> moves, int teamFlag = (int)ETeamFlag.Enemy | (int)ETeamFlag.None)
        {
            if (IsValidSquare(to, team, teamFlag))
            {
                Move move;
                move.From = from;
                move.To = to;
                moves.Add(move);
            }
        }

        public void GetValidKingMoves(EChessTeam team, int pos, List<Move> moves)
        {
            AddMoveIfValidSquare(team, pos, (new BoardPos(pos) + new BoardPos(1, 0)), moves);
            AddMoveIfValidSquare(team, pos, (new BoardPos(pos) + new BoardPos(1, 1)), moves);
            AddMoveIfValidSquare(team, pos, (new BoardPos(pos) + new BoardPos(0, 1)), moves);
            AddMoveIfValidSquare(team, pos, (new BoardPos(pos) + new BoardPos(-1, 1)), moves);
            AddMoveIfValidSquare(team, pos, (new BoardPos(pos) + new BoardPos(-1, 0)), moves);
            AddMoveIfValidSquare(team, pos, (new BoardPos(pos) + new BoardPos(-1, -1)), moves);
            AddMoveIfValidSquare(team, pos, (new BoardPos(pos) + new BoardPos(0, -1)), moves);
            AddMoveIfValidSquare(team, pos, (new BoardPos(pos) + new BoardPos(1, -1)), moves);
        }

        public void GetValidQueenMoves(EChessTeam team, int pos, List<Move> moves)
        {
            GetValidRookMoves(team, pos, moves);
            GetValidBishopMoves(team, pos, moves);
        }

        public void GetValidPawnMoves(EChessTeam team, int pos, List<Move> moves)
        {
            int FrontPos = -1, LeftFrontPos = -1, RightFrontPos = -1;
            if (team == EChessTeam.White)
            {
                FrontPos = new BoardPos(pos).GetTop();
                if (FrontPos != -1)
                {
                    LeftFrontPos = new BoardPos(FrontPos).GetLeft();
                    RightFrontPos = new BoardPos(FrontPos).GetRight();
                }
                if (new BoardPos(pos).Y == 1 && bitBoard.GetPieceFromPos(pos + BOARD_SIZE).piece == EPieceType.None)
                {
                    AddMoveIfValidSquare(team, pos, new BoardPos(FrontPos).GetTop(), moves, (int)ETeamFlag.None);
                }
            }
            else
            {
                FrontPos = new BoardPos(pos).GetBottom();
                if (FrontPos != -1)
                {
                    RightFrontPos = new BoardPos(FrontPos).GetLeft();
                    LeftFrontPos = new BoardPos(FrontPos).GetRight();
                }

                if (new BoardPos(pos).Y == 6 && bitBoard.GetPieceFromPos(pos - BOARD_SIZE).piece == EPieceType.None)
                {
                    AddMoveIfValidSquare(team, pos, new BoardPos(FrontPos).GetBottom(), moves, (int)ETeamFlag.None);
                }
            }

            AddMoveIfValidSquare(team, pos, FrontPos, moves, (int)ETeamFlag.None);
            AddMoveIfValidSquare(team, pos, LeftFrontPos, moves, (int)ETeamFlag.Enemy);
            AddMoveIfValidSquare(team, pos, RightFrontPos, moves, (int)ETeamFlag.Enemy);
        }

        public void GetValidRookMoves(EChessTeam team, int pos, List<Move> moves)
        {
            bool bBreak = false;
            int TopPos = new BoardPos(pos).GetTop();
            while (!bBreak && TopPos >= 0 && bitBoard.GetPieceFromPos(TopPos).team != team)
            {
                AddMoveIfValidSquare(team, pos, TopPos, moves);
                bBreak = bitBoard.GetPieceFromPos(TopPos).team != EChessTeam.None;
                TopPos = new BoardPos(TopPos).GetTop();
            }

            bBreak = false;
            int BottomPos = new BoardPos(pos).GetBottom();
            while (!bBreak && BottomPos >= 0 && bitBoard.GetPieceFromPos(BottomPos).team != team)
            {
                AddMoveIfValidSquare(team, pos, BottomPos, moves);
                bBreak = bitBoard.GetPieceFromPos(BottomPos).team != EChessTeam.None;
                BottomPos = new BoardPos(BottomPos).GetBottom();
            }

            bBreak = false;
            int LeftPos = new BoardPos(pos).GetLeft();
            while (!bBreak && LeftPos >= 0 && bitBoard.GetPieceFromPos(LeftPos).team != team)
            {
                AddMoveIfValidSquare(team, pos, LeftPos, moves);
                bBreak = bitBoard.GetPieceFromPos(LeftPos).team != EChessTeam.None;
                LeftPos = new BoardPos(LeftPos).GetLeft();
            }

            bBreak = false;
            int RightPos = new BoardPos(pos).GetRight();
            while (!bBreak && RightPos >= 0 && bitBoard.GetPieceFromPos(RightPos).team != team)
            {
                AddMoveIfValidSquare(team, pos, RightPos, moves);
                bBreak = bitBoard.GetPieceFromPos(RightPos).team != EChessTeam.None;
                RightPos = new BoardPos(RightPos).GetRight();
            }
        }

        public void GetValidBishopMoves(EChessTeam team, int pos, List<Move> moves)
        {
            bool bBreak = false;
            int TopRightPos = new BoardPos(pos) + new BoardPos(1, 1);
            while (!bBreak && TopRightPos >= 0 && bitBoard.GetPieceFromPos(TopRightPos).team != team)
            {

                AddMoveIfValidSquare(team, pos, TopRightPos, moves);
                bBreak = bitBoard.GetPieceFromPos(TopRightPos).team != EChessTeam.None;
                TopRightPos = new BoardPos(TopRightPos) + new BoardPos(1, 1);
            }

            bBreak = false;
            int TopLeftPos = new BoardPos(pos) + new BoardPos(-1, 1);
            while (!bBreak && TopLeftPos >= 0 && bitBoard.GetPieceFromPos(TopLeftPos).team != team)
            {

                AddMoveIfValidSquare(team, pos, TopLeftPos, moves);
                bBreak = bitBoard.GetPieceFromPos(TopLeftPos).team != EChessTeam.None;
                TopLeftPos = new BoardPos(TopLeftPos) + new BoardPos(-1, 1);
            }

            bBreak = false;
            int BottomRightPos = new BoardPos(pos) + new BoardPos(1, -1);
            while (!bBreak && BottomRightPos >= 0 && bitBoard.GetPieceFromPos(BottomRightPos).team != team)
            {

                AddMoveIfValidSquare(team, pos, BottomRightPos, moves);
                bBreak = bitBoard.GetPieceFromPos(BottomRightPos).team != EChessTeam.None;
                BottomRightPos = new BoardPos(BottomRightPos) + new BoardPos(1, -1);
            }

            bBreak = false;
            int BottomLeftPos = new BoardPos(pos) + new BoardPos(-1, -1);
            while (!bBreak && BottomLeftPos >= 0 && bitBoard.GetPieceFromPos(BottomLeftPos).team != team)
            {

                AddMoveIfValidSquare(team, pos, BottomLeftPos, moves);
                bBreak = bitBoard.GetPieceFromPos(BottomLeftPos).team != EChessTeam.None;
                BottomLeftPos = new BoardPos(BottomLeftPos) + new BoardPos(-1, -1);
            }
        }

        public void GetValidKnightMoves(EChessTeam team, int pos, List<Move> moves)
        {
            AddMoveIfValidSquare(team, pos, new BoardPos(pos) + new BoardPos(1, 2), moves);

            AddMoveIfValidSquare(team, pos, new BoardPos(pos) + new BoardPos(2, 1), moves);

            AddMoveIfValidSquare(team, pos, new BoardPos(pos) + new BoardPos(-1, 2), moves);

            AddMoveIfValidSquare(team, pos, new BoardPos(pos) + new BoardPos(-2, 1), moves);

            AddMoveIfValidSquare(team, pos, new BoardPos(pos) + new BoardPos(1, -2), moves);

            AddMoveIfValidSquare(team, pos, new BoardPos(pos) + new BoardPos(2, -1), moves);

            AddMoveIfValidSquare(team, pos, new BoardPos(pos) + new BoardPos(-1, -2), moves);

            AddMoveIfValidSquare(team, pos, new BoardPos(pos) + new BoardPos(-2, -1), moves);
        }

        public void GetValidMoves(EChessTeam team, List<Move> moves)
        {
            List<PieceData> pieceDatas = bitBoard.GetAllPiecesListOfColor(team);
            foreach (PieceData data in pieceDatas)
            {
                switch (data.piece)
                {
                    case EPieceType.King: GetValidKingMoves(team, data.pos, moves); break;
                    case EPieceType.Queen: GetValidQueenMoves(team, data.pos, moves); break;
                    case EPieceType.Pawn: GetValidPawnMoves(team, data.pos, moves); break;
                    case EPieceType.Rook: GetValidRookMoves(team, data.pos, moves); break;
                    case EPieceType.Bishop: GetValidBishopMoves(team, data.pos, moves); break;
                    case EPieceType.Knight: GetValidKnightMoves(team, data.pos, moves); break;
                    default: break;
                }
            }
        }

        public bool IsValidMove(EChessTeam team, Move move)
        {
            List<Move> validMoves = new List<Move>();
            GetValidMoves(team, validMoves);

            return validMoves.Contains(move);
        }

        // returns true if a pawn promotion occured
        public bool PlayUnsafeMove(Move move)
        {
            bitBoard.MovePiece(move);

            if (CanPromotePawn(move))
            {
                // promote pawn to queen
                bitBoard.PromoteTo(move.To, EPieceType.Queen);
                return true;
            }
            return false;
        }

        private bool CanPromotePawn(Move move)
        {
            PieceData tmpData = bitBoard.GetPieceFromPos(move.To);
            if (tmpData.piece == EPieceType.Pawn)
            {
                BoardPos pos = new BoardPos(move.To);
                if (tmpData.team == EChessTeam.Black && pos.Y == 0 || tmpData.team == EChessTeam.White && pos.Y == (BOARD_SIZE - 1))
                    return true;
            }
            return false;
        }

        // approximation : opponent king must be "eaten" to win instead of detecting checkmate state
        public bool DoesTeamLose(EChessTeam team)
        {
            BitBoard.BitBoardIndex index = bitBoard.EPieceTypeToBitBoardIndex(EPieceType.King, team);
            int idx = 0;
            int iterator = 0;

            bitBoard.FindFirstSetBit(bitBoard.GetBitBoard(index), ref idx, ref iterator);

            if (idx != -1)
                return false;
            return true;
        }

        public bool DoesTeamLoseForCustomBoard(EChessTeam team, BoardState tmpBoard)
        {
            BitBoard.BitBoardIndex index = tmpBoard.bitBoard.EPieceTypeToBitBoardIndex(EPieceType.King, team);
            int idx = 0;
            int iterator = 0;
            tmpBoard.bitBoard.FindFirstSetBit(tmpBoard.bitBoard.GetBitBoard(index), ref idx, ref iterator);
            if (idx != -1)
                return false;
            return true;
        }

        public void SetPieceAtSquare(int index, EChessTeam team, EPieceType piece)
        {
            BitBoard.BitBoardIndex tmpIndex = bitBoard.GetBitBoardIndex(team, piece);
            if(tmpIndex != BitBoard.BitBoardIndex.NONE)
                bitBoard.SetBitValue(tmpIndex, index, true);
            bitBoard.SetBitValue(BitBoard.BitBoardIndex.FREE_CASE, index, false);
            bitBoard.SetBitValue(BitBoard.BitBoardIndex.OCCUPED_CASE, index, true);
        }

        public void Reset()
        {
            if (Squares == null)
            {
                Squares = new List<BoardSquare>();
                bitBoard = new BitBoard();
                bitBoard.InitBitBoard();
                //bitBoard.Test();

                // init squares
                for (int i = 0; i < BOARD_SIZE * BOARD_SIZE; i++)
                {
                    bitBoard.SetBitValue(BitBoard.BitBoardIndex.FREE_CASE, i + 1, true);
                    bitBoard.SetBitValue(BitBoard.BitBoardIndex.OCCUPED_CASE, i + 1, false);
                }

            }
            else
            {
                bitBoard.InitBitBoard();
                for (int i = 0; i < Squares.Count; ++i)
                {
                    SetPieceAtSquare(i, EChessTeam.None, EPieceType.None);
                }
            }

            // White
            for (int i = BOARD_SIZE; i < BOARD_SIZE*2; ++i)
            {
                SetPieceAtSquare(i, EChessTeam.White, EPieceType.Pawn);
            }

            SetPieceAtSquare(0, EChessTeam.White, EPieceType.Rook);
            SetPieceAtSquare(1, EChessTeam.White, EPieceType.Knight);
            SetPieceAtSquare(2, EChessTeam.White, EPieceType.Bishop);
            SetPieceAtSquare(3, EChessTeam.White, EPieceType.Queen);
            SetPieceAtSquare(4, EChessTeam.White, EPieceType.King);
            SetPieceAtSquare(5, EChessTeam.White, EPieceType.Bishop);
            SetPieceAtSquare(6, EChessTeam.White, EPieceType.Knight);
            SetPieceAtSquare(7, EChessTeam.White, EPieceType.Rook);

            // Black
            for (int i = BOARD_SIZE * (BOARD_SIZE - 2) ; i < BOARD_SIZE * (BOARD_SIZE - 1); ++i)
            {
                SetPieceAtSquare(i, EChessTeam.Black, EPieceType.Pawn);
            }

            int startIndex = BOARD_SIZE * (BOARD_SIZE - 1);
            SetPieceAtSquare(startIndex, EChessTeam.Black, EPieceType.Rook);
            SetPieceAtSquare(startIndex + 1, EChessTeam.Black, EPieceType.Knight);
            SetPieceAtSquare(startIndex + 2, EChessTeam.Black, EPieceType.Bishop);
            SetPieceAtSquare(startIndex + 3, EChessTeam.Black, EPieceType.Queen);
            SetPieceAtSquare(startIndex + 4, EChessTeam.Black, EPieceType.King);
            SetPieceAtSquare(startIndex + 5, EChessTeam.Black, EPieceType.Bishop);
            SetPieceAtSquare(startIndex + 6, EChessTeam.Black, EPieceType.Knight);
            SetPieceAtSquare(startIndex + 7, EChessTeam.Black, EPieceType.Rook);

        }
    }
}

