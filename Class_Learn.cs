using System;
using Android.Util;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Threading.Tasks;
using System.Threading;

using System.IO;

namespace Kommentator_160212
{
	public class Class_Learn
	{
		//kommunikasjon
		//samtale
		Class_Samtale samtale = new Class_Samtale ();
		//poeng
		//setning
		String focusword;
		//ord
		Dictionary<String, Class_Word> ordkunnskap = new Dictionary<string, Class_Word> ();

		string path;
		static readonly string Filename = "count";
		string filename;
		int count = 0;

		public String respondToSentence (String textInput) //tolk hva mottaker ønsker og lag et svar
		{
			//Log.Info ("Class_Learn", "respondToSentence " + textInput);
			//loadFileAsync ();
			loadStream ();

			//sjekk om det fortsatt er samme samtale, ved å sammenligne tidspunkt med forrige utsagn

			//putt ord i array for prosessering
			int words = 0;
			foreach (string word in textInput.Split(' ')) {
				words++;
			}
			//Log.Info ("Class_Learn", "respondToSentence words=" + words);
			Class_Word[] sentence = new Class_Word[words];
			int count = 0;
			foreach (string word in textInput.Split(' ')) {
				//Log.Info ("Class_Learn", "respondToSentence count=" + count + " word=" + word);
				sentence [count] = new Class_Word (word);
				count++;
			}

			//tolk betydning og finn ordklasser
			//Log.Info ("Class_Learn", "respondToSentence sentence=" + sentence [0].word + " " + sentence [1].word);
			Class_Word[] interpretedWords = word_interpret (sentence);
			//Log.Info ("Class_Learn", "respondToSentence interpretedWords=" + interpretedWords [0].word + " " + interpretedWords [1].word);

			//lagre nye ord
			sentence_SaveNewWords (interpretedWords);

			//lagre statistikk for hvordan ord brukes
			sentence_setStatistics (interpretedWords);

			//find crucial words
			focusword = get_Focus (interpretedWords);

			//lag respons
			//Log.Info ("Class_Learn", "respondToSentence interpretedWords=" + interpretedWords [0].word);
			String response = respond (interpretedWords);
			//Log.Info ("Class_Learn", "respondToSentence response=" + response);

			//uttal respons
			return response;
		}

		private String get_Focus (Class_Word[] interpretedWords)
		{
			String focus = "";

			return focus;
		}

		public Class_Word[] word_interpret (Class_Word[] words_unknown) //tolk ordene som er mottatt, sjekk at de gir mening
		{
			//Log.Info ("Class_Learn", "word_interpret A words=" + words_unknown.Length + " " + words_unknown [0].word);
			String result = "";

			//Hvis det er mer enn 1 ord, send setningen rekursivt for å sjekke komponentene for valid mening
			//hva skal gjøres om det ikke blir funnet noen mening?
			Class_Word[] words_temp = new Class_Word[words_unknown.Length]; //Instansier ny "setning
			words_temp [0] = words_unknown [0];
			Class_Word[] words_checked;
			int index = words_unknown.Length;
			index = 0;
			if (words_unknown.Length > 1) {
				//reduser setning med 1 ord
				words_temp = new Class_Word[words_unknown.Length - 1];
				foreach (Class_Word n in words_temp) {
					//Log.Info ("Class_Learn", "word_interpret B words_unknown=" + words_unknown[index].word);
					words_temp [index] = words_unknown [index];
				}
				//Log.Info ("Class_Learn", "word_interpret C words_unknown=" + +words_unknown.Length + " " + words_temp.Length);
				//sjekk ordkomboen
				words_checked = word_interpret (words_temp);
				/*Log.Info ("Class_Learn", "word_interpret D words_unknown=" + +words_unknown.Length + " " + words_checked.Length);
				foreach (Class_Word n in words_unknown) {
					Log.Info ("Class_Learn", "word_interpret E n=" + n.word);
					result = result + " " + n.word;
					//Log.Info ("Class_Learn", "B word="+n.word+" result="+result);
				}*/
			}

			//undekromponentene er nå sjekket for mening. Sjekk den tilsendte setningen for mening
			//Log.Info ("Class_Learn", "word_interpret F words_temp 0=" + +words_unknown.Length + " " + words_temp.Length);
			Class_Word[] words_assigned = word_matching (words_unknown);
			//Log.Info ("Class_Learn", "word_interpret G words_temp 0=" + +words_unknown.Length + " " + words_assigned.Length);

			return words_assigned;
		}

