using System;
using System.Collections.Generic;
using System.Linq;
using CoffeeKing.Core;
using CoffeeKing.Orders;
using CoffeeKing.StageFlow;
using CoffeeKing.Util;
using CoffeeKing.View;
using UnityEngine;

namespace CoffeeKing.CustomerLogic
{
    public sealed class CustomerSpawner
    {
        private static readonly string[] AppearanceAssets =
        {
            SpriteAssetNames.Customer01,
            SpriteAssetNames.Customer02,
            SpriteAssetNames.Customer03,
            SpriteAssetNames.Customer04,
            SpriteAssetNames.Customer05
        };

        private static readonly Vector3[] LanePositions =
        {
            new Vector3(-6.0f, 3.0f, 0f),
            new Vector3(6.0f, 3.0f, 0f)
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
            if (stage.AllowedRecipes.Count == stage.CustomerCount)
            {
                var exactPool = stage.AllowedRecipes.ToList();
                for (var index = exactPool.Count - 1; index > 0; index--)
                {
                    var swapIndex = random.Next(index + 1);
                    var temp = exactPool[index];
                    exactPool[index] = exactPool[swapIndex];
                    exactPool[swapIndex] = temp;
                }

                for (var index = 0; index < exactPool.Count; index++)
                {
                    var recipe = exactPool[index];
                    var customer = new Customer(index + 1, recipe, GetPatienceForRecipe(stage, recipe), GetRandomAppearance(random));
                    plannedCustomers.Add(customer);
                    views[customer] = CustomerView.Create(customerLayer, config);
                }
            }
            else
            {
                for (var index = 0; index < stage.CustomerCount; index++)
                {
                    var recipe = stage.AllowedRecipes[random.Next(stage.AllowedRecipes.Count)];
                    var customer = new Customer(index + 1, recipe, GetPatienceForRecipe(stage, recipe), GetRandomAppearance(random));
                    plannedCustomers.Add(customer);
                    views[customer] = CustomerView.Create(customerLayer, config);
                }
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
                if (spawnTimer <= 0f && activeCustomers.Count < stageData.MaxSimultaneousCustomers)
                {
                    SpawnNextCustomer();
                    spawnTimer = Mathf.Max(0.5f, stageData.SpawnIntervalSeconds);
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

        public void MarkRejected(Customer customer)
        {
            if (customer == null)
            {
                return;
            }

            if (activeCustomers.Remove(customer))
            {
                customer.MarkServed();
                ServedCount++;
                views[customer].MarkRejected();
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

        public IReadOnlyList<Customer> ResolveRemainingCustomersAsTimedOut()
        {
            var unresolvedCustomers = new List<Customer>();

            isRunning = false;
            spawnTimer = 0f;
            nextSpawnIndex = plannedCustomers.Count;

            for (var index = 0; index < plannedCustomers.Count; index++)
            {
                var customer = plannedCustomers[index];
                if (customer.IsResolved)
                {
                    continue;
                }

                var wasVisibleCustomer = customer.State != CustomerState.Queued;
                customer.MarkTimedOut();
                activeCustomers.Remove(customer);
                TimedOutCount++;
                unresolvedCustomers.Add(customer);

                if (wasVisibleCustomer)
                {
                    views[customer].MarkTimedOut();
                }
            }

            UpdateHighlights(null);
            return unresolvedCustomers;
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

        private static string GetRandomAppearance(System.Random random)
        {
            return AppearanceAssets[random.Next(AppearanceAssets.Length)];
        }
    }
}
