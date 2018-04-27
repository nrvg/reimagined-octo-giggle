using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dinos;
using System.Linq;

namespace TestDino
{
	[TestClass]
	public class BasicTest
	{
		public BasicTest()
		{
			Player.Init();
		}

		[TestMethod]
		public void FirstHitKill()
		{
			var me = new Player
			{
				Dinos = new[] { 
					new Dino { Color = "yellow", Attack = 22, Health = 66, Name = "Strong"},
				}.ToList(),
				Name = "me",
			};

			var op = new Player
			{
				Dinos = new[] { 
					new Dino { Color = "green", Attack = 197, Health = 629, Name = "cpu1"},
					new Dino { Color = "yellow", Attack = 197, Health = 629, Name = "cpu2"},
					new Dino { Color = "yellow", Attack = 214, Health = 560, Name = "cpu3"},
				}.ToList(),
				Name = "op",
			};

			var myPossibleMoves = me.GeneratePossibleMoves(1).ToArray(); // save or defence 1

			Player.InitBeforeMove(me, op, 1);
			var x = op.Move(1, myPossibleMoves);
			Assert.IsTrue(Player.WINNING_MOVE.Count == 1);
			Assert.IsTrue(Player.WINNING_MOVE.First().Equals(new Move { Attack = 2 }));
		}
	
		[TestMethod]
		public void FirstHitKillAndSwap()
		{
			var me = new Player
			{
				Dinos = new[] { 
					new Dino { Color = "yellow", Attack = 22, Health = 66, Name = "Strong"},
				}.ToList(),
				Name = "me",
			};

			var op = new Player
			{
				Dinos = new[] { 
					new Dino { Color = "yellow", Attack = 19, Health = 629, Name = "cpu2"},
					new Dino { Color = "green", Attack = 44, Health = 629, Name = "cpu1"},																
					new Dino { Color = "yellow", Attack = 21, Health = 560, Name = "cpu3"},
				}.ToList(),
				Name = "op",
			};

			var myPossibleMoves = me.GeneratePossibleMoves(0).ToArray(); // think that this attacked or swapped, so no moves left

			Player.InitBeforeMove(me, op, 1);
			var x = op.Move(1, myPossibleMoves);
			Assert.IsTrue(Player.WINNING_MOVE.Count == 1);
			Assert.IsTrue(Player.WINNING_MOVE.First().Equals(new Move { Attack = 1, SwapIndex = 1 }));
		}

		[TestMethod]
		public void EndSpielNoSureWin()
		{
			var me = new Player
			{
				Dinos = new[] { 
					new Dino { Color = "yellow", Attack = 22, Health = 22, Name = "Strong"},
				}.ToList(),
				Name = "me",
			};

			var op = new Player
			{
				Dinos = new[] { 
					new Dino { Color = "yellow", Attack = 22, Health = 629, Name = "cpu2"},
				}.ToList(),
				Name = "op",
			};

			var myPossibleMoves = me.GeneratePossibleMoves(4).ToArray();

			Player.InitBeforeMove(me, op, 10);
			var x = op.Move(10, myPossibleMoves);
			Assert.IsTrue(x >= 0);
		}

		[TestMethod]
		public void EndSpielKill()
		{
			var me = new Player
			{
				Dinos = new[] { 
					new Dino { Color = "yellow", Attack = 22, Health = 22, Name = "Strong"},
				}.ToList(),
				Name = "me",
			};

			var op = new Player
			{
				Dinos = new[] { 
					new Dino { Color = "yellow", Attack = 22, Health = 629, Name = "cpu2"},
					new Dino { Color = "yellow", Attack = 21, Health = 560, Name = "cpu3"},
				}.ToList(),
				Name = "op",
			};

			var myPossibleMoves = me.GeneratePossibleMoves(3).ToArray();

			Player.InitBeforeMove(me, op, 10);
			var x = op.Move(10, myPossibleMoves);
			Assert.IsTrue(Player.WINNING_MOVE.Count == 1);
			Assert.IsTrue(Player.WINNING_MOVE.First().Equals(new Move { Attack = 4 }));
		}

		[TestMethod]
		public void ReasonableSwap()
		{
			var me = new Player
			{
				Dinos = new[] { 
					new Dino { Color = "yellow", Attack = 22, Health = 22, Name = "a"},
					new Dino { Color = "red", Attack = 22, Health = 22, Name = "b"},
					new Dino { Color = "red", Attack = 22, Health = 22, Name = "c"},
				}.ToList(),
				Name = "me",
			};

			var op = new Player
			{
				Dinos = new[] { 
					new Dino { Color = "blue", Attack = 22, Health = 22, Name = "1"},
					new Dino { Color = "blue", Attack = 22, Health = 22, Name = "2"},
					new Dino { Color = "green", Attack = 22, Health = 22, Name = "3"},
				}.ToList(),
				Name = "op",
			};

			var myPossibleMoves = new[] { me.Clone() };

			Player.InitBeforeMove(me, op, 0); // CPU makes first move
			var x = op.Move(0, myPossibleMoves);
			Assert.IsTrue(Player.WINNING_MOVE.Count == 1);
			Assert.IsTrue(Player.WINNING_MOVE.First().Equals(new Move { SwapIndex = 2 }));
		}

		[TestMethod]
		public void ReasonableSaveAndDodge()
		{
			var me = new Player
			{
				Dinos = new[] { 
					new Dino { Color = "yellow", Attack = 4, Health = 22, Name = "a"},
				}.ToList(),
				Name = "me",
				Def = 4
			};

			var op = new Player
			{
				Dinos = new[] { 
					new Dino { Color = "yellow", Attack = 22, Health = 22, Name = "1"},
				}.ToList(),
				Name = "op",
			};

			var myPossibleMoves = new[] { me.Clone() };

			Player.InitBeforeMove(me, op, 10); 
			var x = op.Move(10, myPossibleMoves);
			// op needs at least 1 defence to avoid possible death from 4 attack next turn
			// op needs at least 1 save to surely kill next turn
			Assert.IsTrue(Player.WINNING_MOVE.All(move => move.Def > 0 && move.Save > 0));

			// if is surely winning, recursion can postpone winning move endlessly
			// so if depth is 3, this test passes,
			// if depth is 5, this test fails. Because recursion makes this winning move later.
			// added special coefficient that incentivies recursion to act sooner
		}

		[TestMethod]
		public void ReasonableSave()
		{
			var me = new Player
			{
				Dinos = new[] { 
					new Dino { Color = "red", Attack = 4, Health = 22, Name = "a"},
				}.ToList(),
				Name = "me",
				Def = 4
			};

			var op = new Player
			{
				Dinos = new[] { 
					new Dino { Color = "blue", Attack = 11, Health = 22, Name = "1"},
					new Dino { Color = "red", Attack = 11, Health = 22, Name = "1"},
				}.ToList(),
				Name = "op",
			};

			var myPossibleMoves = me.GeneratePossibleMoves(6).ToArray(); // we know that me had 6 and did not hit

			Player.InitBeforeMove(me, op, 10);
			var x = op.Move(10, myPossibleMoves);
			Assert.IsTrue(Player.WINNING_MOVE.All(move => move.Save >= 3));
		}
	}
}
