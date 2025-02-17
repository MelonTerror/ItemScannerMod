using System.Collections;
using Harmony;
using MelonLoader;
using MelonLoader.Utils;
using TeamMonumental;
using Unity.Netcode;
using UnityEngine;

[assembly: MelonInfo(typeof(ItemScannerMod.Core), "ItemScannerMod", "1.0.0", "NightPotato & Caznix", null)]
[assembly: MelonGame("TeamMonumental", "Subterror")]
namespace ItemScannerMod;

// TODO: Add ScannerTool to the game and spawn a single instance within the submarine.
// TODO: Make ScannerTool purchasable within the Shop
// TODO: Make AdvancedScannerTool with 2x Radius of NormalScannerTool.
// TODO: Make AdvancedScannerTool purchasable within the Shop
// TODO: Implement Highlight Material
// TODO: Show cooldown on Player HUD when tool is being held.

// TODO: Sync Configuration with Host Settings to avoid cheating from otehr players. (Most Likely TerrorAPI Implementation)
// TODO: Make Keycode Bindable within Game.


public class Core : MelonMod
{

    private MelonPreferences_Category itemScannerMod;
    private MelonPreferences_Entry<int> checkDistance;
    private MelonPreferences_Entry<int> checkCooldown;

    private AssetBundle itemScannerMod_Assets;

    private static UnityEngine.KeyCode ScanItemsKey;
    private int currentCooldown = 0;
    private string[] blacklistedItems = ["Morse Clipboard"];
    public float timeBeforeNormalized = 5f; // Time to wait in seconds

    public override void OnInitializeMelon()
    {
        LoggerInstance.Msg("Initialized.");

        // Mod Configuration File
        LoggerInstance.Msg("Setting up Configuration for Item-Scanner.");
        itemScannerMod = MelonPreferences.CreateCategory("ItemScanner");
        itemScannerMod.SetFilePath("UserData/ItemScanner.cfg");
        checkDistance = itemScannerMod.CreateEntry<int>("Scan Radius", 15);
        checkCooldown = itemScannerMod.CreateEntry<int>("Scan Cooldown", 30);
        itemScannerMod.SaveToFile();
        LoggerInstance.Msg("Created Configruation File for Item-Scanner.");

        MelonLogger.Msg("Attempting to load required assets.");
        LoadResourceBundle();
        

    }

    private void LoadResourceBundle()
    {
        string bundlePath = MelonEnvironment.ModsDirectory + "\\ItemScannerMod\\itemscannermod";
        MelonLogger.Msg(bundlePath);
        if (!File.Exists(bundlePath))
        {
            MelonLogger.Msg("AssetBundle not found: " + bundlePath);
            return;
        }

        itemScannerMod_Assets = AssetBundle.LoadFromFile(bundlePath);

        if (itemScannerMod_Assets == null)
        {
            MelonLogger.Error("Failed to load AssetBundle!");
            return;
        }

        MelonLogger.Msg("Finished loading required assets.");
    }

    public override void OnEarlyInitializeMelon()
    {
        ScanItemsKey = UnityEngine.KeyCode.U;
    }

    public override void OnLateUpdate()
    {
        if (Input.GetKeyDown(ScanItemsKey))
        {
            if (currentCooldown >= MelonPreferences.GetEntryValue<int>("ItemScanner", "Scan Cooldown"))
            {
                MelonLogger.Msg("You are currently on a Cooldown.");
                ToastManager.Instance.PopMainToast("You are currently on a cooldown!", Color.white, Color.red, 5f);
                return;
            }
         
            ScanForItems();
        }
    }

    private void ScanForItems()
    {
        // GetTheCurrentPlayer & Position/Transform
        GameObject player = GameObject.Find("Pre_Player(Clone)");
        Soul playerSoul = player.GetComponent<Player>().GetSoul();
        if (playerSoul.IsLocalPlayer)
        {
            Collider[] hitColliders = Physics.OverlapSphere(playerSoul.transform.position, MelonPreferences.GetEntryValue<int>("ItemScanner", "Scan Radius"));
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.GetComponent<Item>() != null && hitCollider.GetComponent<Caloric>() != null)
                {
                    MelonLogger.Msg(hitCollider.gameObject.name);
                    Item _item = hitCollider.gameObject.GetComponent<Item>();
                    if (!blacklistedItems.Contains(_item.Name))
                    {
                        MeshRenderer _itemMesh = _item.Model.GetComponentInChildren<MeshRenderer>();

                        _itemMesh.material.color = Color.green;
                        Material _outlineMat = itemScannerMod_Assets.LoadAsset<Material>("OutlineMat");

                        if (_outlineMat == null) { MelonLogger.Error("Failed to load material!"); return; }
                        MelonLogger.Msg("Material Loaded: " + _outlineMat.name);
                        MaterialUtils.AddMaterialToGameObject(_itemMesh, _outlineMat);

                        MelonCoroutines.Start(NormalizedItemMaterials(timeBeforeNormalized, _itemMesh, "OutlineMat"));
                    }

                    currentCooldown = MelonPreferences.GetEntryValue<int>("ItemScanner", "Scan Cooldown");
                    MelonCoroutines.Start(HandleCoolDown(currentCooldown));
                    MelonLogger.Msg("Scan Cooldown Started!");
                }
            }
        }
    }


    private IEnumerator NormalizedItemMaterials(float timeToWait, MeshRenderer _itemMesh, string _matName)
    {
        yield return new WaitForSeconds(timeToWait);

        // Logic to execute after the wait
        _itemMesh.material.color = Color.white;
        MaterialUtils.RemoveCustomMaterial(_itemMesh, _matName);

        MelonLogger.Msg("Normalized Item Materials.");
    }

    private IEnumerator HandleCoolDown(float timeToWait)
    {
        yield return new WaitForSeconds(timeToWait);

        // Logic to execute after the wait
        currentCooldown = 0;
        MelonLogger.Msg("Scan Cooldown has been completed.");
    }

}
