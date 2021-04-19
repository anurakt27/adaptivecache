using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace AdaptiveCache
{
    sealed class TreeBuilder
    {
        private TreeNode rootNode;

        #region Private Methods

        /// <summary>
        /// Performes AVL tree balancing.
        /// </summary>
        /// <param name="root"></param>
        private void BalanceTree(ref TreeNode root)
        {
            if (root.left != null) BalanceTree(ref root.left);
            if (root.right != null) BalanceTree(ref root.right);

            int balanceFactor = CalculateBalanceFactor(root);
            if (Math.Abs(balanceFactor) > 1)
            {
                // right skewed
                if (balanceFactor < -1)
                {
                    // RL case
                    if (CalculateBalanceFactor(root.right) > 0)
                    {
                        TreeNode pivot = root.right;
                        root.right = pivot.left;
                        pivot.left = root.right.right;
                        root.right.right = pivot;

                        pivot = root.right;
                        root.right = pivot.left;
                        pivot.left = root;
                        root = pivot;
                    }
                    // RR case
                    else
                    {
                        TreeNode pivot = root.right;
                        root.right = pivot.left;
                        pivot.left = root;
                        root = pivot;
                    }

                }
                // left skewed
                else if (balanceFactor > 1)
                {
                    // LL case
                    if (CalculateBalanceFactor(root.left) > 0)
                    {
                        TreeNode pivot = root.left;
                        root.left = pivot.right;
                        pivot.right = root;
                        root = pivot;
                    }
                    // LR case
                    else
                    {
                        TreeNode pivot = root.left;
                        root.left = pivot.right;
                        pivot.right = root.left.left;
                        root.left.left = pivot;

                        pivot = root.left;
                        root.left = pivot.right;
                        pivot.right = root;
                        root = pivot;
                    }
                }
            }
        }

        /// <summary>
        /// Calculates difference of height b/w left subtree and right subtree of a TreeNode.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private int CalculateBalanceFactor(TreeNode node)
        {
            int heightOfLeftTree = node.left != null ? CalculateHeight(node.left) : 0;
            int heightOfRightTree = node.right != null ? CalculateHeight(node.right) : 0;

            return (heightOfLeftTree - heightOfRightTree);
        }

        /// <summary>
        /// Calculates the height of AVL tree and then returns the ROOT node.
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        private int CalculateHeight(TreeNode root)
        {
            int height = 1;

            if (root == null)
            {
                throw new ArgumentNullException(EXCEPTION_MESSAGES.EMPTY_TREE);
            }
            if (root.left == null && root.right == null) return height;
            if (root.left != null)
            {
                int newRootHeight = CalculateHeight(root.left) + 1;
                if (newRootHeight > height) height = newRootHeight;
            }
            if (root.right != null)
            {
                int newRootHeight = CalculateHeight(root.right) + 1;
                if (newRootHeight > height) height = newRootHeight;
            }

            return height;
        }


        /// <summary>
        /// Update node's LastUpdated field.
        /// </summary>
        /// <param name="root"></param>
        /// <param name="key"></param>
        private void UpdateLastModified(ref TreeNode root, int key)
        {
            if (root == null) return;

            if (root.Key == key) root.LastUpdated = DateTime.Now;
            else if (root.Key > key) UpdateLastModified(ref root.left, key);
            else if (root.Key < key) UpdateLastModified(ref root.right, key);
        }

        #endregion

        #region Public Methods
        /// <summary>
        /// Creates a new node from the given key and then adds it to the tree.
        /// </summary>
        /// <param name="key"></param>
        public void Add(int key)
        {
            if (rootNode == null)
            {
                rootNode = new TreeNode(key);
            }
            else
            {
                rootNode.AddNode(new TreeNode(key));
                BalanceTree(ref rootNode);
            }
        }

        /// <summary>
        /// Creates a new node from the given key and it's value, and then adds it to the tree.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(int key, object value)
        {
            if (rootNode == null)
            {
                rootNode = new TreeNode(key, value);
            }
            else
            {
                rootNode.AddNode(new TreeNode(key, value));
                BalanceTree(ref rootNode);
            }
        }

        /// <summary>
        /// Creates new nodes from the given list of keys and then adds them to the tree.
        /// </summary>
        /// <param name="keys"></param>
        public void AddRange(IEnumerable<int> keys)
        {
            if (keys == null) throw new ArgumentException(EXCEPTION_MESSAGES.NOTHING_TO_ADD);
            foreach (var key in keys)
            {
                if (rootNode == null)
                {
                    rootNode = new TreeNode(key);
                }
                else
                {
                    rootNode.AddNode(new TreeNode(key));
                    BalanceTree(ref rootNode);
                }
            }
        }

        /// <summary>
        /// Creates new nodes from given key-value pairs and then adds them to the tree
        /// </summary>
        /// <param name="keyValues"></param>
        public void AddRange(IDictionary<int, object> keyValues)
        {
            if (keyValues == null) throw new ArgumentException(EXCEPTION_MESSAGES.NOTHING_TO_ADD);
            foreach (var kv in keyValues)
            {
                if (rootNode == null)
                {
                    rootNode = new TreeNode(kv.Key, kv.Value);
                }
                else
                {
                    rootNode.AddNode(new TreeNode(kv.Key, kv.Value));
                    BalanceTree(ref rootNode);
                }
            }
        }

        /// <summary>
        /// Performs deletion of node from Tree and returns updated root.
        /// </summary>
        /// <param name="key"></param>
        public void Delete(int key)
        {
            rootNode = rootNode.Delete(key);
            if (rootNode != null) BalanceTree(ref rootNode);
        }

        /// <summary>
        /// Performs deletion of multiple nodes from Tree and returns updated root.
        /// </summary>
        /// <param name="key"></param>
        public void DeleteRange(IEnumerable<int> keys)
        {
            if (keys == null) throw new ArgumentNullException(EXCEPTION_MESSAGES.NOTHING_TO_DELETE);
            else
            {
                foreach (var key in keys)
                {
                    rootNode = rootNode.Delete(key);
                    if (rootNode != null) BalanceTree(ref rootNode);
                }
            }
        }

        /// <summary>
        /// Delete nodes whose TTL has expired.
        /// </summary>
        /// <param name="root"></param>
        public void DeleteExpiredNodes()
        {
            if (rootNode == null) return;
            List<int> nodes = rootNode
                .Find(x => DateTime.Now - x.LastUpdated >= TimeSpan.FromSeconds(x.TTL))
                .Select(x => x.Key)
                .ToList();

            DeleteRange(nodes);
        }

        /// <summary>
        /// Gets TreeNode value for the given key.
        /// </summary>
        public object GetValue(int key)
        {
            TreeNode node = rootNode.Find(x => x.Key == key).FirstOrDefault();
            if (DateTime.Now - node.LastUpdated >= TimeSpan.FromSeconds(node.TTL)) return null;
            UpdateLastModified(ref rootNode, key);
            return node.Value;
        }

        #endregion
    }

    partial class EXCEPTION_MESSAGES
    {
        public static string EMPTY_TREE => "Tree is empty";
        public static string DUPLICATE_KEY(object key) => $"Duplicate keys not allowed. Key= '{key}'";
        public static string KEY_NOT_FOUND(object key) => $"Couldn't find key: {key}";
        public static string NOTHING_TO_ADD => "Nothing to add to Tree.";
        public static string NOTHING_TO_DELETE => "Nothing to delete from Tree.";
        public static string INVALID_TYPE => "Invalid type for key. Only int and string are valid types";
        public static string NULL_REFERENCE => "Cannot get reference of Node. NULL value was passed.";
    }

    partial class CONSTANTS
    {
        public static int TimeToLive = 30;
        public static int DeletionInterval = 30000;
    }
}
