﻿using BooleanRetrieval.Logic.DataSource;
using BooleanRetrieval.Logic.Indexing;
using BooleanRetrieval.Logic.QueryParsing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BooleanRetrieval.Logic.Searching
{
    public class EtSearcher
    {
        private readonly IIndex _index;
        private readonly INotebookDataSource _dataSource;

        public EtSearcher(IIndex index, INotebookDataSource dataSource)
        {
            _index = index;
            _dataSource = dataSource;
        }

        public List<int> Search(string query)
        {
            var parser = new EtQueryParser();
            var tree = parser.Parse(query);

            return SearchInTree(tree);
        }

        private List<int> SearchInTree(ExpressionTreeNode node, bool notCompareWithAll = false)
        {
            if (node == null)
            {
                throw new Exception("Invalid expression tree");
            }

            if (!string.IsNullOrEmpty(node.Term))
            {
                return _index.Find(node.Term);
            }

            var result = new List<int>();

            if (node.Operation == "AND")
            {
                if (node.Child2.Operation == "NOT")
                {
                    return SearchInTree(node.Child1).Except(SearchInTree(node.Child2, true)).ToList();
                }

                return SearchInTree(node.Child1).Intersect(SearchInTree(node.Child2)).ToList();
            }

            if (node.Operation == "OR")
            {
                return SearchInTree(node.Child1).Concat(SearchInTree(node.Child2)).ToList();
            }

            if (node.Operation == "NOT")
            {
                if (notCompareWithAll)
                {
                    return SearchInTree(node.Child1).ToList();
                }

                return _dataSource.GetAllIds().Except(SearchInTree(node.Child1)).ToList();
            }

            if (node.Operation == "ALL")
            {
                return _dataSource.GetAllIds();
            }

            if (node.Operation == "ZERO")
            {
                return new List<int>();
            }

            throw new Exception("Invalid expression tree");
        }
    }
}
