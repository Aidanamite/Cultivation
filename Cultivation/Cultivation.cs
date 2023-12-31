using UnityEngine;
using System.Collections;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine.UI;
using HMLLibrary;
using UnityEngine.SceneManagement;
using HMLLibrary;
using RaftModLoader;

public class Cultivation : Mod
{
    Harmony harmony;
    static ModData entry;
    public static CustomCrop[] crops;
    static Button unloadButton;
    static Button.ButtonClickedEvent eventStore = null;
    static bool started;
    public static Transform prefabParent;
    static bool CanUnload
    {
        get { return !entry.jsonmodinfo.isModPermanent; }
        set
        {
            if (value != CanUnload)
            {
                entry.jsonmodinfo.isModPermanent = !value;
                ModManagerPage.RefreshModState(entry);
            }
        }
    }

    public void Start()
    {
        entry = modlistEntry;
        started = false;
        unloadButton = entry.modinfo.unloadBtn.GetComponent<Button>();
        if (SceneManager.GetActiveScene().name != Raft_Network.MenuSceneName)
        {
            unloadButton.onClick.Invoke();
            throw new ModLoadException("Mod must be loaded on the main menu");
        }
        prefabParent = new GameObject("prefabParent").transform;
        prefabParent.gameObject.SetActive(false);
        DontDestroyOnLoad(prefabParent.gameObject);
        harmony = new Harmony("com.aidanamite.AdvancedBasicCultivation");
        harmony.PatchAll();
        Resources.LoadAll("");
        harmony.Unpatch(typeof(RConsole).GetMethod("HandleUnityLog", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic), HarmonyPatchType.Prefix, harmony.Id);
        Initialize();
        Resources.UnloadUnusedAssets();
        Log("Mod has been loaded!");
        started = true;
    }

    public override void WorldEvent_WorldLoaded()
    {
        CanUnload = false;
    }

    public override void WorldEvent_WorldUnloaded()
    {
        CanUnload = true;
    }

    public void OnModUnload()
    {
        if (!started)
            return;
        foreach (var crop in crops)
            if (!crop.wasUseable)
            {
                InternalItemAPI.itemObjects.Remove(crop.Seed);
                Traverse.Create(crop.Seed.settings_usable).Field("isUsable").SetValue(false);
            }
        Destroy(prefabParent);
        harmony.UnpatchAll(harmony.Id);
        Log("Mod has been unloaded!");
    }

