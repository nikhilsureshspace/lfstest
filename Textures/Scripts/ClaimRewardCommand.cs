using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using com.spaceape.wolf.commands;
using System.Collections;
using com.spaceape.wolf.config;
using com.spaceape.wolf.model;

namespace comms
{
    public class ClaimRewardCommand : WolfServiceCommand
    {
        private UserProfile profile;
        private RewardTO reward;
        private Position buildingPosition;
		private string duplicateTroopId;

		public static ClaimRewardCommand CreateAndEnqueue(UserProfile profile, RewardTO reward, string duplicateTroop = "")
		{           
			var cmd = new ClaimRewardCommand();
			cmd.profile = profile;
			cmd.reward = reward;
			cmd.duplicateTroopId = duplicateTroop;

			Session.Instance.Network.Enqueue(cmd);
			return cmd;
		}

		public static void Request(UserProfile profile, RewardTO reward, Action<WolfServiceResp> onResponse, Action onOffline)
		{
			var cmd = new ClaimRewardCommand()
			{
				profile = profile,
				reward = reward
			};
			cmd.Execute(onResponse,onOffline);
		}

        protected override void Execute(WolfServiceReq req)
        {
			req.type = ReqRepType.ClaimReward;

			var results = Session.Instance.Profile.Records.ApplyReward(reward.rewardTemplate, Session.Instance.Profile.Inventory);

            req.claimReward = new ReqClaimReward() {
            	rewardId = reward.id,
            	data = results.PopulateToTO()
            };

			if (reward.rewardTemplate is CurrencyReward && reward.rewardTemplate.sourceType == RewardSourceType.Chest)
            {
				var resource = ((CurrencyReward)reward.rewardTemplate).resource;
				var resourceTemplate = ConfigUtils.GetResourceTemplateByType(resource.type);

				if (resourceTemplate != null && resourceTemplate.chestStatId != null)
				{                    
					profile.Stats.AddStat(resourceTemplate.chestStatId.id, resource.amount);    
				}
            }
			if (reward.rewardTemplate is SwitchReward)
            {
				var s = (SwitchReward)reward.rewardTemplate;
				if (!profile.profileExtraTo.switches.Contains(s.playerSwitch_id))
				{
					ArrayUtils.AddToArray<string>(ref profile.profileExtraTo.switches, s.playerSwitch_id);
				}
            }
            reward.claimed = true;

			ConfigUtils.TrackEventForAward(Analytics.Events.RECEIVE_REWARD_EVENT, reward, duplicateTroopId);
        }

		internal override void HandleResponse(WolfServiceResp resp)
		{
			base.HandleResponse(resp);

			var response = resp.claimReward;

			if (response.duplicateRewardNotifications != null)
			{
				foreach (var duplicateRewardNotification in response.duplicateRewardNotifications)
				{
					Session.Instance.Notifications.Add(duplicateRewardNotification, NotificationPeriod.Live);
				}
			}
		}
    }
}
