using CoffeeKing.Orders;
using UnityEngine;

namespace CoffeeKing.CustomerLogic
{
    public enum CustomerState
    {
        Queued,
        Waiting,
        InService,
        Served,
        TimedOut
    }

    public sealed class Customer
    {
        public Customer(int sequenceNumber, DrinkRecipe order, float patienceDuration)
        {
            SequenceNumber = sequenceNumber;
            Order = order;
            PatienceDuration = patienceDuration;
            PatienceRemaining = patienceDuration;
            State = CustomerState.Queued;
            LaneIndex = -1;
        }

        public int SequenceNumber { get; }
        public DrinkRecipe Order { get; }
        public float PatienceDuration { get; }
        public float PatienceRemaining { get; private set; }
        public CustomerState State { get; private set; }
        public int LaneIndex { get; private set; }

        public bool IsResolved => State == CustomerState.Served || State == CustomerState.TimedOut;

        public float PatienceNormalized => PatienceDuration <= 0f ? 0f : Mathf.Clamp01(PatienceRemaining / PatienceDuration);

        public void AssignLane(int laneIndex)
        {
            LaneIndex = laneIndex;
        }

        public void MarkWaiting()
        {
            State = CustomerState.Waiting;
        }

        public void MarkInService()
        {
            State = CustomerState.InService;
        }

        public void MarkServed()
        {
            State = CustomerState.Served;
        }

        public void MarkTimedOut()
        {
            PatienceRemaining = 0f;
            State = CustomerState.TimedOut;
        }

        public bool TickPatience(float deltaTime)
        {
            if (IsResolved || State == CustomerState.Queued)
            {
                return false;
            }

            PatienceRemaining = Mathf.Max(0f, PatienceRemaining - deltaTime);
            if (PatienceRemaining <= 0f)
            {
                MarkTimedOut();
                return true;
            }

            return false;
        }
    }
}
