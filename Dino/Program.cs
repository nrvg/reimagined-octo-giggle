using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Dinos
{
	[Serializable]
	public class Dino
	{
		public string Color;
		public string Name;
		public int Health;
		public int Attack;

		static string[] WEAKNESS = new[] { "red", "green", "yellow", "blue", "red" };

		// green.isWeakerThan(red) == true
		public bool IsWeakerThan(Dino y)
		{
			return Array.IndexOf(WEAKNESS, this.Color, 1) == Array.IndexOf(WEAKNESS, y.Color) + 1;
		}
		public Dino Clone()
		{
			return new Dino
			{
				Color = this.Color,
				Name = this.Name,
				Health = this.Health,
				Attack = this.Attack,
			};
		}
		public override string ToString()
		{
			return string.Format("{0} att {1} health {2} {3}", this.Name, Attack, Health, this.Color);
		}
	}

	public class Move
	{
		public int Save;
		public int Attack;
		public int Def;
		public int? SwapIndex;
		public override string ToString()
		{
			return string.Format("save {0} attack {1} def {2} {3}", Save, Attack, Def, SwapIndex.HasValue ? "swap with " + SwapIndex.Value : "");
		}

		public bool Equals(Move m)
		{
			return this.Save == m.Save && this.Attack == m.Attack && this.Def == m.Def && this.SwapIndex == m.SwapIndex;
		}
	}

	public class State
	{
		public Player Me;
		public Player Other;

		internal bool IsWorseThan(State o)
		{
			// todo: compare swapped dinos?
			// todo: mayb take movenumber into consideration
			return
				Me.Save <= o.Me.Save &&
				Me.Def <= o.Me.Def &&
				Other.Save >= o.Other.Save &&
				Other.Def >= o.Other.Def &&
				Me.DinosAreWorse(o.Me) &&
				!Other.DinosAreWorse(o.Other);
		}
	}

	[Serializable]
	public class Player
	{
		public Player Clone()
		{
			return new Player
			{
				Def = this.Def,
				Save = this.Save,
				Name = this.Name,
				SwappedDinos = this.SwappedDinos,
				Dinos = this.Dinos.Select(x => x.Clone()).ToList(),
			};
		}

		int DinoSum()
		{
			return Dinos.Aggregate(0, (s, n) => s + n.Health);
		}

		public bool DinosAreWorse(Player o)
		{
			//return this.DinoSum() <= o.DinoSum();
			return this.Dinos.Count == o.Dinos.Count && Enumerable.Range(0, this.Dinos.Count).All(i => 
				this.Dinos[i].Name == o.Dinos[i].Name && this.Dinos[i].Health <= o.Dinos[i].Health);
		}

		public int Moves(int moveNumber)
		{
			return this.Save + Player.nextSeq(moveNumber) - (this.SwappedDinos ? 1 : 0);
		}

		public void Swap(int index)
		{
			this.SwappedDinos = true;
			var first = this.Dinos[0];
			var second = this.Dinos[index];
			this.Dinos[index] = first;
			this.Dinos[0] = second;
		}

		public List<Dino> Dinos;
		public int Save;
		public string Name;
		public bool SwappedDinos;
		public int Def;

		public bool IsDead
		{
			get { return !Dinos.Any(); }
		}

		private static double[] STRENGTHS = { 0, 1, 1.2, 1.4, 1.6, 2, 2.2, 2.4, 2.6 };
		public bool Hit(Player a, int withAttack)
		{
			var hitter = this.Dinos.First();
			var receiver = a.Dinos.First();

			var weaknessCoef = hitter.IsWeakerThan(receiver) ? 0.5 : (receiver.IsWeakerThan(hitter)? 1.5 : 1.0);

			var hit = Math.Max(withAttack - a.Def, 0);
			var points = (int)Math.Ceiling(hit * STRENGTHS[hit] * weaknessCoef * hitter.Attack);
			receiver.Health -= points;
			if (receiver.Health <= 0)
			{
				a.Dinos.RemoveAt(0);
				return true;
			}
			return false;
		}

		private bool CanKillWithLess(Player op, int p)
		{
			var clone = op.Clone();
			return this.Hit(clone, Math.Max(p - 1, 0));
		}

		public static Dictionary<int, List<Move>> possibleMoveCombination;

		private static int[] SEQUENCE = {1, 2, 2, 2, 3, 3, 4, 4, 4, 4};

		public static List<Move> WINNING_MOVE;
		public static bool IS_WINNING_SWAP;
		public static int WINNING_SWAP_INDEX;

		const int MAX_DEPTH = 5; // must be odd
		public static List<State>[] BEST;

		public static int size = 0;
		public static int pruned = 0;

		public static bool IsWorse(Player me, Player other, int movenumber)
		{
			var thisState = new State{ Me = me.Clone(), Other = other.Clone() };
			var best = BEST[movenumber] = BEST[movenumber] ?? new List<State>();
			if (!best.Any(x => thisState.IsWorseThan(x)))
			{
				best.Add(thisState);
				size++;
				if (size % 1000 == 0)
					Console.WriteLine(size);
				return false;
			}
			pruned++;
			if (pruned % 100 == 0)
				Console.WriteLine("pruned: " + pruned);

			return true;
		}


		//if (possibleOpponent.Length == 1 && IsWorse(this, possibleOpponent.First(), moveNumber))
		//	return -1;

		//if (moveNumber == 3)
		//{
		//	var str = this.Dinos.Aggregate("", (s, n) => s + n.Health + " ");
		//	Console.WriteLine(string.Format("{0}: {1} - {2}", this.Name, this.Moves, str));
		//}

		private const double E = 0.00001;

		public double Move(int moveNumber, Player[] possibleOpponent)
		{
			var opponentMark = 1.0; // this opponent's position mark
			
			if (this.IsDead)
				goto singleReturnPoint;
			
			if (moveNumber - Player.MoveNumber > MAX_DEPTH)
			{
				// if MAX_DEPTH is odd, this.Name is op
				var op = new[] { Player.P1, Player.P2 }.First(x => x.Name != this.Name);
				var percent = (double)possibleOpponent.First().DinoSum() / op.DinoSum();
				if (percent > 1 - E) // if we cannot kill
					opponentMark = -1 * (double)this.Save / 16;
				else					
					opponentMark = -1 * (1 - percent);
				goto singleReturnPoint;
			}

			if (this.SwappedDinos)
				throw new InvalidOperationException("Swapped Dinos?");
			var moves = this.Moves(moveNumber);
			foreach (var move in Player.possibleMoveCombination[moves]
				.Where(x => x.SwapIndex.GetValueOrDefault() <= this.Dinos.Count - 1))
			{
				//lets see every possible move against every possible opponent state
				var clone = this.Clone();

				if (move.SwapIndex.HasValue && possibleOpponent.All(x => x.Dinos.First().IsWeakerThan(clone.Dinos.First())))
					continue;

				clone.MakeMove(move);
				clone.SwappedDinos = false;

				// if hit is less than defence, do not care to hit at all
				if (move.Attack > 0 && possibleOpponent.All(x => x.Def >= move.Attack)) 
					continue;

				if (possibleOpponent.All(x => clone.CanKillWithLess(x, move.Attack)))
					continue;
				var max = possibleOpponent.Select(op =>
				{
					var opClone = op.Clone();
					clone.Hit(opClone, move.Attack);
					var res = opClone.Move(moveNumber + 1, new[] { clone }); // [-1, 1]
					return res;
				}).ToArray().Max(); // the more the worse for us

				if (max < opponentMark + E)
				{
					if (moveNumber == Player.MoveNumber)
					{
						if (Math.Abs(max - opponentMark) < E) // if max == m
							WINNING_MOVE.Add(move);
						else
							WINNING_MOVE = new[] { move }.ToList();
					}
					opponentMark = max; //minimax

					// if win occured not in the leaf of recursion, but sooner
					// MAX(moveNumber - Player.MoveNumber) == MAX_DEPTH + 1
					if (opponentMark < -1 * Coeff(MAX_DEPTH)) // good enough
						break;
				}
			}

		singleReturnPoint:
			// the deeper down the recursion, the bleaker and more blurred the marks
			return -1 * opponentMark * Coeff(moveNumber - Player.MoveNumber);
		}

		/// <summary>
		/// returns coefficient within (0, 1]. For larger movenumbers returns smaller coefficient
		/// </summary>
		/// <param name="moveNumber"></param>
		/// <returns></returns>
		private double Coeff(int moveNumber)
		{
			return Math.Pow(1 - E, moveNumber);
		}

		public void MakeMove(Move move)
		{
			if (move.SwapIndex.HasValue)
				this.Swap(move.SwapIndex.Value);
			this.Def = move.Def;
			this.Save = move.Save;
		}

		public IEnumerable<Player> GeneratePossibleMoves(int moves)
		{
			return Player.possibleMoveCombination[moves].Where(x => x.Attack == 0 && !x.SwapIndex.HasValue).Select(move =>
			{
				var clone = this.Clone();
				clone.SwappedDinos = false;
				clone.MakeMove(move);
				return clone;
			});
		}

		public static int nextSeq(int mynextMove)
		{
			return (mynextMove <= 5 ? SEQUENCE[mynextMove] : 4);
		}
		
		public static void InitBeforeMove(Player p1, Player p2, int moveNumber)
		{
			Player.P1 = p1.Clone();
			Player.P2 = p2.Clone();
			Player.MoveNumber = moveNumber;
			Player.WINNING_MOVE = new List<Move>();
			Player.BEST = new List<State>[MAX_DEPTH];
		}

		public static void Init()
		{
			Player.possibleMoveCombination = new Dictionary<int, List<Move>>();
			Enumerable.Range(0, 9).ToList().ForEach(sum =>
			{
				Player.possibleMoveCombination.Add(sum, new List<Move>());
			});

			Enumerable.Range(0, 5).ToList().ForEach(save =>
			{
				Enumerable.Range(0, 9).Reverse().ToList().ForEach(att =>
				{
					Enumerable.Range(0, 9).ToList().ForEach(def =>
					{
						var sum = save + att + def;
						if (sum <= 8)
							Player.possibleMoveCombination[sum].Add(new Move 
							{ 
								Save = save, 
								Attack = att,
								Def = def,
								SwapIndex = null,
							});
						if (sum <= 7)
						{
							Player.possibleMoveCombination[sum + 1].Add(new Move
							{
								Save = save,
								Attack = att,
								Def = def,
								SwapIndex = 1,
							});
							Player.possibleMoveCombination[sum + 1].Add(new Move
							{
								Save = save,
								Attack = att,
								Def = def,
								SwapIndex = 2,
							});
						}
					});
				});
			});
		}

		public static Player P1;
		public static Player P2;

		public static int MoveNumber { get; set; }
	}
}