using System;
using System.Collections.Generic;
using UnityEngine;


public partial class ChessGameMgr
{
    public struct PieceData
    {
        public PieceData(EPieceType _ePiece, EChessTeam _eTeam, int _ePos)
        {
            piece = _ePiece;
            team = _eTeam;
            pos = _ePos;
        }

        public EPieceType piece;
        public EChessTeam team;
        public int pos;
    }

    public struct BitBoard
    {

        public enum BitBoardIndex
        {
            NONE = -1,
            WHITE_PAWN = 0,
            WHITE_ROOK = 1,
            WHITE_BISHOP = 2,
            WHITE_KNIGHT = 3,
            WHITE_QUEEN = 4,
            WHITE_KING = 5,

            BLACK_PAWN = 6,
            BLACK_ROOK = 7,
            BLACK_BISHOP = 8,
            BLACK_KNIGHT = 9,
            BLACK_QUEEN = 10,
            BLACK_KING = 11,

            FREE_CASE = 12,
            OCCUPED_CASE = 13,
            FULL_BOARD = 14,
        }

        public enum FullColorBitBoard { 
            BLACK_PIECES = 0,
            WHITE_PIECES = 1
        }

        private long[] bitBoardArray;
        private long[] fullColorBitBoardArray;

        public void InitBitBoard()
        {
            bitBoardArray = new long[14];
            fullColorBitBoardArray = new long[2];
        }

        public BitBoardIndex GetBitBoardIndex(EChessTeam team, EPieceType piece)
        {
            if (team == EChessTeam.None)
                return BitBoardIndex.NONE;

            return EPieceTypeToBitBoardIndex(piece, team);
        }

        public long GetAllColorBitBoard(FullColorBitBoard color)
        {
            fullColorBitBoardArray[(int)color] = 0;

            if (color == FullColorBitBoard.WHITE_PIECES)
            {
                fullColorBitBoardArray[(int)color] |= bitBoardArray[(int)BitBoardIndex.WHITE_PAWN];
                fullColorBitBoardArray[(int)color] |= bitBoardArray[(int)BitBoardIndex.WHITE_ROOK];
                fullColorBitBoardArray[(int)color] |= bitBoardArray[(int)BitBoardIndex.WHITE_BISHOP];
                fullColorBitBoardArray[(int)color] |= bitBoardArray[(int)BitBoardIndex.WHITE_KNIGHT];
                fullColorBitBoardArray[(int)color] |= bitBoardArray[(int)BitBoardIndex.WHITE_QUEEN];
                fullColorBitBoardArray[(int)color] |= bitBoardArray[(int)BitBoardIndex.WHITE_KING];

            }
            else if (color == FullColorBitBoard.BLACK_PIECES)
            {
                fullColorBitBoardArray[(int)color] |= bitBoardArray[(int)BitBoardIndex.BLACK_PAWN];
                fullColorBitBoardArray[(int)color] |= bitBoardArray[(int)BitBoardIndex.BLACK_ROOK];
                fullColorBitBoardArray[(int)color] |= bitBoardArray[(int)BitBoardIndex.BLACK_BISHOP];
                fullColorBitBoardArray[(int)color] |= bitBoardArray[(int)BitBoardIndex.BLACK_KNIGHT];
                fullColorBitBoardArray[(int)color] |= bitBoardArray[(int)BitBoardIndex.BLACK_QUEEN];
                fullColorBitBoardArray[(int)color] |= bitBoardArray[(int)BitBoardIndex.BLACK_KING];
            }
            return fullColorBitBoardArray[(int)color];
        }

        public long GetBitBoard(BitBoardIndex index)
        {
            return bitBoardArray[(int)index];
        }

        #region BitOperation
        
        public void SetBitValue(BitBoardIndex bitBoard, int index, bool state)
        {
            long tmpBoardValue = bitBoardArray[(int)bitBoard];
            bitBoardArray[(int)bitBoard] = (state) ? tmpBoardValue | ((long)1 << index) : tmpBoardValue & ~((long)1 << index);
        }

