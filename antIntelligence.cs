using System;
using System.Collections.Generic;

using AntMe.English;

// Add your own name behind AntMe.Player
namespace AntMe.Player.AlanTuring
{
    // List of markers and their meaning
    // 1 = sugar
    // 2 = fruit
    // 3 = bug
    // .....


    // It is necessary to give a name to your colony. Please also add your first and last name
    [Player(ColonyName = "mySuperCoolAnts",
        FirstName = "Claudio Spiess",
        LastName = "Riccardo Felluga"
	)]

	//  Here different casts can be defined.
	[Caste(Name = "Scout",
           AttackModificator = -1,
           EnergyModificator = 0,
           LoadModificator = -1,
           RangeModificator = 0,
           RotationSpeedModificator = 0,
           SpeedModificator = 0,
           ViewRangeModificator = 2
	)]

    [Caste(Name = "Fighter",
           AttackModificator = 2,
           EnergyModificator = 2,
           LoadModificator = -1,
           RangeModificator = -1,
           RotationSpeedModificator = -1,
           SpeedModificator = 0,
           ViewRangeModificator = -1
    )]

    // No static fields!
    // like
    // static int currentBugsSeen;
    // ...

    //but logically, non static fields are allowed!
    
    public partial class MyAnt : BaseAnt
    {
        Boolean hasTarget = false;
        Sugar myLastSugar;

        #region Caste

        /// <summary>
        /// Determines the caste of a new ant
        /// </summary>
        /// <param name="count">The number of existing ants per caste</param>
        /// <returns>The name of the cast of the new ant spawned next.</returns>
        public override string ChooseType(Dictionary<string, int> count)
        {
            if (count["Scout"] < 10)
               return "Scout";
            else return "Fighter";
            //return "Scout";
        }

        #endregion

        #region Movement

        /// <summary>
		/// Is called whenever the ant has nothing to do
		/// </summary>
		public override void Waits()
		{
            // If waits is called we have no target
            if (this.Caste=="Fighter")
            { 
                hasTarget = false;
                GoBackToAnthill();
                this.Drop();
            }

            if (this.Caste=="Scout")
            {
                this.GoAhead(100);
                int randomTurn = new Random().Next(-10, 10);
                this.TurnByDegrees(randomTurn);
            }
        }

        /// <summary>
        /// Is called once when the ant has exceeded one third of its maximal movement range
        /// </summary>
        public override void BecomesTired()
		{
		}

		#endregion

		#region Food

		/// <summary>
		/// Is called continuously whenever the ant sees at least one pile of sugar
		/// </summary>
		/// <param name="sugar">The nearest pile of sugar.</param>
		public override void Spots(Sugar sugar)
		{
            if (CurrentLoad == 0) GoToTarget(sugar);
        }

        /// <summary>
        /// Is called continuously whenever the ant sees at least one fruit
        /// </summary>
        /// <param name="fruit">Das nächstgelegene Obststück.</param>
        public override void Spots(Fruit fruit)
		{
            if (CurrentLoad == 0) GoToTarget(fruit);
        }

        

		/// <summary>
		/// Is called once if the ant has a pile of sugar as target and arrives at the pile.
		/// </summary>
		/// <param name="sugar">The pile of sugar</param>
		public override void TargetReached(Sugar sugar)
		{
            MakeMark(123, 9999);
            Take(sugar);
            GoBackToAnthill();
        }

        /// <summary>
        /// Is called once if the ant has a fruit as target and arrives at the fruit.
        /// </summary>
        /// <param name="fruit">The fruit.</param>
        public override void TargetReached(Fruit fruit)
		{
            Take(fruit);
            GoBackToAnthill();
        }

		#endregion

		#region Communication

		/// <summary>
		/// Is called once if the ant smells a marker from the own colony. Markers smelled once are not smelled again.
		/// </summary>
		/// <param name="marker">Die nächste neue Markierung.</param>
		public override void SmellsFriend(Marker marker)
		{
            // When someone calls "sugar"
            if (marker.Information == 123 && CurrentLoad == 0)
                GoToTarget(marker);

            // When someone calls "bug" soldiers go there
            if (marker.Information == 12345 && !hasTarget && this.Caste=="Fighter")
            {
                this.GoToTarget(marker);
                hasTarget = true;
            }
                
		}

		/// <summary>
		/// Is called repeatedly whenever the ant sees at least one ant from its colony.
		/// </summary>
		/// <param name="ant">The nearest ant from the same colony</param>
		public override void SpotsFriend(Ant ant)
		{
		}


		#endregion

		#region Fighting

		/// <summary>
		/// Is called repeatedly whenever the ant sees at least one bug.
		/// </summary>
		/// <param name="bug">The nearest bug.</param>
		public override void SpotsEnemy(Bug bug)
		{
            if (this.Caste.Equals("Scout"))
                this.MakeMark(12345, 9999);
            else
                this.Attack(bug);   
		}

		/// <summary>
		/// Is called repeatedly if the ant is attacked by a bug.
		/// </summary>
		/// <param name="bug">The attacking bug.</param>
		public override void UnderAttack(Bug bug)
		{
		}

		#endregion

		#region Other

		/// <summary>
		/// Is called once if the ant dies.
		/// </summary>
		/// <param name="kindofdeath">Why the ant died</param>
		public override void HasDied(KindOfDeath kindofdeath)
		{
		}

		/// <summary>
		/// Is called in every tick.
		/// </summary>
		public override void Tick()
		{
		}

		#endregion
		 
	}
}