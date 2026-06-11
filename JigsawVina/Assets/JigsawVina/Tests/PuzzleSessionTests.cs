using JigsawVina.Core.Data;
using NUnit.Framework;
using UnityEngine;

namespace JigsawVina.Tests
{
    [TestFixture]
    public class PuzzleSessionTests
    {
        [Test]
        public void PuzzleSession_InitializesCorrectGridAndPositions()
        {
            var session = new PuzzleSession(6, 4);

            Assert.AreEqual(6, session.Columns);
            Assert.AreEqual(4, session.Rows);
            Assert.AreEqual(24, session.PieceCount);
            Assert.AreEqual(24, session.Pieces.Count);

            // Piece 0 at bottom-left
            var p0 = session.Pieces[0];
            Assert.AreEqual(0, p0.Index);
            Assert.AreEqual(PuzzleSession.PieceState.Tray, p0.State);
            Assert.AreEqual(0.08333f, p0.TargetNormalizedPosition.x, 0.001f);
            Assert.AreEqual(0.125f, p0.TargetNormalizedPosition.y, 0.001f);

            // Piece 23 at top-right (index 5 in column, index 3 in row)
            var p23 = session.Pieces[23];
            Assert.AreEqual(23, p23.Index);
            Assert.AreEqual(PuzzleSession.PieceState.Tray, p23.State);
            // x = (5 + 0.5f)/6 = 5.5/6 = 0.91667f
            // y = (3 + 0.5f)/4 = 3.5/4 = 0.875f
            Assert.AreEqual(0.91667f, p23.TargetNormalizedPosition.x, 0.001f);
            Assert.AreEqual(0.875f, p23.TargetNormalizedPosition.y, 0.001f);
        }

        [Test]
        public void PuzzleSession_TimerProgresses()
        {
            var session = new PuzzleSession(3, 3);
            Assert.AreEqual(0f, session.ElapsedTime);

            session.Tick(1.5f);
            Assert.AreEqual(1.5f, session.ElapsedTime);

            session.Tick(2.0f);
            Assert.AreEqual(3.5f, session.ElapsedTime);
        }

        [Test]
        public void PuzzleSession_SnapsClosePiece()
        {
            var session = new PuzzleSession(6, 4);
            var boardSize = new Vector2(800f, 600f);

            // Piece 0 local target is (-333.33f, -225f)
            var targetLocal = session.GetLocalTargetPosition(0, boardSize);
            Assert.AreEqual(-333.33f, targetLocal.x, 0.1f);
            Assert.AreEqual(-225f, targetLocal.y, 0.1f);

            // Far position: should NOT snap
            bool snappedFar = session.CheckSnap(0, new Vector2(0f, 0f), boardSize, 50f);
            Assert.IsFalse(snappedFar);
            Assert.AreEqual(PuzzleSession.PieceState.Tray, session.Pieces[0].State);

            // Close position: should snap and lock
            bool snappedClose = session.CheckSnap(0, new Vector2(-320f, -220f), boardSize, 50f);
            Assert.IsTrue(snappedClose);
            Assert.AreEqual(PuzzleSession.PieceState.Locked, session.Pieces[0].State);
        }

        [Test]
        public void PuzzleSession_HintPrioritizesLastInteracted()
        {
            var session = new PuzzleSession(3, 3, maxExclusive => maxExclusive - 1);

            // With no last interaction, use the configured random selector.
            Assert.AreEqual(8, session.GetHintPieceIndex());

            // Set last interacted to 5
            session.LastInteractedPieceIndex = 5;
            Assert.AreEqual(5, session.GetHintPieceIndex());

            // Lock piece 5, then fall back to a random unlocked piece.
            session.LockPiece(5);
            Assert.AreEqual(8, session.GetHintPieceIndex());
        }

        [Test]
        public void PuzzleSession_InvalidGrid_Throws()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new PuzzleSession(0, 4));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new PuzzleSession(4, 0));
        }

        [Test]
        public void PuzzleSession_ReturnAllFloatingToTray()
        {
            var session = new PuzzleSession(3, 3);

            session.UpdatePieceState(0, PuzzleSession.PieceState.Floating);
            session.UpdatePieceState(1, PuzzleSession.PieceState.Locked); // Cannot change from Locked
            session.UpdatePieceState(2, PuzzleSession.PieceState.Tray);

            // Lock piece 1 manually
            session.LockPiece(1);

            session.ReturnAllFloatingToTray();

            Assert.AreEqual(PuzzleSession.PieceState.Tray, session.Pieces[0].State);
            Assert.AreEqual(PuzzleSession.PieceState.Locked, session.Pieces[1].State);
            Assert.AreEqual(PuzzleSession.PieceState.Tray, session.Pieces[2].State);
        }

        [Test]
        public void PuzzleSession_CompletionCondition()
        {
            var session = new PuzzleSession(2, 2);
            Assert.IsFalse(session.IsCompleted);

            session.LockPiece(0);
            session.LockPiece(1);
            session.LockPiece(2);
            Assert.IsFalse(session.IsCompleted);

            session.LockPiece(3);
            Assert.IsTrue(session.IsCompleted);

            // Timer should stop ticking after completion
            float timeAtCompletion = session.ElapsedTime;
            session.Tick(1.0f);
            Assert.AreEqual(timeAtCompletion, session.ElapsedTime);
        }
    }
}