		private Class_Word[] word_matching (Class_Word[] words_unchecked)
		{ //sjekk at ordene gir mening sammen
			//Log.Info ("Class_Learn", "word_matching 1, words=" + words_unchecked.Length);
			//Log.Info ("Class_Learn", "word_matching 2, words=" + words_unchecked [0].word);
			//finn ut dette ved å tillegge ordklasser som passer sammen
			Class_Word[] words_assigned = sentence_figureWordClasses (words_unchecked);
			//Log.Info ("Class_Learn", "word_matching 3, words=" +  + words_unchecked.Length + " " + words_assigned.Length);
			return words_assigned;	
		}

		private Class_Word[] sentence_figureWordClasses (Class_Word[] words_unchecked)
		{
			//Log.Info ("Class_Learn", "sentence_figureWordClasses 1, words=" + words_unchecked.Length);
			//Log.Info ("Class_Learn", "sentence_figureWordClasses 2, words=" + words_unchecked [0].word);
			///if ending is -er, set as verb
			///if ending is -or, set as object

			Class_Word[] words_assigned = words_unchecked;
			//Log.Info ("Class_Learn", "sentence_figureWordClasses 3, words=" + words_unchecked.Length + " " + words_assigned.Length);
			return words_assigned;
		}

		private void sentence_SaveNewWords (Class_Word[] words)
		{
			Log.Info ("Class_Learn", "sentence_SaveNewWords " + words [0].word);
			foreach (Class_Word a in words) {
				Boolean saved = false;
				Class_Word lagret;
				ordkunnskap.TryGetValue (a.word, out lagret);
				if (lagret != null) {
					saved = true;
				}

				if (saved == false) {
					ordkunnskap.Add (a.word, a);
				}

			}
		}

		private void sentence_setStatistics (Class_Word[] words)
		{//lagre statistikk for hvordan hvert ord brukes i setningen som helhet
			//lagre statistikk for hvor ofte et ord brukes i den aktuelle ordklassen som er tillagt

			Log.Info ("Class_Learn", "sentence_setStatistics " + words [0].word);
			foreach (Class_Word a in words) {
				Class_Word lagret;
				ordkunnskap.TryGetValue (a.word, out lagret);
				if (lagret != null) {
					lagret.first_samples++;
				}
			}

			Class_Word lagret2;
			ordkunnskap.TryGetValue (words [0].word, out lagret2);
			lagret2.first_count++;

		}

		public String respond (Class_Word[] receivedChat)//lag et svar
		{
			//Log.Info ("Class_Learn", "respond 1, words=" + interpretedWords.Length);
			//Log.Info ("Class_Learn", "respond 2, words=" + interpretedWords [0].word);
			String response = "";
			//use crucial words
			//include focus word

			//make a sentence that makes sense
			Class_Word[] words_checked = word_matching (receivedChat);

			foreach (Class_Word n in words_checked) {
				response = response + " " + n.word;
			}
			Log.Info ("Class_Learn", "respond #4, =" + +receivedChat.Length + " " + words_checked.Length);
			//writeFileAsync (receivedChat);
			//saveOrdKunnskap ();
			//SaveData ("ordkunnskap", ordkunnskap);
			return response;

		}


		async void loadFileAsync ()
		{
			Log.Info ("---", "loadFileAsync");
			if (File.Exists (filename)) {
				using (var f = new StreamReader (Application.Context.OpenFileInput (Filename))) {
					string line;
					do {
						line = await f.ReadLineAsync ();
					} while (!f.EndOfStream);
					Log.Info ("---", "Loaded=" + line);
					//return int.Parse (line);

				}
			}
		}

		private void loadStream ()
		{
			//deserialize
			/*using (Stream stream = File.Open (Filename, FileMode.Open)) {
				var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter ();

				//ordkunnskap = (Array<Class_Word>)bformatter.Deserialize (stream);
			}*/
		}

		async void writeFileAsync (Class_Word[] receivedChat)
		{
			foreach (Class_Word n in receivedChat) {
				using (var f = new StreamWriter (Application.Context.OpenFileOutput (n.word, FileCreationMode.Append | FileCreationMode.WorldReadable))) {
					//await f.WriteLineAsync (n.word).ConfigureAwait (false);
					Log.Info ("---", "Saved=" + n.word);
				}
			}

		}

		private void saveOrdKunnskap ()
		{
			//serialize
			/*using (Stream stream = File.Open (Filename, FileMode.Create)) {
				var bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter ();

				bformatter.Serialize (stream, ordkunnskap);
			}*/
		}

		protected bool SaveData (string FileName, Class_Word[] Data)
		{
			BinaryWriter Writer = null;
			string Name = @"C:\temp\yourfile.name";

			try {
				// Create a new stream to write to the file
				//Writer = new BinaryWriter (File.OpenWrite (Name));

				// Writer raw data                
				//Writer.Write (Data);
				//Writer.Flush ();
				//Writer.Close ();
			} catch {
				return false;
			}

			return true;
		}
			
	}
}

