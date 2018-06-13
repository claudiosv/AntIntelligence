using System;
using System.Collections.Generic;

using AntMe.English;

namespace AntMe.Player.SuperCoolAntsV5
{

    // It is necessary to give a name to your colony. Please also add your first and last name
    [Player(ColonyName = "SuperCoolAntsV5",
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


    public class MyAnt : BaseAnt
    {

        public int sugarMarkerSize = 99999;
        public int trackLength = 150;
        public int trackAngle = 50;
        public bool lastTurn = false;
        public bool spottedPath = false;
        public int spottedWorkersOnWayBack = 0;
        public Item prevTarget = null;
        Boolean hasTarget = false;
        public enum Markers
        {
            Sugar = 0,
            SugarTrace = 1,
            Fruit = 2,
            Bug = 3
        }


        #region Caste

        /// <summary>
        /// Determines the caste of a new ant
        /// </summary>
        /// <param name="count">The number of existing ants per caste</param>
        /// <returns>The name of the cast of the new ant spawned next.</returns>
        public override string ChooseType(Dictionary<string, int> count)
        {
            if (count["Sugar"] < 25)
            {
                return "Sugar";
            }

            else if (count["Fruit"] < 20) //15
            {
                return "Fruit";
            }
            else
            {
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
            MakeMark((int)Markers.Sugar, 200);
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
            MakeMark((int)Markers.Fruit, 50);
            if (this.Caste.Equals("Fruit"))
            {
                if (CarringFruit == null)
                {
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
            MakeMark((int)Markers.Sugar, 750);
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
                MakeMark((int)Markers.Fruit, 200); //2 == fruit help mark
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
            if (CurrentLoad == 0)
            {
                if (this.Caste.Equals("Sugar") && marker.Id == (int)Markers.Sugar)
                {
                    hasTarget = true;
                    //TurnToTarget(marker);
                    GoToTarget(marker);
                    prevTarget = marker;
                }
                else if (this.Caste.Equals("Sugar") && marker.Id == (int)Markers.SugarTrace)
                {
                    //TurnToTarget(marker);
                    GoAhead(50);
                }
                else if (this.Caste.Equals("Fruit") && marker.Id == (int)Markers.Fruit)
                {
                    //TurnToTarget(marker);
                    GoToTarget(marker);
                    prevTarget = marker;
                }
                else if (this.Caste.Equals("Ninja") && marker.Id == (int)Markers.Bug && !hasTarget)
                {
                    hasTarget = true;
                    //TurnToTarget(marker);
                    GoToTarget(marker);
                    prevTarget = marker;

                }
                else if (this.Caste.Equals("Sugar") && marker.Id == (int)Markers.Fruit && !hasTarget)
                {
                    //hasTarget = true;
                    //TurnToTarget(marker);
                    GoToTarget(marker);
                    prevTarget = marker;

                }

            }
        }

        /// <summary>
        /// Is called repeatedly whenever the ant sees at least one ant from its colony.
        /// </summary>
        /// <param name="ant">The nearest ant from the same colony</param>
        public override void SpotsFriend(Ant ant)
        {
            if (this.Caste.Equals("Ninja"))
            {
                //TurnToTarget(ant);
                // GoToTarget(ant);
            }
        }


        #endregion

        #region Fighting

        /// <summary>
        /// Is called repeatedly whenever the ant sees at least one bug.
        /// </summary>
        /// <param name="bug">The nearest bug.</param>
        public override void SpotsEnemy(Bug bug)
        {
            //if (CarringFruit != null || CurrentLoad != 0)
            //{
            // bug spotted == 3
            //}
            if (this.Caste.Equals("Ninja"))
            {
                Attack(bug);
                MakeMark((int)Markers.Bug, 100);
            }
            else
            {
                MakeMark((int)Markers.Bug, 300);
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
            if (kindofdeath == KindOfDeath.Eaten)
                MakeMark((int)Markers.Bug, 200);
        }

        /// <summary>
        /// Is called in every tick.
        /// </summary>
        public override void Tick()
        {
            if (CarringFruit == null && CurrentLoad != 0)
            {
                MakeMark((int)Markers.SugarTrace, 15); // 1 == sugar track trace
            }
            else if (CarringFruit != null && NeedsCarrier(CarringFruit))
            {
                MakeMark((int)Markers.Fruit, 50); // fruit trace 50
            }
            if (this.Caste.Equals("Ninja"))
            {
                hasTarget = false;
            }
        }

        #endregion

    }
}

namespace AntMe.Player.SuperCoolAntsV4
{

    // It is necessary to give a name to your colony. Please also add your first and last name
    [Player(ColonyName = "SuperCoolAntsV4",
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


    public class MyAnt : BaseAnt
    {

        public int sugarMarkerSize = 99999;
        public int trackLength = 150;
        public int trackAngle = 90;
        public bool lastTurn = false;
        public bool spottedPath = false;
        public int spottedWorkersOnWayBack = 0;
        public Item prevTarget = null;
        Boolean hasTarget = false;
        public enum Markers
        {
            Sugar = 0,
            SugarTrace = 1,
            Fruit = 2,
            Bug = 3
        }


        #region Caste

        /// <summary>
        /// Determines the caste of a new ant
        /// </summary>
        /// <param name="count">The number of existing ants per caste</param>
        /// <returns>The name of the cast of the new ant spawned next.</returns>
        public override string ChooseType(Dictionary<string, int> count)
        {
            if (count["Sugar"] < 25)
            {
                return "Sugar";
            }

            else if (count["Fruit"] < 15) //15
            {
                return "Fruit";
            }
            else
            {
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
                if (CarringFruit == null)
                {
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
            MakeMark((int)Markers.Sugar, 200);

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
                MakeMark((int)Markers.Fruit, 200); //2 == fruit help mark
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
            if (CurrentLoad == 0 && CarringFruit==null)
            {
                if (this.Caste.Equals("Sugar") && marker.Id == (int)Markers.Sugar)
                {
                    //TurnToTarget(marker);
                    GoToTarget(marker);
                    prevTarget = marker;
                }
                else if (this.Caste.Equals("Sugar") && marker.Id == (int)Markers.SugarTrace)
                {
                    //TurnToTarget(marker);
                    GoAhead(50);
                }
                else if (this.Caste.Equals("Fruit") && marker.Id == (int)Markers.Fruit)
                {
                    //TurnToTarget(marker);
                    GoToTarget(marker);
                    prevTarget = marker;
                }
                else if (this.Caste.Equals("Ninja") && marker.Id == (int)Markers.Bug && !hasTarget)
                {
                    hasTarget = true;
                    //TurnToTarget(marker);
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
            if (this.Caste.Equals("Ninja"))
            {
                //TurnToTarget(ant);
                // GoToTarget(ant);
            }
        }


        #endregion

        #region Fighting

        /// <summary>
        /// Is called repeatedly whenever the ant sees at least one bug.
        /// </summary>
        /// <param name="bug">The nearest bug.</param>
        public override void SpotsEnemy(Bug bug)
        {
            //if (CarringFruit != null || CurrentLoad != 0)
            //{
            // bug spotted == 3
            //}
            if (this.Caste.Equals("Ninja"))
            {
                Attack(bug);
                MakeMark((int)Markers.Bug, 100);
            }
            else
            {
                MakeMark((int)Markers.Bug, 300);
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
            if (kindofdeath == KindOfDeath.Eaten)
                MakeMark((int)Markers.Bug, 200);
        }

        /// <summary>
        /// Is called in every tick.
        /// </summary>
        public override void Tick()
        {
            if (CarringFruit == null && CurrentLoad != 0)
            {
                MakeMark((int)Markers.SugarTrace, 15); // 1 == sugar track trace
            }
            else if (CarringFruit != null && NeedsCarrier(CarringFruit))
            {
                MakeMark((int)Markers.Fruit, 50); // fruit trace 50
            }
            if (this.Caste.Equals("Ninja"))
            {
                hasTarget = false;
            }
            if (CarringFruit != null)
                GoBackToAnthill();
        }

        #endregion

    }
}