        public void SetBitValue(FullColorBitBoard color, int index, bool state)
        {
            long tmpBoardValue = fullColorBitBoardArray[(int)color];
            fullColorBitBoardArray[(int)color] = (state) ? tmpBoardValue | ((long)1 << index) : tmpBoardValue & ~((long)1 << index);
        }

        private bool GetBitValue(BitBoardIndex bitBoard, int index)
        {
            bool bit = (bitBoardArray[(int)bitBoard] & ((long)1 << index)) != 0;
            return bit;
        }
        private bool GetBitValue(long bitBoard, int index)
        {
            bool bit = (bitBoard & ((long)1 << index)) != 0;
            return bit;
        }
        private bool GetBitValue(FullColorBitBoard color, int index)
        {
            bool bit = (fullColorBitBoardArray[(int)color] & ((long)1 << index)) != 0;
            return bit;
        }

        public void FindFirstSetBit(long bitBoard, ref int bitIndex, ref int iteratorIdx)
        {
            while (iteratorIdx != 64)
            {
                if (GetBitValue(bitBoard, iteratorIdx))
                {
                    bitIndex = iteratorIdx;
                    iteratorIdx++;
                    return;
                }

                iteratorIdx++;
            }

            bitIndex = -1;
        }

        #endregion

        #region WhiteMove

        public List<PieceData> GetAllWhitePieceslist()
        {
            List<PieceData> pieceList = new List<PieceData>();
            long tmpWhitePieceLong = GetAllColorBitBoard(FullColorBitBoard.WHITE_PIECES);

            long longPiece = bitBoardArray[(int)BitBoardIndex.WHITE_KING] & tmpWhitePieceLong;
            GetKingPiece(ref pieceList, longPiece, EChessTeam.White);

            longPiece = bitBoardArray[(int)BitBoardIndex.WHITE_QUEEN] & tmpWhitePieceLong;
            GetQueenPiece(ref pieceList, longPiece, EChessTeam.White);

            longPiece = bitBoardArray[(int)BitBoardIndex.WHITE_KNIGHT] & tmpWhitePieceLong;
            GetKnightPiece(ref pieceList, longPiece, EChessTeam.White);

            longPiece = bitBoardArray[(int)BitBoardIndex.WHITE_BISHOP] & tmpWhitePieceLong;
            GetBishopPiece(ref pieceList, longPiece, EChessTeam.White);

            longPiece = bitBoardArray[(int)BitBoardIndex.WHITE_ROOK] & tmpWhitePieceLong;
            GetRookPiece(ref pieceList, longPiece, EChessTeam.White);

            longPiece = bitBoardArray[(int)BitBoardIndex.WHITE_PAWN] & tmpWhitePieceLong;
            GetPawnPiece(ref pieceList, longPiece, EChessTeam.White);

            return pieceList;
        }

        private PieceData GetWhitePieceFromPos(int pos)
        {
            int idx = (int)BitBoardIndex.WHITE_PAWN;
            for (; idx <= (int)BitBoardIndex.WHITE_KING; idx++)
            {
                if (GetBitValue(bitBoardArray[idx], pos))
                {
                    return new PieceData(BitBoardIndexToEPieceType((BitBoardIndex)idx), EChessTeam.White, pos);
                }
            }
            return new PieceData(EPieceType.None, EChessTeam.None, -1);
        }

        private List<Move> GetValideMovementForWhitePawn()
        {
            return null;
        }

        private List<Move> GetValideMovementForWhiteRook()
        {
            return null;
        }

        private List<Move> GetValideMovementForWhiteBishop()
        {
            return null;
        }

        private List<Move> GetValideMovementForWhiteKnight()
        {
            return null;
        }

        private List<Move> GetValideMovementForWhiteQueen()
        {
            return null;
        }

        private List<Move> GetValideMovementForWhiteKing()
        {
            return null;
        }

