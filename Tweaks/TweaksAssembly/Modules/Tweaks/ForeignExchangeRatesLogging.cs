using System;
using UnityEngine;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

//Do strike Logging
//Verify battery count works correctly
namespace TweaksAssembly.Modules.Tweaks
{
	internal class ForeignExchangeRatesLogging : ModuleLogging
	{
		string endpoint = "https://fer.eltrick.uk";
		private KMSelectable[] buttons;
		FieldInfo exChangeRatesFld;
		int batteryCount;
		bool batteryRule;
		int baseValue;

		CountryCode baseCode;
		CountryCode targetCode;

		int answer;


		object[] countryCodesArr;
		//Figure out how to get this array from the module
		CountryCode[] countryCodes = new CountryCode[31]
		{
			new CountryCode("AUD", "036"),
			new CountryCode("BGN", "975"),
			new CountryCode("BRL", "986"),
			new CountryCode("CAD", "124"),
			new CountryCode("CHF", "756"),
			new CountryCode("CNY", "156"),
			new CountryCode("DKK", "208"),
			new CountryCode("EUR", "978"),
			new CountryCode("GBP", "826"),
			new CountryCode("HKD", "344"),
			new CountryCode("HRK", "191"),
			new CountryCode("HUF", "348"),
			new CountryCode("IDR", "360"),
			new CountryCode("ILS", "376"),
			new CountryCode("INR", "356"),
			new CountryCode("JPY", "392"),
			new CountryCode("KRW", "410"),
			new CountryCode("MXN", "484"),
			new CountryCode("MYR", "458"),
			new CountryCode("NOK", "578"),
			new CountryCode("NZD", "554"),
			new CountryCode("PHP", "608"),
			new CountryCode("PLN", "985"),
			new CountryCode("RON", "946"),
			new CountryCode("RUB", "643"),
			new CountryCode("SEK", "752"),
			new CountryCode("SGD", "702"),
			new CountryCode("THB", "764"),
			new CountryCode("TRY", "949"),
			new CountryCode("USD", "840"),
			new CountryCode("ZAR", "710")
		};

		public ForeignExchangeRatesLogging(BombComponent bombComponent) : base(bombComponent, "ForeignExchangeRates", "Foreign Exchange Rates")
		{
			// Attempt to change the API currency endpoint in the module to a valid one
			component.SetValue("CURRENCY_API_ENDPOINT", endpoint);
			batteryCount = bombComponent.Bomb.QueryWidgets<int>(KMBombInfo.QUERYKEY_GET_BATTERIES, "numbatteries").Sum();
			Log($"Battery Count: {batteryCount}");
			batteryRule = batteryCount > 1;
			Log("1");
			countryCodesArr = component.GetValue<object[]>("COUNTRY_CODES");
			Log("2");
			Debug.Log($"This should be AUD: {countryCodesArr[0].GetValue<string>("code")}");
			Log("3");
			bombComponent.GetComponent<KMBombModule>().OnActivate += () =>
			{
				bombComponent.StartCoroutine(OnActivate());
			};

			buttons = component.GetValue<KMSelectable[]>("buttons");

			Debug.Log("Button length: " + buttons.Length);
			for (int i = 0; i < buttons.Length; i++)
			{
				int dummy = i;

				buttons[dummy].OnInteract += delegate
				{
					Log("" + dummy);
					string str = $"Pressed {dummy}. ";
					if (component.GetValue<bool>("isReadyForInput"))
					{
						return false;
					}
					str += i != answer ? "Strike!" : "Module Solved";
					Log(str);
					return false;
				};
			}

		}

		private IEnumerator OnActivate()
		{
			Log($"Connecting to {endpoint}....");
			while (!component.GetValue<bool>("isReadyForInput"))
			{
				yield return new WaitForSeconds(.1f);
			}
			answer = component.GetValue<int>("answer");
			if (component.GetValue<bool>("hasRetreivedExchangeRate"))
			{
				Log("LEDS are green");
				GetButtonsStrings();

				object exchangeRate = exChangeRatesFld.GetValue(component);
				Type exchangeType = exchangeRate.GetType();
				FieldInfo rateFld = exchangeType.GetField("rates", BindingFlags.Public | BindingFlags.Instance);
				Dictionary<string, float> rates = (Dictionary<string, float>) rateFld.GetValue(exchangeRate);
				float rate = rates.Values.ElementAt(0);
				float num = baseValue * rate;
				Log($"The conversion rate is {rate}");
				Log($"The target currency value is {num}");
				Log($"The updated target currency value is {(num < 10 ? 0 : Mathf.Floor(num))}");
			}

			else
			{ 
				Log("LEDS are red");
				GetButtonsStrings();

			}

			Log($"Press button {answer}");
		}

		private void GetButtonsStrings()
		{
			Log("There is more than 1 battery. Swapping base and target currency");
			string baseValueStr = GetButtonStrings(6);
			baseValue = int.Parse(baseValueStr);

			baseCode = GetCountryCode(batteryRule ? 3 : 0);
			targetCode = GetCountryCode(batteryRule ? 0 : 3);
			Log($"The base is {CountryCode(baseCode)}");
			Log($"The target is {CountryCode(targetCode)}");
			Log($"The base value is {baseValueStr}");
		}

		private CountryCode GetCountryCode(int index)
		{
			string str = GetButtonStrings(index);
			CountryCode code = countryCodes.FirstOrDefault(c => c.code == str);
			return code ?? countryCodes.First(c => c.ISO4217 == str);
		}

		private string GetButtonStrings(int index)
		{
			string[] arr = Enumerable.Range(index, 3).Select(ix => buttons[ix].GetComponentInChildren<TextMesh>().text).ToArray();
			
			return string.Join("", arr);
		}

		private string CountryCode(CountryCode c)
		{
			return $"{c.code} ({c.ISO4217})";
		}
	}

	internal class CountryCode
	{
		public string code;
		public string ISO4217;

		public CountryCode(string code, string ISO4217)
		{
			this.code = code;
			this.ISO4217 = ISO4217;
		}
	}
}
