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
    public class ConsumeChestCommand : WolfServiceCommand
    {
        private UserProfile profile;
        private BaseReward reward;
        private Transition.Lock transitionLock;
        private bool _isRPC = false;

        internal override bool isRPC
        {
            get
            {
                return _isRPC;
            }
        }

        protected override void Execute(WolfServiceReq req)
        {
            profile.Inventory.consume(reward);
            req.consumeInventoryChest = new ReqConsumeInventoryChest();
            req.consumeInventoryChest.reward = reward;
            req.type = ReqRepType.ConsumeInventoryChest;
        }

        public static ConsumeChestCommand CreateAndEnqueue(UserProfile profile, BaseReward reward)
        {
            var cmd = new ConsumeChestCommand();
            cmd.profile = profile;
            cmd.reward = reward;
            Session.Instance.Network.Enqueue(cmd);
            return cmd;
        }

        public static void CreateAndExecute(UserProfile profile, BaseReward reward, bool withTransition = true)
        {
            Transition.Lock trans = null;
            if(withTransition)
            {
                trans = Transition.Show(TransitionType.QuickLoad);
            }
            var cmd = new ConsumeChestCommand()
            {
                profile = profile,
                reward = reward,
                transitionLock = trans,
                _isRPC = true
            };

            var result = new WolfServiceResult();
            Session.Instance.StartCoroutine(cmd.Execute(result));
        }

        internal override void HandleResponse(WolfServiceResp resp)
        {
            if (transitionLock != null) transitionLock.Release();
            base.HandleResponse(resp);

            var response = resp.consumeInventoryChest;

            if (response.generatedRewardNotifications != null)
            {
                foreach (var generatedRewardNotification in response.generatedRewardNotifications)
                {
                    Session.Instance.Notifications.Add(generatedRewardNotification, NotificationPeriod.Live);
                }
            }
        }

        internal override void HandleMocked()
        {
            #if UNITY_EDITOR
			if (reward != null && reward is ChestReward)
			{
				var r = ((ChestReward)reward);
				var chestItem = new ShopChestItem(Config.Root)
					{
						id = r.chestId_id,
						cost = new Resource(){
							type = ResourceType.Premium,
							amount = 0
						}
					};
				ArrayUtils.AddToArray(ref chestItem.chestIds, r.chestId_id);
				comms.BuyChestCommand.StartCoroutine(chestItem);
			}
			#endif
			if (transitionLock != null) transitionLock.Release();
        }
    }
}
