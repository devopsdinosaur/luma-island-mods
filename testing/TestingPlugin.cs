using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;
using UnityEngine.InputSystem;
using Unity.Mathematics;

public static class PluginInfo {

    public const string TITLE = "Testing";
    public const string NAME = "testing";
    public const string SHORT_DESCRIPTION = "";

    public const string VERSION = "0.0.1";

    public const string AUTHOR = "devopsdinosaur";
    public const string GAME_TITLE = "Luma Island";
    public const string GAME = "luma-island";
    public const string GUID = AUTHOR + "." + GAME + "." + NAME;
    public const string REPO = "luma-island-mods";

    public static Dictionary<string, string> to_dict() {
        Dictionary<string, string> info = new Dictionary<string, string>();
        foreach (FieldInfo field in typeof(PluginInfo).GetFields((BindingFlags) 0xFFFFFFF)) {
            info[field.Name.ToLower()] = (string) field.GetValue(null);
        }
        return info;
    }
}

[BepInPlugin(PluginInfo.GUID, PluginInfo.TITLE, PluginInfo.VERSION)]
public class TestingPlugin : DDPlugin {
    private Harmony m_harmony = new Harmony(PluginInfo.GUID);

    private void Awake() {
        logger = this.Logger;
        try {
            this.m_plugin_info = PluginInfo.to_dict();
            Settings.Instance.load(this);
            DDPlugin.set_log_level(Settings.m_log_level.Value);
            this.create_nexus_page();
            this.m_harmony.PatchAll();
            logger.LogInfo($"{PluginInfo.GUID} v{PluginInfo.VERSION} loaded.");
        } catch (Exception e) {
            logger.LogError("** Awake FATAL - " + e);
        }
    }

    //TileOccupier, Health, Cuttable

    public class Bulldozer : MonoBehaviour {
        private const float CHECK_FREQUENCY = 0.5f;
        private LocalPlayerController m_player;
        
        private void Awake() {
            this.m_player = this.gameObject.GetComponent<LocalPlayerController>();
            this.StartCoroutine(this.run_routine());
        }

        private void bulldoze() {
            try {
                foreach (GameObject obj in Resources.FindObjectsOfTypeAll<GameObject>()) {
                    //if (obj.transform.parent == null || !obj.name.StartsWith("CommonFarmTree")) {
                    //    continue;
                    //}
                    //UnityUtils.list_ancestors(obj.transform, _debug_log);
                    if (!obj.activeSelf) {
                        continue;
                    }
                    float distance = Vector3.Distance(this.m_player.transform.position, obj.transform.position);
                    if (distance >= 10) {
                        continue;
                    }
                    UnityUtils.list_ancestors(obj.transform, _debug_log);
                    _debug_log($"[{Time.time}] distance: {distance}");
                }
                /*
                int aura_radius = 10;
                Level level = this.m_player.Level;
                if (level == null) {
                    return;
                }
                TileOccupier[,] occupiers = (TileOccupier[,]) ReflectionUtils.get_field_value(level, "m_gameObjects");
                int2 player_tile = this.transform.position.ToTile(level);
                foreach (int2 check_pos in new RectangleFillIterator(new int2(player_tile.x - aura_radius, player_tile.y - aura_radius), new int2(player_tile.x + aura_radius, player_tile.y + aura_radius))) {
                    TileOccupier occupier = occupiers[check_pos.x, check_pos.y];
                    Health health = null;
                    Cuttable cuttable = null;
                    if (occupier == null || (health = occupier.gameObject.GetComponent<Health>()) == null || (cuttable = occupier.gameObject.GetComponent<Cuttable>()) == null) {
                        continue;
                    }
                    _debug_log(occupier);
                    foreach (Component component in occupier.gameObject.GetComponents<Component>()) {
                        _debug_log($"--> {component.GetType()}");
                    }
                    _debug_log($"healthData: {(HealthData) ReflectionUtils.get_field_value(health, "m_healthData")}");
                    occupier.transform.position += occupier.transform.up * 2;
                    //health.ChangeHealth(-float.MaxValue, this.m_player);
                    //cuttable.OnKilled(this.m_player);
                }
                */
            } catch (Exception e) {
                _error_log("** Bulldozer.bulldoze ERROR - " + e);
            }
        }

