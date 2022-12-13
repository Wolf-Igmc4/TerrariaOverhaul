﻿using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using TerrariaOverhaul.Core.Configuration;
using TerrariaOverhaul.Core.Time;
using TerrariaOverhaul.Utilities;

namespace TerrariaOverhaul.Common.Camera;

[Autoload(Side = ModSide.Client)]
public sealed class SmoothCameraSystem : ModSystem
{
	//public static readonly ConfigEntry<bool> SmoothCamera = new(ConfigSide.ClientOnly, "Camera", nameof(SmoothCamera), () => true);
	public static readonly RangeConfigEntry<float> CameraSmoothness = new(ConfigSide.ClientOnly, "Camera", nameof(CameraSmoothness), 0f, 2f, () => 1f);

	// The reason this isn't taken at the start of the modifier is because in that case this modifier will smooth out higher
	// priority modifications, like screenshake.
	private static Vector2? oldPosition;

	public override void Load()
	{
		CameraSystem.RegisterCameraModifier(-100, innerAction => {
			//var oldPosition = Main.screenPosition;

			oldPosition ??= Main.screenPosition;

			innerAction();

			var newPosition = Main.screenPosition;
			var difference = newPosition - oldPosition.Value;
			float differenceLength = difference.SafeLength();
			float maxDifferenceLength = new Vector2(Main.screenWidth, Main.screenHeight).SafeLength() * 0.5f + 100f;

#if DEBUG
			/*
			CameraSmoothness.Value = InputSystem.GetKey(Keys.K) ? 0f : 1f;
			*/
#endif

			if (CameraSmoothness > 0f && differenceLength < maxDifferenceLength) {
				const float BaseSmoothness = 0.01f;

				float deltaTime = CameraSystem.LimitCameraUpdateRate ? TimeSystem.LogicDeltaTime : TimeSystem.RenderDeltaTime;

				Main.screenPosition = Damp(oldPosition.Value, newPosition, CameraSmoothness * BaseSmoothness, deltaTime);
			}

			oldPosition = Main.screenPosition;
		});
	}

	public static float Damp(float source, float destination, float smoothing, float dt)
	{
		// See this:
		// https://www.rorydriscoll.com/2016/03/07/frame-rate-independent-damping-using-lerp

		return MathHelper.Lerp(source, destination, 1f - MathF.Pow(smoothing, dt));
	}

	public static Vector2 Damp(Vector2 source, Vector2 destination, float smoothing, float dt) => new(
		Damp(source.X, destination.X, smoothing, dt),
		Damp(source.Y, destination.Y, smoothing, dt)
	);
}