        #endregion

        #region BlackMove

        public List<PieceData> GetAllBlackPiecesList()
        {
            List<PieceData> pieceList = new List<PieceData>();
            long tmpBlackPieceLong = GetAllColorBitBoard(FullColorBitBoard.BLACK_PIECES);

            long longPiece = bitBoardArray[(int)BitBoardIndex.BLACK_KING] & tmpBlackPieceLong;
            GetKingPiece(ref pieceList, longPiece, EChessTeam.Black);

            longPiece = bitBoardArray[(int)BitBoardIndex.BLACK_QUEEN] & tmpBlackPieceLong;
            GetQueenPiece(ref pieceList, longPiece, EChessTeam.Black);

            longPiece = bitBoardArray[(int)BitBoardIndex.BLACK_KNIGHT] & tmpBlackPieceLong;
            GetKnightPiece(ref pieceList, longPiece, EChessTeam.Black);

            longPiece = bitBoardArray[(int)BitBoardIndex.BLACK_BISHOP] & tmpBlackPieceLong;
            GetBishopPiece(ref pieceList, longPiece, EChessTeam.Black);

            longPiece = bitBoardArray[(int)BitBoardIndex.BLACK_ROOK] & tmpBlackPieceLong;
            GetRookPiece(ref pieceList, longPiece, EChessTeam.Black);

            longPiece = bitBoardArray[(int)BitBoardIndex.BLACK_PAWN] & tmpBlackPieceLong;
            GetPawnPiece(ref pieceList, longPiece, EChessTeam.Black);

            return pieceList;
        }

        private PieceData GetBlackPieceFromPos(int pos)
        {
            int idx = (int)BitBoardIndex.BLACK_PAWN;
            for (; idx <= (int) BitBoardIndex.BLACK_KING; idx++)
            {
                if (GetBitValue(bitBoardArray[idx], pos))
                    return new PieceData(BitBoardIndexToEPieceType((BitBoardIndex)idx), EChessTeam.Black, pos);
            }
            return new PieceData(EPieceType.None, EChessTeam.None, -1);
        }

        private List<Move> GetValideMovementForBlackPawn()
        {
            return null;
        }

        private List<Move> GetValideMovementForBlackRook()
        {
            return null;
        }

        private List<Move> GetValideMovementForBlackBishop()
        {
            return null;
        }

        private List<Move> GetValdieMovementForBlackKnight()
        {
            return null;
        }

        private List<Move> GetValideMovementForBlackQueen()
        {
            return null;
        }

        private List<Move> GetValideMovementForBlackKing()
        {
            return null;
        }

        #endregion

        #region GenericMove

        public EPieceType BitBoardIndexToEPieceType(BitBoardIndex index)
        {
            if ((index == BitBoardIndex.BLACK_PAWN) || (index == BitBoardIndex.WHITE_PAWN))
                return EPieceType.Pawn;
            else if ((index == BitBoardIndex.BLACK_ROOK) || (index == BitBoardIndex.WHITE_ROOK))
                return EPieceType.Rook;
            else if ((index == BitBoardIndex.BLACK_BISHOP) || (index == BitBoardIndex.WHITE_BISHOP))
                return EPieceType.Bishop;
            else if ((index == BitBoardIndex.BLACK_KNIGHT) || (index == BitBoardIndex.WHITE_KNIGHT))
                return EPieceType.Knight;
            else if ((index == BitBoardIndex.BLACK_QUEEN) || (index == BitBoardIndex.WHITE_QUEEN))
                return EPieceType.Queen;
            else if ((index == BitBoardIndex.BLACK_KING) || (index == BitBoardIndex.WHITE_KING))
                return EPieceType.King;

            return EPieceType.None;
        }

