using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace LasLibs_CS
{
    public class Las :ILas
    {
        #region Private Members
        private readonly string _blobString;
        private readonly string[] _blobStringPerLine;
        private readonly Dictionary<string, WellProp> MetaData;
        private readonly Dictionary<string, WellProp> Well;
        private readonly Dictionary<string, WellProp> Curve;
        private readonly Dictionary<string, WellProp> Param;
        #endregion

        #region Constructors

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="filePath"></param>
        public Las(string filePath)
        {
            InitializeBlob(filePath, out _blobString, out _blobStringPerLine);
            try
            {
                ExtractParameters(_blobStringPerLine, out MetaData, out Well, out Curve, out Param);
            }
            catch { }
        }
        #endregion

        #region LAS public Methods

        /// <summary>
        /// Returns a colunm in a las file
        /// </summary>
        /// <param name="column"></param>
        /// <returns><see cref="Las"/></returns>
        /// Member of <see cref="ILas"/>
        public LasColumn Column(string column)
        {
            var hds = Header();
            var sB = Data();
            var index = Array.FindIndex(hds, 0, c => c.ToLower() == column.ToLower());
            if (index < 0)
                throw new ColumnError(column);
            LasColumn lasColumn = new LasColumn();
            lasColumn.numbers = sB.numbers.Select(c => c[index]).ToArray();
            lasColumn.literals = sB.literals.Select(c => c[index]).ToArray();
            return lasColumn;
        }

        /// <summary>
        /// Returns the number of columns in a .las file
        /// </summary>
        /// <returns><see cref="int"/></returns>
        /// Member of <see cref="ILas"/>
        public int ColumnCount()
        {
            return Curve.Count;
        }

        /// <summary>
        /// Returns a column in a las file stripped off null values
        /// </summary>
        /// <param name="column"></param>
        /// <returns><see cref="LasColumn"/></returns>
        /// Member of <see cref="ILas"/>
        public LasColumn ColumnStripped(string column)
        {
            var hds = Header();
            var sB = Data();
            var nullValue = Well.GetValueOrDefault("NULL").Value;
            var index = Array.FindIndex(hds, 0, c => c.ToLower() == column.ToLower());
            if (index < 0)
                throw new ColumnError(column);
            LasColumn lasColumn = new LasColumn();
            lasColumn.numbers = sB.numbers.Select(c => c[index]).Where(x => x !=ConvertToValue(nullValue)).ToArray();
            lasColumn.literals = sB.literals.Select(c => c[index]).Where(x => x != nullValue).ToArray();
            return lasColumn;
        }

        /// <summary>
        /// Returns details of  curve parameters.
        /// </summary>
        /// <returns><see cref="Dictionary<string,WellProp>"/></returns>
        /// Member of <see cref="ILas"/>
        public Dictionary<string,WellProp> CurveParams()
        {
           if(Curve.Count < 1)
                throw new PropertyError("Curve");
            return Curve;
        }

        /// <summary>
        /// Returns a two-dimensional array of data in the log
        /// </summary>
        /// <returns><see cref="LasData"/></returns>
        /// Member of <see cref="ILas"/>
        public LasData Data()
        {
            var hds = Header();
            var totalHeadersLength = hds.Length;
            var sB = Regex.Split(_blobString, @"~A(?:\w*\s*)*\n")[1]
                           .Trim();
            var sB2literals = Regex.Split(sB, @"\s+").ToArray();

            if (sB2literals.Length < 0)
                throw new LasError("No data/~A section in the file");

            LasData lasData = new LasData();
            var conliteral = Chunk(sB2literals, totalHeadersLength);
            lasData.literals = conliteral;

            var sB2doubles = sB2literals.Select(s => ConvertToValue(s)).ToArray();
            var condoubles = Chunk(sB2doubles, totalHeadersLength);
            lasData.numbers = condoubles;

            return lasData;
        }

        /// <summary>
        /// Returns a two-dimensional array of data in the log with all rows containing null values stripped off
        /// </summary>
        /// <returns><see cref="LasData"/></returns>
        /// Member of <see cref="ILas"/>
        public LasData DataStripped()
        {
            var hds = Header();
            var nullValue = Well.GetValueOrDefault("NULL").Value;
            var totalHeadersLength = hds.Length;
            var sB = Regex.Split(_blobString, @"~A(?:\w*\s*)*\n")[1]
                           .Trim();
            var sB2literals = Regex.Split(sB, @"\s+").ToArray();

            if (sB2literals.Length < 0)
                throw new LasError("No data/~A section in the file");

            LasData lasData = new LasData();
            lasData.literals = Chunk(sB2literals, totalHeadersLength).Where(x => !x.Contains(nullValue)).ToArray(); 

            var sB2doubles = sB2literals.Select(s => ConvertToValue(s)).ToArray();
            lasData.numbers = Chunk(sB2doubles, totalHeadersLength).Where(x => !x.Contains(ConvertToValue(nullValue))).ToArray();

            return lasData;
        }

        /// <summary>
        /// Returns an array of strings of the logs header/title
        /// </summary>
        /// <returns><see cref="String[]"/></returns>
        /// Member of <see cref="ILas"/>
        public string[] Header()
        {
            if (Curve.Keys.Count < 1)
                throw new LasError("There is no header in the file");

            List<string> keys = new List<string>();
            foreach (var key in Curve.Keys)
                keys.Add(key);
            return keys.ToArray();
        }

        /// <summary>
        /// Returns an object, each well header and description as a key-value pair
        /// </summary>
        /// <returns><see cref="Dictionary<string, string><string,string>"/></returns>
        /// Member of <see cref="ILas"/>
        public Dictionary<string, string> HeaderAndDescr()
        {
            if (Curve.Count < 1)
                throw new LasError("Poorly formatted ~Curve section in the file");
            Dictionary<string, string> headerDisc = new Dictionary<string, string>();
            foreach (var key in Curve.Keys)
                headerDisc.Add(key, Curve.GetValueOrDefault(key).Description);
            return headerDisc;
        }

        /// <summary>
        /// Returns details of  parameters of the well.
        /// </summary>
        /// <returns><see cref="Dictionary<string,WellProp>"/></returns>
        /// Member of <see cref="ILas"/>
        public Dictionary<string,WellProp> LogParams()
        {
            if (Param.Count < 1)
                throw new PropertyError("Log");
            return Param;
        }

        /// <summary>
        /// Returns an extra information about the well stored in ~others section
        /// </summary>
        /// <returns><see cref="string"/></returns>
        /// Member of <see cref="ILas"/>
        public string Other()
        {
            try
            {
                var som = Regex.Split(_blobString, @"~O(?:\w*\s*)*\n\s*")[1];
                var str = "";
                if (som.Length > 0)
                {
                    var some = Regex.Replace(Regex.Split(som, @"~")[0], @"\n\s*", " ").Trim();
                    str = RemoveComment(some);
                }
                if (str.Length <= 0)
                    return " ";
                return str;
            }
            catch
            {
                return " ";
            }
        }

        /// <summary>
        /// Returns the number of rows in a .las file
        /// </summary>
        /// <returns><see cref="int"/></returns>
        /// Member of <see cref="ILas"/>
        public int RowCount()
        {
            return Data().literals.Length;
        }

        /// <summary>
        /// Returns a csv File object in browser | writes csv file to current working driectory in Node
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="directory"></param>
        /// <returns><see cref="void"/></returns>
        /// Member of <see cref="ILas"/>
        public void ToCsv(string filename = "file", string directory = null)
        {
            var hds = Header();
            var data = Data();
            var rHd = string.Join(",", hds) + "\n";
            var rData = data.literals.Select(d => string.Join(",", d))
                                     .Select(d => string.Join("\n", d))
                                     .ToArray();
            
            var workingDir = directory?? Directory.GetParent(Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).FullName).FullName).FullName;
            var csvData = string.Join("\n", rData);
            try
            {
                File.WriteAllText( $@"{workingDir}\{filename}.csv", rHd + csvData);
            }
            catch
            {
                throw new CsvError();
            }
        }

        /// <summary>
        /// Returns a csv File object in browser and writes csv file to current working driectory in Node of data stripped of null values
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="directory"></param>
        /// <returns><see cref="void"/></returns>
        /// Member of <see cref="ILas"/>
        public void ToCsvStripped(string filename = "file",string directory = null)
        {
            var hds = Header();
            var data = Data();
            var nullValue = Well.GetValueOrDefault("NULL").Value;
            var rHd = string.Join(",", hds) + "\n";
            var rData = data.literals.Select(d => string.Join(",", d))
                                     .Select(d => string.Join("\n", d))
                                     .Where(x => !x.Contains(nullValue))
                                     .ToArray();

            var workingDir = directory ?? Directory.GetParent(Directory.GetParent(Directory.GetParent(Directory.GetCurrentDirectory()).FullName).FullName).FullName;
            var csvData = string.Join("\n", rData);
            try
            {
                File.WriteAllText($@"{workingDir}\{filename}.csv", rHd + csvData);
            }
            catch
            {
                throw new CsvError();
            }
        }

        /// <summary>
        /// Returns the version number of the las file
        /// </summary>
        /// <returns><see cref="double"/></returns>
        /// Member of <see cref="ILas"/>
        public double Version()
        {
            return ConvertToValue(MetaData.GetValueOrDefault("VERS").Value);
        }

        /// <summary>
        /// Returns details of  well parameters.
        /// </summary>
        /// <returns><see cref="Dictionary<string,WellProp>"/></returns>
        /// Member of <see cref="ILas"/>
        public Dictionary<string,WellProp> WellParams()
        {
            if (Well.Count < 1)
                throw new PropertyError("Well");
            return Well;
        }

        /// <summary>
        /// Returns true if the las file is of wrapped variant and false otherwise
        /// </summary>
        /// <returns><see cref="bool"/></returns>
        /// Member of <see cref="ILas"/>
        public bool Wrap()
        {
            return MetaData.GetValueOrDefault("WRAP").Value.ToLower() == "yes" ? true : false;
        }
        #endregion
        
        #region Helpers

        private void InitializeBlob(string filePath,out string blobstring,out string[] blobsStringPerLine)
        {
            try{
                blobstring = File.ReadAllText(filePath);
                blobsStringPerLine = File.ReadAllLines(filePath);
            }
            catch{
                blobstring = null;
                blobsStringPerLine = null;
                throw new PathError();
            }
        }

        private T[][] Chunk<T>(T[] arr, int size)
        {
            T[][] overall = new T[arr.Length / size][];
            int index = 0, ctr = 0;
            while (index < arr.Length){
                overall[ctr] = Slice<T>(arr, index, index + size);
                index += size;
                ctr++;
            }
            return overall;
        }

        private T[] Slice<T>(T[] arr, int start, int end)
        {
            T[] slice = new T[end - start];
            for (int i = 0; i < end - start; i++)
                slice[i] = arr[i + start];
            return slice;
        }

        private string ReplaceFirstFound(string text, string search, string replace)
        {
            int index = text.IndexOf(search);
            if (index < 0)
                return text;
            return text.Substring(0, index) + replace + text.Substring(index + search.Length);
        }

        private string RemoveComment(string str)
        {
            var noComment = str.Trim()
                  .Split("\n")
                  .Select(val => val.TrimStart())
                  .Where(f => !(f.ToCharArray()[0] == '#'));
            return string.Join("\n", noComment);
        }

        private double ConvertToValue(string s)
        {
            return double.Parse(s);
        }

        private void ExtractParameters(string[] lasFile,out Dictionary<string,WellProp> m, out  Dictionary<string, WellProp> w, out Dictionary<string, WellProp> c, out Dictionary<string, WellProp> p)
        {
            List<string> version = new List<string>();
            List<string> well = new List<string>();
            List<string> curve = new List<string>();
            List<string> param = new List<string>();

            for (int i = 0; i < lasFile.Length; i++)
            {
                // Extract Version lines
                if (lasFile[i].StartsWith("~V"))
                {
                    version.Add(lasFile[i]);
                    for (int x = i + 1; x < lasFile.Length; x++)
                     {
                       if (lasFile[x].StartsWith('~'))
                          break;
                       if (lasFile[x].StartsWith('#'))
                          continue;
                       version.Add(lasFile[x]);
                    }
                }

                // Extract Well lines
                if (lasFile[i].StartsWith("~W"))
                {
                    well.Add(lasFile[i].Trim());
                    for (int x = i + 1; x < lasFile.Length; x++)
                    {
                        if (lasFile[x].StartsWith("~"))
                            break;
                        if (lasFile[x].StartsWith("#"))
                            continue;
                        well.Add(lasFile[x].Trim());
                    }
                }

                // Extract Curve lines
                if (lasFile[i].StartsWith("~C"))
                {
                    curve.Add(lasFile[i].Trim());
                    for (int x = i + 1; x < lasFile.Length; x++)
                    {
                        if (lasFile[x].StartsWith("~"))
                            break;
                        if (lasFile[x].StartsWith("#"))
                            continue;
                        curve.Add(lasFile[x].Trim());
                    }
                }

                // Extract Parameter lines
                if (lasFile[i].StartsWith("~P"))
                {
                    param.Add(lasFile[i].Trim());
                    for (int x = i + 1; x < lasFile.Length; x++)
                    {
                        if (lasFile[x].StartsWith("~"))
                            break;
                        if (lasFile[x].StartsWith("#"))
                            continue;
                        param.Add(lasFile[x].Trim());
                    }
                }
            }
            m = ExtractProperties(version);
            w = ExtractProperties(well);
            c = ExtractProperties(curve);
            p = ExtractProperties(param);
        }

        private Dictionary<string, WellProp> ExtractProperties(List<string> wordsArray)
        {
            Dictionary<string, WellProp> props = new Dictionary<string, WellProp>();
            foreach (var line in wordsArray)
            {
                if (line.StartsWith("~"))continue;
                var key = line.Substring(0, line.IndexOf(".")).Trim();
                var keyremoved = ReplaceFirstFound(line, key, "").Trim();
                int firstSpace = keyremoved.IndexOf(" ");
                var unit = keyremoved.Substring(1, firstSpace).Trim();
                var removeKeyandUnit = ReplaceFirstFound(keyremoved, "." + unit, "").Trim();
                var value = removeKeyandUnit.Substring(0, removeKeyandUnit.LastIndexOf(":")).Trim();
                var description = Regex.Matches(removeKeyandUnit, @"[:]").Count > 1 ?
                                 removeKeyandUnit.Substring(removeKeyandUnit.LastIndexOf(":") + 1)
                                 : removeKeyandUnit.Substring(removeKeyandUnit.IndexOf(":") + 1);
                props.Add(key, new WellProp
                {
                    Key = key,
                    Value = string.IsNullOrWhiteSpace(value) ? "none" : value,
                    Unit = string.IsNullOrWhiteSpace(unit) ? "none" : unit,
                    Description = string.IsNullOrWhiteSpace(description) ? "none" : description
                });
            }
            return props;
        }

        #endregion
    }
}
