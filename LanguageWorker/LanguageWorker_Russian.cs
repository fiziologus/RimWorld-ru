// Verse.LanguageWorker_Russian
using System;
using Verse;
using System.Text;
using System.Text.RegularExpressions;

public class LanguageWorker_Russian : LanguageWorker
{
	// Для служебного пользования
	private List<string> tmpList = new List<string>;
	// Добавочные файлы
	
	// Неизменяемые слова, вроде "кенгуру"
	private const string pathConst = "LangWork/Constant.txt";

	// Слова изменяющие основу(лев -> льв-ы, льв-ом)
	private const string pathExc = "LangWork/Exception.txt";

	// Список слов вне RulePack: body part, tools etc
	private const string pathUnk = "LangWork/FindGender.txt";

	private bool isConst(string word)
	{
		if Translator.TryGetTranslatedStringsForFile(pathConst, out tmpList)
		{
			foreach(string str in list)
			{
				if (str == word)
				{
					tmpList.Clear();
					return true;
				}
			}
			tmpList.Clear();
			return false;
		}
	}

	private bool Exception(ref string word, ref string mark, bool FindGender)
	{
		string pattern = @"\b" + word + @"::(?<replace>\w+?)\b";
		Regex r = new Regex(pattern);
		Match m = new Match;
		string path = (FindGender ? pathUnk : pathExc);
		if Translator.TryGetTranslatedStringsForFile(path, out tmpList)
		{
			m = r.Match(String.Join(" ", tmpList));
			tmpList.Clear();
			if m.Succes
			{
				if FindGender
					mark = mark.Replace("х", m.Groups["replace"].Value);
				else
					word = m.Groups["replace"].Value;
				return true;
			}
			return false;
		}
	}

	private bool IsWord(string word)
	{
		return (word.Length > 2);
	}

	private string ToAdjMark(string mark)
	{
		return mark
			.Replace("м", "")
			.Replace("ж", "а")
			.Replace("с", "о");
	}

	// Падежи
	private string wordToCase(string word, char wordCase, int Gender, bool Adj, bool Plural)
	{
		bool soft;
		switch(wordCase)
		{
			case 'р':

		}
	}

	public override string Pluralize(string str, int count = -1)
	{
		if (str.NullOrEmpty())
		{
			return str;
		}
		// Берём последнюю букву
		char c = str[str.Length - 1];
		// Если последняя А берём ещё и предпоследнюю
		char c2 = (str.Length != 1 && c == "а") ? str[str.Length - 2] : '\0';
		if ( "гкхжшщч".IndexOf(c) >= 0 )
		{
			return str + "и";
		}
		else if ( ( c == "а" && "гкхжшщч".IndexOf(c2) >= 0 ) || "йья".IndexOf(c) >= 0 )
		{
			return str.Substring(0, str.Length - 1) + "и";
		}
		else if ( c == "о" )
		{
			return str.Substring(0, str.Length - 1) + "а";
		}
		else if ( c == "е" || c == "ё" )
		{
			return str.Substring(0, str.Length - 1) + "я";
		}
		else if ( c == "а" )
		{
			return str.Substring(0, str.Length - 1) + "ы";
		}
		else return str + "ы";
	}

	public override string OrdinalNumber(int number)
	{
		return number + "-й";
	}

