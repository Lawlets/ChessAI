using UnityEngine;
using System.Collections.Generic;
using System.Collections;

/*
 * This class computes AI move decision
 * ComputeMove method is called from ChessGameMgr during AI update turn
 */

public class ChessAI : MonoBehaviour {

    #region singleton
    static ChessAI instance = null;
    public static ChessAI Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<ChessAI>();
            return instance;
        }
    }
    #endregion

    #region AI

    private int aiCount;
    private int maxDepth = 100;

    public ChessGameMgr.Move ComputeMove()
    {
        aiCount = 0;
        maxDepth = 4;

        List<ChessGameMgr.Move> moves = new List<ChessGameMgr.Move>();
        ChessGameMgr.Instance.GetBoardState().GetValidMoves(ChessGameMgr.Instance.TeamTurn, moves);

        Result miniMaxResult = MiniMax(ChessGameMgr.Instance.GetBoardState(), ChessGameMgr.Instance.TeamTurn, 0, int.MinValue, int.MaxValue);
        return miniMaxResult.move;
    }

    Result MiniMax(ChessGameMgr.BoardState board, ChessGameMgr.EChessTeam team, int currentDepth, int alpha, int beta)
    {
        aiCount++;
        ChessGameMgr.EChessTeam otherTeam = (team == ChessGameMgr.EChessTeam.White) ? ChessGameMgr.EChessTeam.Black : ChessGameMgr.EChessTeam.White;
        if (board.DoesTeamLoseForCustomBoard(ChessGameMgr.EChessTeam.White, board) || board.DoesTeamLoseForCustomBoard(ChessGameMgr.EChessTeam.Black, board) || currentDepth == maxDepth)
        {
            Result res = new Result();
            res.score = ChessGameMgr.Instance.Evaluate(team, board);
            return res;
        }

        ChessGameMgr.Move bestMove = new ChessGameMgr.Move();
        int bestScore = int.MaxValue;

        
        if (ChessGameMgr.Instance.TeamTurn == team)
            bestScore = int.MinValue;

        List<ChessGameMgr.Move> moves = new List<ChessGameMgr.Move>();
        ChessGameMgr.Instance.GetBoardState().GetValidMoves(team, moves);

        foreach (ChessGameMgr.Move move in moves)
        {
            ChessGameMgr.BoardState newBoard = board.Clone();

            ChessGameMgr.PieceData dataFrom = newBoard.bitBoard.GetPieceFromPos(move.From);
            ChessGameMgr.PieceData dataTo = newBoard.bitBoard.GetPieceFromPos(move.To);

            ChessGameMgr.Instance.MakeMove(team, move, newBoard);
            ChessGameMgr.Instance.UpdatePieces(newBoard);

            // recurse MiniMax
            Result recursedRes = MiniMax(newBoard, otherTeam, currentDepth + 1, alpha, beta);

            ChessGameMgr.Move tmpMove = new ChessGameMgr.Move();
            tmpMove.From = move.To;
            tmpMove.To = move.From;
            ChessGameMgr.Instance.MakeMove(team, tmpMove, newBoard);
            if(dataTo.piece != ChessGameMgr.EPieceType.None)
                newBoard.SetPieceAtSquare(dataTo.pos, dataTo.team, dataTo.piece);

            ChessGameMgr.Instance.UpdatePieces(newBoard);

            // update the best score
            if (ChessGameMgr.Instance.TeamTurn == team)
            {
                if (recursedRes.score > bestScore) // maximize score
                {
                    bestScore = recursedRes.score;
                    bestMove = move;
                }
                alpha = Mathf.Max(alpha, bestScore);
            }
            else
            {
                if (recursedRes.score < bestScore) // minimize score
                {
                    bestScore = recursedRes.score;
                    bestMove = move;
                }
                beta = Mathf.Min(beta, bestScore);
            }

            if (beta <= alpha)
                break;
        }
        Result bestRes = new Result();
        bestRes.move = bestMove;
        bestRes.score = bestScore;

        return bestRes;
    }

    #endregion

    #region monobehaviour

    // Use this for initialization
    void Start () {
	
	}

    // Update is called once per frame
    void Update()
    {
    }
    #endregion
}

public struct Result
{
    public Result(ChessGameMgr.Move _move, int _score)
    {
        move = _move;
        score = _score;
    }
    public ChessGameMgr.Move move;
    public int score;
}