        public BitBoardIndex EPieceTypeToBitBoardIndex(EPieceType piece, EChessTeam team)
        {
            if(team == EChessTeam.None)
                throw new ArgumentException("team", team.ToString());
            string s_team = (team == EChessTeam.White) ? "WHITE_" : "BLACK_";
            s_team += piece.ToString();
            return (BitBoardIndex)Enum.Parse(typeof(BitBoardIndex), s_team.ToUpper(), true);
        }

        private void GetAllPiecesOfType(ref List<PieceData> list, long bitBoard, EPieceType piece, EChessTeam team)
        {
            int bitIndex = 0;
            int iterator = 0;
            while (bitIndex != -1)
            {
                FindFirstSetBit(bitBoard, ref bitIndex, ref iterator);

                if (bitIndex == -1)
                    return;

                list.Add(new PieceData(piece, team, bitIndex));
            }
        }

        private void GetKingPiece(ref List<PieceData> list, long bitBoard, EChessTeam team)
        {
            GetAllPiecesOfType(ref list, bitBoard, EPieceType.King, team);
        }

        private void GetQueenPiece(ref List<PieceData> list, long bitBoard, EChessTeam team)
        {
            GetAllPiecesOfType(ref list, bitBoard, EPieceType.Queen, team);
        }

        private void GetKnightPiece(ref List<PieceData> list, long bitBoard, EChessTeam team)
        {
            GetAllPiecesOfType(ref list, bitBoard, EPieceType.Knight, team);
        }

        private void GetBishopPiece(ref List<PieceData> list, long bitBoard, EChessTeam team)
        {
            GetAllPiecesOfType(ref list, bitBoard, EPieceType.Bishop, team);
        }

        private void GetRookPiece(ref List<PieceData> list, long bitBoard, EChessTeam team)
        {
            GetAllPiecesOfType(ref list, bitBoard, EPieceType.Rook, team);
        }

        private void GetPawnPiece(ref List<PieceData> list, long bitBoard, EChessTeam team)
        {
            GetAllPiecesOfType(ref list, bitBoard, EPieceType.Pawn, team);
        }

        public PieceData GetPieceFromPos(int pos)
        {
            EChessTeam team = GetColorFromPos(pos);
            switch (team)
            {
                case EChessTeam.White: return GetWhitePieceFromPos(pos);
                case EChessTeam.Black: return GetBlackPieceFromPos(pos);
                case EChessTeam.None: return new PieceData(EPieceType.None, EChessTeam.None, pos);
                default: throw new ArgumentException("team", team.ToString(), null);
            }
        }

        public PieceData GetPieceFromPos(int pos, EChessTeam team)
        {
            switch (team)
            {
                case EChessTeam.White: return GetWhitePieceFromPos(pos);
                case EChessTeam.Black: return GetBlackPieceFromPos(pos);
                case EChessTeam.None: return new PieceData(EPieceType.None, EChessTeam.None, pos);
                default: throw new ArgumentException("team", team.ToString(), null);
            }
        }

        public List<PieceData> GetAllPiecesListOfColor(EChessTeam team)
        {
            switch (team)
            {
                case EChessTeam.White: return GetAllWhitePieceslist();
                case EChessTeam.Black: return GetAllBlackPiecesList();
                default: throw new ArgumentException("team: ", team.ToString(), null);
            }
        }

        public List<PieceData> GetAllPiecesList()
        {
            List<PieceData> list = GetAllWhitePieceslist();
            List<PieceData> BlackList = GetAllBlackPiecesList();
            foreach (PieceData data in BlackList)
                list.Add(data);

            return list;
        }