        private IEnumerator run_routine() {
            for (;;) {
                //this.bulldoze();
                yield return new WaitForSeconds(CHECK_FREQUENCY);
            }
        }
    }

    [HarmonyPatch(typeof(LocalPlayerController), "Initialize")]
    class HarmonyPatch_LocalPlayerController_Initialize {
        private static void Postfix(LevelManager __instance) {
            __instance.gameObject.AddComponent<Bulldozer>();
        }
    }
    
    /*
    private static bool m_added_actions = false;
    private static bool m_trying_to_jump = false;
    private static Rigidbody m_rigid_body = null;

    [HarmonyPatch(typeof(LocalPawn), "Awake")]
	class HarmonyPatch_LocalPawn_Awake {
		private static bool Prefix(LocalPawn __instance) {
			try {
				m_rigid_body = __instance.gameObject.GetComponent<Rigidbody>();
                _debug_log(__instance.name);
				return true;
			} catch (Exception e) {
				logger.LogError("** HarmonyPatch_LocalPawn_Awake.Prefix ERROR - " + e);
			}
			return true;
		}
	}

    [HarmonyPatch(typeof(LocalPawn), "Update")]
    class HarmonyPatch_LocalPawn_Update {
        private static bool Prefix(LocalPawn __instance) {
            try {
                if (m_trying_to_jump) {
                    m_trying_to_jump = false;
                    m_rigid_body.isKinematic = false;
                    m_rigid_body.useGravity = true;
                    m_rigid_body.mass = 1;
                    _debug_log($"jumping?  mass: {m_rigid_body.mass}, isKinematic: {m_rigid_body.isKinematic}, useGravity: {m_rigid_body.useGravity}");
                    m_rigid_body.AddForce(m_rigid_body.transform.up * 10, ForceMode.Impulse);
                }
                return true;
            } catch (Exception e) {
                logger.LogError("** HarmonyPatch_LocalPawn_Update.Prefix ERROR - " + e);
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(LocalPlayerController), "Initialize")]
    class HarmonyPatch_LocalPlayerController_Initialize {
        private static void Postfix(LocalPlayerController __instance, PlayerInput ___m_input) {
            try {
                if (m_added_actions) {
                    return;
                }
                m_added_actions = true;
                UnityUtils.dump_control_paths();
                UnityUtils.add_hotkey_action(___m_input, "jump", "", "/Keyboard/space", performed: context => {
                    m_trying_to_jump = true;
                    _debug_log($"JUMP button pressed");
                });
            } catch (Exception e) {
                logger.LogError("** HarmonyPatch_LocalPlayerController_Initialize.Prefix ERROR - " + e);
            }
        }
    }
    */

    /*
	[HarmonyPatch(typeof(), "")]
	class HarmonyPatch_ {
		private static bool Prefix() {
			
			return true;
		}
	}

	[HarmonyPatch(typeof(), "")]
	class HarmonyPatch_ {
		private static void Postfix() {
			
		}
	}

	[HarmonyPatch(typeof(), "")]
	class HarmonyPatch_ {
		private static bool Prefix() {
			try {

				return true;
			} catch (Exception e) {
				logger.LogError("** XXXXX.Prefix ERROR - " + e);
			}
			return true;
		}
	}

	[HarmonyPatch(typeof(), "")]
	class HarmonyPatch_ {
		private static void Postfix() {
			try {
				
			} catch (Exception e) {
				logger.LogError("** XXXXX.Postfix ERROR - " + e);
			}
		}
	}
	*/
}
