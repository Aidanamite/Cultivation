using UnityEngine;
using System.Collections;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine.UI;
using HMLLibrary;
using UnityEngine.SceneManagement;
using RaftModLoader;
using System;
using Object = UnityEngine.Object;

namespace Cultivation
{
    public class Main : Mod
    {
        public static Main instance;
        Harmony harmony;
        public static CustomCrop[] crops;
        public static List<Object> created = new List<Object>();
        public static Transform prefabParent;
        public override bool CanUnload()
        {
            if (RAPI.IsCurrentSceneMainMenu())
                return base.CanUnload();
            return false;
        }

        public Main()
        {
            if (!RAPI.IsCurrentSceneMainMenu())
                throw new ModLoadException("Mod must be loaded on the main menu");
        }

        public void Start()
        {
            instance = this;
            prefabParent = new GameObject("prefabParent").MarkDestroyOnUnload().transform;
            prefabParent.gameObject.SetActive(false);
            DontDestroyOnLoad(prefabParent.gameObject);
            (harmony = new Harmony("com.aidanamite.AdvancedBasicCultivation")).PatchAll();
            Initialize();
            Log("Mod has been loaded!");
        }

        public void OnModUnload()
        {
            if (crops != null)
                foreach (var crop in crops)
                    if (!crop.wasUseable)
                    {
                        InternalItemAPI.itemObjects.Remove(crop.Seed);
                        Traverse.Create(crop.Seed.settings_usable).Field("isUsable").SetValue(false);
                    }
            foreach (var o in created)
                if (o)
                    Destroy(o);
            harmony?.UnpatchAll(harmony.Id);
            Log("Mod has been unloaded!");
        }

