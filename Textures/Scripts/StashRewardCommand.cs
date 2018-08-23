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
    public class StashRewardCommand : WolfServiceCommand
    {
        private UserProfile profile;
        private RewardTO reward;

        public static StashRewardCommand CreateAndEnqueue(UserProfile profile, RewardTO reward)
        {           
		    var cmd = new StashRewardCommand();
            cmd.profile = profile;
            cmd.reward = reward;
			Session.Instance.Network.Enqueue(cmd);
			return cmd;
		}

        protected override void Execute(WolfServiceReq req)
        {
            req.stashReward = new ReqStashReward() {rewardId = reward.id};
            req.type = ReqRepType.StashReward;

            if (profile.Inventory == null)
                Debug.Log("its null!");

            profile.Inventory.add(reward.rewardTemplate);
            reward.claimed = true;

            ConfigUtils.TrackEventForAward(Analytics.Events.RECEIVE_REWARD_EVENT, reward);
        }
    }
}