        public void MovePiece(Move move)
        {
            EChessTeam color = GetColorFromPos(move.From);
            PieceData pieceFrom = GetPieceFromPos(move.From, color);

            color = GetColorFromPos(move.To);
            PieceData pieceTo = GetPieceFromPos(move.To, color);

            SetBitValue(EPieceTypeToBitBoardIndex(pieceFrom.piece, pieceFrom.team), pieceTo.pos, true);
            SetBitValue(EPieceTypeToBitBoardIndex(pieceFrom.piece, pieceFrom.team), pieceFrom.pos, false);

            if (pieceFrom.team == EChessTeam.White)
            {
                SetBitValue(FullColorBitBoard.WHITE_PIECES, pieceFrom.pos, false);
                SetBitValue(FullColorBitBoard.WHITE_PIECES, pieceTo.pos, true);
            } else if (pieceFrom.team == EChessTeam.Black)
            {
                SetBitValue(FullColorBitBoard.BLACK_PIECES, pieceFrom.pos, false);
                SetBitValue(FullColorBitBoard.BLACK_PIECES, pieceTo.pos, true);
            }

            if (color != EChessTeam.None)
            {
                SetBitValue(EPieceTypeToBitBoardIndex(pieceTo.piece, pieceTo.team), pieceTo.pos, false);
                if (pieceTo.team == EChessTeam.White)
                    SetBitValue(FullColorBitBoard.WHITE_PIECES, pieceTo.pos, false);
                else if (pieceFrom.team == EChessTeam.Black)
                    SetBitValue(FullColorBitBoard.BLACK_PIECES, pieceTo.pos, false);
            }

            SetBitValue(BitBoardIndex.FREE_CASE, pieceFrom.pos, true);
            SetBitValue(BitBoardIndex.FREE_CASE, pieceTo.pos, false);
            SetBitValue(BitBoardIndex.OCCUPED_CASE, pieceFrom.pos, false);
            SetBitValue(BitBoardIndex.OCCUPED_CASE, pieceTo.pos, true);
        }

        public void PromoteTo(int pos, EPieceType piece)
        {
            EChessTeam color = GetColorFromPos(pos);
            PieceData data = GetPieceFromPos(pos, color);
            SetBitValue(EPieceTypeToBitBoardIndex(data.piece, color), pos, false);
            SetBitValue(EPieceTypeToBitBoardIndex(piece, color), pos, true);
        }

        public EChessTeam GetColorFromPos(int pos)
        {
            if (GetPieceFromPos(pos, EChessTeam.White).team != EChessTeam.None)
                return EChessTeam.White;
            else if (GetPieceFromPos(pos, EChessTeam.Black).team != EChessTeam.None)
                return EChessTeam.Black;

            return EChessTeam.None;
        }

        private void EvaluateTeam(List<PieceData> dataList, out int score)
        {
            score = 0;
            foreach (PieceData data in dataList)
            {
                if (data.piece == EPieceType.King)
                    score += 150;
                else if (data.piece == EPieceType.Queen)
                    score += 9;
                else if (data.piece == EPieceType.Knight)
                    score += 5;
                else if (data.piece == EPieceType.Bishop)
                    score += 3;
                else if (data.piece == EPieceType.Rook)
                    score += 3;
                else if (data.piece == EPieceType.Pawn)
                    score += 1;
            }
        }

        public int Evaluate(EChessTeam team)
        {
            if(team == EChessTeam.None)
                throw new ArgumentException("team: ", team.ToString(), null);
            int score;
            int otherTeamScore;

            List<PieceData> currentTeamData = GetAllPiecesListOfColor(team);
            EvaluateTeam(currentTeamData, out score);

            EChessTeam otherTeam = (team == EChessTeam.White) ? EChessTeam.Black : EChessTeam.White;
            List<PieceData> otherTeamData = GetAllPiecesListOfColor(otherTeam);
            EvaluateTeam(otherTeamData, out otherTeamScore);

            int tmpScore = 189 - otherTeamScore;
            return score + Mathf.Abs(tmpScore);
        }

        #endregion

        #region Debug

        public void Print(BitBoardIndex bitBoard)
        {
            string result = "";
            for(int idx = 0; idx != 64; idx++)
            {
                if (idx == 4 || idx == 8 || idx == 12 || idx == 16 || idx == 20 || idx == 24 || idx == 28 || idx == 32 ||
                    idx == 36 || idx == 40 || idx == 44 || idx == 48 || idx == 52 || idx == 56 || idx == 60 || idx == 64)
                    result += "\n";
                result += GetBitValue(bitBoard, idx).ToString() + " ¦ ";
            }
            Debug.Log(result);
        }

