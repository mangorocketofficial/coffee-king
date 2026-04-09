using System;
using System.Collections.Generic;
using System.Linq;
using CoffeeKing.Core;
using CoffeeKing.Orders;
using CoffeeKing.StageFlow;
using CoffeeKing.View;
using UnityEngine;

namespace CoffeeKing.CustomerLogic
{
    public sealed class CustomerSpawner
    {
        private static readonly Vector3[] LanePositions =
        {
            new Vector3(-5.2f, 0.9f, 0f),
            new Vector3(0f, 0.9f, 0f),
            new Vector3(5.2f, 0.9f, 0f)
        };

        private readonly List<Customer> plannedCustomers = new List<Customer>();
        private readonly List<Customer> activeCustomers = new List<Customer>();
        private readonly Dictionary<Customer, CustomerView> views = new Dictionary<Customer, CustomerView>();

        private Transform customerLayer;
        private GameConfig config;
        private StageData stageData;
        private bool isRunning;
        private float spawnTimer;
        private int nextSpawnIndex;

        public event Action<Customer> CustomerTimedOut;

        public IReadOnlyList<Customer> PlannedCustomers => plannedCustomers;
        public IReadOnlyList<Customer> ActiveCustomers => activeCustomers;
        public int TimedOutCount { get; private set; }
        public int ServedCount { get; private set; }

        public void Initialize(GameConfig runtimeConfig, GrayboxSceneContext sceneContext)
        {
            config = runtimeConfig;
            customerLayer = sceneContext.CustomerLayer;
            DestroyViews();
        }

        public void BeginStage(StageData stage, int seed)
        {
            stageData = stage;
            isRunning = false;
            spawnTimer = 0f;
            nextSpawnIndex = 0;
            TimedOutCount = 0;
            ServedCount = 0;
            plannedCustomers.Clear();
            activeCustomers.Clear();
            DestroyViews();

            var random = new System.Random(seed);
            for (var index = 0; index < stage.CustomerCount; index++)
            {
                var recipe = stage.AllowedRecipes[random.Next(stage.AllowedRecipes.Count)];
                var customer = new Customer(index + 1, recipe, GetPatienceForRecipe(stage, recipe));
                plannedCustomers.Add(customer);
                views[customer] = CustomerView.Create(customerLayer, config);
            }
        }

        public void StartSpawning()
        {
            isRunning = true;
            spawnTimer = 0f;
        }

        public void Tick(float deltaTime)
        {
            foreach (var view in views.Values)
            {
                view.Tick(deltaTime);
            }

            if (!isRunning || stageData == null)
            {
                return;
            }

            if (nextSpawnIndex < plannedCustomers.Count)
            {
                spawnTimer -= deltaTime;
                while (spawnTimer <= 0f && activeCustomers.Count < stageData.MaxSimultaneousCustomers && nextSpawnIndex < plannedCustomers.Count)
                {
                    SpawnNextCustomer();
                    spawnTimer += Mathf.Max(0.5f, stageData.SpawnIntervalSeconds);
                }
            }

            for (var index = activeCustomers.Count - 1; index >= 0; index--)
            {
                var customer = activeCustomers[index];
                if (customer.TickPatience(deltaTime))
                {
                    activeCustomers.RemoveAt(index);
                    TimedOutCount++;
                    views[customer].MarkTimedOut();
                    CustomerTimedOut?.Invoke(customer);
                    continue;
                }

                views[customer].SetPatience(customer.PatienceNormalized);
            }
        }

        public bool TryClaimNextCustomer(out Customer customer)
        {
            customer = activeCustomers
                .Where(activeCustomer => activeCustomer.State == CustomerState.Waiting)
                .OrderBy(activeCustomer => activeCustomer.SequenceNumber)
                .FirstOrDefault();

            if (customer == null)
            {
                return false;
            }

            customer.MarkInService();
            UpdateHighlights(customer);
            return true;
        }

        public void ReleaseClaim(Customer customer)
        {
            if (customer == null || customer.IsResolved || customer.State != CustomerState.InService)
            {
                return;
            }

            customer.MarkWaiting();
            UpdateHighlights(null);
        }

        public void MarkServed(Customer customer)
        {
            if (customer == null)
            {
                return;
            }

            if (activeCustomers.Remove(customer))
            {
                customer.MarkServed();
                ServedCount++;
                views[customer].MarkServed();
                UpdateHighlights(null);
            }
        }

        public bool IsFinished => nextSpawnIndex >= plannedCustomers.Count && activeCustomers.Count == 0;

        public string GetQueueSummary()
        {
            if (activeCustomers.Count == 0)
            {
                return nextSpawnIndex >= plannedCustomers.Count ? "Queue empty" : "Waiting for next customer";
            }

            var lines = activeCustomers
                .OrderBy(customer => customer.SequenceNumber)
                .Select(customer =>
                {
                    var marker = customer.State == CustomerState.InService ? ">" : "-";
                    return $"{marker} C{customer.SequenceNumber} {customer.Order.DisplayName} {customer.PatienceRemaining:0}s";
                });

            return string.Join("\n", lines);
        }

        public void DestroyViews()
        {
            foreach (var view in views.Values)
            {
                view.Destroy();
            }

            views.Clear();
        }

        private void SpawnNextCustomer()
        {
            var customer = plannedCustomers[nextSpawnIndex++];
            var laneIndex = AcquireLane();
            customer.AssignLane(laneIndex);
            customer.MarkWaiting();
            activeCustomers.Add(customer);
            views[customer].Bind(customer, LanePositions[laneIndex]);
            views[customer].SetFocused(false);
        }

        private int AcquireLane()
        {
            for (var laneIndex = 0; laneIndex < LanePositions.Length; laneIndex++)
            {
                var isOccupied = activeCustomers.Any(customer => customer.LaneIndex == laneIndex);
                if (!isOccupied)
                {
                    return laneIndex;
                }
            }

            return 0;
        }

        private void UpdateHighlights(Customer focusedCustomer)
        {
            foreach (var activeCustomer in activeCustomers)
            {
                views[activeCustomer].SetFocused(activeCustomer == focusedCustomer);
            }
        }

        private static float GetPatienceForRecipe(StageData stage, DrinkRecipe recipe)
        {
            var patience = stage.PatienceSeconds;

            if (recipe.HasStep(RecipeStep.SteamMilk))
            {
                patience += 25f;
            }

            if (recipe.HasStep(RecipeStep.Syrup))
            {
                patience += 5f;
            }

            return patience;
        }
    }
}
