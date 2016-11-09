using System;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Views.InputMethods;
using Android.Text;
using Android.Speech.Tts;
using System.Collections.Generic;
using System.Linq;
using Android.Util;
using Android.Speech;

namespace Kommentator_160212
{

	[Activity (Label = "Kommentator 160212", MainLauncher = true, Icon = "@drawable/icon")]
	public class MainActivity : Activity, TextToSpeech.IOnInitListener
	{

		Class_Learn learner = new Class_Learn();

		private bool isRecording;
		private readonly int VOICE = 10;
		private TextView textBox;
		private Button recButton;

		private TextToSpeech myTTS;
		TextToSpeech textToSpeech;
		Context context;
		private readonly int MyCheckCode = 101, NeedLang = 103;
		Java.Util.Locale lang;
		public const int CHECK_VOICE_DATA_PASS = 0x00000001;

		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView (Resource.Layout.Main);

			//Speak to text
			isRecording = false;
			recButton = FindViewById<Button> (Resource.Id.btnRecord);
			Log.Info ("MainActivity", "recButton 1=" + recButton);
			textBox = FindViewById<TextView> (Resource.Id.textYourText);
			string rec = Android.Content.PM.PackageManager.FeatureMicrophone;
			if (rec != "android.hardware.microphone") {
				// no microphone, no recording. Disable the button and output an alert
				var alert = new AlertDialog.Builder (recButton.Context);
				alert.SetTitle ("You don't seem to have a microphone to record with");
				alert.SetPositiveButton ("OK", (sender, e) => {
					textBox.Text = "No microphone present";
					recButton.Enabled = false;
					return;
				});

				alert.Show ();
			} else {
				Log.Info ("MainActivity", "recButton 2=" + recButton);
				recButton.Click += delegate {

					// change the text on the button
					recButton.Text = "End Recording";
					isRecording = !isRecording;
					if (isRecording) {
						Log.Info ("MainActivity", "isRecording=" + isRecording);
						// create the intent and start the activity
						var voiceIntent = new Intent (RecognizerIntent.ActionRecognizeSpeech);
						voiceIntent.PutExtra (RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);

						// put a message on the modal dialog
						voiceIntent.PutExtra (RecognizerIntent.ExtraPrompt, Application.Context.GetString (Resource.String.messageSpeakNow));

						// if there is more then 1.5s of silence, consider the speech over
						voiceIntent.PutExtra (RecognizerIntent.ExtraSpeechInputCompleteSilenceLengthMillis, 1500);
						voiceIntent.PutExtra (RecognizerIntent.ExtraSpeechInputPossiblyCompleteSilenceLengthMillis, 1500);
						voiceIntent.PutExtra (RecognizerIntent.ExtraSpeechInputMinimumLengthMillis, 15000);
						voiceIntent.PutExtra (RecognizerIntent.ExtraMaxResults, 1);

						// you can specify other languages recognised here, for example
						// voiceIntent.PutExtra(RecognizerIntent.ExtraLanguage, Java.Util.Locale.German);
						// if you wish it to recognise the default Locale language and German
						// if you do use another locale, regional dialects may not be recognised very well

						voiceIntent.PutExtra (RecognizerIntent.ExtraLanguage, Java.Util.Locale.Default);
						StartActivityForResult (voiceIntent, VOICE);
					}
				};
			}
				
