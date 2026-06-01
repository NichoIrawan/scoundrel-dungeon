using Assets.Scripts;
using Assets.Scripts.SaveSystem;
using Assets.Scripts.ScriptableObjects;
using Assets.Scripts.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.Manager
{
    public class DungeonManager : MonoBehaviour, IDataPersistence
    {
        [SerializeField] private string seed = "default_seed";
        [SerializeField] private bool loadActiveSaveOnStart = true;
        [SerializeField] private EncounterRegistry encounterRegistry;

        private readonly Queue<int> unresolvedNodes = new();
        private DungeonState dungeonState = new();
        private bool dungeonStateInitialized;

        public DungeonState State => dungeonState;
        public int CurrentNode => dungeonState.CurrentNode;
        public IReadOnlyCollection<int> UnresolvedNodes => unresolvedNodes;

        // Fixed Dungeon Mapper for MVP only
        private static readonly (int left, int right)[] FixedTopology = new[]
        {
            (1, 2),    
            (3, -1),   
            (3, -1),   
            (4, 5),    
            (6, -1),   
            (6, -1),   
            (-1, -1), 
        };
        
        // Static for MVP only
        private static readonly (string id, int count)[] DeckDefinition = new[]
        {
            // Monsters
            ("slime",    6),
            ("goblin",   6),
            ("skeleton", 4),
            ("ogre",     2),
            // Weapons
            ("dagger",   4),
            ("sword",    3),
            ("axe",      2),
            // Potions
            ("potion_4",  5),
            ("potion_8",  4),
            ("potion_12", 2),
        };
        private const int TotalDeckSize = 38;
        private const int CardsDrawn = 24;
        private const int RoomCount = 6; 

        private void Awake()
        {
            if (encounterRegistry != null)
                encounterRegistry.Initialize();

            InitializeDungeonState();
        }

        public void GenerateDungeon(string mapSeed = null)
        {
            if (encounterRegistry != null)
                encounterRegistry.Initialize();

            seed = string.IsNullOrWhiteSpace(mapSeed) ? "default_seed" : mapSeed;
            dungeonState = GenerateDungeonState(seed);
            unresolvedNodes.Clear();
            dungeonStateInitialized = true;
        }

        public string[] GetCurrentRoomEncounterIds()
        {
            InitializeDungeonState();
            return dungeonState.Nodes.TryGetValue(dungeonState.CurrentNode, out var node)
                ? node.EncounterIds
                : Array.Empty<string>();
        }

        public string[] GetSelectedEncounterIdsForCurrentRoom()
        {
            InitializeDungeonState();
            return dungeonState.Nodes.TryGetValue(dungeonState.CurrentNode, out var node)
                ? node.SelectedEncounterIds
                : Array.Empty<string>();
        }

        public void SetCurrentRoomSelection(IEnumerable<string> encounterIds)
        {
            InitializeDungeonState();
            if (!dungeonState.Nodes.TryGetValue(dungeonState.CurrentNode, out var node)) return;

            node.SelectedEncounterIds = encounterIds
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToArray();
        }

        public bool IsAtExit() => dungeonState.CurrentNode == 6;
        public bool IsQueueEmpty() => unresolvedNodes.Count == 0;

        public EncounterScriptableObject GetEncounter(string encounterId)
        {
            return encounterRegistry?.TryGetEncounter(encounterId);
        }

        public (int monsters, int weapons, int potions) GetRemainingDeckStats()
        {
            InitializeDungeonState();

            if (encounterRegistry?.Dictionary == null)
                return (0, 0, 0);

            int monsters = 0, weapons = 0, potions = 0;
            foreach (var pair in dungeonState.Nodes.Values
                .Where(n => !n.IsResolved)
                .SelectMany(n => n.EncounterIds))
            {
                var so = encounterRegistry.TryGetEncounter(pair);
                if (so is EnemiesScriptableObject) monsters++;
                else if (so is EquipmentsScriptableObject) weapons++;
                else if (so is ConsumablesScriptableObject) potions++;
            }

            return (monsters, weapons, potions);
        }

        public bool AdvanceToRoom(int chosenNodeId)
        {
            InitializeDungeonState();

            if (!dungeonState.Nodes.TryGetValue(dungeonState.CurrentNode, out var currentNode))
                return false;

            currentNode.IsResolved = true;
            currentNode.IsEscaped = false;

            var left = currentNode.LeftNode;
            var right = currentNode.RightNode;

            if (left >= 0 && right >= 0)
            {
                var unchosenId = (chosenNodeId == left) ? right : left;
                if (!unresolvedNodes.Contains(unchosenId))
                {
                    unresolvedNodes.Enqueue(unchosenId);
                    SyncUnresolvedNodesToState();
                }
            }

            dungeonState.CurrentNode = chosenNodeId;
            SyncUnresolvedNodesToState();
            return true;
        }

        public bool AdvanceToNextRoom()
        {
            InitializeDungeonState();

            if (!dungeonState.Nodes.TryGetValue(dungeonState.CurrentNode, out var currentNode))
                return false;

            currentNode.IsResolved = true;
            currentNode.IsEscaped = false;

            if (unresolvedNodes.Count > 0)
            {
                dungeonState.CurrentNode = unresolvedNodes.Dequeue();
                SyncUnresolvedNodesToState();
                return true;
            }

            var left = currentNode.LeftNode;
            var right = currentNode.RightNode;

            if (left >= 0 && right >= 0)
            {
                unresolvedNodes.Enqueue(right);
            }

            var nextNode = left >= 0 ? left : right;
            if (nextNode < 0 || !dungeonState.Nodes.ContainsKey(nextNode))
                return false;

            dungeonState.CurrentNode = nextNode;
            SyncUnresolvedNodesToState();
            return true;
        }

        public bool EscapeCurrentRoom()
        {
            InitializeDungeonState();

            if (!dungeonState.Nodes.TryGetValue(dungeonState.CurrentNode, out var currentNode))
                return false;

            if (!unresolvedNodes.Contains(currentNode.NodeId))
            {
                currentNode.IsEscaped = true;
                currentNode.IsResolved = false;
                unresolvedNodes.Enqueue(currentNode.NodeId);
                SyncUnresolvedNodesToState();
            }

            var nextNode = currentNode.LeftNode >= 0 ? currentNode.LeftNode : currentNode.RightNode;
            if (nextNode < 0 || !dungeonState.Nodes.ContainsKey(nextNode))
                return false;

            dungeonState.CurrentNode = nextNode;
            return true;
        }

        public bool DequeueNextUnresolvedRoom()
        {
            InitializeDungeonState();
            if (unresolvedNodes.Count == 0) return false;

            dungeonState.CurrentNode = unresolvedNodes.Dequeue();
            SyncUnresolvedNodesToState();
            return true;
        }

        public void LoadData(GameData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            seed = data.Seed;
            dungeonState = UpgradeDungeonState(data);
            SyncUnresolvedNodesFromState();
            dungeonStateInitialized = true;
        }

        public void SaveData(ref GameData data)
        {
            InitializeDungeonState();
            SyncUnresolvedNodesToState();

            data.Seed = seed;
            data.CurrentRoomIndex = dungeonState.CurrentNode;
            data.DungeonState = dungeonState;
            data.DungeonState.Nodes = new SerializedDictionary<int, DungeonNode>(dungeonState.Nodes);

            data.EffigiesInRoom = new SerializedDictionary<int, string[]>(
                dungeonState.Nodes.ToDictionary(p => p.Key, p => p.Value.EncounterIds));
            data.SelectedEffigiesByRoom = new SerializedDictionary<int, string[]>(
                dungeonState.Nodes.ToDictionary(p => p.Key, p => p.Value.SelectedEncounterIds));
        }

        private void InitializeDungeonState()
        {
            if (dungeonStateInitialized) return;

            if (loadActiveSaveOnStart && SaveGameBridge.HasActiveGameData)
            {
                LoadData(SaveGameBridge.ActiveGameData);
            }
            else if (dungeonState.Nodes.Count == 0)
            {
                GenerateDungeon(seed);
            }

            dungeonStateInitialized = true;
        }

        private DungeonState GenerateDungeonState(string mapSeed)
        {
            var state = new DungeonState();
            var random = new System.Random(mapSeed.GetHashCode());

            // Build and shuffle the 38-card deck
            var deck = BuildDeck();
            Shuffle(deck, random);

            var drawn = deck.Take(CardsDrawn).ToList();

            for (int i = 0; i < FixedTopology.Length; i++)
            {
                var (left, right) = FixedTopology[i];
                var encounterIds = new string[4];

                if (i < RoomCount)
                {
                    for (int slot = 0; slot < 4; slot++)
                    {
                        encounterIds[slot] = drawn[i * 4 + slot];
                    }
                }

                state.Nodes[i] = new DungeonNode
                {
                    NodeId = i,
                    EncounterIds = encounterIds,
                    LeftNode = left,
                    RightNode = right,
                    SelectedEncounterIds = Array.Empty<string>()
                };
            }

            state.CurrentNode = 0; 
            return state;
        }

        private static List<string> BuildDeck()
        {
            var deck = new List<string>(TotalDeckSize);
            foreach (var (id, count) in DeckDefinition)
            {
                for (int i = 0; i < count; i++)
                    deck.Add(id);
            }
            return deck;
        }

        private static void Shuffle<T>(List<T> list, System.Random random)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        private DungeonState UpgradeDungeonState(GameData data)
        {
            if (data.DungeonState?.Nodes != null && data.DungeonState.Nodes.Count > 0)
                return data.DungeonState;

            var state = new DungeonState { CurrentNode = data.CurrentRoomIndex };

            foreach (var pair in data.EffigiesInRoom)
            {
                data.SelectedEffigiesByRoom.TryGetValue(pair.Key, out var selectedIds);
                var (left, right) = pair.Key < FixedTopology.Length
                    ? FixedTopology[pair.Key]
                    : (-1, -1);

                state.Nodes[pair.Key] = new DungeonNode
                {
                    NodeId = pair.Key,
                    EncounterIds = pair.Value ?? Array.Empty<string>(),
                    SelectedEncounterIds = selectedIds ?? Array.Empty<string>(),
                    LeftNode = left,
                    RightNode = right
                };
            }

            if (state.Nodes.Count == 0)
                state = GenerateDungeonState(seed);

            return state;
        }

        private void SyncUnresolvedNodesFromState()
        {
            unresolvedNodes.Clear();
            if (dungeonState.UnresolvedNodes == null) return;

            foreach (var nodeId in dungeonState.UnresolvedNodes)
            {
                if (dungeonState.Nodes.ContainsKey(nodeId))
                    unresolvedNodes.Enqueue(nodeId);
            }
        }

        private void SyncUnresolvedNodesToState()
        {
            dungeonState.UnresolvedNodes = unresolvedNodes.ToArray();
        }
    }
}