    public static void Initialize()
    {
        if (crops != null)
            return;
        crops = new CustomCrop[]
        {
            new CustomCrop(
                "Plant_Mushroom",
                GetPrefabByName<Mesh>("Mushroom2"),
                GetPrefabByName<Material>("Log_CaveVines_Mushrooms_SilverAlgae_BerryBush_NoShimmer"),
                Vector3.one,
                Vector3.up * 0.5f,
                ItemManager.GetItemByName("CaveMushroom"),
                GetPrefabByName<Plant>("Plant_Watermelon"),
                RandomDrops: new RandomItem[] { new RandomItem() { obj = ItemManager.GetItemByName("CaveMushroom"), spawnChance = "100%", weight = 1 } },
                RandomDropCount: new Interval_Int() { maxValue = 0, minValue = 1}
            ),
            new CustomCrop(
                "Plant_RedBerries",
                GetPrefabByName<Mesh>("BerryBush"),
                GetPrefabByName<Material>("Log_CaveVines_Mushrooms_SilverAlgae_BerryBush_NoShimmer"),
                Vector3.one * 1.5f,
                Vector3.up * 0.75f,
                ItemManager.GetItemByName("Berries_Red"),
                GetPrefabByName<Plant>("Plant_Pineapple"),
                MaxScale: Vector3.one * 0.4f,
                Yield: new List<Cost>(),
                RandomDrops: new RandomItem[] { new RandomItem() { obj = ItemManager.GetItemByName("Berries_Red"), spawnChance = "100%", weight = 1 } },
                RandomDropCount: new Interval_Int() { maxValue = 1, minValue = 5}
            ),
            new CustomCrop(
                "Plant_Algae",
                GetPrefabByName<Mesh>("SilverAlgae"),
                GetPrefabByName<Material>("Log_CaveVines_Mushrooms_SilverAlgae_BerryBush_NoShimmer"),
                Vector3.one,
                Vector3.up * 0.5f,
                ItemManager.GetItemByName("SilverAlgae"),
                GetPrefabByName<Plant>("Plant_Pineapple"),
                MinScale: Vector3.one * 0.075f,
                Yield: new List<Cost>(),
                RandomDrops: new RandomItem[] { new RandomItem() { obj = ItemManager.GetItemByName("SilverAlgae"), spawnChance = "100%", weight = 1 } },
                RandomDropCount: new Interval_Int() { maxValue = 1, minValue = 3},
                MoistureRequired: 3
            ),
            new CustomCrop(
                "Plant_Kelp",
                GetPrefabByName<Mesh>("Seavine"),
                GetPrefabByName<Material>("NewFlora"),
                new Vector3(1, 6, 1),
                Vector3.up * 3f,
                ItemManager.GetItemByName("SeaVine"),
                GetPrefabByName<Plant>("Plant_Beet"),
                MaxScale: Vector3.one * 0.15f,
                MinScale: Vector3.one * 0.025f,
                MoistureRequired: 10
            ),
            new CustomCrop(
                "Plant_Juniper",
                GetPrefabByName<Mesh>("Bush_02_LOD0"),
                GetPrefabByName<Material>("Bushes"),
                Vector3.one,
                Vector3.up * 0.5f,
                ItemManager.GetItemByName("Juniper"),
                GetPrefabByName<Plant>("Plant_Pineapple"),
                MaxScale: Vector3.one * 0.4f,
                MinScale: Vector3.one * 0.1f,
                RandomDropCount: new Interval_Int() { maxValue = 1, minValue = 3}
            ),
            new CustomCrop(
                "Plant_Turmeric",
                GetPrefabByName<Mesh>("Plane.048"),
                GetPrefabByName<Material>("Vegetation"),
                Vector3.one,
                Vector3.up * 0.5f,
                ItemManager.GetItemByName("Turmeric"),
                GetPrefabByName<Plant>("Plant_Pineapple"),
                MaxScale: Vector3.one * 0.3f,
                MinScale: Vector3.one * 0.05f,
                RandomDropCount: new Interval_Int() { maxValue = 1, minValue = 3}
            ),
            new CustomCrop(
                "Plant_Chili",
                GetPrefabByName<Mesh>("Brush_01_LOD0"),
                GetPrefabByName<Material>("Bushes"),
                Vector3.one * 0.5f,
                Vector3.up * 0.25f,
                ItemManager.GetItemByName("Chili"),
                GetPrefabByName<Plant>("Plant_Beet"),
                MaxScale: Vector3.one * 0.4f,
                MinScale: Vector3.one * 0.1f,
                RandomDropCount: new Interval_Int() { maxValue = 1, minValue = 3}
            )
        };
    }

    public static T GetPrefabByName<T>(string name) where T : Object
    {
        foreach (var item in Resources.FindObjectsOfTypeAll<T>())
            if (item.name == name)
                return item;
        Debug.LogWarning("Failed to find a " + typeof(T).Name + " named " + name);
        return null;
    }
}

class ModLoadException : System.Exception
{
    public ModLoadException(string message) : base(message) { }
}

public class CustomCrop
{
    string name;
    GameObject prefab;
    Mesh model;
    Material material;
    Vector3 size;
    Vector3 center;
    Vector3 minScale;
    Vector3 maxScale;
    Item_Base seed;
    Plant copyRules;
    Item_Base fruit;
    List<Cost> yieldItems;
    RandomItem[] randomDrops;
    Interval_Int randomDropCount;
    int waterCount;
    public bool wasUseable;

    public Item_Base Seed => seed;
    public Plant CopyFrom => copyRules;
    public int WatersRequired => waterCount;

