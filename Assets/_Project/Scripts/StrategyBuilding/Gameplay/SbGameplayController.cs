using System;
using TnieYuPackage.DesignPatterns;
using TnieYuPackage.Utils.Structures;

namespace Game.StrategyBuilding
{
    public enum SbResource
    {
        Money,
        People,
        Wood,
        Stone,
        MaxPeople,
        MaxWood,
        MaxStone
    }

    public enum SbArmy
    {
        Melee,
        Range,
        Strong,
        Quick
    }

    public class SbGameplayController : SingletonBehavior<SbGameplayController>
    {
        #region PROPERTIES

        public ObservableValue<int> moneyNumber;
        public ObservableValue<int> peopleNumber;
        public ObservableValue<int> woodNumber;
        public ObservableValue<int> stoneNumber;
        public ObservableValue<int> maxPeopleNumber;
        public ObservableValue<int> maxWoodNumber;
        public ObservableValue<int> maxStoneNumber;

        public ObservableValue<int> meleeArmyNumber;
        public ObservableValue<int> rangeArmyNumber;
        public ObservableValue<int> strongArmyNumber;
        public ObservableValue<int> quickArmyNumber;

        #endregion

        private void Start()
        {
            DefaultSetup();
        }

        //test
        private void DefaultSetup()
        {
            moneyNumber.Value = 0;
            peopleNumber.Value = 0;
            woodNumber.Value = 0;
            stoneNumber.Value = 0;

            maxPeopleNumber.Value = 10;
            maxWoodNumber.Value = 100;
            maxStoneNumber.Value = 100;
        }

        //support

        public static ref ObservableValue<int> GetResource(SbResource type)
        {
            switch (type)
            {
                case SbResource.Money: return ref Instance.moneyNumber;
                case SbResource.People: return ref Instance.peopleNumber;
                case SbResource.Wood: return ref Instance.woodNumber;
                case SbResource.Stone: return ref Instance.stoneNumber;
                case SbResource.MaxPeople: return ref Instance.maxPeopleNumber;
                case SbResource.MaxWood: return ref Instance.maxWoodNumber;
                case SbResource.MaxStone: return ref Instance.maxStoneNumber;

                default: throw new ArgumentOutOfRangeException();
            }
        }

        public static ref ObservableValue<int> GetArmy(SbArmy type)
        {
            switch (type)
            {
                case SbArmy.Melee: return ref Instance.meleeArmyNumber;
                case SbArmy.Range: return ref Instance.rangeArmyNumber;
                case SbArmy.Strong: return ref Instance.strongArmyNumber;
                case SbArmy.Quick: return ref Instance.quickArmyNumber;
                
                default: throw new ArgumentOutOfRangeException();
            }
        }

        public static bool ValidateCost(ActionCost cost)
        {
            if (Instance.moneyNumber.Value < cost.moneyCost) return false;
            
            if (Instance.peopleNumber.Value < cost.peopleCost) return false;
            
            if (Instance.woodNumber.Value < cost.woodCost) return false;
            
            if (Instance.stoneNumber.Value < cost.stoneCost) return false;

            return true;
        }

        [Obsolete("Using ActionCost.CollectCost instead.")]
        public static void ApplyCost(ActionCost cost)
        {
            Instance.moneyNumber.Value -= cost.moneyCost;
            Instance.peopleNumber.Value -= cost.peopleCost;
            Instance.woodNumber.Value -= cost.woodCost;
            Instance.stoneNumber.Value -= cost.stoneCost;
        } 
    }
}