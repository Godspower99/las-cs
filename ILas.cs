using System;
using System.Collections.Generic;

namespace LasLibs_CS
{
    public interface ILas
    {
        /// <summary>
        /// Returns a colunm in a las file
        /// </summary>
        /// <param name="column"></param>
        /// <returns><see cref="Las"/></returns>
        /// Member of <see cref="ILas"/>
        LasColumn Column(string column);

        /// <summary>
        /// Returns a column in a las file stripped off null values
        /// </summary>
        /// <param name="column"></param>
        /// <returns><see cref="LasColumn"/></returns>
        /// Member of <see cref="ILas"/>
        LasColumn ColumnStripped(string column);

        /// <summary>
        /// Returns a csv File object in browser | writes csv file to current working driectory in Node
        /// </summary>
        /// <param name="filename"></param>
        /// <returns><see cref="void"/></returns>
        /// Member of <see cref="ILas"/>
        void ToCsv(string filename,string directory);

        /// <summary>
        /// Returns a csv File object in browser and writes csv file to current working driectory in Node of data stripped of null values
        /// </summary>
        /// <param name="filename"></param>
        /// <returns><see cref="void"/></returns>
        /// Member of <see cref="ILas"/>
        void ToCsvStripped(string filename,string directory);

        /// <summary>
        /// Returns the number of rows in a .las file
        /// </summary>
        /// <returns><see cref="int"/></returns>
        /// Member of <see cref="ILas"/>
        int RowCount();

        /// <summary>
        /// Returns the number of columns in a .las file
        /// </summary>
        /// <returns><see cref="int"/></returns>
        /// Member of <see cref="ILas"/>
        int ColumnCount();

        /// <summary>
        /// Returns a two-dimensional array of data in the log
        /// </summary>
        /// <returns><see cref="LasData"/></returns>
        /// Member of <see cref="ILas"/>
        LasData Data();

        /// <summary>
        /// Returns a two-dimensional array of data in the log with all rows containing null values stripped off
        /// </summary>
        /// <returns><see cref="LasData"/></returns>
        /// Member of <see cref="ILas"/>
        LasData DataStripped();

        /// <summary>
        /// Returns the version number of the las file
        /// </summary>
        /// <returns><see cref="double"/></returns>
        /// Member of <see cref="ILas"/>
        double Version();

        /// <summary>
        /// Returns true if the las file is of wrapped variant and false otherwise
        /// </summary>
        /// <returns><see cref="bool"/></returns>
        /// Member of <see cref="ILas"/>
        bool Wrap();

        /// <summary>
        /// Returns an extra information about the well stored in ~others section
        /// </summary>
        /// <returns><see cref="string"/></returns>
        /// Member of <see cref="ILas"/>
        string Other();

        /// <summary>
        /// Returns an array of strings of the logs header/title
        /// </summary>
        /// <returns><see cref="String[]"/></returns>
        /// Member of <see cref="ILas"/>
        string[] Header();

        /// <summary>
        /// Returns an object, each well header and description as a key-value pair
        /// </summary>
        /// <returns><see cref="Dictionary<string, string><string,string>"/></returns>
        /// Member of <see cref="ILas"/>
        Dictionary<string, string> HeaderAndDescr();

        /// <summary>
        /// Returns details of  well parameters.
        /// </summary>
        /// <returns><see cref="Dictionary<string,WellProp>"/></returns>
        /// Member of <see cref="ILas"/>
        Dictionary<string, WellProp> WellParams();

        /// <summary>
        /// Returns details of  curve parameters.
        /// </summary>
        /// <returns><see cref="Dictionary<string,WellProp>"/></returns>
        /// Member of <see cref="ILas"/>
        Dictionary<string, WellProp> CurveParams();

        /// <summary>
        /// Returns details of  parameters of the well.
        /// </summary>
        /// <returns><see cref="Dictionary<string,WellProp>"/></returns>
        /// Member of <see cref="ILas"/>
        Dictionary<string,WellProp> LogParams();
    }
}