    public CustomCrop(string Name, Mesh Mesh, Material Material, Vector3 Size, Vector3 Center, Item_Base SeedItem, Plant CopyPlantRules, Item_Base Fruit = null, Vector3 MinScale = default, Vector3 MaxScale = default, List<Cost> Yield = null, RandomItem[] RandomDrops = null, Interval_Int RandomDropCount = null, int MoistureRequired = 1)
    {
        name = Name;
        model = Mesh;
        material = Material;
        size = Size;
        center = Center;
        seed = SeedItem;
        copyRules = CopyPlantRules;
        fruit = Fruit;
        if (MaxScale == default)
            maxScale = Vector3.one * 0.35f;
        else
            maxScale = MaxScale;
        if (MinScale == default)
            minScale = Vector3.one * 0.05f;
        else
            minScale = MinScale;
        if (Yield == null)
        {
            if (fruit == null)
                yieldItems = new List<Cost> { new Cost(seed, 2) };
            else
                yieldItems = new List<Cost> { new Cost(fruit, 1) };
        }
        else
            yieldItems = Yield;
        if (RandomDrops == null)
        {
            if (fruit == null)
                randomDrops = new RandomItem[0];
            else
                randomDrops = new RandomItem[]
                {
                    new RandomItem()
                    {
                        obj = seed,
                        spawnChance = "40%",
                        weight = 2
                    },
                    new RandomItem()
                    {
                        obj = null,
                        spawnChance = "60%",
                        weight = 3
                    }
                };
        }
        else
            randomDrops = RandomDrops;
        if (RandomDropCount == null)
        {
            if (fruit == null)
                randomDropCount = new Interval_Int() { maxValue = 0, minValue = 0 };
            else
                randomDropCount = new Interval_Int() { maxValue = 2, minValue = 0 };
        }
        else
            randomDropCount = RandomDropCount;
        waterCount = MoistureRequired;
        Initialize();
    }

    void Initialize()
    {
        prefab = new GameObject(name);
        prefab.transform.SetParent(Cultivation.prefabParent);
        prefab.layer = copyRules.gameObject.layer;
        prefab.tag = copyRules.tag;

        var mesh = new GameObject(name + "_Model");
        mesh.transform.SetParent(prefab.transform, false);
        mesh.AddComponent<MeshRenderer>().material = material;
        mesh.AddComponent<MeshFilter>().mesh = model;

        var collider = prefab.AddComponent<BoxCollider>();
        collider.size = size;
        collider.center = center;
        collider.enabled = true;
        collider.isTrigger = true;

        var yield = prefab.AddComponent<YieldHandler>();
        yield.yieldAsset = ScriptableObject.CreateInstance<SO_ItemYield>();
        yield.yieldAsset.yieldAssets = yieldItems;

        RandomDropper dropper = null;
        if (randomDrops != null && randomDrops.Length != 0)
        {
            dropper = prefab.AddComponent<RandomDropper>();
            var sO = ScriptableObject.CreateInstance<SO_RandomDropper>();
            sO.randomizer = new Randomizer() { items = randomDrops };
            Traverse.Create(dropper).Field("randomDropperAsset").SetValue(sO);
            Traverse.Create(dropper).Field("amountOfItems").SetValue(randomDropCount);
        }

        var pickup = prefab.AddComponent<PickupItem>();
        pickup.canBePickedUp = false;
        pickup.isDropped = false;
        pickup.pickupItemType = PickupItemType.Default;
        if (fruit == null)
            pickup.pickupTerm = "Item/" + seed.UniqueName;
        else
            pickup.pickupTerm = "Item/" + fruit.UniqueName;
        pickup.requiresHands = true;
        pickup.dropper = dropper;
        pickup.yieldHandler = yield;

        var pickupNetwork = prefab.AddComponent<PickupItem_Networked>();
        var pN = copyRules.GetComponent<PickupItem_Networked>();
        pickupNetwork.spawnType = pN.spawnType;
        pickupNetwork.stopTrackingOnPickup = pN.stopTrackingOnPickup;
        pickupNetwork.stopTrackUseRPC = pN.stopTrackUseRPC;
        Traverse.Create(pickupNetwork).Field("pickupItem").SetValue(pickup);
        pickupNetwork.originalScene = pN.originalScene;

        pickup.networkID = pickupNetwork;

        var plant = prefab.AddComponent<Plant>();
        plant.growTime = copyRules.growTime;
        plant.harvestable = copyRules.harvestable;
        plant.item = seed;
        plant.maxScale = maxScale;
        plant.minScale = minScale;
        plant.playerCanHarvest = copyRules.playerCanHarvest;
        plant.pickupComponent = pickup;

        wasUseable = seed.settings_usable.IsUsable();
        if (!wasUseable)
            Traverse.Create(seed.settings_usable).Field("isUsable").SetValue(true);
    }

    public Plant GetPlant()
    {
        if (prefab == null)
            Initialize();
        return prefab.GetComponent<Plant>();
    }
}

