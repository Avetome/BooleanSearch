﻿using BooleanRetrieval.Logic.DataSource;
using BooleanRetrieval.Logic.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace BooleanRetrieval.Logic.Indexing
{
    /// <summary>
    /// Build a simple inverted index on file and can search on index.
    /// In real live we'll have index and indexBuilder separatly, but here...
    /// </summary>
    public class InvertedIndexer : IIndexer
    {
        private Dictionary<string, HashSet<int>> _invertedIndex;

        /// <summary>
        /// Sharing internal index structure to public it's also looks like bad move. But see above.
        /// </summary>
        public Dictionary<string, HashSet<int>> Index => _invertedIndex;

        public void BuildIndex(INotebookDataSource dataSource)
        {
            _invertedIndex = new Dictionary<string, HashSet<int>>();

            string line;

            var notebooks = dataSource.GetAllNotebook();

            foreach(var item in notebooks)
            {
                // We look on brand and model same way, just because we don't need any ranging yet
                line = item.Value.Brand + "," + item.Value.Model;

                int termStart = 0;

                var i = 0;

                while (true)
                {
                    // TODO: Move symbols to special constants
                    if (char.IsLetterOrDigit(line[i])
                        || line[i] == '-'
                        || line[i] == '.'
                        || line[i] == '/')
                    {
                        termStart++;
                    }
                    else
                    {
                        if (termStart > 0)
                        {
                            AddToInvertedIndex(line.Substring(i - termStart, termStart).ToLower(), item.Key);
                        }

                        termStart = 0;
                    }
                    
                    if (++i == line.Length)
                    {
                        if (termStart > 0)
                        {
                            AddToInvertedIndex(line.Substring(i - termStart, termStart).ToLower(), item.Key);
                        }

                        break;
                    }
                }
            }
        }

        public List<int> FindInIndex(string text)
        {
            // make a copy for list just in case...
            List<int> result = new List<int>();

            if (_invertedIndex.ContainsKey(text))
            {
                result.AddRange(_invertedIndex[text]);
            }

            return result;
        }

        private void AddToInvertedIndex(string term, int id)
        {
            if (_invertedIndex.ContainsKey(term))
            {
                if (!_invertedIndex[term].Contains(id))
                {
                    _invertedIndex[term].Add(id);
                }
            }
            else
            {
                _invertedIndex[term] = new HashSet<int>() { id };
            }
        }
    }
}
