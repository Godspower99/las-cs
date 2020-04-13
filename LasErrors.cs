using System;
namespace LasLibs_CS
{
    public class LasError : Exception
    {
        public string name = "LasError";
        public string message;
        public LasError(string message):base(message)
        {
            this.message = message;
        }
    }

    public class ColumnError : Exception
    {
        public string name = "ColumnError";
        public string message;
        public ColumnError(string column)
        {
            message = $"Column [ {column} ] doesn't exist in the file";
        }
    }

    public class PathError : Exception
    {
        public string name = "PathError";
        public string message = "Path is invalid";
        public PathError() { }
    }

    public class CsvError : Exception
    {
        public string name = "CsvError";
        public string message = "Couldn't convert file to CSV";
        public CsvError() { }
    }

    public class PropertyError : Exception
    {
        public string name = "PropertyError";
        public string message;
        public PropertyError(string property)
        {
            message = $"Property [ {property} ] doesn't exist";
        }
    }
}
