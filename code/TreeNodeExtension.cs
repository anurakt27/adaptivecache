using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Linq;

namespace AdaptiveCache
{
    static class TreeNodeExtension
    {
        /// <summary>
        /// Inserts a new TreeNode into AVL tree.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="newNode"></param>
        public static void AddNode(this TreeNode root, TreeNode newNode)
        {
            if (root.Key == newNode.Key)
            {
                throw new ArgumentException(EXCEPTION_MESSAGES.DUPLICATE_KEY(root.Key));
            }

            if (newNode.Key < root.Key)
            {
                if (root.left == null) root.left = newNode;
                else AddNode(root.left, newNode);
            }
            else
            {
                if (root.right == null) root.right = newNode;
                else AddNode(root.right, newNode);
            }
        }

        /// <summary>
        /// Finds elements based on provided condition and returns a list of TreeNodes for which the condition matched.
        /// Supports only 1 condition, and can't be chained.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        private static void Find(this TreeNode root, Expression<Func<TreeNode, bool>> func, int key, ref List<TreeNode> result)
        {
            if (func.NodeType == ExpressionType.Equal)
            {
                if (func.Compile().Invoke(root))
                {
                    result.Add(root);
                    return;
                }
                if (root.left != null && key < root.Key)
                {
                    Find(root.left, func, key, ref result);
                }
                else if (root.right != null && key > root.Key)
                {
                    Find(root.right, func, key, ref result);
                }
            }
            else
            {
                if (func.Compile().Invoke(root))
                {
                    result.Add(root);
                }
                if (root.left != null)
                {
                    Find(root.left, func, key, ref result);
                }
                if (root.right != null)
                {
                    Find(root.right, func, key, ref result);
                }
            }
        }

        /// <summary>
        /// Finds elements based on provided condition and returns a list of TreeNodes for which the condition matched.
        /// Supports only 1 condition.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static List<TreeNode> Find(this TreeNode root, Expression<Func<TreeNode, bool>> expression)
        {
            // fetching the RHS of the function expression
            BinaryExpression binaryExp = expression.Body as BinaryExpression;
            int rhs = 0;
            if(expression.NodeType == ExpressionType.Equal)
            {
                rhs = Expression.Lambda<Func<int>>(binaryExp.Right).Compile().Invoke();
            }
            
            List<TreeNode> result = new List<TreeNode>();
            Find(root, expression, rhs, ref result);
            return result;
        }

        /// <summary>
        /// Deletes a node from the Tree and returns updated root.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static TreeNode Delete(this TreeNode root, int key)
        {
            if (root == null) return root;
            if (key < root.Key)
            {
                root.left = Delete(root.left, key);
            }
            else if (key > root.Key)
            {
                root.right = Delete(root.right, key);
            }
            else
            {
                if (root.left == null && root.right == null) return null;
                else if (root.left == null) return root.right;
                else if (root.right == null) return root.left;

                int rootKey = root.Key;
                TreeNode node = root.Find(x => x.Key > rootKey).OrderBy(x => x.Key).FirstOrDefault();
                root.Key = node.Key;
                root.Value = node?.Value;
                root.LastUpdated = node.LastUpdated;
                root.right = Delete(root.right, root.Key);
            }
            return root;
        }
    }
}
