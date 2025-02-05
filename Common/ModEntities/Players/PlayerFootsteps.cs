﻿using Terraria;
using TerrariaOverhaul.Common.Systems.Footsteps;
using TerrariaOverhaul.Common.Systems.Time;
using TerrariaOverhaul.Core.Systems.Configuration;
using TerrariaOverhaul.Utilities.Extensions;

namespace TerrariaOverhaul.Common.ModEntities.Players
{
	public sealed class PlayerFootsteps : PlayerBase
	{
		public static readonly ConfigEntry<bool> EnablePlayerFootsteps = new(ConfigSide.ClientOnly, "Ambience", nameof(EnablePlayerFootsteps), () => true);

		private const double FootstepCooldown = 0.1;

		private byte stepState;
		private double lastFootstepTime;

		public override void PostItemCheck()
		{
			if(Main.dedServ || !EnablePlayerFootsteps) {
				return;
			}

			bool onGround = Player.OnGround();
			bool wasOnGround = Player.WasOnGround();
			int legFrame = Player.legFrame.Y / Player.legFrame.Height;

			FootstepType? footstepType = null;

			if(onGround != wasOnGround) {
				if(!onGround || Player.controlJump) {
					footstepType = FootstepType.Jump;
				} else {
					footstepType = FootstepType.Land;
				}
			} else if(onGround) {
				footstepType = FootstepType.Default;
			}

			if(footstepType.HasValue && (footstepType.Value != FootstepType.Default || (stepState == 1 && (legFrame == 16 || legFrame == 17)) || (stepState == 0 && (legFrame == 9 || legFrame == 10)))) {
				double time = TimeSystem.GlobalTime;

				if(time - lastFootstepTime > FootstepCooldown && FootstepSystem.Footstep(Player, footstepType.Value)) {
					stepState = stepState == 0 ? 1 : 0;
					lastFootstepTime = TimeSystem.GlobalTime;
				}
			}

			if(!onGround || legFrame == 0) {
				stepState = 0;
			}
		}
	}
}
