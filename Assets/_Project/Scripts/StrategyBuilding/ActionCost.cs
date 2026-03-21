using System;
using TnieYuPackage.Core;
using TnieYuPackage.GlobalExtensions;
using UnityEngine;

namespace Game.StrategyBuilding
{
    [Serializable]
    public struct ActionCost
    {
        public int moneyCost;
        public int woodCost;
        public int stoneCost;
        public int peopleCost;

        [SerializeField] private bool resourceRefundable;
        [SerializeField] private bool peopleRefundable;

        [SerializeField] private float refundDelayTime;

        public void CollectCost()
        {
#pragma warning disable CS0618 // Type or member is obsolete
            SbGameplayController.ApplyCost(this);
#pragma warning restore CS0618 // Type or member is obsolete

            if (resourceRefundable || peopleRefundable)
                EventManager.Instance.RegistryDelay(RefundCost, refundDelayTime);
        }

        private void RefundCost()
        {
            if (resourceRefundable)
            {
                SbGameplayController.Instance.moneyNumber.Value += moneyCost;
                SbGameplayController.Instance.woodNumber.Value += woodCost;
                SbGameplayController.Instance.stoneNumber.Value += stoneCost;
            }

            if (peopleRefundable)
            {
                SbGameplayController.Instance.peopleNumber.Value += peopleCost;
            }
        }

        public override string ToString()
        {
            return $"Money: {moneyCost}\n" +
                   $"People: {peopleCost}\n" +
                   $"Wood: {woodCost}\n" +
                   $"Stone: {stoneCost}";
        }

        public static void AddCost(ref ActionCost a, in ActionCost b)
        {
            a.moneyCost += b.moneyCost;
            a.peopleCost += b.peopleCost;
            a.woodCost += b.woodCost;
            a.stoneCost += b.stoneCost;
        }

        public static void MinusCost(ref ActionCost a, in ActionCost b)
        {
            a.moneyCost -= b.moneyCost;
            a.peopleCost -= b.peopleCost;
            a.woodCost -= b.woodCost;
            a.stoneCost -= b.stoneCost;
        }
    }
}