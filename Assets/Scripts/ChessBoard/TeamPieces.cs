using System;
using UnityEngine;
using System.Collections.Generic;

/*
 * This class holds chess piece gameObjects for a team
 * Instantiated for each team by the ChessGameMgr
 * It can hide or set a piece at a given position
 */

public partial class ChessGameMgr
{
    public class TeamPieces
    {
        private Dictionary<EPieceType, GameObject[]> pieceTypeDict;

        public TeamPieces()
        {
            pieceTypeDict = new Dictionary<EPieceType, GameObject[]>();

            pieceTypeDict.Add(EPieceType.Pawn, new GameObject[BOARD_SIZE]);
            pieceTypeDict.Add(EPieceType.Rook, new GameObject[2]);
            pieceTypeDict.Add(EPieceType.Bishop, new GameObject[2]);
            pieceTypeDict.Add(EPieceType.Knight, new GameObject[2]);
            pieceTypeDict.Add(EPieceType.Queen, new GameObject[1]);
            pieceTypeDict.Add(EPieceType.King, new GameObject[1]);
        }

        // Add a piece during gameplay - used for pawn promotion
        public void AddPiece(EPieceType type)
        {
            GameObject[] pieces = new GameObject[pieceTypeDict[type].Length + 1];
            for (int i = 0; i < pieceTypeDict[type].Length; i++)
                pieces[i] = pieceTypeDict[type][i];

            pieceTypeDict[type] = pieces;
        }

        public void ClearPromotedPieces()
        {
            // pawns are only promoted to queen for now
            GameObject[] pieces = new GameObject[1];
            pieces[0] = pieceTypeDict[EPieceType.Queen][0];

            for (int i = 1; i < pieceTypeDict[EPieceType.Queen].Length; i++)
                Destroy(pieceTypeDict[EPieceType.Queen][i]);

            pieceTypeDict[EPieceType.Queen] = pieces;
        }

        public void Hide()
        {
            foreach(KeyValuePair<EPieceType, GameObject[]> kvp in pieceTypeDict)
            {
                foreach (GameObject gao in kvp.Value)
                    gao.SetActive(false);
            }
        }

        private void StorePieceInCategory(GameObject crtPiece, GameObject[] pieceArray)
        {
            int i = 0;
            while (i < pieceArray.Length && pieceArray[i] != null) i++;
            pieceArray[i] = crtPiece;
        }

        public void StorePiece(GameObject crtPiece, EPieceType pieceType)
        {
            StorePieceInCategory(crtPiece, pieceTypeDict[pieceType]);
        }

        private void SetPieceCategoryAt(GameObject[] pieceArray, Vector3 pos)
        {
            int i = 0;
            while (i < pieceArray.Length && pieceArray[i].activeSelf) i++;

            if(i > pieceArray.Length)
                throw new AccessViolationException("index is greater then pieceArray length", null);

            pieceArray[i].SetActive(true);
            pieceArray[i].transform.position = pos;
        }

        public void SetPieceAtPos(EPieceType pieceType, Vector3 pos)
        {
            SetPieceCategoryAt(pieceTypeDict[pieceType], pos);
        }

    }
}
