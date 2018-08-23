using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using com.spaceape.wolf.config;
using com.spaceape.wolf.commands;

namespace comms
{
	public class GiftChestCommand : WolfServiceCommand
	{
		Resource cost;
		ShopChestItem chest;
		AllianceMember allianceMember;
		string message;
		string timeLimitedChestId;
		Action OnComplete;
		Transition.Lock transition;

		public static Coroutine StartCoroutine(ShopChestItem chest, Resource cost, AllianceMember allianceMember, string message, string timeLimitedChestId, Action onComplete)
		{
			if(cost != null && cost.type != ResourceType.NA)
			{
				var cmd = new GiftChestCommand()
				{
					chest = chest,
					cost = cost,
					allianceMember = allianceMember,
					message = message,
					timeLimitedChestId = timeLimitedChestId,
					OnComplete = onComplete
				};
				return Session.Instance.StartCoroutine(cmd.BuyCoroutine());
			}
			else
			{
				Logger.WarnCh(LogChannels.IAP, "GiftChestCommand Attempted to purchase with a Cost with no Resource Type");
				return null;
			}
		}
		
		IEnumerator BuyCoroutine()
		{
			transition = Transition.Show(TransitionType.QuickLoad);
			var result = new WolfServiceResult();
			yield return Session.Instance.StartCoroutine(Execute(result));
		}
		
		protected override void Execute(WolfServiceReq req)
		{
			Session.Instance.Profile.Resources.Spend(cost);
			req.giftChest = new ReqGiftChest() {
				shopChestId = chest.id,
				recipientClide = allianceMember.Clide,
				message = message,
				timeLimitedChestId = timeLimitedChestId
			};
			ArrayUtils.AddToArray(ref req.giftChest.costs, cost);

			Analytics.TrackEvent(Analytics.Events.IG_GIFT, new Dictionary<string, string>()
			                     {
				{Analytics.Params.IG_ITEM, chest.id},                    
				{Analytics.Params.IG_CURRENCY_TYPE_SPEND, cost != null ? cost.type.ToString() : "Free"},
				{Analytics.Params.IG_CURRENCY_QUANTITY_SPEND, cost != null ? cost.amount.ToString() : "0"},
				{Analytics.Params.IG_ITEM_QUANTITY, "1"},
				{Analytics.Params.IG_ITEM_GROUP, "chest"}
			});
		}

		internal override void HandleResponse(WolfServiceResp resp)
		{
			if (transition != null) transition.Release();

			base.HandleResponse(resp);

			if(OnComplete != null)
			{
				OnComplete();
			}
		}

		internal override void HandleMocked ()
		{
			if (transition != null) transition.Release();

			base.HandleMocked ();

			if(OnComplete != null)
			{
				OnComplete();
			}
		}
	}
}
