using System;
using UnityEngine;
using System.Collections;
namespace TweaksAssembly.Modules.Tweaks
{
	internal class ForeignExchangeRatesLogging : ModuleLogging
	{
		public ForeignExchangeRatesLogging(BombComponent bombComponent) : base(bombComponent, "ForeignExchangeRates", "Foreign Exchange Rates")
		{
			// Attempt to change the API currency endpoint in the module to a valid one
			component.SetValue("CURRENCY_API_ENDPOINT", "https://fer.eltrick.uk");

			bombComponent.GetComponent<KMBombModule>().OnActivate += () =>
			{
				Log("Test");
				bombComponent.StartCoroutine(OnActivate());
			};
		}

		private IEnumerator OnActivate()
		{
			Log("Connecting..");

			while (!component.GetValue<bool>("isReadyForInput"))
			{
				yield return new WaitForSeconds(.1f);
			}

			if (component.GetValue<bool>("hasRetreivedExchangeRate"))
			{
				Log("LEDS are green");
			}

			else
			{ 
				Log("LEDS are red");
			}
		}
	}
}
