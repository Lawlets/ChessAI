using UnityEngine;
using System.Collections.Generic;
using System.Threading;

/*
 * This singleton manages the whole chess game
 *  - board data (see BoardState class)
 *  - piece models instantiation
 *  - player interactions (piece grab, drag and release)
 *  - AI update calls (see UpdateAITurn and ChessAI class)
 */

public partial class ChessGameMgr : MonoBehaviour {

    #region singleton
    static ChessGameMgr instance = null;
    public static ChessGameMgr Instance {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<ChessGameMgr>();
            return instance;
        }
    }
    #endregion

    private ChessAI chessAI = null;
    private Transform boardTransform = null;
    private static int BOARD_SIZE = 8;
    private int pieceLayerMask;
    private int boardLayerMask;

    #region enums
    public enum EPieceType : int
    {
        None = -1,
        Pawn = 0,
        Rook,
        Bishop,
        Knight,
        Queen,
        King
    }

    public enum EChessTeam
    {
        White = 0,
        Black,
        None
    }

    public enum ETeamFlag : uint
    {
        None = 1 << 0,
        Friend = 1 << 1,
        Enemy = 1 << 2
    }
    #endregion

    #region structs and classes
    public struct BoardSquare
    {
        public EPieceType Piece;
        public EChessTeam Team;
    }

    public struct Move
    {
        public int From;
        public int To;

        public override bool Equals(object o)
        {
            try
            {
                return (bool)(this == (Move)o);
            }
            catch
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return From + To;
        }

        public static bool operator == (Move move1, Move move2)
        {
            return move1.From == move2.From && move1.To == move2.To;
        }

        public static bool operator != (Move move1, Move move2)
        {
            return move1.From != move2.From || move1.To != move2.To;
        }
    }

    #endregion

    #region chess game methods

    BoardState boardState = null;
    public BoardState GetBoardState() { return boardState; }

    private EChessTeam teamTurn;
    public EChessTeam TeamTurn
    {
        get { return teamTurn; }
    }

    List<uint> scores;

    public delegate void PlayerTurnEvent(bool isWhiteMove);
    public event PlayerTurnEvent OnPlayerTurn = null;

    public delegate void ScoreUpdateEvent(uint whiteScore, uint blackScore);
    public event ScoreUpdateEvent OnScoreUpdated = null;

    public void PrepareGame(bool resetScore = true)
    {
        chessAI = ChessAI.Instance;

        // Start game
        boardState.Reset();

        teamTurn = EChessTeam.White;
        if (scores == null)
        {
            scores = new List<uint>();
            scores.Add(0);
            scores.Add(0);
        }
        if (resetScore)
        {
            scores.Clear();
            scores.Add(0);
            scores.Add(0);
        }
    }

    public void PlayTurn(Move move)
    {
        if (boardState.IsValidMove(teamTurn, move))
        {
            
            if (boardState.PlayUnsafeMove(move))
            {
                // promote pawn to queen
                AddQueenAtPos(move.To);
            }

            EChessTeam otherTeam = (teamTurn == EChessTeam.White) ? EChessTeam.Black : EChessTeam.White;
            if (boardState.DoesTeamLose(otherTeam))
            {
                // increase score and reset board
                scores[(int)teamTurn]++;
                if (OnScoreUpdated != null)
                    OnScoreUpdated(scores[0], scores[1]);

                PrepareGame(false);
                // remove extra piece instances if pawn promotions occured
                teamPiecesArray[0].ClearPromotedPieces();
                teamPiecesArray[1].ClearPromotedPieces();
            }
            else
            {
                teamTurn = otherTeam;
            }
            // raise event
            if (OnPlayerTurn != null)
                OnPlayerTurn(teamTurn == EChessTeam.White);
        }
    }

    // used to instantiate newly promoted queen
    private void AddQueenAtPos(int pos)
    {
        teamPiecesArray[(int)teamTurn].AddPiece(EPieceType.Queen);
        Dictionary<EPieceType, GameObject> crtTeamPrefabs = (teamTurn == EChessTeam.White) ? WhitePiecesPrefab : BlackPiecesPrefab;
        GameObject crtPiece = Instantiate<GameObject>(crtTeamPrefabs[EPieceType.Queen]);
        teamPiecesArray[(int)teamTurn].StorePiece(crtPiece, EPieceType.Queen);
        crtPiece.transform.position = GetWorldPos(pos);
    }

    public bool IsPlayerTurn()
    {
        return teamTurn == EChessTeam.White;
    }

    public uint GetScore(EChessTeam team)
    {
        return scores[(int)team];
    }

    public int Evaluate(EChessTeam team, BoardState currentBoard)
    {
        return currentBoard.bitBoard.Evaluate(team);
        
    }

    private void UpdateBoardPiece(Transform pieceTransform, int destPos)
    {
        pieceTransform.position = GetWorldPos(destPos);
    }

    private Vector3 GetWorldPos(int pos)
    {
        Vector3 piecePos = boardTransform.position;
        piecePos.y += zOffset;
        piecePos.x = -widthOffset + pos % BOARD_SIZE;
        piecePos.z = -widthOffset + pos / BOARD_SIZE;

        return piecePos;
    }

    private int GetBoardPos(Vector3 worldPos)
    {
        int xPos = Mathf.FloorToInt(worldPos.x + widthOffset) % BOARD_SIZE;
        int zPos = Mathf.FloorToInt(worldPos.z + widthOffset);

        return xPos + zPos * BOARD_SIZE;
    }

    #endregion

    #region MonoBehaviour

    private TeamPieces[] teamPiecesArray = new TeamPieces[2];
    private float zOffset = 0.5f;
    private float widthOffset = 3.5f;

    // Use this for initialization
    void Start () {

        pieceLayerMask= 1 << LayerMask.NameToLayer("Piece");
        boardLayerMask = 1 << LayerMask.NameToLayer("Board");

        boardTransform = GameObject.FindGameObjectWithTag("Board").transform;

        LoadPiecesPrefab();

        boardState = new BoardState();

        PrepareGame();

        teamPiecesArray[0] = null;
        teamPiecesArray[1] = null;

        CreatePieces();

        if (OnPlayerTurn != null)
            OnPlayerTurn(teamTurn == EChessTeam.White);
        if (OnScoreUpdated != null)
            OnScoreUpdated(scores[0], scores[1]);
    }

	// Update is called once per frame
	void Update () {

        if (teamTurn == EChessTeam.White)
            UpdatePlayerTurn();
        else
            UpdateAITurn();
    }
    #endregion

    #region pieces

    Dictionary<EPieceType, GameObject> WhitePiecesPrefab = new Dictionary<EPieceType, GameObject>();
    Dictionary<EPieceType, GameObject> BlackPiecesPrefab = new Dictionary<EPieceType, GameObject>();

    void LoadPiecesPrefab()
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/Pieces/WhitePawn");
        WhitePiecesPrefab.Add(EPieceType.Pawn, prefab);
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/WhiteKing");
        WhitePiecesPrefab.Add(EPieceType.King, prefab);
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/WhiteQueen");
        WhitePiecesPrefab.Add(EPieceType.Queen, prefab);
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/WhiteBishop");
        WhitePiecesPrefab.Add(EPieceType.Bishop, prefab);
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/WhiteKnight");
        WhitePiecesPrefab.Add(EPieceType.Knight, prefab);
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/WhiteRook");
        WhitePiecesPrefab.Add(EPieceType.Rook, prefab);

        prefab = Resources.Load<GameObject>("Prefabs/Pieces/BlackPawn");
        BlackPiecesPrefab.Add(EPieceType.Pawn, prefab);
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/BlackKing");
        BlackPiecesPrefab.Add(EPieceType.King, prefab);
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/BlackQueen");
        BlackPiecesPrefab.Add(EPieceType.Queen, prefab);
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/BlackBishop");
        BlackPiecesPrefab.Add(EPieceType.Bishop, prefab);
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/BlackKnight");
        BlackPiecesPrefab.Add(EPieceType.Knight, prefab);
        prefab = Resources.Load<GameObject>("Prefabs/Pieces/BlackRook");
        BlackPiecesPrefab.Add(EPieceType.Rook, prefab);
    }

    void CreatePieces()
    {
        // Instantiate all pieces according to board data
        if (teamPiecesArray[0] == null)
            teamPiecesArray[0] = new TeamPieces();
        if (teamPiecesArray[1] == null)
            teamPiecesArray[1] = new TeamPieces();

        Dictionary<EPieceType, GameObject> crtTeamPrefabs = null;
            
        foreach (PieceData data in boardState.bitBoard.GetAllPiecesList())
        {
            crtTeamPrefabs = (data.team == EChessTeam.White) ? WhitePiecesPrefab : BlackPiecesPrefab;
            if (data.piece != EPieceType.None)
            {
                GameObject crtPiece = Instantiate<GameObject>(crtTeamPrefabs[data.piece]);
                teamPiecesArray[(int)data.team].StorePiece(crtPiece, data.piece);
        
                // set position
                Vector3 piecePos = boardTransform.position;
                piecePos.y += zOffset;
                piecePos.x = -widthOffset + data.pos % BOARD_SIZE;
                piecePos.z = -widthOffset + data.pos / BOARD_SIZE;
                crtPiece.transform.position = piecePos;
            }
        }
    }

    public void UpdatePieces(BoardState board)
    {
        teamPiecesArray[0].Hide();
        teamPiecesArray[1].Hide();

        for (int i = 0; i < 64; i++)
        {
            PieceData data = board.bitBoard.GetPieceFromPos(i);

            if (data.team == EChessTeam.None)
                continue;
            teamPiecesArray[(int)data.team].SetPieceAtPos(data.piece, GetWorldPos(i));
        }
    }

    public void UpdatePieces()
    {
        teamPiecesArray[0].Hide();
        teamPiecesArray[1].Hide();

        for (int i = 0; i < 64; i++)
        {
            PieceData data = boardState.bitBoard.GetPieceFromPos(i);

            if (data.team == EChessTeam.None)
                continue;
            teamPiecesArray[(int)data.team].SetPieceAtPos(data.piece, GetWorldPos(i));
        }
    }

    #endregion

    #region gameplay

    Transform grabbed = null;
    float maxDistance = 100f;
    int startPos = 0;
    int destPos = 0;

    void UpdateAITurn()
    {
        Move move = chessAI.ComputeMove();
        PlayTurn(move);

        UpdatePieces();
    }

    public void MakeMove(EChessTeam currentTeam, Move move, BoardState currentBoard)
    {
        currentBoard.bitBoard.MovePiece(move);
    }

    void UpdatePlayerTurn()
    {
        if (Input.GetMouseButton(0))
        {
            if (grabbed)
                ComputeDrag();
            else
                ComputeGrab();
        }
        else if (grabbed != null)
        {
            // find matching square when releasing grabbed piece
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, maxDistance, boardLayerMask))
            {
                grabbed.root.position = hit.transform.position + Vector3.up * zOffset;
            }

            destPos = GetBoardPos(grabbed.root.position);
            if (startPos != destPos)
            {
                Move move = new Move();
                move.From = startPos;
                move.To = destPos;

                PlayTurn(move);

                UpdatePieces();
            }
            else
            {
                grabbed.root.position = GetWorldPos(startPos);
            }
            grabbed = null;
        }
    }

    void ComputeDrag()
    {
        // drag grabbed piece on board
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, maxDistance, boardLayerMask))
        {
            grabbed.root.position = hit.point;
        }
    }

    void ComputeGrab()
    {
        // grab a new chess piece from board
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit, maxDistance, pieceLayerMask))
        {
            grabbed = hit.transform;
            startPos = GetBoardPos(hit.transform.position);
        }
    }

    #endregion
}