        public void Print(FullColorBitBoard color)
        {
            GetAllColorBitBoard(color);
            string result = "";
            for (int idx = 0; idx != 64; idx++)
            {
                if (idx == 4 || idx == 8 || idx == 12 || idx == 16 || idx == 20 || idx == 24 || idx == 28 || idx == 32 ||
                    idx == 36 || idx == 40 || idx == 44 || idx == 48 || idx == 52 || idx == 56 || idx == 60 || idx == 64)
                    result += "\n";
                result += GetBitValue(color, idx).ToString() + " ¦ ";
            }
            Debug.Log(result);
        }

        public void Test()
        {
            //Debug.Log("size of int [" + sizeof(int) + "] ¦ sizeof of long [" + sizeof(long) + "]");

            InitBitBoard();

            //bitBoardArray[(int)BitBoardIndex.WHITE_PAWN] = 8;
            //Debug.Log("bit 4 value of WHITE_PAWN: " + GetBitValue(BitBoardIndex.WHITE_PAWN, 3));
            //Debug.Log("bit 4 value of WHITE_KING: " + GetBitValue(BitBoardIndex.WHITE_KING, 3));
            ////
            //SetBitValue(BitBoardIndex.WHITE_PAWN, 3, false);
            //Debug.Log("set bit 4 of WHITE_PAWN false: " + GetBitValue(BitBoardIndex.WHITE_PAWN, 3));
            ////
            //SetBitValue(BitBoardIndex.WHITE_KING, 4, true);
            //Debug.Log("set bit 5 of WHITE_KING true: " + GetBitValue(BitBoardIndex.WHITE_KING, 4));
            //Debug.Log("print of bit board WHITE_KING: ");
            //Print(BitBoardIndex.WHITE_KING);
            ////
            //int bitIndex = 0;
            //int iterator = 1;
            //FindFirstSetBit(bitBoardArray[(int)BitBoardIndex.WHITE_KING], ref bitIndex, ref iterator);
            //Debug.Log("Find first set bit of WHITE_KING: BitIndex[" + bitIndex + "] ¦¦ iterator [" + iterator + "]");
            ////
            //PieceData tmpData = GetPieceFromPos(bitIndex, EChessTeam.White);
            //Debug.Log("GetPieceFromPose [" + tmpData.pos + "] : PieceType [" + tmpData.piece.ToString() + "] ¦¦ Team [" +
            //          tmpData.team.ToString() + "]");
            
            //Debug.Log("EPieceTypeToBitBoardIndex: Convert ["+EPieceType.King.ToString()+"] to ["+EPieceTypeToBitBoardIndex(EPieceType.King, EChessTeam.White).ToString()+"]");
            //Debug.Log("BitBoardIndexToEPieceType: Convert [" + BitBoardIndex.BLACK_KNIGHT.ToString() + "] to [" + BitBoardIndexToEPieceType(BitBoardIndex.BLACK_KNIGHT).ToString() + "]");

            //SetBitValue(BitBoardIndex.WHITE_KING, 4, true);
            //Print(FullColorBitBoard.WHITE_PIECES);
            //Print(BitBoardIndex.FREE_CASE);
            //
            //Move tmpMove = new Move();
            //tmpMove.From = 4;
            //tmpMove.To = 12;
            //MovePiece(tmpMove);
            //Debug.Log("Move from ["+ tmpMove.From + "] to ["+ tmpMove.To + "]");
            //Debug.Log("Print Full White Board to see move");
            //Print(FullColorBitBoard.WHITE_PIECES);
            //Debug.Log("Print Full Free case to see move");
            //Print(BitBoardIndex.FREE_CASE);
        }

        #endregion
    }
}
