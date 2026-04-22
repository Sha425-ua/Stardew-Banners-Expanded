using System;
using System.Collections.Generic;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.GameData.Objects;

namespace BannersExpandedLogic
{
    public class ModEntry : Mod
    {
        private const string TargetEventID = "3910974";
        private const string ChickenRewardFlag = "Sha_ShaneBanner_Received";
        private const string ChickenBannerID = "(F)MyCustomChickenBanner";

        private const string GoldenCoconutID = "791";
        private const string TropicalBannerID = "(F)MyCustomGoldCoconutBanner";
        private const float CoconutChance = 0.05f;

        private const string OmniGeodeID = "749";
        private const string GeodeBannerID = "(F)MyCustomGeodeBanner";
        private const float GeodeChance = 0.01f;

        private const string FishBannerID = "(F)MyCustomFishBanner";
        private const float FishTreasureChance = 0.03f;

        private const string PearlBannerID = "(F)MyCustomPearlBanner"; 
        private const string PearlRewardFlag = "Sha_PearlBanner_Received"; 
        private const string VanillaPearlFlag = "gotPearl"; 
        private const string MermaidLocationName = "MermaidHouse"; 

        public override void Entry(IModHelper helper)
        {
            helper.Events.GameLoop.OneSecondUpdateTicked += OnOneSecondUpdateTicked;
            helper.Events.Content.AssetRequested += OnAssetRequested;
            helper.Events.Display.MenuChanged += OnMenuChanged;

            helper.Events.GameLoop.DayStarted += OnDayStarted;
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            if (!Context.IsWorldReady || Game1.player == null) return;

            if (Game1.player.GetDaysMarried() >= 112) 
            {
                string mailId = "Sha_425.MomAnniversary_Gift";  

                if (!Game1.player.mailReceived.Contains(mailId) && !Game1.player.mailForTomorrow.Contains(mailId))
                {
                    Game1.mailbox.Add(mailId);
                }
            }
        }
        

        private void OnMenuChanged(object sender, MenuChangedEventArgs e)
        {
            if (e.NewMenu is ItemGrabMenu menu)
            {
                if (menu.context is StardewValley.Tools.FishingRod)
                {


                    if (Game1.random.NextDouble() < FishTreasureChance) 
                    {
                        Item banner = ItemRegistry.Create(FishBannerID, 1);
                        
                        if (banner != null)
                        {
                            bool alreadyHas = false;
                            foreach (var item in menu.ItemsToGrabMenu.actualInventory)
                            {
                                if (item != null && item.ItemId == FishBannerID) 
                                {
                                    alreadyHas = true; 
                                    break; 
                                }
                            }

                            if (!alreadyHas)
                            {
                                menu.ItemsToGrabMenu.actualInventory.Add(banner);
                            }
                        }
                        else
                        {
                            Monitor.Log($"[ERROR] Item {FishBannerID} not found", LogLevel.Error);
                        }
                    }
                }
            }
        }

        private void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
            {
                e.Edit(asset =>
                {
                    var data = asset.GetData<Dictionary<string, ObjectData>>();

                    if (data.TryGetValue(GoldenCoconutID, out var goldenCoconut))
                    {
                        if (goldenCoconut.GeodeDrops == null)
                            goldenCoconut.GeodeDrops = new List<ObjectGeodeDropData>();

                        bool coconutExists = false;
                        foreach (var drop in goldenCoconut.GeodeDrops)
                        {
                            if (drop.ItemId == TropicalBannerID) { coconutExists = true; break; }
                        }

                        if (!coconutExists)
                        {
                            goldenCoconut.GeodeDrops.Add(new ObjectGeodeDropData()
                            {
                                Id = "Sha425_TropicalBannerDrop", 
                                ItemId = TropicalBannerID,
                                Chance = CoconutChance,
                                Condition = null,
                                Precedence = 0
                            });
                        }
                    }

                    if (data.TryGetValue(OmniGeodeID, out var omniGeode))
                    {
                        if (omniGeode.GeodeDrops == null)
                            omniGeode.GeodeDrops = new List<ObjectGeodeDropData>();

                        bool geodeExists = false;
                        foreach (var drop in omniGeode.GeodeDrops)
                        {
                            if (drop.ItemId == GeodeBannerID) { geodeExists = true; break; }
                        }

                        if (!geodeExists)
                        {
                            omniGeode.GeodeDrops.Add(new ObjectGeodeDropData()
                            {
                                Id = "Sha425_GeodeBannerDrop", 
                                ItemId = GeodeBannerID,
                                Chance = GeodeChance,
                                Condition = null,
                                Precedence = 0
                            });
                        }
                    }
                });
            }
        }

        private void OnOneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e)
        {
            if (!Context.IsWorldReady || Game1.player == null) return;

            CheckChickenBanner();
            CheckPearlBanner();
        }

        private void CheckChickenBanner()
        {

            bool eventSeen = Game1.player.eventsSeen.Contains(TargetEventID);
            bool rewardReceived = Game1.player.mailReceived.Contains(ChickenRewardFlag);

            if (eventSeen && !rewardReceived && !Game1.eventUp)
            {
                GiveReward(ChickenBannerID, ChickenRewardFlag, "You received a Chicken Banner!");
            }
        }

        private void CheckPearlBanner()
        {
            if (Game1.currentLocation?.Name == MermaidLocationName && 
                Game1.player.mailReceived.Contains(VanillaPearlFlag) && 
                !Game1.player.mailReceived.Contains(PearlRewardFlag))
            {
                GiveReward(PearlBannerID, PearlRewardFlag, "You received a Pearl Banner!");
            }
        }
        private void GiveReward(string itemID, string mailFlag, string message)
        {
            try
            {
                Item banner = ItemRegistry.Create(itemID);

                if (banner == null)
                {
                    Monitor.Log($"[ERROR] Unable to find item with ID: {itemID}", LogLevel.Error);
                    return;
                }

                if (Game1.player.addItemToInventory(banner) == null)
                {
                    Game1.addHUDMessage(new HUDMessage(message, HUDMessage.newQuest_type));
                }
                else
                {
                    Game1.createItemDebris(banner, Game1.player.getStandingPosition(), 2);
                    Game1.addHUDMessage(new HUDMessage("Inventory full. Banner dropped!", HUDMessage.error_type));
                }

                Game1.player.mailReceived.Add(mailFlag);
            }
            catch (Exception ex)
            {
                Monitor.Log($"[ERROR] Error occurred while giving reward ({itemID}): {ex.Message}", LogLevel.Error);
            }
        }
    }
}