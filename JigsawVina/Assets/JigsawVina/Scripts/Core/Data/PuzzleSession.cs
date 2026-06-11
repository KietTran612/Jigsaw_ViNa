using System;
using System.Collections.Generic;
using UnityEngine;

namespace JigsawVina.Core.Data
{
    public class PuzzleSession
    {
        public enum PieceState
        {
            Tray,
            Floating,
            Locked
        }

        public class PieceData
        {
            public int Index { get; set; }
            public PieceState State { get; set; }
            public Vector2 TargetNormalizedPosition { get; set; }
        }

        public int Columns { get; private set; }
        public int Rows { get; private set; }
        public int PieceCount => Columns * Rows;
        public List<PieceData> Pieces { get; private set; }
        public float ElapsedTime { get; private set; }
        public bool IsCompleted { get; private set; }
        public int LastInteractedPieceIndex { get; set; } = -1;
        private readonly Func<int, int> _randomIndex;

        public PuzzleSession(int columns, int rows, Func<int, int> randomIndex = null)
        {
            if (columns <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(columns));
            }
            if (rows <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(rows));
            }

            Columns = columns;
            Rows = rows;
            _randomIndex = randomIndex ?? (maxExclusive => UnityEngine.Random.Range(0, maxExclusive));
            ElapsedTime = 0f;
            IsCompleted = false;
            Pieces = new List<PieceData>();

            for (int i = 0; i < PieceCount; i++)
            {
                int c = i % Columns;
                int r = i / Columns;
                float x = (c + 0.5f) / Columns;
                float y = (r + 0.5f) / Rows;

                Pieces.Add(new PieceData
                {
                    Index = i,
                    State = PieceState.Tray,
                    TargetNormalizedPosition = new Vector2(x, y)
                });
            }
        }

        public void Tick(float deltaTime)
        {
            if (!IsCompleted)
            {
                ElapsedTime += deltaTime;
            }
        }

        public Vector2 GetLocalTargetPosition(int pieceIndex, Vector2 boardSize)
        {
            if (pieceIndex < 0 || pieceIndex >= Pieces.Count)
            {
                return Vector2.zero;
            }

            var piece = Pieces[pieceIndex];
            return new Vector2(
                (piece.TargetNormalizedPosition.x - 0.5f) * boardSize.x,
                (piece.TargetNormalizedPosition.y - 0.5f) * boardSize.y
            );
        }

        public bool CheckSnap(int pieceIndex, Vector2 localPosition, Vector2 boardSize, float threshold = 50f)
        {
            if (pieceIndex < 0 || pieceIndex >= Pieces.Count) return false;
            if (Pieces[pieceIndex].State == PieceState.Locked) return true;

            Vector2 targetLocal = GetLocalTargetPosition(pieceIndex, boardSize);
            float distance = Vector2.Distance(localPosition, targetLocal);

            if (distance <= threshold)
            {
                LockPiece(pieceIndex);
                return true;
            }

            return false;
        }

        public void LockPiece(int pieceIndex)
        {
            if (pieceIndex < 0 || pieceIndex >= Pieces.Count) return;
            Pieces[pieceIndex].State = PieceState.Locked;
            CheckCompletion();
        }

        public void UpdatePieceState(int pieceIndex, PieceState state)
        {
            if (pieceIndex < 0 || pieceIndex >= Pieces.Count) return;
            if (Pieces[pieceIndex].State == PieceState.Locked) return;
            Pieces[pieceIndex].State = state;
        }

        public int GetHintPieceIndex()
        {
            if (IsCompleted) return -1;

            if (LastInteractedPieceIndex >= 0 && LastInteractedPieceIndex < Pieces.Count)
            {
                if (Pieces[LastInteractedPieceIndex].State != PieceState.Locked)
                {
                    return LastInteractedPieceIndex;
                }
            }

            var unlockedIndices = new List<int>();
            for (int i = 0; i < Pieces.Count; i++)
            {
                if (Pieces[i].State != PieceState.Locked)
                {
                    unlockedIndices.Add(i);
                }
            }

            if (unlockedIndices.Count == 0)
            {
                return -1;
            }

            int selectedIndex = _randomIndex(unlockedIndices.Count);
            if (selectedIndex < 0 || selectedIndex >= unlockedIndices.Count)
            {
                throw new InvalidOperationException("Random index selector returned an out-of-range value.");
            }

            return unlockedIndices[selectedIndex];
        }

        public void ReturnAllFloatingToTray()
        {
            for (int i = 0; i < Pieces.Count; i++)
            {
                if (Pieces[i].State == PieceState.Floating)
                {
                    Pieces[i].State = PieceState.Tray;
                }
            }
        }

        private void CheckCompletion()
        {
            for (int i = 0; i < Pieces.Count; i++)
            {
                if (Pieces[i].State != PieceState.Locked)
                {
                    IsCompleted = false;
                    return;
                }
            }
            IsCompleted = true;
        }
    }
}
