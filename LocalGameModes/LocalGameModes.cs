using BepInEx;
using BepInEx.Configuration;
using ExitGames.Client.Photon;
using LGM.Patches;
using LGM.UI;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using UnboundLib;
using UnboundLib.GameModes;
using UnboundLib.Networking;
using UnboundLib.Utils.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace LGM
{
    public static class NetworkEventType
    {
        public static string ClientConnected = "client_connected";
        public static string SetTeamSize = "set_team_size";
    }

    [Serializable]
    public class DebugOptions
    {
        public static byte[] Serialize(object opts)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, (DebugOptions) opts);
                return ms.ToArray();
            }
        }

        public static DebugOptions Deserialize(byte[] data)
        {
            using (MemoryStream ms = new MemoryStream(data))
            {
                var formatter = new BinaryFormatter();
                return (DebugOptions) formatter.Deserialize(ms);
            }
        }

        public int rounds = 5;
        public int points = 2;
    }

    [BepInDependency("pykess.rounds.plugins.moddingutils", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("pykess.rounds.plugins.cardchoicespawnuniquecardpatch", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("com.willis.rounds.unbound", BepInDependency.DependencyFlags.HardDependency)]
    [BepInPlugin(ModId, ModName, Version)]
    [BepInProcess("Rounds.exe")]
    public class LGMMod : BaseUnityPlugin
    {
        private const string ModName = "LocalGameModes";
        private const string ModId = "io.zeeke.local.LGM";
        public const string Version = "0.1.2";

#if DEBUG
        public static readonly bool DEBUG = true;
#else
        public static readonly bool DEBUG = false;
#endif

        public static LGMMod instance;
        public static CardRemover remover;

        /// 
        /// Competitive Configurations
        /// 

        public static ConfigEntry<bool> WinByTwoRoundsConfig;
        public static ConfigEntry<bool> WinByTwoPointsConfig;
        public static ConfigEntry<int> PickTimerConfig;
        public static ConfigEntry<int> MaxCardsConfig;
        public static ConfigEntry<bool> PassDiscardConfig;
        public static ConfigEntry<bool> DiscardAfterPickConfig;
        public static ConfigEntry<int> PreGamePickCommonConfig;
        public static ConfigEntry<int> PreGamePickUncommonConfig;
        public static ConfigEntry<int> PreGamePickRareConfig;
        public static ConfigEntry<int> PreGamePickStandardConfig;
        public static ConfigEntry<int> PreGameBanConfig;
        public static ConfigEntry<bool> PreGamePickMethodConfig;

        public static bool WinByTwoRounds;
        public static bool WinByTwoPoints;
        public static int PickTimer;
        public static int MaxCards;
        public static bool PassDiscard;
        public static bool DiscardAfterPick;
        public static int PreGamePickCommon;
        public static int PreGamePickUncommon;
        public static int PreGamePickRare;
        public static int PreGamePickStandard;
        public static bool PreGamePickMethod;
        public static int PreGameBan;

        private static Toggle WinByTwoPointsCheckbox;
        private static Toggle WinByTwoRoundsCheckbox;
        private static Toggle PassCheckbox;
        private static Toggle DiscardAfterCheckbox;
        private static Toggle PreGamePickMethodCheckbox;
        private static GameObject StandardSlider;
        private static GameObject CommonSlider;
        private static GameObject UncommonSlider;
        private static GameObject RareSlider;

        /// 
        /// Competitive Configurations
        /// 


        private static bool facesSet = false;

        public static string GetCustomPropertyKey(string prop)
        {
            return $"{ModId}/{prop}";
        }

        public static void DebugLog(object obj)
        {
            if (obj == null)
            {
                obj = "null";
            }
            instance.Logger.LogMessage(obj);
        }

        public static void Log(object obj)
        {
            if (obj == null)
            {
                obj = "null";
            }
            instance.Logger.LogInfo(obj);
        }

        public static bool IsSteamConnected
        {
            get
            {
                try
                {
                    Steamworks.InteropHelp.TestIfAvailableClient();
                    return true;
                }
                catch (Exception e)
                {
                    _ = e;
                    return false;
                }
            }
        }

        public bool IsCeaseFire { get; private set; }

        public Text infoText;
        private Dictionary<string, bool> soundEnabled;
        private Dictionary<string, bool> gmInitialized;

        public DebugOptions debugOptions = new DebugOptions();


        public void Awake()
        {
            LGMMod.instance = this;

            //Awake_ConfigureCompetitive();
            WinByTwoRoundsConfig = Config.Bind("CompetitiveRounds", "WinByTwoRounds", false, "When enabled, if the game is tied at match point, then players must win by two roudns.");
            WinByTwoPointsConfig = Config.Bind("CompetitiveRounds", "WinByTwoPoints", false, "When enabled, if the game is tied at match point, then players must win by two points.");
            PickTimerConfig = Config.Bind("CompetitiveRounds", "PickTimer", 0, "Time limit in seconds for the pick phase, 0 disables the timer");
            MaxCardsConfig = Config.Bind("CompetitiveRounds", "MaxCards", 0, "Maximum number of cards a player can have, 0 disables the limit");
            PassDiscardConfig = Config.Bind("CompetitiveRounds", "Pass Discard", false, "Give players to pass during their discard phase");
            DiscardAfterPickConfig = Config.Bind("CompetitiveRounds", "Discard After Pick", false, "Have players discard only after they have exceeded the max number of cards");
            PreGamePickStandardConfig = Config.Bind("CompetitiveRounds", "Pre-game pick cards", 0, "The number of cards each player will pick before the game from the usual 5-card draw");
            PreGamePickCommonConfig = Config.Bind("CompetitiveRounds", "Pre-game common pick cards", 0, "The number of common cards each player will pick from the entire deck before the game");
            PreGamePickUncommonConfig = Config.Bind("CompetitiveRounds", "Pre-game uncommon pick cards", 0, "The number of uncommon cards each player will pick from the entire deck before the game");
            PreGamePickRareConfig = Config.Bind("CompetitiveRounds", "Pre-game rare pick cards", 0, "The number of rare cards each player will pick from the entire deck before the game");
            PreGamePickMethodConfig = Config.Bind("CompetitiveRounds", "Pre-game pick method", true, "The method used for pre-game pick. If true, use the standard 5-card draw method, if false use the entire deck.");
            PreGameBanConfig = Config.Bind("CompetitiveRounds", "Pre-game baned cards", 0, "The number of cards each player will pick to ban from appearing during the game from the entire deck before the game");

            try
            {
                Patches.PatchUtils.ApplyPatches(ModId);
                this.Logger.LogInfo("initialized");
            }
            catch (Exception e)
            {
                this.Logger.LogError(e.ToString());
            }
        }

        public void Start()
        {
            this.soundEnabled = new Dictionary<string, bool>();
            this.gmInitialized = new Dictionary<string, bool>();

            //Start_ConfigureCompetitive();
            WinByTwoRounds = WinByTwoRoundsConfig.Value;
            WinByTwoPoints = WinByTwoPointsConfig.Value;
            PickTimer = PickTimerConfig.Value;
            MaxCards = MaxCardsConfig.Value;
            PassDiscard = PassDiscardConfig.Value;
            DiscardAfterPick = DiscardAfterPickConfig.Value;
            PreGamePickStandard = PreGamePickStandardConfig.Value;
            PreGamePickCommon = PreGamePickCommonConfig.Value;
            PreGamePickUncommon = PreGamePickUncommonConfig.Value;
            PreGamePickRare = PreGamePickRareConfig.Value;
            PreGamePickMethod = PreGamePickMethodConfig.Value;
            PreGameBan = PreGameBanConfig.Value;

            // add GUI to modoptions menu
            Unbound.RegisterMenu("Competitive Rounds", () => { }, this.CompetitiveRoundsGUI, null, false);

            // add hooks for pre-game picks and bans
            GameModeManager.AddHook(GameModeHooks.HookGameStart, PreGamePickBanHandler.RestoreCardToggles);
            GameModeManager.AddHook(GameModeHooks.HookGameStart, PreGamePickBanHandler.PreGameBan);
            GameModeManager.AddHook(GameModeHooks.HookGameEnd, PreGamePickBanHandler.RestoreCardToggles);
            GameModeManager.AddHook(GameModeHooks.HookGameStart, PreGamePickBanHandler.PreGamePickReset);
            GameModeManager.AddHook(GameModeHooks.HookPickStart, PreGamePickBanHandler.PreGamePicksStandard);
            GameModeManager.AddHook(GameModeHooks.HookPickStart, PreGamePickBanHandler.PreGamePicksCommon);
            GameModeManager.AddHook(GameModeHooks.HookPickStart, PreGamePickBanHandler.PreGamePicksUncommon);
            GameModeManager.AddHook(GameModeHooks.HookPickStart, PreGamePickBanHandler.PreGamePicksRare);

            // add hooks for pick timer
            GameModeManager.AddHook(GameModeHooks.HookPlayerPickStart, TimerHandler.Start);
            GameModeManager.AddHook(GameModeHooks.HookPlayerPickEnd, PickTimerHandler.Cleanup);

            // add hooks for win by 2
            GameModeManager.AddHook(GameModeHooks.HookGameStart, WinByTwo.ResetPoints);
            GameModeManager.AddHook(GameModeHooks.HookRoundEnd, WinByTwo.RoundX2);
            GameModeManager.AddHook(GameModeHooks.HookPointEnd, WinByTwo.PointX2);

            // add hooks for max cards
            GameModeManager.AddHook(GameModeHooks.HookPickStart, (gm) => MaxCardsHandler.DiscardPhase(gm, false));
            GameModeManager.AddHook(GameModeHooks.HookPickEnd, (gm) => MaxCardsHandler.DiscardPhase(gm, true));
            GameModeManager.AddHook(GameModeHooks.HookPickEnd, PickTimerHandler.Cleanup);

            // the last pickstart hook should be the pregamepickfinish
            GameModeManager.AddHook(GameModeHooks.HookPickStart, PreGamePickBanHandler.PreGamePicksFinished);

            // set all playerHasPicked to false
            GameModeManager.AddHook(GameModeHooks.HookGameStart, ResetPlayerHasPicked);
            GameModeManager.AddHook(GameModeHooks.HookPickEnd, ResetPlayerHasPicked);

            // the last pickend hook should set skipFirstPickPhase to false
            GameModeManager.AddHook(GameModeHooks.HookPickEnd, PreGamePickBanHandler.SetSkipFirstPickPhase);

            // handshake to sync settings
            Unbound.RegisterHandshake(LGMMod.ModId, this.OnHandShakeCompleted);

            // close menus and textboxes when at main menu
            On.MainMenuHandler.Awake += (orig, self) =>
            {
                // close text boxes
                if (MaxCardsHandler.textCanvas != null) { MaxCardsHandler.textCanvas.SetActive(false); }
                if (MaxCardsHandler.passCanvas != null) { MaxCardsHandler.passCanvas.SetActive(false); }
                if (PickTimerHandler.timerCanvas != null) { PickTimerHandler.timerCanvas.SetActive(false); }
                if (PreGamePickBanHandler.textCanvas != null) { PreGamePickBanHandler.textCanvas.SetActive(false); }

                // close togglecards menu
                ToggleCardsMenuHandler.Close();

                // restore card toggles
                PreGamePickBanHandler.RestoreCardTogglesAction();

                orig(self);
            };

            //Start_GameModes();
            GameModeManager.AddHandler<GameModes.GM_Deathmatch>("Deathmatch", new GameModes.DeathmatchHandler());
            GameModeManager.AddHandler<GameModes.GM_DoubleUp>("DoubleUp", new GameModes.DoubleUpHandler());

            GameModeManager.OnGameModeChanged += (gm) =>
            {
                this.RedrawCharacterSelections();
                this.RedrawCharacterCreators();

                if (LGMMod.DEBUG && this.gameObject.GetComponent<DebugWindow>().enabled)
                {
                    LGMMod.SetDebugOptions(this.debugOptions);
                }
            };

            GameModeManager.AddHook(GameModeHooks.HookPointStart, gm => this.ToggleCeaseFire(true));
            GameModeManager.AddHook(GameModeHooks.HookBattleStart, gm => this.ToggleCeaseFire(false));
            GameModeManager.AddHook(GameModeHooks.HookInitEnd, this.OnGameModeInitialized);
            GameModeManager.AddHook(GameModeHooks.HookGameStart, this.UnsetFaces);
            GameModeManager.AddHook(GameModeHooks.HookPickStart, this.SetPlayerFaces);

            //Start_Cards();
            remover = gameObject.AddComponent<CardRemover>();

            SceneManager.sceneLoaded += this.OnSceneLoaded;
            this.ExecuteAfterFrames(1, ArtHandler.instance.NextArt);

            Unbound.RegisterHandshake(ModId, () =>
            {
                PhotonNetwork.LocalPlayer.SetModded();
            });

            this.gameObject.AddComponent<RoundEndHandler>();

            if (LGMMod.DEBUG)
            {
                var debugWindow = this.gameObject.AddComponent<DebugWindow>();
                debugWindow.enabled = false;

                var sim = this.gameObject.AddComponent<PhotonLagSimulationGui>();
                sim.enabled = false;

                PhotonPeer.RegisterType(typeof(DebugOptions), 77, DebugOptions.Serialize, DebugOptions.Deserialize);
            }
        }

        public void Update()
        {
            if (LGMMod.DEBUG && Input.GetKeyDown(KeyCode.F8))
            {
                var debugWindow = this.gameObject.GetComponent<DebugWindow>();
                debugWindow.enabled = !debugWindow.enabled;
            }
        }

        internal void SyncDebugOptions()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                NetworkingManager.RPC(typeof(LGMMod), nameof(LGMMod.SetDebugOptions), this.debugOptions);
            }
        }

        [UnboundRPC]
        public static void SetDebugOptions(DebugOptions opts)
        {
            LGMMod.instance.debugOptions = opts;
            GameModeManager.CurrentHandler?.ChangeSetting("roundsToWinGame", opts.rounds);
            GameModeManager.CurrentHandler?.ChangeSetting("pointsToWinRound", opts.points);
        }

        [UnboundRPC]
        private static void SyncSettings(bool win2rounds, bool win2points, int pickTimer, int maxCards, bool pass, bool after, bool pickMethod, int pick, int common, int uncommon, int rare, int ban)
        {
            LGMMod.WinByTwoRounds = win2rounds;
            LGMMod.WinByTwoPoints = win2points;
            LGMMod.PickTimer = pickTimer;
            LGMMod.MaxCards = maxCards;
            LGMMod.PassDiscard = pass;
            LGMMod.DiscardAfterPick = after;
            LGMMod.PreGamePickMethod = pickMethod;
            LGMMod.PreGamePickStandard = pick;
            LGMMod.PreGamePickCommon = common;
            LGMMod.PreGamePickUncommon = uncommon;
            LGMMod.PreGamePickRare = rare;
            LGMMod.PreGameBan = ban;
        }

        private IEnumerator UnsetFaces(IGameModeHandler gm)
        {
            LGMMod.facesSet = false;
            yield break;
        }
        private IEnumerator SetPlayerFaces(IGameModeHandler gm)
        {
            if (LGMMod.facesSet || PhotonNetwork.OfflineMode)
            {
                yield break;
            }
            foreach (Player player in PlayerManager.instance.players)
            {
                if (player.data.view.IsMine)
                {
                    PlayerFace playerFace = CharacterCreatorHandler.instance.selectedPlayerFaces[0];
                    player.data.view.RPC("RPCA_SetFace", RpcTarget.All, new object[]
                    {
                        playerFace.eyeID,
                        playerFace.eyeOffset,
                        playerFace.mouthID,
                        playerFace.mouthOffset,
                        playerFace.detailID,
                        playerFace.detailOffset,
                        playerFace.detail2ID,
                        playerFace.detail2Offset
                    });
                }
            }
            LGMMod.facesSet = true;
            yield break;
        }

        private IEnumerator OnGameModeInitialized(IGameModeHandler gm)
        {
            if (!this.gmInitialized.ContainsKey(gm.Name))
            {
                this.gmInitialized.Add(gm.Name, true);
            }
            else
            {
                this.gmInitialized[gm.Name] = true;
            }

            yield break;
        }

        private void OnHandShakeCompleted()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                NetworkingManager.RPC_Others(typeof(LGMMod), nameof(SyncSettings), new object[] { LGMMod.WinByTwoRounds, LGMMod.WinByTwoPoints, LGMMod.PickTimer, LGMMod.MaxCards, PassDiscard, DiscardAfterPick, PreGamePickMethod, PreGamePickStandard, PreGamePickCommon, PreGamePickUncommon, PreGamePickRare, PreGameBan });
            }
        }

        private static IEnumerator ResetPlayerHasPicked(IGameModeHandler _)
        {
            foreach (Player player in PlayerManager.instance.players)
            {
                CardChoicePatchDoPick.playerHasPicked[player] = false;
                CardChoiceVisuals_Patch_Show.playerHasPicked[player] = false;
            }

            yield break;
        }

        public bool IsGameModeInitialized(string handler)
        {
            return this.gmInitialized.ContainsKey(handler) && this.gmInitialized[handler];
        }

        private IEnumerator ToggleCeaseFire(bool isCeaseFire)
        {
            this.IsCeaseFire = isCeaseFire;
            yield break;
        }

        public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "Main")
            {
                this.gmInitialized.Clear();

                this.ExecuteAfterFrames(1, () =>
                {
                    ArtHandler.instance.NextArt();
                });
            }
        }

        public void RedrawCharacterSelections()
        {
            var uiGo = GameObject.Find("/Game/UI").gameObject;
            var mainMenuGo = uiGo.transform.Find("UI_MainMenu").Find("Canvas").gameObject;
            var charSelectionGroupGo = mainMenuGo.transform.Find("ListSelector").Find("CharacterSelect").GetChild(0).gameObject;

            var currentGameMode = GameModes.Types.GetCurrentGameMode(GameModeManager.CurrentHandlerID);
            for (int i = 0; i < charSelectionGroupGo.transform.childCount; i++)
            {
                var charSelGo = charSelectionGroupGo.transform.GetChild(i).gameObject;
                var faceGo = charSelGo.transform.GetChild(0).gameObject;
                var joinGo = charSelGo.transform.GetChild(1).gameObject;
                var readyGo = charSelGo.transform.GetChild(2).gameObject;

                var textColor = PlayerSkinBank.GetPlayerSkinColors(i % currentGameMode.MaxTeams).winText;
                var faceColor = PlayerSkinBank.GetPlayerSkinColors(i % currentGameMode.MaxTeams).color;

                joinGo.GetComponentInChildren<GeneralParticleSystem>(true).particleSettings.color = textColor;
                readyGo.GetComponentInChildren<GeneralParticleSystem>(true).particleSettings.color = textColor;

                foreach (Transform faceSelector in faceGo.transform.GetChild(0))
                {
                    faceSelector.Find("PlayerScaler_Small").Find("Face").GetComponent<SpriteRenderer>().color = faceColor;
                }
            }
        }

        public void RedrawCharacterCreators()
        {
            var charGo = GameObject.Find("/CharacterCustom");

            var currentGameMode = GameModes.Types.GetCurrentGameMode(GameModeManager.CurrentHandlerID);
            for (int i = 1; i < charGo.transform.childCount; i++)
            {
                var creatorGo = charGo.transform.GetChild(i);
                int playerID = i - 1;
                int teamID = playerID % currentGameMode.MaxTeams;
                var faceColor = PlayerSkinBank.GetPlayerSkinColors(teamID).color;

                var buttonSource = creatorGo.transform.Find("Canvas").Find("Items").GetChild(0);
                buttonSource.Find("Face").gameObject.GetComponent<Image>().color = faceColor;

                foreach (Transform scaler in creatorGo.transform.Find("Faces"))
                {
                    scaler.Find("Face").GetComponent<SpriteRenderer>().color = faceColor;
                }
            }
        }

        public void SetSoundEnabled(string key, bool enabled)
        {
            if (!this.soundEnabled.ContainsKey(key))
            {
                this.soundEnabled.Add(key, enabled);
            }
            else
            {
                this.soundEnabled[key] = enabled;
            }
        }

        public bool GetSoundEnabled(string key)
        {
            return this.soundEnabled.ContainsKey(key) ? this.soundEnabled[key] : true;
        }

        public void SetupGameModes()
        {
            var gameModeGo = GameObject.Find("/Game/UI/UI_MainMenu/Canvas/ListSelector/GameMode");
            var versusGo = gameModeGo.transform.Find("Group").Find("Versus").gameObject;
            var characterSelectGo = GameObject.Find("/Game/UI/UI_MainMenu/Canvas/ListSelector/CharacterSelect");

            var versusText = versusGo.GetComponentInChildren<TextMeshProUGUI>();
            versusText.text = "SWAT";

            var characterSelectPage = characterSelectGo.GetComponent<ListMenuPage>();

            var gameTypes = new Dictionary<string, string>();
            gameTypes.Add("Deathmatch", "FREE FOR ALL");
            gameTypes.Add("DoubleUp", "DOUBLE UP");

            foreach(var type in gameTypes)
            {
                var deathmatchButtonGo = GameObject.Instantiate(versusGo, versusGo.transform.parent);
                deathmatchButtonGo.transform.localScale = Vector3.one;
                deathmatchButtonGo.transform.SetSiblingIndex(1);

                var deathmatchButtonText = deathmatchButtonGo.GetComponentInChildren<TextMeshProUGUI>();
                deathmatchButtonText.text = type.Value;

                GameObject.DestroyImmediate(deathmatchButtonGo.GetComponent<Button>());
                var deathmatchButton = deathmatchButtonGo.AddComponent<Button>();

                deathmatchButton.onClick.AddListener(characterSelectPage.Open);
                deathmatchButton.onClick.AddListener(() => GameModeManager.SetGameMode(type.Key));
            }
        }

        public void InjectUIElements()
        {
            var uiGo = GameObject.Find("/Game/UI");
            var charGo = GameObject.Find("/CharacterCustom");
            var gameGo = uiGo.transform.Find("UI_Game").Find("Canvas").gameObject;
            var mainMenuGo = uiGo.transform.Find("UI_MainMenu").Find("Canvas").gameObject;
            var charSelectionGroupGo = mainMenuGo.transform.Find("ListSelector").Find("CharacterSelect").GetChild(0).gameObject;

            if (!charSelectionGroupGo.transform.Find("CharacterSelect 3"))
            {
                var charSelectInstanceGo1 = charSelectionGroupGo.transform.GetChild(0).gameObject;
                var charSelectInstanceGo2 = charSelectionGroupGo.transform.GetChild(1).gameObject;

                var charSelectInstanceGo3 = GameObject.Instantiate(charSelectInstanceGo1, charSelectionGroupGo.transform);
                charSelectInstanceGo3.name = "CharacterSelect 3";
                charSelectInstanceGo3.transform.localScale = Vector3.one;

                charSelectInstanceGo3.transform.position = charSelectInstanceGo1.transform.position - new Vector3(0, 6, 0);
                charSelectInstanceGo1.transform.position += new Vector3(0, 6, 0);

                foreach (var portrait in charSelectInstanceGo3.transform.GetChild(0).GetChild(0).GetComponentsInChildren<CharacterCreatorPortrait>())
                {
                    portrait.playerId = 2;
                }

                var charSelectInstanceGo4 = GameObject.Instantiate(charSelectInstanceGo2, charSelectionGroupGo.transform);
                charSelectInstanceGo4.name = "CharacterSelect 4";
                charSelectInstanceGo4.transform.localScale = Vector3.one;

                charSelectInstanceGo4.transform.position = charSelectInstanceGo2.transform.position - new Vector3(0, 6, 0);
                charSelectInstanceGo2.transform.position += new Vector3(0, 6, 0);

                foreach (var portrait in charSelectInstanceGo4.transform.GetChild(0).GetChild(0).GetComponentsInChildren<CharacterCreatorPortrait>())
                {
                    portrait.playerId = 3;
                }

                charSelectionGroupGo.GetComponent<GoBack>().goBackEvent.AddListener(charSelectInstanceGo3.GetComponent<CharacterSelectionInstance>().ResetMenu);
                charSelectionGroupGo.GetComponent<GoBack>().goBackEvent.AddListener(charSelectInstanceGo4.GetComponent<CharacterSelectionInstance>().ResetMenu);
            }

            if (!gameGo.transform.Find("PrivateRoom"))
            {
                var privateRoomGo = new GameObject("PrivateRoom");
                privateRoomGo.transform.SetParent(gameGo.transform);
                privateRoomGo.transform.localScale = Vector3.one;

                privateRoomGo.AddComponent<PrivateRoomHandler>();

                var inviteFriendGo = mainMenuGo.transform.Find("ListSelector").Find("Online").Find("Group").Find("Invite friend").gameObject;
                GameObject.DestroyImmediate(inviteFriendGo.GetComponent<Button>());
                var button = inviteFriendGo.AddComponent<Button>();

                button.onClick.AddListener(() =>
                {
                    PrivateRoomHandler.instance.Open();
                    NetworkConnectionHandler.instance.HostPrivate();
                });
            }

            if (!charGo.transform.Find("Creator_Local3"))
            {
                var creatorGo1 = charGo.transform.GetChild(1).gameObject;
                var creatorGo2 = charGo.transform.GetChild(2).gameObject;

                creatorGo1.transform.localPosition = new Vector3(-15, 8, 0);

                // Looks nicer when the right-side CharacterCreator is a bit further to the right
                creatorGo2.transform.localPosition = new Vector3(18, 8, 0);

                var creatorGo3 = GameObject.Instantiate(creatorGo1, charGo.transform);
                creatorGo3.name = "Creator_Local3";
                creatorGo3.transform.localScale = Vector3.one;
                creatorGo3.GetComponent<CharacterCreator>().playerID = 2;

                var creatorGo4 = GameObject.Instantiate(creatorGo2, charGo.transform);
                creatorGo4.name = "Creator_Local4";
                creatorGo4.transform.localScale = Vector3.one;
                creatorGo4.GetComponent<CharacterCreator>().playerID = 3;
            }

            if (!gameGo.transform.Find("RoundStartText"))
            {
                var newPos = gameGo.transform.position + new Vector3(0, 2, 0);
                var baseGo = GameObject.Instantiate(gameGo.transform.Find("PopUpHandler").Find("Yes").gameObject, newPos, Quaternion.identity, gameGo.transform);
                baseGo.name = "RoundStartText";
                baseGo.AddComponent<UI.ScalePulse>();
                baseGo.GetComponent<TextMeshProUGUI>().fontSize = 140f;
                baseGo.GetComponent<TextMeshProUGUI>().fontStyle = FontStyles.Bold;
            }

            if (!gameGo.transform.Find("PopUpMenu"))
            {
                var popupGo = new GameObject("PopUpMenu");
                popupGo.transform.SetParent(gameGo.transform);
                popupGo.transform.localScale = Vector3.one;
                popupGo.AddComponent<UI.PopUpMenu>();
            }
        }

        private void CompetitiveRoundsGUI(GameObject menu)
        {
            Slider maxSlider = null;

            MenuHandler.CreateText("Competitive Rounds Options", menu, out TextMeshProUGUI _, 45);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI _, 15);
            void TimerChanged(float val)
            {
                LGMMod.PickTimerConfig.Value = UnityEngine.Mathf.RoundToInt(val);
                LGMMod.PickTimer = LGMMod.PickTimerConfig.Value;
            }
            MenuHandler.CreateSlider("Pick Phase Timer (seconds)\n0 disables", menu, 30, 0f, 100f, LGMMod.PickTimerConfig.Value, TimerChanged, out UnityEngine.UI.Slider timerSlider, true);
            MenuHandler.CreateText(" ", menu, out TextMeshProUGUI maxCardsWarning, 15);
            void MaxChanged(float val)
            {

                if (LGMMod.PreGamePickMethod && val > 0f && LGMMod.PreGamePickStandard > val)
                {
                    maxCardsWarning.text = "MAX CARDS MUST BE GREATER THAN OR EQUAL TO THE TOTAL NUMBER OF PRE-GAME PICKS";
                    maxCardsWarning.color = Color.red;

                    if (maxSlider != null) { Unbound.Instance.ExecuteAfterSeconds(0.1f, () => { maxSlider.value = (float) LGMMod.PreGamePickStandard; }); }
                }
                else if (!LGMMod.PreGamePickMethod && val > 0f && LGMMod.PreGamePickCommon + LGMMod.PreGamePickUncommon + LGMMod.PreGamePickRare > val)
                {
                    maxCardsWarning.text = "MAX CARDS MUST BE GREATER THAN OR EQUAL TO THE TOTAL NUMBER OF PRE-GAME PICKS";
                    maxCardsWarning.color = Color.red;

                    if (maxSlider != null) { Unbound.Instance.ExecuteAfterSeconds(0.1f, () => { maxSlider.value = (float) (LGMMod.PreGamePickCommon + LGMMod.PreGamePickUncommon + LGMMod.PreGamePickRare); }); }
                }
                else
                {
                    LGMMod.MaxCardsConfig.Value = UnityEngine.Mathf.RoundToInt(val);
                    LGMMod.MaxCards = LGMMod.MaxCardsConfig.Value;
                }
                if (val == 0f)
                {
                    maxCardsWarning.text = " ";
                }
            }
            MenuHandler.CreateSlider("Maximum Number of Cards\n0 disables", menu, 30, 0f, 50f, LGMMod.MaxCardsConfig.Value, MaxChanged, out maxSlider, true);
            void PassCheckboxAction(bool flag)
            {
                LGMMod.PassDiscardConfig.Value = flag;
                if (LGMMod.PassDiscardConfig.Value && LGMMod.DiscardAfterPickConfig.Value)
                {
                    LGMMod.DiscardAfterPickConfig.Value = false;
                    DiscardAfterCheckbox.isOn = false;
                }
                LGMMod.PassDiscard = LGMMod.PassDiscardConfig.Value;
                LGMMod.DiscardAfterPick = LGMMod.DiscardAfterPickConfig.Value;
            }
            void DiscardAfterCheckboxAction(bool flag)
            {
                LGMMod.DiscardAfterPickConfig.Value = flag;
                if (LGMMod.PassDiscardConfig.Value && LGMMod.DiscardAfterPickConfig.Value)
                {
                    LGMMod.PassDiscardConfig.Value = false;
                    PassCheckbox.isOn = false;
                }
                LGMMod.PassDiscard = LGMMod.PassDiscardConfig.Value;
                LGMMod.DiscardAfterPick = LGMMod.DiscardAfterPickConfig.Value;
            }
            PassCheckbox = MenuHandler.CreateToggle(LGMMod.PassDiscardConfig.Value, "Allow players to pass during discard phase", menu, PassCheckboxAction, 30).GetComponent<Toggle>();
            DiscardAfterCheckbox = MenuHandler.CreateToggle(LGMMod.DiscardAfterPickConfig.Value, "Discard phase after pick phase", menu, DiscardAfterCheckboxAction, 30).GetComponent<Toggle>();

            MenuHandler.CreateText(" ", menu, out var _, 15);

            void PickMethodCheckboxAction(bool flag)
            {
                LGMMod.PreGamePickMethod = flag;
                LGMMod.PreGamePickMethodConfig.Value = flag;
                if (LGMMod.PreGamePickMethod)
                {
                    StandardSlider.SetActive(true);
                    CommonSlider.SetActive(false);
                    UncommonSlider.SetActive(false);
                    RareSlider.SetActive(false);
                }
                else
                {
                    StandardSlider.SetActive(false);
                    CommonSlider.SetActive(true);
                    UncommonSlider.SetActive(true);
                    RareSlider.SetActive(true);
                }
            }
            PreGamePickMethodCheckbox = MenuHandler.CreateToggle(LGMMod.PreGamePickMethodConfig.Value, "Use standard 5 card draw for pre-game pick phase", menu, PickMethodCheckboxAction, 30).GetComponent<Toggle>();

            UnityEngine.UI.Slider standard = null;
            UnityEngine.UI.Slider common = null;
            UnityEngine.UI.Slider uncommon = null;
            UnityEngine.UI.Slider rare = null;
            void StandardChanged(float val)
            {
                LGMMod.PreGamePickStandardConfig.Value = UnityEngine.Mathf.RoundToInt(val);
                LGMMod.PreGamePickStandard = LGMMod.PreGamePickStandardConfig.Value;

                if (LGMMod.PreGamePickMethod && LGMMod.MaxCards > 0 && LGMMod.PreGamePickStandard > LGMMod.MaxCards)
                {
                    maxCardsWarning.text = "MAX CARDS MUST BE GREATER THAN OR EQUAL TO THE TOTAL NUMBER OF PRE-GAME PICKS";
                    maxCardsWarning.color = Color.red;

                    maxSlider.value = (float) LGMMod.PreGamePickStandard;
                    MaxChanged((float) LGMMod.PreGamePickStandard);
                }
                else
                {
                    //maxCardsWarning.text = " ";
                }
            }
            void CommonChanged(float val)
            {
                LGMMod.PreGamePickCommonConfig.Value = UnityEngine.Mathf.RoundToInt(val);
                LGMMod.PreGamePickCommon = LGMMod.PreGamePickCommonConfig.Value;

                if (!LGMMod.PreGamePickMethod && LGMMod.MaxCards > 0 && LGMMod.PreGamePickCommon + LGMMod.PreGamePickUncommon + LGMMod.PreGamePickRare > LGMMod.MaxCards)
                {
                    maxCardsWarning.text = "MAX CARDS MUST BE GREATER THAN OR EQUAL TO THE TOTAL NUMBER OF PRE-GAME PICKS";
                    maxCardsWarning.color = Color.red;

                    maxSlider.value = (float) (LGMMod.PreGamePickCommon + LGMMod.PreGamePickUncommon + LGMMod.PreGamePickRare);
                    MaxChanged((float) (LGMMod.PreGamePickCommon + LGMMod.PreGamePickUncommon + LGMMod.PreGamePickRare));
                }
                else
                {
                    //maxCardsWarning.text = " ";
                }
            }
            void UncommonChanged(float val)
            {
                LGMMod.PreGamePickUncommonConfig.Value = UnityEngine.Mathf.RoundToInt(val);
                LGMMod.PreGamePickUncommon = LGMMod.PreGamePickUncommonConfig.Value;
                if (!LGMMod.PreGamePickMethod && LGMMod.MaxCards > 0 && LGMMod.PreGamePickCommon + LGMMod.PreGamePickUncommon + LGMMod.PreGamePickRare > LGMMod.MaxCards)
                {
                    maxCardsWarning.text = "MAX CARDS MUST BE GREATER THAN OR EQUAL TO THE TOTAL NUMBER OF PRE-GAME PICKS";
                    maxCardsWarning.color = Color.red;

                    maxSlider.value = (float) (LGMMod.PreGamePickCommon + LGMMod.PreGamePickUncommon + LGMMod.PreGamePickRare);
                    MaxChanged((float) (LGMMod.PreGamePickCommon + LGMMod.PreGamePickUncommon + LGMMod.PreGamePickRare));
                }
                else
                {
                    //maxCardsWarning.text = " ";
                }
            }
            void RareChanged(float val)
            {
                LGMMod.PreGamePickRareConfig.Value = UnityEngine.Mathf.RoundToInt(val);
                LGMMod.PreGamePickRare = LGMMod.PreGamePickRareConfig.Value;
                if (!LGMMod.PreGamePickMethod && LGMMod.MaxCards > 0 && LGMMod.PreGamePickCommon + LGMMod.PreGamePickUncommon + LGMMod.PreGamePickRare > LGMMod.MaxCards)
                {
                    maxCardsWarning.text = "MAX CARDS MUST BE GREATER THAN OR EQUAL TO THE TOTAL NUMBER OF PRE-GAME PICKS";
                    maxCardsWarning.color = Color.red;

                    maxSlider.value = (float) (LGMMod.PreGamePickCommon + LGMMod.PreGamePickUncommon + LGMMod.PreGamePickRare);
                    MaxChanged((float) (LGMMod.PreGamePickCommon + LGMMod.PreGamePickUncommon + LGMMod.PreGamePickRare));
                }
                else
                {
                    //maxCardsWarning.text = " ";
                }
            }

            StandardSlider = MenuHandler.CreateSlider("Pre-game picks", menu, 30, 0f, 10f, LGMMod.PreGamePickStandardConfig.Value, StandardChanged, out standard, true);
            StandardSlider.SetActive(false);

            CommonSlider = MenuHandler.CreateSlider("Pre-game common picks", menu, 30, 0f, 10f, LGMMod.PreGamePickCommonConfig.Value, CommonChanged, out common, true);
            CommonSlider.SetActive(false);

            UncommonSlider = MenuHandler.CreateSlider("Pre-game uncommon picks", menu, 30, 0f, 10f, LGMMod.PreGamePickUncommonConfig.Value, UncommonChanged, out uncommon, true);
            UncommonSlider.SetActive(false);

            RareSlider = MenuHandler.CreateSlider("Pre-game rare picks", menu, 30, 0f, 10f, LGMMod.PreGamePickRareConfig.Value, RareChanged, out rare, true);
            RareSlider.SetActive(false);


            if (PreGamePickMethodCheckbox.isOn)
            {
                StandardSlider.SetActive(true);
                CommonSlider.SetActive(false);
                UncommonSlider.SetActive(false);
                RareSlider.SetActive(false);
            }
            else
            {
                StandardSlider.SetActive(false);
                CommonSlider.SetActive(true);
                UncommonSlider.SetActive(true);
                RareSlider.SetActive(true);
            }


            MenuHandler.CreateText(" ", menu, out var _, 15);

            void BanChanged(float val)
            {
                LGMMod.PreGameBanConfig.Value = UnityEngine.Mathf.RoundToInt(val);
                LGMMod.PreGameBan = LGMMod.PreGameBanConfig.Value;
            }
            MenuHandler.CreateSlider("Pre-game ban picks", menu, 30, 0f, 10f, LGMMod.PreGameBanConfig.Value, BanChanged, out UnityEngine.UI.Slider ban, true);

            MenuHandler.CreateText(" ", menu, out var _, 15);

            void WinByTwoRoundsCheckboxAction(bool flag)
            {
                LGMMod.WinByTwoRoundsConfig.Value = flag;
                if (LGMMod.WinByTwoPointsConfig.Value && LGMMod.WinByTwoRoundsConfig.Value)
                {
                    LGMMod.WinByTwoPointsConfig.Value = false;
                    WinByTwoPointsCheckbox.isOn = false;
                }
                LGMMod.WinByTwoRounds = LGMMod.WinByTwoRoundsConfig.Value;
            }
            void WinByTwoPointsCheckboxAction(bool flag)
            {
                LGMMod.WinByTwoPointsConfig.Value = flag;
                if (LGMMod.WinByTwoPointsConfig.Value && LGMMod.WinByTwoRoundsConfig.Value)
                {
                    LGMMod.WinByTwoRoundsConfig.Value = false;
                    WinByTwoRoundsCheckbox.isOn = false;
                }
                LGMMod.WinByTwoPoints = LGMMod.WinByTwoPointsConfig.Value;
            }
            WinByTwoPointsCheckbox = MenuHandler.CreateToggle(LGMMod.WinByTwoPointsConfig.Value, "Win By Two Points to break ties", menu, WinByTwoPointsCheckboxAction, 30).GetComponent<Toggle>();
            WinByTwoRoundsCheckbox = MenuHandler.CreateToggle(LGMMod.WinByTwoRoundsConfig.Value, "Win By Two Rounds to break ties", menu, WinByTwoRoundsCheckboxAction, 30).GetComponent<Toggle>();
            MenuHandler.CreateText(" ", menu, out var _, 5);

            void ResetButton()
            {
                timerSlider.value = 0f;
                TimerChanged(0f);
                maxSlider.value = 0f;
                MaxChanged(0f);
                PassCheckbox.isOn = false;
                DiscardAfterCheckbox.isOn = false;
                PassCheckboxAction(false);
                DiscardAfterCheckboxAction(false);
                if (standard != null) { standard.value = 0f; }
                StandardChanged(0f);
                if (common != null) { common.value = 0f; }
                CommonChanged(0f);
                if (uncommon != null) { uncommon.value = 0f; }
                UncommonChanged(0f);
                if (rare != null) { rare.value = 0f; }
                RareChanged(0f);
                ban.value = 0f;
                BanChanged(0f);
                WinByTwoPointsCheckbox.isOn = false;
                WinByTwoRoundsCheckbox.isOn = false;
                WinByTwoPointsCheckboxAction(false);
                WinByTwoRoundsCheckboxAction(false);
            }
            void DefaultButton()
            {
                timerSlider.value = 15f;
                TimerChanged(15f);
                maxSlider.value = 0f;
                MaxChanged(0f);
                PassCheckbox.isOn = false;
                DiscardAfterCheckbox.isOn = false;
                PassCheckboxAction(false);
                DiscardAfterCheckboxAction(false);
                PreGamePickMethodCheckbox.isOn = true;
                PickMethodCheckboxAction(true);
                if (standard != null) { standard.value = 2f; }
                StandardChanged(2f);
                if (common != null) { common.value = 0f; }
                CommonChanged(0f);
                if (uncommon != null) { uncommon.value = 0f; }
                UncommonChanged(0f);
                if (rare != null) { rare.value = 0f; }
                RareChanged(0f);
                ban.value = 2f;
                BanChanged(2f);
                WinByTwoPointsCheckbox.isOn = false;
                WinByTwoRoundsCheckbox.isOn = true;
                WinByTwoPointsCheckboxAction(false);
                WinByTwoRoundsCheckboxAction(true);
            }
            MenuHandler.CreateButton("Disable All", menu, ResetButton, 30);
            MenuHandler.CreateButton("Sane Defaults", menu, DefaultButton, 30);
        }
    }
}