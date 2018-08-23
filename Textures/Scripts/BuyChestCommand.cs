using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using com.spaceape.wolf.config;
using com.spaceape.wolf.commands;

namespace comms
{

	public class BuyChestCommand : WolfServiceCommand
	{
		Resource cost;
		ShopChestItem chest;
		string itemId;
		string timeLimitedChestId;

		public static Action<string,Action<BaseReward>> OnSelectReward;

		public static Coroutine StartCoroutine(ShopChestItem chest, string itemId = "")
		{
			return StartCoroutine(chest, chest.cost, itemId);
		}

		public static Coroutine StartCoroutine(ShopChestItem chest, Resource cost, string itemId = "", string timeLimitedChestId = "")
		{
			if(cost != null
#if !UNITY_EDITOR
				&& cost.type != ResourceType.NA
#endif
			   )
			{
				var cmd = new BuyChestCommand()
				{
					chest = chest,
					cost = cost,
					itemId = itemId,
					timeLimitedChestId = timeLimitedChestId
				};
				return Session.Instance.StartCoroutine(cmd.BuyCoroutine());
			}
			else
			{
				Logger.WarnCh(LogChannels.IAP, "BuyChestCommand Attempted to purchase with a Cost with no Resource Type");
				return null;
			}
		}
		
		IEnumerator BuyCoroutine()
		{
			var transition = Transition.Show(TransitionType.QuickLoad);
			var result = new WolfServiceResult();
			yield return Session.Instance.StartCoroutine(Execute(result));

			if(result.Response != null)
			{
				if(result.Response.buyChest == null)
				{
					BasicDialog.DialogData data = new BasicDialog.DialogData(BasicDialog.DialogType.OneButton);
					data.Title = Lang.Tran("buychest-noreward-title", "Chest");
					data.Headline = Lang.Tran("buychest-noreward-headline", "Sorry!");
					data.Body = Lang.Tran("buychest-noreward", "Better luck next time...");
					BasicDialog.Show(data);
				}
			}

			transition.Release ();
		}
		
		protected override void Execute(WolfServiceReq req)
		{
			var boughtAt = ServerTime.Instance.GetCurrentTimeLong();

			if(!string.IsNullOrEmpty(itemId))
			{
				var profile = Session.Instance.Profile;
				profile.ScheduledItems.PurchaseItem(itemId,boughtAt);
			}

			Session.Instance.Profile.Resources.Spend(cost);
			req.buyChest = new ReqBuyChest() {
				shopChestId = chest.id,
				buyAtTime = boughtAt,
				itemId = itemId,
				timeLimitedChestId = timeLimitedChestId
			};
			ArrayUtils.AddToArray(ref req.buyChest.costs, cost);

			Analytics.TrackEvent(Analytics.Events.IG_BUY, new Dictionary<string, string>()
			                     {
				{Analytics.Params.IG_ITEM, chest.id},                    
				{Analytics.Params.IG_CURRENCY_TYPE_SPEND, cost != null ? cost.type.ToString() : "Free"},
				{Analytics.Params.IG_CURRENCY_QUANTITY_SPEND, cost != null ? cost.amount.ToString() : "0"},
				{Analytics.Params.IG_ITEM_QUANTITY, "1"},
				{Analytics.Params.IG_ITEM_GROUP, "chest"}
			});
		}

		internal override void HandleMocked ()
		{
			base.HandleMocked ();
			#if UNITY_EDITOR
			if (OnSelectReward != null)
			{
				OnSelectReward(chest.chestIds.First(), reward => {
				
					var coreReward = reward as PowerCoreReward;
					if (coreReward != null)
					{
						var id = Guid.NewGuid().ToString();
						coreReward.powerCoreIds = new[] {id};
					}
					
					var to = new NotificationTO(){
						id = DateTime.Now.Ticks.ToString() + "-offline",
						type = NotificationType.Reward,
						reward = new RewardTO(){
							id = DateTime.Now.Ticks.ToString() + "-offline",
							rewardTemplate = reward
						}
					};
					to.reward.rewardTemplate.sourceId = chest.chestIds.First();
					Session.Instance.Notifications.Add(to, NotificationPeriod.Live);
				});
			}
			else
			#endif
			{
				var to = new NotificationTO(){
					type = NotificationType.Reward,
					reward = new RewardTO(){
						id = "offline",
						rewardTemplate = new CurrencyReward(){
							//CurrencyReward = new CurrencyRewardTO(){
								resource = new Resource(){
									amount = 100,
									type = ResourceType.Premium
								}
							//}
						}
					}
				};
				Session.Instance.Notifications.Add(to, NotificationPeriod.Live);
			}
		}
	}
}