        public static void Initialize()
        {
            if (crops != null)
                return;
            var juniperChilli = new Texture2D(0, 0) { name = "JuniperChili_Tex" }.MarkDestroyOnUnload();
            if (!juniperChilli.LoadImage(instance.GetEmbeddedFileBytes("JuniperChili_Tex.png")))
            {
                Debug.LogError("Failed to load Juniper/Chili texture");
                return;
            }
            if (!LoadModel("Berries", 0, out var berries, out _)
                | !LoadModel("Silver algae", 0, out var algae, out _)
                | !LoadModel("SeavineDecorational", 0, out var kelpMesh, out var kelpMat)
                | !LoadModel("Banana", 0, out var turmericMesh, out var turmericMat)
                | !LoadModel("Tree Bush", 0, out var chilliMesh, out var chilliMat)
                )
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
                berries,
                GetPrefabByName<Material>("Log_CaveVines_Mushrooms_SilverAlgae_BerryBush_NoShimmer"),
                Vector3.one * 1.5f,
                Vector3.up * 0.75f,
                ItemManager.GetItemByName("Berries_Red"),
                GetPrefabByName<Plant>("Plant_Pineapple"),
                MaxScale: Vector3.one * 0.3f,
                Yield: new List<Cost>(),
                RandomDrops: new RandomItem[] { new RandomItem() { obj = ItemManager.GetItemByName("Berries_Red"), spawnChance = "100%", weight = 1 } },
                RandomDropCount: new Interval_Int() { maxValue = 1, minValue = 5}
            ),
            new CustomCrop(
                "Plant_Algae",
                algae,
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
                kelpMesh,
                kelpMat,
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
                CreateMesh(
                    "Plant_Juniper",
                    new Vector2(0f, 0.39453125f),
                    new Vector2(0f, 0.0537109375f),
                    new Vector2(0.5478515625f, 0.0537109375f),
                    new Vector2(0.5478515625f, 0.39453125f),
                    (false, 3, 3, (x, y) => Quaternion.Euler(-10,0,0) * new Vector3(x * 0.6f - 0.3f, y * 0.8f, Curve(y,0.5,0.5) * (Curve(x,0.5,0.5) + 0.5f) * 0.3f)),
                    (false, 3, 3, (x, y) => Quaternion.Euler(-10,120,0) * new Vector3(x * 0.6f - 0.3f, y * 0.8f, Curve(y,0.5,0.5) * (Curve(x,0.5,0.5) + 0.5f) * 0.3f)),
                    (false, 3, 3, (x, y) => Quaternion.Euler(-10,240,0) * new Vector3(x * 0.6f - 0.3f, y * 0.8f, Curve(y,0.5,0.5) * (Curve(x,0.5,0.5) + 0.5f) * 0.3f)),
                    (true, 3, 3, (x, y) => Quaternion.Euler(5,60,0) * new Vector3(x * 0.6f - 0.3f, y * 0.7f, Curve(y,0.5,0.5) * (Curve(x,0.5,0.5) + 0.5f) * 0.1f)),
                    (true, 3, 3, (x, y) => Quaternion.Euler(5,180,0) * new Vector3(x * 0.6f - 0.3f, y * 0.7f, Curve(y,0.5,0.5) * (Curve(x,0.5,0.5) + 0.5f) * 0.1f)),
                    (true, 3, 3, (x, y) => Quaternion.Euler(5,320,0) * new Vector3(x * 0.6f - 0.3f, y * 0.7f, Curve(y,0.5,0.5) * (Curve(x,0.5,0.5) + 0.5f) * 0.1f)),
                    (true, 3, 3, (x, y) => Quaternion.Euler(20,40,0) * new Vector3(x * 0.4f - 0.2f, y * 0.6f, Curve(y,0.5,0.5) * (Curve(x,0.5,0.5) + 0.5f) * 0.2f)),
                    (true, 3, 3, (x, y) => Quaternion.Euler(20,160,0) * new Vector3(x * 0.4f - 0.2f, y * 0.6f, Curve(y,0.5,0.5) * (Curve(x,0.5,0.5) + 0.5f) * 0.2f)),
                    (true, 3, 3, (x, y) => Quaternion.Euler(20,280,0) * new Vector3(x * 0.4f - 0.2f, y * 0.6f, Curve(y,0.5,0.5) * (Curve(x,0.5,0.5) + 0.5f) * 0.2f)),
                    (true, 3, 3, (x, y) => Quaternion.Euler(20,100,0) * new Vector3(x * 0.4f - 0.2f, y * 0.6f, Curve(y,0.5,0.5) * (Curve(x,0.5,0.5) + 0.5f) * 0.2f)),
                    (true, 3, 3, (x, y) => Quaternion.Euler(20,220,0) * new Vector3(x * 0.4f - 0.2f, y * 0.6f, Curve(y,0.5,0.5) * (Curve(x,0.5,0.5) + 0.5f) * 0.2f)),
                    (true, 3, 3, (x, y) => Quaternion.Euler(20,340,0) * new Vector3(x * 0.4f - 0.2f, y * 0.6f, Curve(y,0.5,0.5) * (Curve(x,0.5,0.5) + 0.5f) * 0.2f))
                ).MarkDestroyOnUnload(),
                GetPrefabByName<Material>("TP_PottedPlants").Clone("JuniperChili_Tex").MarkDestroyOnUnload().WithTexture("_MainTex",juniperChilli),
                Vector3.one,
                Vector3.up * 0.5f,
                ItemManager.GetItemByName("Juniper"),
                GetPrefabByName<Plant>("Plant_Pineapple"),
                MaxScale: Vector3.one * 0.7f,
                MinScale: Vector3.one * 0.2f,
                RandomDropCount: new Interval_Int() { maxValue = 1, minValue = 3}
            ),
            new CustomCrop(
                "Plant_Turmeric",
                turmericMesh,
                turmericMat,
                Vector3.one,
                Vector3.up * 0.5f,
                ItemManager.GetItemByName("Turmeric"),
                GetPrefabByName<Plant>("Plant_Pineapple"),
                MaxScale: Vector3.one * 0.25f,
                MinScale: Vector3.one * 0.05f,
                RandomDropCount: new Interval_Int() { maxValue = 1, minValue = 3}
            ),
            new CustomCrop(
                "Plant_Chili",
                CreateMesh(
                    "Plant_Chili",
                    new Vector2(0.3046875f, 0.830078125f),
                    new Vector2(0.017578125f, 0.830078125f),
                    new Vector2(0.017578125f, 0.419921875f),
                    new Vector2(0.3046875f, 0.419921875f),
                    (false, 3, 3, (x, y) => Quaternion.Euler(-5,0,0) * new Vector3(x * 0.6f - 0.3f, y * 0.8f, Curve(y,0.5,0.5) * (Curve(x,0.5,0.5) + 0.5f) * 0.2f)),
                    (false, 3, 3, (x, y) => Quaternion.Euler(-5,120,0) * new Vector3(x * 0.6f - 0.3f, y * 0.8f, Curve(y,0.5,0.5) * (Curve(x,0.5,0.5) + 0.5f) * 0.2f)),
                    (false, 3, 3, (x, y) => Quaternion.Euler(-5,240,0) * new Vector3(x * 0.6f - 0.3f, y * 0.8f, Curve(y,0.5,0.5) * (Curve(x,0.5,0.5) + 0.5f) * 0.2f)),
                    (true, 3, 3, (x, y) => Quaternion.Euler(5,60,0) * new Vector3(x * 0.6f - 0.3f, y * 0.7f, Curve(y,0.5,0.5) * (Curve(x,0.5,0.5) + 0.5f) * 0.15f)),
                    (true, 3, 3, (x, y) => Quaternion.Euler(5,180,0) * new Vector3(x * 0.6f - 0.3f, y * 0.7f, Curve(y,0.5,0.5) * (Curve(x,0.5,0.5) + 0.5f) * 0.15f)),
                    (true, 3, 3, (x, y) => Quaternion.Euler(5,320,0) * new Vector3(x * 0.6f - 0.3f, y * 0.7f, Curve(y,0.5,0.5) * (Curve(x,0.5,0.5) + 0.5f) * 0.15f)),
                    (true, 3, 3, (x, y) => Quaternion.Euler(15,40,0) * new Vector3(x * 0.4f - 0.2f, y * 0.6f, Curve(y,0.5,0.5) * (Curve(x,0.5,0.5) + 0.5f) * 0.3f)),
                    (true, 3, 3, (x, y) => Quaternion.Euler(15,160,0) * new Vector3(x * 0.4f - 0.2f, y * 0.6f, Curve(y,0.5,0.5) * (Curve(x,0.5,0.5) + 0.5f) * 0.3f)),
                    (true, 3, 3, (x, y) => Quaternion.Euler(15,280,0) * new Vector3(x * 0.4f - 0.2f, y * 0.6f, Curve(y,0.5,0.5) * (Curve(x,0.5,0.5) + 0.5f) * 0.3f)),
                    (true, 3, 3, (x, y) => Quaternion.Euler(15,100,0) * new Vector3(x * 0.4f - 0.2f, y * 0.6f, Curve(y,0.5,0.5) * (Curve(x,0.5,0.5) + 0.5f) * 0.3f)),
                    (true, 3, 3, (x, y) => Quaternion.Euler(15,220,0) * new Vector3(x * 0.4f - 0.2f, y * 0.6f, Curve(y,0.5,0.5) * (Curve(x,0.5,0.5) + 0.5f) * 0.3f)),
                    (true, 3, 3, (x, y) => Quaternion.Euler(15,340,0) * new Vector3(x * 0.4f - 0.2f, y * 0.6f, Curve(y,0.5,0.5) * (Curve(x,0.5,0.5) + 0.5f) * 0.3f))
                ).MarkDestroyOnUnload(),
                GetPrefabByName<Material>("JuniperChili_Tex"),
                Vector3.one * 0.5f,
                Vector3.up * 0.25f,
                ItemManager.GetItemByName("Chili"),
                GetPrefabByName<Plant>("Plant_Beet"),
                RandomDropCount: new Interval_Int() { maxValue = 1, minValue = 3}
            )
            };