			//Text to speak
			var btnSayIt = FindViewById<Button> (Resource.Id.btnSpeak);
			var editWhatToSay = FindViewById<EditText> (Resource.Id.editSpeech);
			var spinLanguages = FindViewById<Spinner> (Resource.Id.spinLanguage);
			var txtSpeedVal = FindViewById<TextView> (Resource.Id.textSpeed);
			var txtPitchVal = FindViewById<TextView> (Resource.Id.textPitch);
			var seekSpeed = FindViewById<SeekBar> (Resource.Id.seekSpeed);
			var seekPitch = FindViewById<SeekBar> (Resource.Id.seekPitch);
			seekSpeed.Progress = seekPitch.Progress = 127;
			txtSpeedVal.Text = txtPitchVal.Text = "1.0";
			context = btnSayIt.Context;
			textToSpeech = new TextToSpeech (this, this, "com.google.android.tts");
			var langAvailable = new List<string>{ "Default" };
			var localesAvailable = Java.Util.Locale.GetAvailableLocales ().ToList ();
			foreach (var locale in localesAvailable) {
				LanguageAvailableResult res = textToSpeech.IsLanguageAvailable (locale);
				Log.Info ("MainActivity", "locale available=" + locale);
				switch (res) {
				case LanguageAvailableResult.Available:
					langAvailable.Add (locale.DisplayLanguage);
					break;
				case LanguageAvailableResult.CountryAvailable:
					langAvailable.Add (locale.DisplayLanguage);
					break;
				case LanguageAvailableResult.CountryVarAvailable:
					langAvailable.Add (locale.DisplayLanguage);
					break;
				}

			}
			langAvailable = langAvailable.OrderBy (t => t).Distinct ().ToList ();
			var adapter = new ArrayAdapter<string> (this, Android.Resource.Layout.SimpleSpinnerDropDownItem, langAvailable);
			spinLanguages.Adapter = adapter;
			lang = Java.Util.Locale.Default;
			textToSpeech.SetLanguage (lang);
			textToSpeech.SetPitch (1.1f);
			textToSpeech.SetSpeechRate (1.0f);
			btnSayIt.Click += delegate {
				Log.Info ("MainActivity", "btnSayIt: " + editWhatToSay.Text);
				if (!string.IsNullOrEmpty (editWhatToSay.Text))
					textToSpeech.Speak (editWhatToSay.Text, QueueMode.Flush, null);
			};
			seekPitch.StopTrackingTouch += (object sender, SeekBar.StopTrackingTouchEventArgs e) => {
				var seek = sender as SeekBar;
				var progress = seek.Progress / 255f;
				textToSpeech.SetPitch (progress);
				txtPitchVal.Text = progress.ToString ("F2");
			};
			seekSpeed.StopTrackingTouch += (object sender, SeekBar.StopTrackingTouchEventArgs e) => {
				var seek = sender as SeekBar;
				var progress = seek.Progress / 255f;
				textToSpeech.SetSpeechRate (progress);
				txtSpeedVal.Text = progress.ToString ("F2");
			};
			spinLanguages.ItemSelected += (object sender, AdapterView.ItemSelectedEventArgs e) => {
				lang = Java.Util.Locale.GetAvailableLocales ().FirstOrDefault (t => t.DisplayLanguage == langAvailable [(int)e.Id]);
				// create intent to check the TTS has this language installed
				var checkTTSIntent = new Intent ();
				checkTTSIntent.SetAction (TextToSpeech.Engine.ActionCheckTtsData);
				StartActivityForResult (checkTTSIntent, NeedLang);
			};
			//VOLUMKONTROLL!! Volumet er uavhengig fra alle andre volumkontroller på device!

			//parsing
			var btnTell = FindViewById<Button> (Resource.Id.btnTell);
			var say = FindViewById<EditText> (Resource.Id.textYouSay);
			btnTell.Click += delegate {
				Log.Info ("MainActivity", "btnTell: " + say.Text);
				if (!string.IsNullOrEmpty (say.Text)) {
					String tell = learner.respondToSentence (say.Text);
					textBox.Text = tell;
					textToSpeech.Speak (tell, QueueMode.Flush, null);
					//learn (say.Text);
				}
			};
		}

		void TextToSpeech.IOnInitListener.OnInit (OperationResult status)
		{
			if (status == OperationResult.Error)
				textToSpeech.SetLanguage (Java.Util.Locale.Default);
			if (status == OperationResult.Success)
				textToSpeech.SetLanguage (lang);
		}

		protected void onActivityResult (int req, int resultCode, Intent data)
		{
			if (req == NeedLang) {// we need a new language installed
				//if (resultCode == TextToSpeech.Engine.CHECK_VOICE_DATA_PASS) {//Indicates success when checking the installation status of the resources used by the TextToSpeech engine with the ACTION_CHECK_TTS_DATA intent.
				if (resultCode == CHECK_VOICE_DATA_PASS) {//Indicates success when checking the installation status of the resources used by the TextToSpeech engine with the ACTION_CHECK_TTS_DATA intent.
					//the user has the necessary data - create the TTS
					myTTS = new TextToSpeech (this, this);
				} else {
					//no data - install it now
					var installTTS = new Intent ();
					installTTS.SetAction (TextToSpeech.Engine.ActionInstallTtsData);
					StartActivity (installTTS);
				}
			}
		}


		protected override void OnActivityResult (int requestCode, Result resultVal, Intent data)
		{
			if (requestCode == VOICE) {
				if (resultVal == Result.Ok) {
					var matches = data.GetStringArrayListExtra (RecognizerIntent.ExtraResults);
					if (matches.Count != 0) {
						string textInput = matches [0];

						// limit the output to 500 characters
						if (textInput.Length > 500)
							textInput = textInput.Substring (0, 500);
						textBox.Text = textInput;
					} else
						textBox.Text = "No speech was recognised";
					// change the text back on the button
					recButton.Text = "Start Recording";
				}
			}

			base.OnActivityResult (requestCode, resultVal, data);
		}

	}
}