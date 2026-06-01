using Assets.Scripts;
using Assets.Scripts.SaveSystem;
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
        [SerializeField] private int roomUnit = 7;
        [SerializeField] private bool loadActiveSaveOnStart = true;
        [SerializeField] private EncounterRegistry encounterRegistry;

        private readonly Queue<int> unresolvedNodes = new();
        private DungeonState dungeonState = new();
        private bool dungeonStateInitialized;

        public DungeonState State => dungeonState;
        public int CurrentNode => dungeonState.CurrentNode;
        public IReadOnlyCollection<int> UnresolvedNodes => unresolvedNodes;

        private void Awake()
        {
            InitializeDictionary();
            InitializeDungeonState();
        }

        public void GenerateDungeon(int roomCount, string mapSeed)
        {
            roomUnit = Mathf.Max(1, roomCount);
            seed = string.IsNullOrWhiteSpace(mapSeed) ? "default_seed" : mapSeed;
            dungeonState = GenerateDungeonState(roomUnit, seed);
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

            if (!dungeonState.Nodes.TryGetValue(dungeonState.CurrentNode, out var node))
            {
                return;
            }

            node.SelectedEncounterIds = encounterIds
                .Where(encounterId => !string.IsNullOrWhiteSpace(encounterId))
                .Distinct()
                .ToArray();
        }

        public bool AdvanceToNextRoom()
        {
            InitializeDungeonState();

            if (!dungeonState.Nodes.TryGetValue(dungeonState.CurrentNode, out var currentNode))
            {
                return false;
            }

            currentNode.IsResolved = true;
            currentNode.IsEscaped = false;

            if (unresolvedNodes.Count > 0)
            {
                dungeonState.CurrentNode = unresolvedNodes.Dequeue();
                SyncUnresolvedNodesToState();
                return true;
            }

            var nextNode = currentNode.LeftNode >= 0 ? currentNode.LeftNode : currentNode.RightNode;
            if (nextNode < 0 || !dungeonState.Nodes.ContainsKey(nextNode))
            {
                return false;
            }

            dungeonState.CurrentNode = nextNode;
            return true;
        }

        public bool EscapeCurrentRoom()
        {
            InitializeDungeonState();

            if (!dungeonState.Nodes.TryGetValue(dungeonState.CurrentNode, out var currentNode))
            {
                return false;
            }

            if (!unresolvedNodes.Contains(currentNode.NodeId))
            {
                currentNode.IsEscaped = true;
                currentNode.IsResolved = false;
                unresolvedNodes.Enqueue(currentNode.NodeId);
                SyncUnresolvedNodesToState();
            }

            var nextNode = currentNode.LeftNode >= 0 ? currentNode.LeftNode : currentNode.RightNode;
            if (nextNode < 0 || !dungeonState.Nodes.ContainsKey(nextNode))
            {
                return false;
            }

            dungeonState.CurrentNode = nextNode;
            return true;
        }

        public void LoadData(GameData data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            seed = data.Seed;
            roomUnit = data.RoomUnit <= 0 ? 7 : data.RoomUnit;
            dungeonState = UpgradeDungeonState(data);
            SyncUnresolvedNodesFromState();
            dungeonStateInitialized = true;
        }

        public void SaveData(ref GameData data)
        {
            InitializeDungeonState();

            data.Seed = seed;
            data.RoomUnit = roomUnit;
            data.CurrentRoomIndex = dungeonState.CurrentNode;
            data.DungeonState = dungeonState;
            data.DungeonState.Nodes = new SerializedDictionary<int, DungeonNode>(dungeonState.Nodes);
            SyncUnresolvedNodesToState();

            data.EffigiesInRoom = new SerializedDictionary<int, string[]>(
                dungeonState.Nodes.ToDictionary(pair => pair.Key, pair => pair.Value.EncounterIds));
            data.SelectedEffigiesByRoom = new SerializedDictionary<int, string[]>(
                dungeonState.Nodes.ToDictionary(pair => pair.Key, pair => pair.Value.SelectedEncounterIds));
        }

        private void InitializeDungeonState()
        {
            if (dungeonStateInitialized)
            {
                return;
            }

            if (loadActiveSaveOnStart && SaveGameBridge.HasActiveGameData)
            {
                LoadData(SaveGameBridge.ActiveGameData);
            }
            else if (dungeonState.Nodes.Count == 0)
            {
                GenerateDungeon(roomUnit, seed);
            }

            dungeonStateInitialized = true;
        }

        private DungeonState GenerateDungeonState(int roomCount, string mapSeed)
        {
            InitializeDictionary();

            var state = new DungeonState();
            var random = new System.Random(mapSeed.GetHashCode());

            for (int i = 0; i < roomCount; i++)
            {
                state.Nodes[i] = new DungeonNode
                {
                    NodeId = i,
                    EncounterIds = GenerateEncounterIds(random),
                    LeftNode = i + 1 < roomCount ? i + 1 : -1,
                    RightNode = -1
                };
            }

            state.CurrentNode = 0;
            return state;
        }

        private string[] GenerateEncounterIds(System.Random random)
        {
            var encounters = new string[4];

            for (int i = 0; i < encounters.Length; i++)
            {
                encounters[i] = RollEncounterId(random);
            }

            return encounters;
        }

        private string RollEncounterId(System.Random random)
        {
            var roll = random.Next(1, 53);

            if (roll <= 26)
            {
                return PickEncounter(encounterRegistry._enemies, roll - 1);
            }

            if (roll <= 39)
            {
                return PickEncounter(encounterRegistry._equipments, roll - 27);
            }

            return PickEncounter(encounterRegistry._consumables, roll - 40);
        }

        private static string PickEncounter(IReadOnlyList<string> encounterIds, int rollOffset)
        {
            if (encounterIds == null || encounterIds.Count == 0)
            {
                return string.Empty;
            }

            var index = Mathf.Clamp(rollOffset * encounterIds.Count / 13, 0, encounterIds.Count - 1);
            return encounterIds[index];
        }

        private DungeonState UpgradeDungeonState(GameData data)
        {
            if (data.DungeonState?.Nodes != null && data.DungeonState.Nodes.Count > 0)
            {
                return data.DungeonState;
            }

            var state = new DungeonState
            {
                CurrentNode = data.CurrentRoomIndex
            };

            foreach (var pair in data.EffigiesInRoom)
            {
                data.SelectedEffigiesByRoom.TryGetValue(pair.Key, out var selectedIds);
                state.Nodes[pair.Key] = new DungeonNode
                {
                    NodeId = pair.Key,
                    EncounterIds = pair.Value ?? Array.Empty<string>(),
                    SelectedEncounterIds = selectedIds ?? Array.Empty<string>(),
                    LeftNode = pair.Key + 1 < data.RoomUnit ? pair.Key + 1 : -1,
                    RightNode = -1
                };
            }

            if (state.Nodes.Count == 0)
            {
                state = GenerateDungeonState(roomUnit, seed);
            }

            return state;
        }

        private void InitializeDictionary()
        {
            if (encounterRegistry == null)
            {
                Debug.LogWarning("DungeonManager requires an EncounterRegistry to generate encounters.");
                return;
            }

            encounterRegistry.Initialize();
        }

        private void SyncUnresolvedNodesFromState()
        {
            unresolvedNodes.Clear();

            if (dungeonState.UnresolvedNodes == null)
            {
                return;
            }

            foreach (var nodeId in dungeonState.UnresolvedNodes)
            {
                if (dungeonState.Nodes.ContainsKey(nodeId))
                {
                    unresolvedNodes.Enqueue(nodeId);
                }
            }
        }

        private void SyncUnresolvedNodesToState()
        {
            dungeonState.UnresolvedNodes = unresolvedNodes.ToArray();
        }
    }
}