            //if (!tangLoaded)
            //    SceneManager.UnloadSceneAsync(tangInd);

            Resources.UnloadUnusedAssets();
        }

        public static T GetPrefabByName<T>(string name) where T : Object
        {
            foreach (var item in Resources.FindObjectsOfTypeAll<T>())
                if (item.name == name)
                    return item;
            Debug.LogWarning("Failed to find a " + typeof(T).Name + " named " + name);
            return null;
        }

        public static bool LoadModel(string brush, int prefabIndex, out Mesh mesh, out Material material)
        {
            var b = Resources.Load<SO_Brush>(brush);
            if (!b)
            {
                mesh = null;
                material = null;
                Debug.LogError("Failed to load brush \"" + brush + "\"");
                return false;
            }
            if (prefabIndex >= b.prefabs.Count)
            {
                mesh = null;
                material = null;
                Debug.LogError("Brush \"" + brush + "\" does not have index " + prefabIndex);
                return false;
            }
            var lod = b.prefabs[prefabIndex].GetComponent<LODGroup>()?.GetLODs()[0].renderers[0];
            var r = lod ? lod : b.prefabs[prefabIndex].GetComponentInChildren<Renderer>(true);
            if (!r)
            {
                mesh = null;
                material = null;
                Debug.LogError("Brush prefab \"" + brush + "\">" + prefabIndex + " does not have a model");
                return false;
            }
            material = r.sharedMaterial;
            mesh = r is SkinnedMeshRenderer skin ? skin.sharedMesh : r.GetComponent<MeshFilter>().sharedMesh;
            return true;
        }

        public static bool EnsureSceneLoaded(string searchString, out bool alreadyLoaded, out int sceneIndex) // Don't use until figure out a good way to load scene as inactive
        {
            var ind = -1;
            for (int i = SceneManager.sceneCountInBuildSettings - 1; i >= 0; i--)
                if (NameFromIndex(i).Contains(searchString))
                {
                    ind = i;
                    break;
                }
            sceneIndex = ind;
            if (ind == -1)
            {
                alreadyLoaded = false;
                Debug.LogError("Could not find \"" + searchString + "\"");
                return false;
            }
            var scene = SceneManager.GetSceneByBuildIndex(ind);
            if (!(alreadyLoaded = scene.isLoaded))
            {
                SceneManager.LoadScene(ind, LoadSceneMode.Additive);
                foreach (var o in scene.GetRootGameObjects())
                    o.SetActive(false); // This does not work
            }
            return true;
        }

        public static string NameFromIndex(int BuildIndex)
        {
            var path = SceneUtility.GetScenePathByBuildIndex(BuildIndex);
            int start = path.LastIndexOf('/') + 1;
            return path.Substring(start, path.LastIndexOf('.') - start);
        }


        public static Mesh CreateMesh(string name, Vector3[] verts, Vector2[] uvs, int[] tris)
        {
            var m = new Mesh()
            {
                name = name,
                vertices = verts,
                uv = uvs,
                triangles = tris
            };
            m.RecalculateBounds();
            m.RecalculateNormals();
            m.RecalculateTangents();
            return m;
        }

        public static Mesh CreateMesh(string name, Vector2 bl, Vector2 br, Vector2 tr, Vector2 tl, params (bool doubleSided, int XDiv, int YDiv, Func<float,float,Vector3> getVert)[] faces)
        {
            var verts = new List<Vector3>();
            var uvs = new List<Vector2>();
            var tris = new List<int>();
            foreach (var f in faces)
            {
                if (f.XDiv < 0 || f.YDiv < 0)
                    continue;
                var width = f.XDiv + 2;
                var height = f.YDiv + 2;
                var stride = f.doubleSided ? 2 : 1;
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                    {
                        var rx = x / (width - 1f);
                        var ry = y / (height - 1f);
                        verts.Add(f.getVert(rx, ry));
                        uvs.Add(Vector2.LerpUnclamped(Vector2.LerpUnclamped(bl, tl, ry), Vector2.LerpUnclamped(br, tr, ry), rx));
                        if (f.doubleSided)
                        {
                            verts.Add(verts[verts.Count - 1]);
                            uvs.Add(uvs[uvs.Count - 1]);
                        }
                        if (x < width - 1 && y < height - 1)
                        {
                            var pos = verts.Count - stride;
                            tris.Add(pos);
                            tris.Add(pos + stride);
                            tris.Add(pos + stride + width * stride);
                            tris.Add(pos);
                            tris.Add(pos + stride + width * stride);
                            tris.Add(pos + width * stride);
                            if (f.doubleSided)
                            {
                                tris.Add(pos + stride + 1);
                                tris.Add(pos + 1);
                                tris.Add(pos + stride + width * stride + 1);
                                tris.Add(pos + stride + width * stride + 1);
                                tris.Add(pos + 1);
                                tris.Add(pos + width * stride + 1);
                            }
                        }
                    }
            }
            var m = new Mesh()
            {
                name = name,
                vertices = verts.ToArray(),
                uv = uvs.ToArray(),
                triangles = tris.ToArray()
            };
            m.RecalculateBounds();
            m.RecalculateNormals();
            m.RecalculateTangents();
            return m;
        }

        public static float Curve(float value, double radius, double offset) => (float)Math.Sqrt(radius * radius - Math.Abs(offset - value) * Math.Abs(offset - value));
    }

    class ModLoadException : Exception
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
            prefab.transform.SetParent(Main.prefabParent);
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
            foreach (var crop in Main.crops)
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
            foreach (var crop in Main.crops)
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
            foreach (var crop in Main.crops)
            {
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
            foreach (var crop in Main.crops)
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

    public static class ExtensionMethods
    {
        public static T MarkDestroyOnUnload<T>(this T obj) where T : Object
        {
            Main.created.Add(obj);
            return obj;
        }
        public static T Clone<T>(this T obj, string name = null) where T : Object
        {
            var n = Object.Instantiate(obj);
            if (name != null)
                n.name = name;
            else
                n.name = obj.name;
            return n;
        }
        public static Material WithTexture(this Material mat,string property,Texture value)
        {
            mat.SetTexture(property, value);
            return mat;
        }
        public static Material WithTextures(this Material mat,Material source,params string[] textures)
        {
            foreach (var t in textures)
                mat.SetTexture(t, source.GetTexture(t));
            return mat;
        }
    }
}