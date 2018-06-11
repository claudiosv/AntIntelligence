using System;
using System.Collections.Generic;

using AntMe.English;

// Add your own name behind AntMe.Player
namespace AntMe.Player.SuperCoolAnts
{

    // It is necessary to give a name to your colony. Please also add your first and last name
    [Player(ColonyName = "SuperCoolAnts",
        FirstName = "Claudio Spiess",
        LastName = "Riccardo Felluga"
    )]

    //  Here different casts can be defined.
    [Caste(Name = "Sugar",
           AttackModificator = -1,
           EnergyModificator = 0,
           LoadModificator = 2,
           RangeModificator = 0,
           RotationSpeedModificator = -1,
           SpeedModificator = 0,
           ViewRangeModificator = 0
    )]
    [Caste(Name = "Fruit",
           AttackModificator = -1,
           EnergyModificator = 0,
           LoadModificator = 1,
           RangeModificator = 0,
           RotationSpeedModificator = -1,
           SpeedModificator = 1,
           ViewRangeModificator = 0
    )]
    [Caste(Name = "Ninja",
           AttackModificator = 2,
           EnergyModificator = 1,
           LoadModificator = -1,
           RangeModificator = -1,
           RotationSpeedModificator = -1,
           SpeedModificator = 1,
           ViewRangeModificator = -1
    )]
    [Caste(Name = "Anthill",
           AttackModificator = 0,
           EnergyModificator = 0,
           LoadModificator = 0,
           RangeModificator = 0,
           RotationSpeedModificator = 0,
           SpeedModificator = 0,
           ViewRangeModificator = 0
    )]


    public class MyAnt : BaseAnt
    {

        public int sugarMarkerSize = 99999;
        public int trackLength = 150;
        public int trackAngle = 90;
        public bool lastTurn = false;
        public bool spottedPath = false;
        public int spottedWorkersOnWayBack = 0;
        public Item prevTarget = null;
        public Marker anthillMarker = null;


        #region Caste

        /// <summary>
        /// Determines the caste of a new ant
        /// </summary>
        /// <param name="count">The number of existing ants per caste</param>
        /// <returns>The name of the cast of the new ant spawned next.</returns>
        public override string ChooseType(Dictionary<string, int> count)
        {

             if (count["Anthill"] == 0)
            {
                return "Anthill";
            }

            if (count["Sugar"] < 50)
            {
                return "Sugar";
            }

            else if (count["Fruit"] < 30)
            {
                return "Fruit";
            }
            else {
                return "Ninja";
            }
        }

        #endregion

        #region Movement

        /// <summary>
		/// Is called whenever the ant has nothing to do
		/// </summary>
		public override void Waits()
        {
            if (!this.Caste.Equals("Anthill"))
            {
                GoAhead(trackLength);
                if (lastTurn)
                {
                    TurnByDegrees(trackAngle);
                    lastTurn = false;
                }
                else
                {
                    TurnByDegrees(-trackAngle);
                    lastTurn = true;
                }
            }
            else {
                MakeMark(4, 500);
            }
        }

        /// <summary>
        /// Is called once when the ant has exceeded one third of its maximal movement range
        /// </summary>
        public override void BecomesTired()
        {
            GoBackToAnthill();
            prevTarget = this.Target;
        }

        #endregion

        #region Food

        /// <summary>
        /// Is called continuously whenever the ant sees at least one pile of sugar
        /// </summary>
        /// <param name="sugar">The nearest pile of sugar.</param>
        public override void Spots(Sugar sugar)
        {
            if (this.Caste.Equals("Sugar")) //only sugar ants goes to sugar
            {
                if (CurrentLoad == 0)// if it's not carrying anything than take!
                {
                    GoToTarget(sugar);
                    prevTarget = sugar;
                }
            }
        }

        /// <summary>
        /// Is called continuously whenever the ant sees at least one fruit
        /// </summary>
        /// <param name="fruit">Das nächstgelegene Obststück.</param>
        public override void Spots(Fruit fruit)
        {
            if (this.Caste.Equals("Fruit"))
            {
                if (CarringFruit == null) {
                    GoToTarget(fruit);
                    prevTarget = fruit;
                }
            }
        }



        /// <summary>
        /// Is called once if the ant has a pile of sugar as target and arrives at the pile.
        /// </summary>
        /// <param name="sugar">The pile of sugar</param>
        public override void TargetReached(Sugar sugar)
        {
            Take(sugar);
            MakeMark(0, sugarMarkerSize);
            GoAwayFromTarget(sugar);
            GoBackToAnthill();
           
            prevTarget = this.Target;

            //GoAhead(1000);
        }

        /// <summary>
        /// Is called once if the ant has a fruit as target and arrives at the fruit.
        /// </summary>
        /// <param name="fruit">The fruit.</param>
        public override void TargetReached(Fruit fruit)
        {
            Take(fruit);
            if (NeedsCarrier(fruit))
            {
                MakeMark(2, 200); //2 == fruit help mark
            }
            GoBackToAnthill();
            prevTarget = this.Target;
        }

        #endregion

        #region Communication

        /// <summary>
        /// Is called once if the ant smells a marker from the own colony. !!!!!!!!!! Markers smelled once are not smelled again. !!!!!!!!!!
        /// </summary>
        /// <param name="marker">Die nächste neue Markierung.</param>
        public override void SmellsFriend(Marker marker)
        {
            if (marker.Information == 4 && this.Caste.Equals("Sugar"))
            {
                anthillMarker = marker;
            }

            if (CurrentLoad == 0) {
                if (this.Caste.Equals("Sugar") && marker.Id == 0)
                {
                    TurnToTarget(marker);
                    GoToTarget(marker);
                    prevTarget = marker;
                }
                else if (this.Caste.Equals("Sugar") && marker.Id == 1)
                {
                    TurnToTarget(marker);
                    GoAhead(50);
                }
                else if (this.Caste.Equals("Fruit") && marker.Id == 2)
                {
                    TurnToTarget(marker);
                    GoToTarget(marker);
                    prevTarget = marker;
                }
                else if (this.Caste.Equals("Ninja") && marker.Id == 3)
                {
                    TurnToTarget(marker);
                    GoToTarget(marker);
                }

                

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
            if (CarringFruit != null || CurrentLoad != 0)
            {
                MakeMark(3, 200); // bug spotted == 3
            }
            if (this.Caste.Equals("Ninja")) {
                Attack(bug);
            }
		}

		/// <summary>
		/// Is called repeatedly if the ant is attacked by a bug.
		/// </summary>
		/// <param name="bug">The attacking bug.</param>
		public override void UnderAttack(Bug bug)
		{
            if (this.Caste.Equals("Ninja"))
            {
                Attack(bug);
            }
            else
            {
               // GoAwayFromTarget(bug);
               // if (prevTarget != null)
               // {
                  //  GoToTarget(prevTarget);
                //}
            }
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
            if (CarringFruit == null && CurrentLoad != 0 ) {
                MakeMark(1, 15); // 1 == sugar track trace
            } else if (CarringFruit != null && NeedsCarrier(CarringFruit))
            {
                MakeMark(2, 50); // fruit trace
            } else if(this.Caste.Equals("Anthill"))
            {
         //       MakeMark(4, int.MaxValue);
            }
		}

		#endregion
		 
	}
}