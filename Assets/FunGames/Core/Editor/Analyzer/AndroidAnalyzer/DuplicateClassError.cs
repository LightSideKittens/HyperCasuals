using System;
using System.Collections.Generic;

namespace FunGames.Core.Editor.Analyzer
{
    /// <summary>
    /// A conglomerate of information on a conflisting libraries (jar vs aar vs gradle)
    /// </summary>
    [Serializable]
    public class ErrorGroup
    {
        public StringPair OriginPair;
        public List<DuplicateClassError> Errors;
        public List<ResultLine> ResultLines;

        public ErrorGroup(StringPair libPair)
        {
            OriginPair = libPair;
            Errors = new List<DuplicateClassError>();
            ResultLines = new List<ResultLine>();
        }
    }
    
    [Serializable]
    public class StringPair : IEquatable<StringPair>
        {
            public string A;
            public string B;

            public StringPair(string libA, string libB)
            {
                A = libA;
                B = libB;
            }

            public bool Equals(StringPair other)
            {
                return Equals(other.A, other.B);
            }

            public bool Equals(string otherLibA, string otherLibB)
            {
                return (string.Compare(A, otherLibA) == 0 && string.Compare(B, otherLibB) == 0)
                    || (string.Compare(A, otherLibB) == 0 && string.Compare(B, otherLibA) == 0);
            }

            public override string ToString()
            {
                return A + " <> " + B;
            }
        }

    [Serializable]
    public class DuplicateClassError
        {
            public SuspiciousMessage Message;

            public string DuplicateClass;
            public string FirstLib;
            public string FirstOrigin;
            public string SecondLib;
            public string SecondOrigin;

            public DuplicateClassError(
                SuspiciousMessage message, string duplicateClass, string firstLib, 
                string firstOrigin, string secondLib, string secondOrigin)
            {
                Message = message;
                DuplicateClass = duplicateClass;
                FirstLib = firstLib;
                FirstOrigin = firstOrigin;
                SecondLib = secondLib;
                SecondOrigin = secondOrigin;
            }
        }

        [Serializable]
        public class ResultLine
        {
            public enum LineType
            {
                Group,
                ConflictingLibOrDependency,
                DetailsEntry,
                DetailsEntrySub,
                Solution
            }

            public LineType Type;
            public string Text;
            public Dependency Dependency;
            public AndroidLibrary Library;
            public string ButtonLabel;
            public string ButtonAction;

            public bool HasButton => !string.IsNullOrEmpty(ButtonLabel);

            public ResultLine(LineType type, string text)
            {
                Type = type;
                Text = text;
                ButtonLabel = null;
                ButtonAction = null;
                Dependency = null;
                Library = null;
            }

            public ResultLine(
                LineType type, string text, Dependency dependency, 
                AndroidLibrary library, string buttonLabel, string buttonAction)
            {
                Type = type;
                Text = text;
                Dependency = dependency;
                Library = library;
                ButtonLabel = buttonLabel;
                ButtonAction = buttonAction;
            }

            public string GetLibOrDependencyName()
            {
                if (Dependency != null)
                    return Dependency.Name;
                else if (Library != null)
                    return Library.FileName;
                return null;
            }
        }
}