[HarmonyPatch(typeof(PlantManager))]
class Patch_NewPlantManager
{
    static List<PlantManager> modified = new List<PlantManager>();
    [HarmonyPatch("Start")]
    [HarmonyPrefix]
    public static void Start(PlantManager __instance, ref List<Plant> ___allPlants)
    {
        if (!modified.AddUniqueOnly(__instance))
            return;
        foreach (var crop in Cultivation.crops)
            ___allPlants.Add(crop.GetPlant());
    }
    [HarmonyPatch("GetPlantByIndex")]
    [HarmonyPrefix]
    public static void GetPlantByIndex(PlantManager __instance, ref List<Plant> ___allPlants) => Start(__instance, ref ___allPlants);
}

[HarmonyPatch(typeof(Cropplot), "OnFinishedPlacement")]
class Patch_NewCropplot
{
    public static void Prefix(Cropplot __instance)
    {
        if (__instance is Cropplot_Grass)
            return;
        foreach (var crop in Cultivation.crops)
            if (__instance.AcceptsPlantType(crop.CopyFrom.item))
                __instance.acceptableItemTypes.Add(crop.Seed);
    }
}

[HarmonyPatch(typeof(Network_Player), "Start")]
class Patch_NewPlayer
{
    public static void Prefix(Network_Player __instance)
    {
        if (!__instance.IsLocalPlayer)
            return;
        var rightHandParent = __instance.currentModel.rightHandItemHolder.Find("RightHandParent");
        foreach (var crop in Cultivation.crops) {
            GameObject obj = null;
            foreach (var connection in __instance.PlayerItemManager.useItemController.allConnections)
                if (connection.inventoryItem.UniqueIndex == crop.Seed.UniqueIndex)
                    obj = connection.obj;
            if (obj == null)
            {
                obj = new GameObject(crop.Seed.UniqueName);
                obj.SetActive(false);
                obj.transform.localPosition = Vector3.zero;
                obj.transform.SetParent(rightHandParent, false);
                var connect = new ItemConnection() { inventoryItem = crop.Seed, name = crop.Seed.UniqueName, obj = obj };
                __instance.PlayerItemManager.useItemController.allConnections.Add(connect);
                Traverse.Create(__instance.PlayerItemManager.useItemController).Field("connectionDictionary").GetValue<Dictionary<string, ItemConnection>>().Add(crop.Seed.UniqueName, connect);
            }
            if (obj.GetComponent<PlantComponent>() == null)
            {
                var plant = Traverse.Create(obj.AddComponent<PlantComponent>());
                plant.Field("plantPrefab").SetValue(crop.GetPlant());
                plant.Field("plantManager").SetValue(__instance.PlantManager);
            }
            else
                Debug.LogWarning("Could not initialize planting script for " + crop.Seed.settings_Inventory.DisplayName + " because it is already plantable");
        }
    }
}

[HarmonyPatch(typeof(Plant))]
class Patch_UpdatePlantGrowth
{
    [HarmonyPatch("Grow")]
    [HarmonyPrefix]
    public static bool Grow(Plant __instance, ref float ___growTimer, ref float ___growTimeSec)
    {
        foreach (var crop in Cultivation.crops)
            if (__instance.item == crop.Seed)
            {
                if (__instance.FullyGrown())
                    return true;
                var WaterInterval = ___growTimeSec / crop.WatersRequired;
                var next = Mathf.FloorToInt((___growTimer + Time.deltaTime) / WaterInterval);
                if (Mathf.Floor(___growTimer / WaterInterval) < next && next < crop.WatersRequired)
                {
                    ___growTimer = next * WaterInterval;
                    foreach (var slot in __instance.cropplot.plantationSlots)
                        if (slot.plant == __instance)
                        {
                            if (ComponentManager<WeatherManager>.Value.GetCurrentWeatherType() != UniqueWeatherType.Rain)
                                slot.RemoveWater();
                            break;
                        }
                    return false;
                }
                break;
            }
        return true;
    }


    [HarmonyPatch("ShouldBeFullyGrown")]
    [HarmonyPrefix]
    public static void ShouldBeFullyGrown(Plant __instance, ref float ___growTimer, ref float ___growTimeSec) => __instance.transform.localScale = __instance.minScale + (__instance.maxScale - __instance.minScale) * Mathf.Clamp(___growTimer / ___growTimeSec, 0f, 1f);
}

[HarmonyPatch(typeof(RConsole), "HandleUnityLog")]
class Patch_Log
{
    public static bool Prefix() => false;
}