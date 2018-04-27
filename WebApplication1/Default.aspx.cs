using Dinos;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Linq;

namespace WebApplication1
{
	public partial class _Default : System.Web.UI.Page
	{
		protected override void OnPreRender(EventArgs e)
		{
			btnSwap1.Visible = btnSwap2.Visible = !me.SwappedDinos;
			base.OnPreRender(e);
		}
		
		protected int moveNumber
		{
			get 
			{ 
				var x = ViewState["number"];
				return x != null ?(int)x: 0 ;
			}
			set
			{
				ViewState["number"] = value;
			}
		}

		protected Player me
		{
			get { return ViewState["me"] as Player; }
			set { ViewState["me"] = value; }
		}
		protected Player op
		{
			get { return ViewState["op"] as Player; }
			set { ViewState["op"] = value; }
		}
		protected void Page_Load(object sender, EventArgs e)
		{
			if (IsPostBack)
				return;
			Player.Init();
			me = new Player
			{
				//new Dino { Color = "yellow", Attack = 22, Health = 686, Name = "Strong" },
				//new Dino { Color = "yellow", Attack = 83, Health = 217, Name = "a" },
				//new Dino { Color = "yellow", Attack = 75, Health = 241, Name = "b" },
				Dinos = new[] { 
					new Dino { Color = "yellow", Attack = 22, Health = 66, Name = "Strong"},
					new Dino { Color = "yellow", Attack = 83, Health = 27, Name = "a"},
					new Dino { Color = "yellow", Attack = 75, Health = 21, Name = "b"},
				}.ToList(),
				Name = "me",
			};

			op = new Player
			{
				Dinos = new[] { 
					new Dino { Color = "green", Attack = 197, Health = 629, Name = "cpu1"},
					new Dino { Color = "yellow", Attack = 197, Health = 629, Name = "cpu2"},
					new Dino { Color = "yellow", Attack = 214, Health = 560, Name = "cpu3"},
				}.ToList(),
				Name = "op",
			};
		}

		protected void btnSwap2_Click(object sender, EventArgs e)
		{
			me.Swap(2);
			CheckIfNoMovesLeftAndGo();
		}

		protected void btnSwap1_Click(object sender, EventArgs e)
		{
			me.Swap(1);
			CheckIfNoMovesLeftAndGo();
		}

		protected void btnSave_Click(object sender, EventArgs e)
		{
			save.Text = (Parse(save.Text) + 1).ToString();
			CheckIfNoMovesLeftAndGo();
		}

		protected void btnDef_Click(object sender, EventArgs e)
		{
			def.Text = (Parse(def.Text) + 1).ToString();
			CheckIfNoMovesLeftAndGo();
		}

		protected void btnAtt_Click(object sender, EventArgs e)
		{
			att.Text = (Parse(att.Text) + 1).ToString();
			CheckIfNoMovesLeftAndGo();
		}

		protected int GetMovesLeft()
		{
			return me.Moves(moveNumber) - (Parse(save.Text) + Parse(att.Text) + Parse(def.Text));
		}

		protected void CheckIfNoMovesLeftAndGo()
		{
			if (GetMovesLeft() == 0)
			{
				// todo: show hit and only then go into time-consuming recursion
				go();
			}
		}

		private int Parse(string s)
		{
			try
			{
				return int.Parse(s);
			}
			catch
			{
				return 0;
			}
		}

		protected void go_Click(object sender, EventArgs e)
		{
			go();
		}

		protected void go()
		{
			var hit = Parse(att.Text);
			me.Hit(op, hit);
			// has to be before me.Save changes
			var myPossibleMoves = me.GeneratePossibleMoves(me.Moves(moveNumber) - hit).ToArray(); 

			me.MakeMove(new Move
			{
				Def = Parse(def.Text),
				Save = Parse(save.Text),
			});
			att.Text = "";
			def.Text = "";
			save.Text = "";
			try
			{
				me.SwappedDinos = false;
				moveNumber = moveNumber + 1;
				Player.InitBeforeMove(me, op, moveNumber);

				var x = op.Move(moveNumber, myPossibleMoves);

				var moves = Player.WINNING_MOVE;
				var index = new Random(DateTime.Now.Millisecond).Next(moves.Count);
				var move = moves[index];

				op.MakeMove(move);
				op.Hit(me, move.Attack);
				op.SwappedDinos = false;
				moveNumber = moveNumber + 1;
				lbAi.Text = move.ToString() + " mark: " + x;
			}
			catch (Exception ex)
			{
				lbAi.Text = ex.Message;
			}
		}
	}
}