using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LexicalNormalisation
{
    class Program
    {
        Hashtable labelledTokensTable;
        List<string> dictionaryList;
        int totalAccurateMatches;
        int totalRecallMatches;
        StreamReader ReadFile;
        StreamWriter WriteResults;
        string Operations;
        
        public Program(String path)
        {
            labelledTokensTable = new Hashtable();
            dictionaryList = new List<string>();
            totalAccurateMatches = 0;
            totalRecallMatches = 0;
            WriteResults = new StreamWriter(path + "\\Results.txt", false, Encoding.Default);
            WriteResults.AutoFlush = true;
            Operations = "";
            
        }

        public static int NGramDistance(string FirstString, string SecondString)
        {
            //creating grams of both strings
            int g2FirstString = 0;
            int g2SecondString = 0;
            int MatchNumbers = 0;
            int nGramDistance = 0;
            int lf = FirstString.Length - 1;
            int lt = SecondString.Length - 1;
            ArrayList FirstStringGrams = new ArrayList();
            ArrayList SecondStringGrams = new ArrayList();
            FirstStringGrams.Add(FirstString[0].ToString());
            SecondStringGrams.Add(SecondString[0].ToString());

            for (int i = 0; i < lf; i++)
                FirstStringGrams.Add(FirstString.Substring(i, 2));
            FirstStringGrams.Add(FirstString[lf].ToString());

            for (int i = 0; i < lt; i++)
                SecondStringGrams.Add(SecondString.Substring(i, 2));
            SecondStringGrams.Add(SecondString[lt].ToString());
            g2FirstString = FirstStringGrams.Count;
            g2SecondString = SecondStringGrams.Count;

            foreach (var values in FirstStringGrams)
                if (SecondStringGrams.Contains(values))
                    MatchNumbers++;

            nGramDistance = g2FirstString + g2SecondString - (2 * MatchNumbers);
            return nGramDistance;
        }
        public static int GlobalDistanceCalculate(string FirstString, string SecondString)
        {
            int lf = FirstString.Length;
            int lt = SecondString.Length;
            int[,] ResultArray = new int[lt + 1, lf + 1];
            ResultArray[0, 0] = 0;
            int d = -1;
            int i = -1;

            //global distance method
            for (int k = 1; k <= lf; k++)
                ResultArray[0, k] = k * d;
            for (int j = 1; j <= lt; j++)
                ResultArray[j, 0] = j * i;

            for (int j = 1; j <= lt; j++)
                for (int k = 1; k <= lf; k++)
                {
                    int Deletion = ResultArray[j, k - 1] + d;
                    int Insertion = ResultArray[j - 1, k] + i;
                    int RepMatch = ResultArray[j - 1, k - 1] + AreEqual(FirstString[k - 1], SecondString[j - 1]);

                    ResultArray[j, k] = Math.Max(Deletion, Math.Max(Insertion, RepMatch));
                }

            return ResultArray[lt, lf];
        }
        private static int AreEqual(int firstChar, int secondChar)
        {
            if (firstChar.Equals(secondChar))
                return 1; // match value
            else
                return -1; //replace value
        }
        private static string SoundexFunction(string data) //Soundex implemented as it is from TechRepublic
        {

            StringBuilder Result = new StringBuilder();
            if (data != null && data.Length > 0)
            {
                string previousCode = "";
                string currentCode = "";
                string currentLetter = "";

                Result.Append(data.Substring(0, 1));
                for (int i = 0; i < data.Length; i++)
                {
                    currentLetter = data.Substring(i, 1).ToLower();
                    currentCode = "";

                    if ("bfpv".IndexOf(currentLetter) > -1)
                        currentCode = "1";
                    else if ("cgjkqsxz".IndexOf(currentLetter) > -1)
                        currentCode = "2";
                    else if ("dt".IndexOf(currentLetter) > -1)
                        currentCode = "3";
                    else if (currentLetter == "l")
                        currentCode = "4";
                    else if ("mn".IndexOf(currentLetter) > -1)
                        currentCode = "5";
                    else if (currentLetter == "r")
                        currentCode = "6";

                    if (currentCode != previousCode)
                        Result.Append(currentCode);

                    if (Result.Length == 4)
                        break;
                    if (currentCode != "")
                        previousCode = currentCode;
                }
            }

            if (Result.Length < 4)
                Result.Append(new String('0', 4 - Result.Length));
            return Result.ToString().ToUpper();
        }
        public static int Difference(string data1, string data2) //Soundex Difference implemented as it is from TechRepublic
        {
            int result = 0;
            string soundex1 = SoundexFunction(data1);
            string soundex2 = SoundexFunction(data2);

            if (soundex1 == soundex2)
                result = 4;
            else
            {
                string sub1 = soundex1.Substring(1, 3);
                string sub2 = soundex1.Substring(2, 2);
                string sub3 = soundex1.Substring(1, 2);
                string sub4 = soundex1.Substring(1, 1);
                string sub5 = soundex1.Substring(2, 1);
                string sub6 = soundex1.Substring(3, 1);


                if (soundex2.IndexOf(sub1) > -1)
                    result = 3;
                else if (soundex2.IndexOf(sub2) > -1)
                    result = 2;
                else if (soundex2.IndexOf(sub3) > -1)
                    result = 2;
                else
                {
                    if (soundex2.IndexOf(sub4) > -1)
                        result++;
                    if (soundex2.IndexOf(sub5) > -1)
                        result++;
                    if (soundex2.IndexOf(sub6) > -1)
                        result++;
                }
                if (soundex1.Substring(0, 1) == soundex2.Substring(0, 1))
                    result++;
            }
            if (result == 0)
                return 1;
            else
                return result;
        }
        private static List<string> SoundexStart(object Tokens, List<string> dictionaryList, int boundary)
        {
            List<string> SoundexDistanceList = new List<string>();
            foreach (var dictionaryItems in dictionaryList)
            {
                string dicItem = dictionaryItems.ToString().Split('\t')[0];
                int soundDifference = Difference(Tokens.ToString(), dicItem);
                if (soundDifference >= boundary)
                    SoundexDistanceList.Add(dicItem + '\t' + soundDifference);
            }
            SoundexDistanceList.Sort((x, y) => y.ToString().Split('\t')[1].CompareTo(x.ToString().Split('\t')[1])); //Sort in descending order for soundex
            return SoundexDistanceList;
        }
        private static List<string> GlobalEditStart(object Tokens, List<string> dictionaryList, int boundary)
        {
            List<string> GlobalEditResultList = new List<string>();

            foreach (var dictionaryItems in dictionaryList)
            {
                string dicItem = dictionaryItems.ToString().Split('\t')[0];
                int GlobalEditDist = GlobalDistanceCalculate(Tokens.ToString(), dicItem);
                if (GlobalEditDist >= boundary)
                    GlobalEditResultList.Add(dicItem + '\t' + GlobalEditDist);
            }
            GlobalEditResultList.Sort((x, y) => y.ToString().Split('\t')[1].CompareTo(x.ToString().Split('\t')[1])); //Sort in descending order for Gedit
            return GlobalEditResultList;
        }
        private static List<string> NGramStart(object Tokens, List<string> dictionaryList, int boundary)
        {
            List<string> NGramDistanceResultList = new List<string>();
            foreach (var dictionaryItems in dictionaryList)
            {
                string dicItem = dictionaryItems.ToString().Split('\t')[0];
                int NGramDist = NGramDistance(Tokens.ToString(), dicItem);
                if (Math.Abs(NGramDist) <= boundary)
                    NGramDistanceResultList.Add(dicItem + '\t' + NGramDist);
            }
            NGramDistanceResultList.Sort((x, y) => x.ToString().Split('\t')[1].CompareTo(y.ToString().Split('\t')[1])); //Sort in ascending order for NGram
            return NGramDistanceResultList;
        }

        public void Initialize(object Tokens, List<string> OutputList, List<string> dictionaryList)
        {
            List<string> ResultList = dictionaryList;
            int Tokenlength = Tokens.ToString().Length;
            string[] OperationsSplit = Operations.Split('|');

            for (int i = 0; i < OperationsSplit.Length; i++)
            {
                int boundarySplit = int.Parse(OperationsSplit[i].Split('-')[1]);
                if (ResultList.Count > 0)
                {
                    if (OperationsSplit[i].ToString().ToUpper().Contains("G")) //call global edit.
                        ResultList = GlobalEditStart(Tokens, ResultList, Tokenlength - boundarySplit);
                    else if (OperationsSplit[i].ToString().ToUpper().Contains("N")) //call Ngram edit.
                        ResultList = NGramStart(Tokens, ResultList, boundarySplit);
                    else if (OperationsSplit[i].ToString().ToUpper().Contains("S")) //call Soundex edit.
                        if(boundarySplit > 0 && boundarySplit < 5) // Soundex boundary check
                            ResultList = SoundexStart(Tokens, ResultList, boundarySplit);
                }
            }

            if(ResultList.Count > 0)
            {
                List<string> value = (List<string>)labelledTokensTable[Tokens];
                foreach (var val in value)
                {
                    string outputvalue = val.Split('\t')[1];
                    MatchCloserToinList(outputvalue, ResultList);
                    MatchExactToList(outputvalue, ResultList[0]);
                }
            }
        }

        //this function looks for the exact match in the list of returned words from dict sorted with the nearest.
        public void MatchExactToList(string value, string OutputListItem)
        {
            if (value.ToUpper().Equals(OutputListItem.Split('\t')[0].ToUpper()))
                    totalAccurateMatches++;
        }


        //This function matches a token canonical form with the list of all the selected nearest matches
        //to the actual token with the word from dict.txt. If match found, return true. Works to calculate the 
        //recall
        public void MatchCloserToinList(string value, List<string> OutputList)
        {
            foreach (var Outputitems in OutputList)
            {
                string Outputline = Outputitems.Split('\t')[0];
                if (value.ToUpper().Equals(Outputline.ToUpper()))
                    totalRecallMatches++;
            }
        }

        public void FileRead(Program p, string inputPath)
        {
            string[] filePaths = Directory.GetFiles(inputPath);
            foreach (var files in filePaths)
            {

                if (Path.GetFileNameWithoutExtension(files).Equals("dict"))
                {
                    p.ReadFile = new StreamReader(files);
                    while (!p.ReadFile.EndOfStream)
                        p.dictionaryList.Add(p.ReadFile.ReadLine());
                    p.ReadFile.Close();
                }
                if (Path.GetFileNameWithoutExtension(files).Equals("labelled-tokens"))
                {
                    p.ReadFile = new StreamReader(files);
                    string[] line;
                    string key;
                    string value;
                    while (!p.ReadFile.EndOfStream)
                    {
                        line = p.ReadFile.ReadLine().Split('\t');
                        key = line[0];
                        value = line[1] + '\t' + line[2];
                        List<string> CanonicalValueList;
                        if (line[1].Equals("OOV"))
                        {
                            if (!p.labelledTokensTable.ContainsKey(key))
                            {
                                CanonicalValueList = new List<string>();
                                CanonicalValueList.Add(value);
                                p.labelledTokensTable.Add(key, CanonicalValueList);
                            }
                            else
                            {
                                CanonicalValueList = (List<string>)p.labelledTokensTable[key];
                                if (!CanonicalValueList.Contains(value))
                                    CanonicalValueList.Add(value);
                            }
                        }
                    }
                    p.ReadFile.Close();
                }
            }
        }
        public double Calculate_Accuracy(Program p)
        {
            //Accuracy is the exact match that comes after all the processes are done - topmost element in output sorted list with the nearest at the top
            double totalPercentage = ((double)p.totalAccurateMatches / (double)p.labelledTokensTable.Count) * 100;
            return Math.Round(totalPercentage, 2);
        }

        public double Calculate_Recall(Program p)
        {
            //Recall is when you search for a word in the output list of several words.
            double totalPercentage = ((double)p.totalRecallMatches / (double)p.labelledTokensTable.Count) * 100;
            return Math.Round(totalPercentage, 2);
        }
        
        static void Main(string[] args)
        {
            List<string> OutputList = new List<string>();
            Console.WriteLine("Enter the input and the output paths separated by a |");
            string path = Console.ReadLine();
            string inputPath = path.Split('|')[0]; //Enable when taking arguments
            string outputPath = path.Split('|')[1];
            //string inputPath = "C:\\Users\\Haaris\\Desktop\\Semester2\\KT\\Input";
            //string outputPath = "C:\\Users\\Haaris\\Desktop\\Semester2\\KT\\Output";
            Program p = new Program(outputPath);
            Console.WriteLine("Enter the Operations Order-Boundary: (G-2|N-3|S-3) - Soundex boundary has to be between 1 - 4");
            p.Operations = Console.ReadLine();
            
            p.FileRead(p,inputPath);

            int Iterator = 0;
            foreach (var Tokens in p.labelledTokensTable.Keys)
            {
                Iterator++;
                if (Iterator == ((p.labelledTokensTable.Count) / 2))
                    Console.WriteLine("Nearly halfway there!!!");
                OutputList = (List<string>)p.labelledTokensTable[Tokens];
                p.Initialize(Tokens, OutputList, p.dictionaryList);
            }

            double Accuracy = p.Calculate_Accuracy(p);
            double Recall = p.Calculate_Recall(p);
            p.WriteResults.WriteLine("Total Words: " + p.labelledTokensTable.Count);
            p.WriteResults.WriteLine("Total Matches: " + p.totalAccurateMatches + '\t' + "Accuracy: " + Accuracy);
            p.WriteResults.WriteLine("Total Recall Matches: " + p.totalRecallMatches + '\t' + "Recall: " + Recall);
            p.WriteResults.Close();
        }
    }
}