	public override string PostProcessed(string str)
	{
		str = base.PostProcessed(str);

		// Раскрываем скобки
		string pat = @":;\s(?<line>.+?\s;:(?<mark>х\w{2})";
		Match m = Regex.Match(str, pat);
		string tmpMark;
		while (m.Success)
		{
			tmpMark = m.Groups["mark"];
			// Рвём по пробел
			string[] wordsInLine = String.Split(m.Groups["line"], " ");
			int index = 0;
			bool back = false;
			while(index >= 0 && < wordsInLine.Length)
			{
				if (! back)
				{
					// Найдено первое (нужное) существительное
					if (IsWord(wordsInLine[index]) && Exception(ref wordsInLine[index], ref tmpMark, true))
					{
						wordsInLine[index] = wordsInLine[index] + "::" + tmpMark;
						tmpMark = ToAdjMark(tmpMark);
						back = true;
					}
					// если род слова не найден, оставляем как есть
					if (((index + 1) == wordsInLine.Length) && (tmpMark.Contains("х")))
						break;
				}
				// потопали назад, расставляя метки по пути
				else
				{
					if (IsWord(wordsInLine[index]))
						wordsInLine[index] = wordsInLine[index] + "::" + tmpMark;
				}
				// Задаём направление движения
				if back
					index--;
				else
					index++;
			}
			str = str.Replace(":; " + m.Groups[line] + " ;:" + m.Groups["mark"], String.Join(wordInLine, " "))
			m = m.NextMatch();
		}
		// Обрабатываем связи
		pat = @"\b(?<word>\w+?-*?\w*?)::(?<mark>\w{3})(?<link>\d{1})\b";
		m = Regex.Match(str, pat);
		while (m.Success)
		{
			tmpMark = m.Groups["mark"];
			string replace;
			if ((tmpMark.Contains("х")) && (! Exception(ref m.Groups["word"], ref tmpMark, true)))
				replace = "";
			else
			{
				str = Regex.Replace(str, @"\b(" + m.Groups["word"] + "::)x", "$1" + tmpMark[0]);

				tmp Mark = ToAdjMark(tmpMark);
				replace = "::" + tmpMark;
			}

			str = str.Replace("::" + m.Groups["link"], replace);
			m = m.NextMatch();
		}

		// Первый этап пройден. Теперь бежим по меткам
		pat = @"\b(?<word>\w+?-*?\w*?)::(?<mark>\w{3})\b";
		m = Regex.Match(str, pat);
		while (m.Success)
		{
			str = Regex.Replace(
					str, 
					@"\b" + m.Groups["word"] + "::" + m.Groups["mark"] + @"\b",
					Scriber(m.Groups["word"], m.Groups["mark"]));
			m = m.NextMatch();
		}

		string a = str.Substring(2);
		if ((str.EndsWith("1 ", StringComparison.OrdinalIgnoreCase) || str.EndsWith("2 ", StringComparison.OrdinalIgnoreCase) || str.EndsWith("3 ", StringComparison.OrdinalIgnoreCase) || str.EndsWith("4 ", StringComparison.OrdinalIgnoreCase)) && (a == "лет" || a == "сезонов" || a == "дней" || a == "часов"))
		{
			str = str.Insert(1, "n");
		}
		str = str.Replace("1 лет", "1 год");
		str = str.Replace("1 сезонов", "1 сезон");
		str = str.Replace("1 дней", "1 день");
		str = str.Replace("1 часов", "1 час");
		str = str.Replace("2 лет", "2 года");
		str = str.Replace("2 сезонов", "2 сезона");
		str = str.Replace("2 дней", "2 дня");
		str = str.Replace("2 часов", "2 часа");
		str = str.Replace("3 лет", "3 года");
		str = str.Replace("3 сезонов", "3 сезона");
		str = str.Replace("3 дней", "3 дня");
		str = str.Replace("3 часов", "3 часа");
		str = str.Replace("4 лет", "4 года");
		str = str.Replace("4 сезонов", "4 сезона");
		str = str.Replace("4 дней", "4 дня");
		str = str.Replace("4 часов", "4 часа");
		return str;
	}

	private string Scriber(string word, string mark)
	{
		// Парсим команды
		// Иерархия:
		// род - м, ж, с для существительного
		//       а (ж.р.), о (с.р) для прилагательных
		//       х если род неизвестен
		// цисло - ы
		// падеж - р д в т п
		// цифра - связка для согласования
		bool Agj = (! mark.Contains("м") || mark.Contains("а") || mark.Contains("о")) ?
			true :
			false; // Слово прилагательное
		bool Plural = (mark.Contains("ы")) ? true : false;
		int Gender;
		if (mark.Contains("ж") || (Adj && mark.Contains("а")))
			Gender = 1;
		else if (mark.Contain("с") || (Adj && mark.Contains("о")))
			Gender = 2;
		else
			Gender = 0;

		if (Adj)
			int pos = (word[word.Length - 1] == "я") ? 2 : 0; // возвратные причастия

		// Падежи
		foreach(char caseMark in "рдвтп")
		{
			if (mark.IndexOf(caseMark) >= 0)
				return wordToCase(word, caseMark, Gender, Adj, Plural);
		}

		// множественное число
		if (mark[1] == "ы")
			if (Adj)
			{
				pos = word.Length - pos;
				char[] strArr = word.ToCharArray();
				strArr[pos] = 'е';
				word = new string(strArr);
			}
			else
			{
				word = this.Pluralize(word, -1);
			}
		else
		{
			// меняем род прилагательного
			if (Adj)
			{

	}
}
